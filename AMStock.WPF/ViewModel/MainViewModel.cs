using System;
using AMStock.Core;
using AMStock.Core.Common;
using AMStock.Core.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using TransactionTypes = AMStock.Core.Enumerations.TransactionTypes;

namespace AMStock.WPF.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private string _headerText, _titleText;
        readonly static DashBoardViewModel DashBoardViewModel = new ViewModelLocator().DashBoard;
        readonly static PhysicalInventoryViewModel PhysicalInventoryViewModel = new ViewModelLocator().PhysicalInventory;
        readonly static ItemsMovementViewModel ItemsMovementViewModel = new ViewModelLocator().ItemsMovement;
        readonly static OnHandInventoryViewModel OnHandInventoryViewModel = new ViewModelLocator().OnHandInventory;
        readonly static TransactionsViewModel TransactionsViewModel = new ViewModelLocator().Transactions;

        private ViewModelBase _currentViewModel;

        public MainViewModel()
        {
            CheckRoles();
            TitleText = "PinnaStock, Stock Management System (" +
                Singleton.User.UserName + ") - " +
                DateTime.Now.ToString("dd/MM/yyyy") + " (" +
                ReportUtility.getEthCalendarFormated(DateTime.Now, "-") + ") - " +
                new ProductActivationDTO().BiosSn;

            if (UserRoles.OnHandInventory == "Visible")
            {
                HeaderText = "OnHand Inventories";
                OnHandInventoryViewModel.LoadData = true;
                CurrentViewModel = OnHandInventoryViewModel;
            }
            else if (UserRoles.Sales == "Visible")
            {
                HeaderText = "Sales Management";
                TransactionsViewModel.TransactionType = TransactionTypes.Sale;
                CurrentViewModel = TransactionsViewModel;
            }
            DashBoardViewCommand = new RelayCommand(ExecuteDashBoardViewCommand);
            PhysicalInventoryViewCommand = new RelayCommand(ExecutePhysicalInventoriesViewCommand);
            ItemsMovementViewCommand = new RelayCommand(ItemsMovementsViewCommand);
            OnHandInventoryViewCommand = new RelayCommand(ExecuteOnHandInventoriesViewCommand);
            SalesViewCommand = new RelayCommand(ExecuteSalesViewCommand);
            PurchasesViewCommand = new RelayCommand(ExecutePurchasesViewCommand);
        }

        public ViewModelBase CurrentViewModel
        {
            get
            {
                return _currentViewModel;
            }
            set
            {
                if (_currentViewModel == value)
                    return;
                _currentViewModel = value;
                RaisePropertyChanged("CurrentViewModel");
            }
        }

        public RelayCommand DashBoardViewCommand { get; private set; }
        private void ExecuteDashBoardViewCommand()
        {
            HeaderText = "DashBoard";
            CurrentViewModel = DashBoardViewModel;
        }

        public RelayCommand PhysicalInventoryViewCommand { get; private set; }
        private void ExecutePhysicalInventoriesViewCommand()
        {
            HeaderText = "Physical Inventories Management";
            PhysicalInventoryViewModel.LoadData = true;
            CurrentViewModel = PhysicalInventoryViewModel;
        }

        public RelayCommand ItemsMovementViewCommand { get; private set; }
        private void ItemsMovementsViewCommand()
        {
            HeaderText = "Stock Transfer Management";
            ItemsMovementViewModel.LoadData = true;
            CurrentViewModel = ItemsMovementViewModel;
        }

        public RelayCommand OnHandInventoryViewCommand { get; private set; }
        private void ExecuteOnHandInventoriesViewCommand()
        {
            HeaderText = "OnHand Inventories";
            OnHandInventoryViewModel.LoadData = true;
            CurrentViewModel = OnHandInventoryViewModel;
        }

        public RelayCommand SalesViewCommand { get; private set; }
        private void ExecuteSalesViewCommand()
        {
            HeaderText = "Sales Management";
            TransactionsViewModel.TransactionType = TransactionTypes.Sale;
            CurrentViewModel = TransactionsViewModel;
        }

        public RelayCommand PurchasesViewCommand { get; private set; }
        private void ExecutePurchasesViewCommand()
        {
            HeaderText = "Purchase Management";
            TransactionsViewModel.TransactionType = TransactionTypes.Purchase;
            CurrentViewModel = TransactionsViewModel;
        }

        public string HeaderText
        {
            get
            {
                return _headerText;
            }
            set
            {
                if (_headerText == value)
                    return;
                _headerText = value;
                RaisePropertyChanged("HeaderText");
            }
        }
        public string TitleText
        {
            get
            {
                return _titleText;
            }
            set
            {
                if (_titleText == value)
                    return;
                _titleText = value;
                RaisePropertyChanged("TitleText");
            }
        }

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
            UserRoles = Singleton.UserRoles;
            UserRoles.Cpo = UserRoles.Cpo == "Visible" && Singleton.Setting.EnableCpoTransaction ? "Visible" : "Collapsed";
            UserRoles.ItemBorrows = UserRoles.ItemBorrows == "Visible" && Singleton.Setting.EnableCpoTransaction ? "Visible" : "Collapsed";
            UserRoles.Admin = UserRoles.Settings == "Visible" ||
                                UserRoles.UsersMgmt == "Visible" ||
                                UserRoles.BackupRestore == "Visible" ||
                                UserRoles.ClientProfile == "Visible" ||
                                UserRoles.OrganizationProfile == "Visible" ||
                                UserRoles.AddStores == "Visible" ||
                                UserRoles.UpdateStores == "Visible"
                            ? "Visible" : "Collapsed";

            UserRoles.Settings = UserRoles.GeneralSettings == "Visible" ||
                                 UserRoles.TaxSettings == "Visible" ||
                                 UserRoles.AdvancedSettings == "Visible"
                ? "Visible"
                : "Collapsed";
            UserRoles.Sales = UserRoles.ViewSales == "Visible" ||
                                UserRoles.AddSales == "Visible"||
                                UserRoles.PostSales == "Visible" ||
                                UserRoles.UnPostSales == "Visible" ||
                                UserRoles.SalesLineHistory == "Visible" ||
                                UserRoles.DeleteSales == "Visible" ||
                                UserRoles.SalesPayments == "Visible" 
                            ? "Visible" : "Collapsed";
            //UserRoles.ClientProfile = "Visible";// "Collapsed";
        }

        #endregion
    }
}