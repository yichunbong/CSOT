namespace CSOT.Lcd.UserInterface.Analysis
{
    partial class WorkOrderView
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
            this.expandablePanel1 = new Mozart.Studio.UIComponents.ExpandablePanel();
            this.button1 = new System.Windows.Forms.Button();
            this.eqpGroupsCheckedBox = new CSOT.Lcd.UserInterface.Utils.CheckedComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.areaLabel = new System.Windows.Forms.Label();
            this.areaChkBoxEdit = new DevExpress.XtraEditors.CheckedComboBoxEdit();
            this.shiftComboBoxEdit = new DevExpress.XtraEditors.ComboBoxEdit();
            this.dayShiftRangeLabel = new System.Windows.Forms.Label();
            this.dayShiftSpinEdit = new DevExpress.XtraEditors.SpinEdit();
            this.ShopIdLabel = new System.Windows.Forms.Label();
            this.shopIdComboBoxEdit = new DevExpress.XtraEditors.ComboBoxEdit();
            this.btnExcelExport = new System.Windows.Forms.Button();
            this.fromDateEdit = new DevExpress.XtraEditors.DateEdit();
            this.DateLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.spreadsheetControl1 = new DevExpress.XtraSpreadsheet.SpreadsheetControl();
            this.expandablePanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.areaChkBoxEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.shiftComboBoxEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dayShiftSpinEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.shopIdComboBoxEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fromDateEdit.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fromDateEdit.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // expandablePanel1
            // 
            this.expandablePanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(236)))), ((int)(((byte)(239)))));
            this.expandablePanel1.Controls.Add(this.button1);
            this.expandablePanel1.Controls.Add(this.eqpGroupsCheckedBox);
            this.expandablePanel1.Controls.Add(this.label1);
            this.expandablePanel1.Controls.Add(this.areaLabel);
            this.expandablePanel1.Controls.Add(this.areaChkBoxEdit);
            this.expandablePanel1.Controls.Add(this.shiftComboBoxEdit);
            this.expandablePanel1.Controls.Add(this.dayShiftRangeLabel);
            this.expandablePanel1.Controls.Add(this.dayShiftSpinEdit);
            this.expandablePanel1.Controls.Add(this.ShopIdLabel);
            this.expandablePanel1.Controls.Add(this.shopIdComboBoxEdit);
            this.expandablePanel1.Controls.Add(this.btnExcelExport);
            this.expandablePanel1.Controls.Add(this.fromDateEdit);
            this.expandablePanel1.Controls.Add(this.DateLabel);
            this.expandablePanel1.Controls.Add(this.label3);
            this.expandablePanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.expandablePanel1.ForeColor = System.Drawing.Color.SteelBlue;
            this.expandablePanel1.Location = new System.Drawing.Point(0, 0);
            this.expandablePanel1.Name = "expandablePanel1";
            this.expandablePanel1.Size = new System.Drawing.Size(1194, 101);
            this.expandablePanel1.TabIndex = 3;
            this.expandablePanel1.Text = "Dispatching Analysis";
            this.expandablePanel1.UseAnimation = true;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(1098, 48);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 42);
            this.button1.TabIndex = 99;
            this.button1.Text = "Query";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // eqpGroupsCheckedBox
            // 
            this.eqpGroupsCheckedBox.Location = new System.Drawing.Point(439, 58);
            this.eqpGroupsCheckedBox.Name = "eqpGroupsCheckedBox";
            this.eqpGroupsCheckedBox.Size = new System.Drawing.Size(150, 23);
            this.eqpGroupsCheckedBox.TabIndex = 98;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(375, 62);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 14);
            this.label1.TabIndex = 97;
            this.label1.Text = "EqpGroup";
            // 
            // areaLabel
            // 
            this.areaLabel.AutoSize = true;
            this.areaLabel.Location = new System.Drawing.Point(176, 62);
            this.areaLabel.Name = "areaLabel";
            this.areaLabel.Size = new System.Drawing.Size(32, 14);
            this.areaLabel.TabIndex = 95;
            this.areaLabel.Text = "Area";
            // 
            // areaChkBoxEdit
            // 
            this.areaChkBoxEdit.EditValue = "";
            this.areaChkBoxEdit.Location = new System.Drawing.Point(209, 58);
            this.areaChkBoxEdit.Name = "areaChkBoxEdit";
            this.areaChkBoxEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.areaChkBoxEdit.Size = new System.Drawing.Size(148, 20);
            this.areaChkBoxEdit.TabIndex = 94;
            this.areaChkBoxEdit.EditValueChanged += new System.EventHandler(this.areaChkBoxEdit_EditValueChanged);
            // 
            // shiftComboBoxEdit
            // 
            this.shiftComboBoxEdit.Location = new System.Drawing.Point(786, 58);
            this.shiftComboBoxEdit.Name = "shiftComboBoxEdit";
            this.shiftComboBoxEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.shiftComboBoxEdit.Size = new System.Drawing.Size(40, 20);
            this.shiftComboBoxEdit.TabIndex = 90;
            // 
            // dayShiftRangeLabel
            // 
            this.dayShiftRangeLabel.AutoSize = true;
            this.dayShiftRangeLabel.Location = new System.Drawing.Point(887, 62);
            this.dayShiftRangeLabel.Name = "dayShiftRangeLabel";
            this.dayShiftRangeLabel.Size = new System.Drawing.Size(32, 14);
            this.dayShiftRangeLabel.TabIndex = 89;
            this.dayShiftRangeLabel.Text = "Days";
            // 
            // dayShiftSpinEdit
            // 
            this.dayShiftSpinEdit.EditValue = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.dayShiftSpinEdit.Location = new System.Drawing.Point(845, 58);
            this.dayShiftSpinEdit.MaximumSize = new System.Drawing.Size(40, 20);
            this.dayShiftSpinEdit.MinimumSize = new System.Drawing.Size(40, 20);
            this.dayShiftSpinEdit.Name = "dayShiftSpinEdit";
            this.dayShiftSpinEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.dayShiftSpinEdit.Size = new System.Drawing.Size(40, 20);
            this.dayShiftSpinEdit.TabIndex = 88;
            // 
            // ShopIdLabel
            // 
            this.ShopIdLabel.AutoSize = true;
            this.ShopIdLabel.Location = new System.Drawing.Point(34, 62);
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
            this.shopIdComboBoxEdit.SelectedValueChanged += new System.EventHandler(this.shopIdComboBoxEdit_SelectedValueChanged);
            // 
            // btnExcelExport
            // 
            this.btnExcelExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExcelExport.Location = new System.Drawing.Point(1019, 48);
            this.btnExcelExport.Name = "btnExcelExport";
            this.btnExcelExport.Size = new System.Drawing.Size(75, 42);
            this.btnExcelExport.TabIndex = 65;
            this.btnExcelExport.Text = "Export\r\nExcel";
            this.btnExcelExport.UseVisualStyleBackColor = true;
            this.btnExcelExport.Click += new System.EventHandler(this.btnExcelExport_Click);
            // 
            // fromDateEdit
            // 
            this.fromDateEdit.EditValue = null;
            this.fromDateEdit.Location = new System.Drawing.Point(684, 58);
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
            // 
            // DateLabel
            // 
            this.DateLabel.AutoSize = true;
            this.DateLabel.Location = new System.Drawing.Point(611, 62);
            this.DateLabel.Name = "DateLabel";
            this.DateLabel.Size = new System.Drawing.Size(71, 14);
            this.DateLabel.TabIndex = 76;
            this.DateLabel.Text = "Date Range";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(828, 63);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(16, 14);
            this.label3.TabIndex = 74;
            this.label3.Text = "~";
            // 
            // spreadsheetControl1
            // 
            this.spreadsheetControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spreadsheetControl1.Location = new System.Drawing.Point(0, 101);
            this.spreadsheetControl1.Name = "spreadsheetControl1";
            this.spreadsheetControl1.Size = new System.Drawing.Size(1194, 612);
            this.spreadsheetControl1.TabIndex = 5;
            this.spreadsheetControl1.Text = "spreadsheetControl1";
            // 
            // WorkOrderView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.spreadsheetControl1);
            this.Controls.Add(this.expandablePanel1);
            this.Name = "WorkOrderView";
            this.Size = new System.Drawing.Size(1194, 713);
            this.expandablePanel1.ResumeLayout(false);
            this.expandablePanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.areaChkBoxEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.shiftComboBoxEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dayShiftSpinEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.shopIdComboBoxEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fromDateEdit.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fromDateEdit.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Mozart.Studio.UIComponents.ExpandablePanel expandablePanel1;
        private Utils.CheckedComboBox eqpGroupsCheckedBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label areaLabel;
        private DevExpress.XtraEditors.CheckedComboBoxEdit areaChkBoxEdit;
        private DevExpress.XtraEditors.ComboBoxEdit shiftComboBoxEdit;
        private System.Windows.Forms.Label dayShiftRangeLabel;
        private DevExpress.XtraEditors.SpinEdit dayShiftSpinEdit;
        private System.Windows.Forms.Label ShopIdLabel;
        private DevExpress.XtraEditors.ComboBoxEdit shopIdComboBoxEdit;
        private System.Windows.Forms.Button btnExcelExport;
        private DevExpress.XtraEditors.DateEdit fromDateEdit;
        private System.Windows.Forms.Label DateLabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private DevExpress.XtraSpreadsheet.SpreadsheetControl spreadsheetControl1;
    }
}
