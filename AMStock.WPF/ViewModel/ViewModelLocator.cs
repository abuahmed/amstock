using AMStock.Core;
using AMStock.Core.Enumerations;

namespace AMStock.WPF.ViewModel
{
    public class ViewModelLocator
    {
        private static Bootstrapper _bootStrapper;
        
        public ViewModelLocator()
        {
            //Add Code to choose the server/database the user wants to connect to, the line below depends on it
            Singleton.Edition = AMStockEdition.CompactEdition;
            if (_bootStrapper == null)
                _bootStrapper = new Bootstrapper();
        }

        public SplashScreenViewModel Splash
        {
            get
            {
                return _bootStrapper.Container.Resolve<SplashScreenViewModel>();
            }
        }
        public ActivationViewModel Activation
        {
            get
            {
                return _bootStrapper.Container.Resolve<ActivationViewModel>();
            }
        }
        public LoginViewModel Login
        {
            get
            {
                return _bootStrapper.Container.Resolve<LoginViewModel>();
            }
        }
        public ChangePasswordViewModel ChangePassword
        {
            get
            {
                return _bootStrapper.Container.Resolve<ChangePasswordViewModel>();
            }
        }

        public SettingViewModel Setting
        {
            get
            {
                return _bootStrapper.Container.Resolve<SettingViewModel>();
            }
        }
        public MainViewModel Main
        {
            get
            {
                return _bootStrapper.Container.Resolve<MainViewModel>();                     
            }
        }
       
        public BusinessPartnerViewModel BusinessPartner
        {
            get
            {
                return _bootStrapper.Container.Resolve<BusinessPartnerViewModel>();
            }
        }
        public BusinessPartnerDetailViewModel BusinessPartnerDetail
        {
            get
            {
                return _bootStrapper.Container.Resolve<BusinessPartnerDetailViewModel>();
            }
        }
        public ClientViewModel Client
        {
            get
            {
                return _bootStrapper.Container.Resolve<ClientViewModel>();
            }
        }
        public OrganizationViewModel Organization
        {
            get
            {
                return _bootStrapper.Container.Resolve<OrganizationViewModel>();
            }
        }
        public AddressViewModel Address
        {
            get
            {
                return _bootStrapper.Container.Resolve<AddressViewModel>();
            }
        }
        public ContactViewModel Contact
        {
            get
            {
                return _bootStrapper.Container.Resolve<ContactViewModel>();
            }
        }

        public ItemViewModel Item
        {
            get
            {
                return _bootStrapper.Container.Resolve<ItemViewModel>();
            }
        }
        public ItemDetailViewModel ItemDetail
        {
            get
            {
                return _bootStrapper.Container.Resolve<ItemDetailViewModel>();
            }
        }
        public ImportItemsViewModel ImportItems
        {
            get
            {
                return _bootStrapper.Container.Resolve<ImportItemsViewModel>();
            }
        }
        public TransactionItemsListViewModel TransactionItemsList
        {
            get
            {
                return _bootStrapper.Container.Resolve<TransactionItemsListViewModel>();
            }
        }
        public ItemsTransferListViewModel ItemsTransferList
        {
            get
            {
                return _bootStrapper.Container.Resolve<ItemsTransferListViewModel>();
            }
        }

        public PhysicalInventoryViewModel PhysicalInventory
        {
            get
            {
                return _bootStrapper.Container.Resolve<PhysicalInventoryViewModel>();
            }
        }
        public OnHandInventoryViewModel OnHandInventory
        {
            get
            {
                return _bootStrapper.Container.Resolve<OnHandInventoryViewModel>();
            }
        }
        public TransactionsViewModel Transactions
        {
            get
            {
                return _bootStrapper.Container.Resolve<TransactionsViewModel>();//Guid.NewGuid().ToString()
            }
        }
        public ItemsMovementViewModel ItemsMovement
        {
            get
            {
                return _bootStrapper.Container.Resolve<ItemsMovementViewModel>();
            }
        }
        public AttachmentEntryViewModel AttachmentEntry
        {
            get
            {
                return _bootStrapper.Container.Resolve<AttachmentEntryViewModel>();
            }
        }

        public DashBoardViewModel DashBoard
        {
            get
            {
                return _bootStrapper.Container.Resolve<DashBoardViewModel>();
            }
        }
        public WarehouseViewModel Warehouse
        {
            get
            {
                return _bootStrapper.Container.Resolve<WarehouseViewModel>();
            }
        }
        public FinancialAccountViewModel FinancialAccount
        {
            get
            {
                return _bootStrapper.Container.Resolve<FinancialAccountViewModel>();
            }
        }
        public BankGuaranteeViewModel BankGuarantee
        {
            get
            {
                return _bootStrapper.Container.Resolve<BankGuaranteeViewModel>();
            }
        }

        public AddPaymentViewModel AddPayment
        {
            get
            {
                return _bootStrapper.Container.Resolve<AddPaymentViewModel>();
            }
        }
        public CheckEntryViewModel CheckEntry
        {
            get
            {
                return _bootStrapper.Container.Resolve<CheckEntryViewModel>();
            }
        }
        public PaymentClearanceViewModel PaymentClearance
        {
            get
            {
                return _bootStrapper.Container.Resolve<PaymentClearanceViewModel>();
            }
        }
        public PaymentListViewModel PaymentList
        {
            get
            {
                return _bootStrapper.Container.Resolve<PaymentListViewModel>();
            }
        }
        
        public ExpenseLoanViewModel ExpenseLoan
        {
            get
            {
                return _bootStrapper.Container.Resolve<ExpenseLoanViewModel>();
            }
        }
        public ExpenseLoanEntryViewModel ExpenseLoanEntry
        {
            get
            {
                return _bootStrapper.Container.Resolve<ExpenseLoanEntryViewModel>();
            }
        }
        
        public ReportViewerViewModel ReportViewerCommon
        {
            get
            {
                return _bootStrapper.Container.Resolve<ReportViewerViewModel>();
            }
        }
        public CategoryViewModel Categories
        {
            get
            {
                return _bootStrapper.Container.Resolve<CategoryViewModel>();
            }
        }
        public CalendarConvertorViewModel Convertor
        {
            get
            {
                return _bootStrapper.Container.Resolve<CalendarConvertorViewModel>();
            }
        }
        public UserViewModel User
        {
            get
            {
                return _bootStrapper.Container.Resolve<UserViewModel>();
            }
        }
        public BackupRestoreViewModel BackupRestore
        {
            get
            {
                return _bootStrapper.Container.Resolve<BackupRestoreViewModel>();
            }
        }
        public ReservationsViewModel Reservation
        {
            get
            {
                return _bootStrapper.Container.Resolve<ReservationsViewModel>();
            }
        }
        public CpoViewModel Cpo
        {
            get
            {
                return _bootStrapper.Container.Resolve<CpoViewModel>();
            }
        }
        public ItemBorrowViewModel ItemBorrow
        {
            get
            {
                return _bootStrapper.Container.Resolve<ItemBorrowViewModel>();
            }
        }

        public static void Cleanup()
        {
            
        }
    }
}