using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using AMStock.Core;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.DAL;
using AMStock.Repository;
using AMStock.Repository.Interfaces;
using AMStock.Service;
using AMStock.WPF.Reports.DataSets;
using AMStock.WPF.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using AMStock.Core.Common;
using GalaSoft.MvvmLight.Messaging;
using MessageBox = System.Windows.Forms.MessageBox;
using TransactionItemsList = AMStock.WPF.Reports.Transactions.TransactionItemsList;

namespace AMStock.WPF.ViewModel
{
    public class TransactionItemsListViewModel : ViewModelBase
    {
        #region Fields
        private static IUnitOfWork _unitOfWork;
        private TransactionTypes _transaction;
        private DateTime _filterStartDate, _filterEndDate;
        private IEnumerable<TransactionLineDTO> _salesLinesList;
        private IEnumerable<PhysicalInventoryLineDTO> _physicalInventoryLinesList;
        private TransactionLineDetail _selectedTransactionLine;
        private ObservableCollection<TransactionLineDetail> _transactionLines;
        private ICommand _refreshCommand;
        private string _headerText, _businessPartner, _qty;
        #endregion

        #region Constructor
        public TransactionItemsListViewModel()
        {
            CleanUp();
            _unitOfWork = new UnitOfWork(DbContextUtil.GetDbContextInstance());

            FilterStartDate = DateTime.Now.AddYears(-1);
            FilterEndDate = DateTime.Now.AddDays(1);

            Messenger.Default.Register<ItemDTO>(this, (message) => { SelectedItem = message; });
            Messenger.Default.Register<TransactionTypes>(this, (message) => { Transaction = message; });

            LoadCategories();
            GetLiveItems();

            GetWarehouses();
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
            if (_unitOfWork != null)
                _unitOfWork.Dispose();
        }
        #endregion

        #region Public Properties

