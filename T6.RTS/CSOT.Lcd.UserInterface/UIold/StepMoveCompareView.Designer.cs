namespace CSOT.Lcd.UserInterface.Analysis
{
    partial class StepMoveCompareView
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.wipCondSimTimeBtn = new System.Windows.Forms.RadioButton();
            this.wipCondInputBtn = new System.Windows.Forms.RadioButton();
            this.btnData = new System.Windows.Forms.Button();
            this.hourShiftRangeLabel = new System.Windows.Forms.Label();
            this.hourShiftSpinEdit = new DevExpress.XtraEditors.SpinEdit();
            this.dayShiftGroupBox = new System.Windows.Forms.GroupBox();
            this.shiftRadioButton = new System.Windows.Forms.RadioButton();
            this.hourRadioButton = new System.Windows.Forms.RadioButton();
            this.ProductLabel = new System.Windows.Forms.Label();
            this.prodIdCheckedComboBoxEdit = new DevExpress.XtraEditors.CheckedComboBoxEdit();
            this.ShopIdLabel = new System.Windows.Forms.Label();
            this.shopIdComboBoxEdit = new DevExpress.XtraEditors.ComboBoxEdit();
            this.btnChart = new System.Windows.Forms.Button();
            this.fromTimeEdit = new DevExpress.XtraEditors.DateEdit();
            this.DateLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.document1 = new DevExpress.XtraBars.Docking2010.Views.Tabbed.Document(this.components);
            this.dockManager1 = new DevExpress.XtraBars.Docking.DockManager(this.components);
            this.dockPanel2 = new DevExpress.XtraBars.Docking.DockPanel();
            this.dockPanel2_Container = new DevExpress.XtraBars.Docking.ControlContainer();
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.wipGridControl = new DevExpress.XtraGrid.GridControl();
            this.wipGridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.moveGridControl = new DevExpress.XtraGrid.GridControl();
            this.moveGridView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.chartControl1 = new DevExpress.XtraCharts.ChartControl();
            this.expandablePanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.hourShiftSpinEdit.Properties)).BeginInit();
            this.dayShiftGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.prodIdCheckedComboBoxEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.shopIdComboBoxEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fromTimeEdit.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fromTimeEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.document1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dockManager1)).BeginInit();
            this.dockPanel2.SuspendLayout();
            this.dockPanel2_Container.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.wipGridControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.wipGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.moveGridControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.moveGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(xyDiagram1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(series1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(series2)).BeginInit();
            this.SuspendLayout();
            // 
            // expandablePanel1
            // 
            this.expandablePanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(236)))), ((int)(((byte)(239)))));
            this.expandablePanel1.Controls.Add(this.groupBox1);
            this.expandablePanel1.Controls.Add(this.btnData);
            this.expandablePanel1.Controls.Add(this.hourShiftRangeLabel);
            this.expandablePanel1.Controls.Add(this.hourShiftSpinEdit);
            this.expandablePanel1.Controls.Add(this.dayShiftGroupBox);
            this.expandablePanel1.Controls.Add(this.ProductLabel);
            this.expandablePanel1.Controls.Add(this.prodIdCheckedComboBoxEdit);
            this.expandablePanel1.Controls.Add(this.ShopIdLabel);
            this.expandablePanel1.Controls.Add(this.shopIdComboBoxEdit);
            this.expandablePanel1.Controls.Add(this.btnChart);
            this.expandablePanel1.Controls.Add(this.fromTimeEdit);
            this.expandablePanel1.Controls.Add(this.DateLabel);
            this.expandablePanel1.Controls.Add(this.label3);
            this.expandablePanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.expandablePanel1.ForeColor = System.Drawing.Color.SteelBlue;
            this.expandablePanel1.Location = new System.Drawing.Point(0, 0);
            this.expandablePanel1.Name = "expandablePanel1";
            this.expandablePanel1.Size = new System.Drawing.Size(1265, 75);
            this.expandablePanel1.TabIndex = 1;
            this.expandablePanel1.Text = "Step Move Compare";
            this.expandablePanel1.UseAnimation = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.wipCondSimTimeBtn);
            this.groupBox1.Controls.Add(this.wipCondInputBtn);
            this.groupBox1.Location = new System.Drawing.Point(825, 29);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(135, 37);
            this.groupBox1.TabIndex = 92;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "WipCondition";
            // 
            // wipCondSimTimeBtn
            // 
            this.wipCondSimTimeBtn.AutoSize = true;
            this.wipCondSimTimeBtn.Checked = true;
            this.wipCondSimTimeBtn.Location = new System.Drawing.Point(5, 15);
            this.wipCondSimTimeBtn.Name = "wipCondSimTimeBtn";
            this.wipCondSimTimeBtn.Size = new System.Drawing.Size(71, 18);
            this.wipCondSimTimeBtn.TabIndex = 1;
            this.wipCondSimTimeBtn.TabStop = true;
            this.wipCondSimTimeBtn.Text = "SimTime";
            this.wipCondSimTimeBtn.UseVisualStyleBackColor = true;
            // 
            // wipCondInputBtn
            // 
            this.wipCondInputBtn.AutoSize = true;
            this.wipCondInputBtn.Location = new System.Drawing.Point(76, 15);
            this.wipCondInputBtn.Name = "wipCondInputBtn";
            this.wipCondInputBtn.Size = new System.Drawing.Size(55, 18);
            this.wipCondInputBtn.TabIndex = 0;
            this.wipCondInputBtn.Text = "Input";
            this.wipCondInputBtn.UseVisualStyleBackColor = true;
            // 
            // btnData
            // 
            this.btnData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnData.Location = new System.Drawing.Point(1088, 35);
            this.btnData.Name = "btnData";
            this.btnData.Size = new System.Drawing.Size(75, 31);
            this.btnData.TabIndex = 91;
            this.btnData.Text = "ShowData";
            this.btnData.UseVisualStyleBackColor = true;
            this.btnData.Click += new System.EventHandler(this.btnData_Click);
            // 
            // hourShiftRangeLabel
            // 
            this.hourShiftRangeLabel.AutoSize = true;
            this.hourShiftRangeLabel.Location = new System.Drawing.Point(727, 46);
            this.hourShiftRangeLabel.Name = "hourShiftRangeLabel";
            this.hourShiftRangeLabel.Size = new System.Drawing.Size(38, 14);
            this.hourShiftRangeLabel.TabIndex = 89;
            this.hourShiftRangeLabel.Text = "Hours";
            // 
            // hourShiftSpinEdit
            // 
            this.hourShiftSpinEdit.EditValue = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.hourShiftSpinEdit.Location = new System.Drawing.Point(685, 43);
            this.hourShiftSpinEdit.MaximumSize = new System.Drawing.Size(40, 20);
            this.hourShiftSpinEdit.MinimumSize = new System.Drawing.Size(40, 20);
            this.hourShiftSpinEdit.Name = "hourShiftSpinEdit";
            this.hourShiftSpinEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.hourShiftSpinEdit.Size = new System.Drawing.Size(40, 20);
            this.hourShiftSpinEdit.TabIndex = 88;
            // 
            // dayShiftGroupBox
            // 
            this.dayShiftGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dayShiftGroupBox.Controls.Add(this.shiftRadioButton);
            this.dayShiftGroupBox.Controls.Add(this.hourRadioButton);
            this.dayShiftGroupBox.Location = new System.Drawing.Point(965, 28);
            this.dayShiftGroupBox.Name = "dayShiftGroupBox";
            this.dayShiftGroupBox.Size = new System.Drawing.Size(115, 40);
            this.dayShiftGroupBox.TabIndex = 87;
            this.dayShiftGroupBox.TabStop = false;
            this.dayShiftGroupBox.Text = "TimeCondition";
            // 
            // shiftRadioButton
            // 
            this.shiftRadioButton.AutoSize = true;
            this.shiftRadioButton.Location = new System.Drawing.Point(61, 16);
            this.shiftRadioButton.Name = "shiftRadioButton";
            this.shiftRadioButton.Size = new System.Drawing.Size(50, 18);
            this.shiftRadioButton.TabIndex = 1;
            this.shiftRadioButton.TabStop = true;
            this.shiftRadioButton.Text = "Shift";
            this.shiftRadioButton.UseVisualStyleBackColor = true;
            // 
            // hourRadioButton
            // 
            this.hourRadioButton.AutoSize = true;
            this.hourRadioButton.Location = new System.Drawing.Point(10, 16);
            this.hourRadioButton.Name = "hourRadioButton";
            this.hourRadioButton.Size = new System.Drawing.Size(51, 18);
            this.hourRadioButton.TabIndex = 0;
            this.hourRadioButton.TabStop = true;
            this.hourRadioButton.Text = "Hour";
            this.hourRadioButton.UseVisualStyleBackColor = true;
            this.hourRadioButton.CheckedChanged += new System.EventHandler(this.dayRadioButton_CheckedChanged);
            // 
            // ProductLabel
            // 
            this.ProductLabel.AutoSize = true;
            this.ProductLabel.Location = new System.Drawing.Point(188, 46);
            this.ProductLabel.Name = "ProductLabel";
            this.ProductLabel.Size = new System.Drawing.Size(62, 14);
            this.ProductLabel.TabIndex = 86;
            this.ProductLabel.Text = "ProductID";
            // 
            // prodIdCheckedComboBoxEdit
            // 
            this.prodIdCheckedComboBoxEdit.Location = new System.Drawing.Point(252, 43);
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
            this.ShopIdLabel.Location = new System.Drawing.Point(34, 46);
            this.ShopIdLabel.Name = "ShopIdLabel";
            this.ShopIdLabel.Size = new System.Drawing.Size(47, 14);
            this.ShopIdLabel.TabIndex = 84;
            this.ShopIdLabel.Text = "ShopID";
            // 
            // shopIdComboBoxEdit
            // 
            this.shopIdComboBoxEdit.Location = new System.Drawing.Point(82, 43);
            this.shopIdComboBoxEdit.Name = "shopIdComboBoxEdit";
            this.shopIdComboBoxEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.shopIdComboBoxEdit.Size = new System.Drawing.Size(77, 20);
            this.shopIdComboBoxEdit.TabIndex = 83;
            // 
            // btnChart
            // 
            this.btnChart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChart.Location = new System.Drawing.Point(1169, 35);
            this.btnChart.Name = "btnChart";
            this.btnChart.Size = new System.Drawing.Size(75, 31);
            this.btnChart.TabIndex = 65;
            this.btnChart.Text = "ShowChart";
            this.btnChart.UseVisualStyleBackColor = true;
            this.btnChart.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // fromTimeEdit
            // 
            this.fromTimeEdit.EditValue = null;
            this.fromTimeEdit.Location = new System.Drawing.Point(515, 43);
            this.fromTimeEdit.MaximumSize = new System.Drawing.Size(200, 20);
            this.fromTimeEdit.MinimumSize = new System.Drawing.Size(100, 20);
            this.fromTimeEdit.Name = "fromTimeEdit";
            this.fromTimeEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.fromTimeEdit.Properties.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.fromTimeEdit.Properties.DisplayFormat.FormatString = "yyyy-MM-dd HH:mm:ss";
            this.fromTimeEdit.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            this.fromTimeEdit.Properties.Mask.EditMask = "yyyy-MM-dd HH:mm:ss";
            this.fromTimeEdit.Size = new System.Drawing.Size(151, 20);
            this.fromTimeEdit.TabIndex = 75;
            // 
            // DateLabel
            // 
            this.DateLabel.AutoSize = true;
            this.DateLabel.Location = new System.Drawing.Point(442, 46);
            this.DateLabel.Name = "DateLabel";
            this.DateLabel.Size = new System.Drawing.Size(71, 14);
            this.DateLabel.TabIndex = 76;
            this.DateLabel.Text = "Date Range";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(668, 46);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(16, 14);
            this.label3.TabIndex = 74;
            this.label3.Text = "~";
            // 
            // document1
            // 
            this.document1.Caption = "dockPanel1";
            this.document1.ControlName = "dockPanel1";
            this.document1.Properties.AllowClose = DevExpress.Utils.DefaultBoolean.True;
            this.document1.Properties.AllowFloat = DevExpress.Utils.DefaultBoolean.True;
            this.document1.Properties.AllowFloatOnDoubleClick = DevExpress.Utils.DefaultBoolean.True;
            // 
            // dockManager1
            // 
            this.dockManager1.Form = this;
            this.dockManager1.RootPanels.AddRange(new DevExpress.XtraBars.Docking.DockPanel[] {
            this.dockPanel2});
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
            // dockPanel2
            // 
            this.dockPanel2.Controls.Add(this.dockPanel2_Container);
            this.dockPanel2.Dock = DevExpress.XtraBars.Docking.DockingStyle.Bottom;
            this.dockPanel2.FloatVertical = true;
            this.dockPanel2.ID = new System.Guid("30bd896b-173f-498f-b496-92be01a4a360");
            this.dockPanel2.Location = new System.Drawing.Point(0, 272);
            this.dockPanel2.Name = "dockPanel2";
            this.dockPanel2.Options.ShowCloseButton = false;
            this.dockPanel2.OriginalSize = new System.Drawing.Size(200, 233);
            this.dockPanel2.Size = new System.Drawing.Size(1265, 233);
            this.dockPanel2.Text = "Move & Wip Data";
            // 
            // dockPanel2_Container
            // 
            this.dockPanel2_Container.Controls.Add(this.layoutControl1);
            this.dockPanel2_Container.Location = new System.Drawing.Point(4, 24);
            this.dockPanel2_Container.Name = "dockPanel2_Container";
            this.dockPanel2_Container.Size = new System.Drawing.Size(1257, 205);
            this.dockPanel2_Container.TabIndex = 0;
            // 
            // layoutControl1
            // 
            this.layoutControl1.Controls.Add(this.wipGridControl);
            this.layoutControl1.Controls.Add(this.moveGridControl);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(1257, 205);
            this.layoutControl1.TabIndex = 1;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // wipGridControl
            // 
            this.wipGridControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.wipGridControl.Location = new System.Drawing.Point(811, 2);
            this.wipGridControl.MainView = this.wipGridView;
            this.wipGridControl.Name = "wipGridControl";
            this.wipGridControl.Size = new System.Drawing.Size(444, 201);
            this.wipGridControl.TabIndex = 1;
            this.wipGridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.wipGridView});
            // 
            // wipGridView
            // 
            this.wipGridView.GridControl = this.wipGridControl;
            this.wipGridView.Name = "wipGridView";
            this.wipGridView.OptionsSelection.MultiSelect = true;
            this.wipGridView.OptionsView.ShowAutoFilterRow = true;
            this.wipGridView.OptionsView.ShowFooter = true;
            // 
            // moveGridControl
            // 
            this.moveGridControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.moveGridControl.Location = new System.Drawing.Point(2, 2);
            this.moveGridControl.MainView = this.moveGridView;
            this.moveGridControl.Name = "moveGridControl";
            this.moveGridControl.Size = new System.Drawing.Size(805, 201);
            this.moveGridControl.TabIndex = 0;
            this.moveGridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.moveGridView});
            // 
            // moveGridView
            // 
            this.moveGridView.GridControl = this.moveGridControl;
            this.moveGridView.Name = "moveGridView";
            this.moveGridView.OptionsSelection.MultiSelect = true;
            this.moveGridView.OptionsView.ShowAutoFilterRow = true;
            this.moveGridView.OptionsView.ShowFooter = true;
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.CustomizationFormText = "layoutControlGroup1";
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.layoutControlItem2});
            this.layoutControlGroup1.Name = "layoutControlGroup1";
            this.layoutControlGroup1.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 0, 0);
            this.layoutControlGroup1.Size = new System.Drawing.Size(1257, 205);
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.moveGridControl;
            this.layoutControlItem1.CustomizationFormText = "layoutControlItem1";
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(809, 205);
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextVisible = false;
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.wipGridControl;
            this.layoutControlItem2.CustomizationFormText = "layoutControlItem2";
            this.layoutControlItem2.Location = new System.Drawing.Point(809, 0);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Size = new System.Drawing.Size(448, 205);
            this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem2.TextVisible = false;
            // 
            // chartControl1
            // 
            xyDiagram1.AxisX.VisibleInPanesSerializable = "-1";
            xyDiagram1.AxisY.VisibleInPanesSerializable = "-1";
            this.chartControl1.Diagram = xyDiagram1;
            this.chartControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartControl1.Legend.Name = "Default Legend";
            this.chartControl1.Location = new System.Drawing.Point(0, 75);
            this.chartControl1.Name = "chartControl1";
            series1.Name = "Series 1";
            series2.Name = "Series 2";
            this.chartControl1.SeriesSerializable = new DevExpress.XtraCharts.Series[] {
        series1,
        series2};
            this.chartControl1.Size = new System.Drawing.Size(1265, 197);
            this.chartControl1.SmallChartText.Text = "Increase the chart\'s size,\r\nto view its layout.\r\n    ";
            this.chartControl1.TabIndex = 3;
            this.chartControl1.CustomDrawCrosshair += new DevExpress.XtraCharts.CustomDrawCrosshairEventHandler(this.chartControl1_CustomDrawCrosshair);
            this.chartControl1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.chartControl1_MouseClick);
            this.chartControl1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.chartControl1_MouseDoubleClick);
            // 
            // StepMoveCompareView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chartControl1);
            this.Controls.Add(this.expandablePanel1);
            this.Controls.Add(this.dockPanel2);
            this.Name = "StepMoveCompareView";
            this.Size = new System.Drawing.Size(1265, 505);
            this.expandablePanel1.ResumeLayout(false);
            this.expandablePanel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.hourShiftSpinEdit.Properties)).EndInit();
            this.dayShiftGroupBox.ResumeLayout(false);
            this.dayShiftGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.prodIdCheckedComboBoxEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.shopIdComboBoxEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fromTimeEdit.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fromTimeEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.document1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dockManager1)).EndInit();
            this.dockPanel2.ResumeLayout(false);
            this.dockPanel2_Container.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.wipGridControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.wipGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.moveGridControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.moveGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(xyDiagram1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(series1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(series2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartControl1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Mozart.Studio.UIComponents.ExpandablePanel expandablePanel1;
        private System.Windows.Forms.Button btnChart;
        private DevExpress.XtraEditors.DateEdit fromTimeEdit;
        private System.Windows.Forms.Label DateLabel;
        private System.Windows.Forms.Label label3;
        private DevExpress.XtraBars.Docking2010.Views.Tabbed.Document document1;
        private DevExpress.XtraBars.Docking.DockManager dockManager1;
        private DevExpress.XtraBars.Docking.DockPanel dockPanel2;
        private DevExpress.XtraBars.Docking.ControlContainer dockPanel2_Container;
        private DevExpress.XtraCharts.ChartControl chartControl1;
        private DevExpress.XtraGrid.GridControl moveGridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView moveGridView;
        private System.Windows.Forms.Label ShopIdLabel;
        private DevExpress.XtraEditors.ComboBoxEdit shopIdComboBoxEdit;
        private System.Windows.Forms.Label ProductLabel;
        private DevExpress.XtraEditors.CheckedComboBoxEdit prodIdCheckedComboBoxEdit;
        private System.Windows.Forms.GroupBox dayShiftGroupBox;
        private System.Windows.Forms.RadioButton shiftRadioButton;
        private System.Windows.Forms.RadioButton hourRadioButton;
        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraGrid.GridControl wipGridControl;
        private DevExpress.XtraGrid.Views.Grid.GridView wipGridView;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private System.Windows.Forms.Label hourShiftRangeLabel;
        private DevExpress.XtraEditors.SpinEdit hourShiftSpinEdit;
        private System.Windows.Forms.Button btnData;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton wipCondSimTimeBtn;
        private System.Windows.Forms.RadioButton wipCondInputBtn;
    }
}
