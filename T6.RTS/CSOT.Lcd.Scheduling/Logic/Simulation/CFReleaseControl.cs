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
using Mozart.SeePlan.Lcd.Simulation;
using Mozart.SeePlan.Lcd.DataModel;
using Mozart.SeePlan.Simulation;

namespace CSOT.Lcd.Scheduling.Logic.Simulation
{
    [FeatureBind()]
    public partial class CFReleaseControl
    {
        /// <summary>
        /// </summary>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public IList<Mozart.SeePlan.Lcd.Simulation.SyncPartInfo> GET_SYNC_PARTS0(ref bool handled, IList<Mozart.SeePlan.Lcd.Simulation.SyncPartInfo> prevReturnValue)
        {
			//if (InputMart.Instance.GlobalParameters.ApplyFabSyncRelease == false)
			//    return null;
			return null;

            //if (SimHelper.IsCellRunning)
            //    return null;

            //List<SyncPartInfo> results = new List<SyncPartInfo>();
           
            //foreach (var cellProduct in InputMart.Instance.FabProduct.Values)
            //{
            //    if (cellProduct.ProductGroup != Constants.TftCellFront)
            //        continue;

            //    //if ( LikeUtility.Like(cellProduct.ProductID, "__S%"))
            //    //    Console.WriteLine("brad");

            //    List<FabProduct> plist = cellProduct.FindPrevAllProducts();

            //    if (plist == null)
            //        continue;

            //    FabProduct tftPart = null;
            //    FabProduct tftSortPart = null;

            //    FabProduct cfPart = null;
            //    FabProduct cfSortPart = null;

            //    foreach (FabProduct prod in plist)
            //    {
            //        if (prod.ProductGroup == Constants.TFT_Sort)
            //            tftSortPart = prod;
            //        else if (prod.ProductGroup == Constants.TFT)
            //            tftPart = prod;
            //        else if (prod.ProductGroup == Constants.CF)
            //            cfPart = prod;
            //        else if (prod.ProductGroup == Constants.CF_Sort)
            //            cfSortPart = prod;
            //    }

            //    if (tftPart == null || cfPart == null)
            //        continue;

            //    //2016.9.26 관련 function 삭제해야함
            //    //SyncProfile prof = CreateHelper.CreateSyncProfile(cellProduct, tftPart, cfPart);
            //    //profiles[cellProduct] = prof;

            //    SyncPartInfo partInfo = new SyncPartInfo();
            //    partInfo.CellPart = cellProduct;
            //    partInfo.TftPart = tftPart;
            //    partInfo.CfPart = cfPart;

            //    results.Add(partInfo);

            //    FabStep tftLastStep = tftPart.Process.LastStep as FabStep;

            //    float tftTat = tftPart.Process.GetAccumulateTat(tftPart);
            //    float tftSortTat = tftSortPart.Process.GetAccumulateTat(tftSortPart);
            //    float cfTat = cfPart.Process.GetAccumulateTat(cfPart);
            //    float cfSortTat = cfSortPart.Process.GetAccumulateTat(cfSortPart);

            //    if (tftTat + tftSortTat == 0 || cfTat + cfSortTat == 0)
            //        continue;

            //    float gap = (tftTat + tftSortTat) -
            //        (cfTat + cfSortTat + 1); //InputMart.Instance.GlobalParameters.FabSyncSurplusTime);

            //    FabStep checkStep = tftPart.Process.FirstStep as FabStep;
            //    FabProduct checkProdut = tftPart;
            //    FabStep prev = null;
            //    FabStep syncStep = null;

            //    float cumtat = 0;

            //    while (checkStep != null)
            //    {
            //        if (gap < 0)
            //        {
            //            syncStep = checkStep;
            //            break;
            //        }

            //        if (cumtat > gap)
            //        {
            //            syncStep = prev ?? checkStep;
            //            break;
            //        }
            //        //}

            //        checkStep.IsPassCfSyncInStep = false;
            //        var ctat = checkStep.GetTat(checkProdut.ProductID, true);
            //        if (ctat != null)
            //            cumtat += ctat.TAT;

            //         checkStep = checkStep.GetNextStep(checkProdut, ref checkProdut);
            //    }

            //    syncStep = syncStep ?? tftLastStep;
            //    syncStep.IsSyncStep = true;

            //    FabProcess proc = tftPart.Process as FabProcess;

            //    if (proc.SyncSteps.ContainsKey(cfPart) == false)
            //        proc.SyncSteps.Add(cfPart, syncStep);

            //    partInfo.SyncStep = syncStep;
            //    partInfo.TftTAT = tftTat;
            //    //partInfo.TFT_SORT_TAT = tftSortTat.CumTat;
            //    partInfo.CfTAT = cfTat;
            //    //partInfo.CF_SORT_TAT = cfSortTat.CumTat;
            //}

            //return results;
        }

