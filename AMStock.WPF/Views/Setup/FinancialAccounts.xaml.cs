using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AMStock.WPF.ViewModel;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for FinancialAccounts.xaml
    /// </summary>
    public partial class FinancialAccounts : Window
    {
        public FinancialAccounts()
        {
            InitializeComponent();
        }
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) FinancialAccountViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) FinancialAccountViewModel.Errors -= 1;
        }

        private void FinancialAccounts_OnClosing(object sender, CancelEventArgs e)
        {
            FinancialAccountViewModel.CleanUp();
        }

        private void BtnAddNewBa_OnClick(object sender, RoutedEventArgs e)
        {
            TxtBankBranch.Focus();
        }
    }
}
