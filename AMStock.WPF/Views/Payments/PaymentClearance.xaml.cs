using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AMStock.Core.Models;
using AMStock.WPF.ViewModel;
using GalaSoft.MvvmLight.Messaging;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for PaymentClearance.xaml
    /// </summary>
    public partial class PaymentClearance : Window
    {
        public PaymentClearance()
        {
            InitializeComponent();
        }
        public PaymentClearance(PaymentDTO payment)
        {
            PaymentClearanceViewModel.Errors = 0;
            InitializeComponent();
            Messenger.Default.Send<PaymentDTO>(payment);
        }
     
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) PaymentClearanceViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) PaymentClearanceViewModel.Errors -= 1;
        }

        private void WdwPaymentClearance_Loaded(object sender, RoutedEventArgs e)
        {
            TxtStatementNumber.Focus();
        }

        private void PaymentClearance_OnClosing(object sender, CancelEventArgs e)
        {
            PaymentClearanceViewModel.CleanUp();
        }
    }
}
