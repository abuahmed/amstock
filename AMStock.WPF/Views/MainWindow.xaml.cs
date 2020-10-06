using System.Windows;
using AMStock.Core.Enumerations;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #region Files
        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            new ChangePassword().ShowDialog();
        }
        private void CalendarMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new CalendarConvertor().Show();
        }

        private void CustomersMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new BusinessPartners(BusinessPartnerTypes.Customer).Show();
        }
        private void SuppliersMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new BusinessPartners(BusinessPartnerTypes.Supplier).Show();
        }
        private void ItemsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new Items().Show();
        }
        private void CposMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var cpoEntryWindow = new Cpos();
            cpoEntryWindow.ShowDialog();
        }
        private void ItemBorrowsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new ItemBorrows().ShowDialog();
        }
        private void ReservedItemsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("On Development....");
        }
        private void ExpenseCashLoanListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var expenseLoansListWindow = new ExpenseLoans();
            expenseLoansListWindow.Show();
        } 
        #endregion

        #region Sales
        private void SalesReportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new TransactionItemsList(TransactionTypes.Sale, null).Show();
        }
        private void SalesPaymentListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new PaymentList(TransactionTypes.Sale, null, PaymentListTypes.All).Show();
        }
        private void DueSalesPaymentsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new PaymentList(TransactionTypes.Sale, null, PaymentListTypes.NotClearedandOverdue).Show();
        }

        #endregion

        #region Purchases
        private void PurchaseReportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new TransactionItemsList(TransactionTypes.Purchase, null).Show();
        }
        private void PurchasePaymentListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new PaymentList(TransactionTypes.Purchase, null, PaymentListTypes.All).Show();
        }
        private void DuePurchasePaymentsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new PaymentList(TransactionTypes.Purchase, null, PaymentListTypes.NotClearedandOverdue).Show();
        }

        #endregion

        #region Admin
        private void UsersMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new Users().Show();
        }
        private void BackupRestoreMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new BackupRestore().ShowDialog();
        }
        private void CompanyProfileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var clientEntryWindow = new Client();
            clientEntryWindow.ShowDialog();
        }
        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new Settings().ShowDialog();
        }
        private void WarehousesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var warehouses = new Warehouses();
            warehouses.Show();
        }
        private void OrganizationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var organizations = new Organizations();
            organizations.Show();
        }
        private void AccountsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var accounts = new FinancialAccounts();
            accounts.Show();
        }
        #endregion

        private void MnuStockTransfer_OnClick(object sender, RoutedEventArgs e)
        {
                
        }

        private void BankGuaranteesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new BankGuarantees().ShowDialog();
        }

        private void ImportItemsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new ImportItems().Show();
        }
    }
}
