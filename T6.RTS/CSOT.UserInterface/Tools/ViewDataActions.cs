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
    public partial class ViewDataActions : XtraUserControlView
    {
        const string _pageID = "ViewDataActions";

        public ViewDataActions()
        {
            InitializeComponent();
        }

        public ViewDataActions(IServiceProvider serviceProvider)
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

                    if (column.FieldName == "CMD_TEXT")
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
            DataTable dt = CreateSchema(true);

            ModelEngine engine = result.Model.TargetObject as ModelEngine;            
            if (engine == null)
                return dt;

            ModelEngine parent = engine.Parent ?? engine;            
            CollectDataAction(dt, parent);

            if (parent.HasLinkedModels)
            {
                foreach (string fileName in parent.LinkedModels.Values)
                {
                    ModelEngine m = ModelEngine.Load(fileName);
                    if (m == null)
                        continue;

                    CollectDataAction(dt, m);
                }
            }
            
            return dt;
        }

        private void CollectDataAction(DataTable dt, ModelEngine engine)
        {
            HashSet<string> objectSet = new HashSet<string>();
            HashSet<string> dbLinkSet = new HashSet<string>();

            DataTable objectDt = null;
            if (engine.Inputs.GetItem("AllObjects") != null)
            {
                var experiment = engine.GetExperiment(0);
                if (experiment != null)
                    objectDt = experiment.LoadInput("AllObjects");
            }

            CollectObjectSet(objectDt, objectSet, dbLinkSet);

            string modelName = engine.Name;

            //Inputs
            CollectDataAction(dt, engine.Inputs, objectSet, dbLinkSet, modelName);

            //Outputs
            CollectDataAction(dt, engine.Outputs, objectSet, dbLinkSet, modelName);
        }

        private void CollectObjectSet(DataTable dt, HashSet<string> objectSet, HashSet<string> dbLinkSet)
        {
            if (dt == null)
                return;

            DataColumn nameColumn = dt.Columns["OBJECT_NAME"];
            if (nameColumn == null)
                return;

            DataColumn typeColumn = dt.Columns["OBJECT_TYPE"];

            foreach (DataRow row in dt.Rows)
            {
                string name = row[nameColumn] as string;
                if (string.IsNullOrEmpty(name))
                    continue;

                //2자리 이상만 유효, 대문자 기준으로 변환
                string key = name.ToUpper().Trim();
                if (key.Length < 2)
                    continue;

                bool isDbLink = false;
                if (typeColumn != null)
                {
                    string otype = row[typeColumn] as string;
                    if (string.Equals(otype, "DB_LINK", StringComparison.CurrentCultureIgnoreCase)
                        || string.Equals(otype, "DBLINK", StringComparison.CurrentCultureIgnoreCase))
                    {
                        isDbLink = true;
                    }
                }

                HashSet<string> targetSet = isDbLink ? dbLinkSet : objectSet;
                targetSet.Add(key);
            }
        }

        private void CollectDataAction(DataTable dt, Mozart.DataActions.DataItemRepository repository, HashSet<string> objectSet, HashSet<string> dbLinkSet, string modelName)
        {
            if (dt == null || repository == null)
                return;

            foreach (var item in repository.GetItems())
            {
                string category = item.Category;

                foreach (var da in item.Actions)
                {
                    string name = item.Name;
                    string mainDS = da.DataSource;

                    foreach (var cmd in da.Commands)
                    {
                        AddDataRow(dt,
                                   modelName,
                                   "COMMAND_TYPE",
                                   category,
                                   name,
                                   cmd.Name,                                   
                                   mainDS,
                                   cmd.DataSource,
                                   cmd.CommandText,
                                   objectSet,
                                   dbLinkSet);
                    }
                }
            }
        }

        private DataRow AddDataRow(DataTable dt, string modelName, string cmdType, string category, string daName, string cmdName,
            string mainDS, string altDs, string cmdText, HashSet<string> objectList, HashSet<string> dbLinkList)
        {
            DataRow row = dt.NewRow();
                        
            row[dt.Columns["MODEL_NAME"]] = modelName;
            row[dt.Columns["CMD_TYPE"]] = cmdType;
            row[dt.Columns["CATEGORY"]] = category;
            row[dt.Columns["DATAACTION_NAME"]] = daName;
            row[dt.Columns["CMD_NAME"]] = cmdName;            
            row[dt.Columns["MAIN_DB"]] = mainDS;
            row[dt.Columns["ALT_DS"]] = altDs;
            row[dt.Columns["REAL_DS"]] = string.IsNullOrEmpty(altDs) ? mainDS : altDs;
            row[dt.Columns["CMD_TEXT"]] = cmdText;

            if (dt.Columns["MATCHED_INFOS"] != null)
            {
                string infos = GetMatchedInfos(cmdText, objectList, false);
                if (string.IsNullOrEmpty(infos) == false)
                    row[dt.Columns["MATCHED_INFOS"]] = infos;
            }

            if (dt.Columns["MATCHED_DBLINKS"] != null)
            {
                string infos = GetMatchedInfos(cmdText, dbLinkList, true);
                if (string.IsNullOrEmpty(infos) == false)
                    row[dt.Columns["MATCHED_DBLINKS"]] = infos;
            }

            dt.Rows.Add(row);

            return row;
        }

        private string GetMatchedInfos(string cmdText, HashSet<string> findSet, bool isDbLink)
        {
            if (string.IsNullOrEmpty(cmdText))
                return string.Empty;

            if (findSet == null || findSet.Count == 0)
                return string.Empty;

            string str = RemoveComment(cmdText);

            //대문자로 변환 비교
            str = str.ToUpper();

            int length = str.Length;

            List<string> list = new List<string>();
            foreach (var find in findSet)
            {
                if (str.Contains(find))
                {
                    int findLength = find.Length;
                    int startIndex = 0;
                    while (startIndex < length)
                    {
                        int idx = str.IndexOf(find, startIndex);
                        if (idx < 0 || idx >= length)
                            break;

                        //일치한 문자열 앞/뒤 문자가 공백인 경우만 유효                        
                        string prevStr = idx <= 0 ? null : str.Substring(idx - 1, 1);

                        bool isValid = isDbLink ? IsMatched_DbLink(prevStr) : IsMatched_Object(prevStr);
                        if (isValid)
                        {
                            string nextStr = str.Substring(idx + findLength, 1);
                            if (IsMatched_Object(nextStr))
                            {
                                list.Add(find);
                                break;
                            }
                        }

                        startIndex = idx + 1;
                    }
                }
            }

            list.Sort();

            string infos = "";
            foreach (var find in list)
            {
                if (infos == "")
                    infos = find;
                else
                    infos += "," + find;
            }

            return infos;
        }

        private string RemoveComment(string text)
        {
            // /* ~ */ 형식 제거
            int start = text.IndexOf("/*");
            while (start >= 0)
            {
                int end = text.IndexOf("*/");

                if (end < 0)
                    text = text.Remove(start);
                else
                    text = text.Remove(start, (end - start + 2));

                start = text.IndexOf("/*");
            }

            return RemoveCommentLine(text);
        }

        private string RemoveCommentLine(string text)
        {
            StringBuilder sb = new StringBuilder();

            // -- 형식 주석 라인 제외
            StringReader sr = new StringReader(text);
            string lineText = null;
            do
            {
                lineText = sr.ReadLine();
                if (lineText == null)
                    break;

                int start = lineText.IndexOf("--");
                if (start >= 0)
                    lineText = lineText.Remove(start);

                if (string.IsNullOrEmpty(lineText) == false)
                    sb.AppendLine(lineText);
            }
            while (lineText != null);

            return sb.ToString();
        }

        private bool IsMatched_Object(string str)
        {
            return string.IsNullOrEmpty(str)
                   || str == " "
                   || str == "\t"
                   || str == "\r\n"
                   || str == "\r"
                   || str == "\n"
                   || str == "("    //(ObjectName, ObjectName( )
                   || str == ")"    //ObjectName)
                   || str == "."    //계정.TableName
                   || str == ","    //TableName,
                   || str == "@"    //TableName@링크
                   ;
        }

        private bool IsMatched_DbLink(string str)
        {
            return str == "@";
        }
                
        private DataTable CreateSchema(bool isEtc)
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("MODEL_NAME", typeof(string));
            dt.Columns.Add("CMD_TYPE", typeof(string));
            dt.Columns.Add("CATEGORY", typeof(string));
            dt.Columns.Add("DATAACTION_NAME", typeof(string));
            dt.Columns.Add("CMD_NAME", typeof(string));            
            dt.Columns.Add("MAIN_DB", typeof(string));
            dt.Columns.Add("ALT_DS", typeof(string));
            dt.Columns.Add("REAL_DS", typeof(string));
            dt.Columns.Add("CMD_TEXT", typeof(string));

            if (isEtc)
            {
                dt.Columns.Add("MATCHED_INFOS", typeof(string));
                dt.Columns.Add("MATCHED_DBLINKS", typeof(string));
            }

            return dt;
        }

        private void BtnExcel_Click(object sender, EventArgs e)
        {
            GridExportHelper.ExportToExcel(gridControl);
        }
    }
}
