using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mozart.Studio.TaskModel.Projects;
using CSOT.Lcd.Scheduling;
using Mozart.Studio.TaskModel.UserInterface;
using Mozart.Studio.TaskModel.UserLibrary;
using CSOT.Lcd.Scheduling.Outputs;
using CSOT.Lcd.Scheduling.Inputs;
using CSOT.UserInterface.Utils;

namespace CSOT.Lcd.UserInterface.Analysis
{
    public partial class ProductRouteView : XtraGridControlView
    {
        private const int STEP_COLUMN_START_INDEX = 1;
        private const string TFT = "TFT";
        private const string CF = "CF";

        private IExperimentResultItem _result;
        private ResultDataContext _resultCtx;

        private List<StdStep> _stdStepList;
        private Dictionary<string, List<string>> _matchSteps;
        
        //private Dictionary<string, List<string>> _prodRouteLogDic;        
        private Dictionary<string, Color> ProdColors { get; set; }

        private string TargetAreaID
        {
            get { return this.areaComboBox.Text; }
        }

        private bool IsOnlyMainStep
        {
            get
            {
                if (this.chkOnlyMain.Checked == false)
                    return false;

                return true;
            }
        }

        private bool IsUsedProdColor 
        {
            get
            {
                if (this.chkProductColor.Checked == false)
                    return false;

                return true;
            }

        }

        private bool IsMandatory
        {
            get
            {
                if (this.chkOnlyMandatory.Checked == false)
                    return false;

                return true;
            }

        }
        
        public ProductRouteView(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
        }

        protected override void LoadDocument()
        {            
            Initialize();
        }

        private void Initialize()
        {
            var item = (IMenuDocItem)this.Document.ProjectItem;

            _result = (IExperimentResultItem)item.Arguments[0];
            _resultCtx = _result.GetCtx<ResultDataContext>();

            this.ProdColors = new Dictionary<string, Color>();

            SetControl();
        }

        private void SetControl()
        {
            SetControl_AreaID();

            SetProdColors();
        }

        private void SetControl_AreaID()
        {
            this.areaComboBox.Items.Clear();

            SortedSet<string> list = new SortedSet<string>();

            var productRoute = _resultCtx.ProductRouteLog;

            if (productRoute == null)
                return;

            foreach (ProductRouteLog item in productRoute)
            {
                if (list.Contains(item.AREA_ID))
                    continue;

                list.Add(item.AREA_ID);
            }

            foreach (string areaID in list.Reverse())
            {
                if (this.areaComboBox.Items.Contains(areaID))
                    continue;

                this.areaComboBox.Items.Add(areaID);
            }

            if (this.areaComboBox.Items.Count > 0)
                this.areaComboBox.SelectedIndex = 0;
        }

        //private void GetProductRouteDic()
        //{
        //    _prodRouteLogDic = new Dictionary<string, List<string>>();

        //    var prodRouteLog = _resultCtx.ProductRouteLog.Where(x => x.AREA_ID == this.TargetAreaID).OrderBy(x => x.PRODUCT_ID);

        //    foreach (var item in prodRouteLog)
        //    {
        //        string key = CommonHelper.CreateKey(item.AREA_ID, item.PRODUCT_ID);
        //        List<string> stepList;
        //        if (_prodRouteLogDic.TryGetValue(key, out stepList) == false)
        //            _prodRouteLogDic.Add(key, stepList = new List<string>());

        //        if (stepList.Contains(item.STEP_ID) == false)
        //            stepList.Add(item.STEP_ID);
        //    }
        //}

        private void GetStdStepList(string areaID)
        {
            _stdStepList = new List<StdStep>();
            _matchSteps = new Dictionary<string, List<string>>();

            var modelContext = _result.GetCtx<ModelDataContext>();
            var table = modelContext.StdStep;
            
            var finds = table.Where(t => t.AREA_ID == areaID).OrderBy(t => t.STEP_SEQ);
            var stdStepList = _stdStepList = finds.ToList();

            foreach (var stdStep in stdStepList)
            {
                string dspEqpGroupID = stdStep.DSP_EQP_GROUP_ID;
                if (string.IsNullOrEmpty(dspEqpGroupID))
                    continue;

                List<string> list;
                if (_matchSteps.TryGetValue(dspEqpGroupID, out list) == false)
                    _matchSteps.Add(dspEqpGroupID, list = new List<string>());

                string stepID = stdStep.STEP_ID;
                if (list.Contains(stepID))
                    continue;

                list.Add(stepID);
            }
        }

