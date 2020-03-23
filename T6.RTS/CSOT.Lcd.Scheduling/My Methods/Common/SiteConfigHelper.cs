using CSOT.Lcd.Scheduling.Persists;
using CSOT.Lcd.Scheduling.Outputs;
using CSOT.Lcd.Scheduling.Inputs;
using CSOT.Lcd.Scheduling.DataModel;
using Mozart.Task.Execution;
using Mozart.Extensions;
using Mozart.Collections;
using Mozart.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using Mozart.Simulation.Engine;

namespace CSOT.Lcd.Scheduling
{
    [FeatureBind()]
    public static partial class SiteConfigHelper
    {
        public static ConfigGroup GetChamberMixRun()
        {
            ConfigGroup mixRuns = ConfigHelper.GetConfigByGroup(Constants.CONF_ENG_CHAMBER_MIXRUN);

            return mixRuns;
        }

        public static ConfigGroup GetChamberMixRunLoss()
        {
            ConfigGroup lossInfos = ConfigHelper.GetConfigByGroup(Constants.CONF_ENG_CHAMBER_MIXRUN_LOSS);

            return lossInfos;
        }

        public static string GetDefaultDcnStep(string shopID)
        {
            try
            {
                string groupCode = Constants.CONF_ENG_DEFAULT_VALUE;
                string codeName = string.Format("DEFAULT_DCN_STEP_{0}", shopID);

                string codeValue;
                if (ConfigHelper.TryGetValue(groupCode, codeName, out codeValue))
                    return codeValue;
            }
            catch { }

            const string ARRAY_STEP = "A000";
            const string CF_STEP = "C000";
            const string CELL_STEP = "0000";

            if (LcdHelper.Equals(shopID, Constants.ArrayShop))
                return ARRAY_STEP;

            if (LcdHelper.Equals(shopID, Constants.CfShop))
                return CF_STEP;

            if (LcdHelper.Equals(shopID, Constants.CellShop))
                return CELL_STEP;

            return string.Empty;
        }

        public static string GetDefaultOwnerType()
        {
            const string OWNER_P = "OwnerP";

            try
            {
                string groupCode = Constants.CONF_ENG_DEFAULT_VALUE;
                string codeName = "DEFAULT_OWNER_TYPE";

                string codeValue;
                if (ConfigHelper.TryGetValue(groupCode, codeName, out codeValue))
                    return codeValue;
            }
            catch { }            

            return OWNER_P;
        }

        public static string GetDefaultOwnerID()
        {
            const string RESD = "RESD";

            try
            {
                string groupCode = Constants.CONF_ENG_DEFAULT_VALUE;
                string codeName = "DEFAULT_OWNER_ID";

                string codeValue;
                if (ConfigHelper.TryGetValue(groupCode, codeName, out codeValue))
                    return codeValue;
            }
            catch { }

            return RESD;
        }

        public static string GetDefaultProductVersion()
        {
            const string DEFAULT_PRODUCT_VERSION = "A1";

            try
            {
                string groupCode = Constants.CONF_ENG_DEFAULT_VALUE;
                string codeName = "DEFAULT_PRODUCT_VERSION";

                string codeValue;
                if (ConfigHelper.TryGetValue(groupCode, codeName, out codeValue))
                    return codeValue;
            }
            catch { }

            return DEFAULT_PRODUCT_VERSION;
        }

        public static string GetDefaultPhotoStep(string shopID)
        {
            try
            {
                string groupCode = Constants.CONF_ENG_DEFAULT_VALUE;
                string codeName = string.Format("DEFAULT_PHOTO_STEP_{0}", shopID);

                string codeValue;
                if (ConfigHelper.TryGetValue(groupCode, codeName, out codeValue))
                    return codeValue;
            }
            catch { }

            const string ARRAY_PHOTO_STEP = "1300;3300;4300;5300";
            const string CF_PHOTO_STEP = "R300;G300;B300;S300";

            if (LcdHelper.Equals(shopID, Constants.ArrayShop))
                return ARRAY_PHOTO_STEP;

            if (LcdHelper.Equals(shopID, Constants.CfShop))
                return CF_PHOTO_STEP;

            return string.Empty;

        }

