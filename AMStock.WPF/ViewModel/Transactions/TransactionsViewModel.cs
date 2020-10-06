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
using AMStock.WPF.Models;
using AMStock.WPF.Views;
using GalaSoft.MvvmLight.Command;
using Telerik.Windows.Controls;
using ViewModelBase = GalaSoft.MvvmLight.ViewModelBase;

namespace AMStock.WPF.ViewModel
{
    public class TransactionsViewModel : ViewModelBase
    {
        #region Fields
        private static ITransactionService _transactionService;
        private TransactionTypes _transactionType;
        private IPiHeaderService _physicalInventoryService;
        private ICommand _refreshWindowCommand;
        private string _transactionText, _businessPartnerText;
        #endregion

        #region Constructor

        public TransactionsViewModel()
        {
            FillPeriodCombo();
            SelectedPeriod = FilterPeriods.FirstOrDefault(p => p.Value == 1);//Value == 0(show all) Value=3(show this week)

            FillStatusCombo();
            GetWarehouses();
        }

        public void Load()
        {
            CleanUp();
            _transactionService = new TransactionService();

            GetLiveBusinessPartners();

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
            if (_transactionService != null)
                _transactionService.Dispose();
        }

        #region Public Properties
        public TransactionTypes TransactionType
        {
            get { return _transactionType; }
            set
            {
                _transactionType = value;
                RaisePropertyChanged<TransactionTypes>(() => TransactionType);

                if (TransactionType != TransactionTypes.All)
                {
                    Load();
                    CheckRoles();
                    switch (TransactionType)
                    {
                        case TransactionTypes.Sale:
                            TransactionText = "Add New Sale";
                            BusinessPartnerText = "Customer";
                            SummaryVisibility = "Visible";
                            PrintTransactionVisibility = "Visible";
                            BusinessPartnerType = BusinessPartnerTypes.Customer;
                            break;
                        case TransactionTypes.Purchase:
                            TransactionText = "Add New Purchase";
                            BusinessPartnerText = "Supplier";
                            SummaryVisibility = "Collapsed";
                            PrintTransactionVisibility = "Collapsed";
                            BusinessPartnerType = BusinessPartnerTypes.Supplier;
                            break;
                    }
                }
            }
        }
        public IPiHeaderService PhysicalInventoryService
        {
            get { return _physicalInventoryService; }
            set
            {
                _physicalInventoryService = value;
                RaisePropertyChanged<IPiHeaderService>(() => PhysicalInventoryService);
            }
        }
        public string TransactionText
        {
            get { return _transactionText; }
            set
            {
                _transactionText = value;
                RaisePropertyChanged<string>(() => TransactionText);
            }
        }
        public string BusinessPartnerText
        {
            get { return _businessPartnerText; }
            set
            {
                _businessPartnerText = value;
                RaisePropertyChanged<string>(() => BusinessPartnerText);
            }
        }
        #endregion

        public ICommand RefreshWindowCommand
        {
            get
            {
                return _refreshWindowCommand ?? (_refreshWindowCommand = new RelayCommand(Load));
            }
        }
        #endregion

        #region Header

        #region Fields
        private int _totalNumberOfTransaction;
        private string _totalValueOfTransaction, _totalValueOfPurchase, _profit, _expense, _summaryVisibility;
        private bool _saveHeaderEnability, _saveHeaderExpandibility, _addNewTransactionEnability, _unPostEnability;
        private decimal _expenses = 0x0;

        private ObservableCollection<TransactionHeaderDTO> _transactions;
        private TransactionHeaderDTO _selectedTransaction;
        #endregion

