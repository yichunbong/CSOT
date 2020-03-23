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
using Mozart.SeePlan.StatModel;
using Mozart.Simulation.Engine;
using Mozart.SeePlan.Simulation;

namespace CSOT.Lcd.Scheduling.Logic.Simulation
{
    [FeatureBind()]
    public partial class Statistics_LoadHistory
    {
        /// <summary>
        /// </summary>
        /// <param name="sheet"/>
        /// <param name="entity"/>
        /// <param name="aeqp"/>
        /// <param name="state"/>
        /// <returns/>
        public bool FILTER(Mozart.SeePlan.StatModel.StatSheet<LoadHistory> sheet, Mozart.Simulation.Engine.ISimEntity entity, ActiveObject aeqp, Mozart.SeePlan.Simulation.LoadingStates state)
        {
            if (ModelContext.Current.EndTime == aeqp.NowDT && state != LoadingStates.IDLE )
                return true;

            return false;
        }
    }
}
