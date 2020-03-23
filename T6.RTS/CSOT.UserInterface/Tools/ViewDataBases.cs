using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Mozart.Studio.TaskModel.UserInterface;
using Mozart.Studio.TaskModel.Projects;
using Mozart.Linq;
using Mozart.Studio.TaskModel.UserLibrary;
using Mozart.Studio.Application;
using Mozart.Task.Model;
using DevExpress.XtraGrid.Columns;
using CSOT.UserInterface.Utils;

namespace CSOT.UserInterface.Tools
{
    public partial class ViewDataBases : XtraUserControlView
    {
        const string _pageID = "ViewDataBases";

        public ViewDataBases()
        {
            InitializeComponent();
        }

        public ViewDataBases(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
        }

        public IVsApplication Application
        {
            get { return (IVsApplication)this.ServiceProvider.GetService(typeof(IVsApplication)); }
        }

        protected override void LoadDocument()
        {
            if (this.Document == null)
                return;

            IExperimentResultItem result = this.Document.GetResultItem();
            if (result != null)
            {
                gridControl.DataSource = CreateDataTable(result);
                gridView.PopulateColumns();

                foreach (GridColumn column in gridView.Columns)
                {
                    if (column.ColumnType == typeof(DateTime))
                        column.ColumnEdit = repositoryItemDateEdit1;

                    if (column.FieldName == "CONNECTION_STRING")
                        column.ColumnEdit = repositoryItemMemoExEdit1;

                    if (column.ColumnType == typeof(int) ||
                        column.ColumnType == typeof(long) ||
                        column.ColumnType == typeof(decimal))
                    {
                        column.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Custom;
                        column.DisplayFormat.FormatString = "{0:#,###,###,###}";
                        column.SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
                        column.SummaryItem.DisplayFormat = "{0:#,###,###,###}";
                    }
                }

                gridView.BestFitColumns();
            }
        }

        private DataTable CreateDataTable(IExperimentResultItem result)
        {            
            DataTable dt = CreateSchema();

            ModelEngine engine = result.Model.TargetObject as ModelEngine;

            ModelEngine parent = engine.Parent ?? engine;
            CollectDataSource(dt, parent.DataSources, parent.Name);

            if (parent.HasLinkedModels)
            {
                foreach (string fileName in parent.LinkedModels.Values)
                {
                    ModelEngine m = ModelEngine.Load(fileName);
                    if (m == null)
                        continue;

                    CollectDataSource(dt, m.DataSources, m.Name);
                }
            }

            return dt;
        }

        private void CollectDataSource(DataTable dt, Mozart.DataActions.DataSourceRepository repository, string modelName)
        {
            if (dt == null || repository == null)
                return;

            foreach (var ds in repository.GetDataSources())
            {
                if (ds.HasItems == false)
                    break;

                foreach (var it in ds.Items)
                {
                    string dataSource = ParseValue(it.ConnectionString, "data source");
                    string userID = ParseValue(it.ConnectionString, "user id");

                    AddDataRow(dt, modelName, ds.Name, it.Name, dataSource, userID, it.ConnectionString, it.Description);
                }
            }
        }

        private DataRow AddDataRow(DataTable dt, string modelName, 
            string dsName, string conName, string dataSource, string userID, string connectionString, string description)
        {
            DataRow row = dt.NewRow();

            if (string.IsNullOrEmpty(conName))
                conName = "[N/A]";

            row[dt.Columns["MODEL_NAME"]] = modelName;
            row[dt.Columns["DS_NAME"]] = dsName;
            row[dt.Columns["CON_NAME"]] = conName;
            row[dt.Columns["DATA_SOURCE"]] = dataSource;
            row[dt.Columns["USER_ID"]] = userID;
            row[dt.Columns["CONNECTION_STRING"]] = connectionString;
            row[dt.Columns["DESCRIPTION"]] = description;

            dt.Rows.Add(row);

            return row;
        }

        private DataTable CreateSchema()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("MODEL_NAME", typeof(string));
            dt.Columns.Add("DS_NAME", typeof(string));
            dt.Columns.Add("CON_NAME", typeof(string));
            dt.Columns.Add("DATA_SOURCE", typeof(string));
            dt.Columns.Add("USER_ID", typeof(string));
            dt.Columns.Add("CONNECTION_STRING", typeof(string));
            dt.Columns.Add("DESCRIPTION", typeof(string));

            return dt;
        }

        #region Utils

        private string ParseValue(string connectionString, string findkey)
        {
            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(findkey))
                return string.Empty;

            var kv = ParseParamKeyValue(connectionString);
            if (kv != null)
            {
                string dataSource;
                if (kv.TryGetValue(findkey, out dataSource))
                    return dataSource.Replace("'", "");
            }

            return string.Empty;
        }

        private static Dictionary<string, string> ParseParamKeyValue(string paramValue)
        {
            Dictionary<string, string> kv = new Dictionary<string, string>();

            try
            {
                var list = paramValue.Split(';');
                foreach (string str in list)
                {
                    if (str.Contains("=") == false)
                        continue;

                    string[] arr = str.Split('=');
                    if (arr.Length != 2)
                        continue;

                    //검색을 위해 소문자로 변환
                    string key = arr[0].Trim().ToLower();
                    string value = arr[1].Trim();

                    if (kv.ContainsKey(key) == true)
                        continue;

                    kv.Add(key, value);
                }
            }
            catch { }

            return kv;
        }

        #endregion Utils

        private void BtnExcel_Click(object sender, EventArgs e)
        {
            GridExportHelper.ExportToExcel(gridControl);
        }
    }
}
