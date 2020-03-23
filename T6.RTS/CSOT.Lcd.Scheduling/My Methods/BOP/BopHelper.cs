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
using CSOT.Lcd.Scheduling.Persists;
using Mozart.SeePlan.DataModel;
using Mozart.SeePlan.Lcd.DataModel;
using CSOT.Lcd.Scheduling;


namespace CSOT.Lcd.Scheduling
{
    [FeatureBind()]
    public static partial class BopHelper
    {

        public static Dictionary<string, FabStep> DummySteps { get; private set; }

        #region BopBuilder

        public static void BuildProcess(
            FabProcess proc,
            List<LcdStep> steps
            )
        {
            BopBuilder builder
                            = new BopBuilder(Mozart.SeePlan.Lcd.BopType.SINGLE_FLOW);

            builder.ComparePrevSteps = ComparePrevSteps;
            builder.CompareSteps = CompareSteps;

            builder.BuildBop(proc, steps);
        }

        public static void BuildProcess(
            FabProcess proc,
            Dictionary<string, LcdStep> steps,
            Dictionary<string, PrpInfo> prps
            )
        {
            BopBuilder builder = new BopBuilder(Mozart.SeePlan.Lcd.BopType.SINGLE_FLOW);

            builder.ComparePrevSteps = ComparePrevSteps;
            builder.CompareSteps = CompareSteps;

            builder.BuildBop(proc, steps, prps);

			foreach (var step in proc.Mappings.Values)
			{
				if (step.HasJoins == false && step.HasSplits == false)

					if (proc.NonPathSteps.ContainsKey(step.StepID) == false)
						proc.NonPathSteps.Add(step.StepID, step);
			}
        }

        public static int CompareSteps(LcdStep x, LcdStep y)
        {
            int cmp = x.Sequence.CompareTo(y.Sequence);

            if(cmp == 0)
            {
                cmp = HasNext(x).CompareTo(HasNext(y));
            }
            if (cmp == 0)
            {
                cmp = x.StepID.CompareTo(y.StepID);
            }

            return cmp;
        }

        private static int HasNext(LcdStep x)
        {
            if (x.NextStep == null)
                return 1;

            return 0;
        }

        public static int ComparePrevSteps(LcdStep x, LcdStep y)
        {
            return x.Sequence.CompareTo(y.Sequence);
        }

        #endregion


        #region Layer
        public static Layer GetSafeStdLayer(string shopID, string layerID)
        {
            Layer layer = FindLayer(shopID, layerID);

            if (layer == null)
            {
                layer = CreateHelper.CreateLayer(shopID, layerID);

                string key = GetLayerKey(shopID, layerID);
                InputMart.Instance.Layer.Add(key, layer);
            }

            return layer;
        }

        public static Layer FindLayer(string shopID, string layerID)
        {
            string key = GetLayerKey(shopID, layerID);

            Layer item;
            InputMart.Instance.Layer.TryGetValue(key, out item);

            return item;
        }

        private static string GetLayerKey(string shopID, string layerID)
        {
            return LcdHelper.CreateKey(shopID, layerID);
        }

    
        public static string GetLayerStepKey(this FabStep step)
        {
            return string.Format("{0}/{1}/{2}", step.ShopID, step.EqpGroup, step.LayerID);
        }



        public static void AddStdStep(this Layer layer, FabStdStep stdStep)
        {
            if (layer.Steps == null)
                layer.Steps = new List<FabStdStep>();

            int idx = layer.Steps.BinarySearch(stdStep, CompareHelper.FabStdStepComparer.Default);
            if (idx < 0)
                idx = ~idx;
            layer.Steps.Insert(idx, stdStep);
        }

        #endregion



        #region StdStep

        public static FabStdStep FindStdStep(string shopID, string stepID)
        {
            FabStdStep stdStep = InputMart.Instance.FabStdStep.Rows.Find(shopID, stepID);

            return stdStep;
        }
        #endregion


        #region Process
        public static string GetProcessKey(string shopID, string processID)
        {
            return string.Format("{0}/{1}", shopID, processID);
        }

        public static string GetProcessKey2(string productID, string stepID)
        {
            return LcdHelper.CreateKey(productID, stepID);
        }

        public static FabProcess FindProcess(string shopID, string processID)
        {
            string key = BopHelper.GetProcessKey(shopID, processID);

            FabProcess proc;
            InputMart.Instance.FabProcess.TryGetValue(key, out proc);

            return proc;
        }

        public static FabProcess FindProcess2(string productID, string stepID)
        {
            string key = BopHelper.GetProcessKey2(productID, stepID);

            FabProcess proc;
            InputMart.Instance.ProcessMaps.TryGetValue(key, out proc);

            return proc;
        }

        public static float GetAccumulateTat(this Process process, FabProduct product)
        {
            var currentStep = process.LastStep as FabStep;

            float cumTat = 0;

            do
            {
                if (currentStep == null)
                    break;

                var ct = currentStep.GetTat(product.ProductID, true);
                if (ct != null)
                    cumTat += ct.TAT;

                currentStep = currentStep.GetPrevMainStep(product, false);

            } while (true);

            return cumTat;
        }

        #endregion

        public static FabStep FindStep(string shopID, string processID, string stepID)
        {
            FabProcess proc = BopHelper.FindProcess(shopID, processID);
			
			if (proc != null)
                return proc.FindStep(stepID) as FabStep;
            else
                return null;
        }

