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
using System.IO;
using CSOT.UserInterface.Utils;

namespace CSOT.UserInterface.Tools
{
    public partial class ViewSchemas : XtraUserControlView
    {
        const string _pageID = "ViewSchemas";

        public ViewSchemas()
        {
            InitializeComponent();
        }

        public ViewSchemas(IServiceProvider serviceProvider)
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

                    //if (column.FieldName == "CMD_TEXT")
                    //    column.ColumnEdit = repositoryItemMemoExEdit1;

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
            if (engine == null)
                return dt;

            ModelEngine parent = engine.Parent ?? engine;            
            CollectSchemas(dt, parent);

            if (parent.HasLinkedModels)
            {
                foreach (string fileName in parent.LinkedModels.Values)
                {
                    ModelEngine m = ModelEngine.Load(fileName);
                    if (m == null)
                        continue;

                    CollectSchemas(dt, m);
                }
            }
            
            return dt;
        }

        private void CollectSchemas(DataTable dt, ModelEngine engine)
        {
            string modelName = engine.Name;

            //Inputs
            CollectSchemas(dt, engine.Inputs, modelName, "Inputs");

            //Outputs
            CollectSchemas(dt, engine.Outputs, modelName, "Outputs");
        }

        private void CollectSchemas(DataTable dt, Mozart.DataActions.DataItemRepository repository, string modelName, string dataLayer)
        {
            if (dt == null || repository == null)
                return;

            foreach (var item in repository.GetItems())
            {
                string category = item.Category;

                var schema = item.Schema;
                if (schema == null)
                    continue;

                string itemName = item.Name;
                var metaInfos = item.MetaInfo;

                foreach (DataColumn col in schema.Columns)
                {
                    string colName = col.ColumnName;
                    string dataType = col.DataType.Name;

                    string defaultValue = null;
                    bool allowNull = false;
                    bool isPrimaryKey = false;
                    bool hidden = false;
                    string caption = null;
                    string description = null;

                    if (metaInfos != null && metaInfos.Properties != null)
                    {
                        var info = metaInfos.Properties.FirstOrDefault(t => t.Name == col.ColumnName);
                        if (info != null)
                        {
                            defaultValue = info.DefaultValue == null ? null : info.DefaultValue.ToString();
                            allowNull = info.AllowNulls;
                            isPrimaryKey = info.IsPrimaryKey;
                            hidden = info.Hidden;
                            caption = info.Caption;
                            description = info.Description;
                        }
                    }
                    
                    AddDataRow(dt,
                               modelName,
                               dataLayer,
                               category,
                               itemName,
                               colName,
                               dataType,
                               defaultValue,
                               allowNull,
                               isPrimaryKey,
                               hidden,
                               caption,
                               description);
                }                
            }
        }

        private DataRow AddDataRow(DataTable dt, string modelName, string dataLayer, string category, 
            string schemaName, string colName, string dataType, string defaultValue,
            bool allowNull, bool isPrimaryKey, bool hidden, string caption, string description)
        {
            DataRow row = dt.NewRow();

            row[dt.Columns["MODEL_NAME"]] = modelName;
            row[dt.Columns["DATA_LAYER"]] = dataLayer;
            row[dt.Columns["CATEGORY"]] = category;
            row[dt.Columns["SCHEMA_NAME"]] = schemaName;
            row[dt.Columns["COLUMN_NAME"]] = colName;
            row[dt.Columns["DATA_TYPE"]] = dataType;

            if(defaultValue != null)
                row[dt.Columns["DEFAULT_VALUE"]] = defaultValue;            

            if(allowNull)
                row[dt.Columns["ALLOW_NULL"]] = CommonHelper.ToStringYN(allowNull);

            if (isPrimaryKey)
                row[dt.Columns["IS_PRIMARY_KEY"]] = CommonHelper.ToStringYN(isPrimaryKey);

            if (hidden)
                row[dt.Columns["HIDDEN"]] = CommonHelper.ToStringYN(hidden);

            if (caption != null)
                row[dt.Columns["CAPTION"]] = caption;

            if (description != null)
                row[dt.Columns["DESCRIPTION"]] = description;

            dt.Rows.Add(row);

            return row;
        }
              
        private DataTable CreateSchema()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("MODEL_NAME", typeof(string));
            dt.Columns.Add("DATA_LAYER", typeof(string));
            dt.Columns.Add("CATEGORY", typeof(string));
            dt.Columns.Add("SCHEMA_NAME", typeof(string));
            dt.Columns.Add("COLUMN_NAME", typeof(string));
            dt.Columns.Add("DATA_TYPE", typeof(string));
            dt.Columns.Add("DEFAULT_VALUE", typeof(string));            
            dt.Columns.Add("ALLOW_NULL", typeof(string));
            dt.Columns.Add("IS_PRIMARY_KEY", typeof(string));
            dt.Columns.Add("HIDDEN", typeof(string));
            dt.Columns.Add("CAPTION", typeof(string));
            dt.Columns.Add("DESCRIPTION", typeof(string));

            return dt;
        }       

        private void BtnExcel_Click(object sender, EventArgs e)
        {
            GridExportHelper.ExportToExcel(gridControl);
        }
    }
}
