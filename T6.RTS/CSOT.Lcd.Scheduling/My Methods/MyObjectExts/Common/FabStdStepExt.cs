using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using CSOT.Lcd.Scheduling.DataModel;
using CSOT.Lcd.Scheduling.Inputs;
using CSOT.Lcd.Scheduling.Outputs;
using CSOT.Lcd.Scheduling.Persists;
using Mozart.SeePlan.Simulation;
namespace CSOT.Lcd.Scheduling
{
    [FeatureBind()]
    public static partial class FabStdStepExt
    {
        internal static void AddEqp(this FabStdStep stdStep, FabEqp eqp)
        {
            if (stdStep.AllEqps == null)
                stdStep.AllEqps = new List<FabEqp>();

            if (stdStep.AllEqps.Contains(eqp) == false)
                stdStep.AllEqps.Add(eqp);

        }

        internal static void AddBaseEqp(this FabStdStep step, FabEqp eqp)
        {
            if (step.BaseEqps == null)
                step.BaseEqps = new List<string>();

            step.BaseEqps.Add(eqp.EqpID);
        }

        internal static bool IsBaseEqp(this FabStdStep step, string eqpID)
        {
            if (step.BaseEqps == null)
                return false;

            return step.BaseEqps.Contains(eqpID);
        }

        internal static List<FabAoEquipment> GetLoadableEqpList(this FabStdStep stdStep)
        {
            List<FabAoEquipment> list = new List<FabAoEquipment>();
            foreach (var item in stdStep.AllEqps)
            {
                var eqp = AoFactory.Current.GetEquipment(item.EqpID) as FabAoEquipment;
                if (eqp == null)
                    continue;

                list.Add(eqp);
            }

            return list;
        }

        public static int GetWorkingEqpCount(this FabStdStep stdStep, FabLot lot, bool checkProductVersion = true, bool onlyParent = true)
        {
            if(lot == null)
                return 0;
                            
            string productID = lot.CurrentProductID;
            string prodVer = lot.OrigProductVersion;
            string ownerType = lot.OwnerType;
            string ownerID = lot.OwnerID;

            return GetWorkingEqpCount(stdStep, productID, prodVer, ownerType, ownerID, checkProductVersion, onlyParent);
        }

        public static int GetWorkingEqpCount(this FabStdStep stdStep, JobFilterInfo info, bool checkProductVersion = true, bool onlyParent = true)
        {
            if (info == null)
                return 0;
                            
            string productID = info.ProductID;
            string prodVer = info.ProductVersion;
            string ownerType = info.OwnerType;
            string ownerID = info.OwnerID;

            return GetWorkingEqpCount(stdStep, productID, prodVer, ownerType, ownerID, checkProductVersion, onlyParent);
        }

        public static int GetWorkingEqpCount(this FabStdStep stdStep, 
            string productID, string productVersion, string ownerType, string ownerID, bool checkProductVersion = true, bool onlyParent = true)
        {
            var list = GetLoadableEqpList(stdStep);
            if (list == null)
                return 0;

            string shopID = stdStep.ShopID;
            string stepID = stdStep.StepID;

            int cnt = 0;
            foreach (var eqp in list)
            {
                if (eqp.IsParallelChamber)
                {
                    foreach (var subEqp in eqp.SubEqps)
                    {
                        if (subEqp.IsLastPlan(shopID, stepID, productID, productVersion, ownerType, ownerID, checkProductVersion))
                        {
                            cnt++;

                            if(onlyParent)
                                break;
                        }
                    }
                }
                else
                {
                    if (eqp.IsLastPlan(shopID, stepID, productID, productVersion, ownerType, ownerID, checkProductVersion))
                        cnt++;
                }
            }

            return cnt;
        }

        public static List<FabAoEquipment> GetWorkingEqpList(this FabStdStep stdStep, FabLot lot, bool checkProductVersion = true)
        {
            string productID = lot.CurrentProductID;
            string productVersion = lot.CurrentProductVersion;
            string ownerType = lot.OwnerType;
            string ownerID = lot.OwnerID;

            return GetWorkingEqpList(stdStep, productID, productVersion, ownerType, ownerID, checkProductVersion);
        }

        public static List<FabAoEquipment> GetWorkingEqpList(this FabStdStep stdStep, 
            string productID, string productVersion, string ownerType, string ownerID, bool checkProductVersion = true)
        {
            string shopID = stdStep.ShopID;
            string stepID = stdStep.StepID;

            List<FabAoEquipment> result = new List<FabAoEquipment>();

            var list = GetLoadableEqpList(stdStep);
            if (list == null)
                return result;
            
            foreach (var eqp in list)
            {
                if (eqp.IsParallelChamber)
                {
                    foreach (var subEqp in eqp.SubEqps)
                    {
                        if (subEqp.IsLastPlan(shopID, stepID, productID, productVersion, ownerType, ownerID, checkProductVersion))
                        {
                            result.Add(eqp);
                            break;
                        }
                    }
                }
                else
                {
                    if (eqp.IsLastPlan(shopID, stepID, productID, productVersion, ownerType, ownerID, checkProductVersion))
                        result.Add(eqp);
                }
            }

            return result;
        }
    }
}
