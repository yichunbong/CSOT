namespace CSOT.Lcd.UserInterface.Analysis
{
    partial class EqpArrProductAnalView
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
            this.label4 = new System.Windows.Forms.Label();
            this.checkedComboBoxEdit1 = new DevExpress.XtraEditors.CheckedComboBoxEdit();
            this.label3 = new System.Windows.Forms.Label();
            this.shopIdComboBox = new System.Windows.Forms.ComboBox();
            this.EqpIdBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ShowReworkStep = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnQuery = new System.Windows.Forms.Button();
            this.pivotGridControl1 = new DevExpress.XtraPivotGrid.PivotGridControl();
            this.label5 = new System.Windows.Forms.Label();
            this.showOnlyProductionChckBox = new System.Windows.Forms.CheckBox();
            this.expandablePanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.checkedComboBoxEdit1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pivotGridControl1)).BeginInit();
            this.SuspendLayout();
            // 
            // expandablePanel1
            // 
            this.expandablePanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(236)))), ((int)(((byte)(239)))));
            this.expandablePanel1.Controls.Add(this.label5);
            this.expandablePanel1.Controls.Add(this.showOnlyProductionChckBox);
            this.expandablePanel1.Controls.Add(this.label4);
            this.expandablePanel1.Controls.Add(this.checkedComboBoxEdit1);
            this.expandablePanel1.Controls.Add(this.label3);
            this.expandablePanel1.Controls.Add(this.shopIdComboBox);
            this.expandablePanel1.Controls.Add(this.EqpIdBox);
            this.expandablePanel1.Controls.Add(this.label1);
            this.expandablePanel1.Controls.Add(this.ShowReworkStep);
            this.expandablePanel1.Controls.Add(this.label2);
            this.expandablePanel1.Controls.Add(this.btnQuery);
            this.expandablePanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.expandablePanel1.ForeColor = System.Drawing.Color.SteelBlue;
            this.expandablePanel1.Location = new System.Drawing.Point(0, 0);
            this.expandablePanel1.Name = "expandablePanel1";
            this.expandablePanel1.Size = new System.Drawing.Size(1222, 87);
            this.expandablePanel1.TabIndex = 2;
            this.expandablePanel1.Text = "EqpArrange / Product";
            this.expandablePanel1.UseAnimation = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(227, 42);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 14);
            this.label4.TabIndex = 90;
            this.label4.Text = "AREA";
            // 
            // checkedComboBoxEdit1
            // 
            this.checkedComboBoxEdit1.Location = new System.Drawing.Point(269, 37);
            this.checkedComboBoxEdit1.Name = "checkedComboBoxEdit1";
            this.checkedComboBoxEdit1.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.checkedComboBoxEdit1.Size = new System.Drawing.Size(123, 20);
            this.checkedComboBoxEdit1.TabIndex = 89;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(40, 42);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 14);
            this.label3.TabIndex = 88;
            this.label3.Text = "SHOP";
            // 
            // shopIdComboBox
            // 
            this.shopIdComboBox.FormattingEnabled = true;
            this.shopIdComboBox.Location = new System.Drawing.Point(81, 37);
            this.shopIdComboBox.Name = "shopIdComboBox";
            this.shopIdComboBox.Size = new System.Drawing.Size(123, 22);
            this.shopIdComboBox.TabIndex = 87;
            this.shopIdComboBox.Tag = "1";
            // 
            // EqpIdBox
            // 
            this.EqpIdBox.Location = new System.Drawing.Point(466, 35);
            this.EqpIdBox.Name = "EqpIdBox";
            this.EqpIdBox.Size = new System.Drawing.Size(115, 22);
            this.EqpIdBox.TabIndex = 86;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1042, 38);
            this.label1.MaximumSize = new System.Drawing.Size(100, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 28);
            this.label1.TabIndex = 85;
            this.label1.Text = "Show ReworkStep";
            // 
            // ShowReworkStep
            // 
            this.ShowReworkStep.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ShowReworkStep.AutoSize = true;
            this.ShowReworkStep.Location = new System.Drawing.Point(1021, 47);
            this.ShowReworkStep.MaximumSize = new System.Drawing.Size(100, 0);
            this.ShowReworkStep.Name = "ShowReworkStep";
            this.ShowReworkStep.Size = new System.Drawing.Size(15, 14);
            this.ShowReworkStep.TabIndex = 75;
            this.ShowReworkStep.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(415, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 14);
            this.label2.TabIndex = 71;
            this.label2.Text = "EQP ID";
            // 
            // btnQuery
            // 
            this.btnQuery.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnQuery.Location = new System.Drawing.Point(1132, 37);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(75, 31);
            this.btnQuery.TabIndex = 64;
            this.btnQuery.Text = "QUERY";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // pivotGridControl1
            // 
            this.pivotGridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pivotGridControl1.Location = new System.Drawing.Point(0, 87);
            this.pivotGridControl1.Name = "pivotGridControl1";
            this.pivotGridControl1.OptionsView.RowTotalsLocation = DevExpress.XtraPivotGrid.PivotRowTotalsLocation.Near;
            this.pivotGridControl1.OptionsView.ShowFilterSeparatorBar = false;
            this.pivotGridControl1.Size = new System.Drawing.Size(1222, 509);
            this.pivotGridControl1.TabIndex = 51;
            this.pivotGridControl1.CellDoubleClick += new DevExpress.XtraPivotGrid.PivotCellEventHandler(this.pivotGridControl1_CellDoubleClick);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(941, 37);
            this.label5.MaximumSize = new System.Drawing.Size(100, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 28);
            this.label5.TabIndex = 92;
            this.label5.Text = "Show Only Production";
            // 
            // showOnlyProductionChckBox
            // 
            this.showOnlyProductionChckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.showOnlyProductionChckBox.AutoSize = true;
            this.showOnlyProductionChckBox.Checked = true;
            this.showOnlyProductionChckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showOnlyProductionChckBox.Location = new System.Drawing.Point(920, 46);
            this.showOnlyProductionChckBox.MaximumSize = new System.Drawing.Size(100, 0);
            this.showOnlyProductionChckBox.Name = "showOnlyProductionChckBox";
            this.showOnlyProductionChckBox.Size = new System.Drawing.Size(15, 14);
            this.showOnlyProductionChckBox.TabIndex = 91;
            this.showOnlyProductionChckBox.UseVisualStyleBackColor = true;
            // 
            // EqpArrProductAnalView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pivotGridControl1);
            this.Controls.Add(this.expandablePanel1);
            this.Name = "EqpArrProductAnalView";
            this.Size = new System.Drawing.Size(1222, 596);
            this.expandablePanel1.ResumeLayout(false);
            this.expandablePanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.checkedComboBoxEdit1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pivotGridControl1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Mozart.Studio.UIComponents.ExpandablePanel expandablePanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox ShowReworkStep;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnQuery;
        private DevExpress.XtraPivotGrid.PivotGridControl pivotGridControl1;
        private System.Windows.Forms.TextBox EqpIdBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox shopIdComboBox;
        private System.Windows.Forms.Label label4;
        private DevExpress.XtraEditors.CheckedComboBoxEdit checkedComboBoxEdit1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox showOnlyProductionChckBox;
    }
}
