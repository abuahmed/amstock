using System.Windows;
using System.Windows.Controls;
using AMStock.WPF.ViewModel;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for Transactions.xaml
    /// </summary>
    public partial class Transactions : UserControl
    {
        public Transactions()
        {
            TransactionsViewModel.Errors = 0;
            TransactionsViewModel.LineErrors = 0;
            InitializeComponent();
        }
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) TransactionsViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) TransactionsViewModel.Errors -= 1;
        }
        private void Validation_LineError(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) TransactionsViewModel.LineErrors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) TransactionsViewModel.LineErrors -= 1;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            FocusControls();
        }

        private void BtnAddNewTransaction_Click(object sender, RoutedEventArgs e)
        {
            FocusControls();
        }

        private void BtnSaveLine_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //FocusControls();
        }

        private void LstTransactions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FocusControls();
        }

        private void FocusControls()
        {
            LstItemsAutoCompleteBox.Focus();
            //TxtEachPrice.Text = "";
            //TxtNewQuantity.Text = "";
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            FocusControls();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            TransactionsViewModel.CleanUp();
        }

        private void LstItemsAutoCompleteBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TxtNewQuantity.Focus();
        }
    }
}
