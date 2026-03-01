using FrameworkNs;
using HarmonyLib;
using Lanka;
using ProjectSchoolNs;
using ProjectSchoolNs.SchoolNs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static CommunityManager.ManagerConfig;

namespace CommunityManager
{
    [HarmonyPatch(typeof(StudentSourceDevelopPanel), "SetData")]
    internal static class Prefix_StudentSourceDevelopPanel_SetData
    {
        private static MethodInfo privateGenBlock = AccessTools.Method(typeof(StudentSourceDevelopPanel), "GenBlock");

        public static bool Prefix(StudentSourceDevelopPanel __instance, StudentSourceInstance studentSource)
        {
            try
            {
                MajorModifyer InvestItem = new MajorModifyer();

                __instance.itemRoot.DestroyAllChild();
                float maxFactor = studentSource.investmentPreference.GetAllItems().Max((ValueTuple<CurrencyType, float> i) => i.Item2);
                foreach (StudentSourceInvestItem item in StudentSourceGlobalConfig.instance.investItems)
                {
                    if (item is MajorModifyer)
                    {
                        InvestItem = item as MajorModifyer;
                        MajorModifyersData extraData = new MajorModifyersData() { State = false, IdSet = 0 };
                        ManagerConfig.MajorModifyers?.Add(item, extraData);
                    }

                    privateGenBlock.Invoke(__instance, new object[] { studentSource, item, maxFactor });
                }
                if (studentSource.AddedinvestItems != null)
                {
                    foreach (StudentSourceInvestItem item2 in studentSource.AddedinvestItems)
                    {
                        if (item2 is MajorModifyer)
                        {
                            InvestItem = item2 as MajorModifyer;
                            MajorModifyersData extraData = new MajorModifyersData() { State = false, IdSet = 0 };
                            ManagerConfig.MajorModifyers?.Add(item2, extraData);
                        }

                        privateGenBlock.Invoke(__instance, new object[] { studentSource, item2, maxFactor });
                    }
                }

                //======================================================================================================
                foreach (StudentSourceInvestItem item3 in _getAditionalInvestments(InvestItem))
                {
                    privateGenBlock.Invoke(__instance, new object[] { studentSource, item3, maxFactor });
                }
                //======================================================================================================

                return false;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) Debug.LogError("[Community Manager] Prefix_StudentSourceDevelopPanel_SetData Inner ERROR: " + ex.InnerException.Message + "|" + ex.InnerException.Source + "|" + ex.InnerException.StackTrace);
                else Debug.LogError("[Community Manager] Prefix_StudentSourceDevelopPanel_SetData ERROR: " + ex.Message + "|" + ex.Source + "|" + ex.StackTrace);

                return true;
            }
        }

        public static List<StudentSourceInvestItem> _getAditionalInvestments(MajorModifyer InvestItem)
        {
            List<StudentSourceInvestItem> newInvestments = new List<StudentSourceInvestItem>();
            MajorModifyersData extraData;
            try
            {
                StudentSourceInvestItem Festival = new StudentSourceInvestItem_Festival();
                StudentSourceInvestItem Morality = new StudentSourceInvestItem_Morality();
                StudentSourceInvestItem Housing = new StudentSourceInvestItem_Housing();

                extraData = new MajorModifyersData() { State = true, IdSet = 1 };
                StudentSourceInvestItem AspirationOne = CloneInvestmentObject(InvestItem, extraData);
                ManagerConfig.MajorModifyers.Add(AspirationOne, extraData);

                extraData = new MajorModifyersData() { State = true, IdSet = 2 };
                StudentSourceInvestItem AspirationTwo = CloneInvestmentObject(InvestItem, extraData);
                ManagerConfig.MajorModifyers.Add(AspirationTwo, extraData);

                extraData = new MajorModifyersData() { State = true, IdSet = 3 };
                StudentSourceInvestItem AspirationThree = CloneInvestmentObject(InvestItem, extraData);
                ManagerConfig.MajorModifyers.Add(AspirationThree, extraData);

                newInvestments.Add(Festival);
                newInvestments.Add(Morality);
                newInvestments.Add(Housing);
                newInvestments.Add(AspirationOne);
                newInvestments.Add(AspirationTwo);
                newInvestments.Add(AspirationThree);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) Debug.LogError("[Community Manager] _getAditionalInvestments Inner ERROR: " + ex.InnerException.Message + "|" + ex.InnerException.Source + "|" + ex.InnerException.StackTrace);
                else Debug.LogError("[Community Manager] _getAditionalInvestments ERROR: " + ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }
            return newInvestments;
        }