        public static FabProduct FindProduct(string shopID, string productID)
        {
            string key = LcdHelper.CreateKey(shopID, productID);

            if (key == null)
                return null;

            FabProduct prod;
            InputMart.Instance.FabProduct.TryGetValue(key, out prod);

            return prod;
        }

        public static bool IsCellShop(string shopID)
        {
            if (shopID == Constants.CellShop)
                return true;

            return false;
        }

        public static bool IsArrayShop(string shopID)
        {
            if (shopID == Constants.ArrayShop)
                return true;

            return false;
        }

        public static bool IsCfShop(string shopID)
        {
            if (shopID == Constants.CfShop)
                return true;

            return false;
        }

        internal static FabStep CreateDummyStep(string factoryID, string shopID, string stepID)
        {
            FabStep step = new FabStep(stepID);

            step.FactoryID = factoryID;
            step.ShopID = shopID;
            step.StepType = "DUMMY";
            step.IsDummy = true;

            FabStdStep stdStep = new FabStdStep();
            stdStep.FactoryID = factoryID;
            stdStep.AreaID = shopID;
            stdStep.StepID = stepID;

            step.StdStep = stdStep;

            return step;
        }

        internal static FabStep GetSafeDummyStep(string factoryID, string shopID, string stepID)
        {
            if (DummySteps == null)
                DummySteps = new Dictionary<string, FabStep>();

            string key = LcdHelper.CreateKey(shopID, stepID);

            FabStep step;
            if (DummySteps.TryGetValue(key, out step) == false)
            {
                step = CreateDummyStep(factoryID, shopID, stepID);
                DummySteps.Add(key, step);

            }

            return step;
        }

        internal static void WriteProductRoute()
        {
            foreach (var prod in InputMart.Instance.FabProduct.Values)
            {
                //Array제품이 BOP상 CF에도 등록되어 있으므로 제외
                if (prod.IsArrayShopProduct() && IsCfShop(prod.ShopID))
                    continue;

				FabStep firstStep = prod.Process.FirstStep as FabStep;
                FabProduct nextProd = prod;
                FabStep currentStep = firstStep;
                FabStep prvStep = null;

                float idx = 0;
                while (true)
                {
                    idx++;
                    WriteProductRoute(nextProd, currentStep, idx);
               
                    WriteAdditionalSubStep(nextProd, currentStep, prvStep, idx);

                    FabStep nextStep = currentStep.GetNextStep(nextProd, ref nextProd);
                    if (nextStep == null)
                        break;

                    prvStep = currentStep;
                    currentStep = nextStep;

                    if (idx > 200)
                        break;
                }

				foreach (FabStep step in prod.FabProcess.NonPathSteps.Values)
				{
					WriteProductRoute(prod, step, 999);
				}
            }


        }

        private static void WriteAdditionalSubStep(FabProduct nextProd, FabStep currentStep, FabStep prvStep, float idx)
        {
            var prevSteps = currentStep.GetInnerPrevSteps();
            if (prevSteps != null)
            {
                foreach (FabStep subStep in prevSteps)
                {
                    if (subStep.IsMainStep)
                        continue;

                    if (prvStep != null && subStep == prvStep)
                        continue;

                    WriteProductRoute(nextProd, subStep, idx - 0.5f);
                }
            }
        }

        private static void WriteProductRoute(FabProduct prod, FabStep step, float idx)
        {
            Outputs.ProductRouteLog row = new Outputs.ProductRouteLog();

            row.VERSION_NO = ModelContext.Current.VersionNo;

            row.FACTORY_ID = step.FactoryID;
            row.AREA_ID = step.AreaID;
            row.SHOP_ID = step.ShopID;
            row.PRODUCT_ID = prod.ProductID;
            row.PROCESS_ID = prod.ProcessID;
            row.STEP_ID = step.StepID;
            row.STEP_DESC = step.Description;
            row.STEP_SEQ = step.StepSeq;
            row.NEXT_STEP_ID = step.NEXT_STEP_ID;
            row.SEQ = idx;

            row.DSP_EQP_GROUP_ID = step.StdStep.DspEqpGroup;
            row.IS_MANDATORY = step.StdStep.IsMandatory.ToStringYN();
            row.STEP_TYPE = step.StdStep.StepType;
            row.LAYER_ID = step.StdStep.LayerID;

            OutputMart.Instance.ProductRouteLog.Add(row);
        }


		static Dictionary<string, FabStep> _nextMainEqpStepCache = new Dictionary<string, FabStep>();

		internal static FabStep GetNextMandatoryStep(FabLot lot)
		{
			string key = LcdHelper.CreateKey(lot.CurrentProductID, lot.CurrentFabStep.Key);

			FabStep next;
			if (_nextMainEqpStepCache.TryGetValue(key, out next) == false)
			{
				FabProduct nextProd = lot.FabProduct;
				next = lot.CurrentFabStep.GetNextStep(nextProd, ref nextProd);

				int safeCnt = 0;
				while (next != null && next.StdStep.IsMandatory == false)
				{
					next = next.GetNextStep(nextProd, ref nextProd);

                    if (next == null)
						break;

					safeCnt++;
					if (safeCnt == 100)
					{
						Logger.MonitorInfo("CHECK ROUTE : {0} → NEXT MAIN EQP STEP", lot.CurrentFabStep.StepKey);
						next = null;
						break;
					}
				}

				_nextMainEqpStepCache.Add(key, next);
			}

			return next;
		}
	}


}
