using System.Windows;
using System.Windows.Controls;
using AMStock.WPF.ViewModel;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for OnHandInventories.xaml
    /// </summary>
    public partial class OnHandInventories : UserControl
    {
        public OnHandInventories()
        {
            OnHandInventoryViewModel.Errors = 0;
            InitializeComponent();
        }
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) OnHandInventoryViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) OnHandInventoryViewModel.Errors -= 1;
        }
        private void LstItemsAutoCompleteBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LstItemsAutoCompleteBox.SearchText = string.Empty;
        }

        private void LstItemsAutoCompleteBox_GotFocus_1(object sender, RoutedEventArgs e)
        {
            LstItemsAutoCompleteBox.SearchText = string.Empty;
        }

        private void LstItemsAutoCompleteBox_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            LstItemsAutoCompleteBox.SearchText = string.Empty;
        }

        private void OnHandInventories_OnUnloaded(object sender, RoutedEventArgs e)
        {
            OnHandInventoryViewModel.CleanUp();
        }
    }
}
