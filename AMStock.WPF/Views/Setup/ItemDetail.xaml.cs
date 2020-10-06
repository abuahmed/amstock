using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AMStock.Core.Models;
using AMStock.WPF.ViewModel;
using GalaSoft.MvvmLight.Messaging;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for Item.xaml
    /// </summary>
    public partial class ItemDetail : Window
    {
        public ItemDetail()
        {
            ItemDetailViewModel.Errors = 0;
            InitializeComponent();
            TxtItemCode.Focus();
        }
        //public ItemDetail(ItemDTO itemDto)
        //{
        //    ItemDetailViewModel.Errors = 0;
        //    InitializeComponent();           
        //    Messenger.Default.Send<ItemDTO>(itemDto);
        //    Messenger.Reset();
        //}
        public ItemDetail(ItemQuantityDTO itemQtyDto, WarehouseDTO warehouseDto)
        {
            ItemDetailViewModel.Errors = 0;
            InitializeComponent();

            //var itqtyId = itemQtyDto != null ? itemQtyDto.Id : 0;

            //int[] ids = { itqtyId, warehouseDto.Id };
            //Messenger.Default.Send<int[]>(ids);

            Messenger.Default.Send<ItemQuantityDTO>(itemQtyDto);
            Messenger.Default.Send<WarehouseDTO>(warehouseDto);

            Messenger.Reset();
        }
        public ItemDetail(ObservableCollection<TransactionLineDTO> transactionLines, TransactionHeaderDTO transactionHeader)
        {
            ItemDetailViewModel.Errors = 0;
            InitializeComponent();
            
            Messenger.Default.Send<ObservableCollection<TransactionLineDTO>>(transactionLines);
            Messenger.Default.Send<TransactionHeaderDTO>(transactionHeader);

            Messenger.Reset();
        }
        
        public ItemDetail(ItemQuantityDTO itemQtyDto, WarehouseDTO warehouseDto, System.Windows.Visibility itemsQtyVisibility)
        {
            ItemDetailViewModel.Errors = 0;
            InitializeComponent();
            //TxtBlockItemsQuantity.Visibility = itemsQtyVisibility;
            //TxtItemsQuantity.Visibility = itemsQtyVisibility;

            Messenger.Default.Send<ItemQuantityDTO>(itemQtyDto);
            Messenger.Default.Send<WarehouseDTO>(warehouseDto);

            Messenger.Reset();
        }
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) ItemDetailViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) ItemDetailViewModel.Errors -= 1;
        }

        private void WdwItemDetail_Loaded(object sender, RoutedEventArgs e)
        {
            TxtItemCode.Focus();
        }
        private void BtnSaveItem_Click(object sender, RoutedEventArgs e)
        {
            TxtItemCode.Focus();
        }
        private void ItemDetail_OnClosing(object sender, CancelEventArgs e)
        {
            ItemDetailViewModel.CleanUp();
        }
    }
}
