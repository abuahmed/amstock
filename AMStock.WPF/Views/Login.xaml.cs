using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AMStock.WPF.ViewModel;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>    
    public partial class Login : Window
    {        
        public Login()
        {
            LoginViewModel.Errors = 0;
            InitializeComponent();
        }
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) LoginViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) LoginViewModel.Errors -= 1;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WdwLogin_Loaded(object sender, RoutedEventArgs e)
        {
            TxtUserName.Focus();
        }

        private void Login_OnClosing(object sender, CancelEventArgs e)
        {
            LoginViewModel.CleanUp();
        }
    }

    
}
