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
using Mozart.SeePlan.Pegging;
using Mozart.SeePlan;
using Mozart.SeePlan.DataModel;

namespace CSOT.Lcd.Scheduling
{
    [FeatureBind()]
    public static partial class PegHelper
    {
        

        internal static FabPegPart ToFabPegPart(this PegPart pegPart)
        {
            return pegPart as FabPegPart;
        }


        internal static FabPegTarget ToFabPegTarget(this PegTarget pegTarget)
        {
            return pegTarget as FabPegTarget;
        }


        internal static FabPlanWip ToFabPlanWip(this IMaterial m)
        {
            return m as FabPlanWip;
        }

        internal static int ComparePegPart(PegPart x, PegPart y)
        {
            FabPegPart a = x.ToFabPegPart();
            FabPegPart b = y.ToFabPegPart();

            int cmp = a.SampleMs.DueDate.CompareTo(b.SampleMs.DueDate);

            if (cmp == 0)
                cmp = a.SampleMs.LineType.CompareTo(b.SampleMs.LineType);

            if (cmp == 0)
                cmp = a.SampleMs.Qty.CompareTo(b.SampleMs.Qty);

            if (cmp == 0)
                cmp = a.Product.ProductID.CompareTo(b.Product.ProductID);

            return cmp;


        }


        internal static int ComparePegTarget(PegTarget x, PegTarget y)
        {
            FabPegTarget a = x.ToFabPegTarget();
            FabPegTarget b = y.ToFabPegTarget();

            int cmp = a.DueDate.CompareTo(b.DueDate);

            if (cmp == 0)
                cmp = a.LineType.CompareTo(b.LineType);

            if (cmp == 0)
                cmp = a.Qty.CompareTo(b.Qty);

            return cmp;
        }

        internal static void WritePeg(PegTarget target, IMaterial m, double qty)
        {
            Outputs.PegHistory hist = new Outputs.PegHistory();

            FabStep step = target.PegPart.CurrentStage.Tag as FabStep;
            FabPegTarget pt = target as FabPegTarget;
            FabPlanWip wip = m as FabPlanWip;
            FabMoPlan mo = pt.MoPlan as FabMoPlan;
            var prod = (pt.PegPart as FabPegPart).Product as FabProduct;

            hist.VERSION_NO = ModelContext.Current.VersionNo;
            hist.FACTORY_ID = step.FactoryID;
            hist.AREA_ID = step.AreaID;
            hist.SHOP_ID = step.ShopID;
            hist.PRODUCT_ID = prod.ProductID;
            hist.PRODUCT_VERSION = wip.WipInfo.ProductVersion;
            hist.PRODUCT_TYPE = prod.ProductType.ToString();
            hist.PROCESS_ID = prod.ProcessID;
            hist.STEP_ID = step.StepID;
            hist.PEG_QTY = qty;
            hist.LOT_ID = wip.Wip.LotID;
            hist.LOT_QTY = wip.Wip.UnitQty;
            hist.LOT_STATUS = wip.LotState.ToString();
            hist.TARGET_DATE = pt.CalcDate;
            hist.TARGET_PRODUCT_ID = pt.ProductID;
            //hist.TARGET_PROCESS_ID = pt.ProcessID;

            hist.DEMAND_ID = mo.DemandID;
            hist.DEMAND_PRODUCT_ID = mo.PreMoPlan.ProductID;
            hist.DEMAND_PLAN_DATE = mo.DueDate;
            hist.DEMAND_QTY = mo.Qty;

            hist.WIP_PROCESS_ID = wip.WipInfo.WipProcessID;
            hist.WIP_STEP_ID = wip.WipInfo.WipStepID;
            hist.WIP_STATE = wip.WipInfo.WipState;

            hist.OWNER_TYPE = wip.WipInfo.OwnerType;

            hist.TARGET_KEY = pt.TargetKey;

            OutputMart.Instance.PegHistory.Add(hist);


        }

        internal static void WriteActPeg(PreMoPlan target, IMaterial m, double qty)
        {
            InOutAct wip = m as InOutAct;

            Outputs.ActPegHistory hist = new ActPegHistory();

            hist.VERSION_NO = ModelContext.Current.VersionNo;
            hist.FACTORY_ID = target.FactoryID;
            hist.SHOP_ID = target.ShopID;
            hist.PEG_QTY = qty;
            hist.RPT_DATE = wip.AvailableTime;
            hist.PRODUCT_ID = wip.ProductID;
            hist.PRODUCT_VERSION = wip.ProductVersion;
            hist.OWNER_TYPE = wip.OwnerType;
            hist.OWNER_ID = wip.OwnerID;

            hist.ACT_IN_QTY = wip.InQty;
            hist.ACT_OUT_QTY = wip.OutQty;

            hist.DEMAND_ID = target.DemandID;
            hist.DEMAND_PRODUCT_ID = target.ProductID;
            hist.DEMAND_PLAN_DATE = target.DueDate;
            hist.DEMAND_QTY = target.Qty;

            OutputMart.Instance.ActPegHistory.Add(hist);

        }

