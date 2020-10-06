using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AMStock.Core.Models;
using AMStock.WPF.ViewModel;
using GalaSoft.MvvmLight.Messaging;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for CheckEntry.xaml
    /// </summary>
    public partial class CheckEntry : Window
    {
        public CheckEntry()
        {
            InitializeComponent();
        }

        public CheckEntry(CheckDTO check)
        {
            CheckEntryViewModel.Errors = 0;
            InitializeComponent();
            Messenger.Default.Send<CheckDTO>(check);
        }
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) CheckEntryViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) CheckEntryViewModel.Errors -= 1;
        }

        private void WdwCheckEntry_Loaded(object sender, RoutedEventArgs e)
        {
            TxtCheckNumber.Focus();
        }

        private void CheckEntry_OnClosing(object sender, CancelEventArgs e)
        {
            CheckEntryViewModel.CleanUp();
        }
    }
}
