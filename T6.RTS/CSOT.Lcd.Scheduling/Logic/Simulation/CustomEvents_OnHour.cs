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
using Mozart.Simulation.Engine;

namespace CSOT.Lcd.Scheduling.Logic.Simulation
{
    [FeatureBind()]
    public partial class CustomEvents_OnHour
    {
        /// <summary>
        /// </summary>
        /// <param name="evt"/>
        /// <param name="cm"/>
        /// <returns/>
        public bool RUN(Mozart.SeePlan.Simulation.ICalendarEvent evt, ICalendarEventManager cm)
        {
            //double startMin = ModelContext.Current.StartTime.Minute;

            //if (SimHelper.firstFireAtOnHour && startMin != 30)
            //{
            //    int gap = cm.NowDT.Minute - ModelContext.Current.StartTime.Minute;

            //    if (startMin < 30)
            //        evt.Duration =  Time.FromMinutes(30 + startMin);
            //    else if(startMin > 30)
            //        evt.Duration = Time.FromMinutes(Math.Abs(gap));

            //    SimHelper.firstFireAtOnHour = false;

            //    return true;
            //}

            //evt.Duration = Time.FromMinutes(60);
            //SimHelper.firstFireAtOnHour = false;

            OutCollector.WriteStepWip();

            return true;
        }
    }
}
