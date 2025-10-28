using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CommunityManager
{
    internal class ManagerConfig
    {
        private const string MOD_ID = "riftarr.communitymanager";
        private const string JSON_NAME = "CommunityManager.json";

        private static float _lastRefreshRealtime = -9999f;

        private const float DEFAULT_STUDENT_MULTIPLIER = 10f;
        private const float DEFAULT_TUITION_MULTIPLIER = 1.25f;

        private const float DEFAULT_STUDENT_VICTORY_ADJUSTED = 1f;

        private static float _studentMultiplier = DEFAULT_STUDENT_MULTIPLIER;
        private static float _tuitionMultiplier = DEFAULT_TUITION_MULTIPLIER;

        private static float _studentVictoryAdjusted = DEFAULT_STUDENT_VICTORY_ADJUSTED;

        private static int _studentAddedCount = 0;

        //private Dictionary<string, int> studentSources = new Dictionary<string, int>();

        public static float StudentMultiplier
        {
            get
            {
                ManagerConfig.RefreshIfStale();
                return _studentMultiplier;
            }

            set => _studentMultiplier = (float)Math.Ceiling(value);
        }
        public static float TuitionMultiplier
        {
            get
            {
                ManagerConfig.RefreshIfStale();
                return _tuitionMultiplier;
            }

            set => _tuitionMultiplier = (float)Math.Round(value, 2);
        }

        public static float StudentVictoryAdjusted
        {
            get
            {
                ManagerConfig.RefreshIfStale();
                return _studentVictoryAdjusted;
            }

            set => _studentVictoryAdjusted = Mathf.Clamp01(value);
        }

        public static int StudentAddedCount { get; set; }

        private static bool TryReadViaApi(string key, out float value)
        {
            value = 0f;
            bool result;
            try
            {
                Type type;
                if ((type = Type.GetType("EduWorks.ModUI.API, EduWorks_ModUI", false)) == null && (type = Type.GetType("EduWorks.ModUI.API, EduWorks.ModUI", false)) == null)
                {
                    type = (Type.GetType("EduWorks.ModUI.API, ModUI", false) ?? ManagerConfig.FindTypeInLoadedAssemblies("EduWorks.ModUI.API"));
                }
                Type type2 = type;
                bool flag = type2 == null;
                if (flag)
                {
                    result = false;
                }
                else
                {
                    MethodInfo method = type2.GetMethod("TryGetNumber", BindingFlags.Static | BindingFlags.Public);
                    bool flag2 = method == null;
                    if (flag2)
                    {
                        result = false;
                    }
                    else
                    {
                        object[] array = new object[]
                        {
                            MOD_ID,
                            key,
                            0.0
                        };
                        bool flag3 = (bool)method.Invoke(null, array);
                        bool flag4 = !flag3;
                        if (flag4)
                        {
                            result = false;
                        }
                        else
                        {
                            object obj = array[2];
                            double num2;
                            if (obj is double)
                            {
                                double num = (double)obj;
                                num2 = num;
                            }
                            else
                            {
                                num2 = 0.0;
                            }
                            double num3 = num2;
                            value = (float)num3;
                            result = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[CommunityManager] ManagerConfig.TryReadViaApi() ERROR: " + ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
                result = false;
            }
            return result;
        }

        private static void RefreshIfStale()
        {
            try
            {
                float realtimeSinceStartup = Time.realtimeSinceStartup;
                bool refreshExpired = realtimeSinceStartup - ManagerConfig._lastRefreshRealtime < 1f;
                if (!refreshExpired)
                {
                    ManagerConfig._lastRefreshRealtime = realtimeSinceStartup;
                    float student;
                    bool readStudentSucceeded = ManagerConfig.TryReadViaApi("StudentMultiplier", out student);
                    float tuition;
                    bool readTuitionSucceeded = ManagerConfig.TryReadViaApi("TuitionMultiplier", out tuition);
                    float victory;
                    bool readVictorySucceeded = ManagerConfig.TryReadViaApi("StudentVictoryAdjusted", out victory);
                    if (readStudentSucceeded)
                    {
                        ManagerConfig._studentMultiplier = (float)Math.Ceiling(student);
                    }
                    if (readTuitionSucceeded)
                    {
                        ManagerConfig._tuitionMultiplier = (float)Math.Round(tuition, 2);
                    }
                    if (readVictorySucceeded)
                    {
                        ManagerConfig._studentVictoryAdjusted = Mathf.Clamp01(victory);
                    }
                    bool allAPIFailed = !readStudentSucceeded && !readTuitionSucceeded && !readVictorySucceeded;
                    if (allAPIFailed)
                    {
                        float studentBackup;
                        float tuitionBackup;
                        float victoryBackup;
                        bool readConfigJSONSucceeded = ManagerConfig.Load(out studentBackup, out tuitionBackup, out victoryBackup);
                        if (readConfigJSONSucceeded)
                        {
                            ManagerConfig._studentMultiplier = (float)Math.Ceiling(studentBackup);
                            ManagerConfig._tuitionMultiplier = (float)Math.Round(tuitionBackup, 2);
                            ManagerConfig._studentVictoryAdjusted = Mathf.Clamp01(victoryBackup);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[CommunityManager] ManagerConfig.RefreshIfStale() ERROR: " + ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }

        }

        [Serializable]
        private class ConfigData
        {
            public float StudentMultiplier = DEFAULT_STUDENT_MULTIPLIER;
            public float TuitionMultiplier = DEFAULT_STUDENT_MULTIPLIER;

            public float StudentVictoryAdjusted = DEFAULT_STUDENT_VICTORY_ADJUSTED;
        }

        public static bool Load(out float student, out float tuition, out float victory)
        {
            student = DEFAULT_STUDENT_MULTIPLIER;
            tuition = DEFAULT_TUITION_MULTIPLIER;
            victory = DEFAULT_STUDENT_VICTORY_ADJUSTED;
            bool result = false;

            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    ConfigData data = JsonUtility.FromJson<ConfigData>(json) ?? new ConfigData();
                    student = data.StudentMultiplier;
                    tuition = data.TuitionMultiplier;
                    victory = data.StudentVictoryAdjusted;

                    result = true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[CommunityManager] Load failed: " + e.Message);
                result = false;
            }
            return result;
        }

        #region UTILITIES

        private static string ConfigPath
        {
            get
            {
                var asm = typeof(ManagerConfig).Assembly;
                var asmPath = new Uri(asm.CodeBase).LocalPath;
                var asmDir = Path.GetDirectoryName(asmPath) ?? "";
                return Path.Combine(asmDir, JSON_NAME);
            }
        }

        private static Type FindTypeInLoadedAssemblies(string qname)
        {
            bool flag = string.IsNullOrWhiteSpace(qname);
            Type result;
            if (flag)
            {
                result = null;
            }
            else
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        Type type = assembly.GetType(qname, false);
                        bool flag2 = type != null;
                        if (flag2)
                        {
                            return type;
                        }
                    }
                    catch
                    {
                    }
                }
                result = null;
            }
            return result;
        }

        #endregion

    }
}