        /// <summary>
        /// 30 Minutes
        /// </summary>
        /// <returns></returns>
        public static Time GetDefaultDownTime()
        {            
            Time defaultDownTime = Time.FromMinutes(30);

            try
            {
                string groupCode = Constants.CONF_ENG_DEFAULT_VALUE;
                string codeName = "DEFAULT_DOWN_TIME";

                string codeValue;
                if (ConfigHelper.TryGetValue(groupCode, codeName, out codeValue))
                {
                    double dtime;
                    if(double.TryParse(codeValue, out dtime))
                        return Time.FromMinutes(dtime);
                }
            }
            catch { }

            return defaultDownTime;
        }

        /// <summary>
        /// 180 Minutes
        /// </summary>
        /// <returns></returns>
        public static Time GetDefaultHoldTime()
        {
            Time defaultDownTime = Time.FromMinutes(180);

            try
            {
                string groupCode = Constants.CONF_ENG_DEFAULT_VALUE;
                string codeName = "DEFAULT_HOLD_TIME";

                string codeValue;
                if (ConfigHelper.TryGetValue(groupCode, codeName, out codeValue))
                {
                    double dtime;
                    if (double.TryParse(codeValue, out dtime))
                        return Time.FromMinutes(dtime);
                }
            }
            catch { }

            return defaultDownTime;
        }

        /// <summary>
        /// 15 Minutes
        /// </summary>
        /// <returns></returns>
        public static Time GetDefaultEqpStateCheckTime()
        {
            Time defaultCheckTime = Time.FromMinutes(15);

            try
            {
                string groupCode = Constants.CONF_ENG_DEFAULT_VALUE;
                string codeName = "DEFAULT_EQP_STATE_CHECK_TIME";

                string codeValue;
                if (ConfigHelper.TryGetValue(groupCode, codeName, out codeValue))
                {
                    double dtime;
                    if (double.TryParse(codeValue, out dtime))
                        return Time.FromMinutes(dtime);
                }
            }
            catch { }

            return defaultCheckTime;
        }

		/// <summary>
		/// 15 Minutes
		/// </summary>
		/// <returns></returns>
		public static int GetDefaultLotPriority()
		{
			int defaultLotPriority = 5;

			try
			{
				string groupCode = Constants.CONF_ENG_DEFAULT_VALUE;
				string codeName = "DEFAULT_LOT_PRIORITY";

				string codeValue;
				if (ConfigHelper.TryGetValue(groupCode, codeName, out codeValue))
				{
					int priority;
					if (int.TryParse(codeValue, out priority))
						return priority;
				}
			}
			catch { }

			return defaultLotPriority;
		}

        /// <summary>
        /// 120 Minutes
        /// </summary>
        /// <returns></returns>
        public static Time GetDefaultWaitTAT()
        {
            Time defaultWaitTime = Time.FromMinutes(120);

            try
            {
                string groupCode = Constants.CONF_ENG_DEFAULT_VALUE;
                string codeName = "DEFAULT_WAIT_TIME";

                string codeValue;
                if (ConfigHelper.TryGetValue(groupCode, codeName, out codeValue))
                {
                    double dtime;
                    if (double.TryParse(codeValue, out dtime))
                        return Time.FromMinutes(dtime);
                }
            }
            catch { }

            return defaultWaitTime;
        }

        /// <summary>
        /// 1080 Minutes
        /// </summary>
        /// <returns></returns>
        public static Time GetDefaultRunTAT()
        {
            Time defaultRunTime = Time.FromMinutes(1080);

            try
            {
                string groupCode = Constants.CONF_ENG_DEFAULT_VALUE;
                string codeName = "DEFAULT_RUN_TIME";

                string codeValue;
                if (ConfigHelper.TryGetValue(groupCode, codeName, out codeValue))
                {
                    double dtime;
                    if (double.TryParse(codeValue, out dtime))
                        return Time.FromMinutes(dtime);
                }
            }
            catch { }

            return defaultRunTime;
        }

