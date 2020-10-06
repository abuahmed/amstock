using AMStock.WPF.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for Items.xaml
    /// </summary>
    public partial class Items : Window
    {
        public Items()
        {
            ItemViewModel.Errors = 0;
            InitializeComponent();
        }

        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) ItemViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) ItemViewModel.Errors -= 1;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtItemCode.Focus();
        }

        private void BtnAddNew_Click(object sender, RoutedEventArgs e)
        {
            TxtItemCode.Focus();
            //TxtPurchasePrice.Text = "";
            //TxtSellPrice.Text = "";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ItemViewModel.CleanUp();
        }
    }
}
