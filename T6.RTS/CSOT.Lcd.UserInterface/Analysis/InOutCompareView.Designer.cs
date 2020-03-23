namespace CSOT.Lcd.UserInterface.Analysis
{
    partial class InOutCompareView
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
            this.groupControl3 = new DevExpress.XtraEditors.GroupControl();
            this.cfOutTxtEdit = new DevExpress.XtraEditors.TextEdit();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.cfInTxtEdit = new DevExpress.XtraEditors.TextEdit();
            this.groupControl2 = new DevExpress.XtraEditors.GroupControl();
            this.arrayOutTxtEdit = new DevExpress.XtraEditors.TextEdit();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.arrayInTxtEdit = new DevExpress.XtraEditors.TextEdit();
            this.label1 = new System.Windows.Forms.Label();
            this.inOutChkBoxEdit = new DevExpress.XtraEditors.CheckedComboBoxEdit();
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
            this.pivotGridControl1 = new DevExpress.XtraPivotGrid.PivotGridControl();
            this.chartControl1 = new DevExpress.XtraCharts.ChartControl();
            this.dockManager1 = new DevExpress.XtraBars.Docking.DockManager(this.components);
            this.dockPanel1 = new DevExpress.XtraBars.Docking.DockPanel();
            this.dockPanel1_Container = new DevExpress.XtraBars.Docking.ControlContainer();
            this.expandablePanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl3)).BeginInit();
            this.groupControl3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cfOutTxtEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cfInTxtEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).BeginInit();
            this.groupControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.arrayOutTxtEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.arrayInTxtEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inOutChkBoxEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.shiftComboBoxEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dayShiftSpinEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.prodIdCheckedComboBoxEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.shopIdComboBoxEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fromDateEdit.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fromDateEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pivotGridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(xyDiagram1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(series1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(series2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dockManager1)).BeginInit();
            this.dockPanel1.SuspendLayout();
            this.dockPanel1_Container.SuspendLayout();
            this.SuspendLayout();
            // 
            // expandablePanel1
            // 
            this.expandablePanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(236)))), ((int)(((byte)(239)))));
            this.expandablePanel1.Controls.Add(this.groupControl3);
            this.expandablePanel1.Controls.Add(this.groupControl2);
            this.expandablePanel1.Controls.Add(this.label1);
            this.expandablePanel1.Controls.Add(this.inOutChkBoxEdit);
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
            this.expandablePanel1.Size = new System.Drawing.Size(1417, 87);
            this.expandablePanel1.TabIndex = 2;
            this.expandablePanel1.Text = "Step Move Compare";
            this.expandablePanel1.UseAnimation = true;
            // 
            // groupControl3
            // 
            this.groupControl3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupControl3.AppearanceCaption.ForeColor = System.Drawing.Color.SteelBlue;
            this.groupControl3.AppearanceCaption.Options.UseForeColor = true;
            this.groupControl3.AppearanceCaption.Options.UseTextOptions = true;
            this.groupControl3.AppearanceCaption.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.groupControl3.Controls.Add(this.cfOutTxtEdit);
            this.groupControl3.Controls.Add(this.label6);
            this.groupControl3.Controls.Add(this.label7);
            this.groupControl3.Controls.Add(this.cfInTxtEdit);
            this.groupControl3.Location = new System.Drawing.Point(897, 27);
            this.groupControl3.Name = "groupControl3";
            this.groupControl3.Size = new System.Drawing.Size(205, 51);
            this.groupControl3.TabIndex = 102;
            this.groupControl3.Text = "CF";
            this.groupControl3.Visible = false;
            // 
            // cfOutTxtEdit
            // 
            this.cfOutTxtEdit.Location = new System.Drawing.Point(137, 26);
            this.cfOutTxtEdit.Name = "cfOutTxtEdit";
            this.cfOutTxtEdit.Size = new System.Drawing.Size(62, 20);
            this.cfOutTxtEdit.TabIndex = 107;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.Color.SteelBlue;
            this.label6.Location = new System.Drawing.Point(96, 29);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 14);
            this.label6.TabIndex = 106;
            this.label6.Text = "OUT :";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.ForeColor = System.Drawing.Color.SteelBlue;
            this.label7.Location = new System.Drawing.Point(3, 29);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(27, 14);
            this.label7.TabIndex = 105;
            this.label7.Text = "IN :";
            // 
            // cfInTxtEdit
            // 
            this.cfInTxtEdit.Location = new System.Drawing.Point(30, 26);
            this.cfInTxtEdit.Name = "cfInTxtEdit";
            this.cfInTxtEdit.Size = new System.Drawing.Size(62, 20);
            this.cfInTxtEdit.TabIndex = 103;
            // 
            // groupControl2
            // 
            this.groupControl2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupControl2.AppearanceCaption.ForeColor = System.Drawing.Color.SteelBlue;
            this.groupControl2.AppearanceCaption.Options.UseForeColor = true;
            this.groupControl2.AppearanceCaption.Options.UseTextOptions = true;
            this.groupControl2.AppearanceCaption.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.groupControl2.Controls.Add(this.arrayOutTxtEdit);
            this.groupControl2.Controls.Add(this.label5);
            this.groupControl2.Controls.Add(this.label4);
            this.groupControl2.Controls.Add(this.arrayInTxtEdit);
            this.groupControl2.Location = new System.Drawing.Point(1109, 27);
            this.groupControl2.Name = "groupControl2";
            this.groupControl2.Size = new System.Drawing.Size(205, 51);
            this.groupControl2.TabIndex = 102;
            this.groupControl2.Text = "ARRAY";
            // 
            // arrayOutTxtEdit
            // 
            this.arrayOutTxtEdit.Location = new System.Drawing.Point(137, 26);
            this.arrayOutTxtEdit.Name = "arrayOutTxtEdit";
            this.arrayOutTxtEdit.Size = new System.Drawing.Size(62, 20);
            this.arrayOutTxtEdit.TabIndex = 101;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.SteelBlue;
            this.label5.Location = new System.Drawing.Point(96, 29);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(40, 14);
            this.label5.TabIndex = 100;
            this.label5.Text = "OUT :";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.SteelBlue;
            this.label4.Location = new System.Drawing.Point(3, 29);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(27, 14);
            this.label4.TabIndex = 99;
            this.label4.Text = "IN :";
            // 
            // arrayInTxtEdit
            // 
            this.arrayInTxtEdit.Location = new System.Drawing.Point(32, 26);
            this.arrayInTxtEdit.Name = "arrayInTxtEdit";
            this.arrayInTxtEdit.Size = new System.Drawing.Size(62, 20);
            this.arrayInTxtEdit.TabIndex = 98;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(410, 54);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 14);
            this.label1.TabIndex = 92;
            this.label1.Text = "IN/OUT";
            // 
            // inOutChkBoxEdit
            // 
            this.inOutChkBoxEdit.Location = new System.Drawing.Point(462, 50);
            this.inOutChkBoxEdit.Name = "inOutChkBoxEdit";
            this.inOutChkBoxEdit.Properties.AllowMultiSelect = true;
            this.inOutChkBoxEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.inOutChkBoxEdit.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.Standard;
            this.inOutChkBoxEdit.Size = new System.Drawing.Size(77, 20);
            this.inOutChkBoxEdit.TabIndex = 91;
            // 
            // shiftComboBoxEdit
            // 
            this.shiftComboBoxEdit.Location = new System.Drawing.Point(725, 50);
            this.shiftComboBoxEdit.Name = "shiftComboBoxEdit";
            this.shiftComboBoxEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.shiftComboBoxEdit.Size = new System.Drawing.Size(40, 20);
            this.shiftComboBoxEdit.TabIndex = 90;
            // 
            // dayShiftRangeLabel
            // 
            this.dayShiftRangeLabel.AutoSize = true;
            this.dayShiftRangeLabel.Location = new System.Drawing.Point(826, 54);
            this.dayShiftRangeLabel.Name = "dayShiftRangeLabel";
            this.dayShiftRangeLabel.Size = new System.Drawing.Size(32, 14);
            this.dayShiftRangeLabel.TabIndex = 89;
            this.dayShiftRangeLabel.Text = "Days";
            // 
            // dayShiftSpinEdit
            // 
            this.dayShiftSpinEdit.EditValue = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.dayShiftSpinEdit.Location = new System.Drawing.Point(784, 50);
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
            this.ProductLabel.Location = new System.Drawing.Point(172, 54);
            this.ProductLabel.Name = "ProductLabel";
            this.ProductLabel.Size = new System.Drawing.Size(62, 14);
            this.ProductLabel.TabIndex = 86;
            this.ProductLabel.Text = "ProductID";
            // 
            // prodIdCheckedComboBoxEdit
            // 
            this.prodIdCheckedComboBoxEdit.Location = new System.Drawing.Point(236, 50);
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
            // 
            // queryBtn
            // 
            this.queryBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.queryBtn.Location = new System.Drawing.Point(1322, 39);
            this.queryBtn.Name = "queryBtn";
            this.queryBtn.Size = new System.Drawing.Size(75, 36);
            this.queryBtn.TabIndex = 65;
            this.queryBtn.Text = "Query";
            this.queryBtn.UseVisualStyleBackColor = true;
            this.queryBtn.Click += new System.EventHandler(this.queryBtn_Click);
            // 
            // fromDateEdit
            // 
            this.fromDateEdit.EditValue = null;
            this.fromDateEdit.Location = new System.Drawing.Point(623, 50);
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
            this.DateLabel.Location = new System.Drawing.Point(550, 54);
            this.DateLabel.Name = "DateLabel";
            this.DateLabel.Size = new System.Drawing.Size(71, 14);
            this.DateLabel.TabIndex = 76;
            this.DateLabel.Text = "Date Range";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(767, 54);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(16, 14);
            this.label3.TabIndex = 74;
            this.label3.Text = "~";
            // 
            // pivotGridControl1
            // 
            this.pivotGridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pivotGridControl1.Location = new System.Drawing.Point(0, 87);
            this.pivotGridControl1.Name = "pivotGridControl1";
            this.pivotGridControl1.Size = new System.Drawing.Size(1417, 222);
            this.pivotGridControl1.TabIndex = 3;
            // 
            // chartControl1
            // 
            xyDiagram1.AxisX.VisibleInPanesSerializable = "-1";
            xyDiagram1.AxisY.VisibleInPanesSerializable = "-1";
            this.chartControl1.Diagram = xyDiagram1;
            this.chartControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartControl1.Legend.Name = "Default Legend";
            this.chartControl1.Location = new System.Drawing.Point(0, 0);
            this.chartControl1.Name = "chartControl1";
            series1.Name = "Series 1";
            series2.Name = "Series 2";
            this.chartControl1.SeriesSerializable = new DevExpress.XtraCharts.Series[] {
        series1,
        series2};
            this.chartControl1.Size = new System.Drawing.Size(1409, 225);
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
            this.dockPanel1.ID = new System.Guid("5ea22e04-34fc-4a50-8a4c-16ad5609f336");
            this.dockPanel1.Location = new System.Drawing.Point(0, 309);
            this.dockPanel1.Name = "dockPanel1";
            this.dockPanel1.Options.ShowCloseButton = false;
            this.dockPanel1.OriginalSize = new System.Drawing.Size(200, 253);
            this.dockPanel1.Size = new System.Drawing.Size(1417, 253);
            this.dockPanel1.Text = "Chart";
            // 
            // dockPanel1_Container
            // 
            this.dockPanel1_Container.Controls.Add(this.chartControl1);
            this.dockPanel1_Container.Location = new System.Drawing.Point(4, 24);
            this.dockPanel1_Container.Name = "dockPanel1_Container";
            this.dockPanel1_Container.Size = new System.Drawing.Size(1409, 225);
            this.dockPanel1_Container.TabIndex = 0;
            // 
            // InOutCompareView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pivotGridControl1);
            this.Controls.Add(this.expandablePanel1);
            this.Controls.Add(this.dockPanel1);
            this.Name = "InOutCompareView";
            this.Size = new System.Drawing.Size(1417, 562);
            this.expandablePanel1.ResumeLayout(false);
            this.expandablePanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl3)).EndInit();
            this.groupControl3.ResumeLayout(false);
            this.groupControl3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cfOutTxtEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cfInTxtEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).EndInit();
            this.groupControl2.ResumeLayout(false);
            this.groupControl2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.arrayOutTxtEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.arrayInTxtEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inOutChkBoxEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.shiftComboBoxEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dayShiftSpinEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.prodIdCheckedComboBoxEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.shopIdComboBoxEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fromDateEdit.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fromDateEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pivotGridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(xyDiagram1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(series1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(series2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dockManager1)).EndInit();
            this.dockPanel1.ResumeLayout(false);
            this.dockPanel1_Container.ResumeLayout(false);
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
        private System.Windows.Forms.Label label1;
        private DevExpress.XtraEditors.CheckedComboBoxEdit inOutChkBoxEdit;
        private DevExpress.XtraPivotGrid.PivotGridControl pivotGridControl1;
        private DevExpress.XtraCharts.ChartControl chartControl1;
        private DevExpress.XtraBars.Docking.DockManager dockManager1;
        private DevExpress.XtraBars.Docking.DockPanel dockPanel1;
        private DevExpress.XtraBars.Docking.ControlContainer dockPanel1_Container;
        private DevExpress.XtraEditors.GroupControl groupControl3;
        private DevExpress.XtraEditors.TextEdit cfOutTxtEdit;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private DevExpress.XtraEditors.TextEdit cfInTxtEdit;
        private DevExpress.XtraEditors.GroupControl groupControl2;
        private DevExpress.XtraEditors.TextEdit arrayOutTxtEdit;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private DevExpress.XtraEditors.TextEdit arrayInTxtEdit;
    }
}