        private void BindGrid()
        {
            this.gridView1.Columns.Clear();

            GetStdStepList(this.TargetAreaID);
            DataTable dt = CreateSchema();            

            var result = _resultCtx.ProductRouteLog.Where(x => x.AREA_ID == this.TargetAreaID);
            var groups = result.GroupBy(t => t.PRODUCT_ID);

            foreach (var it in groups)
            {
                string productID = it.Key;
                var infos = it.OrderBy(t => t.SEQ);
                if (infos == null)
                    continue;

                DataRow row = dt.NewRow();
                row[Schema.PRODUCT_ID] = productID;

                foreach (var item in infos)
                {
                    string stepID = item.STEP_ID;

                    if (dt.Columns.Contains(stepID) == false)
                        continue;

                    //row[stepID] = item.SEQ;
                    row[stepID] = "Y";
                }

                dt.Rows.Add(row);
            }

            this.gridControl1.DataSource = dt;
        }

        private void Query()
        {
            this.Cursor = Cursors.WaitCursor;

            BindGrid();
            DesignGrid();

            this.Cursor = Cursors.Default;
        }

        private void DesignGrid()
        {
            var grid = this.gridView1;
            grid.OptionsView.ColumnAutoWidth = false;
            grid.BestFitColumns();

            grid.ColumnPanelRowHeight = 65;

            int count = grid.Columns.Count;
            int startCol = STEP_COLUMN_START_INDEX;                       

            for (int i = startCol; i < count; i++)
            {
                var col = grid.Columns[i];

                col.Width = 10;
                col.OptionsColumn.AllowEdit = false;

                string stepID = col.FieldName;
                col.ToolTip = GetToolTip(stepID);
            }
        }

        private string GetToolTip(string stepID)
        {
            if (string.IsNullOrEmpty(stepID))
                return stepID;

            var stdStepList = _stdStepList;

            var stdStep = _stdStepList.Find(t => t.STEP_ID == stepID);
            if (stdStep == null)
                return stepID;

            string dspEqpGroupID = stdStep.DSP_EQP_GROUP_ID;
            string infos = "";
                        
            if(string.IsNullOrEmpty(dspEqpGroupID) == false)
            {
                List<string> matchStepList;
                if (_matchSteps.TryGetValue(dspEqpGroupID, out matchStepList))
                {
                    infos = CommonHelper.ToString(matchStepList);

                    if (string.IsNullOrEmpty(infos) == false)
                        infos = " : " + infos;
                }
            }            

            string toolTip = string.Format("{0}\n[{1}{2}]", 
                                            stepID,
                                            dspEqpGroupID ?? "-",
                                            infos);

            return toolTip;
        }

        private DataTable CreateSchema()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add(Schema.PRODUCT_ID, typeof(string));

            var stdStepList = _stdStepList;

            if(this.IsOnlyMainStep)
                stdStepList = stdStepList.FindAll(t => StringHelper.Equals(t.STEP_TYPE, "MAIN"));

            if (this.IsMandatory)
                stdStepList = stdStepList.FindAll(t => StringHelper.Equals(t.IS_MANDATORY, "Y"));

            foreach (var stdStep in stdStepList)
            {
                string stepID = stdStep.STEP_ID;

                if (dt.Columns.Contains(stepID))
                    continue;

                var col = dt.Columns.Add(stepID, typeof(string));
                col.Caption = GetCaption_Step(stepID);
            }

            return dt;
        }

        private string GetCaption_Step(string stepID)
        {
            if (string.IsNullOrEmpty(stepID))
                return stepID;

            StringBuilder sb = new StringBuilder();

            int count = stepID.Length;
            for (int i = 0; i < count; i++)
            {
                var c = stepID[i];

                if(i > 0)
                    sb.Append("\n");

                sb.Append(c);
            }

            return sb.ToString();
        }