        /// <summary>
        /// </summary>
        /// <param name="syncPart"/>
        /// <param name="wips"/>
        /// <param name="handled"/>
        public void INITIALIZE_SYNC_PART_INFO0(SyncPartInfo syncPart, IList<IHandlingBatch> wips, ref bool handled)
        {
            if (SimHelper.IsCellRunning)
                return;

            var list = wips.Where(x => (x as FabLot).Product == syncPart.TftPart ||
                                        (x as FabLot).Product == syncPart.CfPart);
            if (list == null)
                return;

            foreach (var hb in list)
            {
                var lot = hb as FabLot;

                FabStep step = lot.WipInfo.InitialStep as FabStep;
                if (step.IsPassCfSyncInStep == false)
                    continue;

                //FabProduct cellInProduct = BopHelper.GetCellInProduct(product);

                //if (cellInProduct == null)
                //    continue;

                //SyncProfile prof;
                //if (profiles.TryGetValue(cellInProduct, out prof) == false)
                //    continue;

                //CellInCompProfile comp;
                //if (prof.Profiles.TryGetValue(product.ProductID, out comp) == false)
                //    continue;

                //AgentHelper.AddLotProfile(comp, CreateHelper.CreateLotProfile(lot, DateTime.MinValue));                   

                //var syncPartInfo = manager.GetSyncPart(cellInProduct);
                //if (syncPartInfo == null)
                //    continue;

                var profileLot = new LotProfile(lot, DateTime.MinValue);
                syncPart.AddProfileLot(profileLot, lot.CurrentShopID == Constants.ArrayShop);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="syncPart"/>
        /// <param name="handled"/>
        public void WRITE_INITIAL_LOG0(Mozart.SeePlan.Lcd.Simulation.SyncPartInfo syncPart, ref bool handled)
        {
            //if (InputMart.Instance.SimulationRunType != SimRunType.TFT_CF)
            //    return;

            //Outputs.FrontSyncWipInfo sync = new Outputs.FrontSyncWipInfo();

            //sync.VERSION_NO = ModelContext.Current.VersionNo;
            //sync.CELL_PRODUCT_ID = syncPart.CellPart.ProductID;

            //sync.TFT_PRODUCT_ID = syncPart.TftPart.ProductID;
            //sync.CF_PRODUCT_ID = syncPart.CfPart.ProductID;
            //sync.TFT_WIP = syncPart.TotalTftQty;
            //sync.CF_WIP = syncPart.TotalCfQty;


            //if (syncPart.SyncStep == null)
            //    return;

            //sync.TFT_TAT = syncPart.TftTAT / 3600 / 24;
            //sync.CF_TAT = syncPart.CfTAT / 3600 / 24;
            //sync.TAT_GAP = sync.TFT_TAT - sync.CF_TAT;
            //sync.TFT_SYNC_STEP = syncPart.SyncStep.StepID;

            //OutputMart.Instance.FrontSyncWipInfo.Add(sync);
        }

        /// <summary>
        /// </summary>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public EntityEventKind GET_EVENT_TYPE0(ref bool handled, Mozart.SeePlan.Lcd.Simulation.EntityEventKind prevReturnValue)
        {
            if (SimHelper.IsCellRunning)
                return EntityEventKind.None;

            //if (!InputMart.Instance.GlobalParameters.ApplyFabSyncRelease)
            //    return EntityEventKind.None;

				

            return EntityEventKind.DispatchIn;
        }

        /// <summary>
        /// </summary>
        /// <param name="lot"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public bool IS_SYNC_TARGET_LOT0(Mozart.SeePlan.Lcd.Simulation.Lot lot, ref bool handled, bool prevReturnValue)
        {
            //FabProduct prod = lot.CurrentFabProduct;
            var fabLot = lot as FabLot;
            if (fabLot.CurrentShopID != Constants.ArrayShop)
                return false;

            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="cfPart"/>
        /// <param name="requiredQty"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public IList<Mozart.SeePlan.Simulation.IHandlingBatch> RELEASE_IN0(Mozart.SeePlan.Lcd.DataModel.Product cfPart, int requiredQty, ref bool handled, IList<Mozart.SeePlan.Simulation.IHandlingBatch> prevReturnValue)
        {
            var prod = cfPart as FabProduct;

            List<ShopInTarget> delList = new List<ShopInTarget>();

            List<IHandlingBatch> InputBatches = new List<IHandlingBatch>();

            //if (prod.ShopID == Constants.CellShop)
            //    prod = prod.GetPrevFirst();

            //if (prod == null ||prod.IsFrontProduct() == false)
            //    return null;

            //var targets = InputMart.Instance.ShopInTargetProdView.FindRows(prod).ToList<ShopInTarget>();

            //if (targets == null || targets.Count() == 0)
            //    return null;

            //targets.Sort((x, y) => x.TargetDate.CompareTo(y.TargetDate));

            //ShopInTarget first = targets.First();

            //int qty = requiredQty == int.MaxValue ? first.TargetQty : requiredQty;

            //int InputBatchLotQty = (int)Math.Ceiling((double)qty / (double)prod.CstSize);
            //string batchID = EntityHelper.CreateBatchID(prod.ProductID, AoFactory.Current.NowDT);
            //int batchSize = prod.CstSize * InputBatchLotQty;
            //int inputSum = 0;
            //int lotQty = 0;

            //ProductType prodType = ProductType.Production;

            //BatchInfo batch = EntityHelper.GetSafeBatchInfo(batchID);

            //foreach (var tg in targets)
            //{
            //    prodType = tg.ProdType;

            //    while (tg.RemainQty > 0 && inputSum < batchSize)
            //    {
            //        int currentQty = lotQty;
            //        if (tg.RemainQty > (prod.CstSize - currentQty))
            //        {
            //            lotQty += (prod.CstSize - currentQty);
            //            tg.RemainQty -= (prod.CstSize - currentQty);
            //        }
            //        else
            //        {
            //            lotQty += tg.RemainQty;
            //            tg.RemainQty = 0;
            //            delList.Add(tg);
            //        }

            //        if (lotQty == prod.CstSize)
            //        {
            //            FabLot lot = EntityHelper.CreateFrontInLot(prod, prodType, lotQty, batch);
            //            lot.FrontInTarget = tg;

            //            InputBatches.Add(lot);
            //            inputSum += lot.UnitQty;
            //            lotQty = 0;
            //        }
            //    }
            //    if (batchSize <= inputSum)
            //        break;
            //}
            //if (lotQty > 0)
            //{
            //    FabLot lot = EntityHelper.CreateFrontInLot(prod, prodType, lotQty, batch);
            //    InputBatches.Add(lot);
            //    inputSum += lot.UnitQty;
            //    targets.Last().RemainQty = 0;
            //}

            //foreach (ShopInTarget del in delList)
            //{
            //    InputMart.Instance.ShopInTarget.Rows.Remove(del);
            //}

            return InputBatches;
        }
    }
}
