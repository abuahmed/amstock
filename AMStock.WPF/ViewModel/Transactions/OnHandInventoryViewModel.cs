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
using AMStock.WPF.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using Button = System.Windows.Controls.Button;
using Excel = Microsoft.Office.Interop.Excel;
using MessageBox = System.Windows.MessageBox;

namespace AMStock.WPF.ViewModel
{
    public class OnHandInventoryViewModel : ViewModelBase
    {
        #region Fields
        private static IItemQuantityService _itemQuantityService;
        private ICommand _refreshWindowCommand, _itemViewCommand;
        private IEnumerable<WarehouseDTO> _warehouses;
        private WarehouseDTO _selectedWarehouse;
        private int _totalNumberOfItemQuantityTypes;
        private string _totalValueOfItemsQuantity, _totalPurchaseValueOfItemsQuantity, _profitOfItemsQuantity;
        private string _searchText, _totalNumberOfItemsQuantity, _textSearchMode;
        private bool _itemDetailEnability, _startsWithCheckBox;
        private bool _onlyOnOrderChecked, _onlyOnDeliveryChecked, _onlyOnReservationChecked, _loadData;
        #endregion

        #region Constructor

        public OnHandInventoryViewModel()
        {
            FillByQuantityCombo();
            GetWarehouses();
        }

        private void Load()
        {
            CleanUp();
            _itemQuantityService = new ItemQuantityService();

            LoadCategories();
            ItemDetailEnability = false;

            if (Warehouses != null && Warehouses.Any())
            {
                if (SelectedWarehouse == null)
                    SelectedWarehouse = Warehouses.FirstOrDefault(w => w.IsDefault) ?? Warehouses.FirstOrDefault();
                else
                    SelectedWarehouse = SelectedWarehouse;
            }

            CheckRoles();
            TextSearchMode = "Contains";
        }
        public static void CleanUp()
        {
            if (_itemQuantityService != null)
                _itemQuantityService.Dispose();
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
        #endregion

        #region Properties
        public string TextSearchMode
        {
            get { return _textSearchMode; }
            set
            {
                _textSearchMode = value;
                RaisePropertyChanged<string>(() => this.TextSearchMode);
            }
        }
        public bool StartsWithCheckBox
        {
            get { return _startsWithCheckBox; }
            set
            {
                _startsWithCheckBox = value;
                RaisePropertyChanged<bool>(() => this.StartsWithCheckBox);
                TextSearchMode = StartsWithCheckBox ? "StartsWith" : "Contains";
            }
        }
        public string TotalNumberOfItemsQuantity
        {
            get { return _totalNumberOfItemsQuantity; }
            set
            {
                _totalNumberOfItemsQuantity = value;
                RaisePropertyChanged<string>(() => TotalNumberOfItemsQuantity);
            }
        }
        public string TotalValueOfItemsQuantity
        {
            get { return _totalValueOfItemsQuantity; }
            set
            {
                _totalValueOfItemsQuantity = value;
                RaisePropertyChanged<string>(() => TotalValueOfItemsQuantity);
            }
        }
        public string TotalPurchaseValueOfItemsQuantity
        {
            get { return _totalPurchaseValueOfItemsQuantity; }
            set
            {
                _totalPurchaseValueOfItemsQuantity = value;
                RaisePropertyChanged<string>(() => TotalPurchaseValueOfItemsQuantity);
            }
        }
        public string ProfitOfItemsQuantity
        {
            get { return _profitOfItemsQuantity; }
            set
            {
                _profitOfItemsQuantity = value;
                RaisePropertyChanged<string>(() => ProfitOfItemsQuantity);
            }
        }
        public int TotalNumberOfItemQuantityTypes
        {
            get { return _totalNumberOfItemQuantityTypes; }
            set
            {
                _totalNumberOfItemQuantityTypes = value;
                RaisePropertyChanged<int>(() => TotalNumberOfItemQuantityTypes);
            }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                RaisePropertyChanged<string>(() => SearchText);
                if (ItemQuantityList != null)
                {
                    if (!string.IsNullOrEmpty(SearchText))
                    {
                        switch (TextSearchMode)
                        {
                            case "Contains":
                                ItemsQuantity = new ObservableCollection<ItemQuantityDTO>
                                    (ItemQuantityList.Where(c => c.Item.ItemCode.ToLower().Contains(value.ToLower()))
                                        .ToList());
                                break;
                            case "StartsWith":
                                ItemsQuantity = new ObservableCollection<ItemQuantityDTO>
                                    (ItemQuantityList.Where(c => c.Item.ItemCode.ToLower().StartsWith(value.ToLower()))
                                        .ToList());
                                break;
                        }
                    }
                    else
                        ItemsQuantity = new ObservableCollection<ItemQuantityDTO>(ItemQuantityList);
                }
                SetSummary();
            }
        }

