using Newtonsoft.Json;
using ProjectSchoolNs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityExtensions;

namespace CommunityManager
{
    internal class ManagerConfig
    {
        private const string MOD_ID = "riftarr.communitymanager";
        private const string JSON_NAME = "CommunityManager.json";
        private static float _lastRefreshRealtime = -9999f;
        private const float DEFAULT_STUDENT_BASE = 20f;
        private const float DEFAULT_STUDENT_MULTIPLIER = 1f;
        private const float DEFAULT_STUDENT_RANDOM = 5f;
        private const bool DEFAULT_STUDENT_RELATION = true;
        private const bool DEFAULT_STUDENT_BATCH = true;
        private const float DEFAULT_TUITION_MULTIPLIER = 1.25f;
        private const bool DEFAULT_STUDENT_VICTORY_ADJUSTED = true;

        private static float _studentBase = DEFAULT_STUDENT_BASE;
        private static float _studentMultiplier = DEFAULT_STUDENT_MULTIPLIER;
        private static float _studentRandom = DEFAULT_STUDENT_RANDOM;
        private static bool _studentRelation = DEFAULT_STUDENT_RELATION;
        private static bool _studentBatch = DEFAULT_STUDENT_BATCH;
        private static float _tuitionMultiplier = DEFAULT_TUITION_MULTIPLIER;
        private static bool _studentVictoryAdjusted = DEFAULT_STUDENT_VICTORY_ADJUSTED;

        private static Dictionary<string, VariableReference> Parameters = new Dictionary<string, VariableReference>()
        {
            { "StudentBase",            new VariableReference(() => _studentBase,            val => { _studentBase = (float)val; })          },
            { "StudentMultiplier",      new VariableReference(() => _studentMultiplier,      val => { _studentMultiplier = (float)val; })    },
            { "StudentRandom",          new VariableReference(() => _studentRandom,          val => { _studentRandom = (float)val; })        },
            { "StudentRelation",        new VariableReference(() => _studentRelation,        val => { _studentRelation = (bool)val; })       },
            { "StudentBatch",           new VariableReference(() => _studentBatch,           val => { _studentBatch = (bool)val; })          },
            { "TuitionMultiplier",      new VariableReference(() => _tuitionMultiplier,      val => { _tuitionMultiplier = (float)val; })    },
            { "StudentVictoryAdjusted", new VariableReference(() => _studentVictoryAdjusted, val => { _studentVictoryAdjusted = (bool)val; })},
        };
        private static ConfigData LoadedData = new ConfigData();

