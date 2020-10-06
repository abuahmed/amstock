using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AMStock.WPF.ViewModel;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for BankGuarantees.xaml
    /// </summary>
    public partial class BankGuarantees : Window
    {
        public BankGuarantees()
        {
            BankGuaranteeViewModel.Errors = 0;
            InitializeComponent();
        }
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) BankGuaranteeViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) BankGuaranteeViewModel.Errors -= 1;
        }

        private void BankGuarantees_OnClosing(object sender, CancelEventArgs e)
        {
            BankGuaranteeViewModel.CleanUp();
        }

        private void BtnAddNewBa_OnClick(object sender, RoutedEventArgs e)
        {
            TxtBankBranch.Focus();
        }
    }
}