        //Post Process
        internal static void WriteUnpegHistory(FabPlanWip wip, string reason)
        {
            Outputs.UnpegHistory hist = new Outputs.UnpegHistory();

            hist.VERSION_NO = ModelContext.Current.VersionNo;

            hist.FACTORY_ID = wip.FactoryID;
            hist.AREA_ID = wip.AreaID;
            hist.SHOP_ID = wip.ShopID;
            hist.STEP_ID = wip.StepID;

            hist.LOT_ID = wip.WipInfo.LotID;

            hist.UNPEG_REASON = reason;
            hist.REMAIN_QTY = wip.Qty;

            hist.PRODUCT_ID = wip.ProductID;
            hist.PRODUCT_VERSION = wip.ProductVersion;
            hist.PROCESS_ID = wip.ProcessID;

            hist.LOT_QTY = wip.Qty;
            hist.LOT_STATUS = wip.LotState.ToString();


            hist.HOLD_CODE = wip.WipInfo.HoldCode;
            hist.WIP_PROCESS_ID = wip.WipInfo.WipProcessID;
            hist.WIP_STEP_ID = wip.WipInfo.WipStepID;
            hist.WIP_STATE = wip.WipInfo.WipState;

            hist.OWNER_TYPE = wip.WipInfo.OwnerType;

            OutputMart.Instance.UnpegHistory.Add(hist);
        }

        //Wip Persist
        internal static void WriteUnpegHistory(Wip item, string reason, FabStep mainStep)
        {
            Outputs.UnpegHistory hist = new Outputs.UnpegHistory();

            hist.VERSION_NO = ModelContext.Current.VersionNo;
            hist.FACTORY_ID = item.FACTORY_ID;
            hist.AREA_ID = Constants.NULL_ID;
            hist.SHOP_ID = item.SHOP_ID;
            hist.LOT_ID = item.LOT_ID;
            hist.UNPEG_REASON = reason;
            hist.REMAIN_QTY = item.GLASS_QTY;
            hist.PRODUCT_ID = item.PRODUCT_ID;
            hist.PRODUCT_VERSION = item.PRODUCT_VERSION;

            hist.PROCESS_ID = item.PROCESS_ID;
            hist.STEP_ID = item.STEP_ID;

            if (mainStep != null)
            {
                hist.PROCESS_ID = mainStep.ProcessID;
                hist.STEP_ID = mainStep.StepID;
            }

            hist.LOT_QTY = item.PANEL_QTY;
            hist.HOLD_CODE = item.HOLD_CODE;
            hist.LOT_STATUS = item.LOT_STATUS;

            hist.OWNER_TYPE = item.OWNER_TYPE;

            hist.WIP_PROCESS_ID = item.PROCESS_ID;
            hist.WIP_STEP_ID = item.STEP_ID;
            hist.WIP_STATE = item.LOT_STATUS;

            OutputMart.Instance.UnpegHistory.Add(hist);
        }

        internal static void WriteUnpegHistory(BankWip item, string reason)
        {
            Outputs.UnpegHistory hist = new Outputs.UnpegHistory();

            hist.VERSION_NO = ModelContext.Current.VersionNo;
            hist.FACTORY_ID = item.FACTORY_ID;
            hist.AREA_ID = Constants.NULL_ID;
            hist.SHOP_ID = item.SHOP_ID;
            hist.LOT_ID = item.LOT_ID;
            hist.UNPEG_REASON = reason;
            hist.REMAIN_QTY = item.GLASS_QTY;
            hist.PRODUCT_ID = item.PRODUCT_ID;
            hist.PRODUCT_VERSION = item.PRODUCT_VERSION;

            hist.PROCESS_ID = item.PROCESS_ID;
            hist.STEP_ID = item.STEP_ID;

            hist.LOT_QTY = item.PANEL_QTY;
            hist.HOLD_CODE = Constants.NULL_ID;
            hist.LOT_STATUS = item.LOT_STATUS;

            hist.OWNER_TYPE = item.OWNER_TYPE;

            hist.WIP_PROCESS_ID = item.PROCESS_ID;
            hist.WIP_STEP_ID = item.STEP_ID;
            hist.WIP_STATE = item.LOT_STATUS;

            OutputMart.Instance.UnpegHistory.Add(hist);
        }


        internal static void WriteStepTarget(FabPegTarget pt, bool isOut, string stepType)
        {
            WriteStepTarget(pt, isOut, stepType, false);
        }

