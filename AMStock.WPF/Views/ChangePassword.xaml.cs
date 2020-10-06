using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AMStock.WPF.ViewModel;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for ChangePassword.xaml
    /// </summary>
    public partial class ChangePassword : Window
    {
        public ChangePassword()
        {
            ChangePasswordViewModel.Errors = 0;
            InitializeComponent();
        }
        public ChangePassword(string oldPassword)
        {
            ChangePasswordViewModel.Errors = 0;
            InitializeComponent();
            TxtOldPassword.Password = oldPassword;
        }
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) ChangePasswordViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) ChangePasswordViewModel.Errors -= 1;
        }

        private void WdwChangePassword_Loaded(object sender, RoutedEventArgs e)
        {
            if (TxtOldPassword.IsVisible)
                TxtOldPassword.Focus();
            else
                TxtPassword.Focus();
        }

        private void ChangePassword_OnClosing(object sender, CancelEventArgs e)
        {
            //ChangePasswordViewModel.CleanUp();
        }
    }
}
