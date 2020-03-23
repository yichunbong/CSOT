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
using Mozart.Simulation.Engine;
using Mozart.SeePlan.Simulation;

namespace CSOT.Lcd.Scheduling.Logic.Simulation
{
    [FeatureBind()]
    public partial class TransferControl
    {
        /// <summary>
        /// </summary>
        /// <param name="hb"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public Time GET_TRANSFER_TIME0(Mozart.SeePlan.Simulation.IHandlingBatch hb, ref bool handled, Mozart.Simulation.Engine.Time prevReturnValue)
        {
            //FabLot lot = hb.Sample as FabLot;

            //if (lot.PreviousStep != null)
            //    return Time.FromMinutes(lot.PreviousFabStep.TransferTime);

            return Time.Zero;
        }

        /// <summary>
        /// </summary>
        /// <param name="hb"/>
        /// <param name="handled"/>
        public void ON_TRANSFER0(IHandlingBatch hb, ref bool handled)
        {
            FabLot lot = hb.ToFabLot();

            FabPlanInfo plan = lot.CurrentFabPlan;

            plan.TransferStartTime = AoFactory.Current.NowDT;

        }

        /// <summary>
        /// </summary>
        /// <param name="hb"/>
        /// <param name="handled"/>
        public void ON_TRANSFERED0(Mozart.SeePlan.Simulation.IHandlingBatch hb, ref bool handled)
        {
            FabLot lot = hb.ToFabLot();
            FabPlanInfo plan = lot.CurrentFabPlan;

            plan.TransferEndTime = AoFactory.Current.NowDT;
        }
    }
}
