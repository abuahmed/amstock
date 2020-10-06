using System.ComponentModel;
using AMStock.Core.Models;
using System.Windows;
using System.Windows.Controls;
using AMStock.WPF.ViewModel;
using GalaSoft.MvvmLight.Messaging;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for ContactEntry.xaml
    /// </summary>
    public partial class ContactEntry : Window
    {
        public ContactEntry()
        {
            ContactViewModel.Errors = 0;
            InitializeComponent();
        }

        public ContactEntry(ContactDTO contactDTO)
        {
            ContactViewModel.Errors = 0;
            InitializeComponent();
            Messenger.Default.Send<ContactDTO>(contactDTO);
            Messenger.Reset();
        }

        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) ContactViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) ContactViewModel.Errors -= 1;
        }
       
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtFullName.Focus();
        }


    }
}
