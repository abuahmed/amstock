using System.Windows;
using System.Windows.Controls;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.WPF.ViewModel;
using GalaSoft.MvvmLight.Messaging;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for ExpenseLoanEntry.xaml
    /// </summary>
    public partial class ExpenseLoanEntry : Window
    {
        public ExpenseLoanEntry()
        {
            ExpenseLoanEntryViewModel.Errors = 0;
            InitializeComponent();
        }
        public ExpenseLoanEntry(PaymentTypes paymentType)
        {
            ExpenseLoanEntryViewModel.Errors = 0;
            InitializeComponent();
            Messenger.Default.Send<PaymentTypes>(paymentType);
        }
        public ExpenseLoanEntry(PaymentDTO paymentDTO)
        {
            ExpenseLoanEntryViewModel.Errors = 0;
            InitializeComponent();
            Messenger.Default.Send<PaymentDTO>(paymentDTO);
        }
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) ExpenseLoanEntryViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) ExpenseLoanEntryViewModel.Errors -= 1;
        }

        private void WdwExpenseLoanEntry_Loaded(object sender, RoutedEventArgs e)
        {
            TxtReason.Focus();
        }
    }
}
