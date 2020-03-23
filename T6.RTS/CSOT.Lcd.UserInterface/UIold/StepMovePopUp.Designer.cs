namespace CSOT.Lcd.UserInterface.Analysis
{
    partial class StepMovePopUp
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.expandablePanel1 = new Mozart.Studio.UIComponents.ExpandablePanel();
            this.btnExportExel = new System.Windows.Forms.Button();
            this.moveOver0CheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.wipCondSimTimeBtn = new System.Windows.Forms.RadioButton();
            this.wipCondInputBtn = new System.Windows.Forms.RadioButton();
            this.areaLabel = new System.Windows.Forms.Label();
            this.areaChkBoxEdit = new DevExpress.XtraEditors.CheckedComboBoxEdit();
            this.mainOnlyCheckBox = new System.Windows.Forms.CheckBox();
            this.hourShiftRangeLabel = new System.Windows.Forms.Label();
            this.hourShiftSpinEdit = new DevExpress.XtraEditors.SpinEdit();
            this.hourShiftGroupBox = new System.Windows.Forms.GroupBox();
            this.shiftRadioButton = new System.Windows.Forms.RadioButton();
            this.hourRadioButton = new System.Windows.Forms.RadioButton();
            this.ShopIdLabel = new System.Windows.Forms.Label();
            this.shopIdComboBoxEdit = new DevExpress.XtraEditors.ComboBoxEdit();
            this.btnQuery = new System.Windows.Forms.Button();
            this.fromTimeEdit = new DevExpress.XtraEditors.DateEdit();
            this.DateLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.pivotGridControl1 = new DevExpress.XtraPivotGrid.PivotGridControl();
            this.expandablePanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.areaChkBoxEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.hourShiftSpinEdit.Properties)).BeginInit();
            this.hourShiftGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.shopIdComboBoxEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fromTimeEdit.Properties.CalendarTimeProperties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fromTimeEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pivotGridControl1)).BeginInit();
            this.SuspendLayout();
            // 
            // expandablePanel1
            // 
            this.expandablePanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(236)))), ((int)(((byte)(239)))));
            this.expandablePanel1.Controls.Add(this.btnExportExel);
            this.expandablePanel1.Controls.Add(this.moveOver0CheckBox);
            this.expandablePanel1.Controls.Add(this.groupBox1);
            this.expandablePanel1.Controls.Add(this.areaLabel);
            this.expandablePanel1.Controls.Add(this.areaChkBoxEdit);
            this.expandablePanel1.Controls.Add(this.mainOnlyCheckBox);
            this.expandablePanel1.Controls.Add(this.hourShiftRangeLabel);
            this.expandablePanel1.Controls.Add(this.hourShiftSpinEdit);
            this.expandablePanel1.Controls.Add(this.hourShiftGroupBox);
            this.expandablePanel1.Controls.Add(this.ShopIdLabel);
            this.expandablePanel1.Controls.Add(this.shopIdComboBoxEdit);
            this.expandablePanel1.Controls.Add(this.btnQuery);
            this.expandablePanel1.Controls.Add(this.fromTimeEdit);
            this.expandablePanel1.Controls.Add(this.DateLabel);
            this.expandablePanel1.Controls.Add(this.label3);
            this.expandablePanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.expandablePanel1.ForeColor = System.Drawing.Color.SteelBlue;
            this.expandablePanel1.Location = new System.Drawing.Point(0, 0);
            this.expandablePanel1.Name = "expandablePanel1";
            this.expandablePanel1.Size = new System.Drawing.Size(1373, 75);
            this.expandablePanel1.TabIndex = 2;
            this.expandablePanel1.Text = "Step Move Compare";
            this.expandablePanel1.UseAnimation = true;
            // 
            // btnExportExel
            // 
            this.btnExportExel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportExel.Location = new System.Drawing.Point(1187, 29);
            this.btnExportExel.Name = "btnExportExel";
            this.btnExportExel.Size = new System.Drawing.Size(84, 38);
            this.btnExportExel.TabIndex = 96;
            this.btnExportExel.Text = "EXPORT TO\r\nEXCEL";
            this.btnExportExel.UseVisualStyleBackColor = true;
            this.btnExportExel.Click += new System.EventHandler(this.btnExportExel_Click);
            // 
            // moveOver0CheckBox
            // 
            this.moveOver0CheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.moveOver0CheckBox.AutoSize = true;
            this.moveOver0CheckBox.Checked = true;
            this.moveOver0CheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.moveOver0CheckBox.Location = new System.Drawing.Point(831, 52);
            this.moveOver0CheckBox.Name = "moveOver0CheckBox";
            this.moveOver0CheckBox.Size = new System.Drawing.Size(77, 16);
            this.moveOver0CheckBox.TabIndex = 95;
            this.moveOver0CheckBox.Text = "Move > 0";
            this.moveOver0CheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.wipCondSimTimeBtn);
            this.groupBox1.Controls.Add(this.wipCondInputBtn);
            this.groupBox1.Location = new System.Drawing.Point(919, 29);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(139, 37);
            this.groupBox1.TabIndex = 94;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "WipCondition";
            // 
            // wipCondSimTimeBtn
            // 
            this.wipCondSimTimeBtn.AutoSize = true;
            this.wipCondSimTimeBtn.Location = new System.Drawing.Point(61, 15);
            this.wipCondSimTimeBtn.Name = "wipCondSimTimeBtn";
            this.wipCondSimTimeBtn.Size = new System.Drawing.Size(74, 16);
            this.wipCondSimTimeBtn.TabIndex = 1;
            this.wipCondSimTimeBtn.Text = "SimTime";
            this.wipCondSimTimeBtn.UseVisualStyleBackColor = true;
            // 
            // wipCondInputBtn
            // 
            this.wipCondInputBtn.AutoSize = true;
            this.wipCondInputBtn.Checked = true;
            this.wipCondInputBtn.Location = new System.Drawing.Point(8, 15);
            this.wipCondInputBtn.Name = "wipCondInputBtn";
            this.wipCondInputBtn.Size = new System.Drawing.Size(50, 16);
            this.wipCondInputBtn.TabIndex = 0;
            this.wipCondInputBtn.TabStop = true;
            this.wipCondInputBtn.Text = "Input";
            this.wipCondInputBtn.UseVisualStyleBackColor = true;
            // 
            // areaLabel
            // 
            this.areaLabel.AutoSize = true;
            this.areaLabel.Location = new System.Drawing.Point(519, 46);
            this.areaLabel.Name = "areaLabel";
            this.areaLabel.Size = new System.Drawing.Size(31, 12);
            this.areaLabel.TabIndex = 93;
            this.areaLabel.Text = "Area";
            // 
            // areaChkBoxEdit
            // 
            this.areaChkBoxEdit.Location = new System.Drawing.Point(548, 42);
            this.areaChkBoxEdit.Name = "areaChkBoxEdit";
            this.areaChkBoxEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.areaChkBoxEdit.Size = new System.Drawing.Size(148, 20);
            this.areaChkBoxEdit.TabIndex = 92;
            // 
            // mainOnlyCheckBox
            // 
            this.mainOnlyCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.mainOnlyCheckBox.AutoSize = true;
            this.mainOnlyCheckBox.Checked = true;
            this.mainOnlyCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mainOnlyCheckBox.Location = new System.Drawing.Point(831, 31);
            this.mainOnlyCheckBox.Name = "mainOnlyCheckBox";
            this.mainOnlyCheckBox.Size = new System.Drawing.Size(82, 16);
            this.mainOnlyCheckBox.TabIndex = 91;
            this.mainOnlyCheckBox.Text = "Main Only";
            this.mainOnlyCheckBox.UseVisualStyleBackColor = true;
            // 
            // hourShiftRangeLabel
            // 
            this.hourShiftRangeLabel.AutoSize = true;
            this.hourShiftRangeLabel.Location = new System.Drawing.Point(461, 46);
            this.hourShiftRangeLabel.Name = "hourShiftRangeLabel";
            this.hourShiftRangeLabel.Size = new System.Drawing.Size(38, 12);
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
            this.hourShiftSpinEdit.Location = new System.Drawing.Point(418, 42);
            this.hourShiftSpinEdit.MaximumSize = new System.Drawing.Size(40, 20);
            this.hourShiftSpinEdit.MinimumSize = new System.Drawing.Size(40, 20);
            this.hourShiftSpinEdit.Name = "hourShiftSpinEdit";
            this.hourShiftSpinEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.hourShiftSpinEdit.Size = new System.Drawing.Size(40, 20);
            this.hourShiftSpinEdit.TabIndex = 88;
            // 
            // hourShiftGroupBox
            // 
            this.hourShiftGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.hourShiftGroupBox.Controls.Add(this.shiftRadioButton);
            this.hourShiftGroupBox.Controls.Add(this.hourRadioButton);
            this.hourShiftGroupBox.Location = new System.Drawing.Point(1062, 28);
            this.hourShiftGroupBox.Name = "hourShiftGroupBox";
            this.hourShiftGroupBox.Size = new System.Drawing.Size(115, 40);
            this.hourShiftGroupBox.TabIndex = 87;
            this.hourShiftGroupBox.TabStop = false;
            this.hourShiftGroupBox.Text = "TimeCondition";
            // 
            // shiftRadioButton
            // 
            this.shiftRadioButton.AutoSize = true;
            this.shiftRadioButton.Location = new System.Drawing.Point(61, 16);
            this.shiftRadioButton.Name = "shiftRadioButton";
            this.shiftRadioButton.Size = new System.Drawing.Size(47, 16);
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
            this.hourRadioButton.Size = new System.Drawing.Size(49, 16);
            this.hourRadioButton.TabIndex = 0;
            this.hourRadioButton.TabStop = true;
            this.hourRadioButton.Text = "Hour";
            this.hourRadioButton.UseVisualStyleBackColor = true;
            this.hourRadioButton.CheckedChanged += new System.EventHandler(this.dayRadioButton_CheckedChanged);
            // 
            // ShopIdLabel
            // 
            this.ShopIdLabel.AutoSize = true;
            this.ShopIdLabel.Location = new System.Drawing.Point(34, 47);
            this.ShopIdLabel.Name = "ShopIdLabel";
            this.ShopIdLabel.Size = new System.Drawing.Size(45, 12);
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
            // btnQuery
            // 
            this.btnQuery.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnQuery.Location = new System.Drawing.Point(1277, 29);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(84, 38);
            this.btnQuery.TabIndex = 65;
            this.btnQuery.Text = "QUERY";
            this.btnQuery.UseVisualStyleBackColor = true;
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // fromTimeEdit
            // 
            this.fromTimeEdit.EditValue = new System.DateTime(2017, 2, 10, 14, 30, 56, 0);
            this.fromTimeEdit.Location = new System.Drawing.Point(249, 42);
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
            this.fromTimeEdit.Size = new System.Drawing.Size(150, 20);
            this.fromTimeEdit.TabIndex = 75;
            // 
            // DateLabel
            // 
            this.DateLabel.AutoSize = true;
            this.DateLabel.Location = new System.Drawing.Point(176, 46);
            this.DateLabel.Name = "DateLabel";
            this.DateLabel.Size = new System.Drawing.Size(70, 12);
            this.DateLabel.TabIndex = 76;
            this.DateLabel.Text = "Date Range";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(401, 46);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(14, 12);
            this.label3.TabIndex = 74;
            this.label3.Text = "~";
            // 
            // pivotGridControl1
            // 
            this.pivotGridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pivotGridControl1.Location = new System.Drawing.Point(0, 75);
            this.pivotGridControl1.Name = "pivotGridControl1";
            this.pivotGridControl1.Size = new System.Drawing.Size(1373, 437);
            this.pivotGridControl1.TabIndex = 3;
            // 
            // StepMovePopUp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1373, 512);
            this.Controls.Add(this.pivotGridControl1);
            this.Controls.Add(this.expandablePanel1);
            this.Name = "StepMovePopUp";
            this.Text = "StepMovePopUp";
            this.expandablePanel1.ResumeLayout(false);
            this.expandablePanel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.areaChkBoxEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.hourShiftSpinEdit.Properties)).EndInit();
            this.hourShiftGroupBox.ResumeLayout(false);
            this.hourShiftGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.shopIdComboBoxEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fromTimeEdit.Properties.CalendarTimeProperties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fromTimeEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pivotGridControl1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Mozart.Studio.UIComponents.ExpandablePanel expandablePanel1;
        private System.Windows.Forms.Label hourShiftRangeLabel;
        private DevExpress.XtraEditors.SpinEdit hourShiftSpinEdit;
        private System.Windows.Forms.GroupBox hourShiftGroupBox;
        private System.Windows.Forms.RadioButton shiftRadioButton;
        private System.Windows.Forms.RadioButton hourRadioButton;
        private System.Windows.Forms.Label ShopIdLabel;
        private DevExpress.XtraEditors.ComboBoxEdit shopIdComboBoxEdit;
        private System.Windows.Forms.Button btnQuery;
        private DevExpress.XtraEditors.DateEdit fromTimeEdit;
        private System.Windows.Forms.Label DateLabel;
        private System.Windows.Forms.Label label3;
        private DevExpress.XtraPivotGrid.PivotGridControl pivotGridControl1;
        private System.Windows.Forms.CheckBox mainOnlyCheckBox;
        private System.Windows.Forms.Label areaLabel;
        private DevExpress.XtraEditors.CheckedComboBoxEdit areaChkBoxEdit;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton wipCondSimTimeBtn;
        private System.Windows.Forms.RadioButton wipCondInputBtn;
        private System.Windows.Forms.CheckBox moveOver0CheckBox;
        private System.Windows.Forms.Button btnExportExel;
    }
}