        public bool ItemDetailEnability
        {
            get { return _itemDetailEnability; }
            set
            {
                _itemDetailEnability = value;
                RaisePropertyChanged<bool>(() => ItemDetailEnability);
            }
        }
        public bool OnlyOnOrderChecked
        {
            get { return _onlyOnOrderChecked; }
            set
            {
                _onlyOnOrderChecked = value;
                RaisePropertyChanged<bool>(() => OnlyOnOrderChecked);
                GetLiveItemsQuantity();
            }
        }
        public bool OnlyOnDeliveryChecked
        {
            get { return _onlyOnDeliveryChecked; }
            set
            {
                _onlyOnDeliveryChecked = value;
                RaisePropertyChanged<bool>(() => OnlyOnDeliveryChecked);
                GetLiveItemsQuantity();
            }
        }
        public bool OnlyOnReservationChecked
        {
            get { return _onlyOnReservationChecked; }
            set
            {
                _onlyOnReservationChecked = value;
                RaisePropertyChanged<bool>(() => OnlyOnReservationChecked);
                GetLiveItemsQuantity();
            }
        }
        #endregion

        #region Warehouse
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
                MessageBox.Show(SelectedWarehouse.DisplayName +
                    " is ACTIVE",
                    SelectedWarehouse.DisplayName,
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                GetLiveItemsQuantity();

            }
        }
        public void GetWarehouses()
        {
            Warehouses = Singleton.WarehousesList;
        }
        #endregion

        #region Categories
        private CategoryDTO _selectedCategory;
        private ObservableCollection<CategoryDTO> _categories;

        public void LoadCategories()
        {
            var criteria = new SearchCriteria<CategoryDTO>();
            criteria.FiList.Add(c => c.NameType == NameTypes.Category);

            IList<CategoryDTO> categoriesList = new CategoryService(true)
                .GetAll(criteria)
                .OrderBy(i => i.DisplayName)
                .ToList();

            if (categoriesList.Count > 1)
                categoriesList.Insert(0, new CategoryDTO
                {
                    DisplayName = "All",
                    Id = -1
                });

            Categories = new ObservableCollection<CategoryDTO>(categoriesList);
        }
        public CategoryDTO SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                _selectedCategory = value;
                RaisePropertyChanged<CategoryDTO>(() => SelectedCategory);

                if (SelectedCategory == null) return;
                if (SelectedCategory.DisplayName == "All")
                {
                    GetLiveItemsQuantity();
                    return;
                }

