using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mozart.Studio.TaskModel.Projects;
using Mozart.Studio.TaskModel.UserLibrary;
using CSOT.UserInterface.Utils;
using System.Data;
using CSOT.Lcd.UserInterface.DataMappings;
using CSOT.Lcd.Scheduling;
using Mozart.Studio.Application;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;

namespace CSOT.Lcd.UserInterface.Common
{
    public class Globals
    {
        public enum AREA
        {
            TFT = 0,
            CF = 1,
            CELL = 2,
            NONE = 3
        }
                
        public const string ARRAY = "ARRAY";
        public const string CF = "CF";
        public const string CELL = "CELL";

        //public static string FormattedLayerText(string tool)
        //{
        //    if (!tool.Contains('.'))
        //        return tool;

        //    return tool.Split('.')[1];
        //}       

        public static void InitFactoryTime(IExperimentResultItem result)
        {
            FactoryConfiguration.Check(result.Model, CreateFactoryTime);
        }
        public static void InitFactoryTime(IModelProject project)
        {
            FactoryConfiguration.Check(project, CreateFactoryTime);
        }

        private static FactoryTimeInfo CreateFactoryTime(IModelProject project)
        {
            var info = new FactoryTimeInfo();
            info.Name = project.Name;
            info.StartOffset = TimeSpan.FromHours(6);
            info.ShiftHours = 12;
            info.ShiftNames = new string[] { "A", "B" };

            return info;
        }

        public static DateTime GetPlanStartTime_OLD(IExperimentResultItem result)
        {
            return result.Experiment.GetArgument("plan-start", DateTime.Now);
        }

        public static DateTime GetPlanStartTime(IExperimentResultItem result)
        {
            if (result.Experiment.GetArgument("nextShiftVer", false))
            {
                int shift = ShopCalendar.ClassifyShift(GetPlanStartTime_OLD(result).AddHours(8));
                return ShopCalendar.GetShiftStartTime(GetPlanStartTime_OLD(result).AddHours(8), shift);
            }
            else
            {
                var st = result.Experiment.GetArgument("start-time");

                if (st == null)
                    return DateTime.Now;

                var planStartTime = DateTime.MinValue;
                try
                {
                    planStartTime = st.ToString().DbToDateTime();
                }
                catch
                {
                    return DateTime.Now;
                }

                if (planStartTime == DateTime.MinValue)
                    return DateTime.Now;

                return planStartTime;
            }
        }

        public static int GetPlanPeriod(IExperimentResultItem result)
        {
            var value = result.Experiment.GetArgument("period");

            if (value != null)
            {
                int period;
                if (int.TryParse(value.ToString(), out period))
                    return period;
            }

            return 7;
        }

        public static int GetResultPlanPeriod(IExperimentResultItem _result)
        {
            return _result.GetPlanPeriod(1);
        }

        public static DateTime GetResultStartTime(IExperimentResultItem _result)
        {
            return _result.StartTime;
        }

        public static string CreateFilter(string filter, string colName, string sOperator, string item, string preConjunctivAdverb = "")
        {
            if (item == null)
                return filter;

            string modConAdverb = string.IsNullOrEmpty(preConjunctivAdverb) ? string.Empty : " " + preConjunctivAdverb + " ";

            if (item != Consts.ALL)
                filter += string.Format("{0}{1} {2} '{3}'", modConAdverb, colName, sOperator, item);

            return filter;
        }


        public static string CreateFilter(string filter, string colName, string sOperator, List<string> list,
            string conjunctivAdverb, string preConjunctivAdverb = "", bool useBracket = false)
        {
            if (list == null)
                return filter;

            if (string.IsNullOrEmpty(filter) == false && string.IsNullOrEmpty(preConjunctivAdverb) == false)
                filter = filter + " " + preConjunctivAdverb + " ";

            string addFilter = string.Empty;
            foreach (string item in list)
            {
                if (string.IsNullOrEmpty(addFilter))
                    addFilter = string.Format("{0} {1} '{2}'", colName, sOperator, item);
                else
                    addFilter += " " + conjunctivAdverb + " " + string.Format("{0} {1} '{2}'", colName, sOperator, item);
            }

            if (useBracket)
                addFilter = "(" + addFilter + ")";

            filter = string.IsNullOrEmpty(filter) ? addFilter : filter + " " + addFilter;

            return filter;
        }

        public static DataTable GetConsInfo(ModelDataContext modelDataContext)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(EqpGanttChartData.Const.Schema.CATEGORY, typeof(string));
            dt.Columns.Add(EqpGanttChartData.Const.Schema.CODE, typeof(string));
            dt.Columns.Add(EqpGanttChartData.Const.Schema.DESCRIPTION, typeof(string));

