#region using
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AMStock.Core;
using AMStock.Core.Common;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.Service;
using AMStock.Service.Interfaces;
using AMStock.WPF.Models;
using AMStock.WPF.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
#endregion

namespace AMStock.WPF.ViewModel
{
    public class ItemsMovementViewModel : ViewModelBase
    {
        #region Fields
        private static IItemsMovementHeaderService _imHeaderService;
        private ObservableCollection<ItemsMovementHeaderDTO> _itemsMovements;
        private ItemsMovementHeaderDTO _selectedItemsMovement;
        private ICommand _refreshWindowCommand, _filterByDateCommand;
        private int _totalItemsCounted;
        private bool _loadData, _saveHeaderEnability, _unPostEnability;
        #endregion

        #region Constructor
        public ItemsMovementViewModel()
        {
            FillCombo();
            FillPeriodCombo();

            SelectedPeriod = FilterPeriods.FirstOrDefault(p => p.Value == 1);//Value == 0(show all) Value=3(show this week)
        }

        public bool LoadData
        {
            get { return _loadData; }
            set
            {
                _loadData = value;
                RaisePropertyChanged<bool>(() => LoadData);
                if (LoadData)
                    Load();
            }
        }

        public void Load()
        {
            CleanUp();
            _imHeaderService = new ItemsMovementHeaderService();
            
            GetWarehouses();
            GetLiveItemsMovements();
        }

        public ICommand RefreshWindowCommand
        {
            get
            {
                return _refreshWindowCommand ?? (_refreshWindowCommand = new RelayCommand(Load));
            }
        }

        public static void CleanUp()
        {
            if (_imHeaderService != null)
                _imHeaderService.Dispose();
        }
        #endregion

        #region Header

        #region Fields
        private int _totalNumberOfItemsMovement;
        private ICommand _addNewItemsMovementCommand, _saveItemsMovementCommand, 
            _postItemsMovementCommand, _unPostItemsMovementCommand;
        private IEnumerable<ItemsMovementHeaderDTO> _itemsMovementList;
        #endregion

        #region Properties
        public int TotalNumberOfItemsMovement
        {
            get { return _totalNumberOfItemsMovement; }
            set
            {
                _totalNumberOfItemsMovement = value;
                RaisePropertyChanged<int>(() => TotalNumberOfItemsMovement);
            }
        }
        public bool SaveHeaderEnability
        {
            get { return _saveHeaderEnability; }
            set
            {
                _saveHeaderEnability = value;
                RaisePropertyChanged<bool>(() => SaveHeaderEnability);
            }
        }
        public bool UnPostEnability
        {
            get { return _unPostEnability; }
            set
            {
                _unPostEnability = value;
                RaisePropertyChanged<bool>(() => UnPostEnability);
            }
        }
        public ItemsMovementHeaderDTO SelectedItemsMovement
        {
            get { return _selectedItemsMovement; }
            set
            {
                _selectedItemsMovement = value;
                RaisePropertyChanged<ItemsMovementHeaderDTO>(() => SelectedItemsMovement);
                if (SelectedItemsMovement == null) return;

                SelectedFromWarehouse = Warehouses.FirstOrDefault(w => w.Id == SelectedItemsMovement.FromWarehouseId);
                SelectedToWarehouse = Warehouses.FirstOrDefault(w => w.Id == SelectedItemsMovement.ToWarehouseId);

                switch (SelectedItemsMovement.Status)
                {
                    case TransactionStatus.Posted:
                        SaveHeaderEnability = false;
                        UnPostEnability = true;
                        break;
                    case TransactionStatus.Draft:
                        SaveHeaderEnability = true;
                        UnPostEnability = false;
                        TransactionLine = new ImTransactionLineModel();
                        break;
                }
                SelectedItem = null;
                _isEdit = false;
                GetItemsMovementLines();
            }
        }
        public IEnumerable<ItemsMovementHeaderDTO> ItemsMovementList
        {
            get { return _itemsMovementList; }
            set
            {
                _itemsMovementList = value;
                RaisePropertyChanged<IEnumerable<ItemsMovementHeaderDTO>>(() => ItemsMovementList);
            }
        }
        public ObservableCollection<ItemsMovementHeaderDTO> ItemsMovements
        {
            get { return _itemsMovements; }
            set
            {
                _itemsMovements = value;
                RaisePropertyChanged<ObservableCollection<ItemsMovementHeaderDTO>>(() => ItemsMovements);
                GetSummary();
                AddNewItemsMovementHeader();
            }
        }
        public void GetSummary()
        {
            TotalNumberOfItemsMovement = ItemsMovements.Count();
        }
        public void GetLiveItemsMovements()
        {
            var criteria = new SearchCriteria<ItemsMovementHeaderDTO>
            {
                CurrentUserId = Singleton.User.UserId,
                BeginingDate = FilterStartDate,
                EndingDate = FilterEndDate
            };

            //if (SelectedFromWarehouse != null && SelectedFromWarehouse.Id != -1)
            //criteria.SelectedWarehouseId = SelectedFromWarehouse.Id;

            var imList = _imHeaderService.GetAll(criteria).ToList();

            //imList = imList.Where(im => im.ShowLines).ToList();

            ItemsMovements = new ObservableCollection<ItemsMovementHeaderDTO>(imList);
        }
        #endregion

