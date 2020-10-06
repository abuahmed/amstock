using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AMStock.Core.Models;
using AMStock.WPF.ViewModel;
using GalaSoft.MvvmLight.Messaging;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for AttachmentEntry.xaml
    /// </summary>
    public partial class AttachmentEntry : Window
    {
        public AttachmentEntry()
        {
            AttachmentEntryViewModel.Errors = 0;
            InitializeComponent();
        }
        public AttachmentEntry(TransactionHeaderDTO transaction)
        {
            AttachmentEntryViewModel.Errors = 0;
            InitializeComponent();
            Messenger.Default.Send<TransactionHeaderDTO>(transaction);
        }
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) AttachmentEntryViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) AttachmentEntryViewModel.Errors -= 1;
        }

        private void WdwAttachmentEntry_Loaded(object sender, RoutedEventArgs e)
        {
            TxtAttachmentNumber.Focus();
        }

        private void AttachmentEntry_OnClosing(object sender, CancelEventArgs e)
        {
            AttachmentEntryViewModel.CleanUp();
        }
    }
}