            var x = (from a in modelDataContext.Eqp
                     select new { SHOP_ID = a.SHOP_ID, DSP_EQP_GROUP_ID = a.DSP_EQP_GROUP_ID }).Distinct();

            Dictionary<string, string> AreaDic = new Dictionary<string, string>();
            foreach (var info in modelDataContext.StdStep)
            {
                if (string.IsNullOrEmpty(info.DSP_EQP_GROUP_ID))
                    continue;

                string key = info.DSP_EQP_GROUP_ID;
                if (AreaDic.ContainsKey(key) == false)
                    AreaDic.Add(key, info.AREA_ID);
            }

            foreach (var eqpinfo in x)
            {
                DataRow dRow = dt.NewRow();
                dRow[EqpGanttChartData.Const.Schema.CATEGORY] = "AREA_INFO";

                string eqpGroupID = eqpinfo.DSP_EQP_GROUP_ID;
                if (AreaDic.ContainsKey(eqpGroupID) == false)
                    continue;

                var areaID = AreaDic[eqpinfo.DSP_EQP_GROUP_ID];
                dRow[EqpGanttChartData.Const.Schema.CODE] = areaID;
                dRow[EqpGanttChartData.Const.Schema.DESCRIPTION] = eqpinfo.DSP_EQP_GROUP_ID;
                dt.Rows.Add(dRow);
            }

            return dt;
        }

        public static void SetLocalSetting(IServiceProvider serviceProvider, string settingName, string settingValue)
        {
            IVsApplication application = (IVsApplication)serviceProvider.GetService(typeof(IVsApplication));
            application.SetSetting(settingName, settingValue);
        }

        public static string GetLocalSetting(IServiceProvider serviceProvider, string loadingName)
        {
            IVsApplication application = (IVsApplication)serviceProvider.GetService(typeof(IVsApplication));
            string loadingValue = application.GetSetting(loadingName);
            return loadingValue;
        }

        public static void SetGridViewColumnWidth(GridView grid)
        {
            grid.OptionsView.ColumnAutoWidth = false;

            foreach (GridColumn col in grid.Columns)
            {                
                string key = col.Name;
                int width = col.Width;

                switch(key)
                {
                    case "SHOP_ID": width = 70; break;
                    case "EQP_GROUP": width = 80; break;
                    case "EQP_ID": width = 80; break;
                    case "SUB_EQP_ID": width = 80; break;
                    case "STATE": width = 50; break;
                    case "LOT_ID": width = 100; break;
                    case "LAYER": width = 80; break;
                    case "OWNER_TYPE": width = 100; break;
                    case "OPERATION_NAME": width = 60; break;
                    case "TOOL_ID": width = 100; break;
                    case "PRODUCT_ID": width = 100; break;
                    case "PRODUCT_VERSION": width = 140; break;
                    case "START_TIME": width = 140; break;
                    case "END_TIME": width = 140; break;
                    case "DISPATCHING_TIME": width = 140; break;
                    case "IN_QTY": width = 60; break;
                    case "OUT_QTY": width = 60; break;
                    case "GAP_TIME": width = 120; break;
                    case "LOT_PRIORITY": width = 100; break;
                    case "EQP_RECIPE": width = 80; break;
                    case "WIP_INIT_RUN": width = 100; break;
                    case "SELECTED_LOT": width = 100; break;
                    case "SELECTED_PRODUCT": width = 150; break;
                    case "SELECTED_STEP": width = 150; break;
                    case "INIT_WIP_CNT": width = 100; break;
                    case "FILTERED_WIP_CNT": width = 150; break;
                    case "SELECTED_WIP_CNT": width = 150; break;
                    case "FILTERED_REASON": width = 300; break;
                    case "FILTERED_PRODUCT": width = 200; break;
                    case "DISPATCH_PRODUCT": width = 200; break;
                    case "PRESET_ID": width = 100; break;
                }

                col.Width = width;
            }
        }
        
        public static string GetAreaIDByProductID(string productID, string shopID)
        {
            if (shopID == CELL)
                return AREA.CELL.ToString();

            if (string.IsNullOrEmpty(productID))
                return AREA.NONE.ToString();

            if (productID.StartsWith("F"))
                return AREA.CF.ToString();

            return AREA.TFT.ToString();
        }

        public static int Comparer_AreaID(string x, string y)
        {
            var a = CommonHelper.ToEnum(x, AREA.NONE);
            var b = CommonHelper.ToEnum(y, AREA.NONE);

            return a.CompareTo(b);
        }
    }
}
