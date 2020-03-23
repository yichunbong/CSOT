namespace CSOT.Lcd.UserInterface.Analysis
{
    partial class SimulationWipView
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            DevExpress.XtraCharts.XYDiagram xyDiagram1 = new DevExpress.XtraCharts.XYDiagram();
            DevExpress.XtraCharts.Series series1 = new DevExpress.XtraCharts.Series();
            DevExpress.XtraCharts.Series series2 = new DevExpress.XtraCharts.Series();
            this.expandablePanel1 = new Mozart.Studio.UIComponents.ExpandablePanel();
            this.shiftComboBoxEdit = new DevExpress.XtraEditors.ComboBoxEdit();
            this.dayShiftRangeLabel = new System.Windows.Forms.Label();
            this.dayShiftSpinEdit = new DevExpress.XtraEditors.SpinEdit();
            this.ProductLabel = new System.Windows.Forms.Label();
            this.prodIdCheckedComboBoxEdit = new DevExpress.XtraEditors.CheckedComboBoxEdit();
            this.ShopIdLabel = new System.Windows.Forms.Label();
            this.shopIdComboBoxEdit = new DevExpress.XtraEditors.ComboBoxEdit();
            this.queryBtn = new System.Windows.Forms.Button();
            this.fromDateEdit = new DevExpress.XtraEditors.DateEdit();
            this.DateLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.chartControl1 = new DevExpress.XtraCharts.ChartControl();
            this.dockManager1 = new DevExpress.XtraBars.Docking.DockManager(this.components);
            this.dockPanel1 = new DevExpress.XtraBars.Docking.DockPanel();
            this.dockPanel1_Container = new DevExpress.XtraBars.Docking.ControlContainer();
            this.pivotGridControl1 = new DevExpress.XtraPivotGrid.PivotGridControl();
            this.expandablePanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.shiftComboBoxEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dayShiftSpinEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.prodIdCheckedComboBoxEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.shopIdComboBoxEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fromDateEdit.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fromDateEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(xyDiagram1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(series1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(series2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dockManager1)).BeginInit();
            this.dockPanel1.SuspendLayout();
            this.dockPanel1_Container.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pivotGridControl1)).BeginInit();
            this.SuspendLayout();
            // 
            // expandablePanel1
            // 
            this.expandablePanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(236)))), ((int)(((byte)(239)))));
            this.expandablePanel1.Controls.Add(this.shiftComboBoxEdit);
            this.expandablePanel1.Controls.Add(this.dayShiftRangeLabel);
            this.expandablePanel1.Controls.Add(this.dayShiftSpinEdit);
            this.expandablePanel1.Controls.Add(this.ProductLabel);
            this.expandablePanel1.Controls.Add(this.prodIdCheckedComboBoxEdit);
            this.expandablePanel1.Controls.Add(this.ShopIdLabel);
            this.expandablePanel1.Controls.Add(this.shopIdComboBoxEdit);
            this.expandablePanel1.Controls.Add(this.queryBtn);
            this.expandablePanel1.Controls.Add(this.fromDateEdit);
            this.expandablePanel1.Controls.Add(this.DateLabel);
            this.expandablePanel1.Controls.Add(this.label3);
            this.expandablePanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.expandablePanel1.ForeColor = System.Drawing.Color.SteelBlue;
            this.expandablePanel1.Location = new System.Drawing.Point(0, 0);
            this.expandablePanel1.Name = "expandablePanel1";
            this.expandablePanel1.Size = new System.Drawing.Size(1143, 101);
            this.expandablePanel1.TabIndex = 3;
            this.expandablePanel1.Text = "Progress Rate";
            this.expandablePanel1.UseAnimation = true;
            // 
            // shiftComboBoxEdit
            // 
            this.shiftComboBoxEdit.Location = new System.Drawing.Point(349, 58);
            this.shiftComboBoxEdit.Name = "shiftComboBoxEdit";
            this.shiftComboBoxEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.shiftComboBoxEdit.Size = new System.Drawing.Size(40, 20);
            this.shiftComboBoxEdit.TabIndex = 90;
            // 
            // dayShiftRangeLabel
            // 
            this.dayShiftRangeLabel.AutoSize = true;
            this.dayShiftRangeLabel.Location = new System.Drawing.Point(450, 63);
            this.dayShiftRangeLabel.Name = "dayShiftRangeLabel";
            this.dayShiftRangeLabel.Size = new System.Drawing.Size(37, 14);
            this.dayShiftRangeLabel.TabIndex = 89;
            this.dayShiftRangeLabel.Text = "Shifts";
            // 
            // dayShiftSpinEdit
            // 
            this.dayShiftSpinEdit.EditValue = new decimal(new int[] {
            6,
            0,
            0,
            0});
            this.dayShiftSpinEdit.Location = new System.Drawing.Point(408, 58);
            this.dayShiftSpinEdit.MaximumSize = new System.Drawing.Size(40, 20);
            this.dayShiftSpinEdit.MinimumSize = new System.Drawing.Size(40, 20);
            this.dayShiftSpinEdit.Name = "dayShiftSpinEdit";
            this.dayShiftSpinEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dayShiftSpinEdit.Size = new System.Drawing.Size(40, 20);
            this.dayShiftSpinEdit.TabIndex = 88;
            // 
            // ProductLabel
            // 
            this.ProductLabel.AutoSize = true;
            this.ProductLabel.Location = new System.Drawing.Point(500, 63);
            this.ProductLabel.Name = "ProductLabel";
            this.ProductLabel.Size = new System.Drawing.Size(62, 14);
            this.ProductLabel.TabIndex = 86;
            this.ProductLabel.Text = "ProductID";
            // 
            // prodIdCheckedComboBoxEdit
            // 
            this.prodIdCheckedComboBoxEdit.Location = new System.Drawing.Point(564, 58);
            this.prodIdCheckedComboBoxEdit.Name = "prodIdCheckedComboBoxEdit";
            this.prodIdCheckedComboBoxEdit.Properties.AllowMultiSelect = true;
            this.prodIdCheckedComboBoxEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.prodIdCheckedComboBoxEdit.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.Standard;
            this.prodIdCheckedComboBoxEdit.Size = new System.Drawing.Size(163, 20);
            this.prodIdCheckedComboBoxEdit.TabIndex = 85;
            // 
            // ShopIdLabel
            // 
            this.ShopIdLabel.AutoSize = true;
            this.ShopIdLabel.Location = new System.Drawing.Point(34, 63);
            this.ShopIdLabel.Name = "ShopIdLabel";
            this.ShopIdLabel.Size = new System.Drawing.Size(47, 14);
            this.ShopIdLabel.TabIndex = 84;
            this.ShopIdLabel.Text = "ShopID";
            // 
            // shopIdComboBoxEdit
            // 
            this.shopIdComboBoxEdit.Location = new System.Drawing.Point(82, 58);
            this.shopIdComboBoxEdit.Name = "shopIdComboBoxEdit";
            this.shopIdComboBoxEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.shopIdComboBoxEdit.Size = new System.Drawing.Size(77, 20);
            this.shopIdComboBoxEdit.TabIndex = 83;
            // 
            // queryBtn
            // 
            this.queryBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.queryBtn.Location = new System.Drawing.Point(1047, 48);
            this.queryBtn.Name = "queryBtn";
            this.queryBtn.Size = new System.Drawing.Size(75, 42);
            this.queryBtn.TabIndex = 65;
            this.queryBtn.Text = "Query";
            this.queryBtn.UseVisualStyleBackColor = true;
            this.queryBtn.Click += new System.EventHandler(this.queryBtn_Click);
            // 
            // fromDateEdit
            // 
            this.fromDateEdit.EditValue = null;
            this.fromDateEdit.Location = new System.Drawing.Point(247, 58);
            this.fromDateEdit.MaximumSize = new System.Drawing.Size(200, 20);
            this.fromDateEdit.MinimumSize = new System.Drawing.Size(100, 20);
            this.fromDateEdit.Name = "fromDateEdit";
            this.fromDateEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.fromDateEdit.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.fromDateEdit.Properties.DisplayFormat.FormatString = "yyyy-MM-dd";
            this.fromDateEdit.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.fromDateEdit.Size = new System.Drawing.Size(100, 20);
            this.fromDateEdit.TabIndex = 75;
            // 
            // DateLabel
            // 
            this.DateLabel.AutoSize = true;
            this.DateLabel.Location = new System.Drawing.Point(174, 63);
            this.DateLabel.Name = "DateLabel";
            this.DateLabel.Size = new System.Drawing.Size(71, 14);
            this.DateLabel.TabIndex = 76;
            this.DateLabel.Text = "Date Range";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(391, 63);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(16, 14);
            this.label3.TabIndex = 74;
            this.label3.Text = "~";
            // 
            // chartControl1
            // 
            xyDiagram1.AxisX.VisibleInPanesSerializable = "-1";
            xyDiagram1.AxisY.VisibleInPanesSerializable = "-1";
            this.chartControl1.Diagram = xyDiagram1;
            this.chartControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartControl1.Legend.Name = "Default Legend";
            this.chartControl1.Location = new System.Drawing.Point(0, 101);
            this.chartControl1.Name = "chartControl1";
            series1.Name = "Series 1";
            series2.Name = "Series 2";
            this.chartControl1.SeriesSerializable = new DevExpress.XtraCharts.Series[] {
        series1,
        series2};
            this.chartControl1.Size = new System.Drawing.Size(1143, 224);
            this.chartControl1.SmallChartText.Text = "Increase the chart\'s size,\r\nto view its layout.\r\n    ";
            this.chartControl1.TabIndex = 4;
            // 
            // dockManager1
            // 
            this.dockManager1.Form = this;
            this.dockManager1.RootPanels.AddRange(new DevExpress.XtraBars.Docking.DockPanel[] {
            this.dockPanel1});
            this.dockManager1.TopZIndexControls.AddRange(new string[] {
            "DevExpress.XtraBars.BarDockControl",
            "DevExpress.XtraBars.StandaloneBarDockControl",
            "System.Windows.Forms.StatusBar",
            "System.Windows.Forms.MenuStrip",
            "System.Windows.Forms.StatusStrip",
            "DevExpress.XtraBars.Ribbon.RibbonStatusBar",
            "DevExpress.XtraBars.Ribbon.RibbonControl",
            "DevExpress.XtraBars.Navigation.OfficeNavigationBar",
            "DevExpress.XtraBars.Navigation.TileNavPane"});
            // 
            // dockPanel1
            // 
            this.dockPanel1.Controls.Add(this.dockPanel1_Container);
            this.dockPanel1.Dock = DevExpress.XtraBars.Docking.DockingStyle.Bottom;
            this.dockPanel1.ID = new System.Guid("7e387841-2e6b-43a9-86f3-e14787a431cc");
            this.dockPanel1.Location = new System.Drawing.Point(0, 325);
            this.dockPanel1.Name = "dockPanel1";
            this.dockPanel1.Options.ShowCloseButton = false;
            this.dockPanel1.OriginalSize = new System.Drawing.Size(200, 200);
            this.dockPanel1.Size = new System.Drawing.Size(1143, 200);
            this.dockPanel1.Text = "StepWip Data";
            // 
            // dockPanel1_Container
            // 
            this.dockPanel1_Container.Controls.Add(this.pivotGridControl1);
            this.dockPanel1_Container.Location = new System.Drawing.Point(4, 24);
            this.dockPanel1_Container.Name = "dockPanel1_Container";
            this.dockPanel1_Container.Size = new System.Drawing.Size(1135, 172);
            this.dockPanel1_Container.TabIndex = 0;
            // 
            // pivotGridControl1
            // 
            this.pivotGridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pivotGridControl1.Location = new System.Drawing.Point(0, 0);
            this.pivotGridControl1.Name = "pivotGridControl1";
            this.pivotGridControl1.Size = new System.Drawing.Size(1135, 172);
            this.pivotGridControl1.TabIndex = 0;
            // 
            // SimulationWipView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chartControl1);
            this.Controls.Add(this.expandablePanel1);
            this.Controls.Add(this.dockPanel1);
            this.Name = "SimulationWipView";
            this.Size = new System.Drawing.Size(1143, 525);
            this.expandablePanel1.ResumeLayout(false);
            this.expandablePanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.shiftComboBoxEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dayShiftSpinEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.prodIdCheckedComboBoxEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.shopIdComboBoxEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fromDateEdit.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fromDateEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(xyDiagram1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(series1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(series2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dockManager1)).EndInit();
            this.dockPanel1.ResumeLayout(false);
            this.dockPanel1_Container.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pivotGridControl1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Mozart.Studio.UIComponents.ExpandablePanel expandablePanel1;
        private DevExpress.XtraEditors.ComboBoxEdit shiftComboBoxEdit;
        private System.Windows.Forms.Label dayShiftRangeLabel;
        private DevExpress.XtraEditors.SpinEdit dayShiftSpinEdit;
        private System.Windows.Forms.Label ProductLabel;
        private DevExpress.XtraEditors.CheckedComboBoxEdit prodIdCheckedComboBoxEdit;
        private System.Windows.Forms.Label ShopIdLabel;
        private DevExpress.XtraEditors.ComboBoxEdit shopIdComboBoxEdit;
        private System.Windows.Forms.Button queryBtn;
        private DevExpress.XtraEditors.DateEdit fromDateEdit;
        private System.Windows.Forms.Label DateLabel;
        private System.Windows.Forms.Label label3;
        private DevExpress.XtraCharts.ChartControl chartControl1;
        private DevExpress.XtraBars.Docking.DockManager dockManager1;
        private DevExpress.XtraBars.Docking.DockPanel dockPanel1;
        private DevExpress.XtraBars.Docking.ControlContainer dockPanel1_Container;
        private DevExpress.XtraPivotGrid.PivotGridControl pivotGridControl1;
    }
}
