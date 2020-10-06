using System.ComponentModel;
using AMStock.WPF.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for Warehouses.xaml
    /// </summary>
    public partial class Warehouses : Window
    {
        public Warehouses()
        {
            WarehouseViewModel.Errors = 0;
            WarehouseViewModel.LineErrors = 0;
            InitializeComponent();
        }
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) WarehouseViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) WarehouseViewModel.Errors -= 1;
        }
        private void Validation_ErrorLine(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) WarehouseViewModel.LineErrors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) WarehouseViewModel.LineErrors -= 1;
        }

        private void BtnAddNew_Click(object sender, RoutedEventArgs e)
        {
            TxtWarehouseName.Focus();
        }

        private void Warehouses_OnClosing(object sender, CancelEventArgs e)
        {
            WarehouseViewModel.CleanUp();
        }
    }
}
