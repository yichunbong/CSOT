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
    public partial class PREPARE_WIP
    {
        /// <summary>
        /// </summary>
        /// <param name="pegPart"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public PegPart PREPARE_WIP0(PegPart pegPart, ref bool handled, PegPart prevReturnValue)
        {
            foreach (FabWipInfo wip in InputMart.Instance.FabWipInfo.Values)
            {
                FabPlanWip planWip = CreateHelper.CreatePlanWip(wip);

                //OwnerE 는 패깅하지 않음.
                if (wip.OwnerType == Constants.OwnerE)
                {
                    PegHelper.WriteUnpegHistory(planWip, wip.OwnerType);
                    continue;
                }

                InputMart.Instance.FabPlanWip.ImportRow(planWip);
            }

            return pegPart;
        }
    }
}