        #region Commands
        public ICommand AddNewItemsMovementCommand
        {
            get
            {
                return _addNewItemsMovementCommand ?? (_addNewItemsMovementCommand = new RelayCommand(AddNewItemsMovementHeader));
            }
        }
        private void AddNewItemsMovementHeader()
        {
            SelectedFromWarehouse = Warehouses.FirstOrDefault();
            SelectedToWarehouse = Warehouses.FirstOrDefault();

            SelectedItemsMovement = new ItemsMovementHeaderDTO
                          {
                              MovementDate = DateTime.Now,
                              Status = TransactionStatus.Draft,
                              FromWarehouseId = -1,
                              ToWarehouseId = -1,
                              MovementNumber = _imHeaderService.GetNewMovementNumber(1, false)
                          };

            ItemsMovementLines = new ObservableCollection<ItemsMovementLineDTO>();
        }

        public ICommand SaveItemsMovementCommand
        {
            get
            {
                return _saveItemsMovementCommand ?? (_saveItemsMovementCommand = new RelayCommand(SaveItemsMovementHeader, CanSave));
            }
        }
        private void SaveItemsMovementHeader()
        {
            try
            {
                if (SelectedFromWarehouse.Id == -1 || SelectedToWarehouse.Id == -1)
                {
                    MessageBox.Show("Choose From and To stores/shops first...");
                    return;
                }
                if (SelectedFromWarehouse.Id == SelectedToWarehouse.Id)
                {
                    MessageBox.Show("From store/shop is same as To store/shop...");
                    return;
                }
                SelectedItemsMovement.FromWarehouseId = SelectedFromWarehouse.Id;
                SelectedItemsMovement.ToWarehouseId = SelectedToWarehouse.Id;

                var newObject = SelectedItemsMovement.Id;

                var stat = _imHeaderService.InsertOrUpdate(SelectedItemsMovement);
                if (stat != string.Empty)
                    MessageBox.Show("Can't save"
                                    + Environment.NewLine + stat, "Can't save", MessageBoxButton.OK,
                        MessageBoxImage.Error);

                else if (newObject == 0)
                    ItemsMovements.Insert(0, SelectedItemsMovement);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't save"
                                  + Environment.NewLine + exception.Message, "Can't save", MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }
        }

