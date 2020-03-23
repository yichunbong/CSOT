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
using Mozart.SeePlan;

namespace CSOT.Lcd.Scheduling.Logic.Simulation
{
    [FeatureBind()]
    public partial class InOutControl
    {
        /// <summary>
        /// </summary>
        /// <param name="agent"/>
        /// <param name="handled"/>
        public void INITIALIZE_CONTROL0(Mozart.SeePlan.Simulation.InOutAgent agent, ref bool handled)
        {
            string key = agent.Key;
            if (key == ReleasePlanMaster.AGENT_KEY)
            {                
                ReleasePlanMaster.Initialize();
            }

            //if (agent.Key == Constants.AgentKey_Front)
            //{
            //    if (InputMart.Instance.GlobalParameters.ApplyUseInPlanOnly)
            //    {
            //        InputMart.Instance.FrontInTarget.Clear();

            //        DateTime planStartTime = ShopCalendar.SplitDate(ModelContext.Current.StartTime);

            //        //foreach (InOutPlan entity in InputMart.Instance.InOutPlan.DefaultView)
            //        //{
            //        //    string factoryID = entity.FACTORY_ID;
            //        //    string shopID = entity.SHOP_ID;
            //        //    string productID = entity.PRODUCT_ID;

            //        //    //string productVersion = Constants.NULL_ID;


            //        //    //ProductType prodType = Helper.Parse<ProductType>(entity.PRODUCTION_TYPE);
                        
            //        //    string inOut = entity.IN_OUT.Trim();
            //        //    int planQty = (int)entity.PLAN_QTY;
            //        //    DateTime planDate = entity.PLAN_DATE;

            //        //    if (planQty == 0)
            //        //        continue;

            //        //    if (planDate < planStartTime)
            //        //        continue;

            //        //    if ((inOut == "IN" && shopID == Constants.ArrayShop) == false)
            //        //        continue;

            //        //    FabProduct prod = BopHelper.FindProduct(shopID, productID);
            //        //    if (prod == null)
            //        //        continue;

            //        //    FabMoPlan plan = CreateHelper.CreateMoPlan(entity, null);
            //        //    plan.ShopID = shopID;
            //        //    plan.DueDate = entity.PLAN_DATE;
            //        //    plan.DemandID = productID + entity.PLAN_DATE.DbToStringTrimSec();

            //        //    FrontInTarget inTarget = InputMart.Instance.FrontInTargetView.FindRows(prod, plan.DueDate).FirstOrDefault();
                     
            //        //    if (inTarget == null)
            //        //    {

            //        //        //TODO : 실적차감
            //        //        int actQty = 0;
                            
            //        //        //if (ShopCalendar.StartTimeOfDay(planDate) == ShopCalendar.StartTimeOfDay(ModelContext.Current.StartTime))
            //        //        //{
            //        //        //    actQty = (int)InputMart.Instance.InOutActProdView.FindRows(productID, inOut)
            //        //        //        .Where(x => x.SHOP_ID == shopID && x.PRODUCT_TYPE == prodType.ToString() )
            //        //        //        .Sum(x => x.ACT_QTY);
            //        //        //}

            //        //        inTarget = new FrontInTarget();

            //        //        inTarget.Product = prod;
            //        //        inTarget.ProductGroup = inTarget.Product.ProductGroup;
            //        //        inTarget.TargetDate = plan.DueDate;
            //        //       // inTarget.ProdType = prodType;
            //        //        inTarget.TargetQty = planQty;

            //        //        int inQty = planQty - actQty > 0 ? planQty - actQty : 0;
            //        //        if (inQty <= 0)
            //        //            continue;

            //        //        inTarget.RemainQty = inQty;

            //        //        InputMart.Instance.FrontInTarget.Rows.Add(inTarget);
            //        //    }
            //        //    else
            //        //    {
            //        //        inTarget.TargetQty += planQty;
            //        //        inTarget.RemainQty += planQty;
            //        //    }

            //        //}
            //    }
            //}
        }

        /// <summary>
        /// </summary>
        /// <param name="agent"/>
        /// <param name="waits"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public List<IHandlingBatch> RUN_AGENT0(Mozart.SeePlan.Simulation.InOutAgent agent, List<Mozart.SeePlan.Simulation.IHandlingBatch> waits, ref bool handled, List<Mozart.SeePlan.Simulation.IHandlingBatch> prevReturnValue)
        {
            DateTime now = agent.NowDT;

            string key = agent.Key;
            if (key == ReleasePlanMaster.AGENT_KEY)
            {                
                ReleasePlanMaster.Allocate(agent);
            }

            if (SimHelper.IsTftRunning)
                return null;

            List<IHandlingBatch> rlots = new List<IHandlingBatch>();
            foreach (var hb in waits)
            {
                var lot = hb.Sample as FabLot;
                if (lot.ReleaseTime <= now)
                {
                    rlots.Add(lot);

                    //System.Diagnostics.Debug.WriteLine(string.Format("{0}/{1}/{2}/{3}", lot.LotID, lot.CurrentProductID, lot.UnitQty, lot.ReleaseTime));
                }

            }

            handled = true;
            return rlots;
        }

