using System.Windows;
using System.Windows.Controls;
using AMStock.WPF.ViewModel;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for PhysicalInventories.xaml
    /// </summary>
    public partial class PhysicalInventories : UserControl
    {
        public PhysicalInventories()
        {
            PhysicalInventoryViewModel.Errors = 0;
            PhysicalInventoryViewModel.LineErrors = 0;
            InitializeComponent();            
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {

        }
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) PhysicalInventoryViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) PhysicalInventoryViewModel.Errors -= 1;
        }
        private void Validation_ErrorLine(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) PhysicalInventoryViewModel.LineErrors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) PhysicalInventoryViewModel.LineErrors -= 1;
        }

        private void BtnAddNewPi_Click(object sender, RoutedEventArgs e)
        {
            LstItemsAutoCompleteBox.Focus();
        }

        private void BtnSaveLine_Click(object sender, RoutedEventArgs e)
        {
            LstItemsAutoCompleteBox.Focus();
        }

        private void LstPis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LstItemsAutoCompleteBox.Focus();
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            LstItemsAutoCompleteBox.Focus();
        }

        private void PiUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LstItemsAutoCompleteBox.Focus();
        }

        private void PhysicalInventories_OnUnloaded(object sender, RoutedEventArgs e)
        {
            PhysicalInventoryViewModel.CleanUp();
        }
    }
}
