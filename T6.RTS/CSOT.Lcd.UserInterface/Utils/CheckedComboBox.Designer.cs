namespace CSOT.Lcd.UserInterface.Utils
{
    partial class CheckedComboBox
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
            this.popupContainerControl1 = new DevExpress.XtraEditors.PopupContainerControl();
            this.checkedListBox = new DevExpress.XtraEditors.CheckedListBoxControl();
            this.comboBox = new DevExpress.XtraEditors.PopupContainerEdit();
            ((System.ComponentModel.ISupportInitialize)(this.popupContainerControl1)).BeginInit();
            this.popupContainerControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.checkedListBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBox.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // popupContainerControl1
            // 
            this.popupContainerControl1.Controls.Add(this.checkedListBox);
            this.popupContainerControl1.Location = new System.Drawing.Point(3, 3);
            this.popupContainerControl1.Name = "popupContainerControl1";
            this.popupContainerControl1.Size = new System.Drawing.Size(208, 255);
            this.popupContainerControl1.TabIndex = 19;
            // 
            // checkedListBox
            // 
            this.checkedListBox.CheckOnClick = true;
            this.checkedListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkedListBox.HotTrackItems = true;
            this.checkedListBox.Location = new System.Drawing.Point(0, 0);
            this.checkedListBox.Name = "checkedListBox";
            this.checkedListBox.Size = new System.Drawing.Size(208, 255);
            this.checkedListBox.TabIndex = 0;
            // 
            // comboBox
            // 
            this.comboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBox.Location = new System.Drawing.Point(0, 0);
            this.comboBox.Name = "comboBox";
            this.comboBox.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.comboBox.Properties.PopupControl = this.popupContainerControl1;
            this.comboBox.Size = new System.Drawing.Size(214, 20);
            this.comboBox.TabIndex = 18;
            this.comboBox.QueryDisplayText += new DevExpress.XtraEditors.Controls.QueryDisplayTextEventHandler(this.comboBox_QueryDisplayText);
            // 
            // CheckedComboBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.popupContainerControl1);
            this.Controls.Add(this.comboBox);
            this.Name = "CheckedComboBox";
            this.Size = new System.Drawing.Size(214, 62);
            ((System.ComponentModel.ISupportInitialize)(this.popupContainerControl1)).EndInit();
            this.popupContainerControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.checkedListBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBox.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PopupContainerControl popupContainerControl1;
        private DevExpress.XtraEditors.CheckedListBoxControl checkedListBox;
        private DevExpress.XtraEditors.PopupContainerEdit comboBox;
    }
}