        private Color GetBarColorByProductID(string productID)
        {
            Color color = this.BackColor;
            if (string.IsNullOrEmpty(productID))
                return color;
                                                       
            if (this.IsUsedProdColor)
            {
                Color prodColor;
                if (this.ProdColors.TryGetValue(productID, out prodColor))
                    color = prodColor;
            }
            else
            {
                color = ColorGenHelper.GetColorByKey(productID, ColorGenHelper.CATETORY_PRODUCT);
            }
            
            return color;
        }

        protected void SetProdColors()
        {            
            var modelContext = _result.GetCtx<ModelDataContext>();
            var product = modelContext.Product;
            if (product == null)
                return;

            var dic = this.ProdColors;

            foreach (var it in product)
            {
                string key = it.PRODUCT_ID;
                if (string.IsNullOrEmpty(key) || dic.ContainsKey(key))
                    continue;

                int a, r, g, b;
                if (TryGetArgb(it.VIEW_COLOR, out a, out r, out g, out b) == false)
                    continue;

                Color color = Color.FromArgb(a, r, g, b);
                dic.Add(key, color);
            }
        }
                
        private bool TryGetArgb(string viewColor, out int a, out int r, out int g, out int b)
        {
            a = -1;
            r = -1;
            g = -1;
            b = -1;

            if (string.IsNullOrEmpty(viewColor))
                return false;

            var arr = viewColor.Split(',');

            int count = arr.Length;
            if (count < 4)
                return false;

            if (int.TryParse(arr[0], out a) == false)
                return false;

            if (int.TryParse(arr[1], out r) == false)
                return false;

            if (int.TryParse(arr[2], out g) == false)
                return false;

            if (int.TryParse(arr[3], out b) == false)
                return false;

            return true;
        }

        public class ResultData
        {
            public string ProductID { get; set; }
            public string StepID { get; set; }
            public string StepType { get; set; }
            public int StepSeq { get; set; }
        }

        public class Schema
        {
            public const string PRODUCT_ID = "PRODUCT_ID";
            public const string STEP_ID = "STEP_ID";
            public const string STEP_TYPE = "STEP_TYPE";
            public const string STEP_SEQ = "STEP_SEQ";
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            Query();
        }

        private void GridView1_CustomDrawColumnHeader(object sender, DevExpress.XtraGrid.Views.Grid.ColumnHeaderCustomDrawEventArgs e)
        {
            if (e == null || e.Column == null)
                return;
                            
            if (e.Column.FieldName == Schema.PRODUCT_ID)
                return;

            var stdStepList = _stdStepList;
            if (stdStepList == null)
                return;            
            
            string stepID = e.Column.FieldName;
            
            var stdStep = stdStepList.Find(t => t.STEP_ID == stepID);
            bool isMain = false;
            bool isMandatory = false;
            bool isOtherShopStep = false;

            if(stdStep != null)
            {
                isMain = StringHelper.Equals(stdStep.STEP_TYPE, "MAIN");
                isMandatory = CommonHelper.ToBoolYN(stdStep.IS_MANDATORY);

                if (StringHelper.Equals(this.TargetAreaID, TFT))
                {
                    if(StringHelper.Equals(stdStep.SHOP_ID, CF))
                        isOtherShopStep = true;
                }
            }

            if(isMain == false)
                e.Appearance.ForeColor = Color.DarkGray;

            if (isMandatory == false)
                e.Appearance.FontStyleDelta = FontStyle.Underline;

            //e.Appearance.FontStyleDelta = FontStyle.Bold;

            if (isOtherShopStep)
                e.Appearance.FontStyleDelta = FontStyle.Bold;
        }

        private void GridView1_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e == null || e.Column == null)
                return;

            if (e.Column.FieldName == Schema.PRODUCT_ID)
                return;

            if (string.IsNullOrEmpty(e.DisplayText) == false)            
            {
                string productID = (string)this.gridView1.GetRowCellValue(e.RowHandle, Schema.PRODUCT_ID);
                var color = GetBarColorByProductID(productID);

                e.Appearance.BackColor = color;
                e.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

                //Font Color : 보색으로 변경
                if(this.IsUsedProdColor)
                    e.Appearance.ForeColor = ColorGenHelper.GetComplementaryColor(color);
            }
        }
    }
}
