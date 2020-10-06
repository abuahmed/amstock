using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AMStock.Core.Enumerations;
using AMStock.WPF.ViewModel;
using GalaSoft.MvvmLight.Messaging;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for Categories.xaml
    /// </summary>
    public partial class Categories : Window
    {
        public Categories()
        {
            CategoryViewModel.Errors = 0;
            InitializeComponent();
        }
        public Categories(NameTypes categoryType)
        {
            CategoryViewModel.Errors = 0;
            InitializeComponent();            
            Messenger.Default.Send<NameTypes>(categoryType);
            Messenger.Reset();
        }
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) CategoryViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) CategoryViewModel.Errors -= 1;
        }

        private void WdwCategories_Loaded(object sender, RoutedEventArgs e)
        {
            TxtCategoryName.Focus();
        }

        private void BtnAdd_OnClick(object sender, RoutedEventArgs e)
        {
            TxtCategoryName.Focus();
        }

        private void Categories_OnClosing(object sender, CancelEventArgs e)
        {
            CategoryViewModel.CleanUp();
        }
    }
}