        public ICommand PostItemsMovementCommand
        {
            get
            {
                return _postItemsMovementCommand ?? (_postItemsMovementCommand = new RelayCommand(PostItemsMovementHeader));
            }
        }
        private void PostItemsMovementHeader()
        {
            try
            {
                var stat = _imHeaderService.Post(SelectedItemsMovement);
                if (stat != string.Empty)
                    MessageBox.Show("Can't post"
                                    + Environment.NewLine + stat, "Can't post", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                else
                    AddNewItemsMovementHeader();

            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't post"
                                  + Environment.NewLine + exception.Message, "Can't post", MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }
        }

        public ICommand UnPostItemsMovementCommand
        {
            get
            {
                return _unPostItemsMovementCommand ?? (_unPostItemsMovementCommand = new RelayCommand(UnPostItemsMovementHeader));
            }
        }
        private void UnPostItemsMovementHeader()
        {
            try
            {
                var stat = _imHeaderService.UnPost(SelectedItemsMovement);
                if (stat != string.Empty)
                    MessageBox.Show("Can't unPost"
                                    + Environment.NewLine + stat, "Can't unPost", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                else
                    AddNewItemsMovementHeader();

            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't unPost"
                                  + Environment.NewLine + exception.Message, "Can't unPost", MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }
        }
        public ICommand FilterByDateCommand
        {
            get { return _filterByDateCommand ?? (_filterByDateCommand = new RelayCommand(ExecuteFilterByDateCommand)); }
        }
        private void ExecuteFilterByDateCommand()
        {
            GetLiveItemsMovements();
        }
        #endregion


        #region Filter Header
        private string _filterPeriod;
        private DateTime _filterStartDate, _filterEndDate;
        private IList<ListDataItem> _filterPeriods;
        private ListDataItem _selectedPeriod;
        private IList<ListDataItem> _filterLines;
        private ListDataItem _selectedFilter;

        public string FilterPeriod
        {
            get { return _filterPeriod; }
            set
            {
                _filterPeriod = value;
                RaisePropertyChanged<string>(() => FilterPeriod);
            }
        }
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
        private void FillPeriodCombo()
        {
            FilterPeriods = new List<ListDataItem>
            {
                new ListDataItem {Display = "All", Value = 0},
                new ListDataItem {Display = "Today", Value = 1},
                new ListDataItem {Display = "Yesterday", Value = 2},
                new ListDataItem {Display = "This Week", Value = 3},
                new ListDataItem {Display = "Last Week", Value = 4}
            };
        }
        public IList<ListDataItem> FilterPeriods
        {
            get { return _filterPeriods; }
            set
            {
                _filterPeriods = value;
                RaisePropertyChanged<IList<ListDataItem>>(() => FilterPeriods);
            }
        }
        public ListDataItem SelectedPeriod
        {
            get { return _selectedPeriod; }
            set
            {
                _selectedPeriod = value;
                RaisePropertyChanged<ListDataItem>(() => SelectedPeriod);
                if (SelectedPeriod == null) return;
                switch (SelectedPeriod.Value)
                {
                    case 0:
                        FilterStartDate = DateTime.Now.AddYears(-1);
                        FilterEndDate = DateTime.Now;
                        break;
                    case 1:
                        FilterStartDate = DateTime.Now;
                        FilterEndDate = DateTime.Now;
                        break;
                    case 2:
                        FilterStartDate = DateTime.Now.AddDays(-1);
                        FilterEndDate = DateTime.Now.AddDays(-1);
                        break;
                    case 3:
                        FilterStartDate = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);
                        FilterEndDate = DateTime.Now.AddDays(7 - (int)DateTime.Now.DayOfWeek - 1);
                        break;
                    case 4:
                        FilterStartDate = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek - 7);
                        FilterEndDate = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek - 1);
                        break;
                }
            }
        }

        private void FillCombo()
        {
            FilterLines = new List<ListDataItem>
            {
                new ListDataItem {Display = "No-Filter", Value = 0},
                new ListDataItem {Display = "Equal", Value = 1},
                new ListDataItem {Display = "Not Equal", Value = 2},
                new ListDataItem {Display = "More Counted", Value = 3},
                new ListDataItem {Display = "Less Counted", Value = 4}
            };
        }

        public IList<ListDataItem> FilterLines
        {
            get { return _filterLines; }
            set
            {
                _filterLines = value;
                RaisePropertyChanged<IList<ListDataItem>>(() => FilterLines);
            }
        }
        public ListDataItem SelectedFilter
        {
            get { return _selectedFilter; }
            set
            {
                _selectedFilter = value;
                RaisePropertyChanged<ListDataItem>(() => SelectedFilter);
                FilterItemsMovementLines();
            }
        }
        private void FilterItemsMovementLines()
        {
            //if (SelectedFilter == null) return;
            //switch (SelectedFilter.Value)
            //{
            //    case 0:
            //        ItemsMovementLines = new ObservableCollection<ItemsMovementLineDTO>(ItemsMovementLinesList);
            //        break;
            //    case 1:
            //        ItemsMovementLines = new ObservableCollection<ItemsMovementLineDTO>(ItemsMovementLinesList.Where(im => im.CountedQty == im.ExpectedQty));
            //        break;
            //    case 2:
            //        ItemsMovementLines = new ObservableCollection<ItemsMovementLineDTO>(ItemsMovementLinesList.Where(im => im.CountedQty != im.ExpectedQty));
            //        break;
            //    case 3:
            //        ItemsMovementLines = new ObservableCollection<ItemsMovementLineDTO>(ItemsMovementLinesList.Where(im => im.CountedQty > im.ExpectedQty));
            //        break;
            //    case 4:
            //        ItemsMovementLines = new ObservableCollection<ItemsMovementLineDTO>(ItemsMovementLinesList.Where(im => im.CountedQty < im.ExpectedQty));
            //        break;

            //}
        }
        #endregion