        internal static void WriteStepTarget(FabPegTarget pt, bool isOut, string stepType, bool isExtraAdd)
        {
            FabPegPart pp = pt.PegPart as FabPegPart;
            FabStep step = pp.Current.Step;
            FabProduct prod = pp.FabProduct;  

            //Key : FactoryID, ShopID, ProductID, StepID, StepType, TargetShift, TargetKey
            string factoryID = step.FactoryID;
            string shopID = step.ShopID;
            string prodductID = prod.ProductID;
            string stepID = step.StepID;

            stepType = GetStepType(stepType);

            if (stepType == Constants.IN)
                stepID = string.Format("{0}_{1}", shopID, Constants.IN);
            else if (stepType == Constants.OUT)
                stepID = string.Format("{0}_{1}", shopID, Constants.OUT);


            DateTime targetShift = ShopCalendar.ShiftStartTimeOfDayT(pt.CalcDate);
            string targetKey = pt.TargetKey;

            Outputs.StepTarget row
               = OutputMart.Instance.StepTarget.Find(factoryID, shopID, prodductID, stepID, stepType, targetShift, targetKey);

            if (row != null)
            {
                if (isOut)
                {
                    row.TARGET_OUT_QTY = +Convert.ToDecimal(pt.CalcQty);
                }
                else
                {
                    row.TARGET_IN_QTY = +Convert.ToDecimal(pt.CalcQty);
                }

            }
            else
            {
                row = new Outputs.StepTarget();

                row.VERSION_NO = ModelContext.Current.VersionNo;
                row.FACTORY_ID = step.FactoryID;
                row.AREA_ID = step.AreaID;
                row.SHOP_ID = step.ShopID;
                row.PRODUCT_ID = prod.ProductID;
                row.STEP_ID = stepID;

                row.TARGET_DATE = pt.CalcDate;
                row.TARGET_SHIFT = targetShift;

                row.DEMAND_ID = pt.FabMoPlan.DemandID;
                row.DEMAND_PRODUCT_ID = pt.FabMoPlan.PreMoPlan.ProductID;
                row.DEMAND_PLAN_DATE = pt.FabMoPlan.DueDate;
                row.DEMAND_QTY = pt.FabMoPlan.Qty;

                row.TARGET_KEY = pt.TargetKey;

                row.STEP_TYPE = stepType;
				row.SEQ = pt.Seq++;// GetSequence(step, stepType, isExtraAdd);

				if (isOut)
                {
                    row.TARGET_IN_QTY = 0;
                    row.TARGET_OUT_QTY = Convert.ToDecimal(pt.CalcQty);
                }
                else
                {
                    row.TARGET_IN_QTY = Convert.ToDecimal(pt.CalcQty);
                    row.TARGET_OUT_QTY = 0;
                }

                //InTarget용(BuildInPlan)
                if (isExtraAdd)
                {
                    row.TARGET_IN_QTY = Convert.ToDecimal(pt.CalcQty);
                    row.TARGET_OUT_QTY = Convert.ToDecimal(pt.CalcQty);
                }



                OutputMart.Instance.StepTarget.Add(row);
            }
        }

        private static string GetStepType(string stepType)
        {
            string type = stepType.IsNullOrEmpty() ? Constants.NULL_ID : stepType;

            switch (stepType)
            {
                case "DUMMY":
                    type = "BANK";
                    break;

            }

            return type;
        }

        private static int GetSequence(FabStep step, string stepType, bool isExtraAdd)
        {
            if (isExtraAdd == false)
                return step.StepSeq;

            if (stepType == Constants.OUT)
                return step.StepSeq + 1;

            return -1;
        }

        private static string GetStepType(FabStep step, bool isOut)
        {
            if (isOut == false && step.IsFirst)
                return Constants.IN.ToString();

            return step.StepType;
        }



        internal static Step GetLastPeggingStgep(PegPart pegPart)
        {
            FabPegPart pp = pegPart as FabPegPart;
            FabProduct product = pp.Product as FabProduct;
            Step step = product.Process.LastStep;

            pp.AddCurrentPlan(product, step as FabStep);

            //StepTarget 추가기록
            foreach (FabPegTarget pt in pp.PegTargetList)
                PegHelper.WriteStepTarget(pt, true, Constants.OUT, true);

            return step;
        }

        internal static Step GetPrevPeggingStep(PegPart pegPart, Step currentStep)
        {
            Step prevStep = currentStep.GetDefaultPrevStep();

            FabPegPart pp = pegPart.ToFabPegPart();
            if (pp.HasInterBom)
            {
                prevStep = pp.InterBom.ChangeStep;
                pp.InterBom = null;
            }

            pp.AddCurrentPlan(pp.FabProduct, prevStep as FabStep);

            return prevStep;
        }

        internal static string CreateTargetKey(string idx, string postfix)
        {
            string key = string.Format("{0}{1}", idx, postfix);

            return key.Trim();
        }
    }
}
