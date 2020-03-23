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
namespace CSOT.Lcd.Scheduling
{
    [FeatureBind()]
    public static partial class PegPartExt
    {
        public static void AddCurrentPlan(this FabPegPart pp, FabProduct prod, FabStep step)
        {
            if (prod == null || step == null)
            {
                //TODO : Write Error
                return;
            }

            PlanStep plan = new PlanStep();
            plan.Product = prod;
            plan.Step = step;
            plan.isDummy = step.IsDummy;

            pp.AddCurrentPlan(plan);
        }

        public static void AddCurrentPlan(this FabPegPart pp, PlanStep plan)
        {
            if (pp.Steps == null)
                pp.Steps = new List<PlanStep>();

            pp.Steps.Add(plan);
        }
    }
}
