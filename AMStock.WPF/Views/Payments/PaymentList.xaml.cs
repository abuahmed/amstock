using System.ComponentModel;
using System.Windows;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.WPF.ViewModel;
using GalaSoft.MvvmLight.Messaging;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for PaymentList.xaml
    /// </summary>
    public partial class PaymentList : Window
    {
        public PaymentList()
        {
            InitializeComponent();
        }
        public PaymentList(TransactionTypes transaction, BusinessPartnerDTO businessPartner, PaymentListTypes listType)
        {
            InitializeComponent();
            Messenger.Default.Send<BusinessPartnerDTO>(businessPartner);
            Messenger.Default.Send<PaymentListTypes>(listType);
            Messenger.Default.Send<TransactionTypes>(transaction);
            Messenger.Reset();
        }

        private void PaymentList_OnClosing(object sender, CancelEventArgs e)
        {
            PaymentListViewModel.CleanUp();
        }
    }
}
