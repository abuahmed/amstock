using System.ComponentModel;
using AMStock.Core.Enumerations;
using AMStock.WPF.ViewModel;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Messaging;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for BusinessPartners.xaml
    /// </summary>
    public partial class BusinessPartners : Window
    {
        public BusinessPartners()
        {
            BusinessPartnerViewModel.Errors = 0;
            InitializeComponent();
        }
        public BusinessPartners(BusinessPartnerTypes businessPartnerType)
        {
            BusinessPartnerViewModel.Errors = 0;
            InitializeComponent();
            Messenger.Default.Send<BusinessPartnerTypes>(businessPartnerType);
            Messenger.Reset();
        }
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) BusinessPartnerViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) BusinessPartnerViewModel.Errors -= 1;
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CustName.Focus();
        }

        private void BusinessPartners_OnClosing(object sender, CancelEventArgs e)
        {
            BusinessPartnerViewModel.CleanUp();
        }
    }
}
