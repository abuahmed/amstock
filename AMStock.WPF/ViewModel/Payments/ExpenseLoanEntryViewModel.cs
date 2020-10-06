using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AMStock.Core;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.DAL;
using AMStock.Repository;
using AMStock.Repository.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace AMStock.WPF.ViewModel
{
    public class ExpenseLoanEntryViewModel : ViewModelBase
    {
        #region Fields
        private static IUnitOfWork _unitOfWork;
        private PaymentTypes _paymentType;
        private PaymentDTO _selectedPayment;
        private string _headerText, _businessPartnerPerson;

        private ICommand _addNewPaymentCommand, _savePaymentCommand, _closeExpenseLoanViewCommand;
        #endregion

        #region Constructor
        public ExpenseLoanEntryViewModel()
        {
            CleanUp();
            _unitOfWork = new UnitOfWork(DbContextUtil.GetDbContextInstance());

            GetWarehouses();

            Messenger.Default.Register<PaymentTypes>(this, (message) =>
            {
                PaymentType = message;
            });
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

        #region Public Properties
        public PaymentTypes PaymentType
        {
            get { return _paymentType; }
            set
            {
                _paymentType = value;
                RaisePropertyChanged<PaymentTypes>(() => PaymentType);
                switch (PaymentType)
                {
                    case PaymentTypes.CashOut:
                        HeaderText = "Add Expense";
                        BusinessPartnerPerson = "Given To:";
                        break;
                    case PaymentTypes.CashIn:
                        HeaderText = "Add Cash Loan";
                        BusinessPartnerPerson = "Accepted From:";
                        break;
                }
                if (PaymentType == PaymentTypes.Sale || SelectedPayment != null) return;
                AddNewPayment();
            }
        }
        public PaymentDTO SelectedPayment
        {
            get { return _selectedPayment; }
            set
            {
                _selectedPayment = value;
                RaisePropertyChanged<PaymentDTO>(() => SelectedPayment);
                if (SelectedPayment != null && SelectedPayment.Id != 0)
                {
                    PaymentType = SelectedPayment.PaymentType;
                    switch (SelectedPayment.PaymentType)
                    {
                        case PaymentTypes.CashOut:
                            HeaderText = "Edit Expense";
                            break;
                        case PaymentTypes.CashIn:
                            HeaderText = "Edit Cash Loan";
                            break;
                    }
                    if (Warehouses != null)
                        SelectedWarehouse = Warehouses.FirstOrDefault(w => w.Id == SelectedPayment.WarehouseId);
                }
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
        public string BusinessPartnerPerson
        {
            get { return _businessPartnerPerson; }
            set
            {
                _businessPartnerPerson = value;
                RaisePropertyChanged<string>(() => BusinessPartnerPerson);
            }
        }
        #endregion

        #region Commands
        public ICommand AddNewPaymentCommand
        {
            get
            {
                return _addNewPaymentCommand ?? (_addNewPaymentCommand = new RelayCommand(AddNewPayment));
            }
        }
        private void AddNewPayment()
        {
            if (Warehouses != null)
                SelectedWarehouse = Warehouses.FirstOrDefault(w => w.IsDefault) ?? Warehouses.FirstOrDefault();
            else return;

            if (SelectedWarehouse != null && SelectedWarehouse.Id != -1 && SelectedPayment == null)
                SelectedPayment = new PaymentDTO
                {
                    PaymentType = PaymentType,
                    PaymentDate = DateTime.Now,
                    PaymentMethod = PaymentMethods.Cash,
                    Status = PaymentStatus.NotCleared,
                    WarehouseId = SelectedWarehouse.Id
                };
        }

        public ICommand SavePaymentCommand
        {
            get
            {
                return _savePaymentCommand ?? (_savePaymentCommand = new RelayCommand<Object>(SavePayment, CanSave));
            }
        }
        private void SavePayment(object obj)
        {
            SelectedPayment.WarehouseId = SelectedWarehouse.Id;

            if (SelectedPayment.Id == 0)
            {
                _unitOfWork.Repository<PaymentDTO>().Insert(SelectedPayment);
            }
            else
            {
                _unitOfWork.Repository<PaymentDTO>().Update(SelectedPayment);
            }
            _unitOfWork.Commit();
            CloseWindow(obj);
        }

        public ICommand CloseExpenseLoanViewCommand
        {
            get
            {
                return _closeExpenseLoanViewCommand ?? (_closeExpenseLoanViewCommand = new RelayCommand<Object>(CloseWindow));
            }
        }
        public void CloseWindow(object obj)
        {
            if (obj != null)
            {
                var window = obj as Window;
                if (window != null)
                {
                    window.DialogResult = true;
                    window.Close();
                }
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
            Warehouses = Singleton.WarehousesList.Where(w => w.Id != -1).ToList();
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
