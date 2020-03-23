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
using Mozart.SeePlan.DataModel;
using Mozart.Simulation.Engine;

namespace CSOT.Lcd.Scheduling.Logic.Simulation
{
    [FeatureBind()]
    public partial class Route
    {
        /// <summary>
        /// </summary>
        /// <param name="da"/>
        /// <param name="hb"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public IList<string> GET_LOADABLE_EQP_LIST0(Mozart.SeePlan.Simulation.DispatchingAgent da, Mozart.SeePlan.Simulation.IHandlingBatch hb, ref bool handled, IList<string> prevReturnValue)
        {
            var lot = hb.Sample as FabLot; 

            if ((hb.CurrentStep as FabStep).StdStep.IsMandatory == false)
                return null;

            List<string> eqps = EqpArrangeMaster.GetLoadableEqpList(lot);

            return eqps;
        }

        /// <summary>
        /// </summary>
        /// <param name="lot"/>
        /// <param name="task"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public LoadInfo CREATE_LOAD_INFO0(Mozart.SeePlan.Simulation.ILot lot, Mozart.SeePlan.DataModel.Step task, ref bool handled, Mozart.SeePlan.DataModel.LoadInfo prevReturnValue)
        {
            FabLot flot = lot as FabLot;
            FabStep step = task as FabStep;

            FabPlanInfo info = new FabPlanInfo(step);
            info.ShopID = step.ShopID;
            info.LotID = flot.LotID;
            info.Product = flot.FabProduct;
            info.UnitQty = flot.UnitQty;

            info.ProductID = info.Product.ProductID;
            info.ProcessID = info.Product.ProcessID;

            info.OwnerType = flot.OwnerType;
            info.OwnerID = flot.OwnerID;            

            //Change ProductVersion
            flot.CurrentProductVersion = step.IsArrayShop ? flot.OrigProductVersion : "00001";

            info.ProductVersion = flot.CurrentProductVersion;
                        
            if (flot.CurrentProcessID != info.ProcessID)
            {
                flot.Route = step.Process;
            }

            info.WipInfo = flot.Wip;
            info.Lot = flot;

            info.LotFilterInfo = new LotFilterInfo();
            info.LotFilterInfo.FilterType = DispatchFilter.None;
            info.LotFilterInfo.Reason = Constants.NULL_ID;
            info.LotFilterInfo.RecipeTimes = new Dictionary<string, EqpRecipeInfo>();

			if (flot.PlanSteps == null)
				flot.PlanSteps = new List<string>();

			flot.PlanSteps.Add(step.StepKey);

            return info;
        }

