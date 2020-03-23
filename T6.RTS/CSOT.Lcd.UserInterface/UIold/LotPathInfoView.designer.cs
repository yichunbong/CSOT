using Northwoods.Go;
using System.Windows.Forms;
namespace CSOT.Lcd.UserInterface.Analysis
{
    partial class LotPathInfoView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.goView1 = new Northwoods.Go.GoView();
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.dockPanel3 = new DevExpress.XtraBars.Docking.DockPanel();
            this.dockPanel3_Container = new DevExpress.XtraBars.Docking.ControlContainer();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.dockManager2 = new DevExpress.XtraBars.Docking.DockManager(this.components);
            this.dockPanel4 = new DevExpress.XtraBars.Docking.DockPanel();
            this.dockPanel4_Container = new DevExpress.XtraBars.Docking.ControlContainer();
            this.gcLotInfoList = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.dockPanel2 = new DevExpress.XtraBars.Docking.DockPanel();
            this.dockPanel2_Container = new DevExpress.XtraBars.Docking.ControlContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_OK = new System.Windows.Forms.Button();
            this.lotID_TextBox = new System.Windows.Forms.TextBox();
            this.meLotInfo = new DevExpress.XtraEditors.MemoEdit();
            this.panelContainer1 = new DevExpress.XtraBars.Docking.DockPanel();
            this.hideContainerLeft = new DevExpress.XtraBars.Docking.AutoHideContainer();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            this.dockPanel3.SuspendLayout();
            this.dockPanel3_Container.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dockManager2)).BeginInit();
            this.dockPanel4.SuspendLayout();
            this.dockPanel4_Container.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcLotInfoList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            this.dockPanel2.SuspendLayout();
            this.dockPanel2_Container.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.meLotInfo.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // goView1
            // 
            this.goView1.AllowCopy = false;
            this.goView1.AllowDelete = false;
            this.goView1.AllowDragOut = false;
            this.goView1.AllowDrop = false;
            this.goView1.AllowEdit = false;
            this.goView1.AllowInsert = false;
            this.goView1.AllowKey = false;
            this.goView1.AllowLink = false;
            this.goView1.AllowMove = false;
            this.goView1.AllowReshape = false;
            this.goView1.AllowResize = false;
            this.goView1.ArrowMoveLarge = 10F;
            this.goView1.ArrowMoveSmall = 1F;
            this.goView1.BackColor = System.Drawing.Color.White;
            this.goView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.goView1.Location = new System.Drawing.Point(0, 0);
            this.goView1.Name = "goView1";
            this.goView1.Size = new System.Drawing.Size(690, 186);
            this.goView1.TabIndex = 0;
            this.goView1.Text = "goView1";
            this.goView1.ObjectDoubleClicked += new Northwoods.Go.GoObjectEventHandler(this.goView1_ObjectDoubleClicked);
            // 
            // layoutControl1
            // 
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.Margin = new System.Windows.Forms.Padding(0);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(390, 464);
            this.layoutControl1.TabIndex = 0;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.CustomizationFormText = "layoutControlGroup1";
            this.layoutControlGroup1.Name = "layoutControlGroup1";
            this.layoutControlGroup1.Size = new System.Drawing.Size(390, 464);
            // 
            // dockPanel3
            // 
            this.dockPanel3.Controls.Add(this.dockPanel3_Container);
            this.dockPanel3.Dock = DevExpress.XtraBars.Docking.DockingStyle.Fill;
            this.dockPanel3.FloatVertical = true;
            this.dockPanel3.ID = new System.Guid("e7c6ba1b-cd12-4434-9367-ede4cfe4ac96");
            this.dockPanel3.Location = new System.Drawing.Point(259, 0);
            this.dockPanel3.Name = "dockPanel3";
            this.dockPanel3.OriginalSize = new System.Drawing.Size(752, 200);
            this.dockPanel3.Size = new System.Drawing.Size(698, 214);
            this.dockPanel3.Text = "ProductLotPathView";
            // 
            // dockPanel3_Container
            // 
            this.dockPanel3_Container.Controls.Add(this.goView1);
            this.dockPanel3_Container.Location = new System.Drawing.Point(4, 23);
            this.dockPanel3_Container.Name = "dockPanel3_Container";
            this.dockPanel3_Container.Size = new System.Drawing.Size(690, 186);
            this.dockPanel3_Container.TabIndex = 0;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.dockPanel3;
            this.layoutControlItem1.CustomizationFormText = "layoutControlItem1";
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(213, 79);
            this.layoutControlItem1.TextSize = new System.Drawing.Size(50, 20);
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.CustomizationFormText = "Process ID :";
            this.layoutControlItem2.Location = new System.Drawing.Point(10, 0);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Padding = new DevExpress.XtraLayout.Utils.Padding(0, 0, 3, 0);
            this.layoutControlItem2.Size = new System.Drawing.Size(242, 26);
            this.layoutControlItem2.Text = "Find Product :";
            this.layoutControlItem2.TextSize = new System.Drawing.Size(77, 14);
            // 
            // dockManager2
            // 
            this.dockManager2.Form = this;
            this.dockManager2.RootPanels.AddRange(new DevExpress.XtraBars.Docking.DockPanel[] {
            this.dockPanel4,
            this.dockPanel2,
            this.dockPanel3});
            this.dockManager2.TopZIndexControls.AddRange(new string[] {
            "DevExpress.XtraBars.BarDockControl",
            "DevExpress.XtraBars.StandaloneBarDockControl",
            "System.Windows.Forms.StatusBar",
            "DevExpress.XtraBars.Ribbon.RibbonStatusBar",
            "DevExpress.XtraBars.Ribbon.RibbonControl"});
            // 
            // dockPanel4
            // 
            this.dockPanel4.Controls.Add(this.dockPanel4_Container);
            this.dockPanel4.Dock = DevExpress.XtraBars.Docking.DockingStyle.Bottom;
            this.dockPanel4.FloatVertical = true;
            this.dockPanel4.ID = new System.Guid("82deb0cf-6edc-4a08-b78a-397c7cfcdbf7");
            this.dockPanel4.Location = new System.Drawing.Point(0, 214);
            this.dockPanel4.Name = "dockPanel4";
            this.dockPanel4.Options.ShowCloseButton = false;
            this.dockPanel4.OriginalSize = new System.Drawing.Size(158, 250);
            this.dockPanel4.Size = new System.Drawing.Size(957, 250);
            this.dockPanel4.Text = "Lot Infomation";
            // 
            // dockPanel4_Container
            // 
            this.dockPanel4_Container.Controls.Add(this.gcLotInfoList);
            this.dockPanel4_Container.Location = new System.Drawing.Point(4, 24);
            this.dockPanel4_Container.Name = "dockPanel4_Container";
            this.dockPanel4_Container.Size = new System.Drawing.Size(949, 222);
            this.dockPanel4_Container.TabIndex = 0;
            // 
            // gcLotInfoList
            // 
            this.gcLotInfoList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gcLotInfoList.Location = new System.Drawing.Point(0, 0);
            this.gcLotInfoList.MainView = this.gridView1;
            this.gcLotInfoList.Name = "gcLotInfoList";
            this.gcLotInfoList.Size = new System.Drawing.Size(949, 222);
            this.gcLotInfoList.TabIndex = 12;
            this.gcLotInfoList.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.GridControl = this.gcLotInfoList;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsView.ColumnAutoWidth = false;
            this.gridView1.OptionsView.ShowAutoFilterRow = true;
            // 
            // dockPanel2
            // 
            this.dockPanel2.Controls.Add(this.dockPanel2_Container);
            this.dockPanel2.Dock = DevExpress.XtraBars.Docking.DockingStyle.Left;
            this.dockPanel2.FloatVertical = true;
            this.dockPanel2.ID = new System.Guid("cb5bab4e-a977-4bcf-9ee8-f677f391db1c");
            this.dockPanel2.Location = new System.Drawing.Point(0, 0);
            this.dockPanel2.Name = "dockPanel2";
            this.dockPanel2.OriginalSize = new System.Drawing.Size(259, 232);
            this.dockPanel2.Size = new System.Drawing.Size(259, 214);
            this.dockPanel2.Text = "Search";
            // 
            // dockPanel2_Container
            // 
            this.dockPanel2_Container.Controls.Add(this.label1);
            this.dockPanel2_Container.Controls.Add(this.btn_OK);
            this.dockPanel2_Container.Controls.Add(this.lotID_TextBox);
            this.dockPanel2_Container.Controls.Add(this.meLotInfo);
            this.dockPanel2_Container.Location = new System.Drawing.Point(4, 23);
            this.dockPanel2_Container.Name = "dockPanel2_Container";
            this.dockPanel2_Container.Size = new System.Drawing.Size(250, 187);
            this.dockPanel2_Container.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 14);
            this.label1.TabIndex = 19;
            this.label1.Text = "LotID:";
            // 
            // btn_OK
            // 
            this.btn_OK.Location = new System.Drawing.Point(185, 17);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(65, 22);
            this.btn_OK.TabIndex = 18;
            this.btn_OK.Text = "OK";
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            // 
            // lotID_TextBox
            // 
            this.lotID_TextBox.Location = new System.Drawing.Point(52, 17);
            this.lotID_TextBox.Name = "lotID_TextBox";
            this.lotID_TextBox.Size = new System.Drawing.Size(127, 22);
            this.lotID_TextBox.TabIndex = 17;
            // 
            // meLotInfo
            // 
            this.meLotInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.meLotInfo.EditValue = "";
            this.meLotInfo.Location = new System.Drawing.Point(3, 55);
            this.meLotInfo.Name = "meLotInfo";
            this.meLotInfo.Properties.AllowFocused = false;
            this.meLotInfo.Size = new System.Drawing.Size(244, 129);
            this.meLotInfo.TabIndex = 16;
            // 
            // panelContainer1
            // 
            this.panelContainer1.Dock = DevExpress.XtraBars.Docking.DockingStyle.Fill;
            this.panelContainer1.FloatVertical = true;
            this.panelContainer1.ID = new System.Guid("f16d909d-87bb-46d4-8001-f3742c33cc1b");
            this.panelContainer1.Location = new System.Drawing.Point(0, 0);
            this.panelContainer1.Name = "panelContainer1";
            this.panelContainer1.OriginalSize = new System.Drawing.Size(499, 200);
            this.panelContainer1.Size = new System.Drawing.Size(499, 464);
            // 
            // hideContainerLeft
            // 
            this.hideContainerLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(245)))), ((int)(((byte)(241)))));
            this.hideContainerLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.hideContainerLeft.Location = new System.Drawing.Point(0, 0);
            this.hideContainerLeft.Name = "hideContainerLeft";
            this.hideContainerLeft.Size = new System.Drawing.Size(20, 464);
            // 
            // LotPathInfoView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dockPanel3);
            this.Controls.Add(this.dockPanel2);
            this.Controls.Add(this.dockPanel4);
            this.Name = "LotPathInfoView";
            this.Size = new System.Drawing.Size(957, 464);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            this.dockPanel3.ResumeLayout(false);
            this.dockPanel3_Container.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dockManager2)).EndInit();
            this.dockPanel4.ResumeLayout(false);
            this.dockPanel4_Container.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gcLotInfoList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            this.dockPanel2.ResumeLayout(false);
            this.dockPanel2_Container.ResumeLayout(false);
            this.dockPanel2_Container.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.meLotInfo.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraBars.Docking.DockManager dockManager2;
        private DevExpress.XtraBars.Docking.DockPanel dockPanel3;
        private DevExpress.XtraBars.Docking.ControlContainer dockPanel3_Container;
        private DevExpress.XtraBars.Docking.DockPanel dockPanel4;
        private DevExpress.XtraBars.Docking.ControlContainer dockPanel4_Container;
        private DevExpress.XtraGrid.GridControl gcLotInfoList;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private Northwoods.Go.GoView goView1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraBars.Docking.DockPanel panelContainer1;
        private DevExpress.XtraBars.Docking.AutoHideContainer hideContainerLeft;
        private DevExpress.XtraBars.Docking.DockPanel dockPanel2;
        private DevExpress.XtraBars.Docking.ControlContainer dockPanel2_Container;
        private DevExpress.XtraEditors.MemoEdit meLotInfo;
        private Label label1;
        private Button btn_OK;
        private TextBox lotID_TextBox;
    }
}