        /// <summary>
        /// 40 Seconds
        /// </summary>
        /// <returns></returns>
        public static Time GetDefaultTactTime()
        {
            Time defaultRunTime = Time.FromSeconds(40);

            try
            {
                string groupCode = Constants.CONF_ENG_DEFAULT_VALUE;
                string codeName = "DEFAULT_TACT_TIME";

                string codeValue;
                if (ConfigHelper.TryGetValue(groupCode, codeName, out codeValue))
                {
                    double dtime;
                    if (double.TryParse(codeValue, out dtime))
                        return Time.FromMinutes(dtime);
                }
            }
            catch { }

            return defaultRunTime;
        }

        /// <summary>
        /// 60 Seconds
        /// </summary>
        /// <returns></returns>
        public static Time GetDefaultFlowTime()
        {
            Time defaultRunTime = Time.FromSeconds(60);

            try
            {
                string groupCode = Constants.CONF_ENG_DEFAULT_VALUE;
                string codeName = "DEFAULT_FLOW_TIME";

                string codeValue;
                if (ConfigHelper.TryGetValue(groupCode, codeName, out codeValue))
                {
                    double dtime;
                    if (double.TryParse(codeValue, out dtime))
                        return Time.FromMinutes(dtime);
                }
            }
            catch { }

            return defaultRunTime;
        }

        /// <summary>
        /// 0 Minutes
        /// </summary>
        /// <returns></returns>
        public static Time GetDefaultTransferTime()
        {
            Time defaultTransferTime = Time.FromMinutes(0);

            try
            {
                string groupCode = Constants.CONF_ENG_DEFAULT_VALUE;
                string codeName = "DEFAULT_TRANSFER_TIME";

                string codeValue;
                if (ConfigHelper.TryGetValue(groupCode, codeName, out codeValue))
                {
                    double dtime;
                    if (double.TryParse(codeValue, out dtime))
                        return Time.FromMinutes(dtime);
                }
            }
            catch { }

            return defaultTransferTime;
        }

        public static Time GetAllowAdjustPMIdleTime()
        {
            Time defaultTransferTime = Time.FromMinutes(30);

            try
            {
                string groupCode = Constants.CONF_ENG_DEFAULT_VALUE;
                string codeName = "CONF_ALLOW_ADJ_PM_IDLE_TIME";

                string codeValue;
                if (ConfigHelper.TryGetValue(groupCode, codeName, out codeValue))
                {
                    double dtime;
                    if (double.TryParse(codeValue, out dtime))
                        return Time.FromMinutes(dtime);
                }
            }
            catch { }

            return defaultTransferTime;
            
        }

        /// <summary>
        /// 15 Minutes
        /// </summary>
        /// <returns></returns>
        public static Time GetDefaultSubStepHoldTime()
        {
            Time defaultCheckTime = Time.FromMinutes(15);

            try
            {
                string groupCode = Constants.CONF_ENG_DEFAULT_VALUE;
                string codeName = "DEFAULT_SUBSTEP_HOLD_TIME";

                string codeValue;
                if (ConfigHelper.TryGetValue(groupCode, codeName, out codeValue))
                {
                    double dtime;
                    if (double.TryParse(codeValue, out dtime))
                        return Time.FromMinutes(dtime);
                }
            }
            catch { }

            return defaultCheckTime;
        }

        /// <summary>
        /// 15 Minutes
        /// </summary>
        /// <returns></returns>
        public static Time GetDefaultMinBucketTime()
        {
            Time defaultCheckTime = Time.FromMinutes(15);

            try
            {
                string groupCode = Constants.CONF_ENG_DEFAULT_VALUE;
                string codeName = "DEFAULT_MIN_BUCKET_TIME";

                string codeValue;
                if (ConfigHelper.TryGetValue(groupCode, codeName, out codeValue))
                {
                    double dtime;
                    if (double.TryParse(codeValue, out dtime))
                        return Time.FromMinutes(dtime);
                }
            }
            catch { }

            return defaultCheckTime;
        }
    }
}