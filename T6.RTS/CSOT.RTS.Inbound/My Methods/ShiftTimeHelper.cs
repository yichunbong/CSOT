using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Mozart.Common;
using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using CSOT.RTS.Inbound.DataModel;
using CSOT.RTS.Inbound.Inputs;
using CSOT.RTS.Inbound.Outputs;
using CSOT.RTS.Inbound.Persists;
using Mozart.SeePlan;
namespace CSOT.RTS.Inbound
{
    [FeatureBind()]
    public static partial class ShiftTimeHelper
    {
        public static void Initialize(this FactoryConfiguration curr)
        {
            try
            {
                //var table = InputMart.Instance.ShiftTime;

                //List<string> shiftNames = new List<string>();

                //foreach (var entity in table.DefaultView)
                //{
                //    string shiftName = entity.SHIFT_NAME;
                //    string startTimeStr = entity.START_TIME;

                //    if (shiftNames.Contains(shiftName) == false)
                //        shiftNames.Add(shiftName);
                //}

                FactoryTimeInfo info = new FactoryTimeInfo();

                info.Default = false;
                info.Name = null;
                info.ShiftNames = new string[]{"A", "B"};
                info.StartOffset = TimeSpan.FromHours(7.5);
                info.ShiftHours = 12;
                info.StartOfWeek = DayOfWeek.Monday;

                curr.TimeInfo = info;
            }
            catch { }
        }       
    }
}
