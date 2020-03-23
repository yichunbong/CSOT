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

namespace CSOT.Lcd.Scheduling
{
    [FeatureBind()]
    public static partial class TransferMaster
    {
        #region 설비간 이동시간 관련
        /// <summary>
        /// 현재설비의 이동시간을 등록함
        /// </summary>
        /// <param name="eqp"><현재설비/param>
        /// <param name="targetEqp">대상설비</param>
        /// <param name="transferTime">이동시간(분)</param>
        /// <param name="isFrom">true:이전설비 → 현재설비, false: 현재설비 → 다음설비</param>
        public static void AddTransferTime(this FabEqp eqp, string targetEqp, int transferTime, bool isFrom)
        {
            Dictionary<string, int> dic = isFrom ? eqp.TransferTimeFrom : eqp.TransferTimeTo;

            if (dic == null)
                dic = new Dictionary<string, int>();

            if (dic.ContainsKey(targetEqp) == false)
                dic.Add(targetEqp, transferTime);
        }

        public static Time GetTransferTime(AoEquipment fromEqp, AoEquipment toEqp)
        {
            FabEqp from = fromEqp.Target as FabEqp;
            FabEqp to = toEqp.Target as FabEqp;

            return GetTransferTime(from.OutStocker, to.InStocker);
        }

        public static Time GetTransferTime(FabLot lot, AoEquipment eqp)
        {
            string from = null;
            string to = null;

            if(lot != null && lot.PreviousFabPlan != null)
            {
                if (lot.PreviousFabPlan.IsLoaded)
                {
                    FabEqp prevEqp = lot.PreviousPlan.LoadedResource as FabEqp;
                    from = prevEqp.OutStocker;
                }
                else
                {
                    //init wip step --> next step
                    if(lot.Plans.Count == 2)
                    {
                        string wipEqpID = lot.Wip.WipEqpID;                        
                        var prevEqp = ResHelper.FindEqp(wipEqpID);
                        from = prevEqp != null ? prevEqp.OutStocker : wipEqpID;
                    }
                }
            }

            if(eqp != null)
            {
                to = (eqp.Target as FabEqp).InStocker;
            }                
            
            return GetTransferTime(from, to);
        }

        private static Time GetTransferTime(string from, string to)
        {
            //TimeSpan defaultTime = TimeSpan.FromMinutes(InputMart.Instance.GetConfigParameters(string.Empty).DefaultTransferTime);
            Time defaultTime = SiteConfigHelper.GetDefaultTransferTime();

            if (LcdHelper.IsEmptyID(from) || LcdHelper.IsEmptyID(to))
                return defaultTime;

            EqpMoveTime time = InputMart.Instance.EqpMoveTimeView.FindRows(from, to).FirstOrDefault();

            if(time != null)
                return Time.FromMinutes(time.TRANSFER_TIME);

            return defaultTime;
        }

        #endregion
    }
}