        /// <summary>
        /// </summary>
        /// <param name="hb"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public bool IS_INPUT_CONTROL0(Mozart.SeePlan.Simulation.IHandlingBatch hb, ref bool handled, bool prevReturnValue)
        {
            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="hb"/>
        /// <param name="infos"/>
        /// <param name="handled"/>
        public void PRE_PEGGING0(Mozart.SeePlan.Simulation.IHandlingBatch hb, List<Mozart.SeePlan.DataModel.PeggingInfo> infos, ref bool handled)
        {
            //FabLot lot = hb.Sample as FabLot;
        }

        /// <summary>
        /// </summary>
        /// <param name="hb"/>
        /// <param name="now"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public IHandlingBatch[] STEP_CHANGE0(Mozart.SeePlan.Simulation.IHandlingBatch hb, DateTime now, ref bool handled, Mozart.SeePlan.Simulation.IHandlingBatch[] prevReturnValue)
        {
            FabLot lot = hb.ToFabLot();
            FabPlanInfo plan = lot.CurrentFabPlan;


            /*
             * StepChange
            */
            ILot[] lots = hb.StepChange(now);

            //설비의 OutPort에서 Lot을 모으는 InterceptMove를 사용할 경우 해당 위치에서 별도 집계 필요함.
            InFlowMaster.ChangeWipLocation(hb, EventType.TrackOut);
            

            //FabPlanInfo prev = lot.PreviousFabPlan;

            ////InLineMap 다음StepSkip
            //if (prev.IsLoaded && prev.FabStep.StdStep.IsBaseEqp(prev.ResID))
            //{
            //    plan = lot.CurrentFabPlan;

            //    plan.TransferStartTime = now;
            //    plan.TransferEndTime = now;
            //    plan.EqpInEndTime = now;
            //    plan.EqpInStartTime = now;

            //    plan.Start(now, null);
            //    plan.End(now, null);

            //    lots = hb.StepChange(now);
            //}

            QTimeMaster.StepChange(lots, now);

            return lots;
        }



        /// <summary>
        /// </summary>
        /// <param name="lot"/>
        /// <param name="loadInfo"/>
        /// <param name="step"/>
        /// <param name="now"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public Step GET_NEXT_STEP1(Mozart.SeePlan.Simulation.ILot lot, Mozart.SeePlan.DataModel.LoadInfo loadInfo, Mozart.SeePlan.DataModel.Step step, DateTime now, ref bool handled, Mozart.SeePlan.DataModel.Step prevReturnValue)
        {
            FabLot flot = lot as FabLot;
            var fstep = step as FabStep;
            FabPlanInfo plan = loadInfo as FabPlanInfo;

            FabProduct prod = flot.FabProduct;                          

            FabProduct nextProd = prod;
            var nextStep = GetNextStep(fstep, prod, plan, ref nextProd);

            flot.Product = nextProd;

            return nextStep;
        }
        
        private FabStep GetNextStep(FabStep step, FabProduct product, FabPlanInfo plan, ref FabProduct nextProduct)
        {
            string eqpID = plan.ResID;            
            var targEqp = ResHelper.FindEqp(eqpID);

            string eqpGroup = targEqp == null ? null : targEqp.EqpGroup;
            string runMode = plan.EqpLoadInfo == null ? null : plan.EqpLoadInfo.RunMode;
            string productID = plan.ProductID;

            var branchStep = BranchStepMaster.GetBranchStep(eqpGroup, runMode, productID);
            if(branchStep != null)
            {
                var currProd = product;
                var currStep = step.GetNextStep(currProd, ref currProd);

                bool existStep = false;
                while (currStep != null)
                {
                    if (currStep.StepID == branchStep.NextStepID)
                    {
                        existStep = true;
                        break;
                    }

                    currStep = currStep.GetNextStep(currProd, ref currProd);
                }

                if(existStep)
                {
                    nextProduct = currProd;
                    return currStep;
                }
            }

            return step.GetNextStep(product, ref nextProduct);
        }

        //public Step GET_NEXT_STEP1(Mozart.SeePlan.Simulation.ILot lot, Mozart.SeePlan.DataModel.LoadInfo loadInfo, Mozart.SeePlan.DataModel.Step step, DateTime now, ref bool handled, Mozart.SeePlan.DataModel.Step prevReturnValue)
        //{
        //    FabPlanInfo plan = loadInfo as FabPlanInfo;
        //    FabProduct prod = plan.Product;
        //    FabLot mlot = lot as FabLot;

        //    #region PartChange
        //    if (prod.HasNextInterBom)
        //    {
        //        FabInterBom bom;
        //        prod.TryGetNextInterRoute(step as FabStep, out bom);

        //        if (bom != null)
        //        {
        //            plan.InterBom = bom;
        //            mlot.Product = bom.ChangeProduct;

        //        }
        //    }
        //    #endregion

        //    if (plan.InterBom != null)
        //        return plan.InterBom.ChangeStep;

        //    return step.GetDefaultNextStep();
        //}

        /// <summary>
        /// </summary>
        /// <param name="hb"/>
        /// <param name="handled"/>
        public void ON_DONE0(Mozart.SeePlan.Simulation.IHandlingBatch hb, ref bool handled)
        {
            FabLot lot = hb.ToFabLot();

            InFlowMaster.OnDoneWipLocation(hb);

            if (SimHelper.IsTftRunning)
                InOutProfileMaster.AddOut(lot, AoFactory.Current.NowDT);
        }

        /// <summary>
        /// </summary>
        /// <param name="hb"/>
        /// <param name="ao"/>
        /// <param name="now"/>
        /// <param name="handled"/>
        public void ON_START_TASK0(Mozart.SeePlan.Simulation.IHandlingBatch hb, Mozart.Simulation.Engine.ActiveObject ao, DateTime now, ref bool handled)
        {
            FabLot lot = hb.ToFabLot();
            hb.Apply((x, y) => LoadHelper.OnStartTask(x as FabLot));

            InFlowMaster.ChangeWipLocation(hb, EventType.TrackIn);

            if (ao is AoEquipment)
            {
                FabAoEquipment eqp = ao as FabAoEquipment;

                MaskMaster.StartTask(lot, eqp);
				JigMaster.StartTask(lot, eqp);

                //TODO : 설비의 Property로 작성필요 (LastPlan의 Plan을 보고)
                if (lot.CurrentFabPlan.CurrentRecipeTime != null)
                    eqp.IsEqpRecipeRun = true;
                else
                    eqp.IsEqpRecipeRun = false;                
            }

            OutCollector.Write_Rtd_LotUpkTracking(lot);
        }

        /// <summary>
        /// </summary>
        /// <param name="hb"/>
        /// <param name="ao"/>
        /// <param name="now"/>
        /// <param name="handled"/>
        public void ON_END_TASK0(Mozart.SeePlan.Simulation.IHandlingBatch hb, Mozart.Simulation.Engine.ActiveObject ao, DateTime now, ref bool handled)
        {
			FabLot lot = hb.ToFabLot();
			hb.Apply((x, y) => LoadHelper.OnEndTask(x as FabLot));

            if (ao is AoEquipment)
            {
                FabAoEquipment aeqp = ao as FabAoEquipment;                

                MaskMaster.EndTask(lot, aeqp);
				JigMaster.EndTask(lot, aeqp);
            }			
        }

        /// <summary>
        /// Batch Input으로 처음들어오는 곳
        /// </summary>
        /// <param name="factory"/>
        /// <param name="hb"/>
        /// <param name="handled"/>
        public void ON_RELEASE0(AoFactory factory, IHandlingBatch hb, ref bool handled)
        {
			hb.MoveFirst(factory.NowDT);

            FabLot lot = hb.ToFabLot();
            
            if (lot.ReleasePlan != null)
                lot.ReleasePlan.IsRelease = true;

            InFlowMaster.ChangeWipLocation(hb, EventType.Release);

			OutCollector.CollectInputLot(lot);
			OutCollector.WriteInputLotLog(lot, factory.NowDT);
        }

        /// <summary>
        /// </summary>
        /// <param name="da"/>
        /// <param name="hb"/>
        /// <param name="handled"/>
        public void ON_DISPATCH_IN0(Mozart.SeePlan.Simulation.DispatchingAgent da, Mozart.SeePlan.Simulation.IHandlingBatch hb, ref bool handled)
        {

         
        }
    }
}
