using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraEditors.Repository;

namespace CSOT.UserInterface.ComponentFactory
{
    public class GridControlFactory
    {
        public static GridControl CreateGridControl()
        {
            var gridControl = new GridControl();

            InitializeControl(gridControl);

            return gridControl;
        }

        private static void InitializeControl(GridControl gridControl)
        {
            var gridView = new GridView();
            var repositoryItemDateEdit1 = new RepositoryItemDateEdit();
            var repositoryItemComboBox1 = new RepositoryItemComboBox();
            var repositoryItemTimeEdit1 = new RepositoryItemTimeEdit();
            var repositoryItemDateEdit2 = new RepositoryItemDateEdit();
            var repositoryItemMemoExEdit1 = new RepositoryItemMemoExEdit();

            gridControl.Dock = System.Windows.Forms.DockStyle.Fill;
            gridControl.Location = new System.Drawing.Point(0, 0);
            gridControl.MainView = gridView;
            gridControl.Name = "gridControl";
            gridControl.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            repositoryItemDateEdit1,
            repositoryItemComboBox1,
            repositoryItemTimeEdit1,
            repositoryItemDateEdit2,
            repositoryItemMemoExEdit1});
            gridControl.Size = new System.Drawing.Size(718, 504);
            gridControl.TabIndex = 26;
            gridControl.UseEmbeddedNavigator = true;
            gridControl.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            gridView});
            // 
            // gridView
            // 
            gridView.GridControl = gridControl;
            gridView.Name = "gridView";
            gridView.OptionsSelection.MultiSelect = true;
            gridView.OptionsView.BestFitMaxRowCount = 10;
            gridView.OptionsView.ColumnAutoWidth = false;
            gridView.OptionsView.NewItemRowPosition = DevExpress.XtraGrid.Views.Grid.NewItemRowPosition.Bottom;
            gridView.OptionsView.ShowAutoFilterRow = true;
            gridView.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.ShowAlways;
            gridView.OptionsView.ShowFooter = true;
            // 
            // repositoryItemDateEdit1
            // 
            repositoryItemDateEdit1.AutoHeight = false;
            repositoryItemDateEdit1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            repositoryItemDateEdit1.CalendarTimeEditing = DevExpress.Utils.DefaultBoolean.True;
            repositoryItemDateEdit1.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            repositoryItemDateEdit1.CalendarView = DevExpress.XtraEditors.Repository.CalendarView.Vista;
            repositoryItemDateEdit1.Mask.EditMask = "yyyy/MM/dd HH:mm:ss";
            repositoryItemDateEdit1.Mask.UseMaskAsDisplayFormat = true;
            repositoryItemDateEdit1.Name = "repositoryItemDateEdit1";
            repositoryItemDateEdit1.VistaDisplayMode = DevExpress.Utils.DefaultBoolean.True;
            // 
            // repositoryItemComboBox1
            // 
            repositoryItemComboBox1.AutoHeight = false;
            repositoryItemComboBox1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            repositoryItemComboBox1.Name = "repositoryItemComboBox1";
            // 
            // repositoryItemTimeEdit1
            // 
            repositoryItemTimeEdit1.AutoHeight = false;
            repositoryItemTimeEdit1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            repositoryItemTimeEdit1.Name = "repositoryItemTimeEdit1";
            // 
            // repositoryItemDateEdit2
            // 
            repositoryItemDateEdit2.AutoHeight = false;
            repositoryItemDateEdit2.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            repositoryItemDateEdit2.CalendarTimeProperties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            repositoryItemDateEdit2.Name = "repositoryItemDateEdit2";
            // 
            // repositoryItemMemoExEdit1
            // 
            repositoryItemMemoExEdit1.AutoHeight = false;
            repositoryItemMemoExEdit1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            repositoryItemMemoExEdit1.Name = "repositoryItemMemoExEdit1";
            repositoryItemMemoExEdit1.ReadOnly = true;
            repositoryItemMemoExEdit1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            repositoryItemMemoExEdit1.ShowIcon = false;
        }
    }
}
