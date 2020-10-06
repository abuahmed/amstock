using System.Windows;
using System.Windows.Controls;
using AMStock.Core.Enumerations;
using AMStock.WPF.ViewModel;
using GalaSoft.MvvmLight.Messaging;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for BusinessPartnerDetail.xaml
    /// </summary>
    public partial class BusinessPartnerDetail : Window
    {
        public BusinessPartnerDetail()
        {
            BusinessPartnerDetailViewModel.Errors = 0;
            InitializeComponent();
        }
        public BusinessPartnerDetail(BusinessPartnerTypes businessPartnerType)
        {
            BusinessPartnerDetailViewModel.Errors = 0;
            InitializeComponent();
            Messenger.Default.Send<BusinessPartnerTypes>(businessPartnerType);
            Messenger.Reset();
        }
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) BusinessPartnerDetailViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) BusinessPartnerDetailViewModel.Errors -= 1;
        }

        private void WdwBusinessPartnerDetail_Loaded(object sender, RoutedEventArgs e)
        {
            TxtCustName.Focus();
        }

        private void WdwBusinessPartnerDetail_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            BusinessPartnerDetailViewModel.CleanUp();
        }
    }
}
