using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AMStock.Core;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.Service;
using AMStock.WPF.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace AMStock.WPF.ViewModel
{
    public class AddPaymentViewModel : ViewModelBase
    {
        #region Fields
        private WarehouseDTO _warehouse;
        private BusinessPartnerDTO _businessPartner;

        private PaymentDTO _selectedPayment;
        private PaymentModel _selectedPaymentModel;
        private TransactionHeaderDTO _selectedTransaction, _selectedTransactionOld;
        private CheckDTO _selectedCheck;

        private ICommand _savePaymentCommand, _addCheckCommand, _closePaymentViewCommand;
        #endregion

        #region Constructor
        public AddPaymentViewModel()
        {
            Messenger.Default.Register<BusinessPartnerDTO>(this, (message) =>
            {
                BusinessPartner = message;
            });

            Messenger.Default.Register<PaymentDTO>(this, (message) =>
            {
                SelectedPayment = message;
            });
            Messenger.Default.Register<TransactionHeaderDTO>(this, (message) =>
            {
                SelectedTransactionOld = message;
            });

        }
        #endregion

        #region Properties
        public WarehouseDTO Warehouse
        {
            get { return _warehouse; }
            set
            {
                _warehouse = value;
                RaisePropertyChanged<WarehouseDTO>(() => Warehouse);
            }
        }
        public BusinessPartnerDTO BusinessPartner
        {
            get { return _businessPartner; }
            set
            {
                _businessPartner = value;
                RaisePropertyChanged<BusinessPartnerDTO>(() => BusinessPartner);
                if (BusinessPartner != null)
                {
                }
            }
        }
        public TransactionHeaderDTO SelectedTransactionOld
        {
            get { return _selectedTransactionOld; }
            set
            {
                _selectedTransactionOld = value;
                RaisePropertyChanged<TransactionHeaderDTO>(() => SelectedTransactionOld);

                if (SelectedTransactionOld != null)
                {
                    var cri = new SearchCriteria<TransactionHeaderDTO>
                    {
                        CurrentUserId = Singleton.User.UserId
                    };
                    cri.FiList.Add(t => t.Id == SelectedTransactionOld.Id);
                    SelectedTransaction = new TransactionService(true).GetAll(cri)
                                                    .FirstOrDefault();

                }
            }
        }
        public TransactionHeaderDTO SelectedTransaction
        {
            get { return _selectedTransaction; }
            set
            {
                _selectedTransaction = value;
                RaisePropertyChanged<TransactionHeaderDTO>(() => SelectedTransaction);
                if (SelectedTransaction != null)
                {
                    BusinessPartner = SelectedTransaction.BusinessPartner;
                    Warehouse = SelectedTransaction.Warehouse;
                    SelectedPaymentModel = new PaymentModel
                    {
                        AmountRequired = SelectedTransaction.TotalDue,
                        PaymentDate = DateTime.Now,
                        Amount = SelectedTransaction.TotalDue,
                        ReachedLimit = ReachedCreditLimit()
                    };
                }
            }
        }
        public PaymentDTO SelectedPayment
        {
            get { return _selectedPayment; }
            set
            {
                _selectedPayment = value;
                RaisePropertyChanged<PaymentDTO>(() => SelectedPayment);

                if (SelectedPayment != null)
                {
                    if (SelectedPayment.TransactionId != 0)
                    {
                        var cri = new SearchCriteria<TransactionHeaderDTO>
                        {
                            CurrentUserId = Singleton.User.UserId
                        };
                        cri.FiList.Add(t => t.Id == SelectedPayment.TransactionId);
                        SelectedTransaction = new TransactionService(true).GetAll(cri)
                           .FirstOrDefault();
                    }

                    SelectedPaymentModel = new PaymentModel
                    {
                        AmountRequired = SelectedPayment.Amount,
                        PaymentDate = DateTime.Now,
                        Amount = SelectedPayment.Amount,
                        ReachedLimit = ReachedCreditLimit()
                    };
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
        public PaymentModel SelectedPaymentModel
        {
            get { return _selectedPaymentModel; }
            set
            {
                _selectedPaymentModel = value;
                RaisePropertyChanged<PaymentModel>(() => SelectedPaymentModel);
            }
        }

        public bool ReachedCreditLimit()
        {
            if (SelectedPaymentModel != null && SelectedPaymentModel.AmountLeft > 0)
            {
                var setting = Singleton.Setting;
                if (setting.CheckCreditLimit)
                {
                    if (setting.CreditLimitType == CreditLimitTypes.Both || setting.CreditLimitType == CreditLimitTypes.Amount)
                        if (BusinessPartner.CreditLimit < (BusinessPartner.TotalCredit + SelectedPaymentModel.AmountLeft))
                        {
                            MessageBox.Show("Above allowed credit amount limit!", "Over Credit limit");
                            return true;
                        }

                    if (setting.CreditLimitType == CreditLimitTypes.Both || setting.CreditLimitType == CreditLimitTypes.Transactions)
                        if (BusinessPartner.MaxNoCreditTransactions < (BusinessPartner.TotalNoOfOutstandingTransactions + 1))
                        {
                            MessageBox.Show("Above allowed credit transactions limit!", "Over Credit limit");
                            return true;
                        }
                }
            }
            return false;
        }
        #endregion

        #region Commands
        public ICommand SavePaymentViewCommand
        {
            get { return _savePaymentCommand ?? (_savePaymentCommand = new RelayCommand<Object>(SavePayment, CanSave)); }
        }
        private void SavePayment(object obj)
        {
            if (ReachedCreditLimit()) return;

            if (SelectedTransaction != null || SelectedPayment != null)
            {
                var stat = new PaymentService().PostPayments(SelectedTransaction, SelectedPayment, SelectedPaymentModel, SelectedCheck);
                if (stat == string.Empty)
                    CloseWindow(obj);
                else
                    MessageBox.Show(stat);
            }
        }

        public ICommand AddCheckCommand
        {
            get { return _addCheckCommand ?? (_addCheckCommand = new RelayCommand(AddCheck)); }
        }
        private void AddCheck()
        {
            var dueDate = DateTime.Now.AddDays(BusinessPartner.PaymentTerm);
            if (SelectedCheck == null)
            {
                SelectedCheck = new CheckDTO
                {
                    CheckDate = dueDate,
                    CheckDueDate = dueDate,
                    CheckAmount = SelectedPaymentModel.AmountLeft.ToString("N"),
                    CustomerBankAccount = new FinancialAccountDTO()
                };
            }

            var checkEntry = new CheckEntry(SelectedCheck);
            checkEntry.ShowDialog();
            if (checkEntry.DialogResult != null && !(bool)checkEntry.DialogResult)
                SelectedCheck = null;
        }

        public ICommand ClosePaymentViewCommand
        {
            get
            {
                return _closePaymentViewCommand ?? (_closePaymentViewCommand = new RelayCommand<Object>(CloseWindow));
            }
        }
        public void CloseWindow(object obj)
        {
            if (obj == null) return;
            var window = obj as Window;
            if (window == null) return;
            window.DialogResult = true;
            window.Close();
        }
        #endregion

        #region Validation
        public static int Errors { get; set; }
        public bool CanSave(object obj)
        {
            return Errors == 0;
        }
        #endregion

    }
}
