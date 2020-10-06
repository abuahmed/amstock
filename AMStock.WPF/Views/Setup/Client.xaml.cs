using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AMStock.WPF.ViewModel;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for ClientDetail.xaml
    /// </summary>
    public partial class Client : Window
    {
        public Client()
        {
            ClientViewModel.Errors = 0;
            ClientViewModel.LineErrors = 0;
            InitializeComponent();
        }
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) ClientViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) ClientViewModel.Errors -= 1;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtCustName.Focus();
        }

        private void Client_OnClosing(object sender, CancelEventArgs e)
        {
            ClientViewModel.CleanUp();
        }
    }
}
