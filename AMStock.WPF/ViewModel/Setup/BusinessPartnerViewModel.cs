using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AMStock.Core;
using AMStock.Core.Common;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.DAL;
using AMStock.DAL.Interfaces;
using AMStock.Service;
using AMStock.Service.Interfaces;
using AMStock.WPF.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using TransactionTypes = AMStock.Core.Enumerations.TransactionTypes;

namespace AMStock.WPF.ViewModel
{
    public class BusinessPartnerViewModel : ViewModelBase
    {
        #region Fields
        private static IBusinessPartnerService _businessPartnerService;
        private BusinessPartnerTypes _businessPartnerTypes;
        private IEnumerable<BusinessPartnerDTO> _businessPartners;
        private ObservableCollection<BusinessPartnerDTO> _filteredBusinessPartners;
        private BusinessPartnerDTO _selectedBusinessPartner;
        private BusinessPartnerAddressDTO _selectedBusinessPartnerAddress;
        private BusinessPartnerContactDTO _selectedBusinessPartnerContact;

        private ICommand _addNewBusinessPartnerViewCommand,
                            _saveBusinessPartnerViewCommand,
                            _deleteBusinessPartnerViewCommand,
                            _viewCreditCommand, _refreshListCommand;

        private ObservableCollection<BusinessPartnerAddressDTO> _businessPartnerAddresses;
        private ObservableCollection<BusinessPartnerContactDTO> _businessPartnerContacts;
        private ICommand _businessPartnerAddressViewCommand, _businessPartnerContactViewCommand;

        private string _searchText, _viewCreditsVisibility, _advancedSettingVisibility;
        private string _businessPartnerText, _creditTransactionsVisibility, _creditAmountVisibility;

        private IDbContext _iDbContext;
        private IDbSet<BusinessPartnerAddressDTO> _bpAddr;
        private IDbSet<BusinessPartnerContactDTO> _bpCont;
        #endregion

        #region Constructor
        public BusinessPartnerViewModel()
        {
            Load();
            Messenger.Default.Register<BusinessPartnerTypes>(this, (message) =>
            {
                BusinessPartnerType = message;
            });
        }

        public void Load()
        {
            CleanUp();
            _iDbContext = DbContextUtil.GetDbContextInstance();
            _businessPartnerService = new BusinessPartnerService(_iDbContext);
            _bpAddr = _iDbContext.Set<BusinessPartnerAddressDTO>();
            _bpCont = _iDbContext.Set<BusinessPartnerContactDTO>();
        }

        public void RefreshList()
        {
            Load();
            GetLiveBusinessPartners();
        }

        public static void CleanUp()
        {
            if (_businessPartnerService != null)
                _businessPartnerService.Dispose();
        }
        #endregion