                try
                {
                    if (ItemQuantityList != null)
                    {
                        ItemQuantityList = ItemQuantityList
                            .Where(iq => iq.Item.Category.DisplayName == SelectedCategory.DisplayName);
                        ItemsQuantity = new ObservableCollection<ItemQuantityDTO>(ItemQuantityList);
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Can't Filter By Category"
                                      + Environment.NewLine + exception.Message, "Can't Filter By Category", MessageBoxButton.OK,
                          MessageBoxImage.Error);
                }
            }
        }
        public ObservableCollection<CategoryDTO> Categories
        {
            get { return _categories; }
            set
            {
                _categories = value;
                RaisePropertyChanged<ObservableCollection<CategoryDTO>>(() => Categories);
            }
        }
        #endregion

        #region Filter by Qty
        private int _filterQuantity;
        private decimal _filterPurchasePrice, _filterSalePrice;
        private ObservableCollection<ListDataItem> _filterByQuantity, _filterByPrice;
        private ListDataItem _selectedQuantity, _selectedPurchasePrice, _selectedSalePrice;
        private ICommand _filterByQuantityViewCommand, _filterReservationsViewCommand;

        private void FillByQuantityCombo()
        {
            var filterByQuantity = new List<ListDataItem>
            {
                new ListDataItem {Display = "All", Value = 0},
                new ListDataItem {Display = "Above", Value = 1},
                new ListDataItem {Display = "Below", Value = 2},
                new ListDataItem {Display = "Equals", Value = 3},
                //new ListDataItem {Display = "Above Safe Qty", Value = 4},
                //new ListDataItem {Display = "Below Safe Qty", Value = 5},
                //new ListDataItem {Display = "Equals Safe Qty", Value = 6}
            };
            FilterByQuantity = new ObservableCollection<ListDataItem>(filterByQuantity);

            var filterByPrice = new List<ListDataItem>
            {
                new ListDataItem {Display = "All", Value = 0},
                new ListDataItem {Display = "Above", Value = 1},
                new ListDataItem {Display = "Below", Value = 2},
                new ListDataItem {Display = "Equals", Value = 3}
            };
            FilterByPrice = new ObservableCollection<ListDataItem>(filterByPrice);
        }
        public ObservableCollection<ListDataItem> FilterByQuantity
        {
            get { return _filterByQuantity; }
            set
            {
                _filterByQuantity = value;
                RaisePropertyChanged<ObservableCollection<ListDataItem>>(() => FilterByQuantity);
            }
        }
        public ObservableCollection<ListDataItem> FilterByPrice
        {
            get { return _filterByPrice; }
            set
            {
                _filterByPrice = value;
                RaisePropertyChanged<ObservableCollection<ListDataItem>>(() => FilterByPrice);
            }
        }

        public ListDataItem SelectedQuantity
        {
            get { return _selectedQuantity; }
            set
            {
                _selectedQuantity = value;
                RaisePropertyChanged<ListDataItem>(() => SelectedQuantity);
            }
        }
        public int FilterQuantity
        {
            get { return _filterQuantity; }
            set
            {
                _filterQuantity = value;
                RaisePropertyChanged<int>(() => FilterQuantity);
            }
        }

        public ListDataItem SelectedPurchasePrice
        {
            get { return _selectedPurchasePrice; }
            set
            {
                _selectedPurchasePrice = value;
                RaisePropertyChanged<ListDataItem>(() => SelectedPurchasePrice);
            }
        }
        public decimal FilterPurchasePrice
        {
            get { return _filterPurchasePrice; }
            set
            {
                _filterPurchasePrice = value;
                RaisePropertyChanged<decimal>(() => FilterPurchasePrice);
            }
        }

        public ListDataItem SelectedSalePrice
        {
            get { return _selectedSalePrice; }
            set
            {
                _selectedSalePrice = value;
                RaisePropertyChanged<ListDataItem>(() => SelectedSalePrice);
            }
        }
        public decimal FilterSalePrice
        {
            get { return _filterSalePrice; }
            set
            {
                _filterSalePrice = value;
                RaisePropertyChanged<decimal>(() => FilterSalePrice);
            }
        }

        public ICommand FilterByQuantityandPriceViewCommand
        {
            get { return _filterByQuantityViewCommand ?? (_filterByQuantityViewCommand = new RelayCommand(ExecuteFilterByQuantityViewCommand, CanSave)); }
        }
        public void ExecuteFilterByQuantityViewCommand()
        {
            int filterQty;
            decimal purchasePrice, salePrice;

            if (ItemQuantityList != null)
                ItemsQuantity = new ObservableCollection<ItemQuantityDTO>(ItemQuantityList.ToList());
            else return;

            if (SelectedQuantity != null && int.TryParse(FilterQuantity.ToString(), out filterQty))
                switch (SelectedQuantity.Value)
                {
                    #region Filter By Qty
                    case 0:
                        break;
                    case 1:
                        ItemsQuantity = new ObservableCollection<ItemQuantityDTO>(
                                        ItemsQuantity.Where(iq => iq.QuantityOnHand > filterQty).ToList());
                        break;
                    case 2:
                        ItemsQuantity = new ObservableCollection<ItemQuantityDTO>(
                                        ItemsQuantity.Where(iq => iq.QuantityOnHand < filterQty).ToList());
                        break;
                    case 3:
                        ItemsQuantity = new ObservableCollection<ItemQuantityDTO>(
                                        ItemsQuantity.Where(iq => iq.QuantityOnHand == filterQty).ToList());
                        break;
                    case 4:
                        ItemsQuantity = new ObservableCollection<ItemQuantityDTO>(
                                        ItemsQuantity.Where(iq => iq.QuantityOnHand >= iq.Item.SafeQuantity).ToList());
                        break;
                    case 5:
                        ItemsQuantity = new ObservableCollection<ItemQuantityDTO>(
                                        ItemsQuantity.Where(iq => iq.QuantityOnHand <= iq.Item.SafeQuantity).ToList());
                        break;
                    case 6:
                        ItemsQuantity = new ObservableCollection<ItemQuantityDTO>(
                                        ItemsQuantity.Where(iq => iq.QuantityOnHand == iq.Item.SafeQuantity).ToList());
                        break;
                    #endregion
                }
            if (SelectedPurchasePrice != null && decimal.TryParse(FilterPurchasePrice.ToString(), out purchasePrice))
                switch (SelectedPurchasePrice.Value)
                {
                    #region Filter By Purchase Price
                    case 0:
                        break;
                    case 1:
                        ItemsQuantity = new ObservableCollection<ItemQuantityDTO>(
                                        ItemsQuantity.Where(iq => iq.Item.PurchasePrice > purchasePrice).ToList());
                        break;
                    case 2:
                        ItemsQuantity = new ObservableCollection<ItemQuantityDTO>(
                                        ItemsQuantity.Where(iq => iq.Item.PurchasePrice < purchasePrice).ToList());
                        break;
                    case 3:
                        ItemsQuantity = new ObservableCollection<ItemQuantityDTO>(
                                        ItemsQuantity.Where(iq => iq.Item.PurchasePrice == purchasePrice).ToList());
                        break;
                    #endregion
                }
            if (SelectedSalePrice != null && decimal.TryParse(FilterSalePrice.ToString(), out salePrice))
                switch (SelectedSalePrice.Value)
                {
                    #region Filter By Sale Price
                    case 0:
                        break;
                    case 1:
                        ItemsQuantity = new ObservableCollection<ItemQuantityDTO>(
                                        ItemsQuantity.Where(iq => iq.Item.SellPrice > salePrice).ToList());
                        break;
                    case 2:
                        ItemsQuantity = new ObservableCollection<ItemQuantityDTO>(
                                        ItemsQuantity.Where(iq => iq.Item.SellPrice < salePrice).ToList());
                        break;
                    case 3:
                        ItemsQuantity = new ObservableCollection<ItemQuantityDTO>(
                                        ItemsQuantity.Where(iq => iq.Item.SellPrice == salePrice).ToList());
                        break;
                    #endregion
                }
        }

        public ICommand FilterReservationsViewCommand
        {
            get { return _filterReservationsViewCommand ?? (_filterReservationsViewCommand = new RelayCommand(ExecuteFilterReservationsViewCommand, CanSave)); }
        }

        public void ExecuteFilterReservationsViewCommand()
        {
            if (ItemsQuantity != null)
                ItemsQuantity = new ObservableCollection<ItemQuantityDTO>(
                    ItemsQuantity.Where(iq => iq.QuantityReserved > 0).ToList());
        }

        #endregion

        #region ItemQuantity

        #region Fields
        private ItemQuantityDTO _selectedItemQuantity;
        private ItemQuantityDTO _selectedItemQuantityForSearch;
        private IEnumerable<ItemQuantityDTO> _itemQuantityList;
        private ObservableCollection<ItemQuantityDTO> _itemsQuantity;
        #endregion

        #region Public Properties
        public ItemQuantityDTO SelectedItemQuantity
        {
            get { return _selectedItemQuantity; }
            set
            {
                _selectedItemQuantity = value;
                RaisePropertyChanged<ItemQuantityDTO>(() => SelectedItemQuantity);

                ItemDetailEnability = SelectedItemQuantity != null && (SelectedWarehouse != null && SelectedWarehouse.Id != -1);
            }
        }
        public ItemQuantityDTO SelectedItemQuantityForSearch
        {
            get { return _selectedItemQuantityForSearch; }
            set
            {
                _selectedItemQuantityForSearch = value;
                RaisePropertyChanged<ItemQuantityDTO>(() => SelectedItemQuantityForSearch);

                if (SelectedItemQuantityForSearch != null && SelectedItemQuantityForSearch.Item != null && !string.IsNullOrEmpty(SelectedItemQuantityForSearch.Item.ItemDetail))
                {
                    SelectedItemQuantity = SelectedItemQuantityForSearch;
                    SelectedItemQuantityForSearch.Item.ItemDetail = "";
                }
            }
        }
        public ObservableCollection<ItemQuantityDTO> ItemsQuantity
        {
            get { return _itemsQuantity; }
            set
            {
                _itemsQuantity = value;
                RaisePropertyChanged<ObservableCollection<ItemQuantityDTO>>(() => ItemsQuantity);

                SetSummary();
            }
        }
        public IEnumerable<ItemQuantityDTO> ItemQuantityList
        {
            get { return _itemQuantityList; }
            set
            {
                _itemQuantityList = value;
                RaisePropertyChanged<IEnumerable<ItemQuantityDTO>>(() => ItemQuantityList);
            }
        }
        #endregion

        public void GetLiveItemsQuantity()
        {
            try
            {
                ItemQuantityList = new List<ItemQuantityDTO>();

                var criteria = new SearchCriteria<ItemQuantityDTO>()
                {
                    CurrentUserId = Singleton.User.UserId
                };

                if (SelectedWarehouse != null && SelectedWarehouse.Id != -1)
                    criteria.SelectedWarehouseId = SelectedWarehouse.Id;

                int totalCount;
                ItemQuantityList = _itemQuantityService.GetAll(criteria, out totalCount).ToList();

                if (OnlyOnOrderChecked)
                    ItemQuantityList = ItemQuantityList.Where(q => q.QuantityOnOrder > 0).ToList();

                if (OnlyOnDeliveryChecked)
                    ItemQuantityList = ItemQuantityList.Where(q => q.QuantityOnDelivery > 0).ToList();

                if (OnlyOnReservationChecked)
                    ItemQuantityList = ItemQuantityList.Where(q => q.QuantityReserved > 0).ToList();

                //if (SelectedWarehouse != null && SelectedWarehouse.Id != -1)
                //{
                //    var items = new ItemService(true).GetAll().OrderBy(i => i.Id).ToList();
                //    var itemsQty = (from itemDTO in items
                //                    where ItemQuantityList.All(iq => iq.ItemId != itemDTO.Id)
                //                    select new ItemQuantityDTO()
                //                    {
                //                        Item = itemDTO,
                //                        QuantityOnHand = 0,
                //                        ItemId = itemDTO.Id,
                //                        Warehouse = SelectedWarehouse
                //                    }).ToList();

                //    ItemQuantityList = ItemQuantityList.Concat(itemsQty);
                //}

                ItemsQuantity = new ObservableCollection<ItemQuantityDTO>(ItemQuantityList.OrderBy(i => i.Item.CategoryId));
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't load inventory, please refresh window again..."
                                  + Environment.NewLine + exception.Message, "Can't Load Inventory", MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }

        }

        private void SetSummary()
        {
            TotalNumberOfItemQuantityTypes = ItemsQuantity.Count;
            TotalNumberOfItemsQuantity = ItemsQuantity.Sum(iq => iq != null ? iq.QuantityOnHand : 0).ToString("N0");
            TotalValueOfItemsQuantity = ItemsQuantity.Sum(iq => iq != null ? iq.TotalPrice : 0).ToString("C");
            TotalPurchaseValueOfItemsQuantity = ItemsQuantity.Sum(iq => iq != null ? iq.TotalPurchasePrice : 0).ToString("C");
            ProfitOfItemsQuantity = (ItemsQuantity.Sum(iq => iq != null ? iq.TotalPrice : 0)
                - ItemsQuantity.Sum(iq => iq != null ? iq.TotalPurchasePrice : 0)).ToString("C");
        }

        #endregion

        #region Commands
        public ICommand RefreshWindowCommand
        {
            get
            {
                return _refreshWindowCommand ?? (_refreshWindowCommand = new RelayCommand(ExcuteRefreshWIndow));
            }
        }
        private void ExcuteRefreshWIndow()
        {
            Load();
        }

        public ICommand ItemViewCommand
        {
            get { return _itemViewCommand ?? (_itemViewCommand = new RelayCommand<Object>(ExecuteItemViewCommand)); }
        }
        private void ExecuteItemViewCommand(object button)
        {
            try
            {
                var clickedbutton = button as Button;
                ItemDetail itemDetailWindow;
                bool? dialogueResult;
                if (clickedbutton == null) return;
                var buttenTag = clickedbutton.Tag.ToString();
                switch (buttenTag)
                {
                    case "AddNew":
                        itemDetailWindow = new ItemDetail(null, SelectedWarehouse);
                        itemDetailWindow.ShowDialog();
                        dialogueResult = itemDetailWindow.DialogResult;
                        if (dialogueResult != null && (bool)dialogueResult)
                        {
                            Load();
                        }
                        break;
                    case "ViewEdit":
                        itemDetailWindow = new ItemDetail(SelectedItemQuantity, SelectedWarehouse);
                        itemDetailWindow.ShowDialog();
                        dialogueResult = itemDetailWindow.DialogResult;
                        if (dialogueResult != null && (bool)dialogueResult)
                        {
                            Load();
                        }
                        break;
                    case "SalesList":
                        if (SelectedItemQuantity != null)
                        {
                            var salesDetailWindow = new TransactionItemsList(TransactionTypes.Sale, SelectedItemQuantity.Item);
                            salesDetailWindow.ShowDialog();
                        }
                        break;
                    case "PurchasesList":
                        var purchasesDetailWindow = new TransactionItemsList(TransactionTypes.Purchase, SelectedItemQuantity.Item);
                        purchasesDetailWindow.ShowDialog();
                        break;
                    case "PiList":
                        var piDetailWindow = new TransactionItemsList(TransactionTypes.Pi, SelectedItemQuantity.Item);
                        piDetailWindow.ShowDialog();
                        break;
                    case "ImList":
                        var imDetailWindow = new ItemsTransferList(SelectedItemQuantity.Item);
                        imDetailWindow.ShowDialog();
                        break;
                    case "Reserve":
                        var reserveWindow = new Reservations(SelectedItemQuantity);
                        reserveWindow.ShowDialog();
                        dialogueResult = reserveWindow.DialogResult;
                        if (dialogueResult != null && (bool)dialogueResult)
                            Load();
                        break;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't open window"
                                  + Environment.NewLine + exception.Message, "Can't open window", MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }
        }

        #endregion

        #region Validation
        public static int Errors { get; set; }
        public bool CanSave()
        {
            return Errors == 0;
        }
        #endregion

        #region Previlege Visibility
        private UserRolesModel _userRoles;

        public UserRolesModel UserRoles
        {
            get { return _userRoles; }
            set
            {
                _userRoles = value;
                RaisePropertyChanged<UserRolesModel>(() => UserRoles);
            }
        }

        private void CheckRoles()
        {
            UserRoles = Singleton.UserRoles;

            UserRoles.ReserveItems = Singleton.Setting.EnableReservations ? UserRoles.ReserveItems : "Collapsed";
        }

        #endregion

        #region Export To Excel
        private ICommand _exportToExcelCommand;
        public ICommand ExportToExcelCommand
        {
            get { return _exportToExcelCommand ?? (_exportToExcelCommand = new RelayCommand(ExecuteExportToExcelCommand)); }
        }

        private void ExecuteExportToExcelCommand()
        {
            string[] columnsHeader = {"Item Name","Purchased","Sold","Balance"};

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
            var xlApp = new Excel.Application();

            try
            {
                Excel.Workbook excelBook = xlApp.Workbooks.Add();
                var excelWorksheet = (Excel.Worksheet)excelBook.Worksheets[1];
                xlApp.Visible = true;

                int rowsTotal = ItemsQuantity.Count;
                int colsTotal = columnsHeader.Count();

                var with1 = excelWorksheet;
                with1.Cells.Select();
                with1.Cells.Delete();

                var iC = 0;
                for (iC = 0; iC <= colsTotal - 1; iC++)
                {
                    with1.Cells[1, iC + 1].Value = columnsHeader[iC];
                }

                var I = 0;
                for (I = 0; I <= rowsTotal - 1; I++)
                {
                    with1.Cells[I + 2, 0 + 1].value = ItemsQuantity[I].Item.ItemCode;
                    with1.Cells[I + 2, 1 + 1].value = ItemsQuantity[I].QuantityPurchased;
                    with1.Cells[I + 2, 2 + 1].value = ItemsQuantity[I].QuantitySold;
                    with1.Cells[I + 2, 3 + 1].value = ItemsQuantity[I].QuantityOnHand;
                }

                with1.Rows["1:1"].Font.FontStyle = "Bold";
                with1.Rows["1:1"].Font.Size = 12;

                with1.Cells.Columns.AutoFit();
                with1.Cells.Select();
                with1.Cells.EntireColumn.AutoFit();
                with1.Cells[1, 1].Select();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't Add New Item"
                                  + Environment.NewLine + exception.Message, "Can't Add New Item", MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }
            finally
            {
                //RELEASE ALLOACTED RESOURCES
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                xlApp = null;
            }
        }
        #endregion
    }
}
