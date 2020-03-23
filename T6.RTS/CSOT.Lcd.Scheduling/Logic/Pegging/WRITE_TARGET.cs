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

namespace CSOT.Lcd.Scheduling.Logic.Pegging
{
    [FeatureBind()]
    public partial class WRITE_TARGET
    {
        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <param name="isOut"/>
        /// <param name="handled"/>
        public void WRITE_TARGET0(Mozart.SeePlan.Pegging.PegPart pegPart, bool isOut, ref bool handled)
        {

            FabPegPart pp = pegPart as FabPegPart;
            FabStep step = pegPart.CurrentStage.Tag as FabStep;
            FabProduct product = pp.Product as FabProduct;

            foreach (FabPegTarget pt in pegPart.PegTargetList)
                PegHelper.WriteStepTarget(pt, isOut, step.StepType);
        }

      

        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public object GET_STEP_PLAN_KEY0(PegPart pegPart, ref bool handled, object prevReturnValue)
        {
            if (pegPart.CurrentStage.State == "CellBankStage")
                return null;


            return (pegPart as FabPegPart).Product.ProductID;
        }

        /// <summary>
        /// </summary>
        /// <param name="pegTarget"/>
        /// <param name="stepPlanKey"/>
        /// <param name="step"/>
        /// <param name="isRun"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public Mozart.SeePlan.DataModel.StepTarget CREATE_STEP_TARGET0(Mozart.SeePlan.Pegging.PegTarget pegTarget, object stepPlanKey, Mozart.SeePlan.DataModel.Step step, bool isRun, ref bool handled, Mozart.SeePlan.DataModel.StepTarget prevReturnValue)
        {
            var pt = pegTarget as FabPegTarget;
            var st = new FabStepTarget(stepPlanKey, step, pt.Qty, pt.DueDate, isRun);
            st.Mo = pegTarget.MoPlan as FabMoPlan;

            return st;
        }
    }
}
