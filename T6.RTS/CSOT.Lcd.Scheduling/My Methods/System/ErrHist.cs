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
    public static partial class ErrHist
    {
        static Dictionary<string, string> CheckList = new Dictionary<string, string>();

        private static void AddRow(ErrCategory category,
            ErrLevel level,
            string factoryID,
            string shopID,
            string lotID,
            string productID,
            string productVer,
            string processID,
            string eqpID,
            string stepID,
            string reason,
            string detail
          )
        {
            Outputs.ErrorHistory item = new ErrorHistory();

            item.VERSION_NO = ModelContext.Current.VersionNo;

            item.ERR_CATEGORY = category.ToString();
            item.ERR_LEVEL = level.ToString();
            item.FACTORY_ID = factoryID;
            item.SHOP_ID = shopID;
            item.LOT_ID = lotID;
            item.PRODUCT_ID = productID;
            item.PRODUCT_VERSION = productVer;
            item.PROCESS_ID = processID;
            item.EQP_ID = eqpID;
            item.STEP_ID = stepID;
            item.ERR_REASON = reason;
            item.REASON_DETAIL = detail;

            OutputMart.Instance.ErrorHistory.Add(item);
        }

        public static void WriteIf(string key,
            ErrCategory category,
            ErrLevel level,
            string factoryID,
            string shopID,
            string lotID,
            string productID,
            string productVer,
            string processID,
            string eqpID,
            string stepID,
            string reason,
            string detail
            )
        {
            if (CheckList.ContainsKey(key))
                return;

            CheckList.Add(key, key);

            AddRow(category,
                   level,
                   factoryID,
                   shopID,
                   lotID,
                   productID,
                   productVer,
                   processID,
                   eqpID,
                   stepID,
                   reason,
                   detail);
        }

		internal static void WriteLoadWipError(string key, Wip item, ErrLevel errLevel, string reason, string detail)
		{
			key = string.Format("Load Wip:{0}", key);

			WriteIf(key,
					ErrCategory.PERSIST,
					errLevel,
					item.FACTORY_ID,
					item.SHOP_ID,
					item.LOT_ID,
					item.PRODUCT_ID,
					item.PRODUCT_VERSION,
					item.PROCESS_ID,
					item.EQP_ID,
					item.STEP_ID,
					reason,
					detail);
		}
	}
}
