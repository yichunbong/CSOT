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
    public static partial class FabLotExt
    {
        public static FabEqp GetLastLoadedEqp(this FabLot lot, bool started)
        {
            if (started)
            {
                if (lot.CurrentPlan != null && lot.CurrentFabPlan.IsLoaded)
                    return lot.CurrentFabPlan.LoadedResource as FabEqp;

                return lot.PreviousPlan != null ? lot.PreviousPlan.LoadedResource as FabEqp : null;
            }
            else
            {
                return lot.Wip != null ? lot.Wip.InitialEqp as FabEqp : null;
            }
        }
    }
}
