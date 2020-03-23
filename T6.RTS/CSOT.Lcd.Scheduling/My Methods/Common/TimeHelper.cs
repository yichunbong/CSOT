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
using Mozart.SeePlan.DataModel;
using Mozart.SeePlan;

namespace CSOT.Lcd.Scheduling
{
    [FeatureBind()]
    public static partial class TimeHelper
    {
        public static StepTime GetStepTime(this FabStep step, string eqpID, string productID)
        {
            List<StepTime> list;
            if (step.StepTimes.TryGetValue(productID, out list))
            {
                foreach (StepTime st in list)
                {
                    if (st.EqpID == eqpID)
                        return st;
                }
            }

            return null;
        }

        public static float GetTactTime(this FabStep step, string eqpID, string productID)
        {
            List<StepTime> list;
            if (step.StepTimes.TryGetValue(productID, out list))
            {
                foreach (StepTime st in list)
                {
                    if (st.EqpID == eqpID)
                        return st.TactTime;
                }
            }

            return 0f;
        }

        public static float GetProcTime(this FabStep step, string eqpID, string productID)
        {
            List<StepTime> list;
            if (step.StepTimes.TryGetValue(productID, out list))
            {
                foreach (StepTime st in list)
                {
                    if (st.EqpID == eqpID)
                        return st.ProcTime;
                }
            }

            return 0f;
        }

        public static decimal GetAvgTactTime(FabStep step, FabProduct prod, string productVersion)
        {
            if (step.AvgTactTime < 0)
            {
                List<string> eqps = EqpArrangeMaster.GetLoadableEqpList(step.StdStep, prod.ProductID, productVersion);

                if (eqps == null)
                {
                    step.AvgTactTime = 0;
                    return step.AvgTactTime;
                }

                int n = eqps == null ? 0 : eqps.Count;

                decimal s = 0;
                foreach (string eqpID in eqps)
                {
                    StepTime tactTime = step.GetStepTime(eqpID, prod.ProductID);

                    if (tactTime == null || tactTime.TactTime <= 0)
                        continue;

                    s += Convert.ToDecimal(1d / tactTime.TactTime);
                }

                if (s > 0m)
                {
                    step.AvgTactTime = n / s;
                }
                else
                {
                    step.AvgTactTime = (decimal)SiteConfigHelper.GetDefaultTactTime().TotalSeconds;
                }
            }

            return step.AvgTactTime;
        }

        public static decimal GetAvgProcTime(FabStep step, FabProduct prod, string productVersion)
        {
            if (step.AvgFlowTime < 0)
            {

                List<string> list = EqpArrangeMaster.GetLoadableEqpList(step.StdStep, prod.ProductID, productVersion);
                if (list == null)
                {
                    StepTat tat = step.GetTat(prod.ProductID, true);
                    if (tat != null)
                        step.AvgFlowTime = Convert.ToDecimal(tat.TAT * 60);

                    return step.AvgFlowTime;
                }

                decimal n = list.Count;
                decimal s = 0;

                foreach (string eqpID in list)
                {
                    StepTime st = step.GetStepTime(eqpID, prod.ProductID);

                    if (st == null || st.ProcTime <= 0)
                        continue;

                    s = s + Convert.ToDecimal(1d / st.ProcTime);
                }

                if (s > 0m)
                {
                    step.AvgFlowTime = Math.Round(n / (decimal)s, 2);
                }
                else
                {
                    step.AvgFlowTime = (decimal)SiteConfigHelper.GetDefaultFlowTime().TotalSeconds;
                }
            }
            return step.AvgFlowTime;
        }

        public static decimal GetHarmonicTactTime(FabStep step, List<FabAoEquipment> assignedEqps, string productID)
        {
            if (assignedEqps == null)
                return 0;


            float tactSum = 0;
            foreach (var eqp in assignedEqps)
            {
                StepTime st = step.GetStepTime(eqp.EqpID, productID);

                if (st == null)
                    continue;

                float tact = (1f / st.TactTime);
                tactSum += tact;
            }

            if (tactSum == 0)
                return 0;

            return Convert.ToDecimal(1f / tactSum);
        }

        public static void AdjustStepTime(string productID, FabStep step, ref float tact, ref float proc)
        {
            return;

#if false //StepTime값 보정할 때 사용, Input값이 정확하면 사용할 필요없음
            if (step.IsMandatoryStep)
            {
                proc = proc / (float)prod.CstSize;
            }
            else
            {
                if (step.StepType == "PRODUCTION")
                    proc = proc / (float)prod.CstSize;
                else
                    proc = proc / 3f; 
            }

            if (tact > proc)
                proc = tact * 1.2f;             
#endif
        }
        public static StepTime GetAverageStepTime(IEnumerable<Inputs.EqpStepTime> list, FabStep step, FabEqp eqp, string productID, string reason)
        {
            float tact = list.Average(x => x.TACT_TIME);
            float proc = list.Average(x => x.PROC_TIME);

            AdjustStepTime(productID, step, ref tact, ref proc);

            return CreateAverageStepTime(step, eqp, productID, tact, proc, reason);
        }

        public static StepTime CreateAverageStepTime(FabStep step, FabEqp eqp, string productID, float tact, float proc, string reason)
        {
            StepTime st = CreateHelper.CreateStepTime(eqp, step, productID, tact, proc);

            List<StepTime> list;
            if (step.StepTimes.TryGetValue(productID, out list) == false)
                list = new List<StepTime>();

            list.Add(st);

            //TODO : Output:
            //OutputMart.Instance.AppxStepTime.Add(item);

            return st;
        }

        public static DateTime GetRptDate_1Hour(DateTime t)
        {
            //1시간 단위
            int baseHours = 1;

            //DayStartTime 기준
            int baseMinute = ShopCalendar.StartTimeOfDayT(ModelContext.Current.StartTime).Minute;

            //ex) HH:30:00
            DateTime rptDate = LcdHelper.Trim(t, "HH").AddMinutes(baseMinute);

            //baseMinute(ex.30분) 이상인 경우 이후 시간대 baseMinute의 실적
            //07:30 = 06:30(초과) ~ 07:30(이하)인경우, 06:40 --> 07:30, 07:30 --> 07:30, 07:40 --> 08:30
            if (t.Minute > baseMinute)
            {
                rptDate = rptDate.AddHours(baseHours);
            }

            return rptDate;
        }

        public static decimal GetAvgTactTimeForEqps(FabStep step, FabProduct product, List<FabAoEquipment> workingEqps)
        {
            Decimal defaultTactTime = (decimal)SiteConfigHelper.GetDefaultTactTime().TotalSeconds;

            if (workingEqps == null)
                return defaultTactTime;

            int n = workingEqps.Count;

            decimal s = 0;
            foreach (var eqp in workingEqps)
            {
                string eqpID = eqp.EqpID;
                StepTime tactTime = step.GetStepTime(eqpID, product.ProductID);

                if (tactTime == null || tactTime.TactTime <= 0)
                    continue;

                s += Convert.ToDecimal(1d / tactTime.TactTime);
            }

            if (s > 0m)
            {
                return n / s;
            }
            else
            {
                return defaultTactTime;
            }
        }
    }
}
