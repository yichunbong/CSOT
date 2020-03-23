using CSOT.Lcd.Scheduling.Persists;
using CSOT.Lcd.Scheduling.Outputs;
using CSOT.Lcd.Scheduling.Inputs;
using CSOT.Lcd.Scheduling.DataModel;
using Mozart.Task.Execution;
using Mozart.Extensions;
using Mozart.Collections;
using Mozart.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace CSOT.Lcd.Scheduling
{
    [FeatureBind()]
    public static partial class BranchStepMaster
    {
        private static Dictionary<string, List<BranchStepInfo>> BranchSteps
        {
            get { return InputMart.Instance.Dashboard.BranchSteps; }
        }

        internal static void AddBranchStep(BranchStep entity)
        {
            if (entity == null)
                return;

            string eqpGroup = entity.EQP_GROUP_ID;
            string runMode = entity.RUN_MODE;

            if (string.IsNullOrEmpty(eqpGroup) || string.IsNullOrEmpty(runMode))
                return;
                            
            var infos = BranchStepMaster.BranchSteps;

            string key = CreateKey(eqpGroup, runMode);

            List<BranchStepInfo> list;
            if (infos.TryGetValue(key, out list) == false)
                infos.Add(key, list = new List<BranchStepInfo>());

            var item = CreateHelper.CreateBranchStepInfo(entity);
            list.Add(item);
        }

        internal static BranchStepInfo GetBranchStep(string eqpGroup, string runMode)
        {
            return GetBranchStep(eqpGroup, runMode, null, false);
        }

        internal static BranchStepInfo GetBranchStep(string eqpGroup, string runMode, string productID, bool checkProduct = true)
        {
            var infos = BranchStepMaster.BranchSteps;
            if (infos == null || infos.Count == 0)
                return null;

            string key = CreateKey(eqpGroup, runMode);
            if (string.IsNullOrEmpty(key))
                return null;

            List<BranchStepInfo> list;
            if (infos.TryGetValue(key, out list) == false || list == null)
                return null;

            var matchList = list.FindAll(t => t.IsMatched(productID, checkProduct));
            if (matchList == null || matchList.Count == 0)
                return null;

            if (matchList.Count > 1)
                matchList.Sort(BranchStepInfoComparer.Default);

            return matchList[0];
        }

        private static string CreateKey(string eqpGroup, string runMode)
        {
            return LcdHelper.CreateKey(eqpGroup, runMode);
        }

        #region BranchStepInfo Func   

        internal static bool IsMatched(this BranchStepInfo info, string productID, bool checkProduct = true)
        {
            if (checkProduct == false)
                return true;

            if (string.IsNullOrEmpty(productID))
                return false;

            if (info.IsAllProduct)
                return true;

            if (info.ProductList == null || info.ProductList.Count == 0)
                return true;

            if (info.ProductList.Contains(productID))
                return true;
                            
            return false;
        }

        internal static bool IsLoadable(this BranchStepInfo info, string productID, string ownerType)
        {
            string defaultOwnerType = SiteConfigHelper.GetDefaultOwnerType();

            if (ownerType != defaultOwnerType)
                return false;

            if (info.IsAllProduct || info.IsMatched(productID))
                return true;

            return false;
        }

        #endregion BranchStepInfo Func

        private class BranchStepInfoComparer : IComparer<BranchStepInfo>
        {
            public int Compare(BranchStepInfo x, BranchStepInfo y)
            {
                if (object.ReferenceEquals(x, y))
                    return 0;

                int cmp = x.Priority.CompareTo(y.Priority);

                return cmp;
            }

            public static BranchStepInfoComparer Default = new BranchStepInfoComparer();
        }
    }
}