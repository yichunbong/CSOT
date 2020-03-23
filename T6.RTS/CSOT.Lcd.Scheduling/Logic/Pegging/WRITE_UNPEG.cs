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
    public partial class WRITE_UNPEG
    {
        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <param name="handled"/>
        public void WRITE_UNPEG0(Mozart.SeePlan.Pegging.PegPart pegPart, ref bool handled)
        {

            //Wip
            foreach (var wip in InputMart.Instance.FabPlanWip.Rows)
                WriteUnpegHistory(wip);


            //Cell BankWip
            foreach (var item in PegMaster.CellBankPlanWips.Values)
            {
                foreach (var wip in item)
                {
                    WriteUnpegHistory(wip);
                }
            }
        }

        private void WriteUnpegHistory(FabPlanWip planWip)
        {
            if (planWip.Qty == 0)
                return;

            if (planWip.Qty == 0)
            {
                PegHelper.WriteUnpegHistory(planWip, "LOT_KIT_REMAIN");
            }
            else if (planWip.MapCount == 0)
            {
                PegHelper.WriteUnpegHistory(planWip, "NO TARGET");
            }
            else if (planWip.Qty > 0)
            {
                PegHelper.WriteUnpegHistory(planWip, "EXCESS");
            }
        }
    }
}
