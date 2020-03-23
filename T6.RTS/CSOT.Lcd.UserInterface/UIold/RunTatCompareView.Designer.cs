namespace CSOT.Lcd.UserInterface.Analysis
{
    partial class RunTatCompareView
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
            this.eqpIdTextBox = new System.Windows.Forms.TextBox();
            this.shiftComboBoxEdit = new DevExpress.XtraEditors.ComboBoxEdit();
            this.shiftRangeLabel = new System.Windows.Forms.Label();
            this.shiftSpinEdit = new DevExpress.XtraEditors.SpinEdit();
            this.ProductLabel = new System.Windows.Forms.Label();
            this.ShopIdLabel = new System.Windows.Forms.Label();
            this.shopIdComboBoxEdit = new DevExpress.XtraEditors.ComboBoxEdit();
            this.btnQuery = new System.Windows.Forms.Button();
            this.fromDateEdit = new DevExpress.XtraEditors.DateEdit();
            this.DateLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.expandablePanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.shiftComboBoxEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.shiftSpinEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.shopIdComboBoxEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fromDateEdit.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fromDateEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // expandablePanel1
            // 
            this.expandablePanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(236)))), ((int)(((byte)(239)))));
            this.expandablePanel1.Controls.Add(this.eqpIdTextBox);
            this.expandablePanel1.Controls.Add(this.shiftComboBoxEdit);
            this.expandablePanel1.Controls.Add(this.shiftRangeLabel);
            this.expandablePanel1.Controls.Add(this.shiftSpinEdit);
            this.expandablePanel1.Controls.Add(this.ProductLabel);
            this.expandablePanel1.Controls.Add(this.ShopIdLabel);
            this.expandablePanel1.Controls.Add(this.shopIdComboBoxEdit);
            this.expandablePanel1.Controls.Add(this.btnQuery);
            this.expandablePanel1.Controls.Add(this.fromDateEdit);
            this.expandablePanel1.Controls.Add(this.DateLabel);
            this.expandablePanel1.Controls.Add(this.label3);
            this.expandablePanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.expandablePanel1.ForeColor = System.Drawing.Color.SteelBlue;
            this.expandablePanel1.Location = new System.Drawing.Point(0, 0);
            this.expandablePanel1.Name = "expandablePanel1";
            this.expandablePanel1.Size = new System.Drawing.Size(1019, 87);
            this.expandablePanel1.TabIndex = 2;
            this.expandablePanel1.Text = "Run TAT Compare";
            this.expandablePanel1.UseAnimation = true;
            // 
            // eqpIdTextBox
            // 
            this.eqpIdTextBox.Location = new System.Drawing.Point(234, 49);
            this.eqpIdTextBox.Name = "eqpIdTextBox";
            this.eqpIdTextBox.Size = new System.Drawing.Size(115, 22);
            this.eqpIdTextBox.TabIndex = 91;
            // 
            // shiftComboBoxEdit
            // 
            this.shiftComboBoxEdit.Location = new System.Drawing.Point(553, 50);
            this.shiftComboBoxEdit.Name = "shiftComboBoxEdit";
            this.shiftComboBoxEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.shiftComboBoxEdit.Size = new System.Drawing.Size(40, 20);
            this.shiftComboBoxEdit.TabIndex = 90;
            // 
            // shiftRangeLabel
            // 
            this.shiftRangeLabel.AutoSize = true;
            this.shiftRangeLabel.Location = new System.Drawing.Point(654, 54);
            this.shiftRangeLabel.Name = "shiftRangeLabel";
            this.shiftRangeLabel.Size = new System.Drawing.Size(37, 14);
            this.shiftRangeLabel.TabIndex = 89;
            this.shiftRangeLabel.Text = "Shifts";
            // 
            // shiftSpinEdit
            // 
            this.shiftSpinEdit.EditValue = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.shiftSpinEdit.Location = new System.Drawing.Point(612, 50);
            this.shiftSpinEdit.MaximumSize = new System.Drawing.Size(40, 20);
            this.shiftSpinEdit.MinimumSize = new System.Drawing.Size(40, 20);
            this.shiftSpinEdit.Name = "shiftSpinEdit";
            this.shiftSpinEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.shiftSpinEdit.Size = new System.Drawing.Size(40, 20);
            this.shiftSpinEdit.TabIndex = 88;
            // 
            // ProductLabel
            // 
            this.ProductLabel.AutoSize = true;
            this.ProductLabel.Location = new System.Drawing.Point(188, 54);
            this.ProductLabel.Name = "ProductLabel";
            this.ProductLabel.Size = new System.Drawing.Size(40, 14);
            this.ProductLabel.TabIndex = 86;
            this.ProductLabel.Text = "EqpID";
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
            this.shopIdComboBoxEdit.EditValue = "ARRAY";
            this.shopIdComboBoxEdit.Enabled = false;
            this.shopIdComboBoxEdit.Location = new System.Drawing.Point(82, 50);
            this.shopIdComboBoxEdit.Name = "shopIdComboBoxEdit";
            this.shopIdComboBoxEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.shopIdComboBoxEdit.Size = new System.Drawing.Size(77, 20);
            this.shopIdComboBoxEdit.TabIndex = 83;
            // 
            // btnQuery
            // 
            this.btnQuery.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnQuery.Location = new System.Drawing.Point(923, 41);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(75, 36);
            this.btnQuery.TabIndex = 65;
            this.btnQuery.Text = "QUERY";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // fromDateEdit
            // 
            this.fromDateEdit.EditValue = null;
            this.fromDateEdit.Location = new System.Drawing.Point(451, 50);
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
            this.DateLabel.Location = new System.Drawing.Point(378, 54);
            this.DateLabel.Name = "DateLabel";
            this.DateLabel.Size = new System.Drawing.Size(71, 14);
            this.DateLabel.TabIndex = 76;
            this.DateLabel.Text = "Date Range";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(595, 54);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(16, 14);
            this.label3.TabIndex = 74;
            this.label3.Text = "~";
            // 
            // gridControl1
            // 
            this.gridControl1.Cursor = System.Windows.Forms.Cursors.Default;
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.Location = new System.Drawing.Point(0, 87);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(1019, 427);
            this.gridControl1.TabIndex = 3;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsMenu.ShowAddNewSummaryItem = DevExpress.Utils.DefaultBoolean.True;
            this.gridView1.OptionsSelection.MultiSelect = true;
            this.gridView1.OptionsView.ShowAutoFilterRow = true;
            this.gridView1.OptionsView.ShowFooter = true;
            // 
            // TimeInfoCompareView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridControl1);
            this.Controls.Add(this.expandablePanel1);
            this.Name = "TimeInfoCompareView";
            this.Size = new System.Drawing.Size(1019, 514);
            this.expandablePanel1.ResumeLayout(false);
            this.expandablePanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.shiftComboBoxEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.shiftSpinEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.shopIdComboBoxEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fromDateEdit.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fromDateEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Mozart.Studio.UIComponents.ExpandablePanel expandablePanel1;
        private DevExpress.XtraEditors.ComboBoxEdit shiftComboBoxEdit;
        private System.Windows.Forms.Label shiftRangeLabel;
        private DevExpress.XtraEditors.SpinEdit shiftSpinEdit;
        private System.Windows.Forms.Label ProductLabel;
        private System.Windows.Forms.Label ShopIdLabel;
        private DevExpress.XtraEditors.ComboBoxEdit shopIdComboBoxEdit;
        private System.Windows.Forms.Button btnQuery;
        private DevExpress.XtraEditors.DateEdit fromDateEdit;
        private System.Windows.Forms.Label DateLabel;
        private System.Windows.Forms.Label label3;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private System.Windows.Forms.TextBox eqpIdTextBox;
    }
}
