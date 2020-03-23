using System;
using DevExpress.XtraPivotGrid;
using System.IO;
using System.Data;
using System.ComponentModel;
using DevExpress.Utils;
using System.Xml;
using DevExpress.Data.PivotGrid;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using Mozart.Studio.TaskModel.UserLibrary;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid;

namespace CSOT.UserInterface.Utils
{
    public class PivotGridHelper
    {
        public static void AddCustomTotal(PivotGridField field, PivotSummaryType sType, string tag, FormatType hFormatType,
               string hFormatString, FormatType cFormatType, string FormatStr)
        {
            if (field.TotalsVisibility != PivotTotalsVisibility.CustomTotals)
                field.TotalsVisibility = PivotTotalsVisibility.CustomTotals;

            PivotGridCustomTotal customTotal = new PivotGridCustomTotal(sType);
            field.CustomTotals.Add(customTotal);

            customTotal.Tag = tag;
            customTotal.Format.FormatType = hFormatType;
            customTotal.Format.FormatString = hFormatString;
            customTotal.CellFormat.FormatType = cFormatType;
            customTotal.CellFormat.FormatString = FormatStr;
        }

        public static bool AddCustomTotal(PivotGridControl gridControl, string fieldName,
            PivotSummaryType sType, string tag, FormatType hFormatType,
            string hFormatString, FormatType cFormatType, string FormatStr)
        {
            PivotGridField targetField;
            try
            {
                targetField = gridControl.Fields[fieldName];
            }
            catch
            {
                return false;
            }

            if (targetField != null)
            {
                PivotGridHelper.AddCustomTotal(targetField, sType, tag,
                    hFormatType, hFormatString, cFormatType, FormatStr);
                return true;
            }

            return false;

        }

        public static void ClearCustomTotals(PivotGridField field)
        {
            field.CustomTotals.Clear();
        }

        public static string GetFieldName(string fieldName)
        {
            return XtraPivotGridHelper.GetFieldName(fieldName);
        }

        public static object[] StringToObjects(string str)
        {
            return XtraPivotGridHelper.StringToObjects(str);
        }

        //Get the array of visible Fields in a particular area
        public static PivotGridField GetPivotGridField(PivotGridControl pivotGridControl, PivotArea area, string fieldName)
        {
            foreach (PivotGridField f in pivotGridControl.Fields)
            {
                if (f.Visible && f.Area == area && f.FieldName == fieldName)
                    return f;
            }
            return null;
        }

        //Get info for data fields
        public static string GetFieldCellValues(PivotGridControl pivotGridControl, PivotArea area, string fieldName, PivotCellEventArgs cellInfo)
        {
            PivotGridField pgf = GetPivotGridField(pivotGridControl, area, fieldName);
            return IsTotalField(pgf, cellInfo) ? string.Empty : cellInfo.GetCellValue(pgf).ToString();
        }

        //Get info for column and row fields
        public static string GetFieldValues(PivotGridControl pivotGridControl, PivotArea area, string fieldName, PivotCellEventArgs cellInfo)
        {
            PivotGridField pgf = GetPivotGridField(pivotGridControl, area, fieldName);
            return IsTotalField(pgf, cellInfo) ? string.Empty : cellInfo.GetFieldValue(pgf).ToString();
        }

        public static bool IsTotalField(PivotGridField[] fields, PivotCellEventArgs cellInfo)
        {
            int nCount = 0;
            foreach (PivotGridField pgf in fields)
            {
                if (IsTotalField(pgf, cellInfo))
                    nCount++;
            }
            return fields.Length == nCount;
        }

        public static bool IsTotalField(PivotGridField pgf, PivotCellEventArgs cellInfo)
        {
            try
            {
                cellInfo.GetFieldValue(pgf).ToString();
                return false;
            }
            catch
            {
                return true;
            }
        }

