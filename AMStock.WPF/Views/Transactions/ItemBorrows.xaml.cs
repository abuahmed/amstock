using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AMStock.WPF.ViewModel;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for ItemBorrows.xaml
    /// </summary>
    public partial class ItemBorrows : Window
    {
        public ItemBorrows()
        {
            ItemBorrowViewModel.Errors = 0;
            InitializeComponent();
        }
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) ItemBorrowViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) ItemBorrowViewModel.Errors -= 1;
        }

        private void CmbTypeOfItemBorrows_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ItemBorrows_OnClosing(object sender, CancelEventArgs e)
        {
            ItemBorrowViewModel.CleanUp();
        }
    }
}
