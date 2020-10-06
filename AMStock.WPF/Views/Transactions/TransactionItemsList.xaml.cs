using System.ComponentModel;
using System.Windows;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.WPF.ViewModel;
using GalaSoft.MvvmLight.Messaging;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for SalesDetailList.xaml
    /// </summary>
    public partial class TransactionItemsList : Window
    {
        public TransactionItemsList()
        {
            InitializeComponent();
        }
        public TransactionItemsList(TransactionTypes transactionType, ItemDTO itemDto)
        {
            InitializeComponent();
            Messenger.Default.Send<TransactionTypes>(transactionType);
            Messenger.Default.Send<ItemDTO>(itemDto);
            Messenger.Reset();
        }

        private void TransactionItemsList_OnClosing(object sender, CancelEventArgs e)
        {
            TransactionItemsListViewModel.CleanUp();
        }
    }
}