        #endregion

        #region Lines

        #region Fields
        private bool _isEdit;
        private ObservableCollection<ItemsMovementLineDTO> _physicalInventoryLines;
        private ItemsMovementLineDTO _selectedItemsMovementLine;
        private IEnumerable<ItemsMovementLineDTO> _physicalInventoryLinesList;
        private ICommand _addItemsMovementLineCommand, _editItemsMovementLineCommand, _deleteItemsMovementLineCommand;
        #endregion

        #region Public Properties
        public int TotalItemsCounted
        {
            get { return _totalItemsCounted; }
            set
            {
                _totalItemsCounted = value;
                RaisePropertyChanged<int>(() => TotalItemsCounted);
            }
        }
        public ItemsMovementLineDTO SelectedItemsMovementLine
        {
            get { return _selectedItemsMovementLine; }
            set
            {
                _selectedItemsMovementLine = value;
                RaisePropertyChanged<ItemsMovementLineDTO>(() => SelectedItemsMovementLine);
            }
        }
        public ObservableCollection<ItemsMovementLineDTO> ItemsMovementLines
        {
            get { return _physicalInventoryLines; }
            set
            {
                _physicalInventoryLines = value;
                RaisePropertyChanged<ObservableCollection<ItemsMovementLineDTO>>(() => ItemsMovementLines);

                TotalItemsCounted = ItemsMovementLines.Count();

                var lineCounts = ItemsMovementLines.Count;
                if (SelectedItemsMovement != null)
                {
                    SelectedItemsMovement.CountLines = lineCounts;
                }
                TotalItemsCounted = lineCounts;
            }
        }
        public IEnumerable<ItemsMovementLineDTO> ItemsMovementLinesList
        {
            get { return _physicalInventoryLinesList; }
            set
            {
                _physicalInventoryLinesList = value;
                RaisePropertyChanged<IEnumerable<ItemsMovementLineDTO>>(() => ItemsMovementLinesList);
            }
        }
        #endregion

        public void GetItemsMovementLines()
        {
            ItemsMovementLinesList = new List<ItemsMovementLineDTO>();

            if (SelectedItemsMovement != null && SelectedItemsMovement.Id != 0)
                ItemsMovementLinesList = _imHeaderService.GetChilds(SelectedItemsMovement.Id, false);

            ItemsMovementLines = new ObservableCollection<ItemsMovementLineDTO>(ItemsMovementLinesList);
        }

        #region Commands

        public ICommand AddItemsMovementLineCommand
        {
            get
            {
                return _addItemsMovementLineCommand ?? (_addItemsMovementLineCommand = new RelayCommand(SaveLine, CanSaveLine));
            }
        }
        private void SaveLine()
        {
            try
            {
                SaveItemsMovementHeader();

                if (SelectedItemsMovement == null || SelectedItem == null ||
                    SelectedItemsMovement.Status != TransactionStatus.Draft)
                {
                    MessageBox.Show("Can't add item, try again later....");
                    return;
                }

                var newObject = SelectedItemsMovement.Id;

                if (_isEdit == false)
                    SelectedItemsMovementLine = new ItemsMovementLineDTO
                    {
                        ItemsMovementHeader = SelectedItemsMovement
                    };


                SelectedItemsMovementLine.ItemId = TransactionLine.Item.Id;
                SelectedItemsMovementLine.MovedQuantity = TransactionLine.ItemMovedQuantity;
                SelectedItemsMovementLine.OriginPreviousQuantity = TransactionLine.ItemOriginQuantity;
                SelectedItemsMovementLine.DestinationPreviousQuantity = TransactionLine.ItemDestinationQuantity;

                var stat = _imHeaderService.InsertOrUpdateChild(SelectedItemsMovementLine);
                if (stat == string.Empty)
                {
                    if (newObject == 0)
                        ItemsMovements.Insert(0, SelectedItemsMovement);

                    _isEdit = false;
                    GetSummary();
                    SelectedItemsMovement = SelectedItemsMovement;
                }
                else
                    MessageBox.Show("Problem adding physical Inventory item, please try again..." + Environment.NewLine +
                                        stat);

            }
            catch (Exception exception)
            {
                MessageBox.Show("Problem adding item, please try again..." + Environment.NewLine +
                    exception.Message + Environment.NewLine +
                    exception.InnerException);
            }
        }

