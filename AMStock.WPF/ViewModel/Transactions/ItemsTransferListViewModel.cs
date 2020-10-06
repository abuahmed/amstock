using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using AMStock.Core;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.DAL;
using AMStock.Repository;
using AMStock.Repository.Interfaces;
using AMStock.Service;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MessageBox = System.Windows.Forms.MessageBox;

namespace AMStock.WPF.ViewModel
{
    public class ItemsTransferListViewModel : ViewModelBase
    {
        #region Fields
        private static IUnitOfWork _unitOfWork;
        private DateTime _filterStartDate, _filterEndDate;
        private IEnumerable<ItemsMovementLineDTO> _salesLinesList;
        private ItemsMovementLineDTO _selectedTransactionLine;
        private ObservableCollection<ItemsMovementLineDTO> _transactionLines;
        private ICommand _refreshCommand;
        #endregion

        #region Constructor
        public ItemsTransferListViewModel()
        {
            CleanUp();
            _unitOfWork = new UnitOfWork(DbContextUtil.GetDbContextInstance());

            FilterStartDate = DateTime.Now.AddYears(-1);
            FilterEndDate = DateTime.Now.AddDays(1);
            GetTransactionLines();

            Messenger.Default.Register<ItemDTO>(this, (message) =>
            {
                SelectedItem = message;
            });

            LoadCategories();
            GetLiveItems();
            GetWarehouses();
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
        public void ExcuteRefreshCommand()
        {
            GetTransactionLines();
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

        public ItemsMovementLineDTO SelectedTransactionLine
        {
            get { return _selectedTransactionLine; }
            set
            {
                _selectedTransactionLine = value;
                RaisePropertyChanged<ItemsMovementLineDTO>(() => SelectedTransactionLine);
            }
        }
        public IEnumerable<ItemsMovementLineDTO> TransactionLinesList
        {
            get { return _salesLinesList; }
            set
            {
                _salesLinesList = value;
                RaisePropertyChanged<IEnumerable<ItemsMovementLineDTO>>(() => TransactionLinesList);
            }
        }
        public ObservableCollection<ItemsMovementLineDTO> TransactionLines
        {
            get { return _transactionLines; }
            set
            {
                _transactionLines = value;
                RaisePropertyChanged<ObservableCollection<ItemsMovementLineDTO>>(() => TransactionLines);

                TotalNumberOfRows = TransactionLines.Count;
                TotalNumberOfTransaction = (int)TransactionLines.Sum(s => s.MovedQuantity);
            }
        }

        #endregion

        #region Methods

        public void GetTransactionLines()
        {
            TransactionLinesList = new List<ItemsMovementLineDTO>();
            var criteria = new SearchCriteria<ItemsMovementLineDTO>
            {
                CurrentUserId = Singleton.User.UserId,
                BeginingDate = FilterStartDate,
                EndingDate = FilterEndDate
            };

            criteria.FiList.Add(sa => sa.ItemsMovementHeader.Status != TransactionStatus.Order);

            if (SelectedFromWarehouse != null && SelectedFromWarehouse.Id != -1)
                criteria.FiList.Add(sa => sa.ItemsMovementHeader.FromWarehouseId != SelectedFromWarehouse.Id);

            if (SelectedToWarehouse != null && SelectedToWarehouse.Id != -1)
                criteria.FiList.Add(sa => sa.ItemsMovementHeader.ToWarehouseId != SelectedToWarehouse.Id);

            TransactionLinesList = new ItemsMovementHeaderService().GetAllChilds(criteria).ToList();

            if (SelectedCategory != null && SelectedCategory.Id != -1)
                TransactionLinesList = TransactionLinesList.Where(w => w.Item.CategoryId == SelectedCategory.Id);

            if (SelectedItem != null && (SelectedItem.ItemCode != "" && SelectedItem.ItemCode != "All"))
                SearchText = SelectedItem.Id.ToString();
            else
                ViewTransactionLines(TransactionLinesList);
        }

        public void ViewTransactionLines(IEnumerable<ItemsMovementLineDTO> transactionLines)
        {
            TransactionLines = new ObservableCollection<ItemsMovementLineDTO>(transactionLines);
        }

        #endregion

        #region Warehouse
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
            Warehouses = Singleton.WarehousesList;
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
                    var salesList = TransactionLinesList.Where(c =>
                        c.Item.Id.ToString().ToLower().Equals(value.ToLower())).ToList();

                    ViewTransactionLines(salesList);
                }
                else
                    ViewTransactionLines(TransactionLinesList);
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
                GetTransactionLines();
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
        private int _totalNumberOfRows, _totalNumberOfTransaction;
        private string _summaryVisibility;

        public int TotalNumberOfRows
        {
            get { return _totalNumberOfRows; }
            set
            {
                _totalNumberOfRows = value;
                RaisePropertyChanged<int>(() => TotalNumberOfRows);
            }
        }
        public int TotalNumberOfTransaction
        {
            get { return _totalNumberOfTransaction; }
            set
            {
                _totalNumberOfTransaction = value;
                RaisePropertyChanged<int>(() => TotalNumberOfTransaction);
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

        #region Export To Excel
        private ICommand _exportToExcelCommand;
        public ICommand ExportToExcelCommand
        {
            get { return _exportToExcelCommand ?? (_exportToExcelCommand = new RelayCommand(ExecuteExportToExcelCommand)); }
        }

        private void ExecuteExportToExcelCommand()
        {
            string[] columnsHeader = {"From","To", "Date","Number","Item Code",
                                         "Item Name","Origin Qty.","Destination Qty.","Moved Qty."};

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
                    with1.Cells[I + 2, 0 + 1].value = TransactionLines[I].ItemsMovementHeader.FromWarehouse.DisplayName;
                    with1.Cells[I + 2, 1 + 1].value = TransactionLines[I].ItemsMovementHeader.ToWarehouse.DisplayName;
                    with1.Cells[I + 2, 2 + 1].value = TransactionLines[I].ItemsMovementHeader.MovementDateString;
                    with1.Cells[I + 2, 3 + 1].value = TransactionLines[I].ItemsMovementHeader.MovementNumber;
                    with1.Cells[I + 2, 4 + 1].value = TransactionLines[I].Item.ItemCode;
                    with1.Cells[I + 2, 5 + 1].value = TransactionLines[I].Item.DisplayName;
                    with1.Cells[I + 2, 6 + 1].value = TransactionLines[I].OriginPreviousQuantity;
                    with1.Cells[I + 2, 7 + 1].value = TransactionLines[I].DestinationPreviousQuantity;
                    with1.Cells[I + 2, 8 + 1].value = TransactionLines[I].MovedQuantity;
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
