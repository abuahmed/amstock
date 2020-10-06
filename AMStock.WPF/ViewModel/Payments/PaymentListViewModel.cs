using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using AMStock.Core;
using AMStock.Core.Common;
using AMStock.Core.Enumerations;
using AMStock.Core.Extensions;
using AMStock.Core.Models;
using AMStock.DAL;
using AMStock.Repository;
using AMStock.Repository.Interfaces;
using AMStock.Service;
using AMStock.Service.Interfaces;
using AMStock.WPF.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MessageBox = System.Windows.MessageBox;

namespace AMStock.WPF.ViewModel
{
    public class PaymentListViewModel : ViewModelBase
    {
        #region Fields
        //private IUnitOfWork _unitOfWork;
        private ICommand _addNewPaymentCommand, _refreshCommand;
        private ICommand _convertCheckToCreditCommand, _deleteClearanceCommand;
        private bool _addNewPaymentCommandVisibility;
        private string _headerText, _businessPartner, _paymentActionContent, _paymentActionVisibility;
        private string _checkExpanderVisibility, _clearanceExpanderVisibility;
        private TransactionTypes _transaction;
        private PaymentListTypes _paymentListType;
        private PaymentDTO _selectedPayment;
        private PaymentClearanceDTO _selectedPaymentClearance;
        private CheckDTO _selectedCheck;
        private IEnumerable<PaymentDTO> _paymentList;
        private ObservableCollection<PaymentDTO> _payments;
        private ObservableCollection<TransactionLineDTO> _transactionLines;
        private int _totalNumberOfPayments;
        private string _totalValueOfPayments, _totalValueOfPurchases;
        #endregion

        #region Constructor

        private static IPaymentService _paymentService;
        private static IUnitOfWork _unitOfWork;

        public PaymentListViewModel()
        {
            CleanUp();
            var dbContext = DbContextUtil.GetDbContextInstance();
            _paymentService = new PaymentService(dbContext);
            _unitOfWork = new UnitOfWork(dbContext);

            FillCombos();
            CheckRoles();
            
            var toDay = DateTime.Now.AddDays(1);
            var fifteendaysAgo = DateTime.Now.AddYears(-1);

            FilterStartDate = new DateTime(fifteendaysAgo.Year, fifteendaysAgo.Month, fifteendaysAgo.Day, 0, 0, 0);
            FilterEndDate = new DateTime(toDay.Year, toDay.Month, toDay.Day, 23, 59, 59);

            Messenger.Default.Register<BusinessPartnerDTO>(this, (message) => { SelectedBusinessPartner = message; });
            Messenger.Default.Register<TransactionTypes>(this, (message) => { Transaction = message; });
            Messenger.Default.Register<PaymentListTypes>(this, (message) => { PaymentListType = message; });

            GetWarehouses();

            if (Warehouses != null && Warehouses.Any())
            {
                if (SelectedWarehouse == null)
                    SelectedWarehouse = Warehouses.FirstOrDefault(w => w.IsDefault) ?? Warehouses.FirstOrDefault();
                else
                    SelectedWarehouse = SelectedWarehouse;
            }
            
            ClearanceExpanderVisibility = "Collapsed";
            CheckExpanderVisibility = "Collapsed";
            PaymentActionVisibility = "Collapsed";
        }

        public static void CleanUp()
        {
            if (_paymentService != null)
                _paymentService.Dispose();
            if (_unitOfWork != null)
                _unitOfWork.Dispose();
        }
        #endregion

