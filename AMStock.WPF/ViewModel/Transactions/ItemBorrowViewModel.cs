using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AMStock.Core;
using AMStock.Core.Common;
using AMStock.Core.Enumerations;
using AMStock.Core.Extensions;
using AMStock.Core.Models;
using AMStock.Service;
using AMStock.Service.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace AMStock.WPF.ViewModel
{
    public class ItemBorrowViewModel : ViewModelBase
    {
        #region Fields
        private static IItemBorrowService _itemBorrowService;
        private ItemBorrowDTO _selectedItemBorrow;
        private IEnumerable<ItemBorrowDTO> _itemBorrowList;
        private ObservableCollection<ItemBorrowDTO> _itemBorrows;
        private int _totalNumberOfItemBorrows;
        private string _totalValueOfItemBorrows;
        private ICommand _refreshWindowCommand;
        private ICommand _addNewItemBorrowCommand, _saveItemBorrowCommand, _deleteItemBorrowCommand;

        #endregion

        #region Constructor
        public ItemBorrowViewModel()
        {
            FilterStartDate = DateTime.Now.AddDays(-30);
            FilterEndDate = DateTime.Now.AddDays(1);

            FillItemBorrowTypesCombo();
            GetLiveItemsQuantity();

            GetWarehouses();
            Load();
        }

        public void Load()
        {
            CleanUp();
            _itemBorrowService = new ItemBorrowService();

            if (Warehouses != null && Warehouses.Any())
            {
                if (SelectedWarehouse == null)
                    SelectedWarehouse = Warehouses.FirstOrDefault(w => w.IsDefault) ?? Warehouses.FirstOrDefault();
                else
                    SelectedWarehouse = SelectedWarehouse;
            }
        }
        public static void CleanUp()
        {
            if (_itemBorrowService != null)
                _itemBorrowService.Dispose();
        }
        public ICommand RefreshWindowCommand
        {
            get
            {
                return _refreshWindowCommand ?? (_refreshWindowCommand = new RelayCommand(Load));
            }
        }
        #endregion

        #region Public Properties
        public int TotalNumberOfItemBorrows
        {
            get { return _totalNumberOfItemBorrows; }
            set
            {
                _totalNumberOfItemBorrows = value;
                RaisePropertyChanged<int>(() => TotalNumberOfItemBorrows);
            }
        }
        public string TotalValueOfItemBorrows
        {
            get { return _totalValueOfItemBorrows; }
            set
            {
                _totalValueOfItemBorrows = value;
                RaisePropertyChanged<string>(() => TotalValueOfItemBorrows);
            }
        }
        public bool AddNewItemBorrowCommandEnability
        {
            get { return _addNewItemBorrowCommandEnability; }
            set
            {
                _addNewItemBorrowCommandEnability = value;
                RaisePropertyChanged<bool>(() => AddNewItemBorrowCommandEnability);
            }
        }
        public bool SaveItemBorrowCommandEnability
        {
            get { return _saveItemBorrowCommandEnability; }
            set
            {
                _saveItemBorrowCommandEnability = value;
                RaisePropertyChanged<bool>(() => SaveItemBorrowCommandEnability);
            }
        }

        public ItemBorrowDTO SelectedItemBorrow
        {
            get { return _selectedItemBorrow; }
            set
            {
                _selectedItemBorrow = value;
                RaisePropertyChanged<ItemBorrowDTO>(() => SelectedItemBorrow);
                if (SelectedItemBorrow != null && SelectedItemBorrow.Item != null)
                {
                    SelectedItemQuantity = ItemsQuantity.FirstOrDefault(i => i.ItemId == SelectedItemBorrow.ItemId);
                    if (SelectedItemBorrow.Warehouse != null) SelectedWarehouseForItem = SelectedItemBorrow.Warehouse;
                }
            }
        }
        public ObservableCollection<ItemBorrowDTO> ItemBorrows
        {
            get { return _itemBorrows; }
            set
            {
                _itemBorrows = value;
                RaisePropertyChanged<ObservableCollection<ItemBorrowDTO>>(() => ItemBorrows);
                if (ItemBorrows != null)
                {
                    //if (ItemBorrows.Count > 0)
                    //    SelectedItemBorrow = ItemBorrows.FirstOrDefault();
                    //else
                        ExcuteAddNewItemBorrowCommand();
                    TotalNumberOfItemBorrows = ItemBorrows.Count;
                }
            }
        }
        public IEnumerable<ItemBorrowDTO> ItemBorrowList
        {
            get { return _itemBorrowList; }
            set
            {
                _itemBorrowList = value;
                RaisePropertyChanged<IEnumerable<ItemBorrowDTO>>(() => ItemBorrowList);
            }
        }
        #endregion

        #region Commands
        public ICommand AddNewItemBorrowCommand
        {
            get
            {
                return _addNewItemBorrowCommand ?? (_addNewItemBorrowCommand = new RelayCommand(ExcuteAddNewItemBorrowCommand));
            }
        }
        private void ExcuteAddNewItemBorrowCommand()
        {
            if (SelectedWarehouse != null && SelectedWarehouse.Id != -1)
            {
                SelectedItemBorrow = new ItemBorrowDTO
                {
                    ItemBorrowDate = DateTime.Now,
                    WarehouseId = SelectedWarehouse.Id,
                    Quantity = 1
                };
                SelectedItemQuantity = null;
                SelectedWarehouseForItem = SelectedWarehouse;
            }
        }

        public ICommand SaveItemBorrowCommand
        {
            get
            {
                return _saveItemBorrowCommand ?? (_saveItemBorrowCommand = new RelayCommand(ExcuteSaveItemBorrowCommand, CanSave));
            }
        }
        private void ExcuteSaveItemBorrowCommand()
        {
            if (SelectedItemQuantity == null)
            {
                MessageBox.Show("Choose item first", "No item selected");
                return;
            }
            if (SelectedItemBorrow.QuantityReturned > SelectedItemBorrow.Quantity)
            {
                MessageBox.Show("Qty Above Borrowed quantity");
                return;
            }
            try
            {
                SelectedItemBorrow.ItemBorrowType = (ItemBorrowTypes)SelectedItemBorrowType.Value;

                //var newObject = SelectedItemBorrow.Id;

                var stat = _itemBorrowService.InsertOrUpdate(SelectedItemBorrow);
                if (stat != string.Empty)
                    MessageBox.Show("Can't save"
                                    + Environment.NewLine + stat, "Can't save", MessageBoxButton.OK,
                        MessageBoxImage.Error);

                else GetLiveItemBorrows();
                    //if (newObject == 0)
                    //ItemBorrows.Insert(0, SelectedItemBorrow);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't save"
                                  + Environment.NewLine + exception.Message, "Can't save", MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }
        }

        public ICommand DeleteItemBorrowCommand
        {
            get
            {
                return _deleteItemBorrowCommand ?? (_deleteItemBorrowCommand = new RelayCommand(ExcuteDeleteItemBorrowCommand, CanSave));
            }
        }
        private void ExcuteDeleteItemBorrowCommand()
        {
            if (MessageBox.Show("Are you Sure You want to Delete this ItemBorrow?", "Delete ItemBorrow", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    SelectedItemBorrow.Enabled = false;
                    var stat = _itemBorrowService.Disable(SelectedItemBorrow);
                    if (stat == string.Empty)
                    {
                        ItemBorrows.Remove(SelectedItemBorrow);
                        GetLiveItemBorrows();
                    }
                    else
                    {
                        MessageBox.Show("Can't Delete, may be the data is already in use..."
                             + Environment.NewLine + stat, "Can't Delete",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Can't Delete, may be the data is already in use..."
                         + Environment.NewLine + ex.Message + Environment.NewLine + ex.InnerException, "Can't Delete",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        #endregion

        public void GetLiveItemBorrows()
        {
            ItemBorrowList = new List<ItemBorrowDTO>();
            var criteria = new SearchCriteria<ItemBorrowDTO>()
            {
                CurrentUserId = Singleton.User.UserId,
                BeginingDate = FilterStartDate,
                EndingDate = FilterEndDate
            };

            if (SelectedWarehouse != null && SelectedWarehouse.Id != -1)
                criteria.SelectedWarehouseId = SelectedWarehouse.Id;

            if (!string.IsNullOrEmpty(FilterByPerson))
                criteria.FiList.Add(pi => pi.PersonName.Contains(FilterByPerson));

            if (!string.IsNullOrEmpty(FilterByReason))
                criteria.FiList.Add(pi => pi.ShopName.Contains(FilterByReason));

            if (SelectedItemBorrowType != null)
            {
                var btype = (ItemBorrowTypes)SelectedItemBorrowType.Value;
                criteria.FiList.Add(p => p.ItemBorrowType == btype);
            }

            ItemBorrowList = _itemBorrowService.GetAll(criteria).ToList();

            if (SelectedItemBorrowStatus != null && SelectedItemBorrowStatus.Value != -1)
                ItemBorrowList = ItemBorrowList.Where(p => p.ReturnCompleted == SelectedItemBorrowStatus.Display);


            ItemBorrows = new ObservableCollection<ItemBorrowDTO>(ItemBorrowList);

        }

        #region Warehouse
        private IEnumerable<WarehouseDTO> _warehouses;
        private WarehouseDTO _selectedWarehouse, _selectedWarehouseForItem;

        public IEnumerable<WarehouseDTO> Warehouses
        {
            get { return _warehouses; }
            set
            {
                _warehouses = value;
                RaisePropertyChanged<IEnumerable<WarehouseDTO>>(() => Warehouses);
            }
        }
        public WarehouseDTO SelectedWarehouse
        {
            get { return _selectedWarehouse; }
            set
            {
                _selectedWarehouse = value;
                RaisePropertyChanged<WarehouseDTO>(() => SelectedWarehouse);
                if (SelectedWarehouse != null)
                {
                    AddNewItemBorrowCommandEnability = SelectedWarehouse.Id != -1;
                    SaveItemBorrowCommandEnability = SelectedWarehouse.Id != -1;
                    GetLiveItemBorrows();
                }
            }
        }
        public WarehouseDTO SelectedWarehouseForItem
        {
            get { return _selectedWarehouseForItem; }
            set
            {
                _selectedWarehouseForItem = value;
                RaisePropertyChanged<WarehouseDTO>(() => SelectedWarehouseForItem);
            }
        }
        public void GetWarehouses()
        {
            Warehouses = Singleton.WarehousesList;
        }
        #endregion

        #region Filter Header
        private DateTime _filterStartDate, _filterEndDate;
        private string _filterByPerson, _filterByReason;
        private List<ListDataItem> _itemBorrowTypes, _itemBorrowStatuses;
        private ListDataItem _selectedItemBorrowType, _selectedItemBorrowStatus;

        public DateTime FilterStartDate
        {
            get { return _filterStartDate; }
            set
            {
                _filterStartDate = value;
                RaisePropertyChanged<DateTime>(() => FilterStartDate);
            }
        }
        public DateTime FilterEndDate
        {
            get { return _filterEndDate; }
            set
            {
                _filterEndDate = value;
                RaisePropertyChanged<DateTime>(() => FilterEndDate);
            }
        }

        public string FilterByPerson
        {
            get { return _filterByPerson; }
            set
            {
                _filterByPerson = value;
                RaisePropertyChanged<string>(() => FilterByPerson);
                GetLiveItemBorrows();
            }
        }
        public string FilterByReason
        {
            get { return _filterByReason; }
            set
            {
                _filterByReason = value;
                RaisePropertyChanged<string>(() => FilterByReason);
                GetLiveItemBorrows();
            }
        }

        public List<ListDataItem> ItemBorrowTypesList
        {
            get { return _itemBorrowTypes; }
            set
            {
                _itemBorrowTypes = value;
                RaisePropertyChanged<List<ListDataItem>>(() => ItemBorrowTypesList);
            }
        }
        public ListDataItem SelectedItemBorrowType
        {
            get { return _selectedItemBorrowType; }
            set
            {
                _selectedItemBorrowType = value;
                RaisePropertyChanged<ListDataItem>(() => SelectedItemBorrowType);
                //GetLiveItemBorrows();
            }
        }

        public List<ListDataItem> ItemBorrowStatuses
        {
            get { return _itemBorrowStatuses; }
            set
            {
                _itemBorrowStatuses = value;
                RaisePropertyChanged<List<ListDataItem>>(() => ItemBorrowStatuses);
            }
        }
        public ListDataItem SelectedItemBorrowStatus
        {
            get { return _selectedItemBorrowStatus; }
            set
            {
                _selectedItemBorrowStatus = value;
                RaisePropertyChanged<ListDataItem>(() => SelectedItemBorrowStatus);
                //GetLiveItemBorrows();
            }
        }

        private void FillItemBorrowTypesCombo()
        {
            ItemBorrowStatuses = new List<ListDataItem>
            {
                new ListDataItem {Display = "All", Value = -1},
                new ListDataItem {Display = "Not Returned", Value = 0},
                //new ListDataItem {Display = "Partially Returned", Value = 1},
                new ListDataItem {Display = "Fully Returned", Value = 2}
            };
            ItemBorrowTypesList = new List<ListDataItem>
            {
                new ListDataItem {Display = EnumUtil.GetEnumDesc(ItemBorrowTypes.BorrowedTo), Value =(int) ItemBorrowTypes.BorrowedTo},
                new ListDataItem {Display = EnumUtil.GetEnumDesc(ItemBorrowTypes.BorrowedFrom), Value =(int) ItemBorrowTypes.BorrowedFrom}
            };
        }

        #endregion

        #region ItemsQuantity
        private ObservableCollection<ItemQuantityDTO> _itemsQuantity;
        private ItemQuantityDTO _selectedItemQuantity;
        private bool _addNewItemBorrowCommandEnability;
        private bool _saveItemBorrowCommandEnability;

        public ObservableCollection<ItemQuantityDTO> ItemsQuantity
        {
            get { return _itemsQuantity; }
            set
            {
                _itemsQuantity = value;
                RaisePropertyChanged<ObservableCollection<ItemQuantityDTO>>(() => ItemsQuantity);
            }
        }
        public ItemQuantityDTO SelectedItemQuantity
        {
            get { return _selectedItemQuantity; }
            set
            {
                _selectedItemQuantity = value;
                RaisePropertyChanged<ItemQuantityDTO>(() => SelectedItemQuantity);

                if (SelectedItemQuantity != null)
                {
                    SelectedItemBorrow.ItemId = SelectedItemQuantity.ItemId;
                }
            }
        }

        public void GetLiveItemsQuantity()
        {
            var criteria = new SearchCriteria<ItemQuantityDTO>()
            {
                CurrentUserId = Singleton.User.UserId
            };

            if (SelectedWarehouse != null && SelectedWarehouse.Id != -1)
                criteria.SelectedWarehouseId = SelectedWarehouse.Id;
            int totalCount = 0;
            var itemQuantityList = new ItemQuantityService(false).GetAll(criteria, out totalCount).ToList();

            ItemsQuantity = new ObservableCollection<ItemQuantityDTO>(itemQuantityList);
        }


        #endregion

        #region Validation
        public static int Errors { get; set; }
        public bool CanSave()
        {
            return Errors == 0;
        }
        #endregion
    }
}