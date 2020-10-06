using System.Windows;
using System.Windows.Controls;
using AMStock.Core.Models;
using AMStock.WPF.ViewModel;
using GalaSoft.MvvmLight.Messaging;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for AddPayment.xaml
    /// </summary>
    public partial class AddPayment : Window
    {
        public AddPayment()
        {
            AddPaymentViewModel.Errors = 0;
            InitializeComponent();
        }
        public AddPayment(BusinessPartnerDTO customerDTO, WarehouseDTO warehouseDTO)
        {
            AddPaymentViewModel.Errors = 0;
            InitializeComponent();
            Messenger.Default.Send<WarehouseDTO>(warehouseDTO);
            Messenger.Default.Send<BusinessPartnerDTO>(customerDTO);
        }
        public AddPayment(PaymentDTO payment)
        {
            AddPaymentViewModel.Errors = 0;
            InitializeComponent();
            Messenger.Default.Send<PaymentDTO>(payment);
        }
        public AddPayment(TransactionHeaderDTO transaction)
        {
            AddPaymentViewModel.Errors = 0;
            InitializeComponent();
            Messenger.Default.Send<TransactionHeaderDTO>(transaction);
        }
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) AddPaymentViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) AddPaymentViewModel.Errors -= 1;
        }

        private void WdwAddPayment_Loaded(object sender, RoutedEventArgs e)
        {
            TxtCashAmount.Focus();
        }
    }
}