        public ICommand EditItemsMovementLineCommand
        {
            get
            {
                return _editItemsMovementLineCommand ?? (_editItemsMovementLineCommand = new RelayCommand(EditLine, CanSave));
            }
        }
        private void EditLine()
        {
            if (SelectedItemsMovementLine == null || SelectedItemsMovementLine.Id == 0 ||
                SelectedItemsMovement.Status != TransactionStatus.Draft)
            {
                MessageBox.Show("First choose Item to edit...", "Problem Editing");
                return;
            }

            try
            {
                _isEdit = true;

                var item = Items.FirstOrDefault(i => i.Id == SelectedItemsMovementLine.ItemId);
                if (item == null)
                {
                    MessageBox.Show("Can't Edit Item, May be item doesn't exist  in the store...");
                    return;
                }

                if (!_itemRepeated)
                    SelectedItem = item;

                TransactionLine = new ImTransactionLineModel
                {
                    Item = item,
                    ItemMovedQuantity = SelectedItemsMovementLine.MovedQuantity,
                    ItemOriginQuantity = SelectedItemsMovementLine.OriginPreviousQuantity,
                    ItemDestinationQuantity = SelectedItemsMovementLine.DestinationPreviousQuantity
                };

                _itemRepeated = false;
            }
            catch
            {
                MessageBox.Show("Can't Edit Item please try again...");
            }
        }

        public ICommand DeleteItemsMovementLineCommand
        {
            get
            {
                return _deleteItemsMovementLineCommand ?? (_deleteItemsMovementLineCommand = new RelayCommand(DeleteLine));
            }
        }
        private void DeleteLine()
        {
            if (MessageBox.Show("Are you Sure You want to Delete this Item?", "Delete Item", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    SelectedItemsMovementLine.Enabled = false;
                    var stat = _imHeaderService.DisableChild(SelectedItemsMovementLine);
                    if (stat == string.Empty)
                    {
                        ItemsMovementLines.Remove(SelectedItemsMovementLine);
                        _isEdit = false;
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

        #endregion

        #region Warehouses
        private IEnumerable<WarehouseDTO> _warehouses;
        private WarehouseDTO _selectedFromWarehouse, _selectedToWarehouse;

        public IEnumerable<WarehouseDTO> Warehouses
        {
            get { return _warehouses; }
            set
            {
                _warehouses = value;
                RaisePropertyChanged<IEnumerable<WarehouseDTO>>(() => Warehouses);
            }
        }

        public WarehouseDTO SelectedFromWarehouse
        {
            get { return _selectedFromWarehouse; }
            set
            {
                _selectedFromWarehouse = value;
                RaisePropertyChanged<WarehouseDTO>(() => SelectedFromWarehouse);
                if (SelectedFromWarehouse != null && SelectedFromWarehouse.Id != -1)
                    GetLiveItemsQuantity();
            }
        }
        public WarehouseDTO SelectedToWarehouse
        {
            get { return _selectedToWarehouse; }
            set
            {
                _selectedToWarehouse = value;
                RaisePropertyChanged<WarehouseDTO>(() => SelectedToWarehouse);
            }
        }

        public void GetWarehouses()
        {
            var warehouses = new WarehouseService(true).GetAll().ToList();
            warehouses.Insert(0, new WarehouseDTO
            {
                Id = -1,
                DisplayName = "Choose Store/Shop"
            });
            Warehouses = warehouses;
            SelectedFromWarehouse = Warehouses.FirstOrDefault();
            SelectedToWarehouse = Warehouses.FirstOrDefault();
        }
        #endregion

        #region Items

        #region Fields
        private ImTransactionLineModel _transactionLine;
        private ItemDTO _selectedItem;
        private IEnumerable<ItemDTO> _itemsList;
        private ObservableCollection<ItemDTO> _items;
        private ICommand _addNewItemCommand;
        private bool _itemRepeated;
        #endregion

        #region Public Properties
        public ImTransactionLineModel TransactionLine
        {
            get { return _transactionLine; }
            set
            {
                _transactionLine = value;
                RaisePropertyChanged<ImTransactionLineModel>(() => TransactionLine);
            }
        }
        public ItemDTO SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                RaisePropertyChanged<ItemDTO>(() => SelectedItem);
                _itemRepeated = false;
                if (SelectedItem != null)
                {
                    if (!_isEdit)
                        foreach (var line in ItemsMovementLines.Where(line => line.ItemId == SelectedItem.Id))
                        {
                            if (MessageBox.Show("The item (" + line.Item.DisplayName + ") with counted qty of " + line.MovedQuantity +
                                                   " is already in the list," + Environment.NewLine +
                                                   " Do you want to update it?", "Edit Line", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.No) !=
                                                   MessageBoxResult.Yes)
                            {
                                _isEdit = false;
                                SelectedItem = null;
                                return;
                            }
                            SelectedItemsMovementLine = line;
                            _itemRepeated = true;
                            EditLine();
                            return;
                        }

                    var itqorigin = new ItemQuantityService(true).GetByCriteria(SelectedFromWarehouse.Id, SelectedItem.Id);
                    var itqdest = new ItemQuantityService(true).GetByCriteria(SelectedToWarehouse.Id, SelectedItem.Id);

                    TransactionLine = new ImTransactionLineModel
                    {
                        Item = SelectedItem,
                        ItemOriginQuantity = itqorigin == null ? 0 : itqorigin.QuantityOnHand,
                        ItemDestinationQuantity = itqdest == null ? 0 : itqdest.QuantityOnHand
                    };
                }
                else
                {
                    TransactionLine = new ImTransactionLineModel();
                }

            }
        }
        public IEnumerable<ItemDTO> ItemsList
        {
            get { return _itemsList; }
            set
            {
                _itemsList = value;
                RaisePropertyChanged<IEnumerable<ItemDTO>>(() => ItemsList);
            }
        }
        public ObservableCollection<ItemDTO> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged<ObservableCollection<ItemDTO>>(() => Items);
            }
        }

