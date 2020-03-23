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

namespace CSOT.Lcd.Scheduling.Logic.Pegging
{
    [FeatureBind()]
    public partial class SHIFT_TAT
    {

        /// <summary>
        /// </summary>
        /// <param name="pegTarget"/>
        /// <param name="oldDueDate"/>
        /// <param name="newDueDate"/>
        /// <param name="tat"/>
        /// <param name="isRun"/>
        /// <param name="handled"/>
        public void UPDATE_TAT_INFO0(Mozart.SeePlan.Pegging.PegTarget pegTarget, DateTime oldDueDate, DateTime newDueDate, TimeSpan tat, bool isRun, ref bool handled)
        {

        }

        /// <summary>
        /// </summary>
        /// <param name="pegTarget"/>
        /// <param name="stage"/>
        /// <param name="isRun"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public TimeSpan GET_TARGET_TAT0(Mozart.SeePlan.Pegging.PegTarget pegTarget, Mozart.SeePlan.Pegging.PegStage stage, bool isRun, ref bool handled, TimeSpan prevReturnValue)
        {
            FabPegTarget target = pegTarget as FabPegTarget;
            FabStep step = pegTarget.PegPart.CurrentStep as FabStep;

            float waitTat = (float)SiteConfigHelper.GetDefaultWaitTAT().TotalMinutes;
            float runTat = (float)SiteConfigHelper.GetDefaultRunTAT().TotalMinutes;            

            StepTat tat = step.GetTat(target.ProductID, target.IsMainLint);
            if(tat != null)
            {
                waitTat = tat.WaitTat;
                runTat = tat.RunTat;                
            }

            float time = isRun ? runTat : waitTat;

            return TimeSpan.FromMinutes(time);
        }

        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <param name="stage"/>
        /// <param name="isRun"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public bool USE_TARGET_TAT0(PegPart pegPart, PegStage stage, bool isRun, ref bool handled, bool prevReturnValue)
        {
            return true;
        }
    }
}
