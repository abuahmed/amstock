using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AMStock.WPF.ViewModel;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for Cpos.xaml
    /// </summary>
    public partial class Cpos : Window
    {
        public Cpos()
        {
            CpoViewModel.Errors = 0;
            InitializeComponent();
        }
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) CpoViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) CpoViewModel.Errors -= 1;
        }

        private void Cpos_OnClosing(object sender, CancelEventArgs e)
        {
            CpoViewModel.CleanUp();
        }
    }
}
