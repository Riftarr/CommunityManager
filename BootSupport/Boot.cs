using ModFrameworkNs.Publish;
using System;
using System.Reflection;
using UnityEngine;

namespace BootSupport
{
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



    public sealed class BootShim : BootableModFunctionBase
    {
        // Anti double-announce noise (same-session only):
        private static bool s_announced;
        private static string s_token;

        // IMPORTANT: set when Core puts us to sleep or we disable ourselves;
        // forces a re-announce on next enable so Core can poke us again.
        private static volatile bool s_needReannounce;

        private string _token;
        private Action _lateTry; // AssemblyLoad rebound until we find Core

        private string _modId = BootConfig.ModId;
        private string _modName = BootConfig.ModName;

        // Ignore late signals while disabled
        private volatile bool _isDisabled;
        protected override void OnInit() { }

        protected override void OnEnable()
        {
            try
            {
                _isDisabled = false;

                // If we were put to sleep/off earlier, re-announce so Core re-pokes us.
                if (s_needReannounce)
                {
                    TryAnnounceOrDefer();
                    s_needReannounce = false;
                    return;
                }

                // First boot fast-path: avoid redundant Announce within the same session.
                if (s_announced && !string.IsNullOrEmpty(s_token))
                {
                    _token = s_token;
                    return;
                }

                // Normal first-time path.
                TryAnnounceOrDefer();
            }
            catch (Exception ex)
            {
                Debug.LogError("[" + (_modName ?? "Mod") + "]: boot.cs OnEnable failed: " + ex);
            }
        }
        protected override void OnDisable()
        {
            try
            {
                _isDisabled = true;

                // Mark that next enable should re-announce.
                s_needReannounce = true;

                var cc = ResolveCommandCenter();
                var tok = !string.IsNullOrEmpty(_token) ? _token : s_token;

                if (cc != null && !string.IsNullOrEmpty(tok))
                {
                    var mi = cc.GetMethod("NotifyOffline", BindingFlags.Public | BindingFlags.Static);
                    // Only sticky-disable from explicit user action (e.g., Mod UI). Generic disable => Sleep.
                    mi?.Invoke(null, new object[] { tok, false /* temporarily off (sleep) */ });
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[" + (_modName ?? "Mod") + "]: boot.cs OnDisable failed: " + ex);
            }
            finally
            {
                try { AppDomain.CurrentDomain.AssemblyLoad -= OnAsmLoad; } catch { }
                _lateTry = null;
            }
        }

        // ── signal handler from Core ──
        private void OnSignal(string code, string message)
        {
            if (_isDisabled) return;

            try
            {
                var c = (code ?? "").ToLowerInvariant();
                switch (c)
                {
                    case "token":
                        // Core hands our token here (before "go"). Cache it for AckReady/ReportError.
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
                            // Core already patched Harmony. Simply acknowledge success.
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
                                // Tell Core the reason for summary output, and go back to sleep.
                                ReportErrorToCore(err ?? "Boot failed.");
                                Debug.Log("[" + (_modName ?? "Mod") + "]: Poked by Core, but my boot failed (" + (err ?? "Unknown") + "). Sleeping...");
                                AckReady(false);
                            }
                        }
                        return;
                    case "wait":
                        // Quiet
                        return;
                    case "sleep":
                        // Core put us to sleep (e.g., dependency went offline). Re-announce on next enable.
                        s_needReannounce = true;
                        return;
                    case "off":
                        // Fully off; also re-announce on next enable.
                        s_needReannounce = true;
                        return;
                    case "error":
                        Debug.LogError("[" + (_modName ?? "Mod") + "]: Core error: " + message);
                        return;
                }
            }
            catch { /* Never throw into Core. It does not appreciate it */ }
        }

        private void AckReady(bool ok)
        {
            var cc = ResolveCommandCenter();
            if (cc == null)
            {
                Debug.LogError("[" + (_modName ?? "Mod") + "]: AckReady: Core not found"); // Something has gone terribly wrong if we hit this lmao
                return;
            }

            if (string.IsNullOrEmpty(_token))
                Debug.LogError("[" + (_modName ?? "Mod") + "]: AckReady: _token is NULL/empty!");

            var mi = cc.GetMethod("AckReady", BindingFlags.Public | BindingFlags.Static);
            mi?.Invoke(null, new object[] { _token, ok });
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
            {
                // No boot entry. We'll treat this as if it were a success.
                return true;
            }

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

        // ── announce to Core (or defer until Core loads) ──
        private void TryAnnounceOrDefer()
        {
            var cc = ResolveCommandCenter();
            if (cc != null) { Announce(cc); return; }

            _lateTry = () =>
            {
                var cc2 = ResolveCommandCenter();
                if (cc2 == null) return;
                Announce(cc2);
                try { AppDomain.CurrentDomain.AssemblyLoad -= OnAsmLoad; } catch { }
                _lateTry = null;
            };

            AppDomain.CurrentDomain.AssemblyLoad += OnAsmLoad;
        }

        private void OnAsmLoad(object _, AssemblyLoadEventArgs __)
        {
            try { if (_lateTry != null) _lateTry(); } catch { /* never throw */ }
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
                    null,                        // harmonyId -> default to modId in Core
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

            var mi = t.GetMethod(method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            return mi;
        }

        // Allows resolution by "Namespace.Type, Assembly" OR by scanning all loaded assemblies.
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

                    var isDyn = false;
                    try { isDyn = asm.IsDynamic; } catch { isDyn = false; }
                    if (isDyn) continue;

#endif

                    t = asm.GetType(simple, false);
                    if (t != null) return t;
                }
                catch { }
            }

            // last resort: scan names
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = asm.GetTypes(); } catch { continue; }
                for (int i = 0; i < types.Length; i++)
                {
                    var tt = types[i];
                    if (tt.FullName == qname || tt.Name == qname) return tt;
                }
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