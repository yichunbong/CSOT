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
    public partial class BucketControl
    {
        /// <summary>
        /// </summary>
        /// <param name="hb"/>
        /// <param name="bucketer"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public Time GET_BUCKET_TIME0(IHandlingBatch hb, AoBucketer bucketer, ref bool handled, Time prevReturnValue)
        {
            FabStep step = hb.CurrentStep as FabStep;
            FabLot lot = hb.Sample as FabLot;

            string productID = lot.CurrentProductID;
            var tatInfo = step.GetTat(productID, true);

            if (tatInfo == null)
                return Time.Zero;

            Time remainTime = Time.FromMinutes(tatInfo.TAT);

            //Wip 초기화시 TrackIn 반영
            if (lot.IsWipHandle)
            {
                Time now = bucketer.Now;
                bool isRunWip = lot.Wip.CurrentState == EntityState.RUN;
                Time stateTat = isRunWip ? tatInfo.RunTat : tatInfo.WaitTat;
                
                //current state
                DateTime stateInTime = lot.Wip.WipStateTime;

                if (lot.Wip.IsInputLot)
                    stateInTime = lot.ReleaseTime;
                else if (lot.Wip.CurrentState == EntityState.HOLD)
                    stateInTime = lot.HoldStartTime;

                Time stayTime = now - stateInTime;
                remainTime = Time.Max(stateTat - stayTime, Time.Zero);

                //+ runTat (run wip 제외)
                if (isRunWip == false)
                    remainTime += tatInfo.RunTat;

                //Max(defaultMinBucketTime, minBucketTime) (2020.03.11 - by.liujian(유건))
                Time defaultMinBucketTime = SiteConfigHelper.GetDefaultMinBucketTime();
                remainTime = Time.Max(remainTime, defaultMinBucketTime);
            }

            lot.CurrentFabPlan.AoBucketTime = Time.Max(remainTime, Time.Zero);

            return remainTime;
        }
    }
}