        public ICommand RefreshCommand
        {
            get
            {
                return _refreshCommand ?? (_refreshCommand = new RelayCommand(ExcuteRefreshCommand));
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

        public string HeaderText
        {
            get { return _headerText; }
            set
            {
                _headerText = value;
                RaisePropertyChanged<string>(() => HeaderText);
            }
        }
        public string Qty
        {
            get { return _qty; }
            set
            {
                _qty = value;
                RaisePropertyChanged<string>(() => Qty);
            }
        }
        public string BusinessPartner
        {
            get { return _businessPartner; }
            set
            {
                _businessPartner = value;
                RaisePropertyChanged<string>(() => BusinessPartner);
            }
        }

        public TransactionTypes Transaction
        {
            get { return _transaction; }
            set
            {
                _transaction = value;
                RaisePropertyChanged<TransactionTypes>(() => Transaction);

                GetLiveBusinessPartners();
                BusinessPartnerVisibility = "Visible";

                switch (Transaction)
                {
                    case TransactionTypes.Sale:
                        GetTransactionLines();
                        HeaderText = "Sales Detail List";
                        BusinessPartner = "Customer";
                        Qty = "Qty Sold";
                        SummaryVisibility = "Visible";
                        break;
                    case TransactionTypes.Purchase:
                        GetTransactionLines();
                        HeaderText = "Purchases Detail List";
                        BusinessPartner = "Supplier";
                        Qty = "Qty Purchased";
                        SummaryVisibility = "Collapsed";
                        break;
                    case TransactionTypes.Pi:
                        BusinessPartnerVisibility = "Collapsed";
                        GetPhysicalInventoryLines();
                        HeaderText = "PI Detail List";
                        BusinessPartner = "";
                        Qty = "Qty Counted";
                        SummaryVisibility = "Collapsed";
                        break;
                }
            }
        }
        public TransactionLineDetail SelectedTransactionLine
        {
            get { return _selectedTransactionLine; }
            set
            {
                _selectedTransactionLine = value;
                RaisePropertyChanged<TransactionLineDetail>(() => SelectedTransactionLine);
            }
        }
        public IEnumerable<TransactionLineDTO> TransactionLinesList
        {
            get { return _salesLinesList; }
            set
            {
                _salesLinesList = value;
                RaisePropertyChanged<IEnumerable<TransactionLineDTO>>(() => TransactionLinesList);
            }
        }
        public ObservableCollection<TransactionLineDetail> TransactionLines
        {
            get { return _transactionLines; }
            set
            {
                _transactionLines = value;
                RaisePropertyChanged<ObservableCollection<TransactionLineDetail>>(() => TransactionLines);

                TotalNumberOfRows = TransactionLines.Count;
                TotalNumberOfTransaction = TransactionLines.Sum(s => s.Unit);
                TotalValueOfTransaction = TransactionLines.Sum(s => s.LinePrice).ToString("N");
                TotalValueOfPurchase = (TransactionLines.Sum(s => s.PurchasePrice * s.Unit)).ToString("N");
                Profit = (Convert.ToDecimal(TotalValueOfTransaction) - Convert.ToDecimal(TotalValueOfPurchase)).ToString("N");
            }
        }

        public IEnumerable<PhysicalInventoryLineDTO> PhysicalInventoryLinesList
        {
            get { return _physicalInventoryLinesList; }
            set
            {
                _physicalInventoryLinesList = value;
                RaisePropertyChanged<IEnumerable<PhysicalInventoryLineDTO>>(() => PhysicalInventoryLinesList);
            }
        }
        #endregion

        #region Methods
        public void ExcuteRefreshCommand()
        {
            switch (Transaction)
            {
                case TransactionTypes.Sale:
                    GetTransactionLines();
                    break;
                case TransactionTypes.Purchase:
                    GetTransactionLines();
                    break;
                case TransactionTypes.Pi:
                    GetPhysicalInventoryLines();
                    break;
            }
        }

        public void GetTransactionLines()
        {
            TransactionLinesList = new List<TransactionLineDTO>();
            var criteria = new SearchCriteria<TransactionLineDTO>
            {
                CurrentUserId = Singleton.User.UserId,
                BeginingDate = FilterStartDate,
                EndingDate = FilterEndDate
            };
            criteria.FiList.Add(sa => sa.Transaction.TransactionType == Transaction &&
                                 sa.Transaction.Status != TransactionStatus.Order);

            if (SelectedWarehouse != null && SelectedWarehouse.Id != -1)
                criteria.SelectedWarehouseId = SelectedWarehouse.Id;

            if (SelectedBusinessPartner != null && SelectedBusinessPartner.Id != -1)
                criteria.BusinessPartnerId = SelectedBusinessPartner.Id;

            int totalCount;
            TransactionLinesList = new TransactionService()
                .GetAllChilds(criteria, out totalCount)
                .ToList();

            if (SelectedCategory != null && SelectedCategory.Id != -1)
                TransactionLinesList = TransactionLinesList.Where(w => w.Item.CategoryId == SelectedCategory.Id);

            if (SelectedItem != null && (SelectedItem.ItemCode != "" && SelectedItem.ItemCode != "All"))
                SearchText = SelectedItem.Id.ToString();
            else
                ViewTransactionLines(TransactionLinesList, null);
        }

        public void GetPhysicalInventoryLines()
        {
            PhysicalInventoryLinesList = new List<PhysicalInventoryLineDTO>();
            var criteria = new SearchCriteria<PhysicalInventoryLineDTO>
            {
                CurrentUserId = Singleton.User.UserId,
                BeginingDate = FilterStartDate,
                EndingDate = FilterEndDate
            };
            criteria.FiList.Add(sa => sa.PhysicalInventory.Status != PhysicalInventoryStatus.Draft);

            if (SelectedWarehouse != null && SelectedWarehouse.Id != -1)
                criteria.SelectedWarehouseId = SelectedWarehouse.Id;

            PhysicalInventoryLinesList = new PiHeaderService().GetAllChilds(criteria).ToList();

            PhysicalInventoryLinesList = PhysicalInventoryLinesList.Where(pi => pi.PhysicalInventory.ShowLines).ToList();

            if (SelectedCategory != null && SelectedCategory.Id != -1)
                TransactionLinesList = TransactionLinesList.Where(w => w.Item.CategoryId == SelectedCategory.Id);

            if (SelectedItem != null && (SelectedItem.ItemCode != "" && SelectedItem.ItemCode != "All"))
                SearchText = SelectedItem.Id.ToString();
            else
                ViewTransactionLines(null, PhysicalInventoryLinesList.ToList());
        }

        public void ViewTransactionLines(IEnumerable<TransactionLineDTO> transactionLines,
                                         IEnumerable<PhysicalInventoryLineDTO> piLines)
        {
            switch (Transaction)
            {
                case TransactionTypes.Purchase:
                case TransactionTypes.Sale:
                    {
                        #region Transaction
                        var salesLinesTransactionList = transactionLines.Select(salesLineDto => new TransactionLineDetail
                        {
                            TransactionDate = salesLineDto.Transaction.TransactionDate,
                            TransactionId = salesLineDto.Transaction.Id,
                            TransactionNumber = salesLineDto.Transaction.TransactionNumber,
                            DisplayName = salesLineDto.Transaction.BusinessPartner.DisplayName,
                            ItemCode = salesLineDto.Item.ItemCode,
                            ItemDisplayName = salesLineDto.Item.DisplayName,
                            Unit = salesLineDto.Unit,
                            EachPrice = salesLineDto.EachPrice,
                            LinePrice = salesLineDto.LinePrice,
                            PurchasePrice = salesLineDto.Item.PurchasePrice,
                            WarehouseName = salesLineDto.Transaction.Warehouse.DisplayName
                        }).ToList();

                        TransactionLines = new ObservableCollection<TransactionLineDetail>(salesLinesTransactionList);
                        #endregion
                    }
                    break;
                case TransactionTypes.Pi:
                    {
                        #region Pi
                        var piLinesTransactionList = piLines.Select(piLineDto => new TransactionLineDetail
                        {
                            TransactionDate = piLineDto.PhysicalInventory.PhysicalInventoryDate,
                            TransactionId = piLineDto.PhysicalInventory.Id,
                            TransactionNumber = piLineDto.PhysicalInventory.PhysicalInventoryNumber,
                            DisplayName = "",
                            ItemCode = piLineDto.Item.ItemCode,
                            ItemDisplayName = piLineDto.Item.DisplayName,
                            Unit = piLineDto.CountedQty,
                            EachPrice = piLineDto.Item.SellPrice,
                            LinePrice = piLineDto.CountedQty * piLineDto.Item.SellPrice,
                            PurchasePrice = piLineDto.Item.PurchasePrice,
                            WarehouseName = piLineDto.PhysicalInventory.Warehouse.DisplayName
                        }).ToList();

                        TransactionLines = new ObservableCollection<TransactionLineDetail>(piLinesTransactionList);
                        #endregion
                    }
                    break;
            }
        }

        #endregion

        #region Warehouse
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
            }
        }
        public void GetWarehouses()
        {
            Warehouses = Singleton.WarehousesList;
        }
        #endregion