        private class ObjectDict<TObject> : Dictionary<string, TObject> where TObject : UnityEngine.Object
        {
            private Dictionary<string, string> namePathDict = new Dictionary<string, string>();

            public ObjectDict(List<ObjectNamePathPair> objectPathPaths)
            {
                if (objectPathPaths == null)
                {
                    return;
                }

                foreach (ObjectNamePathPair objectPathPath in objectPathPaths)
                {
                    if (!string.IsNullOrEmpty(objectPathPath.Path) && !string.IsNullOrEmpty(objectPathPath.Name))
                    {
                        namePathDict[objectPathPath.Name] = objectPathPath.Path;
                    }
                }
            }

            public ObjectDict(List<ObjectNameAssetPair> objectNameAssetPairs)
            {
                if (objectNameAssetPairs == null)
                {
                    return;
                }

                foreach (ObjectNameAssetPair objectNameAssetPair in objectNameAssetPairs)
                {
                    if (!string.IsNullOrEmpty(objectNameAssetPair.Name) && objectNameAssetPair.Asset is TObject value)
                    {
                        base[objectNameAssetPair.Name] = value;
                    }
                }
            }

            public TObject GetObject(string name)
            {
                if (string.IsNullOrEmpty(name))
                {
                    return null;
                }

                if (TryGetValue(name, out var value))
                {
                    return value;
                }

                if (namePathDict.TryGetValue(name, out var value2))
                {
                    value = (base[name] = Framework.AssetMgr.LoadAsset<TObject>(value2));
                }

                return value;
            }

            public bool TryGetObject(string name, out TObject obj)
            {
                obj = null;
                if (string.IsNullOrEmpty(name))
                {
                    return false;
                }

                if (TryGetValue(name, out var value))
                {
                    obj = value;
                    return obj;
                }

                if (namePathDict.TryGetValue(name, out var value2))
                {
                    value = (base[name] = Framework.AssetMgr.LoadAsset<TObject>(value2));
                    obj = value;
                }

                return obj;
            }
        }

        private static MajorModifyer CloneInvestmentObject(MajorModifyer Original, MajorModifyersData extraData)
        {
            MajorModifyer newItem = new MajorModifyer();

            try
            {
                newItem.ID = Original.ID;
                newItem.nameI18n = new I18nText() { Value = $"Change Study Plan (Group {extraData.IdSet.ToString()})" };//Original.nameI18n;
                newItem.post = Original.post;
                newItem.Donatetype = Original.Donatetype;
                newItem.onDonate = Original.onDonate;
                newItem.stake = Original.stake;

                newItem.baseCurrency = Original.baseCurrency;
                newItem.MoneyByPerpertyValue = Original.MoneyByPerpertyValue;
                newItem.contributionByPerpertyValue = Original.contributionByPerpertyValue;
                newItem.PTAPByPerpertyValue = Original.PTAPByPerpertyValue;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) Debug.LogError("[Community Manager] CloneInvestmentObject Inner ERROR: " + ex.InnerException.Message + "|" + ex.InnerException.Source + "|" + ex.InnerException.StackTrace);
                else Debug.LogError("[Community Manager] CloneInvestmentObject ERROR: " + ex.Message + "|" + ex.Source + "|" + ex.StackTrace);
            }

            return newItem;
        }
    }
}
