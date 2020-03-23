namespace CSOT.Lcd.UserInterface.Analysis
{
    partial class ProgressRateView
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
            this.popUpDataBtn = new System.Windows.Forms.Button();
            this.sectorSizeSpinEdit = new DevExpress.XtraEditors.SpinEdit();
            this.label1 = new System.Windows.Forms.Label();
            this.shiftComboBoxEdit = new DevExpress.XtraEditors.ComboBoxEdit();
            this.dayShiftRangeLabel = new System.Windows.Forms.Label();
            this.dayShiftSpinEdit = new DevExpress.XtraEditors.SpinEdit();
            this.ProductLabel = new System.Windows.Forms.Label();
            this.prodIdCheckedComboBoxEdit = new DevExpress.XtraEditors.CheckedComboBoxEdit();
            this.ShopIdLabel = new System.Windows.Forms.Label();
            this.shopIdComboBoxEdit = new DevExpress.XtraEditors.ComboBoxEdit();
            this.showChartBtn = new System.Windows.Forms.Button();
            this.fromDateEdit = new DevExpress.XtraEditors.DateEdit();
            this.DateLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.chartControl1 = new DevExpress.XtraCharts.ChartControl();
            this.dockManager1 = new DevExpress.XtraBars.Docking.DockManager(this.components);
            this.expandablePanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sectorSizeSpinEdit.Properties)).BeginInit();
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
            this.SuspendLayout();
            // 
            // expandablePanel1
            // 
            this.expandablePanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(236)))), ((int)(((byte)(239)))));
            this.expandablePanel1.Controls.Add(this.popUpDataBtn);
            this.expandablePanel1.Controls.Add(this.sectorSizeSpinEdit);
            this.expandablePanel1.Controls.Add(this.label1);
            this.expandablePanel1.Controls.Add(this.shiftComboBoxEdit);
            this.expandablePanel1.Controls.Add(this.dayShiftRangeLabel);
            this.expandablePanel1.Controls.Add(this.dayShiftSpinEdit);
            this.expandablePanel1.Controls.Add(this.ProductLabel);
            this.expandablePanel1.Controls.Add(this.prodIdCheckedComboBoxEdit);
            this.expandablePanel1.Controls.Add(this.ShopIdLabel);
            this.expandablePanel1.Controls.Add(this.shopIdComboBoxEdit);
            this.expandablePanel1.Controls.Add(this.showChartBtn);
            this.expandablePanel1.Controls.Add(this.fromDateEdit);
            this.expandablePanel1.Controls.Add(this.DateLabel);
            this.expandablePanel1.Controls.Add(this.label3);
            this.expandablePanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.expandablePanel1.ForeColor = System.Drawing.Color.SteelBlue;
            this.expandablePanel1.Location = new System.Drawing.Point(0, 0);
            this.expandablePanel1.Name = "expandablePanel1";
            this.expandablePanel1.Size = new System.Drawing.Size(1093, 87);
            this.expandablePanel1.TabIndex = 2;
            this.expandablePanel1.Text = "Progress Rate";
            this.expandablePanel1.UseAnimation = true;
            // 
            // popUpDataBtn
            // 
            this.popUpDataBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.popUpDataBtn.Location = new System.Drawing.Point(916, 41);
            this.popUpDataBtn.Name = "popUpDataBtn";
            this.popUpDataBtn.Size = new System.Drawing.Size(75, 36);
            this.popUpDataBtn.TabIndex = 94;
            this.popUpDataBtn.Text = "PopUp\r\nData";
            this.popUpDataBtn.UseVisualStyleBackColor = true;
            this.popUpDataBtn.Click += new System.EventHandler(this.popUpDataBtn_Click);
            // 
            // sectorSizeSpinEdit
            // 
            this.sectorSizeSpinEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sectorSizeSpinEdit.EditValue = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.sectorSizeSpinEdit.Location = new System.Drawing.Point(864, 50);
            this.sectorSizeSpinEdit.MaximumSize = new System.Drawing.Size(45, 20);
            this.sectorSizeSpinEdit.MinimumSize = new System.Drawing.Size(45, 20);
            this.sectorSizeSpinEdit.Name = "sectorSizeSpinEdit";
            this.sectorSizeSpinEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.sectorSizeSpinEdit.Size = new System.Drawing.Size(45, 20);
            this.sectorSizeSpinEdit.TabIndex = 93;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(794, 52);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 14);
            this.label1.TabIndex = 92;
            this.label1.Text = "Sector Size";
            // 
            // shiftComboBoxEdit
            // 
            this.shiftComboBoxEdit.Location = new System.Drawing.Point(349, 50);
            this.shiftComboBoxEdit.Name = "shiftComboBoxEdit";
            this.shiftComboBoxEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.shiftComboBoxEdit.Size = new System.Drawing.Size(40, 20);
            this.shiftComboBoxEdit.TabIndex = 90;
            this.shiftComboBoxEdit.EditValueChanged += new System.EventHandler(this.shiftComboBoxEdit_EditValueChanged);
            // 
            // dayShiftRangeLabel
            // 
            this.dayShiftRangeLabel.AutoSize = true;
            this.dayShiftRangeLabel.Location = new System.Drawing.Point(450, 54);
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
            this.dayShiftSpinEdit.Location = new System.Drawing.Point(408, 50);
            this.dayShiftSpinEdit.MaximumSize = new System.Drawing.Size(40, 20);
            this.dayShiftSpinEdit.MinimumSize = new System.Drawing.Size(40, 20);
            this.dayShiftSpinEdit.Name = "dayShiftSpinEdit";
            this.dayShiftSpinEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dayShiftSpinEdit.Size = new System.Drawing.Size(40, 20);
            this.dayShiftSpinEdit.TabIndex = 88;
            this.dayShiftSpinEdit.EditValueChanged += new System.EventHandler(this.dayShiftSpinEdit_EditValueChanged);
            // 
            // ProductLabel
            // 
            this.ProductLabel.AutoSize = true;
            this.ProductLabel.Location = new System.Drawing.Point(500, 54);
            this.ProductLabel.Name = "ProductLabel";
            this.ProductLabel.Size = new System.Drawing.Size(62, 14);
            this.ProductLabel.TabIndex = 86;
            this.ProductLabel.Text = "ProductID";
            // 
            // prodIdCheckedComboBoxEdit
            // 
            this.prodIdCheckedComboBoxEdit.Location = new System.Drawing.Point(564, 50);
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
            this.ShopIdLabel.Location = new System.Drawing.Point(34, 54);
            this.ShopIdLabel.Name = "ShopIdLabel";
            this.ShopIdLabel.Size = new System.Drawing.Size(47, 14);
            this.ShopIdLabel.TabIndex = 84;
            this.ShopIdLabel.Text = "ShopID";
            // 
            // shopIdComboBoxEdit
            // 
            this.shopIdComboBoxEdit.Location = new System.Drawing.Point(82, 50);
            this.shopIdComboBoxEdit.Name = "shopIdComboBoxEdit";
            this.shopIdComboBoxEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.shopIdComboBoxEdit.Size = new System.Drawing.Size(77, 20);
            this.shopIdComboBoxEdit.TabIndex = 83;
            this.shopIdComboBoxEdit.EditValueChanged += new System.EventHandler(this.shopIdComboBoxEdit_EditValueChanged);
            // 
            // showChartBtn
            // 
            this.showChartBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.showChartBtn.Location = new System.Drawing.Point(997, 41);
            this.showChartBtn.Name = "showChartBtn";
            this.showChartBtn.Size = new System.Drawing.Size(75, 36);
            this.showChartBtn.TabIndex = 65;
            this.showChartBtn.Text = "Show\r\nChart";
            this.showChartBtn.UseVisualStyleBackColor = true;
            this.showChartBtn.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // fromDateEdit
            // 
            this.fromDateEdit.EditValue = null;
            this.fromDateEdit.Location = new System.Drawing.Point(247, 50);
            this.fromDateEdit.MaximumSize = new System.Drawing.Size(200, 20);
            this.fromDateEdit.MinimumSize = new System.Drawing.Size(100, 20);
            this.fromDateEdit.Name = "fromDateEdit";
            this.fromDateEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.fromDateEdit.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.fromDateEdit.Properties.DisplayFormat.FormatString = "yyyy-MM-dd";
            this.fromDateEdit.Size = new System.Drawing.Size(100, 20);
            this.fromDateEdit.TabIndex = 75;
            this.fromDateEdit.EditValueChanged += new System.EventHandler(this.fromDateEdit_EditValueChanged);
            // 
            // DateLabel
            // 
            this.DateLabel.AutoSize = true;
            this.DateLabel.Location = new System.Drawing.Point(174, 54);
            this.DateLabel.Name = "DateLabel";
            this.DateLabel.Size = new System.Drawing.Size(71, 14);
            this.DateLabel.TabIndex = 76;
            this.DateLabel.Text = "Date Range";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(391, 54);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(16, 14);
            this.label3.TabIndex = 74;
            this.label3.Text = "~";
            // 
            // chartControl1
            // 
            xyDiagram1.AxisX.Title.Text = "Axis of arguments";
            xyDiagram1.AxisX.VisibleInPanesSerializable = "-1";
            xyDiagram1.AxisY.Title.Text = "Axis of values";
            xyDiagram1.AxisY.VisibleInPanesSerializable = "-1";
            this.chartControl1.Diagram = xyDiagram1;
            this.chartControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartControl1.EmptyChartText.Text = "";
            this.chartControl1.Location = new System.Drawing.Point(0, 87);
            this.chartControl1.Name = "chartControl1";
            series1.Name = "Series 1";
            series2.Name = "Series 2";
            this.chartControl1.SeriesSerializable = new DevExpress.XtraCharts.Series[] {
        series1,
        series2};
            this.chartControl1.Size = new System.Drawing.Size(1093, 348);
            this.chartControl1.SmallChartText.Text = "Increase the chart\'s size,\r\nto view its layout.\r\n    ";
            this.chartControl1.TabIndex = 3;
            // 
            // dockManager1
            // 
            this.dockManager1.Form = this;
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
            // ProgressRateView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chartControl1);
            this.Controls.Add(this.expandablePanel1);
            this.Name = "ProgressRateView";
            this.Size = new System.Drawing.Size(1093, 435);
            this.expandablePanel1.ResumeLayout(false);
            this.expandablePanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sectorSizeSpinEdit.Properties)).EndInit();
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
        private System.Windows.Forms.Button showChartBtn;
        private DevExpress.XtraEditors.DateEdit fromDateEdit;
        private System.Windows.Forms.Label DateLabel;
        private System.Windows.Forms.Label label3;
        private DevExpress.XtraCharts.ChartControl chartControl1;
        private System.Windows.Forms.Label label1;
        private DevExpress.XtraEditors.SpinEdit sectorSizeSpinEdit;
        private DevExpress.XtraBars.Docking.DockManager dockManager1;
        private System.Windows.Forms.Button popUpDataBtn;
    }
}
