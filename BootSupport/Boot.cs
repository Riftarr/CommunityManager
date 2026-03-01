// File: boot.cs
// Purpose: Universal, minimal boot shim any mod can ship.
//  • Announces itself to EduWorks.Core.CommandCenter (by reflection).
//  • Core handles waking mods up after Harmony is loaded.
//  • Reports failures back to Core.
//
// • You do NOT need to include a 'BootableModFunctionBase' elsewhere in your mod. boot.cs includes it already

using System;
using System.Reflection;
using UnityEngine;
using ModFrameworkNs.Publish;

namespace BootSupport
{
    // ========== CONFIG START ==========
    internal static class BootConfig
    {
        public const string ModName = "Community Manager";

        public const string ModId = "riftarr.communitymanager";

        public static readonly string[] Dependencies = { "eduworks.core", "eduworks.modui" };

        public static readonly bool HarmonyOnly = true;

        public const string Boot = null;

        public const bool CustomCleanup = false;

        public const string CleanupEntry = null;
    }
    // ========== CONFIG END ==========

// ========== Do not modify anything below this point ==========
    internal sealed class BootPoller : MonoBehaviour
    {
        private Action _tick;
        public void Init(Action tick) { _tick = tick; }

        private void Update()
        {
            try { _tick?.Invoke(); }
            catch { }
        }
    }

    public sealed class BootShim : BootableModFunctionBase
    {
        private static bool s_announced;
        private static string s_token;
        private static volatile bool s_needReannounce;

        // IMPORTANT: Core may request Sleep vs Off. Carry that into NotifyOffline().
        private static volatile bool s_nextDisableIsPermanent = true;

        private string _token;
        private volatile bool _isDisabled;

        private GameObject _pollGo;
        private BootPoller _poller;

        private string _modId = BootConfig.ModId;
        private string _modName = BootConfig.ModName;

        protected override void OnInit() { }

        protected override void OnEnable()
        {
            _isDisabled = false;

            // If we were put to sleep/off earlier, re-announce so Core re-pokes us.
            if (s_needReannounce)
            {
                StartPollingForCore();
                s_needReannounce = false;
                return;
            }

            // Fast path: already announced this session.
            if (s_announced && !string.IsNullOrEmpty(s_token))
            {
                _token = s_token;
                return;
            }

            StartPollingForCore();
        }