        #region Public Properties
        public string HeaderText
        {
            get { return _headerText; }
            set
            {
                _headerText = value;
                RaisePropertyChanged<string>(() => HeaderText);
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
        public string PaymentActionContent
        {
            get { return _paymentActionContent; }
            set
            {
                _paymentActionContent = value;
                RaisePropertyChanged<string>(() => PaymentActionContent);
            }
        }
        public string PaymentActionVisibility
        {
            get { return _paymentActionVisibility; }
            set
            {
                _paymentActionVisibility = value;
                RaisePropertyChanged<string>(() => PaymentActionVisibility);
            }
        }
        public bool AddNewPaymentCommandVisibility
        {
            get { return _addNewPaymentCommandVisibility; }
            set
            {
                _addNewPaymentCommandVisibility = value;
                RaisePropertyChanged<bool>(() => AddNewPaymentCommandVisibility);
            }
        }

        public int TotalNumberOfPayments
        {
            get { return _totalNumberOfPayments; }
            set
            {
                _totalNumberOfPayments = value;
                RaisePropertyChanged<int>(() => TotalNumberOfPayments);
            }
        }
        public string TotalValueOfPayments
        {
            get { return _totalValueOfPayments; }
            set
            {
                _totalValueOfPayments = value;
                RaisePropertyChanged<string>(() => TotalValueOfPayments);
            }
        }
        public string TotalValueOfPurchases
        {
            get { return _totalValueOfPurchases; }
            set
            {
                _totalValueOfPurchases = value;
                RaisePropertyChanged<string>(() => TotalValueOfPurchases);
            }
        }
        public PaymentDTO SelectedPayment
        {
            get { return _selectedPayment; }
            set
            {
                _selectedPayment = value;
                RaisePropertyChanged<PaymentDTO>(() => SelectedPayment);

                CheckExpanderVisibility = "Collapsed";
                ClearanceExpanderVisibility = "Collapsed";
                PaymentActionVisibility = "Collapsed";

                if (SelectedPayment != null)
                {
                    if (SelectedPayment.Status == PaymentStatus.NotDeposited)
                    {
                        PaymentActionVisibility = UserRoles.DepositPayments == "Visible" ? "Visible" : "Collapsed";
                        PaymentActionContent = "Deposit Payment";
                    }
                    else if (SelectedPayment.Status == PaymentStatus.NotCleared)
                    {
                        PaymentActionVisibility = UserRoles.ClearPayments == "Visible" ? "Visible" : "Collapsed";
                        PaymentActionContent = Singleton.Setting.HandleBankTransaction ? "Clear Payment" : "Deposit Check";
                    }
                    if (SelectedPayment.PaymentMethod == PaymentMethods.Credit)
                        PaymentActionContent = "Add Payment";

                    if (SelectedPayment.PaymentMethod == PaymentMethods.Cash && SelectedPayment.Status != PaymentStatus.NotDeposited)
                    {
                        ClearanceExpanderVisibility = Singleton.Setting.HandleBankTransaction ? "Visible" : "Collapsed";
                        SelectedPaymentClearance = SelectedPayment.Clearance;
                    }
                    if (SelectedPayment.PaymentMethod == PaymentMethods.Check)
                    {
                        CheckExpanderVisibility = "Visible";
                        if (SelectedPayment.Check != null) SelectedCheck = SelectedPayment.Check;
                        if (SelectedPayment.Status == PaymentStatus.Cleared)
                        {
                            ClearanceExpanderVisibility = Singleton.Setting.HandleBankTransaction ? "Visible" : "Collapsed";
                            SelectedPaymentClearance = SelectedPayment.Clearance;
                        }
                    }

                    var lines = _unitOfWork.Repository<TransactionLineDTO>()
                        .Query()
                        .Include(i => i.Item)
                        .Filter(t => t.TransactionId == SelectedPayment.TransactionId)
                        .Get();
                    TransactionLines = new ObservableCollection<TransactionLineDTO>(lines);
                }
            }
        }
        public CheckDTO SelectedCheck
        {
            get { return _selectedCheck; }
            set
            {
                _selectedCheck = value;
                RaisePropertyChanged<CheckDTO>(() => SelectedCheck);
            }
        }
        public PaymentClearanceDTO SelectedPaymentClearance
        {
            get { return _selectedPaymentClearance; }
            set
            {
                _selectedPaymentClearance = value;
                RaisePropertyChanged<PaymentClearanceDTO>(() => SelectedPaymentClearance);
            }
        }
        public IEnumerable<PaymentDTO> PaymentList
        {
            get { return _paymentList; }
            set
            {
                _paymentList = value;
                RaisePropertyChanged<IEnumerable<PaymentDTO>>(() => PaymentList);
            }
        }
        public ObservableCollection<PaymentDTO> Payments
        {
            get { return _payments; }
            set
            {
                _payments = value;
                RaisePropertyChanged<IEnumerable<PaymentDTO>>(() => Payments);

                TotalNumberOfPayments = Payments.Count;
                TotalValueOfPayments = Payments.Sum(s => Convert.ToDecimal(s.Amount)).ToString("N");
                if (Payments.Any())
                    SelectedPayment = Payments.FirstOrDefault();
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
                GetPayments();

                switch (Transaction)
                {
                    case TransactionTypes.Sale:
                        HeaderText = "Sales Payments";
                        BusinessPartner = "Customer";
                        break;
                    case TransactionTypes.Purchase:
                        HeaderText = "Purchase Payments";
                        BusinessPartner = "Supplier";
                        break;
                }
            }
        }
        public ObservableCollection<TransactionLineDTO> TransactionLines
        {
            get { return _transactionLines; }
            set
            {
                _transactionLines = value;
                RaisePropertyChanged<IEnumerable<TransactionLineDTO>>(() => TransactionLines);
            }
        }
        public PaymentListTypes PaymentListType
        {
            get { return _paymentListType; }
            set
            {
                _paymentListType = value;
                RaisePropertyChanged<PaymentListTypes>(() => PaymentListType);
                SelectedPaymentStatusList =
                    PaymentStatusList.FirstOrDefault(l => l.Value == (int)PaymentListType);
            }
        }

        public string CheckExpanderVisibility
        {
            get { return _checkExpanderVisibility; }
            set
            {
                _checkExpanderVisibility = value;
                RaisePropertyChanged<string>(() => this.CheckExpanderVisibility);
            }
        }
        public string ClearanceExpanderVisibility
        {
            get { return _clearanceExpanderVisibility; }
            set
            {
                _clearanceExpanderVisibility = value;
                RaisePropertyChanged<string>(() => this.ClearanceExpanderVisibility);
            }
        }
        #endregion

        #region Commands
        public ICommand RefreshCommand
        {
            get
            {
                return _refreshCommand ?? (_refreshCommand = new RelayCommand(ExcuteRefreshCommand));
            }
        }
        private void ExcuteRefreshCommand()
        {
            GetPayments();
        }

        public ICommand AddNewPaymentCommand
        {
            get
            {
                return _addNewPaymentCommand ?? (_addNewPaymentCommand = new RelayCommand(ExcuteAddNewPaymentCommand));
            }
        }
        private void ExcuteAddNewPaymentCommand()
        {
            if (SelectedPayment != null)
            {
                if (SelectedPayment.PaymentMethod == PaymentMethods.Credit)
                    new AddPayment(SelectedPayment).ShowDialog();
                else if (SelectedPayment.Status != PaymentStatus.Cleared)
                {
                    if (Singleton.Setting.HandleBankTransaction)
                        new PaymentClearance(SelectedPayment).ShowDialog();
                    else
                    {
                        try
                        {
                            SelectedPayment.Status = PaymentStatus.Cleared;
                            _unitOfWork.Repository<PaymentDTO>().Update(SelectedPayment);
                        }
                        catch
                        {
                            MessageBox.Show("Can't deposit payment, try again...");
                        }
                    }
                }

                GetPayments();
            }
        }

        public ICommand ConvertCheckToCreditCommand
        {
            get { return _convertCheckToCreditCommand ?? (_convertCheckToCreditCommand = new RelayCommand(ExecuteConvertCheckToCreditCommand)); }
        }
        public void ExecuteConvertCheckToCreditCommand()
        {
            if (SelectedPayment.Transaction.BusinessPartner.AllowCreditsWithoutCheck && SelectedPayment.Clearance == null)//(SelectedPayment.Transaction.BusinessPartner.PaymentMethod == PaymentMethods.Credit && SelectedPayment.Clearance == null)
            {
                try
                {
                    SelectedCheck.Enabled = false;
                    _unitOfWork.Repository<CheckDTO>().Update(SelectedCheck);

                    SelectedPayment.PaymentMethod = PaymentMethods.Credit;
                    SelectedPayment.CheckId = null;

                    _unitOfWork.Repository<PaymentDTO>().Update(SelectedPayment);

                    _unitOfWork.Commit();
                }
                catch
                {
                    MessageBox.Show("Problem converting check to credit");
                }
            }
            else
            {
                MessageBox.Show("Problem converting check to credit");
            }
        }

        public ICommand DeleteClearanceCommand
        {
            get { return _deleteClearanceCommand ?? (_deleteClearanceCommand = new RelayCommand(ExecuteDeleteClearanceCommand)); }
        }
        public void ExecuteDeleteClearanceCommand()
        {
            try
            {
                //if(SelectedPayment.PaymentMethod==PaymentMethods.Cash)
                SelectedPaymentClearance.Enabled = false;
                _unitOfWork.Repository<PaymentClearanceDTO>().Update(SelectedPaymentClearance);

                SelectedPayment.Status = SelectedPayment.PaymentMethod == PaymentMethods.Cash ? PaymentStatus.NotDeposited : PaymentStatus.NotCleared;

                SelectedPayment.ClearanceId = null;

                _unitOfWork.Repository<PaymentDTO>().Update(SelectedPayment);

                _unitOfWork.Commit();
            }
            catch
            {
                MessageBox.Show("Problem deleting clearance...");
            }
        }
        #endregion

        public void GetPayments()
        {
            PaymentList = new List<PaymentDTO>();
            TransactionLines=new ObservableCollection<TransactionLineDTO>();

            var criteria = new SearchCriteria<PaymentDTO>
            {
                CurrentUserId = Singleton.User.UserId,
                TransactionType = (int)Transaction,
                BeginingDate = FilterStartDate,
                EndingDate = FilterEndDate
            };

            criteria.FiList.Add(p => p.Transaction != null);

            if (SelectedBusinessPartner != null && SelectedBusinessPartner.Id != -1)
                criteria.FiList.Add(p => p.Transaction.BusinessPartnerId == SelectedBusinessPartner.Id);

            if (SelectedPaymentStatusList != null)
                criteria.PaymentListType = SelectedPaymentStatusList.Value;

            if (SelectedPaymentMethodList != null)
                criteria.PaymentMethodType = SelectedPaymentMethodList.Value;

            if (SelectedWarehouse != null && SelectedWarehouse.Id != -1)
            {
                criteria.SelectedWarehouseId = SelectedWarehouse.Id;
            }

            PaymentList = _paymentService.GetAll(criteria);


            Payments = new ObservableCollection<PaymentDTO>(PaymentList);

        }

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
                ////GetPayments();
            }
        }
        public void GetWarehouses()
        {
            Warehouses = Singleton.WarehousesList;
        }
        #endregion

