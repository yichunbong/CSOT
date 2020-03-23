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
namespace CSOT.RTS.Inbound
{
    [FeatureBind()]
    public static partial class ConfigHelper
    {
        public const string DEFAULT_PRODUCT_VERSION = "DEFAULT_PRODUCT_VERSION";
        public const string PHOTO_EQP_GROUP_PATTERN = "PHOTO_EQP_GROUP_PATTERN";
        public const string ENG_DEFAULT_VALUE = "ENG_DEFAULT_VALUE";

        public static string GetCodeMap(string codeGroup, string codeName)        
        {
            if (codeGroup == null || codeName == null)
                return codeName;

            string findCode;
            if (ConfigHelper.TryGetValue(codeGroup, codeName, out findCode))
                return findCode;

            return codeName;
        }

        public static bool TryGetValue(string codeGroup, string codeName, out string value)
        {
            value = codeName;

            if (LcdHelper.IsNullOrEmpty_AnyOne(codeGroup, codeName))
                return false;

            var conf = InputMart.Instance.RTS_CONFIG;
            if (conf == null)
                return false;

            var find = conf.Rows.Find(codeGroup, codeName);
            if (find != null)
            {
                value = find.CODE_VALUE;
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Default : 1Hours
        /// </summary>
        /// <returns></returns>
        public static TimeSpan GetDefaultFixedPmDelayTime()
        {
            TimeSpan defaultTime = TimeSpan.FromHours(1);

            try
            {
                string groupCode = ConfigHelper.ENG_DEFAULT_VALUE;
                string codeName = "DEFAULT_FIXED_PM_DELAY_HOURS";

                string codeValue;
                if (ConfigHelper.TryGetValue(groupCode, codeName, out codeValue))
                {
                    double dtime;
                    if (double.TryParse(codeValue, out dtime))
                        return TimeSpan.FromHours(dtime);
                }
            }
            catch { }

            return defaultTime;
        }


        public static string GetCodeMap_LineOperMode(string lineOperMode)
        {
            //RTS_SYS_STD_CODES >> ENG_LINEOPERMODE
            string codeGroup = "ENG_LINEOPERMODE";

            return GetCodeMap(codeGroup, lineOperMode);
        }

        public static string GetCodeMap_InboundCodeMap(string codeName, string defaultValue)
        {
            //RTS_SYS_STD_CODES >> ENG_INBOUND_CODE_MAP
            string codeGroup = "ENG_INBOUND_CODE_MAP";

            string findCode;
            if (ConfigHelper.TryGetValue(codeGroup, codeName, out findCode))
                return findCode;

            return defaultValue;
        }

        public static string GetCodeMap_ChamberID(string chamberID)
        {
            //RTS_SYS_STD_CODES >> ENG_CHAMBER_UNIT_MAP
            string codeGroup = "ENG_CHAMBER_UNIT_MAP";

            return GetCodeMap(codeGroup, chamberID);
        }

        public static string GetCodeMap_HoldCode(string holdCode)
        {
            //RTS_SYS_STD_CODES >> ENG_HOLD_CODE_MAP
            string codeGroup = "ENG_HOLD_CODE_MAP";

            return GetCodeMap(codeGroup, holdCode);
        }

        public static bool CheckHoldStep(string shopID, string stepID, out string holdCode)
        {
            holdCode = null;

            if (shopID == null || stepID == null)
                return false;

            //RTS_SYS_STD_CODES >> ENG_HOLD_STEP
            string codeGroup = "ENG_HOLD_STEP";
            string key = string.Format("{0}_{1}", shopID, stepID);

            string findCode = GetCodeMap(codeGroup, key);
            if(key != findCode)
            {
                holdCode = findCode;
                return true;
            }

            return false;
        }
    }
}