        public static void SumFocusedCellValuesInGrid(PivotGridControl pivotGridControl)
        {
            int sum = 0;
            int grpSum = 0;
            int prevX = 0;
            int prevY = 0;
            int grpCnt = 0;
            string result = string.Empty;

            if (pivotGridControl.Cells.MultiSelection.SelectedCells.Count == 0)
                return;

            foreach (Point cell in pivotGridControl.Cells.MultiSelection.SelectedCells)
            {
                if (grpSum == 0)
                {
                    grpSum += Convert.ToInt32(pivotGridControl.Cells.GetCellInfo(cell.X, cell.Y).Value);
                    prevX = cell.X;
                    prevY = cell.Y;

                    continue;
                }

                if ((cell.X == prevX || cell.Y == prevY) || (cell.X == prevY) && (cell.Y == prevX))
                {
                    grpSum += Convert.ToInt32(pivotGridControl.Cells.GetCellInfo(cell.X, cell.Y).Value);
                }
                else
                {
                    result += string.Format("GROUP_" + "{0}" + ": {1}" + "\n", grpCnt, grpSum);
                    sum += grpSum;
                    grpCnt++;
                    grpSum = Convert.ToInt32(pivotGridControl.Cells.GetCellInfo(cell.X, cell.Y).Value); ;
                }

                prevX = cell.X;
                prevY = cell.Y;
            }

            result += string.Format("GROUP_" + "{0}" + ": {1}" + "\n", grpCnt, grpSum);
            sum += grpSum;

            result += string.Format("전체합계 : " + "{0}", sum);
            MessageBox.Show(result, "합계", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public class DataViewTable : XtraPivotGridHelper.DataViewTable
        {
            public XtraPivotGridHelper.DataViewColumn AddColumn(string name, Type type, PivotArea pivotArea, PivotSummaryType? pivotSummaryType, PivotGroupInterval? groupInterval)
            {
                return AddColumn(name, name, type, pivotArea, pivotSummaryType, groupInterval);
            }
        }

        public static void ExportToExcel(PivotGridControl pivotGrid, string docName)
        {
            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Title = "Save Excel File";
            dialog.Filter = "Excel files (*.xls)|*.xls";
            dialog.FileName = StringHelper.Trim(docName.Replace('/', '_') + ".xls");

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                pivotGrid.ExportToXls(dialog.FileName);

                if (MessageBox.Show("Excel File을 여시겠습니까?", "Open", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    System.Diagnostics.Process ps = new System.Diagnostics.Process();
                    ps.StartInfo.FileName = dialog.FileName;
                    ps.Start();
                }
            }
        }

        public class DataViewColumn : DataColumn
        {
            public PivotArea FieldArea;
            public PivotSummaryType? SummaryType;
            public PivotGroupInterval? GroupInterval;

            public DataViewColumn()
                : base()
            {
            }

            public DataViewColumn(string name, Type type, PivotArea fieldArea, PivotSummaryType? summaryType, PivotGroupInterval? groupInterval)
                : base(name, type)
            {
                this.FieldArea = fieldArea;
                this.SummaryType = summaryType;
                this.GroupInterval = groupInterval;
            }
        }
    }

    public class PivotGridLayoutHelper
    {
        static private DataSet GreateSchemaDs()
        {
            DataSet ds = new DataSet("PivotGridLayout");

            ds.Tables.AddRange(new DataTable[] { FieldProperty.GreateSchemaDt(), OptionsView.GreateSchemaDt() });

            return ds;
        }

        static public DataSet GetLayOutFromPivotGrid(PivotGridControl pivotGrid)
        {
            try
            {
                DataSet ds = GreateSchemaDs();

                FieldProperty.GetPivotGridFieldProperty(pivotGrid, ds);
                OptionsView.GetPivotGridOtionsView(pivotGrid, ds);
                

                return ds;
            }
            catch
            {
                return null;
            }
        }

        static public void ApplyLayOutFromPivotGrid(PivotGridControl pivotGrid, DataSet ds)
        {
            try
            {
                if (ds == null)
                    return;

                FieldProperty.LoadXml_FieldProperty(pivotGrid, ds);
                OptionsView.LoadXml_OtionsView(pivotGrid, ds);
            }
            catch { }
        }

        public static void SaveLayout(PivotGridControl pivotGrid, string pageID, string appPath)
        {
            string message = "Save Default Layout ?";
            DialogResult result = MessageBox.Show(message, "Save Default Layout", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

            if (result == DialogResult.OK)
            {
                string dirPath = string.Format("{0}\\DefaultLayOut", appPath);
                string fileName = string.Format("{0}.xml", pageID);
                XtraPivotGridHelper.SaveXml(pivotGrid, dirPath, fileName);
            }
        }

        static public void SaveXml(PivotGridControl pivotGrid, string dirPath, string fileName)
        {
            try
            {
                DataSet ds = GetLayOutFromPivotGrid(pivotGrid);

                if (ds == null) return;

                if (ValidateItemPath(dirPath, true) == false) return;

                string filePath = string.Format("{0}\\{1}", dirPath, fileName);

                FileStream stream = new FileStream(filePath, FileMode.Create);
                XmlTextWriter xmlWriter = new XmlTextWriter(stream, System.Text.Encoding.Unicode);

                ds.WriteXml(xmlWriter);

                xmlWriter.Close();
                stream.Close();
            }
            catch { }
        }

        static public DataSet LoadXml(PivotGridControl pivotGrid, string dirPath, string fileName)
        {
            try
            {
                string filePath = string.Format("{0}\\{1}", dirPath, fileName);
                DataSet ds = GreateSchemaDs();
                ds.ReadXml(filePath, XmlReadMode.IgnoreSchema);

                ApplyLayOutFromPivotGrid(pivotGrid, ds);

                return ds;
            }
            catch
            {
                return null;
            }
        }

        private class FieldProperty
        {
            public const string TableName = "FieldProperty";
            public const string FieldName = "FieldName";
            public const string AreaPosition = "AreaPosition";
            public const string PositionIndex = "PositionIndex";
            public const string ValuesExcluded = "ValuesExcluded";
            public const string SortOrder = "SortOrder";
            public const string Width = "Width";
            public const string Visible = "Visible";

            static public DataTable GreateSchemaDt()
            {
                DataTable dt = new DataTable(TableName);

                dt.Columns.Add(FieldName, typeof(string));
                dt.Columns.Add(AreaPosition, typeof(string));
                dt.Columns.Add(PositionIndex, typeof(string));
                dt.Columns.Add(ValuesExcluded, typeof(string));
                dt.Columns.Add(SortOrder, typeof(string));
                dt.Columns.Add(Width, typeof(string));
                dt.Columns.Add(Visible, typeof(string));

                return dt;
            }

            static public void GetPivotGridFieldProperty(PivotGridControl pivotGrid, DataSet ds)
            {
                DataTable dt = ds.Tables[TableName];
                foreach (PivotGridField field in pivotGrid.Fields)
                {
                    DataRow row = dt.NewRow();

                    row[FieldName] = field.FieldName;
                    row[AreaPosition] = field.Area.ToString();
                    row[PositionIndex] = field.AreaIndex.ToString();
                    row[ValuesExcluded] = ObjectsToString(field.FilterValues.ValuesExcluded);
                    row[SortOrder] = field.SortOrder.ToString();
                    row[Width] = field.Width.ToString();
                    row[Visible] = field.Visible.ToString();

                    dt.Rows.Add(row);
                }
            }

            static public void LoadXml_FieldProperty(PivotGridControl pivotGrid, DataSet ds)
            {
                DataTable dt = ds.Tables[TableName];

                foreach (DataRow row in dt.Rows)
                {
                    string sFieldName = row.GetString(FieldName);

                    if (string.IsNullOrEmpty(sFieldName)) continue;
                    //TODO : 확인 필요
                    PivotGridField field = pivotGrid.Fields.GetFieldByName(PivotGridHelper.GetFieldName(sFieldName));

                    if (field == null) continue;

                    string sAreaPosition = row.GetString(AreaPosition);
                    string sPositionIndex = row.GetString(PositionIndex);
                    string sValuesExcluded = row.GetString(ValuesExcluded);
                    string sSortOrder = row.GetString(SortOrder);
                    string sWidth = row.GetString(Width);
                    string sVisible = row.GetString(Visible);

                    field.SetAreaPosition(ConvertToPivotArea(sAreaPosition), Convert.ToInt32(sPositionIndex));
                    if (string.IsNullOrEmpty(sValuesExcluded) == false) field.FilterValues.SetValues(StringToObjects(sValuesExcluded), PivotFilterType.Excluded, true);
                    field.SortOrder = ConvertToPivotSortOrder(sSortOrder);
                    field.Width = Convert.ToInt32(sWidth);
                    field.Visible = Convert.ToBoolean(sVisible);
                }
            }
        }

        private class OptionsView
        {
            public const string TableName = "OptionsView";
            public const string PropertieName = "PropertieName";
            public const string Checked = "Checked";

            static public DataTable GreateSchemaDt()
            {
                DataTable dt = new DataTable(TableName);

                dt.Columns.Add(PropertieName, typeof(string));
                dt.Columns.Add(Checked, typeof(string));

                return dt;
            }

            static public void GetPivotGridOtionsView(PivotGridControl pivotGrid, DataSet ds)
            {
                DataTable dt = ds.Tables[TableName];
                if (dt == null)
                    return;

                PropertyDescriptorCollection pds = TypeDescriptor.GetProperties(pivotGrid.OptionsView);
                foreach (PropertyDescriptor pd in pds)
                {
                    if (pd.PropertyType.Equals(typeof(bool)) && pd.Name.IndexOf("Total") > -1)
                    {
                        DataRow row = dt.NewRow();

                        row[PropertieName] = pd.Name;
                        row[Checked] = SetOptions.OptionValueByString(pd.Name, pivotGrid.OptionsView);

                        dt.Rows.Add(row);
                    }
                }
            }

            static public void LoadXml_OtionsView(PivotGridControl pivotGrid, DataSet ds)
            {
                DataTable dt = ds.Tables[TableName];
                if (dt == null)
                    return;

                foreach (DataRow row in dt.Rows)
                {
                    string sPropertieName = row.GetString(PropertieName);
                    string sChecked = row.GetString(Checked);

                    SetOptions.SetOptionValueByString(sPropertieName, pivotGrid.OptionsView, Convert.ToBoolean(sChecked));
                }
            }
        }

        static private bool ValidateItemPath(string itemPath, bool createIfNeeded)
        {
            if (string.IsNullOrEmpty(itemPath))
                return false;

            char[] invalidChars = new char[] 
			{ 
                '"', '<', '>', '|', '?', '*', '\0', '\b', '\x10', '\x11', 
				'\x12', '\x14', '\x15', '\x16', '\x17', '\x18', '\x19'
            };

            try
            {
                if (itemPath.IndexOfAny(invalidChars) >= 0
                    || !Path.IsPathRooted(itemPath)
                    || Path.GetFullPath(itemPath) == null
                    || itemPath.Length > 256)
                {
                    return false;
                }

                if (createIfNeeded)
                    Directory.CreateDirectory(itemPath);

                return true;
            }
            catch
            {
                return false;
            }
        }
        static public string ObjectsToString(object[] objects)
        {
            string str = "";

            foreach (object oValue in objects)
            {
                string sValue = oValue.ToString();

                if (str == "") str = sValue;
                else str += "," + sValue;
            }

            return str;
        }
        static public object[] StringToObjects(string str)
        {
            return str.Split(',');
        }
        static public PivotArea ConvertToPivotArea(string sAreaPosition)
        {
            if (sAreaPosition == PivotArea.ColumnArea.ToString()) return PivotArea.ColumnArea;

            if (sAreaPosition == PivotArea.DataArea.ToString()) return PivotArea.DataArea;

            if (sAreaPosition == PivotArea.RowArea.ToString()) return PivotArea.RowArea;

            if (sAreaPosition == PivotArea.FilterArea.ToString()) return PivotArea.FilterArea;

            return PivotArea.FilterArea;
        }
        static public PivotSortOrder ConvertToPivotSortOrder(string sSortOrder)
        {
            if (sSortOrder == PivotSortOrder.Ascending.ToString()) return PivotSortOrder.Ascending;

            if (sSortOrder == PivotSortOrder.Descending.ToString()) return PivotSortOrder.Descending;

            return PivotSortOrder.Ascending;
        }
    }

    public class PivotGridExportHelper
    {
        #region Print And Export
        PivotGridControl exportGrid;
        Thread thread;
        bool stop;
        static void DoExportEx(string title, string filter, string exportFormat, PivotGridControl grid)
        {
            if (grid == null) return;

            string fname = grid.FindForm().Text;
            fname = fname.Replace(" ", "");
            fname = fname.Replace('/', '_');
            //fname = FileUtility.GetValidateItemName(fname);
            fname = fname + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");

            string fileName = ShowSaveFileDialog(title, filter, fname);

            if (string.IsNullOrEmpty(fileName) == false)
            {
                PivotGridExportHelper exporter = new PivotGridExportHelper();
                exporter.exportGrid = grid;
                exporter.stop = false;
                exporter.thread = new Thread(new ThreadStart(exporter.StartExport));
                exporter.thread.Start();
                Cursor currentCursor = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;

                switch (exportFormat)
                {
                    case "htm": grid.ExportToHtml(fileName);
                        break;
                    case "mht": grid.ExportToMht(fileName);
                        break;
                    case "pdf": grid.ExportToPdf(fileName);
                        break;
                    case "xls": grid.ExportToXls(fileName);
                        break;
                    case "rtf": grid.ExportToRtf(fileName);
                        break;
                    case "txt": grid.ExportToText(fileName, "\t");
                        break;
                }
                exporter.EndExport();
                Cursor.Current = currentCursor;
                OpenFile(fileName);
            }
        }

        public static string ShowSaveFileDialog(string title, string filter, string fileName)
        {
            SaveFileDialog dlg = new SaveFileDialog();

            if (string.IsNullOrEmpty(fileName))
            {
                string name = System.Windows.Forms.Application.ProductName;
                int n = name.LastIndexOf(".") + 1;
                if (n > 0) name = name.Substring(n, name.Length - n);
                fileName = name;
            }

            dlg.Title = "Export To " + title;
            dlg.FileName = fileName;
            dlg.Filter = filter;
            if (dlg.ShowDialog() == DialogResult.OK) return dlg.FileName;
            return "";
        }

        public static void ExportToExcel(PivotGridControl grid)
        {
            DoExportEx("Microsoft Excel Document", "Microsoft Excel|*.xls", "xls", grid);
        }
        public static void ExportToMht(PivotGridControl grid)
        {
            DoExportEx("Mht Document", "Mht Files|*.mht", "mht", grid);
        }

        public static void ExportToPdf(PivotGridControl grid)
        {
            DoExportEx("Pdf Document", "Pdf Files|*.pdf", "pdf", grid);
        }

        public static void ExportToRtf(PivotGridControl grid)
        {
            DoExportEx("Rtf Document", "Rtf Files|*.rtf", "rtf", grid);
        }

        public static void ExportToHtml(PivotGridControl grid)
        {
            DoExportEx("HTML Document", "HTML Documents|*.html", "htm", grid);
        }

        public static void ExportToTxt(PivotGridControl grid)
        {
            DoExportEx("Text Document", "Text Files|*.txt", "txt", grid);
        }

        public static void OpenFile(string fileName)
        {
            if (DevExpress.XtraEditors.XtraMessageBox.Show("Do you want to open this file?", "Export To...", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = fileName;
                    process.StartInfo.Verb = "Open";
                    process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                    process.Start();
                }
                catch
                {
                    DevExpress.XtraEditors.XtraMessageBox.Show("Cannot find an application on your system suitable for openning the file with exported data.",
                        System.Windows.Forms.Application.ProductName,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        
        void StartExport()
        {
            Thread.Sleep(400);
            if (stop)
                return;

            Control control = exportGrid;
            while (control.Parent != null)
                control = control.Parent;

            try
            {
                var progressForm = new CSOT.UserInterface.UIComponentFactory.ExportProgressDialog(control as Form);
                progressForm.Show();

                while (!stop)
                {
                    System.Windows.Forms.Application.DoEvents();
                    Thread.Sleep(100);
                }

                progressForm.Dispose();
            }
            catch
            {
            }
        }

        void EndExport()
        {
            stop = true;
            thread.Join();
        }

        #endregion
    }

    public class GridExportHelper
    {
        #region Print And Export
        GridControl exportGrid;
        Thread thread;
        bool stop;
        static void DoExportEx(string title, string filter, string exportFormat, GridControl grid)
        {
            if (grid == null) return;

            string fname = grid.FindForm().Text;
            fname = fname.Replace(" ", "");
            fname = fname.Replace('/', '_');
            //fname = FileUtility.GetValidateItemName(fname);
            fname = fname + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");

            string fileName = ShowSaveFileDialog(title, filter, fname);

            if (string.IsNullOrEmpty(fileName) == false)
            {
                GridExportHelper exporter = new GridExportHelper();
                exporter.exportGrid = grid;
                exporter.stop = false;
                exporter.thread = new Thread(new ThreadStart(exporter.StartExport));
                exporter.thread.Start();
                Cursor currentCursor = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;

                switch (exportFormat)
                {
                    case "htm": grid.ExportToHtml(fileName);
                        break;
                    case "mht": grid.ExportToMht(fileName);
                        break;
                    case "pdf": grid.ExportToPdf(fileName);
                        break;
                    case "xls": grid.ExportToXls(fileName);
                        break;
                    case "rtf": grid.ExportToRtf(fileName);
                        break;
                    case "txt": grid.ExportToText(fileName);
                        break;
                }
                exporter.EndExport();
                Cursor.Current = currentCursor;
                OpenFile(fileName);
            }
        }

        public static string ShowSaveFileDialog(string title, string filter, string fileName)
        {
            SaveFileDialog dlg = new SaveFileDialog();

            if (string.IsNullOrEmpty(fileName))
            {
                string name = System.Windows.Forms.Application.ProductName;
                int n = name.LastIndexOf(".") + 1;
                if (n > 0) name = name.Substring(n, name.Length - n);
                fileName = name;
            }

            dlg.Title = "Export To " + title;
            dlg.FileName = fileName;
            dlg.Filter = filter;
            if (dlg.ShowDialog() == DialogResult.OK) return dlg.FileName;
            return "";
        }

        public static void ExportToExcel(GridControl grid)
        {
            DoExportEx("Microsoft Excel Document", "Microsoft Excel|*.xls", "xls", grid);
        }
        public static void ExportToMht(GridControl grid)
        {
            DoExportEx("Mht Document", "Mht Files|*.mht", "mht", grid);
        }

        public static void ExportToPdf(GridControl grid)
        {
            DoExportEx("Pdf Document", "Pdf Files|*.pdf", "pdf", grid);
        }

        public static void ExportToRtf(GridControl grid)
        {
            DoExportEx("Rtf Document", "Rtf Files|*.rtf", "rtf", grid);
        }

        public static void ExportToHtml(GridControl grid)
        {
            DoExportEx("HTML Document", "HTML Documents|*.html", "htm", grid);
        }

        public static void ExportToTxt(GridControl grid)
        {
            DoExportEx("Text Document", "Text Files|*.txt", "txt", grid);
        }

        public static void OpenFile(string fileName)
        {
            if (DevExpress.XtraEditors.XtraMessageBox.Show("Do you want to open this file?", "Export To...", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = fileName;
                    process.StartInfo.Verb = "Open";
                    process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                    process.Start();
                }
                catch
                {
                    DevExpress.XtraEditors.XtraMessageBox.Show("Cannot find an application on your system suitable for openning the file with exported data.",
                        System.Windows.Forms.Application.ProductName,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        void StartExport()
        {
            Thread.Sleep(400);
            if (stop)
                return;

            Control control = exportGrid;
            while (control.Parent != null)
                control = control.Parent;

            try
            {
                var progressForm = new CSOT.UserInterface.UIComponentFactory.ExportProgressDialog(control as Form);
                progressForm.Show();

                while (!stop)
                {
                    System.Windows.Forms.Application.DoEvents();
                    Thread.Sleep(100);
                }

                progressForm.Dispose();
            }
            catch
            {
            }
        }

        void EndExport()
        {
            stop = true;
            thread.Join();
        }

        #endregion
    }
}