        public static float StudentBase
        {
            get
            {
                ManagerConfig.RefreshIfStale();
                return _studentBase;
            }

            set => _studentBase = (float)Math.Ceiling(value);
        }
        public static float StudentMultiplier
        {
            get
            {
                ManagerConfig.RefreshIfStale();
                return _studentMultiplier;
            }

            set => _studentMultiplier = (float)Math.Ceiling(value);
        }
        public static float StudentRandom
        {
            get
            {
                ManagerConfig.RefreshIfStale();
                return _studentRandom;
            }

            set
            {
                if (value >= (StudentBase / 2f)) _studentRandom = (float)Math.Ceiling((StudentBase / 2f) - 1f);
                else _studentRandom = (float)Math.Ceiling(value);
            }
        }
        public static bool StudentRelation
        {
            get
            {
                ManagerConfig.RefreshIfStale();
                return _studentRelation;
            }

            set => _studentRelation = value;
        }
        public static bool StudentBatch
        {
            get
            {
                ManagerConfig.RefreshIfStale();
                return _studentBatch;
            }

            set => _studentBatch = value;
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
        public static bool StudentVictoryAdjusted
        {
            get
            {
                ManagerConfig.RefreshIfStale();
                return _studentVictoryAdjusted;
            }

            set => _studentVictoryAdjusted = value;
        }


        public static int StudentAddedCount { get; set; } = 0;
        public static float StudentBaseAdjustRatio { get { return StudentBase / 20f; } }
        public static SchoolStudentSourses StudentSourceData { get; private set; } = SchoolStudentSourses.Load();
        public static Dictionary<StudentSourceInvestItem, MajorModifyersData> MajorModifyers = new Dictionary<StudentSourceInvestItem, MajorModifyersData>();


        private static bool TryReadViaApi(string key, out float value, float fallbackDefault)
        {
            value = fallbackDefault;
            bool result = false;
            try
            {
                Type type;
                if ((type = Type.GetType("EduWorks.ModUI.API, EduWorks_ModUI", false)) == null && (type = Type.GetType("EduWorks.ModUI.API, EduWorks.ModUI", false)) == null)
                {
                    type = (Type.GetType("EduWorks.ModUI.API, ModUI", false) ?? ManagerConfig.FindTypeInLoadedAssemblies("EduWorks.ModUI.API"));
                }
                if (type == null) return result;

                MethodInfo method = type.GetMethod("TryGetNumber", BindingFlags.Static | BindingFlags.Public);
                if (method == null) return result;

                object[] array = new object[]
                {
                    MOD_ID,
                    key,
                    0.0
                };

                if (!(bool)method.Invoke(null, array)) return result;

                object obj = array[2];
                if (obj is double v) value = (float)v;
                result = true;
            }
            catch (Exception ex)
            {
                Debug.LogError("[CommunityManager] ManagerConfig.TryReadViaApi() ERROR: " + ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
            return result;
        }
        private static bool TryReadViaApi(string key, out bool value, bool fallbackDefault)
        {
            value = fallbackDefault;
            bool result = false;
            try
            {
                Type type;
                if ((type = Type.GetType("EduWorks.ModUI.API, EduWorks_ModUI", false)) == null && (type = Type.GetType("EduWorks.ModUI.API, EduWorks.ModUI", false)) == null)
                {
                    type = (Type.GetType("EduWorks.ModUI.API, ModUI", false) ?? ManagerConfig.FindTypeInLoadedAssemblies("EduWorks.ModUI.API"));
                }
                if (type == null) return result;

                MethodInfo method = type.GetMethod("TryGetNumber", BindingFlags.Static | BindingFlags.Public);
                if (method == null) return result;

                object[] array = new object[]
                {
                    MOD_ID,
                    key,
                    0.0
                };

                if (!(bool)method.Invoke(null, array)) return result;

                object obj = array[2];
                if (obj is double v) value = v == 1f;
                result = true;
            }
            catch (Exception ex)
            {
                Debug.LogError("[CommunityManager] ManagerConfig.TryReadViaApi() ERROR: " + ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
            return result;
        }

        private static void RefreshIfStale()
        {
            try
            {
                float realtimeSinceStartup = Time.realtimeSinceStartup;
                bool refreshExpired = (realtimeSinceStartup - _lastRefreshRealtime) < 1f;
                if (!refreshExpired)
                {
                    _lastRefreshRealtime = realtimeSinceStartup;

                    if (!TryReadViaApi("StudentBase", out _studentBase, DEFAULT_STUDENT_BASE)) Load("StudentBase");
                    if (!TryReadViaApi("StudentMultiplier", out _studentMultiplier, DEFAULT_STUDENT_MULTIPLIER)) Load("StudentMultiplier");
                    if (!TryReadViaApi("StudentRandom", out _studentRandom, DEFAULT_STUDENT_RANDOM)) Load("StudentRandom");
                    if (!TryReadViaApi("StudentRelation", out _studentRelation, DEFAULT_STUDENT_RELATION)) Load("StudentRelation");
                    if (!TryReadViaApi("StudentBatch", out _studentBatch, DEFAULT_STUDENT_BATCH)) Load("StudentBatch");
                    if (!TryReadViaApi("TuitionMultiplier", out _tuitionMultiplier, DEFAULT_TUITION_MULTIPLIER)) Load("TuitionMultiplier");
                    if (!TryReadViaApi("StudentVictoryAdjusted", out _studentVictoryAdjusted, DEFAULT_STUDENT_VICTORY_ADJUSTED)) Load("StudentVictoryAdjusted");
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
            public float _lastConfigLoadtime = _lastRefreshRealtime;

            public float StudentBase = DEFAULT_STUDENT_BASE;
            public float StudentMultiplier = DEFAULT_STUDENT_MULTIPLIER;
            public float StudentRandom = DEFAULT_STUDENT_RANDOM;
            public bool StudentRelation = DEFAULT_STUDENT_RELATION;
            public bool StudentBatch = DEFAULT_STUDENT_BATCH;

            public float TuitionMultiplier = DEFAULT_TUITION_MULTIPLIER;

            public bool StudentVictoryAdjusted = DEFAULT_STUDENT_VICTORY_ADJUSTED;
        }

        public static bool Load(string parameterKey)
        {
            bool result = false;

            try
            {
                if (LoadedData._lastConfigLoadtime != _lastRefreshRealtime && File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    LoadedData = JsonUtility.FromJson<ConfigData>(json) ?? new ConfigData();
                }

                Parameters[parameterKey].Set(LoadedData.GetFieldValue(parameterKey));
                result = true;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) Debug.LogError("[Community Manager] Load failed Inner:  " + ex.InnerException.Message + "|" + ex.InnerException.Source + "|" + ex.InnerException.StackTrace);
                else Debug.LogError("[Community Manager] Load failed: " + ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
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
            Type result = null;
            if (string.IsNullOrWhiteSpace(qname)) return result;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type type = assembly.GetType(qname, false);
                    if (type != null) return type;
                }
                catch (Exception ex)
                {
                    Debug.LogError("[CommunityManager] ManagerConfig.FindTypeInLoadedAssemblies() ERROR: " + ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
                }
            }

            return result;
        }

        private sealed class VariableReference
        {
            public Func<object> Get { get; private set; }
            public Action<object> Set { get; private set; }
            public VariableReference(Func<object> getter, Action<object> setter)
            {
                Get = getter;
                Set = setter;
            }
        }

        public sealed class SchoolStudentSourses
        {
            
            public Dictionary<string, Dictionary<long, StudentSource>> StudentSorces { get; set; } = new Dictionary<string, Dictionary<long, StudentSource>>();

            public void UpdateSchoolSource(string SchoolName, long StudentSourceId, StudentSource studentSource)
            {
                try
                {
                    if (StudentSorces.ContainsKey(SchoolName))
                    {
                        if (StudentSorces[SchoolName].ContainsKey(StudentSourceId)) StudentSorces[SchoolName][StudentSourceId] = studentSource;
                        else StudentSorces[SchoolName].Add(StudentSourceId, studentSource);
                    }
                    else
                    {
                        AddSchool(SchoolName);
                        StudentSorces[SchoolName].Add(StudentSourceId, studentSource);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null) Debug.LogError("[Community Manager] ManagerConfig.UpdateSchoolSource Inner ERROR: " + ex.InnerException.Message + "|" + ex.InnerException.Source + "|" + ex.InnerException.StackTrace);
                    else Debug.LogError("[Community Manager] ManagerConfig.UpdateSchoolSource ERROR: " + ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
                }
                
            }

            public bool Save()
            {
                bool Saved = false;
                try
                {
                    string data = JsonConvert.SerializeObject(StudentSorces, Formatting.Indented);
                    File.WriteAllText(SavePath, data);
                    Saved = true;
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null) Debug.LogError("[Community Manager] SchoolStudentSourses.Save() ERROR: " + ex.InnerException.Message + "|" + ex.InnerException.Source + "|" + ex.InnerException.StackTrace);
                    else Debug.LogError("[Community Manager] SchoolStudentSourses.Save() ERROR: " + ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
                }
                return Saved;
            }

            public static SchoolStudentSourses Load()
            {
                SchoolStudentSourses data = new SchoolStudentSourses();
                try
                {
                    if (!File.Exists(SavePath)) return data;

                    string json = File.ReadAllText(SavePath);
                    data.StudentSorces = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<long, StudentSource>>>(json) ?? new Dictionary<string, Dictionary<long, StudentSource>>();
                }
                catch (Exception ex)
                {
                    Debug.LogError("[CommunityManager] ManagerConfig.RefreshIfStale() ERROR: " + ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
                }
                return data;
            }

            private static string SavePath
            {
                get
                {
                    return Path.Combine(Application.persistentDataPath, "CommunityManager_StudentSources.json");
                }
            }
            private void AddSchool(string SchoolName)
            {
                StudentSorces.Add(SchoolName, new Dictionary<long, StudentSource>());
            }
        }

        public sealed class StudentSource
        {
            public string SourceDescription { get; set; }
            public float StudentBase { get; set; }
            public float StudentMultiplier { get; set; }
            public float StudentRandom { get; set; }
            public float HousingSubsidiesMultiplier { get; set; }
            public float TotalStudentCount { get{ return Mathf.CeilToInt((this.StudentBase + this.StudentRandom) * (this.StudentMultiplier + this.HousingSubsidiesMultiplier)); } }
            public float NextTotalStudentCount { get { return Mathf.CeilToInt((this.StudentBase + this.StudentRandom) * (this.StudentMultiplier + this.HousingSubsidiesMultiplier + 1)); } }

            public StudentSource()
            {
                SourceDescription = "";
                StudentBase = DEFAULT_STUDENT_BASE;
                StudentMultiplier = DEFAULT_STUDENT_MULTIPLIER;
                StudentRandom = DEFAULT_STUDENT_RANDOM;
                HousingSubsidiesMultiplier = 0;
            }

            public bool Equals(int StudentCount)
            {
               return StudentCount == TotalStudentCount;
            }
        }

        public sealed class MajorModifyersData
        {
            public bool State { get; set; } = false;
            public int IdSet { get; set; }
        }

        #endregion
    }
}
