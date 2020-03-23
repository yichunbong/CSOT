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
    public static partial class EntityHelper
    {
         static Dictionary<string, int> PreFixLotIDIndex = new Dictionary<string, int>();


         internal static EntityState GetEntityState(FabWipInfo wip)
         {
            return GetEntityState(wip.WipState.ToUpper());
         }

         internal static EntityState GetEntityState(string wipState)
         {

             if (wipState == "WAIT")
                 return EntityState.WAIT;

             if (wipState == "RUN")
                 return EntityState.RUN;

             if (string.Compare(wipState, "HOLD", true) == 0)
                 return EntityState.HOLD;

             return EntityState.WAIT;
         }

        internal static FabLot CreateFrontInLot(FabProduct product, ProductType prodType, int inQty, BatchInfo batch)
        {
            FabStep step = product.Process.FirstStep as FabStep;

            //TODO : 2019.8.27 미사용파악 

            //FabWipInfo info = CreateHelper.CreateWipInfoDummy(
            //    CreateFrontInLotID(product),
            //    batch,
            //    product,
            //    product.Process as FabProcess,
            //    step,
            //    prodType,
            //    Constants.NULL_ID,
            //    2,
            //    inQty,
            //    EntityState.WAIT,
            //    null,
            //    string.Empty,
            //    AoFactory.Current.NowDT,
            //    DateTime.MinValue);

            FabWipInfo info = new FabWipInfo();

            EntityHelper.AddBatchInfo(batch, step, inQty);

            FabLot lot = CreateHelper.CreateLot(info, LotState.CREATE);
            lot.LotState = Mozart.SeePlan.Simulation.LotState.CREATE;


            return lot;
        }

        internal static string CreateFrontInLotID(FabProduct prod)
        {
            string prefix = string.Format("L{0}I", prod.ShopID);
            
            return GetLotID(prefix);
        }

        internal static string CreateCellInLotID(FabProduct prod)
        {
            string prefix = string.Format("C{0}I", prod.ShopID);
            
            return GetLotID(prefix);
        }

        private static string GetLotID(string prefix)
        {
            int index;
            if (PreFixLotIDIndex.TryGetValue(prefix, out index) == false)
                PreFixLotIDIndex.Add(prefix, index = 0);

            string lotID = string.Format("{0}{1:0000}", prefix, ++index);
            PreFixLotIDIndex[prefix] = index;

            return lotID;
        }


        internal static void AddBatchInfo(BatchInfo info, FabStep step, int lotQty)
        {
            info.RemainQty += lotQty;
            info.BatchQty += lotQty;

            FabStep first = info.InitFirstLotStep;

            if (first == null)
            {
                info.InitFirstLotStep = step;
                return;
            }
        }

        internal static BatchInfo GetSafeBatchInfo(string batchID)
        {
            if (string.IsNullOrEmpty(batchID))
                return null;

            BatchInfo batch;

            if (InputMart.Instance.BatchInfo.TryGetValue(batchID, out batch) == false)
                InputMart.Instance.BatchInfo.Add(batchID, batch = CreateHelper.CreateBatchInfo(batchID));

            return batch;
        }


        internal static string CreateBatchID(string productID, DateTime now)
        {
            return productID + now.ToString("yyyyMMddhhmmss");
        }

        internal static void AddLotFilteredInfo(this FabPlanInfo info, string reason, DispatchFilter type)
        {
            info.LotFilterInfo.Reason = reason;
            info.LotFilterInfo.FilterType = type;
        }

        internal static bool IsMatched(this FabPlanInfo info, FabLot lot, bool checkProductVersion)
        {
            if (lot == null)
                return false;

            string shopID = lot.CurrentShopID;
            string stepID = lot.CurrentStepID;
            string productID = lot.CurrentProductID;

            string productVersion = lot.CurrentProductVersion;
            string ownerType = lot.OwnerType;
            string ownerID = lot.OwnerID;

            return info.IsMatched(shopID, stepID, productID, productVersion, ownerType, ownerID, checkProductVersion);
        }

        internal static bool IsMatched(this FabPlanInfo info, string shopID, string stepID, 
            string productID, string productVersion, string ownerType, string ownerID, bool checkProductVersion)
        {
            if (info == null)
                return false;

            if (info.ShopID != shopID)
                return false;

            if (info.StepID != stepID)
                return false;

            if (info.ProductID != productID)
                return false;

            if (checkProductVersion)
            {
                var stdStep = info.FabStep.StdStep;
                if (EqpArrangeMaster.IsFixedProductVer(stdStep, productVersion))
                {
                    if (info.ProductVersion != productVersion)
                        return false;
                }
            }

            if (info.OwnerType != ownerType)
                return false;

            //TODO : OwnerID 추가 필요
            //if (info.OwnerID != ownerID)
            //    return false;

            return true;
        }
    }
}
