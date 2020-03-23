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
    public partial class CustomEvents_CalcInflowProfile
    {
        /// <summary>
        /// </summary>
        /// <param name="evt"/>
        /// <param name="cm"/>
        /// <returns/>
        public bool INITIALIZE(Mozart.SeePlan.Simulation.ICalendarEvent evt, ICalendarEventManager cm)
        {                        
            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="evt"/>
        /// <param name="cm"/>
        /// <returns/>
        public bool RUN(ICalendarEvent evt, ICalendarEventManager cm)
        {
   
            InFlowAgent.Management();
            return true;
        }
    }
}
