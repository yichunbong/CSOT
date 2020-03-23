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
    public partial class Misc
    {
        /// <summary>
        /// </summary>
        /// <param name="aeqp"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public int GET_CHAMBER_CAPACITY0(Mozart.SeePlan.Simulation.AoEquipment aeqp, ref bool handled, int prevReturnValue)
        {
            return aeqp.Target.ChildCount;
        }

        /// <summary>
        /// </summary>
        /// <param name="aeqp"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public string[] GET_CHAMBER_IDS0(AoEquipment aeqp, ref bool handled, string[] prevReturnValue)
        {
            FabEqp eqp = aeqp.Target as FabEqp;

            return eqp.GetSubEqpIDs().ToArray();
        }
    }
}
