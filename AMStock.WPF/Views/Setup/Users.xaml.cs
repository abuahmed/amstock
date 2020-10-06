using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AMStock.WPF.ViewModel;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for Users.xaml
    /// </summary>
    public partial class Users : Window
    {
        public Users()
        {
            UserViewModel.Errors = 0;
            InitializeComponent();
        }
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) UserViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) UserViewModel.Errors -= 1;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void BtnAddNew_Click(object sender, RoutedEventArgs e)
        {
            TxtFullName.Focus();
        }

        private void Users_OnClosing(object sender, CancelEventArgs e)
        {
            UserViewModel.CleanUp();
        }
    }
}
