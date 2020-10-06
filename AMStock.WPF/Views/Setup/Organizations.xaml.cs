using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AMStock.WPF.ViewModel;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for Organizations.xaml
    /// </summary>
    public partial class Organizations : Window
    {
        public Organizations()
        {
            OrganizationViewModel.Errors = 0;
            InitializeComponent();
        }
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) OrganizationViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) OrganizationViewModel.Errors -= 1;
        }

        private void BtnAddNew_Click(object sender, RoutedEventArgs e)
        {
            TxtOrganizationName.Focus();
        }

        private void Organizations_OnClosing(object sender, CancelEventArgs e)
        {
            OrganizationViewModel.CleanUp();
        }
    }
}
