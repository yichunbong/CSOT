using CSOT.Lcd.Scheduling.Persists;
using CSOT.Lcd.Scheduling.Outputs;
using CSOT.Lcd.Scheduling.Inputs;
using CSOT.Lcd.Scheduling.DataModel;
using Mozart.Task.Execution;
using Mozart.Extensions;
using Mozart.Collections;
using Mozart.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using Mozart.Simulation.Engine;
using Mozart.SeePlan.Simulation;

namespace CSOT.Lcd.Scheduling
{
    [FeatureBind()]
    public static partial class DebugHelper
    {        
        public static void AddDebugTime(AoFactory aoFactory)
        {
            if (GlobalParameters.Instance.ApplyDebug == false)
                return;
            
            DateTime planStartTime = ModelContext.Current.StartTime;
            DateTime planEndTime = ModelContext.Current.EndTime;

            DateTime debugTime = GlobalParameters.Instance.DebugTime;
            if (debugTime < planStartTime || debugTime >= planEndTime)
                return;

            Time delay = debugTime - planStartTime;

            //StepWip
            if (delay > 0)
                aoFactory.AddTimeout(delay, DebugHelper.OnWriteStepWip);
        }
        
        private static void OnWriteStepWip(object sender, object args)
        {            
            OutCollector.WriteStepWip();
        }

        public static FabWeightPreset GetDebugPreset(string presetID)
        {
            if (GlobalParameters.Instance.ApplyDebug == false)
                return null;

            string debugPreset = InputMart.Instance.GlobalParameters.DebugPreset;
            if (string.IsNullOrEmpty(debugPreset) == false && LcdHelper.IsEmptyID(debugPreset) == false)
            {
                int index = presetID.IndexOf("_");
                if (index >= 0)
                {
                    string testPresetID = LcdHelper.Concat(debugPreset, presetID.Substring(index));
                    var preset = WeightHelper.GetWeightPreset(testPresetID);

                    return preset;
                }
            }

            return null;
        }
    }
}