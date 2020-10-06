using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.DAL;
using AMStock.Repository;
using AMStock.Repository.Interfaces;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace AMStock.WPF.ViewModel
{
    public class PaymentClearanceViewModel : ViewModelBase
    {
        #region Fileds
        private static IUnitOfWork _unitOfWork;
        private PaymentDTO _selectedPayment;
        private CheckDTO _selectedCheck;
        private ICommand _addPaymentCommand;
        private ObservableCollection<FinancialAccountDTO> _financialAccounts;
        private FinancialAccountDTO _selectedClientFinancialAccount;
        private PaymentClearanceDTO _selectedPaymentClearance;
        private string _headerText, _depositVisibility, _clearanceVisibility;
        #endregion

        #region Constructor
        public PaymentClearanceViewModel()
        {
            CleanUp();
            _unitOfWork = new UnitOfWork(DbContextUtil.GetDbContextInstance());
            LoadClientAcounts();

            Messenger.Default.Register<PaymentDTO>(this, (message) =>
            {
                SelectedPayment = _unitOfWork.Repository<PaymentDTO>().FindById(message.Id);
            });

        }

        public static void CleanUp()
        {
            if (_unitOfWork != null)
                _unitOfWork.Dispose();
        }
        #endregion

        #region Properties

        public PaymentDTO SelectedPayment
        {
            get { return _selectedPayment; }
            set
            {
                _selectedPayment = value;
                RaisePropertyChanged<PaymentDTO>(() => SelectedPayment);
                if (SelectedPayment != null)
                {
                    if (SelectedPayment.Status == PaymentStatus.NotDeposited)
                    {
                        HeaderText = "Deposit Payment";
                        DepositVisibility = "Visible";
                        ClearanceVisibility = "Collapsed";

                        SelectedPaymentClearance = new PaymentClearanceDTO
                        {
                            DepositedOnDate = DateTime.Now,
                            //DepositedById = 3
                        };
                    }
                    else
                    {
                        HeaderText = "Clear Payment";
                        DepositVisibility = "Collapsed";
                        ClearanceVisibility = "Visible";

                        SelectedPaymentClearance = _unitOfWork.Repository<PaymentClearanceDTO>()
                            .Query().Filter(p => p.Id == SelectedPayment.ClearanceId).Get()
                            .FirstOrDefault();

                        if (SelectedPaymentClearance != null)
                        {
                            SelectedPaymentClearance.ClearedOnDate = DateTime.Now;
                            //SelectedPaymentClearance.ClearedById = 3;
                        }
                        else
                        {
                            SelectedPaymentClearance = new PaymentClearanceDTO
                            {
                                ClearedOnDate = DateTime.Now,
                                //ClearedById = 3
                            };
                        }
                    }
                }
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
        public CheckDTO SelectedCheck
        {
            get { return _selectedCheck; }
            set
            {
                _selectedCheck = value;
                RaisePropertyChanged<CheckDTO>(() => SelectedCheck);
            }
        }

        public string HeaderText
        {
            get { return _headerText; }
            set
            {
                _headerText = value;
                RaisePropertyChanged<string>(() => this.HeaderText);
            }
        }
        public string DepositVisibility
        {
            get { return _depositVisibility; }
            set
            {
                _depositVisibility = value;
                RaisePropertyChanged<string>(() => this.DepositVisibility);
            }
        }
        public string ClearanceVisibility
        {
            get { return _clearanceVisibility; }
            set
            {
                _clearanceVisibility = value;
                RaisePropertyChanged<string>(() => this.ClearanceVisibility);
            }
        }
        #endregion

        #region Commands
        public ICommand AddStatementCommand
        {
            get { return _addPaymentCommand ?? (_addPaymentCommand = new RelayCommand<Object>(ExecuteAddStatementCommand, CanSave)); }
        }
        private void ExecuteAddStatementCommand(object obj)
        {
            try
            {
                if (SelectedPaymentClearance != null && SelectedPayment != null)
                {
                    SelectedPaymentClearance.ClientAccountId = SelectedClientFinancialAccount.Id;

                    switch (SelectedPayment.Status)
                    {
                        case PaymentStatus.NotDeposited:
                            SelectedPayment.Status = PaymentStatus.NotCleared;
                            break;
                        case PaymentStatus.NotCleared:
                            SelectedPayment.Status = PaymentStatus.Cleared;
                            break;
                    }

                    if (SelectedPaymentClearance.Id == 0)
                        _unitOfWork.Repository<PaymentClearanceDTO>().Insert(SelectedPaymentClearance);
                    else
                        _unitOfWork.Repository<PaymentClearanceDTO>().Update(SelectedPaymentClearance);

                    SelectedPayment.Clearance = SelectedPaymentClearance;

                    _unitOfWork.Repository<PaymentDTO>().Update(SelectedPayment);


                    _unitOfWork.Commit();
                    CloseWindow(obj);
                }
            }
            catch
            {
                MessageBox.Show("Got problem while clearing Payment!!", "Clearance Problem");
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

        #region ClientAcounts
        public void LoadClientAcounts()
        {
            IEnumerable<FinancialAccountDTO> categoriesList = _unitOfWork.Repository<FinancialAccountDTO>()
                .Query()
                .Filter(f => f.WarehouseId != null)
                .Get()
                .OrderBy(i => i.Id).ToList();
            ClientAcounts = new ObservableCollection<FinancialAccountDTO>(categoriesList);

            if (ClientAcounts.Any())
                SelectedClientFinancialAccount = ClientAcounts.FirstOrDefault();
        }

        public FinancialAccountDTO SelectedClientFinancialAccount
        {
            get { return _selectedClientFinancialAccount; }
            set
            {
                _selectedClientFinancialAccount = value;
                RaisePropertyChanged<FinancialAccountDTO>(() => SelectedClientFinancialAccount);
            }
        }
        public ObservableCollection<FinancialAccountDTO> ClientAcounts
        {
            get { return _financialAccounts; }
            set
            {
                _financialAccounts = value;
                RaisePropertyChanged<ObservableCollection<FinancialAccountDTO>>(() => ClientAcounts);
            }
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