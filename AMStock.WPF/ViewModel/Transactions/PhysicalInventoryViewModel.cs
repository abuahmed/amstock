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
    public class PhysicalInventoryViewModel : ViewModelBase
    {
        #region Fields
        private static IPiHeaderService _piHeaderService;
        private ObservableCollection<PhysicalInventoryHeaderDTO> _physicalInventories;
        private PhysicalInventoryHeaderDTO _selectedPhysicalInventory;
        private ICommand _refreshWindowCommand, _filterByDateCommand;
        private int _totalItemsCounted;
        private bool _loadData, _saveHeaderEnability;
        #endregion

        #region Constructor
        public PhysicalInventoryViewModel()
        {
            FillCombo();
            FillPeriodCombo();

            SelectedPeriod = FilterPeriods.FirstOrDefault(p => p.Value == 0);//Value == 0(show all) Value=3(show this week)
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
            _piHeaderService = new PiHeaderService();

            LoadItems();

            GetWarehouses();
            if (Warehouses.Any())
                SelectedWarehouse = Warehouses.FirstOrDefault(w => w.IsDefault) ?? Warehouses.FirstOrDefault();
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
            if (_piHeaderService != null)
                _piHeaderService.Dispose();
        }
        #endregion

        #region Header

        #region Fields
        private int _totalNumberOfPi;
        private ICommand _addNewPiCommand, _savePiCommand, _postPiCommand;
        private bool _addNewPiEnability;
        private IEnumerable<PhysicalInventoryHeaderDTO> _physicalInventoryList;
        #endregion

        #region Properties
        public int TotalNumberOfPi
        {
            get { return _totalNumberOfPi; }
            set
            {
                _totalNumberOfPi = value;
                RaisePropertyChanged<int>(() => TotalNumberOfPi);
            }
        }
        public bool AddNewPiEnability
        {
            get { return _addNewPiEnability; }
            set
            {
                _addNewPiEnability = value;
                RaisePropertyChanged<bool>(() => AddNewPiEnability);
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

        public PhysicalInventoryHeaderDTO SelectedPi
        {
            get { return _selectedPhysicalInventory; }
            set
            {
                _selectedPhysicalInventory = value;
                RaisePropertyChanged<PhysicalInventoryHeaderDTO>(() => SelectedPi);
                if (SelectedPi == null) return;

                switch (SelectedPi.Status)
                {
                    case PhysicalInventoryStatus.Posted:
                        SaveHeaderEnability = false;
                        break;
                    case PhysicalInventoryStatus.Draft:
                        SaveHeaderEnability = true;
                        TransactionLine = new PiTransactionLineModel();
                        break;
                }
                SelectedItem = null;
                _isEdit = false;
                GetPiLines();
            }
        }
        public IEnumerable<PhysicalInventoryHeaderDTO> PiList
        {
            get { return _physicalInventoryList; }
            set
            {
                _physicalInventoryList = value;
                RaisePropertyChanged<IEnumerable<PhysicalInventoryHeaderDTO>>(() => PiList);
            }
        }
        public ObservableCollection<PhysicalInventoryHeaderDTO> PhysicalInventories
        {
            get { return _physicalInventories; }
            set
            {
                _physicalInventories = value;
                RaisePropertyChanged<ObservableCollection<PhysicalInventoryHeaderDTO>>(() => PhysicalInventories);
                GetSummary();
                AddNewPiHeader();
            }
        }
        #endregion

        public void GetSummary()
        {
            TotalNumberOfPi = PhysicalInventories.Count();
        }

        #region Commands
        public ICommand AddNewPhysicalInventoryCommand
        {
            get
            {
                return _addNewPiCommand ?? (_addNewPiCommand = new RelayCommand(AddNewPiHeader));
            }
        }
        private void AddNewPiHeader()
        {
            if (SelectedWarehouse != null && SelectedWarehouse.Id == -1) return;
            SelectedPi = null;
            if (SelectedWarehouse != null)
            {
                SelectedPi = new PhysicalInventoryHeaderDTO
                {
                    PhysicalInventoryDate = DateTime.Now,
                    Status = PhysicalInventoryStatus.Draft,
                    WarehouseId = SelectedWarehouse.Id,
                    PhysicalInventoryNumber =
                        _piHeaderService.GetNewPhysicalInventoryNumber(SelectedWarehouse.Id, false)
                };

                PhysicalInventoryLines = new ObservableCollection<PhysicalInventoryLineDTO>();
            }
        }

        public ICommand SavePhysicalInventoryCommand
        {
            get
            {
                return _savePiCommand ?? (_savePiCommand = new RelayCommand(SavePiHeader, CanSave));
            }
        }
        private void SavePiHeader()
        {
            try
            {
                var newObject = SelectedPi.Id;

                var stat = _piHeaderService.InsertOrUpdate(SelectedPi);
                if (stat != string.Empty)
                    MessageBox.Show("Can't save"
                                    + Environment.NewLine + stat, "Can't save", MessageBoxButton.OK,
                        MessageBoxImage.Error);

                else if (newObject == 0)
                    PhysicalInventories.Insert(0, SelectedPi);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't save"
                                  + Environment.NewLine + exception.Message, "Can't save", MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }
        }

        public ICommand PostPhysicalInventoryCommand
        {
            get
            {
                return _postPiCommand ?? (_postPiCommand = new RelayCommand(PostPiHeader));
            }
        }
        private void PostPiHeader()
        {
            try
            {
                var stat = _piHeaderService.Post(SelectedPi);
                if (stat != string.Empty)
                    MessageBox.Show("Can't post"
                                    + Environment.NewLine + stat, "Can't post", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                else
                    AddNewPiHeader();

            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't post"
                                  + Environment.NewLine + exception.Message, "Can't post", MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }
        }

        public ICommand FilterByDateCommand
        {
            get { return _filterByDateCommand ?? (_filterByDateCommand = new RelayCommand(ExecuteFilterByDateCommand)); }
        }
        private void ExecuteFilterByDateCommand()
        {
            GetLivePhysicalInventories();
        }
        #endregion

        public void GetLivePhysicalInventories()
        {
            var criteria = new SearchCriteria<PhysicalInventoryHeaderDTO>
            {
                CurrentUserId = Singleton.User.UserId,
                BeginingDate = FilterStartDate,
                EndingDate = FilterEndDate
            };
            if (SelectedWarehouse != null && SelectedWarehouse.Id != -1)
                criteria.SelectedWarehouseId = SelectedWarehouse.Id;

            var piList = _piHeaderService.GetAll(criteria).ToList();

            //To filter the type of pi
            piList = piList.Where(pi => pi.ShowLines).ToList();

            PhysicalInventories = new ObservableCollection<PhysicalInventoryHeaderDTO>(piList);
        }

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
                        FilterStartDate = new DateTime(2014, 1, 1);
                        FilterEndDate = new DateTime(2016, 1, 1);
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
                FilterPhysicalInventoryLines();
            }
        }
        private void FilterPhysicalInventoryLines()
        {
            if (SelectedFilter == null) return;
            switch (SelectedFilter.Value)
            {
                case 0:
                    PhysicalInventoryLines = new ObservableCollection<PhysicalInventoryLineDTO>(PiLinesList);
                    break;
                case 1:
                    PhysicalInventoryLines = new ObservableCollection<PhysicalInventoryLineDTO>(PiLinesList.Where(pi => pi.CountedQty == pi.ExpectedQty));
                    break;
                case 2:
                    PhysicalInventoryLines = new ObservableCollection<PhysicalInventoryLineDTO>(PiLinesList.Where(pi => pi.CountedQty != pi.ExpectedQty));
                    break;
                case 3:
                    PhysicalInventoryLines = new ObservableCollection<PhysicalInventoryLineDTO>(PiLinesList.Where(pi => pi.CountedQty > pi.ExpectedQty));
                    break;
                case 4:
                    PhysicalInventoryLines = new ObservableCollection<PhysicalInventoryLineDTO>(PiLinesList.Where(pi => pi.CountedQty < pi.ExpectedQty));
                    break;

            }
        }
        #endregion

        #endregion

        #region Lines

        #region Fields
        private bool _isEdit;
        private ObservableCollection<PhysicalInventoryLineDTO> _physicalInventoryLines;
        private PhysicalInventoryLineDTO _selectedPhysicalInventoryLine;
        private IEnumerable<PhysicalInventoryLineDTO> _physicalInventoryLinesList;
        private ICommand _addPiLineCommand, _editPiLineCommand, _deletePiLineCommand;
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
        public PhysicalInventoryLineDTO SelectedPiLine
        {
            get { return _selectedPhysicalInventoryLine; }
            set
            {
                _selectedPhysicalInventoryLine = value;
                RaisePropertyChanged<PhysicalInventoryLineDTO>(() => SelectedPiLine);


            }
        }
        public ObservableCollection<PhysicalInventoryLineDTO> PhysicalInventoryLines
        {
            get { return _physicalInventoryLines; }
            set
            {
                _physicalInventoryLines = value;
                RaisePropertyChanged<ObservableCollection<PhysicalInventoryLineDTO>>(() => PhysicalInventoryLines);

                //TotalItemsCounted = PhysicalInventoryLines.Count();

                var lineCounts = PhysicalInventoryLines.Count;
                if (SelectedPi != null)
                {
                    SelectedPi.CountLines = lineCounts;
                }
                TotalItemsCounted = lineCounts;
            }
        }
        public IEnumerable<PhysicalInventoryLineDTO> PiLinesList
        {
            get { return _physicalInventoryLinesList; }
            set
            {
                _physicalInventoryLinesList = value;
                RaisePropertyChanged<IEnumerable<PhysicalInventoryLineDTO>>(() => PiLinesList);
            }
        }
        #endregion

        public void GetPiLines()
        {
            PiLinesList = new List<PhysicalInventoryLineDTO>();

            if (SelectedPi != null && SelectedPi.Id != 0)
            {
                PiLinesList = _piHeaderService.GetChilds(SelectedPi.Id, false);
            }
            PiLinesList = PiLinesList
                .OrderBy(i => i.Item != null ? i.Item.DisplayName : null)
                .ThenBy(i => i.Item != null ? i.Item.ItemCode : null);
            PhysicalInventoryLines = new ObservableCollection<PhysicalInventoryLineDTO>(PiLinesList);
        }

        #region Commands

        public ICommand AddPiLineCommand
        {
            get
            {
                return _addPiLineCommand ?? (_addPiLineCommand = new RelayCommand(SaveLine, CanSaveLine));
            }
        }
        private void SaveLine()
        {
            try
            {
                if (SelectedPi == null || SelectedItem == null ||
                    SelectedPi.Status != PhysicalInventoryStatus.Draft)
                {
                    MessageBox.Show("Can't add item, try again later....");
                    return;
                }

                var newObject = SelectedPi.Id;

                if (_isEdit == false)
                    SelectedPiLine = new PhysicalInventoryLineDTO()
                    {
                        PhysicalInventory = SelectedPi,
                        PhysicalInventoryLineType = PhysicalInventoryLineTypes.AfterPi
                    };


                SelectedPiLine.ItemId = TransactionLine.Item.Id;
                SelectedPiLine.CountedQty = TransactionLine.ItemCountedQuantity;
                SelectedPiLine.ExpectedQty = TransactionLine.ItemCurrentQuantity;


                var stat = _piHeaderService.InsertOrUpdateChild(SelectedPiLine);
                if (stat == string.Empty)
                {
                    if (newObject == 0)
                        PhysicalInventories.Insert(0, SelectedPi);

                    _isEdit = false;
                    GetSummary();
                    SelectedPi = SelectedPi;
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

        public ICommand EditPiLineCommand
        {
            get
            {
                return _editPiLineCommand ?? (_editPiLineCommand = new RelayCommand(EditLine, CanSave));
            }
        }
        private void EditLine()
        {
            if (SelectedPiLine == null || SelectedPiLine.Id == 0 ||
                SelectedPi.Status != PhysicalInventoryStatus.Draft)
            {
                MessageBox.Show("First choose Item to edit...", "Problem Editing");
                return;
            }

            try
            {
                _isEdit = true;

                var item = Items.FirstOrDefault(i => i.Id == SelectedPiLine.ItemId);
                if (item == null)
                {
                    MessageBox.Show("Can't Edit Item, May be item doesn't exist  in the store...");
                    return;
                }

                if (!_itemRepeated)
                    SelectedItem = item;

                TransactionLine = new PiTransactionLineModel
                {
                    Item = item,
                    ItemCountedQuantity = SelectedPiLine.CountedQty,
                    ItemCurrentQuantity = SelectedPiLine.ExpectedQty
                };

                _itemRepeated = false;
            }
            catch
            {
                MessageBox.Show("Can't Edit Item please try again...");
            }
        }

        public ICommand DeletePiLineCommand
        {
            get
            {
                return _deletePiLineCommand ?? (_deletePiLineCommand = new RelayCommand(DeleteLine));
            }
        }
        private void DeleteLine()
        {
            if (MessageBox.Show("Are you Sure You want to Delete this Item?", "Delete Item", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    SelectedPiLine.Enabled = false;
                    var stat = _piHeaderService.DisableChild(SelectedPiLine);
                    if (stat == string.Empty)
                    {
                        PhysicalInventoryLines.Remove(SelectedPiLine);
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
        private WarehouseDTO _selectedWarehouse;

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
                    AddNewPiEnability = SelectedWarehouse.Id != -1;
                    SaveHeaderEnability = SelectedWarehouse.Id != -1;
                    GetLivePhysicalInventories();
                }
            }
        }

        public void GetWarehouses()
        {
            Warehouses = Singleton.WarehousesList;
        }
        #endregion

        #region Items

        #region Fields
        private PiTransactionLineModel _transactionLine;
        private ItemDTO _selectedItem;
        private bool _itemRepeated;
        private IEnumerable<ItemDTO> _itemsList;
        private ObservableCollection<ItemDTO> _items;
        private ICommand _addNewItemCommand;
        #endregion

        #region Public Properties
        public PiTransactionLineModel TransactionLine
        {
            get { return _transactionLine; }
            set
            {
                _transactionLine = value;
                RaisePropertyChanged<PiTransactionLineModel>(() => TransactionLine);
                if (TransactionLine != null)
                {
                }
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
                        foreach (var line in PhysicalInventoryLines.Where(line => line.ItemId == SelectedItem.Id))
                        {
                            if (MessageBox.Show("The item (" + line.Item.DisplayName + ") with counted qty of " + line.CountedQty +
                                                   " is already in the list," + Environment.NewLine +
                                                   " Do you want to update it?", "Edit Line", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.No) !=
                                                   MessageBoxResult.Yes)
                            {
                                _isEdit = false;
                                SelectedItem = null;
                                return;
                            }
                            SelectedPiLine = line;
                            _itemRepeated = true;
                            EditLine();
                            return;
                        }

                    var itq = new ItemQuantityService(true).GetByCriteria(SelectedWarehouse.Id, SelectedItem.Id);
                    TransactionLine = new PiTransactionLineModel()
                    {
                        Item = SelectedItem,
                        ItemCurrentQuantity = itq == null ? 0 : itq.QuantityOnHand
                    };
                }
                else
                {
                    TransactionLine = new PiTransactionLineModel();
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

        public void LoadItems()
        {
            var itemsList = new ItemService(true).GetAll().OrderBy(i => i.Id);
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
            var itemWindow = new ItemDetail(null, SelectedWarehouse, Visibility.Collapsed);
            itemWindow.ShowDialog();
            var dialogueResult = itemWindow.DialogResult;
            if (dialogueResult == null || !(bool)dialogueResult) return;
            LoadItems();
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
