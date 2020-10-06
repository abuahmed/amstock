using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.DAL;
using AMStock.Repository;
using AMStock.Repository.Interfaces;
using AMStock.WPF.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace AMStock.WPF.ViewModel
{
    public class CheckEntryViewModel : ViewModelBase
    {
        #region Fields
        private static IUnitOfWork _unitOfWork;
        private CheckDTO _selectedCheck;
        private FinancialAccountDTO _customerFinancialAccount;
        private ICommand _addCheckCommand;
        #endregion
        
        #region Constructor
        public CheckEntryViewModel()
        {
            CleanUp();
            _unitOfWork = new UnitOfWork(DbContextUtil.GetDbContextInstance());

            LoadClientAcounts();
            LoadBanks();

            Messenger.Default.Register<CheckDTO>(this, (message) =>
            {
                SelectedCheck = message;
            });
        }
        public static void CleanUp()
        {
            if (_unitOfWork != null)
                _unitOfWork.Dispose();
        }
        #endregion

        #region Properties

        
        public CheckDTO SelectedCheck
        {
            get { return _selectedCheck; }
            set
            {
                _selectedCheck = value;
                RaisePropertyChanged<CheckDTO>(() => SelectedCheck);
                if (SelectedCheck != null)
                {
                    CustomerFinancialAccount = SelectedCheck.CustomerBankAccount;
                    //CheckDateMax = SelectedCheck.CheckDueDate;
                }
            }
        }
        public FinancialAccountDTO CustomerFinancialAccount
        {
            get { return _customerFinancialAccount; }
            set
            {
                _customerFinancialAccount = value;
                RaisePropertyChanged<FinancialAccountDTO>(() => CustomerFinancialAccount);
            }
        }
        //public DateTime CheckDateMax
        //{
        //    get { return _checkDateMax; }
        //    set
        //    {
        //        _checkDateMax = value;
        //        RaisePropertyChanged<DateTime>(() => CheckDateMax);
        //    }
        //}
        #endregion

        #region ClientAcounts
        private FinancialAccountDTO _selectedClientFinancialAccount;
        private ObservableCollection<FinancialAccountDTO> _clientAccounts;
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
            get { return _clientAccounts; }
            set
            {
                _clientAccounts = value;
                RaisePropertyChanged<ObservableCollection<FinancialAccountDTO>>(() => ClientAcounts);
            }
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

        public void LoadBanks()
        {
            IEnumerable<CategoryDTO> categoriesList = _unitOfWork.Repository<CategoryDTO>()
                .Query().Filter(c => c.NameType == NameTypes.Bank).Get().OrderBy(i => i.Id)
                .ToList();
            Banks = new ObservableCollection<CategoryDTO>(categoriesList);
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
            if (dialogueResult == null || !(bool)dialogueResult) return;
            LoadBanks();
            SelectedBank = Banks.FirstOrDefault();
        }
        #endregion

        #region Commands
        public ICommand AddCheckCommand
        {
            get { return _addCheckCommand ?? (_addCheckCommand = new RelayCommand<Object>(ExecuteAddCheckCommand, CanSave)); }
        }
        private void ExecuteAddCheckCommand(object obj)
        {
            try
            {
                SelectedCheck.ClientBankAccountId = SelectedClientFinancialAccount.Id;
                CustomerFinancialAccount.BankName = SelectedBank.DisplayName;

                if (SelectedCheck.Id == 0)
                {
                    SelectedCheck.CustomerBankAccount = CustomerFinancialAccount;
                }
                else
                {
                    SelectedCheck.CustomerBankAccountId = CustomerFinancialAccount.Id;
                }
                CloseWindow(obj);
            }
            catch
            {
                MessageBox.Show("Got problem while getting check!", "Check Problem");
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