        #region Filter List
        private DateTime _filterStartDate, _filterEndDate;
        private string _filterByPerson, _filterByReason;
        private PaymentListTypes _selectedPaymentStatus;
        private List<ListDataItem> _paymentStatusList, _paymentMethodList;
        private ListDataItem _selectedPaymentStatusList, _selectedPaymentMethodList;

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
                //GetPayments();
            }
        }
        public string FilterByReason
        {
            get { return _filterByReason; }
            set
            {
                _filterByReason = value;
                RaisePropertyChanged<string>(() => FilterByReason);
                //GetPayments();
            }
        }

        public List<ListDataItem> PaymentStatusList
        {
            get { return _paymentStatusList; }
            set
            {
                _paymentStatusList = value;
                RaisePropertyChanged<List<ListDataItem>>(() => PaymentStatusList);
            }
        }
        public List<ListDataItem> PaymentMethodList
        {
            get { return _paymentMethodList; }
            set
            {
                _paymentMethodList = value;
                RaisePropertyChanged<List<ListDataItem>>(() => PaymentMethodList);
            }
        }

        public ListDataItem SelectedPaymentStatusList
        {
            get { return _selectedPaymentStatusList; }
            set
            {
                _selectedPaymentStatusList = value;
                RaisePropertyChanged<ListDataItem>(() => SelectedPaymentStatusList);
                //GetPayments();
            }
        }
        public ListDataItem SelectedPaymentMethodList
        {
            get { return _selectedPaymentMethodList; }
            set
            {
                _selectedPaymentMethodList = value;
                RaisePropertyChanged<ListDataItem>(() => SelectedPaymentMethodList);
                //GetPayments();
            }
        }
        public PaymentListTypes SelectedPaymentStatus
        {
            get { return _selectedPaymentStatus; }
            set
            {
                _selectedPaymentStatus = value;
                RaisePropertyChanged<PaymentListTypes>(() => SelectedPaymentStatus);
                //GetPayments();
            }
        }

        private void FillCombos()
        {
            if (Singleton.Setting.HandleBankTransaction)
            {
                PaymentStatusList = new List<ListDataItem>
                {
                new ListDataItem {Display = "All", Value = 0},
                new ListDataItem {Display = EnumUtil.GetEnumDesc(PaymentListTypes.Cleared), Value = (int)PaymentListTypes.Cleared},
                new ListDataItem {Display = EnumUtil.GetEnumDesc(PaymentListTypes.NotCleared), Value =  (int)PaymentListTypes.NotCleared},
                new ListDataItem {Display = EnumUtil.GetEnumDesc(PaymentListTypes.NotClearedandOverdue), Value =  (int)PaymentListTypes.NotClearedandOverdue},
                new ListDataItem {Display = EnumUtil.GetEnumDesc(PaymentListTypes.NotDeposited), Value =  (int)PaymentListTypes.NotDeposited},
                new ListDataItem {Display = EnumUtil.GetEnumDesc(PaymentListTypes.DepositedNotCleared), Value =  (int)PaymentListTypes.DepositedNotCleared},
                new ListDataItem {Display = EnumUtil.GetEnumDesc(PaymentListTypes.DepositedCleared), Value =  (int)PaymentListTypes.DepositedCleared},
                new ListDataItem {Display = EnumUtil.GetEnumDesc(PaymentListTypes.CreditNotCleared), Value =  (int)PaymentListTypes.CreditNotCleared},
                new ListDataItem {Display = EnumUtil.GetEnumDesc(PaymentListTypes.CheckNotCleared), Value =  (int)PaymentListTypes.CheckNotCleared},
                new ListDataItem {Display = EnumUtil.GetEnumDesc(PaymentListTypes.CheckCleared), Value =  (int)PaymentListTypes.CheckCleared}
                };
            }
            else
            {
                PaymentStatusList = new List<ListDataItem>
                {
                new ListDataItem {Display = "All", Value = 0},
                new ListDataItem {Display = EnumUtil.GetEnumDesc(PaymentListTypes.Cleared), Value = (int)PaymentListTypes.Cleared},
                new ListDataItem {Display = EnumUtil.GetEnumDesc(PaymentListTypes.NotCleared), Value =  (int)PaymentListTypes.NotCleared},
                new ListDataItem {Display = EnumUtil.GetEnumDesc(PaymentListTypes.NotClearedandOverdue), Value =  (int)PaymentListTypes.NotClearedandOverdue},
                };
            }

            PaymentMethodList = new List<ListDataItem>
                {
                new ListDataItem {Display = "All", Value = -1},
                new ListDataItem {Display = PaymentMethods.Cash.ToString(), Value = (int)PaymentMethods.Cash},
                new ListDataItem {Display = PaymentMethods.Credit.ToString(), Value =  (int)PaymentMethods.Credit},
                new ListDataItem {Display = PaymentMethods.Check.ToString(), Value =  (int)PaymentMethods.Check},
                };
        }
        #endregion

        #region Business Partner
        private ObservableCollection<BusinessPartnerDTO> _businessPartners;
        private BusinessPartnerDTO _selectedBusinessPartner, _businessPartnerFilter;

        public ObservableCollection<BusinessPartnerDTO> BusinessPartners
        {
            get { return _businessPartners; }
            set
            {
                _businessPartners = value;
                RaisePropertyChanged<ObservableCollection<BusinessPartnerDTO>>(() => BusinessPartners);
            }
        }
        public BusinessPartnerDTO SelectedBusinessPartnerFilter
        {
            get { return _businessPartnerFilter; }
            set
            {
                _businessPartnerFilter = value;
                RaisePropertyChanged<BusinessPartnerDTO>(() => SelectedBusinessPartnerFilter);
                if (SelectedBusinessPartnerFilter != null)
                {
                    SelectedBusinessPartner =
                        BusinessPartners.FirstOrDefault(bp => bp.Id == SelectedBusinessPartnerFilter.Id);
                }
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
        private void GetLiveBusinessPartners()
        {
            var criteria = new SearchCriteria<BusinessPartnerDTO>
            {
                TransactionType = (int)Transaction
            };

            IList<BusinessPartnerDTO> bpList = new BusinessPartnerService(true)
                .GetAll(criteria)
                .OrderByDescending(i => i.Id)
                .ToList();

            if (bpList.Count > 1)
                bpList.Insert(0, new BusinessPartnerDTO
                {
                    DisplayName = "All",
                    Id = -1
                });
            BusinessPartners = new ObservableCollection<BusinessPartnerDTO>(bpList);
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
            UserRoles = new UserRolesModel();
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
            string[] columnsHeader = {"Store", "Payment Date","Method","Transaction No.",BusinessPartner,"No of Items",
                                         "Total Cost","Amount","Status"};

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
            var xlApp = new Microsoft.Office.Interop.Excel.Application();

            try
            {
                Microsoft.Office.Interop.Excel.Workbook excelBook = xlApp.Workbooks.Add();
                var excelWorksheet = (Microsoft.Office.Interop.Excel.Worksheet)excelBook.Worksheets[1];
                xlApp.Visible = true;

                int rowsTotal = Payments.Count;
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
                    with1.Cells[I + 2, 0 + 1].value = Payments[I].Transaction.Warehouse.DisplayName;
                    with1.Cells[I + 2, 1 + 1].value = Payments[I].PaymentDateString;
                    with1.Cells[I + 2, 2 + 1].value = Payments[I].PaymentMethodString;
                    with1.Cells[I + 2, 3 + 1].value = Payments[I].Transaction.TransactionNumber;
                    with1.Cells[I + 2, 4 + 1].value = Payments[I].Transaction.BusinessPartner.DisplayName;
                    with1.Cells[I + 2, 5 + 1].value = Payments[I].Transaction.CountLines;
                    with1.Cells[I + 2, 6 + 1].value = Payments[I].Transaction.TotalCost;
                    with1.Cells[I + 2, 7 + 1].value = Payments[I].Amount;
                    with1.Cells[I + 2, 8 + 1].value = Payments[I].StatusString;
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
                System.Windows.Forms.MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