        #endregion

        //public void LoadItems()
        //{
        //    var itemsList = new ItemService(true)
        //        .GetAll()
        //        .OrderBy(i => i.Id);
        //    Items = new ObservableCollection<ItemDTO>(itemsList);
        //}
        public void GetLiveItemsQuantity()
        {
            var criteria = new SearchCriteria<ItemQuantityDTO>
            {
                CurrentUserId = Singleton.User.UserId
            };

            if (SelectedFromWarehouse != null && SelectedFromWarehouse.Id != -1)
                criteria.SelectedWarehouseId = SelectedFromWarehouse.Id;

            criteria.FiList.Add(p => p.QuantityOnHand > 0);

            int totalCount;
            var itemQuantityList = new ItemQuantityService(true)
                .GetAll(criteria, out totalCount)
                .OrderBy(p => p.Id)
                .ToList();

            IList<ItemDTO> itemsList = itemQuantityList.Select(itemQuantityDTO => new ItemDTO
            {
                DisplayName = itemQuantityDTO.Item.DisplayName,
                ItemCode = itemQuantityDTO.Item.ItemCode,
                Id = itemQuantityDTO.ItemId
            }).ToList();

            Items = new ObservableCollection<ItemDTO>(itemsList);
        }
        #region Commands
        public ICommand AddNewItemCommand
        {
            get
            {
                return _addNewItemCommand ?? (_addNewItemCommand = new RelayCommand(ExcuteAddNewCommand));
            }
        }
        private void ExcuteAddNewCommand()
        {
            var itemWindow = new ItemDetail(null, SelectedFromWarehouse, Visibility.Collapsed);
            itemWindow.ShowDialog();
            var dialogueResult = itemWindow.DialogResult;
            if (dialogueResult == null || !(bool)dialogueResult) return;
            GetLiveItemsQuantity();
            //Items = new ObservableCollection<ItemDTO>(ItemsList);
            SelectedItem = Items.FirstOrDefault(i => i.ItemCode == itemWindow.TxtItemCode.Text);
        }
        #endregion

        #endregion

        #region Validation
        public static int Errors { get; set; }
        public bool CanSave()
        {
            return Errors == 0;
        }

        public static int LineErrors { get; set; }
        public bool CanSaveLine()
        {
            return LineErrors == 0;
        }
        #endregion
    }
}
