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
    public partial class ItemsTransferList : Window
    {
        public ItemsTransferList()
        {
            InitializeComponent();
        }
        public ItemsTransferList(ItemDTO itemDto)
        {
            InitializeComponent();
            Messenger.Default.Send<ItemDTO>(itemDto);
            Messenger.Reset();
        }

        private void ItemsTransferList_OnClosing(object sender, CancelEventArgs e)
        {
            ItemsTransferListViewModel.CleanUp();
        }
    }
}
