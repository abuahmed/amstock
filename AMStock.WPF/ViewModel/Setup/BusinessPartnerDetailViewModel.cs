using System;
using System.Windows;
using System.Windows.Input;
using AMStock.Core;
using AMStock.Core.Common;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.Service;
using AMStock.Service.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace AMStock.WPF.ViewModel
{
    public class BusinessPartnerDetailViewModel : ViewModelBase
    {
        #region Fileds
        private static IBusinessPartnerService _businessPartnerService;
        private BusinessPartnerDTO _selectedBusinessPartner;
        private BusinessPartnerTypes _businessPartnerType;
        private string _headerText;
        private string _businessPartner;
        private ICommand _saveBusinessPartnerViewCommand, _closeBusinessPartnerViewCommand;//, _cleanUpCommand;
        #endregion

        #region Constructor
        public BusinessPartnerDetailViewModel()
        {
            if (_businessPartnerService != null)
                _businessPartnerService.Dispose();
            _businessPartnerService = new BusinessPartnerService();

            CheckRoles();

            Messenger.Default.Register<BusinessPartnerTypes>(this, (message) =>
            {
                BusinessPartnerType = message;
            });
        }

        public static void CleanUp()
        {
            if (_businessPartnerService != null)
                _businessPartnerService.Dispose();
        }

        #endregion

        #region Properties
        public BusinessPartnerTypes BusinessPartnerType
        {
            get { return _businessPartnerType; }
            set
            {
                _businessPartnerType = value;
                RaisePropertyChanged<BusinessPartnerTypes>(() => BusinessPartnerType);
                if (BusinessPartnerType != BusinessPartnerTypes.All)
                {
                    SelectedBusinessPartner = new BusinessPartnerDTO
                    {
                        BusinessPartnerType = BusinessPartnerType,
                        CreditLimit = 0,
                        MaxNoCreditTransactions = 0,
                        PaymentTerm = 0,
                        AllowCreditsWithoutCheck = true,
                        //Address = new AddressDTO
                        //{
                        //    Country = "Ethiopia",
                        //    City = "Addis Abeba"
                        //}
                    };
                    switch (BusinessPartnerType)
                    {
                        case BusinessPartnerTypes.Customer:
                            HeaderText = "Customer Detail";
                            BusinessPartner = "Customer Name";
                            break;
                        case BusinessPartnerTypes.Supplier:
                            HeaderText = "Supplier Detail";
                            BusinessPartner = "Supplier Name";
                            break;
                    }
                }
            }
        }
        public BusinessPartnerDTO SelectedBusinessPartner
        {
            get { return _selectedBusinessPartner; }
            set
            {
                _selectedBusinessPartner = value;
                RaisePropertyChanged<BusinessPartnerDTO>(() => SelectedBusinessPartner);
            }
        }

        public string HeaderText
        {
            get { return _headerText; }
            set
            {
                _headerText = value;
                RaisePropertyChanged<string>(() => HeaderText);
            }
        }
        public string BusinessPartner
        {
            get { return _businessPartner; }
            set
            {
                _businessPartner = value;
                RaisePropertyChanged<string>(() => BusinessPartner);
            }
        }
        #endregion

        #region Commands
        public ICommand SaveCloseBusinessPartnerViewCommand
        {
            get { return _saveBusinessPartnerViewCommand ?? (_saveBusinessPartnerViewCommand = new RelayCommand<Object>(SaveBusinessPartner, CanSave)); }
        }
        private void SaveBusinessPartner(object obj)
        {
            try
            {
                var newObject = SelectedBusinessPartner.Id;

                var stat = _businessPartnerService.InsertOrUpdate(SelectedBusinessPartner);
                if (stat != string.Empty)
                    MessageBox.Show("Can't save"
                                    + Environment.NewLine + stat, "Can't save", MessageBoxButton.OK,
                        MessageBoxImage.Error);

                else if (newObject == 0)
                    CloseWindow(obj);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't save"
                                  + Environment.NewLine + exception.Message, "Can't save", MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }

        }

        public ICommand CloseBusinessPartnerViewCommand
        {
            get
            {
                return _closeBusinessPartnerViewCommand ?? (_closeBusinessPartnerViewCommand = new RelayCommand<Object>(CloseWindow));
            }
        }
        private void CloseWindow(object obj)
        {
            if (obj != null)
            {
                var window = obj as Window;

                if (window != null)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
        }

        //public ICommand CleanUpCommand
        //{
        //    get
        //    {
        //        return _cleanUpCommand ?? (_cleanUpCommand = new RelayCommand(Cleanup));
        //    }
        //}

        #endregion

        #region Validation

        public static int Errors { get; set; }
        public bool CanSave(object parameter)
        {
            return Errors == 0;
        }
        
        #endregion

        #region Previlege Visibility
        private UserRolesModel _userRoles;

        public UserRolesModel UserRoles
        {
            get { return _userRoles; }
            set
            {
                _userRoles = value;
                RaisePropertyChanged<UserRolesModel>(() => UserRoles);
            }
        }

        private void CheckRoles()
        {
            UserRoles = Singleton.UserRoles;
        }

        #endregion
    }

}
