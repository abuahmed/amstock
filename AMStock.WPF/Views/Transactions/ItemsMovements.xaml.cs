using System.Windows;
using System.Windows.Controls;
using AMStock.WPF.ViewModel;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for ItemsMovements.xaml
    /// </summary>
    public partial class ItemsMovements : UserControl
    {
        public ItemsMovements()
        {
            ItemsMovementViewModel.Errors = 0;
            ItemsMovementViewModel.LineErrors = 0;
            InitializeComponent();            
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {

        }
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) ItemsMovementViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) ItemsMovementViewModel.Errors -= 1;
        }
        private void Validation_ErrorLine(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) ItemsMovementViewModel.LineErrors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) ItemsMovementViewModel.LineErrors -= 1;
        }

        private void BtnAddNewItemsMovement_Click(object sender, RoutedEventArgs e)
        {
            LstItemsAutoCompleteBox.Focus();
        }

        private void BtnSaveLine_Click(object sender, RoutedEventArgs e)
        {
            LstItemsAutoCompleteBox.Focus();
        }

        private void LstItemsMovements_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LstItemsAutoCompleteBox.Focus();
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            LstItemsAutoCompleteBox.Focus();
        }

        private void ItemsMovementUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LstItemsAutoCompleteBox.Focus();
        }

        private void ItemsMovements_OnUnloaded(object sender, RoutedEventArgs e)
        {
            ItemsMovementViewModel.CleanUp();
        }
    }
}
