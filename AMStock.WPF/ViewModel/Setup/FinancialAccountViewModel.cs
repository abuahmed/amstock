using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
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
    public class FinancialAccountViewModel : ViewModelBase
    {
        #region Fields
        private static IFinancialAccountService _financialAccountService;
        private FinancialAccountDTO _selectedfinancialAccount;
        private ObservableCollection<FinancialAccountDTO> _financialAccounts;
        private string _accountsVisibility;
        private bool _addNewAccountCommandVisibility;
        private ICommand _addNewAccountCommand, _saveAccountCommand, _deleteAccountCommand;
        #endregion

        #region Constructor
        public FinancialAccountViewModel()
        {
            CleanUp();
            _financialAccountService = new FinancialAccountService();

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
            if (_financialAccountService != null)
                _financialAccountService.Dispose();
        }
        #endregion

        #region Properties

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
        public ObservableCollection<FinancialAccountDTO> FinancialAccounts
        {
            get { return _financialAccounts; }
            set
            {
                _financialAccounts = value;
                RaisePropertyChanged<ObservableCollection<FinancialAccountDTO>>(() => FinancialAccounts);
                ExecuteAddNewAccountCommand();
            }
        }
        public FinancialAccountDTO SelectedFinancialAccount
        {
            get { return _selectedfinancialAccount; }
            set
            {
                _selectedfinancialAccount = value;
                RaisePropertyChanged<FinancialAccountDTO>(() => SelectedFinancialAccount);
                if (SelectedFinancialAccount != null)
                {
                    SelectedBank = Banks.FirstOrDefault(b => b.DisplayName == SelectedFinancialAccount.BankName);
                }
            }
        }
        #endregion

        #region Commands
        public ICommand AddNewFinancialAccountCommand
        {
            get { return _addNewAccountCommand ?? (_addNewAccountCommand = new RelayCommand(ExecuteAddNewAccountCommand)); }
        }
        private void ExecuteAddNewAccountCommand()
        {
            SelectedFinancialAccount = new FinancialAccountDTO
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

        public ICommand SaveFinancialAccountCommand
        {
            get { return _saveAccountCommand ?? (_saveAccountCommand = new RelayCommand(ExecuteSaveAccountCommand, CanSaveLine)); }
        }
        private void ExecuteSaveAccountCommand()
        {
            try
            {
                SelectedFinancialAccount.BankName = SelectedBank.DisplayName;

                var isNewObject = SelectedFinancialAccount.Id;
                var stat = _financialAccountService.InsertOrUpdate(SelectedFinancialAccount);

                if (string.IsNullOrEmpty(stat))
                {
                    if (isNewObject == 0)
                    {
                        FinancialAccounts.Insert(0, SelectedFinancialAccount);
                    }
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

        public ICommand DeleteFinancialAccountCommand
        {
            get { return _deleteAccountCommand ?? (_deleteAccountCommand = new RelayCommand(ExecuteDeleteAccountCommand, CanSaveLine)); }
        }
        private void ExecuteDeleteAccountCommand()
        {
            if (MessageBox.Show("Are you Sure You want to delete?", "Delete Account",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            try
            {
                SelectedFinancialAccount.Enabled = false;
                _financialAccountService.Disable(SelectedFinancialAccount);
                GetFinancialAccounts();
            }
            catch
            {
                MessageBox.Show("Can't delete the account, may be the account is already in use...", "Can't Delete",
                        MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GetFinancialAccounts()
        {
            var criteria = new SearchCriteria<FinancialAccountDTO>()
            {
                CurrentUserId = Singleton.User.UserId
            };

            if (SelectedWarehouse != null && SelectedWarehouse.Id != -1)
                criteria.SelectedWarehouseId = SelectedWarehouse.Id;

            var financialAccountsList = _financialAccountService.GetAll(criteria).ToList();
            FinancialAccounts = new ObservableCollection<FinancialAccountDTO>(financialAccountsList.OrderByDescending(f => f.Id));
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
                    GetFinancialAccounts();
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
        public bool CanSave(object obj)
        {
            return Errors == 0;

        }

        public static int LineErrors { get; set; }
        public bool CanSaveLine()
        {
            return LineErrors == 0;

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