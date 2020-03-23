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

namespace CSOT.Lcd.Scheduling.Logic.Simulation
{
    [FeatureBind()]
    public partial class CustomEvents_WakeupFrontEqp
    {
        /// <summary>
        /// </summary>
        /// <param name="evt"/>
        /// <param name="cm"/>
        /// <returns/>
        public bool RUN(Mozart.SeePlan.Simulation.ICalendarEvent evt, ICalendarEventManager cm)
        {

            //foreach (var eqp in AoFactory.Current.Equipments.Values)
            //    eqp.WakeUp();

            return true;
        }
    }
}
