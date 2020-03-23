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
    public static partial class DashboardMasterFunc
    {
        #region CellCodeMap

        public static void AddCellCodeMaps(this DashboardMaster dashboard, string productID, string cellCode)
        {
            if (string.IsNullOrEmpty(cellCode))
                return;

            string key = productID;
            if (string.IsNullOrEmpty(key))
                return;

            var maps = dashboard.CellCodeMaps;
            if (maps.ContainsKey(key))
                return;

            maps.Add(key, cellCode);
        }

        public static string GetCellCode(this DashboardMaster dashboard, string productID)
        {
            string key = productID;
            if (string.IsNullOrEmpty(key))
                return null;

            var maps = dashboard.CellCodeMaps;

            string cellCode;
            if (maps.TryGetValue(key, out cellCode))
                return cellCode;

            return null;
        }

        #endregion CellCodeMap
    }
}