        #region Public Properties
        public int TotalNumberOfTransaction
        {
            get { return _totalNumberOfTransaction; }
            set
            {
                _totalNumberOfTransaction = value;
                RaisePropertyChanged<int>(() => TotalNumberOfTransaction);
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
        public string Expenses
        {
            get { return _expense; }
            set
            {
                _expense = value;
                RaisePropertyChanged<string>(() => Expenses);
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

        public bool AddNewTransactionEnability
        {
            get { return _addNewTransactionEnability; }
            set
            {
                _addNewTransactionEnability = value;
                RaisePropertyChanged<bool>(() => AddNewTransactionEnability);
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
        public bool SaveHeaderExpandibility
        {
            get { return _saveHeaderExpandibility; }
            set
            {
                _saveHeaderExpandibility = value;
                RaisePropertyChanged<bool>(() => SaveHeaderExpandibility);
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

        public IEnumerable<TransactionHeaderDTO> TransactionList
        {
            get { return _transactionsList; }
            set
            {
                _transactionsList = value;
                RaisePropertyChanged<IEnumerable<TransactionHeaderDTO>>(() => TransactionList);
            }
        }
        public ObservableCollection<TransactionHeaderDTO> Transactions
        {
            get { return _transactions; }
            set
            {
                _transactions = value;
                RaisePropertyChanged<ObservableCollection<TransactionHeaderDTO>>(() => Transactions);

                GetSummary();

                AddNewTransaction();
            }
        }
        public TransactionHeaderDTO SelectedTransaction
        {
            get { return _selectedTransaction; }
            set
            {
                _selectedTransaction = value;
                RaisePropertyChanged<TransactionHeaderDTO>(() => SelectedTransaction);

                if (SelectedTransaction == null) return;

                if (BusinessPartners != null)
                    SelectedBusinessPartner =
                        BusinessPartners.FirstOrDefault(b => b.Id == SelectedTransaction.BusinessPartnerId);
                else//we can't have transaction with out business partner
                {
                    Load();
                    return;
                }


                switch (SelectedTransaction.Status)
                {
                    case TransactionStatus.Posted:
                        UnPostEnability = SelectedWarehouse.Id != -1;
                        SaveHeaderEnability = false;
                        SaveHeaderExpandibility = false;
                        TransactionLine = null;
                        break;
                    case TransactionStatus.Order:
                        UnPostEnability = false;
                        SaveHeaderEnability = SelectedWarehouse.Id != -1;
                        SaveHeaderExpandibility = true;
                        TransactionLine = new TransactionLineModel();
                        break;
                }

                SelectedItemQuantity = null;
                _isEdit = false;
                UnitPricePlusTax = false;
                GetTransactionLines();
                GetPayments();
            }
        }
        #endregion

        public void GetTransactions()
        {
            GetExpenses();

            try
            {
                TransactionList = new List<TransactionHeaderDTO>();

                var criteria = new SearchCriteria<TransactionHeaderDTO>
                {
                    CurrentUserId = Singleton.User.UserId,
                    BeginingDate = FilterStartDate,
                    EndingDate = FilterEndDate,
                    //Page = 1,
                    //PageSize = 31
                };

                criteria.FiList.Add(p => p.TransactionType == TransactionType);

                if (SelectedBusinessPartnerForFilter != null && SelectedBusinessPartnerForFilter.Id != -1)
                    criteria.BusinessPartnerId = SelectedBusinessPartnerForFilter.Id;

                if (SelectedWarehouse != null && SelectedWarehouse.Id != -1)
                    criteria.SelectedWarehouseId = SelectedWarehouse.Id;

                //if (SelectedStatus != null)
                //    criteria.PaymentListType = SelectedStatus.Value;
                //else
                //    criteria.PaymentListType = 1;//Only show those with no payment

                TransactionList = _transactionService.GetAll(criteria);

                if (TransactionList != null)
                    Transactions = new ObservableCollection<TransactionHeaderDTO>(TransactionList.OrderByDescending(t => t.TransactionDate));
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't Get Transactions"
                                  + Environment.NewLine + exception.Message, "Can't Get Transactions", MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }
        }

        public void GetSummary()
        {
            if (Transactions != null)
            {
                TotalNumberOfTransaction = Transactions.Count();
                TotalValueOfTransaction = Transactions.Where(t => t.Status != TransactionStatus.Order)
                    .Sum(s => s.TotalDue).ToString("N");
                TotalValueOfPurchase = Transactions.Where(t => t.Status != TransactionStatus.Order)
                    .Sum(s => s.TotalPurchasingCost).ToString("N");
                Profit = (Transactions.Where(t => t.Status != TransactionStatus.Order).Sum(s => s.TotalDue) - 
                    Transactions.Where(t => t.Status != TransactionStatus.Order).Sum(s => s.TotalPurchasingCost) - 
                    _expenses).ToString("N");
            }
        }

        private void GetExpenses()
        {
            try
            {
                var criteria = new SearchCriteria<PaymentDTO>
                {
                    CurrentUserId = Singleton.User.UserId,
                    BeginingDate = FilterStartDate,
                    EndingDate = FilterEndDate
                };
                criteria.FiList.Add(p => p.PaymentType == PaymentTypes.CashOut);

                if (SelectedWarehouse != null && SelectedWarehouse.Id != -1)
                    criteria.SelectedWarehouseId = SelectedWarehouse.Id;

                var paymentList2 = new PaymentService(true).GetAll(criteria).ToList();

                _expenses = paymentList2.Sum(p => p.Amount);
                Expenses = _expenses.ToString("N");
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't Get Expenses"
                                  + Environment.NewLine + exception.Message, "Can't Get Expenses", MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }
        }

        #region Commands
        private ICommand _addNewTransactionCommand, _saveTransactionCommand, _postTransactionCommand;
        private ICommand _unPostTransactionCommand, _deleteTransactionCommand;
        private ICommand _filterByDateCommand;

        public ICommand AddNewTransactionCommand
        {
            get
            {
                return _addNewTransactionCommand ?? (_addNewTransactionCommand = new RelayCommand(AddNewTransaction));
            }
        }
        private void AddNewTransaction()
        {
            try
            {
                if (SelectedWarehouse != null && SelectedWarehouse.Id == -1) return;

                SelectedTransaction = null;
                if (BusinessPartners != null) SelectedBusinessPartner = BusinessPartners.FirstOrDefault();

                if (SelectedBusinessPartner != null && SelectedWarehouse != null)
                {
                    SelectedTransaction = new TransactionHeaderDTO
                    {
                        TransactionType = TransactionType,
                        TransactionDate = DateTime.Now,
                        Status = TransactionStatus.Order,
                        WarehouseId = SelectedWarehouse.Id,
                        BusinessPartnerId = SelectedBusinessPartner.Id
                    };
                    TransactionLines = new ObservableCollection<TransactionLineDTO>();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't add new"
                                  + Environment.NewLine + exception.Message, "Can't add new", MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }
        }

        public ICommand SaveTransactionCommand
        {
            get
            {
                return _saveTransactionCommand ?? (_saveTransactionCommand = new RelayCommand(SaveTransaction));
            }
        }
        private void SaveTransaction()
        {
            try
            {
                SelectedTransaction.BusinessPartnerId = SelectedBusinessPartner.Id;

                var newObject = SelectedTransaction.Id;

                var stat = _transactionService.InsertOrUpdate(SelectedTransaction);
                if (stat != string.Empty)
                    MessageBox.Show("Can't save"
                                    + Environment.NewLine + stat, "Can't save", MessageBoxButton.OK,
                        MessageBoxImage.Error);

                else if (newObject == 0)
                {
                    Transactions.Insert(0, SelectedTransaction);
                    SelectedTransaction = SelectedTransaction;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't save"
                                  + Environment.NewLine + exception.Message, "Can't save", MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }
        }

        public ICommand PostTransactionCommand
        {
            get
            {
                return _postTransactionCommand ?? (_postTransactionCommand = new RelayCommand(ExcutePostTransaction));
            }
        }
        private void ExcutePostTransaction()
        {
            try
            {
                if (TransactionLines.Count == 0)
                {
                    MessageBox.Show("No Items To Post, Add Item First....");
                    return;
                }

                ////Added for direct posting
                //var selectedPaymentModel = new PaymentModel
                //{
                //    AmountRequired = SelectedTransaction.TotalDue,
                //    PaymentDate = DateTime.Now,
                //    Amount = SelectedTransaction.TotalDue,
                //    ReachedLimit = false
                //};

                //var stat = new PaymentService().PostPayments(SelectedTransaction, null, selectedPaymentModel, null);
                //if (stat == string.Empty)
                //{
                //    Load();
                //    AddNewTransaction();
                //}
                //else
                //    MessageBox.Show(stat);

                //bellow for common posting
                var addPayment = new AddPayment(SelectedTransaction);
                addPayment.ShowDialog();
                if (addPayment.DialogResult != null && (bool)addPayment.DialogResult)
                {
                    Load();
                    AddNewTransaction();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't post"
                                  + Environment.NewLine + exception.Message, "Can't post", MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }

        }

        public ICommand UnPostTransactionCommand
        {
            get
            {
                return _unPostTransactionCommand ?? (_unPostTransactionCommand = new RelayCommand(ExcuteUnPostTransaction));
            }
        }
        private void ExcuteUnPostTransaction()
        {
            if (MessageBox.Show("Are you Sure You want to Un-post this Transaction?", "Un-post Transaction",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            try
            {
                var stat = _transactionService.UnPost(SelectedTransaction);
                if (stat != string.Empty)
                    MessageBox.Show("Can't unpost"
                                    + Environment.NewLine + stat, "Can't save", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                else
                {
                    var tranId = SelectedTransaction.Id;
                    Load();
                    if (Transactions != null) SelectedTransaction = Transactions.FirstOrDefault(t => t.Id == tranId);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't unpost!!, please try again, after refreshing the window..."
                                  + Environment.NewLine + exception.Message, "Can't unpost", MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }
        }

        public ICommand DeleteTransactionCommand
        {
            get
            {
                return _deleteTransactionCommand ?? (_deleteTransactionCommand = new RelayCommand(ExcuteDeleteTransaction));
            }
        }
        private void ExcuteDeleteTransaction()
        {
            try
            {
                if (MessageBox.Show("Are you Sure You want to Delete this Transaction?", "Delete Transaction",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    SelectedTransaction.Enabled = false;
                    var stat = _transactionService.Disable(SelectedTransaction);
                    if (stat == string.Empty)
                    {
                        Transactions.Remove(SelectedTransaction);
                        Transactions = Transactions;
                    }
                    else
                        MessageBox.Show("Can't Delete Transaction, Please try again!!" + Environment.NewLine + stat,
                                         "Can't delete", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't Delete Transaction, Please try again!!"
                                    + Environment.NewLine + exception.Message, "Can't delete", MessageBoxButton.OK,
                        MessageBoxImage.Error);
            }

        }

        public ICommand FilterByDateCommand
        {
            get { return _filterByDateCommand ?? (_filterByDateCommand = new RelayCommand(ExecuteFilterByDateCommand)); }
        }
        private void ExecuteFilterByDateCommand()
        {
            GetTransactions();
        }
        #endregion

        #endregion

        #region Payments
        private ObservableCollection<PaymentDTO> _payments;
        private string _paymentListVisibility;

        public ObservableCollection<PaymentDTO> Payments
        {
            get { return _payments; }
            set
            {
                _payments = value;
                RaisePropertyChanged<ObservableCollection<PaymentDTO>>(() => Payments);
                PaymentListVisibility = Payments.Count > 0 ? "Visible" : "Collapsed";
            }
        }
        public string PaymentListVisibility
        {
            get { return _paymentListVisibility; }
            set
            {
                _paymentListVisibility = value;
                RaisePropertyChanged<string>(() => PaymentListVisibility);
            }
        }

        private void GetPayments()
        {
            Payments = new ObservableCollection<PaymentDTO>();
            if (SelectedTransaction != null && SelectedTransaction.Id != 0)
            {
                var paymentList = new PaymentService().GetAll().Where(s => s.TransactionId == SelectedTransaction.Id);
                Payments = new ObservableCollection<PaymentDTO>(paymentList);
            }
        }
        #endregion

        #region Lines

        #region Fields
        private bool _isEdit, _unitPricePlusTax;
        private string _totalItemsValue, _unitPricePlusTaxVisibility;
        private int _totalItemsCounted;
        private ICommand _addTransactionLineCommand, _editTransactionLineCommand, _deleteTransactionLineCommand;
        private ObservableCollection<TransactionLineDTO> _transactionsLines;
        private TransactionLineDTO _selectedTransactionLine;
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
        public bool UnitPricePlusTax
        {
            get { return _unitPricePlusTax; }
            set
            {
                _unitPricePlusTax = value;
                RaisePropertyChanged<bool>(() => UnitPricePlusTax);
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
        public string UnitPricePlusTaxVisibility
        {
            get { return _unitPricePlusTaxVisibility; }
            set
            {
                _unitPricePlusTaxVisibility = value;
                RaisePropertyChanged<string>(() => UnitPricePlusTaxVisibility);
            }
        }

        public TransactionLineDTO SelectedTransactionLine
        {
            get { return _selectedTransactionLine; }
            set
            {
                _selectedTransactionLine = value;
                RaisePropertyChanged<TransactionLineDTO>(() => SelectedTransactionLine);
            }
        }
        public ObservableCollection<TransactionLineDTO> TransactionLines
        {
            get { return _transactionsLines; }
            set
            {
                _transactionsLines = value;
                RaisePropertyChanged<ObservableCollection<TransactionLineDTO>>(() => TransactionLines);

                if (TransactionLines != null)
                {
                    var lineCounts = TransactionLines.Count;
                    var lineValues = TransactionLines.Sum(s => s.LinePrice);
                    if (SelectedTransaction != null)
                    {
                        SelectedTransaction.CountLines = lineCounts;
                        SelectedTransaction.TotalCost = lineValues;
                    }
                    TotalItemsCounted = lineCounts;
                    TotalItemsValue = lineValues.ToString("N");
                }
            }
        }
        #endregion

        public void GetTransactionLines()
        {
            if (SelectedTransaction != null && SelectedTransaction.Id != 0)
            {
                var transactionLinesList = _transactionService.GetChilds(SelectedTransaction.Id, false);
                TransactionLines = new ObservableCollection<TransactionLineDTO>(transactionLinesList);
            }
        }

        #region Commands
        public ICommand AddTransactionLineCommand
        {
            get
            {
                return _addTransactionLineCommand ?? (_addTransactionLineCommand = new RelayCommand<Object>(SaveLine, CanSaveLine));
            }
        }
        private void SaveLine(object obj)
        {
            if (SelectedTransaction == null || SelectedItemQuantity == null ||
                SelectedTransaction.Status != TransactionStatus.Order)
            {
                MessageBox.Show("Can't add item, try again later....");
                return;
            }
            if (TransactionLine == null || (SelectedTransaction.TransactionType == TransactionTypes.Sale &&
                                            TransactionLine.UnitQuantity > TransactionLine.ItemCurrentQuantity))
            {
                MessageBox.Show("Can't add item, check item qty first and try again....");
                return;
            }
            var newObject = SelectedTransaction.Id;

            try
            {
                SelectedTransaction.BusinessPartnerId = SelectedBusinessPartner.Id;

                if (_isEdit == false)
                    SelectedTransactionLine = new TransactionLineDTO()
                    {
                        Transaction = SelectedTransaction,
                        TransactionId = SelectedTransaction.Id
                    };


                if (SelectedTransactionLine != null && TransactionLine.ItemQuantity != null)
                {
                    SelectedTransactionLine.ItemId = TransactionLine.ItemQuantity.ItemId;
                    SelectedTransactionLine.Unit = (decimal) TransactionLine.UnitQuantity;
                    SelectedTransactionLine.EachPrice = TransactionLine.EachPrice;

                    if (UnitPricePlusTax)
                        SelectedTransactionLine.EachPrice = Convert.ToDecimal(
                            (TransactionLine.EachPrice /
                             (1 + (Singleton.Setting.TaxPercent * (decimal)0.01))
                                ).ToString("N2"));

                    if (_transactionService != null)
                    {
                        var stat = _transactionService.InsertOrUpdateChild(SelectedTransactionLine);
                        if (stat == string.Empty)
                        {
                            if (newObject == 0)
                            {
                                if (Transactions != null) Transactions.Insert(0, SelectedTransaction);
                            }

                            GetSummary();
                            SelectedTransaction = SelectedTransaction;

                            var txtBox = obj as RadAutoCompleteBox;
                            if (txtBox != null) txtBox.Focus();
                        }
                        else
                            MessageBox.Show("Problem adding transaction item, please try again..." + Environment.NewLine +
                                            stat);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Problem adding transaction item, please try again..." + Environment.NewLine +
                    exception.Message + Environment.NewLine +
                    exception.InnerException);
            }
        }

        public ICommand EditTransactionLineCommand
        {
            get
            {
                return _editTransactionLineCommand ?? (_editTransactionLineCommand = new RelayCommand(EditLine, CanSave));
            }
        }
        private void EditLine()
        {
            if (SelectedTransactionLine == null || SelectedTransactionLine.Id == 0 ||
                SelectedTransaction.Status != TransactionStatus.Order)
            {
                MessageBox.Show("First choose Item to edit...", "Problem Editing");
                return;
            }

            try
            {
                _isEdit = true;
                if (ItemsQuantity != null && SelectedTransactionLine != null && SelectedTransactionLine.Transaction != null)
                {
                    var itemQuantity = ItemsQuantity
                        .FirstOrDefault(i => i.ItemId == SelectedTransactionLine.ItemId &&
                                             SelectedTransactionLine.Transaction.WarehouseId == i.WarehouseId);
                    if (itemQuantity == null)
                    {
                        //MessageBox.Show("Can't Edit Item, May be item doesn't exist  in the store...");
                        //return;
                        itemQuantity=new ItemQuantityDTO()
                        {
                            Item = SelectedTransactionLine.Item,
                            Warehouse = SelectedTransactionLine.Transaction.Warehouse,
                            ItemId = SelectedTransactionLine.ItemId,
                            WarehouseId = SelectedTransactionLine.Transaction.WarehouseId,
                        };
                    }
                    SelectedItemQuantity = itemQuantity;
                    TransactionLine = new TransactionLineModel
                    {
                        ItemQuantity = itemQuantity,
                        UnitQuantity = SelectedTransactionLine.Unit,
                        EachPrice = SelectedTransactionLine.EachPrice
                    };
                }
            }
            catch
            {
                MessageBox.Show("Can't Edit Item please try again...");
            }
        }

        public ICommand DeleteTransactionLineCommand
        {
            get
            {
                return _deleteTransactionLineCommand ?? (_deleteTransactionLineCommand = new RelayCommand<Object>(DeleteLine));
            }
        }
        private void DeleteLine(object obj)
        {
            if (SelectedTransactionLine == null || SelectedTransactionLine.Id == 0 ||
                SelectedTransaction.Status != TransactionStatus.Order)
            {
                MessageBox.Show("First choose Item to delete...", "Problem Deleting");
                return;
            }

            if (MessageBox.Show("Are you Sure You want to Delete this Item?", "Delete Item",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    SelectedTransactionLine.Enabled = false;

                    var stat = _transactionService.DisableChild(SelectedTransactionLine);
                    if (stat == string.Empty)
                    {
                        //TransactionLines.Remove(SelectedTransactionLine);
                        GetSummary();
                        SelectedTransaction = SelectedTransaction;

                        var txtBox = obj as RadAutoCompleteBox;
                        if (txtBox != null) txtBox.Focus();
                    }
                    else
                    {
                        MessageBox.Show("Can't Delete, may be the data is already in use..."
                                        + Environment.NewLine + stat, "Can't Delete",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
                catch (Exception exception)
                {
                    MessageBox.Show("Can't Delete, may be the data is already in use..."
                                    + Environment.NewLine + exception.Message + Environment.NewLine +
                                    exception.InnerException, "Can't Delete",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        #endregion

        #endregion

        #region Items Quantity
        private TransactionLineModel _transactionLine;
        private ObservableCollection<ItemQuantityDTO> _itemsQuantity;
        private ItemQuantityDTO _selectedItemQuantity;
        private ICommand _addNewItemCommand;

        public TransactionLineModel TransactionLine
        {
            get { return _transactionLine; }
            set
            {
                _transactionLine = value;
                RaisePropertyChanged<TransactionLineModel>(() => TransactionLine);
            }
        }
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
                    #region Checking for Repeated Items

                    if (!_isEdit)
                        foreach (var transactionLine in
                                TransactionLines.Where(transactionLine => transactionLine.ItemId == SelectedItemQuantity.ItemId))
                        {
                            if (MessageBox.Show(
                                "The item (" + transactionLine.Item.ItemCode + ") with qty of " +
                                transactionLine.Unit +
                                " is already in the list," + Environment.NewLine +
                                " Do you want to update it?", "Edit Line", MessageBoxButton.YesNoCancel,
                                MessageBoxImage.Warning) != MessageBoxResult.Yes)
                            {
                                _isEdit = false;
                                SelectedItemQuantity = null;
                                return;
                            }
                            else
                            {
                                SelectedTransactionLine = transactionLine;
                                TransactionLine = new TransactionLineModel
                                {
                                    ItemQuantity = SelectedItemQuantity,
                                    UnitQuantity = SelectedTransactionLine.Unit,
                                    EachPrice = SelectedTransactionLine.EachPrice
                                };
                                _isEdit = true;

                                return;
                            }

                        }

                    #endregion

                    TransactionLine = new TransactionLineModel
                    {
                        ItemQuantity = SelectedItemQuantity,
                        //UnitQuantity = 1,
                        EachPrice = SelectedItemQuantity.Item != null
                            ? Convert.ToDecimal((SelectedItemQuantity.Item.SellPrice /
                                                 (1 + (Singleton.Setting.TaxPercent * (decimal)0.01)))
                                .ToString("N2"))
                            : 0
                    };
                }
                else
                {
                    TransactionLine = new TransactionLineModel();
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

            //if (TransactionType == TransactionTypes.Sale)
            //    criteria.FiList.Add(p => p.QuantityOnHand > 0);

            int totalCount = 0;
            var itemQuantityList = new ItemQuantityService(false).GetAll(criteria,out totalCount)
                .OrderBy(p => p.Id).ToList();

            #region For Purchase Exclusive
            //if (TransactionType == TransactionTypes.Purchase)
            //{
                var items = new ItemService(true).GetAll().OrderBy(i => i.Id).ToList();

                var itemsQty = new List<ItemQuantityDTO>();
                foreach (var itemDTO in items)
                {
                    if (itemQuantityList.All(iq => iq.Item.Id != itemDTO.Id))
                    {
                        var itemQty = new ItemQuantityDTO()
                        {
                            Item = itemDTO,
                            QuantityOnHand = 0,
                            ItemId = itemDTO.Id,
                            Warehouse = SelectedWarehouse
                        };
                        itemsQty.Add(itemQty);
                    }
                }

                itemQuantityList = itemQuantityList.Concat(itemsQty).ToList();

            //}
            #endregion

            ItemsQuantity = new ObservableCollection<ItemQuantityDTO>(itemQuantityList);
        }

        public ICommand AddNewItemCommand
        {
            get
            {
                return _addNewItemCommand ?? (_addNewItemCommand = new RelayCommand(ExcuteAddNewItemCommand));
            }
        }
        private void ExcuteAddNewItemCommand()
        {
            try
            {
                //var itemWindow = new ItemDetail(null, SelectedWarehouse);
                SaveTransaction();
                var itemWindow=new ItemDetail(TransactionLines,SelectedTransaction);
                itemWindow.ShowDialog();
                //var dialogueResult = itemWindow.DialogResult;
                //if (dialogueResult != null && (bool)dialogueResult)
                //{
                    GetTransactionLines();
                    GetLiveItemsQuantity();
                    if (ItemsQuantity != null && ItemsQuantity.Any())
                        SelectedItemQuantity = ItemsQuantity.FirstOrDefault(it =>
                            it.Item.ItemCode == itemWindow.TxtItemCode.Text);
                //}
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't Add New Item"
                                  + Environment.NewLine + exception.Message, "Can't Add New Item", MessageBoxButton.OK,
                      MessageBoxImage.Error);
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
                if (SelectedWarehouse != null)
                {
                    MessageBox.Show(SelectedWarehouse.DisplayName +
                                      " is ACTIVE",
                                      SelectedWarehouse.DisplayName,
                                      MessageBoxButton.OK, MessageBoxImage.Exclamation);

                    AddNewTransactionEnability = SelectedWarehouse.Id != -1;
                    SaveHeaderEnability = SelectedWarehouse.Id != -1;
                    GetTransactions();
                    GetLiveItemsQuantity();
                }
            }
        }
        public void GetWarehouses()
        {
            Warehouses = Singleton.WarehousesList;
        }
        #endregion

        #region Business Partner

        #region Fields
        private ObservableCollection<BusinessPartnerDTO> _businessPartners;
        private ObservableCollection<BusinessPartnerDTO> _businessPartnersForFilter;

        private BusinessPartnerDTO _selectedBusinessPartner;
        private BusinessPartnerDTO _selectedBusinessPartnerForFilter;
        private BusinessPartnerTypes _businessPartnerType;
        #endregion

        #region Public Properties
        public ObservableCollection<BusinessPartnerDTO> BusinessPartners
        {
            get { return _businessPartners; }
            set
            {
                _businessPartners = value;
                RaisePropertyChanged<ObservableCollection<BusinessPartnerDTO>>(() => BusinessPartners);
                if (BusinessPartners.Any())
                    SelectedBusinessPartner = BusinessPartners.FirstOrDefault();
            }
        }
        public BusinessPartnerDTO SelectedBusinessPartner
        {
            get { return _selectedBusinessPartner; }
            set
            {
                _selectedBusinessPartner = value;
                RaisePropertyChanged<BusinessPartnerDTO>(() => SelectedBusinessPartner);
            }
        }

        public ObservableCollection<BusinessPartnerDTO> BusinessPartnersForFilter
        {
            get { return _businessPartnersForFilter; }
            set
            {
                _businessPartnersForFilter = value;
                RaisePropertyChanged<ObservableCollection<BusinessPartnerDTO>>(() => BusinessPartnersForFilter);
            }
        }
        public BusinessPartnerDTO SelectedBusinessPartnerForFilter
        {
            get { return _selectedBusinessPartnerForFilter; }
            set
            {
                _selectedBusinessPartnerForFilter = value;
                RaisePropertyChanged<BusinessPartnerDTO>(() => SelectedBusinessPartnerForFilter);
                if (SelectedBusinessPartnerForFilter != null)
                    GetTransactions();
            }
        }

        public BusinessPartnerTypes BusinessPartnerType
        {
            get { return _businessPartnerType; }
            set
            {
                _businessPartnerType = value;
                RaisePropertyChanged<BusinessPartnerTypes>(() => BusinessPartnerType);
            }
        }
        #endregion

        public void GetLiveBusinessPartners()
        {
            try
            {
                var criteria = new SearchCriteria<BusinessPartnerDTO>()
                {
                    TransactionType = (int)TransactionType
                };

                var bpList = new BusinessPartnerService()
                    .GetAll(criteria)
                    .OrderBy(i => i.Id)
                    .ToList();

                BusinessPartners = new ObservableCollection<BusinessPartnerDTO>(bpList);

                if (bpList.Count > 1)
                    bpList.Insert(0, new BusinessPartnerDTO
                    {
                        DisplayName = "All",
                        Id = -1
                    });

                BusinessPartnersForFilter = new ObservableCollection<BusinessPartnerDTO>(bpList);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't Load " + BusinessPartnerType
                                  + Environment.NewLine + exception.Message, "Can't Get " + BusinessPartnerType, MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }
        }

        #region Commands
        private ICommand _businessPartnerDetailViewCommand;
        public ICommand BusinessPartnerDetailViewCommand
        {
            get { return _businessPartnerDetailViewCommand ?? (_businessPartnerDetailViewCommand = new RelayCommand(ExecuteBusinessPartnerDetailViewCommand)); }
        }
        private void ExecuteBusinessPartnerDetailViewCommand()
        {
            try
            {
                var businessPartnerDetailWindow = new BusinessPartnerDetail(BusinessPartnerType);
                businessPartnerDetailWindow.ShowDialog();
                var dialogueResult = businessPartnerDetailWindow.DialogResult;
                if (dialogueResult != null && (bool)dialogueResult)
                {
                    GetLiveBusinessPartners();
                    if (BusinessPartners != null)
                        SelectedBusinessPartner =
                            BusinessPartners.FirstOrDefault(b => b.DisplayName == businessPartnerDetailWindow.TxtCustName.Text);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't Add New Customer"
                                  + Environment.NewLine + exception.Message, "Can't Add New Customer", MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }
        }

        #endregion

        #endregion

        #region Filter List

        #region Fields
        private IEnumerable<TransactionHeaderDTO> _transactionsList;
        private ObservableCollection<ListDataItem> _filterPeriods, _filterStatus;
        private ListDataItem _selectedPeriod, _selectedStatus;
        private TransactionHeaderDTO _selectedTransactionForFilter;
        private string _filterPeriod;
        private DateTime _filterStartDate, _filterEndDate;

        #endregion

        #region By Period
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
            IList<ListDataItem> filterPeriods2 = new List<ListDataItem>();
            filterPeriods2.Add(new ListDataItem { Display = "All", Value = 0 });
            filterPeriods2.Add(new ListDataItem { Display = "Today", Value = 1 });
            filterPeriods2.Add(new ListDataItem { Display = "Yesterday", Value = 2 });
            filterPeriods2.Add(new ListDataItem { Display = "This Week", Value = 3 });
            filterPeriods2.Add(new ListDataItem { Display = "Last Week", Value = 4 });
            FilterPeriods = new ObservableCollection<ListDataItem>(filterPeriods2);
        }
        public ObservableCollection<ListDataItem> FilterPeriods
        {
            get { return _filterPeriods; }
            set
            {
                _filterPeriods = value;
                RaisePropertyChanged<ObservableCollection<ListDataItem>>(() => FilterPeriods);
            }
        }
        public ListDataItem SelectedPeriod
        {
            get { return _selectedPeriod; }
            set
            {
                _selectedPeriod = value;
                RaisePropertyChanged<ListDataItem>(() => SelectedPeriod);
                if (SelectedPeriod != null)
                {
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
        }

        #endregion

        #region By Status
        private void FillStatusCombo()
        {
            //var filterStatus2 = new List<ListDataItem>
            //{
            //    new ListDataItem {Display = "All", Value = 0},
            //    new ListDataItem {Display = EnumUtil.GetEnumDesc(PaymentStatus.NoPayment), Value = 1},
            //    new ListDataItem {Display = EnumUtil.GetEnumDesc(PaymentStatus.NotCleared), Value = 2},
            //    new ListDataItem {Display = EnumUtil.GetEnumDesc(PaymentStatus.Cleared), Value = 3}
            //};
            var filterStatus3 = new List<ListDataItem>
            {
                new ListDataItem {Display = "All", Value = 0},
                new ListDataItem {Display = "Draft", Value = 1},
                //new ListDataItem {Display = EnumUtil.GetEnumDesc(PaymentStatus.NotCleared), Value = 2},
                new ListDataItem {Display = "Posted", Value = 3}
            };
            FilterStatus = new ObservableCollection<ListDataItem>(filterStatus3);
        }
        public ObservableCollection<ListDataItem> FilterStatus
        {
            get { return _filterStatus; }
            set
            {
                _filterStatus = value;
                RaisePropertyChanged<ObservableCollection<ListDataItem>>(() => FilterStatus);
            }
        }
        public ListDataItem SelectedStatus
        {
            get { return _selectedStatus; }
            set
            {
                _selectedStatus = value;
                RaisePropertyChanged<ListDataItem>(() => SelectedStatus);
                GetTransactions();
            }
        }
        //public IEnumerable<TransactionHeaderDTO> GetTransactionListForStatus()
        //{
        //    if (SelectedStatus == null) return TransactionList;
        //    switch (SelectedStatus.Value)
        //    {
        //        case 0:
        //            TransactionList = TransactionList;
        //            break;
        //        case 1:
        //            TransactionList = TransactionList.Where(s => s.PaymentCompleted == EnumUtil.GetEnumDesc(PaymentStatus.NoPayment)).ToList();
        //            break;
        //        case 2:
        //            TransactionList = TransactionList.Where(s => s.PaymentCompleted == EnumUtil.GetEnumDesc(PaymentStatus.NotCleared)).ToList();
        //            break;
        //        case 3:
        //            TransactionList = TransactionList.Where(s => s.PaymentCompleted == EnumUtil.GetEnumDesc(PaymentStatus.Cleared)).ToList();
        //            break;
        //    }
        //    return TransactionList;
        //}
        #endregion

        #region By Transaction
        public TransactionHeaderDTO SelectedTransactionForFilter
        {
            get { return _selectedTransactionForFilter; }
            set
            {
                _selectedTransactionForFilter = value;
                RaisePropertyChanged<TransactionHeaderDTO>(() => SelectedTransactionForFilter);

                Transactions = SelectedTransactionForFilter != null
                    ? new ObservableCollection<TransactionHeaderDTO>(TransactionList.Where(s => s.TransactionNumber == SelectedTransactionForFilter.TransactionNumber))
                    : new ObservableCollection<TransactionHeaderDTO>(TransactionList);
            }
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
        public bool CanSaveLine(object obj)
        {
            return LineErrors == 0;
        }
        #endregion

        #region Previlege Visibility
        private UserRolesModel _userRoles;
        private string _addTransaction, _postTransaction, _unpostTransaction, _deleteTransaction, _printTransactionVisibility;

        public UserRolesModel UserRoles
        {
            get { return _userRoles; }
            set
            {
                _userRoles = value;
                RaisePropertyChanged<UserRolesModel>(() => UserRoles);
            }
        }
        public string AddTransaction
        {
            get { return _addTransaction; }
            set
            {
                _addTransaction = value;
                RaisePropertyChanged<string>(() => AddTransaction);
            }
        }
        public string PostTransaction
        {
            get { return _postTransaction; }
            set
            {
                _postTransaction = value;
                RaisePropertyChanged<string>(() => PostTransaction);
            }
        }
        public string UnPostTransaction
        {
            get { return _unpostTransaction; }
            set
            {
                _unpostTransaction = value;
                RaisePropertyChanged<string>(() => UnPostTransaction);
            }
        }
        public string DeleteTransaction
        {
            get { return _deleteTransaction; }
            set
            {
                _deleteTransaction = value;
                RaisePropertyChanged<string>(() => DeleteTransaction);
            }
        }
        public string PrintTransactionVisibility
        {
            get { return _printTransactionVisibility; }
            set
            {
                _printTransactionVisibility = value;
                RaisePropertyChanged<string>(() => PrintTransactionVisibility);
            }
        }

        private void CheckRoles()
        {
            UserRoles = Singleton.UserRoles;
            if (UserRoles != null)
            {
                switch (TransactionType)
                {
                    case TransactionTypes.Sale:
                        AddTransaction = UserRoles.AddSales;
                        PostTransaction = UserRoles.PostSales;
                        UnPostTransaction = UserRoles.UnPostSales;
                        DeleteTransaction = UserRoles.DeleteSales;
                        break;
                    case TransactionTypes.Purchase:
                        AddTransaction = UserRoles.AddPurchase;
                        PostTransaction = UserRoles.PostPurchase;
                        UnPostTransaction = UserRoles.UnPostPurchase;
                        DeleteTransaction = UserRoles.DeletePurchase;
                        break;
                }
            }
            UnitPricePlusTaxVisibility = Singleton.Setting.TaxType == TaxTypes.NoTax ? "Collapsed" : "Visible";
        }
        #endregion

        #region Get Attachment
        private ICommand _printTransactionCommand;
        public ICommand PrintTransactionCommand
        {
            get
            {
                return _printTransactionCommand ?? (_printTransactionCommand = new RelayCommand<Object>(PrintTransaction));
            }
        }
        public void PrintTransaction(object obj)
        {
            new AttachmentEntry(SelectedTransaction).ShowDialog();
        }
        #endregion
    }
}