        #region Public Properties
        public BusinessPartnerTypes BusinessPartnerType
        {
            get { return _businessPartnerTypes; }
            set
            {
                _businessPartnerTypes = value;
                RaisePropertyChanged<BusinessPartnerTypes>(() => BusinessPartnerType);
                if (BusinessPartnerType != BusinessPartnerTypes.All)
                {
                    CheckRoles();
                    GetLiveBusinessPartners();

                    AdvancedSettingVisibility = "Visible";

                    switch (BusinessPartnerType)
                    {
                        case BusinessPartnerTypes.Customer:
                            BusinessPartnerText = "Customers";
                            break;
                        case BusinessPartnerTypes.Supplier:
                            BusinessPartnerText = "Suppliers";
                            break;
                    }
                }
            }
        }
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                RaisePropertyChanged<string>(() => SearchText);
                if (BusinessPartnerList != null)
                {
                    if (!string.IsNullOrEmpty(SearchText))
                    {
                        BusinessPartners = new ObservableCollection<BusinessPartnerDTO>
                            (BusinessPartnerList.Where(c => c.DisplayName.ToLower().Contains(value.ToLower()) ||
                                                            c.Code.ToLower().Contains(value.ToLower())).ToList());
                    }
                    else
                        BusinessPartners = new ObservableCollection<BusinessPartnerDTO>(BusinessPartnerList);

                }
            }
        }
        public string BusinessPartnerText
        {
            get { return _businessPartnerText; }
            set
            {
                _businessPartnerText = value;
                RaisePropertyChanged<string>(() => BusinessPartnerText);
            }
        }
        public string AdvancedSettingVisibility
        {
            get { return _advancedSettingVisibility; }
            set
            {
                _advancedSettingVisibility = value;
                RaisePropertyChanged<string>(() => AdvancedSettingVisibility);
            }
        }
        public string ViewCreditsVisibility
        {
            get { return _viewCreditsVisibility; }
            set
            {
                _viewCreditsVisibility = value;
                RaisePropertyChanged<string>(() => ViewCreditsVisibility);
            }
        }
        public string CreditAmountVisibility
        {
            get { return _creditAmountVisibility; }
            set
            {
                _creditAmountVisibility = value;
                RaisePropertyChanged<string>(() => CreditAmountVisibility);
            }
        }
        public string CreditTransactionsVisibility
        {
            get { return _creditTransactionsVisibility; }
            set
            {
                _creditTransactionsVisibility = value;
                RaisePropertyChanged<string>(() => CreditTransactionsVisibility);
            }
        }

        public BusinessPartnerDTO SelectedBusinessPartner
        {
            get { return _selectedBusinessPartner; }
            set
            {
                _selectedBusinessPartner = value;
                RaisePropertyChanged<BusinessPartnerDTO>(() => SelectedBusinessPartner);
                if (SelectedBusinessPartner != null)
                {
                    ViewCreditsVisibility = SelectedBusinessPartner.TotalCredits > 0 ? "Visible" : "Collapsed";
                    GetBusinessPartnerAddresses();
                    GetBusinessPartnerContacts();
                }
            }
        }
        public IEnumerable<BusinessPartnerDTO> BusinessPartnerList
        {
            get { return _businessPartners; }
            set
            {
                _businessPartners = value;
                RaisePropertyChanged<IEnumerable<BusinessPartnerDTO>>(() => BusinessPartnerList);
            }
        }
        public ObservableCollection<BusinessPartnerDTO> BusinessPartners
        {
            get { return _filteredBusinessPartners; }
            set
            {
                _filteredBusinessPartners = value;
                RaisePropertyChanged<ObservableCollection<BusinessPartnerDTO>>(() => BusinessPartners);

                if (BusinessPartners != null && BusinessPartners.Any())
                    SelectedBusinessPartner = BusinessPartners.FirstOrDefault();
                else
                    AddNewBusinessPartner();
            }
        }

        public ObservableCollection<BusinessPartnerAddressDTO> BusinessPartnerAddresses
        {
            get { return _businessPartnerAddresses; }
            set
            {
                _businessPartnerAddresses = value;
                RaisePropertyChanged<ObservableCollection<BusinessPartnerAddressDTO>>(() => BusinessPartnerAddresses);
            }
        }
        public BusinessPartnerAddressDTO SelectedBusinessPartnerAddress
        {
            get { return _selectedBusinessPartnerAddress; }
            set
            {
                _selectedBusinessPartnerAddress = value;
                RaisePropertyChanged<BusinessPartnerAddressDTO>(() => SelectedBusinessPartnerAddress);
            }
        }

        public ObservableCollection<BusinessPartnerContactDTO> BusinessPartnerContacts
        {
            get { return _businessPartnerContacts; }
            set
            {
                _businessPartnerContacts = value;
                RaisePropertyChanged<ObservableCollection<BusinessPartnerContactDTO>>(() => BusinessPartnerContacts);
            }
        }
        public BusinessPartnerContactDTO SelectedBusinessPartnerContact
        {
            get { return _selectedBusinessPartnerContact; }
            set
            {
                _selectedBusinessPartnerContact = value;
                RaisePropertyChanged<BusinessPartnerContactDTO>(() => SelectedBusinessPartnerContact);
            }
        }

        #endregion

        #region Commands

        public ICommand BusinessPartnerAddressViewCommand
        {
            get { return _businessPartnerAddressViewCommand ?? (_businessPartnerAddressViewCommand = new RelayCommand(ExcuteBusinessPartnerAddress)); }
        }
        public void ExcuteBusinessPartnerAddress()
        {
            if (SelectedBusinessPartnerAddress == null)
                GetNewSelectedBusinessPartnerAddress();

            var businessPartnerAddress = new AddressEntry(SelectedBusinessPartnerAddress.Address);
            businessPartnerAddress.ShowDialog();

            var dialogueResult = businessPartnerAddress.DialogResult;
            if (dialogueResult != null && (bool)dialogueResult)
            {
                SelectedBusinessPartnerAddress.BusinessPartner = SelectedBusinessPartner;
                _bpAddr.AddOrUpdate(SelectedBusinessPartnerAddress);
                _iDbContext.SaveChanges();

                GetBusinessPartnerAddresses();
            }
        }
        public void GetBusinessPartnerAddresses()
        {
            var bpAddresses = _bpAddr
                .Include(b => b.Address)
                .Where(b => b.BusinessPartnerId == SelectedBusinessPartner.Id)
                .ToList();
            BusinessPartnerAddresses = new ObservableCollection<BusinessPartnerAddressDTO>(bpAddresses);
        }
        public void GetNewSelectedBusinessPartnerAddress()
        {
            SelectedBusinessPartnerAddress = new BusinessPartnerAddressDTO
            {
                BusinessPartner = SelectedBusinessPartner,
                Address = CommonUtility.GetDefaultAddress()
            };
        }

        public ICommand BusinessPartnerContactViewCommand
        {
            get { return _businessPartnerContactViewCommand ?? (_businessPartnerContactViewCommand = new RelayCommand(ExcuteBusinessPartnerContact)); }
        }
        public void ExcuteBusinessPartnerContact()
        {
            if (SelectedBusinessPartnerContact == null)
                GetNewSelectedBusinessPartnerContact();

            var businessPartnerContact = new ContactEntry(SelectedBusinessPartnerContact.Contact);
            businessPartnerContact.ShowDialog();

            var dialogueResult = businessPartnerContact.DialogResult;
            if (dialogueResult != null && (bool)dialogueResult)
            {
                SelectedBusinessPartnerContact.BusinessPartner = SelectedBusinessPartner;
                _bpCont.AddOrUpdate(SelectedBusinessPartnerContact);
                _iDbContext.SaveChanges();

                GetBusinessPartnerContacts();
            }
        }
        public void GetBusinessPartnerContacts()
        {
            var bpAddresses = _bpCont
                .Include(b => b.Contact).Include(b => b.Contact.Address)
                .Where(b => b.BusinessPartnerId == SelectedBusinessPartner.Id)
                .ToList();
            BusinessPartnerContacts = new ObservableCollection<BusinessPartnerContactDTO>(bpAddresses);
        }
        public void GetNewSelectedBusinessPartnerContact()
        {
            SelectedBusinessPartnerContact = new BusinessPartnerContactDTO
            {
                BusinessPartner = SelectedBusinessPartner,
                Contact = new ContactDTO()
                {
                    Address = CommonUtility.GetDefaultAddress()
                }
            };
        }

        public ICommand AddNewBusinessPartnerViewCommand
        {
            get { return _addNewBusinessPartnerViewCommand ?? (_addNewBusinessPartnerViewCommand = new RelayCommand(AddNewBusinessPartner)); }
        }
        private void AddNewBusinessPartner()
        {
            GetNewSelectedBusinessPartnerAddress();
            GetNewSelectedBusinessPartnerContact();

            SelectedBusinessPartner = new BusinessPartnerDTO
            {
                BusinessPartnerType = BusinessPartnerType,
                CreditLimit = 0,
                MaxNoCreditTransactions = 0,
                PaymentTerm = 0,
                AllowCreditsWithoutCheck = true,
                //Addresses = new[] { SelectedBusinessPartnerAddress }
            };
        }

        public ICommand SaveBusinessPartnerViewCommand
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
                    BusinessPartners.Insert(0, SelectedBusinessPartner);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't save"
                                  + Environment.NewLine + exception.Message, "Can't save", MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }
        }

        public ICommand DeleteBusinessPartnerViewCommand
        {
            get { return _deleteBusinessPartnerViewCommand ?? (_deleteBusinessPartnerViewCommand = new RelayCommand<Object>(DeleteBusinessPartner, CanSave)); }
        }
        private void DeleteBusinessPartner(object obj)
        {
            if (MessageBox.Show("Are you Sure You want to Delete this BusinessPartner?", "Delete BusinessPartner", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    SelectedBusinessPartner.Enabled = false;
                    var stat = _businessPartnerService.Disable(SelectedBusinessPartner);
                    if (stat == string.Empty)
                    {
                        BusinessPartners.Remove(SelectedBusinessPartner);
                    }
                    else
                    {
                        MessageBox.Show("Can't Delete, may be the data is already in use..."
                             + Environment.NewLine + stat, "Can't Delete",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Can't Delete, may be the data is already in use..."
                         + Environment.NewLine + ex.Message + Environment.NewLine + ex.InnerException, "Can't Delete",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public ICommand ViewCreditCommand
        {
            get { return _viewCreditCommand ?? (_viewCreditCommand = new RelayCommand<Object>(ViewCredits, CanSave)); }
        }
        public void ViewCredits(object obj)
        {
            switch (BusinessPartnerType)
            {
                case BusinessPartnerTypes.Customer:
                    new PaymentList(TransactionTypes.Sale, SelectedBusinessPartner, PaymentListTypes.NotCleared).ShowDialog();
                    break;
                case BusinessPartnerTypes.Supplier:
                    new PaymentList(TransactionTypes.Purchase, SelectedBusinessPartner, PaymentListTypes.NotCleared).ShowDialog();
                    break;
            }
        }

        public ICommand RefreshListCommand
        {
            get
            {
                return _refreshListCommand ?? (_refreshListCommand = new RelayCommand(RefreshList));
            }
        }
        #endregion

        public void GetLiveBusinessPartners()
        {
            var criteria = new SearchCriteria<BusinessPartnerDTO>();
            criteria.FiList.Add(b => b.BusinessPartnerType == BusinessPartnerType);

            BusinessPartnerList = _businessPartnerService.GetAll(criteria)
               .OrderBy(i => i.Id)
               .Skip(1)
               .ToList();

            BusinessPartners = new ObservableCollection<BusinessPartnerDTO>(BusinessPartnerList);
        }

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
            UserRoles.CustomersAdvanced = //BusinessPartnerType == BusinessPartnerTypes.Customer &&
                                          UserRoles.CustomersAdvanced == "Visible" &&
                                          Singleton.Setting.CheckCreditLimit
                                          ? "Visible" : "Collapsed";
            CreditAmountVisibility = Singleton.Setting.CreditLimitType == CreditLimitTypes.Both ||
                                     Singleton.Setting.CreditLimitType == CreditLimitTypes.Amount
                ? "Visible" : "Collapsed";
            CreditTransactionsVisibility = Singleton.Setting.CreditLimitType == CreditLimitTypes.Both ||
                                     Singleton.Setting.CreditLimitType == CreditLimitTypes.Transactions
                ? "Visible" : "Collapsed";
        }

        #endregion
    }
}
