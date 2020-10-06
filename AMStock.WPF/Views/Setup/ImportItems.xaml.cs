using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AMStock.WPF.ViewModel;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for ImportItems.xaml
    /// </summary>
    public partial class ImportItems : Window
    {
        public ImportItems()
        {
            ImportItemsViewModel.Errors = 0;
            InitializeComponent();
        }
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) ImportItemsViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) ImportItemsViewModel.Errors -= 1;
        }

        private void ImportItems_OnClosing(object sender, CancelEventArgs e)
        {
            ImportItemsViewModel.CleanUp();
        }
    }
}
