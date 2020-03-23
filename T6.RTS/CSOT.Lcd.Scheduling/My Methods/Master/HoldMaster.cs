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

namespace CSOT.Lcd.Scheduling
{
    [FeatureBind()]
    public static partial class HoldMaster
    {        
        internal static float DefaultHoldTime { get; private set; }
        
        internal static void AddHoldInfo(HoldTime item)
        {
            string key = GetKey(item.SHOP_ID, item.HOLD_CODE);

            HoldInfo info = GetHoldInfo(item.SHOP_ID, item.HOLD_CODE);

            if (info == null)
            {
                info = CreateHelper.CreateHoldInfo(item);
                InputMart.Instance.HoldInfo.Add(key, info);

                if (info.HoldCode == "DEFAULT")
                    SetDefaultHoldTime(info.HoldTime);
            }

           
            return;
        }

        internal static HoldInfo GetHoldInfo(string shopID, string holdCode)
        {
            string key = GetKey(shopID, holdCode);

            HoldInfo info;
            InputMart.Instance.HoldInfo.TryGetValue(key, out info);

            return info;
        }

        /// <summary>
        /// HoldTime 단위(분)
        /// </summary>
        internal static float GetHoldTime(string shopID, string holdCode)
        {
            HoldInfo info = GetHoldInfo(shopID, holdCode);

            if (info == null)
                return DefaultHoldTime;

            return info.HoldTime;
        }


        private static string GetKey(string shopID, string holdCode)
        {
            string key = LcdHelper.CreateKey(shopID, holdCode);

            return key;

        }

        internal static void SetDefaultHoldTime(Time defaultHoldTime)
        {
            HoldMaster.DefaultHoldTime = (float)defaultHoldTime.TotalMinutes;
        }
    }
}
