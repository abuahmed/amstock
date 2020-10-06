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
using AMStock.WPF.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace AMStock.WPF.ViewModel
{
    public class BankGuaranteeViewModel : ViewModelBase
    {
        #region Fields
        private static IBankGuaranteeService _bankGuaranteeService;
        private BankGuaranteeDTO _selectedbankGuarantee;
        private ObservableCollection<BankGuaranteeDTO> _bankGuarantees;
        private int _totalNumberOfGuarantees;
        private string _totalValueOfGuarantees;
        private string _accountsVisibility;
        private bool _addNewAccountCommandVisibility;
        private ICommand _addNewAccountCommand, _saveAccountCommand, _deleteAccountCommand;
        #endregion

        #region Constructor
        public BankGuaranteeViewModel()
        {
            CleanUp();
            _bankGuaranteeService = new BankGuaranteeService();

            CheckRoles();
            LoadBanks();
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
            if (_bankGuaranteeService != null)
                _bankGuaranteeService.Dispose();
        }
        #endregion

        #region Properties
        public int TotalNumberOfGuarantees
        {
            get { return _totalNumberOfGuarantees; }
            set
            {
                _totalNumberOfGuarantees = value;
                RaisePropertyChanged<int>(() => TotalNumberOfGuarantees);
            }
        }
        public string TotalValueOfGuarantees
        {
            get { return _totalValueOfGuarantees; }
            set
            {
                _totalValueOfGuarantees = value;
                RaisePropertyChanged<string>(() => TotalValueOfGuarantees);
            }
        }
        public string AccountsVisibility
        {
            get { return _accountsVisibility; }
            set
            {
                _accountsVisibility = value;
                RaisePropertyChanged<string>(() => AccountsVisibility);
            }
        }
        public bool AddNewAccountCommandVisibility
        {
            get { return _addNewAccountCommandVisibility; }
            set
            {
                _addNewAccountCommandVisibility = value;
                RaisePropertyChanged<bool>(() => AddNewAccountCommandVisibility);
            }
        }
        public ObservableCollection<BankGuaranteeDTO> BankGuarantees
        {
            get { return _bankGuarantees; }
            set
            {
                _bankGuarantees = value;
                RaisePropertyChanged<ObservableCollection<BankGuaranteeDTO>>(() => BankGuarantees);
                if (BankGuarantees != null)
                {
                    TotalNumberOfGuarantees = BankGuarantees.Count;
                    TotalValueOfGuarantees = BankGuarantees.Sum(bg => bg.GuaranteedAmount).ToString();//.ToString("N2");
                }
                ExecuteAddNewAccountCommand();
            }
        }
        public BankGuaranteeDTO SelectedBankGuarantee
        {
            get { return _selectedbankGuarantee; }
            set
            {
                _selectedbankGuarantee = value;
                RaisePropertyChanged<BankGuaranteeDTO>(() => SelectedBankGuarantee);
                if (SelectedBankGuarantee != null)
                {
                    SelectedBank = Banks.FirstOrDefault(b => b.DisplayName == SelectedBankGuarantee.BankName);
                }
            }
        }
        #endregion

        #region Commands
        public ICommand AddNewBankGuaranteeCommand
        {
            get { return _addNewAccountCommand ?? (_addNewAccountCommand = new RelayCommand(ExecuteAddNewAccountCommand)); }
        }
        private void ExecuteAddNewAccountCommand()
        {
            SelectedBankGuarantee = new BankGuaranteeDTO
            {
                WarehouseId = SelectedWarehouse.Id
            };
            if (Banks != null)
            {
                var comercialBank = Banks.FirstOrDefault(b => b.DisplayName.ToLower().Contains("commercial"));
                SelectedBank = comercialBank ?? Banks.FirstOrDefault();
            }
            AddNewAccountCommandVisibility = true;
        }

        public ICommand SaveBankGuaranteeCommand
        {
            get { return _saveAccountCommand ?? (_saveAccountCommand = new RelayCommand(ExecuteSaveAccountCommand, CanSave)); }
        }
        private void ExecuteSaveAccountCommand()
        {
            try
            {
                SelectedBankGuarantee.BankName = SelectedBank.DisplayName;

                //var isNewObject = SelectedBankGuarantee.Id;
                var stat = _bankGuaranteeService.InsertOrUpdate(SelectedBankGuarantee);

                if (string.IsNullOrEmpty(stat))
                {
                    GetBankGuarantees();
                    //if (isNewObject == 0)
                    //{
                    //    BankGuarantees.Insert(0, SelectedBankGuarantee);
                    //}
                }
                else
                {
                    MessageBox.Show("Got Problem while saving, try again..." + Environment.NewLine + stat, "save error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }

            }
            catch (Exception exception)
            {
                MessageBox.Show("Got Problem while saving, try again..." + Environment.NewLine + exception.Message, "save error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public ICommand DeleteBankGuaranteeCommand
        {
            get { return _deleteAccountCommand ?? (_deleteAccountCommand = new RelayCommand(ExecuteDeleteAccountCommand, CanSave)); }
        }
        private void ExecuteDeleteAccountCommand()
        {
            if (MessageBox.Show("Are you Sure You want to delete?", "Delete Account",
                MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            try
            {
                SelectedBankGuarantee.Enabled = false;
                _bankGuaranteeService.Disable(SelectedBankGuarantee);
                GetBankGuarantees();
            }
            catch
            {
                MessageBox.Show("Can't delete the account, may be the account is already in use...", "Can't Delete",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GetBankGuarantees()
        {
            var criteria = new SearchCriteria<BankGuaranteeDTO>()
            {
                CurrentUserId = Singleton.User.UserId
            };

            if (SelectedWarehouse != null && SelectedWarehouse.Id != -1)
                criteria.SelectedWarehouseId = SelectedWarehouse.Id;

            var bankGuaranteesList = _bankGuaranteeService.GetAll(criteria).ToList();
            BankGuarantees = new ObservableCollection<BankGuaranteeDTO>(bankGuaranteesList.OrderByDescending(f => f.Id));
        }
        #endregion

        #region Warehouses
        private ObservableCollection<WarehouseDTO> _warehouses;
        private WarehouseDTO _selectedWarehouse;

        public ObservableCollection<WarehouseDTO> Warehouses
        {
            get { return _warehouses; }
            set
            {
                _warehouses = value;
                RaisePropertyChanged<ObservableCollection<WarehouseDTO>>(() => Warehouses);
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
                    AddNewAccountCommandVisibility = SelectedWarehouse.Id != -1;
                    GetBankGuarantees();
                }
            }
        }

        public void GetWarehouses()
        {
            Warehouses = new ObservableCollection<WarehouseDTO>(Singleton.WarehousesList);
        }

        #endregion

        #region Banks
        private CategoryDTO _selectedBank;
        private ObservableCollection<CategoryDTO> _banks;
        private ICommand _addNewBankCommand;

        public CategoryDTO SelectedBank
        {
            get { return _selectedBank; }
            set
            {
                _selectedBank = value;
                RaisePropertyChanged<CategoryDTO>(() => SelectedBank);
            }
        }
        public ObservableCollection<CategoryDTO> Banks
        {
            get { return _banks; }
            set
            {
                _banks = value;
                RaisePropertyChanged<ObservableCollection<CategoryDTO>>(() => Banks);
            }
        }
        public ICommand AddNewBankCommand
        {
            get
            {
                return _addNewBankCommand ?? (_addNewBankCommand = new RelayCommand(ExcuteAddNewCategoryCommand));
            }
        }
        private void ExcuteAddNewCategoryCommand()
        {
            var category = new Categories(NameTypes.Bank);
            category.ShowDialog();
            var dialogueResult = category.DialogResult;
            if (dialogueResult != null && (bool)dialogueResult)
            {
                LoadBanks();
                SelectedBank = Banks.FirstOrDefault(b => b.DisplayName.ToLower().Contains(category.TxtCategoryName.Text.ToLower()));
            }

        }

        public void LoadBanks()
        {
            var criteria = new SearchCriteria<CategoryDTO>();
            criteria.FiList.Add(c => c.NameType == NameTypes.Bank);

            IEnumerable<CategoryDTO> banksList = new CategoryService(true).GetAll(criteria)
                .OrderBy(i => i.DisplayName)
                .ToList();
            Banks = new ObservableCollection<CategoryDTO>(banksList);
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

        private bool _storeNameEnability;
        public bool StoreNameEnability
        {
            get { return _storeNameEnability; }
            set
            {
                _storeNameEnability = value;
                RaisePropertyChanged<bool>(() => StoreNameEnability);
            }
        }
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

            StoreNameEnability = UserRoles.AddStores == "Visible";
        }

        #endregion
    }
}