        protected override void OnDisable()
        {
            try
            {
                _isDisabled = true;
                s_needReannounce = true;

                StopPolling();

                var cc = ResolveCommandCenter();
                var tok = !string.IsNullOrEmpty(_token) ? _token : s_token;

                if (cc != null && !string.IsNullOrEmpty(tok))
                {
                    var mi = cc.GetMethod("NotifyOffline", BindingFlags.Public | BindingFlags.Static);

                    // Use Core’s last requested mode if available.
                    bool permanentlyOff = s_nextDisableIsPermanent;
                    mi?.Invoke(null, new object[] { tok, permanentlyOff });

                    // reset to default for manual disables, etc.
                    s_nextDisableIsPermanent = true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[" + (_modName ?? "Mod") + "]: boot.cs OnDisable failed: " + ex);
            }
        }

        private void StartPollingForCore()
        {
            if (_pollGo != null) return;

            _pollGo = new GameObject("[BootShim.Poller] " + (_modId ?? "mod"));
            UnityEngine.Object.DontDestroyOnLoad(_pollGo);
            _poller = _pollGo.AddComponent<BootPoller>();
            _poller.Init(TickTryAnnounce);
        }

        private void StopPolling()
        {
            try
            {
                if (_pollGo != null) UnityEngine.Object.Destroy(_pollGo);
            }
            catch { }
            _pollGo = null;
            _poller = null;
        }

        private void TickTryAnnounce()
        {
            if (_isDisabled) { StopPolling(); return; }

            var cc = ResolveCommandCenter();
            if (cc == null) return;

            StopPolling();
            Announce(cc);
        }

        // signal handler from Core
        private void OnSignal(string code, string message)
        {
            if (_isDisabled) return;

            try
            {
                var c = (code ?? "").ToLowerInvariant();
                switch (c)
                {
                    case "token":
                        if (string.IsNullOrEmpty(_token) && !string.IsNullOrEmpty(message))
                        {
                            _token = message;
                            s_token = _token;
                            s_announced = true;
                        }
                        return;

                    case "go":
                        Debug.Log("[" + (_modName ?? "Mod") + "]: Poked by Core. Waking up!");
                        if (BootConfig.HarmonyOnly)
                        {
                            AckReady(true);
                        }
                        else
                        {
                            if (RunBoot(out var err))
                            {
                                AckReady(true);
                            }
                            else
                            {
                                ReportErrorToCore(err ?? "Boot failed.");
                                Debug.Log("[" + (_modName ?? "Mod") + "]: Boot failed (" + (err ?? "Unknown") + ").");
                                AckReady(false);
                            }
                        }
                        return;

                    case "sleep":
                        // Tell OnDisable() this is not permanent
                        s_nextDisableIsPermanent = false;
                        s_needReannounce = true;
                        return;

                    case "off":
                        // Permanent off
                        s_nextDisableIsPermanent = true;
                        s_needReannounce = true;
                        return;

                    case "error":
                        Debug.LogError("[" + (_modName ?? "Mod") + "]: Core error: " + message);
                        return;
                }
            }
            catch { }
        }

        private void AckReady(bool ok)
        {
            var cc = ResolveCommandCenter();
            if (cc == null)
            {
                Debug.LogError("[" + (_modName ?? "Mod") + "]: AckReady: Core not found"); // Something has gone terribly wrong if we hit this lmao
                return;
            }
            var tok = _token;
            if (string.IsNullOrEmpty(tok))
                Debug.LogError("[" + (_modName ?? "Mod") + "]: AckReady: _token is NULL/empty!");

            var mi = cc.GetMethod("AckReady", BindingFlags.Public | BindingFlags.Static);
            mi?.Invoke(null, new object[] { tok, ok });
        }

        private void ReportErrorToCore(string msg)
        {
            var cc = ResolveCommandCenter();
            if (cc == null) return;

            var mi = cc.GetMethod("ReportError", BindingFlags.Public | BindingFlags.Static);
            mi?.Invoke(null, new object[] { _token, msg ?? "Unknown error." });
        }

        private bool RunBoot(out string error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(BootConfig.Boot))
                return true;

            try
            {
                var mi = ResolveMethodLoose(BootConfig.Boot);
                if (mi == null)
                    throw new InvalidOperationException("Boot entry not found: " + BootConfig.Boot);

                if (!mi.IsStatic || mi.GetParameters().Length != 0)
                    throw new InvalidOperationException("Boot entry must be 'static void " + mi.Name + "()'");

                mi.Invoke(null, null);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private void Announce(Type commandCenter)
        {
            try
            {
                // string Announce(string modId, string modName, string harmonyId, string bootEntry, string cleanupEntry,
                //                 bool harmonyOnly, bool customCleanup, string[] depIds, Action<string,string> onSignal)
                var mi = commandCenter.GetMethod("Announce", BindingFlags.Public | BindingFlags.Static, null,
                    new Type[]
                    {
                        typeof(string), typeof(string), typeof(string), typeof(string), typeof(string),
                        typeof(bool), typeof(bool), typeof(string[]), typeof(Action<string,string>)
                    },
                    null);

                if (mi == null) throw new MissingMethodException("CommandCenter", "Announce");

                var deps = BootConfig.Dependencies ?? new string[0];

                var tokenObj = mi.Invoke(null, new object[]
                {
                    _modId,
                    _modName,
                    null, // harmonyId -> default to modId in Core
                    BootConfig.Boot ?? null,
                    BootConfig.CleanupEntry ?? null,
                    BootConfig.HarmonyOnly,
                    BootConfig.CustomCleanup,
                    deps,
                    (Action<string,string>)OnSignal
                });

                _token = tokenObj as string ?? tokenObj?.ToString();
                s_token = _token;
                s_announced = !string.IsNullOrEmpty(_token);

                if (string.IsNullOrEmpty(_token))
                    Debug.LogError("[" + (_modName ?? "Mod") + "]: Announce returned no token!");
            }
            catch (Exception ex)
            {
                Debug.LogError("[" + (_modName ?? "Mod") + "]: Announce failed: " + ex);
            }
        }

        private static MethodInfo ResolveMethodLoose(string entry)
        {
            if (string.IsNullOrWhiteSpace(entry)) return null;

            var s = entry.Trim();
            int lastDot = s.LastIndexOf('.');
            if (lastDot <= 0 || lastDot >= s.Length - 1) return null;

            string typeName = s.Substring(0, lastDot).Trim();
            string method = s.Substring(lastDot + 1).Trim();

            var t = ResolveTypeLoose(typeName);
            if (t == null) return null;

            return t.GetMethod(method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        }

        private static Type ResolveTypeLoose(string qname)
        {
            if (string.IsNullOrWhiteSpace(qname)) return null;

            var t = Type.GetType(qname, false);
            if (t != null) return t;

            string simple = qname.Trim();
            int comma = simple.IndexOf(',');
            if (comma >= 0) simple = simple.Substring(0, comma).Trim();

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
#if NETFRAMEWORK || NETSTANDARD2_0
                    bool isDyn = false;
                    try { isDyn = asm.IsDynamic; } catch { isDyn = false; }
                    if (isDyn) continue;
#endif
                    t = asm.GetType(simple, false);
                    if (t != null) return t;
                }
                catch { }
            }
            return null;
        }

        private static Type ResolveCommandCenter()
        {
            string[] candidates =
            {
                "EduWorks.Core.CommandCenter, EduWorks.Core",
                "EduWorks.Core.CommandCenter, Core",
                "EduWorks.Core.CommandCenter"
            };

            for (int i = 0; i < candidates.Length; i++)
            {
                var t = Type.GetType(candidates[i], false);
                if (t != null) return t;
            }

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var t = asm.GetType("EduWorks.Core.CommandCenter", false);
                    if (t != null) return t;
                }
                catch { }
            }

            return null;
        }
    }
}