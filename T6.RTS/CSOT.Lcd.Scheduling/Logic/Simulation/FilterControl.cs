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
    public partial class FilterControl
    {
        /// <summary>
        /// </summary>
        /// <param name="eqp"/>
        /// <param name="wips"/>
        /// <param name="ctx"/>
        /// <param name="handled"/>
        /// <param name="prevReturnValue"/>
        /// <returns/>
        public IList<IHandlingBatch> DO_FILTER1(Mozart.SeePlan.Simulation.AoEquipment eqp, IList<Mozart.SeePlan.Simulation.IHandlingBatch> wips, Mozart.SeePlan.Simulation.IDispatchContext ctx, ref bool handled, IList<Mozart.SeePlan.Simulation.IHandlingBatch> prevReturnValue)
        {
            //if (eqp.EqpID == "THCVDC00" && eqp.NowDT >= LcdHelper.StringToDateTime("20200108 090229"))
            //    Console.WriteLine("B");

            if (wips == null)
                return wips;

            eqp.StartDispatch_ParallelChamber();

            List<IHandlingBatch> revaluation = new List<IHandlingBatch>();

            //▶▶ 1차 필터 
            FilterMaster.DoFilter(eqp, wips, ctx);

            //▶▶ 2차 Group별 필터(다시 살릴 수 있음)
            FilterMaster.DoGroupFilter(eqp, wips, ctx, revaluation);

            //▶▶ Group에서 제외된 Lot 재평가
            if (wips.Count == 0 && revaluation.Count > 0)
            {
                FilterMaster.Revaluate(eqp, revaluation);

                if (revaluation.Count > 0)
                    return revaluation;
            }
            else
            {
                foreach (var item in revaluation)
                    FilterMaster.WriteFilterInfo(item, eqp);
            }

            return wips;
        }
    }
}