        #region BusinessPartner
        private ObservableCollection<BusinessPartnerDTO> _businessPartners;
        private BusinessPartnerDTO _selectedBusinessPartner;
        private string _businessPartnerVisibility;

        public string BusinessPartnerVisibility
        {
            get { return _businessPartnerVisibility; }
            set
            {
                _businessPartnerVisibility = value;
                RaisePropertyChanged<string>(() => BusinessPartnerVisibility);
            }
        }
        public ObservableCollection<BusinessPartnerDTO> BusinessPartners
        {
            get { return _businessPartners; }
            set
            {
                _businessPartners = value;
                RaisePropertyChanged<ObservableCollection<BusinessPartnerDTO>>(() => BusinessPartners);
            }
        }
        public BusinessPartnerDTO SelectedBusinessPartner
        {
            get { return _selectedBusinessPartner; }
            set
            {
                _selectedBusinessPartner = value;
                RaisePropertyChanged<BusinessPartnerDTO>(() => SelectedBusinessPartner);
                if (SelectedBusinessPartner != null)
                    GetTransactionLines();
            }
        }
        private void GetLiveBusinessPartners()
        {
            try
            {
                var criteria = new SearchCriteria<BusinessPartnerDTO>
                {
                    TransactionType = (int)Transaction
                };

                var bpList = new BusinessPartnerService()
                    .GetAll(criteria)
                    .OrderBy(i => i.Id)
                    .ToList();

                if (bpList.Count > 1)
                    bpList.Insert(0, new BusinessPartnerDTO
                    {
                        DisplayName = "All",
                        Id = -1
                    });

                BusinessPartners = new ObservableCollection<BusinessPartnerDTO>(bpList);
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show("Can't Load Business Partner"
                                  + Environment.NewLine + exception.Message, "Can't Get ", MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }
        }
        #endregion

        #region Items
        private string _searchText;
        private ItemDTO _selectedItem;
        private ObservableCollection<ItemDTO> _items;

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                RaisePropertyChanged<string>(() => SearchText);

                if (SearchText != "" && SearchText != "All")
                {
                    switch (Transaction)
                    {
                        case TransactionTypes.Sale:
                        case TransactionTypes.Purchase:
                            {
                                var salesList = TransactionLinesList.Where(c =>
                                    c.Item.Id.ToString().ToLower().Equals(value.ToLower())
                                    ).ToList();
                                ViewTransactionLines(salesList, null);
                            }
                            break;
                        case TransactionTypes.Pi:
                            {
                                var piList = PhysicalInventoryLinesList.Where(c =>
                                   c.Item.Id.ToString().ToLower().Equals(value.ToLower())
                                   ).ToList();
                                ViewTransactionLines(null, piList);
                            }
                            break;
                    }
                }
                else
                {
                    switch (Transaction)
                    {
                        case TransactionTypes.Sale:
                        case TransactionTypes.Purchase:
                            ViewTransactionLines(TransactionLinesList, null);
                            break;
                        case TransactionTypes.Pi:
                            ViewTransactionLines(null, PhysicalInventoryLinesList);
                            break;
                    }
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
                if (SelectedItem != null)
                {
                    SearchText = SelectedItem.Id == -1 ? "All" : SelectedItem.Id.ToString();
                }
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

        private void GetLiveItems()
        {
            var items = _unitOfWork.Repository<ItemDTO>().Query()
                                .Get()
                                .OrderByDescending(i => i.Id)
                                .ToList();
            if (items.Count > 1)
                items.Insert(0, new ItemDTO
                {
                    DisplayName = "",
                    ItemCode = "All",
                    Id = -1
                });
            Items = new ObservableCollection<ItemDTO>(items);
        }
        #endregion

        #region Categories
        private CategoryDTO _selectedCategory;
        private ObservableCollection<CategoryDTO> _categories;

        public void LoadCategories()
        {
            IList<CategoryDTO> categoriesList = _unitOfWork.Repository<CategoryDTO>()
                .Query()
                .Filter(c => c.NameType == NameTypes.Category)
                .Get()
                .OrderByDescending(i => i.Id)
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

                switch (Transaction)
                {
                    case TransactionTypes.Sale:
                    case TransactionTypes.Purchase:
                        GetTransactionLines();
                        break;
                    case TransactionTypes.Pi:
                        GetPhysicalInventoryLines();
                        break;
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

        #region Summary

        private int _totalNumberOfRows;
        private decimal _totalNumberOfTransaction;
        private string _totalItemsValue, _totalValueOfTransaction, _totalValueOfPurchase, _profit, _summaryVisibility;

        public int TotalNumberOfRows
        {
            get { return _totalNumberOfRows; }
            set
            {
                _totalNumberOfRows = value;
                RaisePropertyChanged<int>(() => TotalNumberOfRows);
            }
        }
        public decimal TotalNumberOfTransaction
        {
            get { return _totalNumberOfTransaction; }
            set
            {
                _totalNumberOfTransaction = value;
                RaisePropertyChanged<decimal>(() => TotalNumberOfTransaction);
            }
        }
        public string TotalItemsValue
        {
            get { return _totalItemsValue; }
            set
            {
                _totalItemsValue = value;
                RaisePropertyChanged<string>(() => TotalItemsValue);
            }
        }
        public string TotalValueOfTransaction
        {
            get { return _totalValueOfTransaction; }
            set
            {
                _totalValueOfTransaction = value;
                RaisePropertyChanged<string>(() => TotalValueOfTransaction);
            }
        }
        public string TotalValueOfPurchase
        {
            get { return _totalValueOfPurchase; }
            set
            {
                _totalValueOfPurchase = value;
                RaisePropertyChanged<string>(() => TotalValueOfPurchase);
            }
        }
        public string Profit
        {
            get { return _profit; }
            set
            {
                _profit = value;
                RaisePropertyChanged<string>(() => Profit);
            }
        }
        public string SummaryVisibility
        {
            get { return _summaryVisibility; }
            set
            {
                _summaryVisibility = value;
                RaisePropertyChanged<string>(() => SummaryVisibility);
            }
        }
        #endregion

        #region Print List
        private ICommand _printListCommandView;

        public ICommand PrintListCommandView
        {
            get
            {
                return _printListCommandView ?? (_printListCommandView = new RelayCommand<Object>(PrintList));
            }
        }
        public void PrintList(object obj)
        {

            var myReport4 = new TransactionItemsList();
            myReport4.SetDataSource(GetListDataSet());

            //MenuItem menu = obj as MenuItem;
            //if (menu != null)
            //    new ReportUtility().DirectPrinter(myReport4);
            //else
            //{
            var report = new ReportViewerCommon(myReport4);
            report.ShowDialog();
            //}
        }
        public TransactionDataSet GetListDataSet()
        {
            var myDataSet = new TransactionDataSet();
            var serNo = 1;
            foreach (var line in TransactionLines)
            {

                myDataSet.TransactionList.Rows.Add(
                    "Store: " + line.WarehouseName,
                    "On Date: " + line.TransactionDateString,
                    BusinessPartner + " Name: " + line.DisplayName,
                    "Transaction Number: " + line.TransactionNumber,
                    line.TransactionStatus,
                    serNo,
                    line.ItemCode,
                    line.ItemDisplayName,
                    "",
                    0,
                    0,
                    line.EachPrice,
                    line.Unit,
                    line.LinePrice,
                    0);

                serNo++;
            }

            return myDataSet;
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
            string[] columnsHeader = {"Store", "Date","Number",BusinessPartner,"Item Code",
                                         "Item Name",Qty,"Unit Price","Line Price"};

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
            var xlApp = new Microsoft.Office.Interop.Excel.Application();

            try
            {
                Microsoft.Office.Interop.Excel.Workbook excelBook = xlApp.Workbooks.Add();
                var excelWorksheet = (Microsoft.Office.Interop.Excel.Worksheet)excelBook.Worksheets[1];
                xlApp.Visible = true;

                int rowsTotal = TransactionLines.Count;
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
                    with1.Cells[I + 2, 0 + 1].value = TransactionLines[I].WarehouseName;
                    with1.Cells[I + 2, 1 + 1].value = TransactionLines[I].TransactionDateString;
                    with1.Cells[I + 2, 2 + 1].value = TransactionLines[I].TransactionNumber;
                    with1.Cells[I + 2, 3 + 1].value = TransactionLines[I].DisplayName;
                    with1.Cells[I + 2, 4 + 1].value = TransactionLines[I].ItemCode;
                    with1.Cells[I + 2, 5 + 1].value = TransactionLines[I].ItemDisplayName;
                    with1.Cells[I + 2, 6 + 1].value = TransactionLines[I].Unit;
                    with1.Cells[I + 2, 7 + 1].value = TransactionLines[I].EachPrice;
                    with1.Cells[I + 2, 8 + 1].value = TransactionLines[I].LinePrice;
                }

                with1.Rows["1:1"].Font.FontStyle = "Bold";
                with1.Rows["1:1"].Font.Size = 12;

                with1.Cells.Columns.AutoFit();
                with1.Cells.Select();
                with1.Cells.EntireColumn.AutoFit();
                with1.Cells[1, 1].Select();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                //RELEASE ALLOACTED RESOURCES
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                xlApp = null;
            }
        }

        public void ImportFromExcel()
        {

        }
        #endregion
    }
}