        /// <summary>
        /// </summary>
        /// <param name="agent"/>
        /// <param name="waits"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public List<IHandlingBatch> RUN_FRONT_IN_AGENT(Mozart.SeePlan.Simulation.InOutAgent agent, List<Mozart.SeePlan.Simulation.IHandlingBatch> waits, ref bool handled, List<Mozart.SeePlan.Simulation.IHandlingBatch> prevReturnValue)
        {
            if (SimHelper.IsCellRunning)
                return prevReturnValue;

            List<ShopInTarget> delList = new List<ShopInTarget>();

            DateTime inputLimitDate = ModelContext.Current.StartTime == ShopCalendar.StartTimeOfDayT(ModelContext.Current.StartTime) ?
                ModelContext.Current.StartTime.AddDays(InputMart.Instance.GlobalParameters.period - 1)
                : ShopCalendar.StartTimeOfDayT(ModelContext.Current.StartTime.AddDays(InputMart.Instance.GlobalParameters.period));

            // 두시간에 한번씩 투입을 하면서 현재 시간보다 이전 Target 을 묶어 배치 사이즈에 맞추어 투입함
            List<IHandlingBatch> InputBatches = new List<IHandlingBatch>();
            foreach (FabProduct prod in InputMart.Instance.FabProduct.Values)
            {
                if (prod.IsFrontProduct(false) == false)
                    continue;

                //if (InputMart.Instance.GlobalParameters.ApplyFabSyncRelease)
                //{
                //    if (prod.ShopID == Constants.CFShop)
                //        continue;
                //}
                //var targets = InputMart.Instance.ShopInTargetProdView.FindRows(prod).ToList<ShopInTarget>();
                //targets.Sort((x, y) => x.TargetDate.CompareTo(y.TargetDate));

                //if (targets == null || targets.Count() == 0)
                //    continue;

                //ShopInTarget first = targets.First();
                ////var targetDate = ShopCalendar.StartTimeOfNextDayT(first.TargetDate);
                //var targetDate = first.TargetDate;

                //if (targetDate > agent.NowDT || targetDate > inputLimitDate)
                //    continue;

                //int InputBatchLotQty = (int)Math.Ceiling((double)first.RemainQty / (double)prod.CstSize);
                //string batchID = EntityHelper.CreateBatchID(prod.ProductID, agent.NowDT);
                //int batchSize = prod.CstSize * InputBatchLotQty;

                //int inputSum = 0;
                //int lotQty = 0;

                //ProductType prodType = ProductType.Production;

                //BatchInfo info = EntityHelper.GetSafeBatchInfo(batchID);

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
                //            // 투입 Lot 생성
                //            FabLot lot = EntityHelper.CreateFrontInLot(prod, prodType, lotQty, info);
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
                //    FabLot lot = EntityHelper.CreateFrontInLot(prod, prodType, lotQty, info);
                //    InputBatches.Add(lot);
                //   // targets.Last().RemainQty = 0;
                //}
            }

            foreach (ShopInTarget del in delList)
            {
                InputMart.Instance.ShopInTarget.Rows.Remove(del);
            }

            return InputBatches; 
        }

        /// <summary>
        /// </summary>
        /// <param name="agent"/>
        /// <param name="hb"/>
        /// <param name="handled"/>
        public void ON_EXIT_CONTROL0(Mozart.SeePlan.Simulation.InOutAgent agent, Mozart.SeePlan.Simulation.IHandlingBatch hb, ref bool handled)
        {
            //if (InputMart.Instance.SimulationRunType != SimRunType.TFT_CF)
            //    return;

            //Outputs.EqpPlan plan = new Outputs.EqpPlan();

            //FabLot lot = hb.Sample as FabLot;

            //plan.VERSION_NO = ModelContext.Current.VersionNo;
            //plan.FACTORY_ID = lot.CurrentFactoryID;
            //plan.SHOP_ID = lot.CurrentShopID;
            //plan.LOT_ID = lot.LotID;
            //plan.BATCH_ID = lot.BatchID;
            //plan.STEP_ID = lot.CurrentShopID + "00000";
            //plan.STEP_TYPE = "Front-In";
            //plan.PRODUCT_ID = lot.CurrentProductID;
            //plan.UNIT_QTY = lot.UnitQty;
            //plan.EQP_ID = string.IsNullOrEmpty(lot.CurrentPlan.ResID) ? Constants.NULL_ID : lot.CurrentPlan.ResID;
            //plan.LAYER_ID = lot.CurrentFabStep.LayerID;
            //plan.PROCESS_ID = lot.CurrentProcessID;
            //plan.PRODUCT_KIND = lot.CurrentFabProduct.ProductKind;
            //plan.START_TIME = agent.NowDT;
            //plan.END_TIME = agent.NowDT;
            //plan.EQP_STATUS = "BUSY";
            //plan.WIP_STEP_ID = Constants.NULL_ID;

            //if (lot.FrontInTarget != null)
            //{
            //    var mo = lot.FrontInTarget.Mo.FirstOrDefault();
            //    if (mo != null)
            //    {
            //        plan.TARGET_DATE = lot.FrontInTarget.TargetDate;
            //        plan.DEMAND_ID = mo.DemandID;
            //        plan.DEMAND_PLAN_DATE = mo.DueDate;
            //        plan.TAR_PLAN_QTY = Convert.ToDecimal(mo.Qty);
            //        plan.TARGET_PRODUCT_ID = mo.ProductID;
            //    }
            //}

            //OutputMart.Instance.EqpPlan.Add(plan);

            //InFlowAgent.ChangeWipLocation(lot, EventType.Release);
        }
    }
}
