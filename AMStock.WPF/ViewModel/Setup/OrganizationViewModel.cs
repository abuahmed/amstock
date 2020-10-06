using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AMStock.Core.Models;
using AMStock.Service;
using AMStock.Service.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MessageBox = System.Windows.MessageBox;

namespace AMStock.WPF.ViewModel
{
    public class OrganizationViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<OrganizationDTO> _filteredOrganizations;
        private OrganizationDTO _selectedOrganization;
        private static IOrganizationService _organizationService;
        private ICommand _addNewOrganizationViewCommand, _saveOrganizationViewCommand, _deleteOrganizationViewCommand;

        #endregion

        #region Constructor
        public OrganizationViewModel()
        {
            CleanUp();
            _organizationService = new OrganizationService();
            GetLiveOrganizations();
        }
        public static void CleanUp()
        {
            if (_organizationService != null)
                _organizationService.Dispose();
        }
        #endregion

        #region Properties
        public ObservableCollection<OrganizationDTO> Organizations
        {
            get { return _filteredOrganizations; }
            set
            {
                _filteredOrganizations = value;
                RaisePropertyChanged<ObservableCollection<OrganizationDTO>>(() => Organizations);

                if (Organizations.Any())
                    SelectedOrganization = Organizations.FirstOrDefault();
                else
                    ExecuteAddNewOrganizationViewCommand();
            }
        }

        public OrganizationDTO SelectedOrganization
        {
            get { return _selectedOrganization; }
            set
            {
                _selectedOrganization = value;
                RaisePropertyChanged<OrganizationDTO>(() => SelectedOrganization);

            }
        }

        #endregion

        #region Commands
        public ICommand AddNewOrganizationViewCommand
        {
            get { return _addNewOrganizationViewCommand ?? (_addNewOrganizationViewCommand = new RelayCommand(ExecuteAddNewOrganizationViewCommand)); }
        }
        private void ExecuteAddNewOrganizationViewCommand()
        {
            SelectedOrganization = new OrganizationDTO
            {
                ClientId = 1,
                Address = new AddressDTO
                {
                    Country = "Ethiopia",
                    City = "Addis Abeba"
                }
            };
        }

        public ICommand SaveOrganizationViewCommand
        {
            get { return _saveOrganizationViewCommand ?? (_saveOrganizationViewCommand = new RelayCommand<Object>(ExecuteSaveOrganizationViewCommand, CanSave)); }
        }
        private void ExecuteSaveOrganizationViewCommand(object obj)
        {
            try
            {
                var stat = _organizationService.InsertOrUpdate(SelectedOrganization);
                if (stat == string.Empty)
                    CloseWindow(obj);
                else
                    MessageBox.Show("Got Problem while saving, try again..." + Environment.NewLine + stat, "save error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Got Problem while saving, try again..." + Environment.NewLine + exception.Message, "save error",
                    MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        public ICommand DeleteOrganizationViewCommand
        {
            get { return _deleteOrganizationViewCommand ?? (_deleteOrganizationViewCommand = new RelayCommand(ExecuteDeleteOrganizationViewCommand, CanSaveLine)); }
        }
        private void ExecuteDeleteOrganizationViewCommand()
        {
            if (MessageBox.Show("Are you Sure You want to Delete this Organization?", "Delete Organization",
                MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {

                try
                {
                    SelectedOrganization.Enabled = false;
                    _organizationService.Disable(SelectedOrganization);
                    GetLiveOrganizations();
                }
                catch (Exception)
                {
                    MessageBox.Show("Can't delete the organization, may be the organization is already in use...", "Can't Delete",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void CloseWindow(object obj)
        {
            if (obj == null) return;
            var window = obj as Window;
            if (window != null)
            {
                window.Close();
            }
        }
        #endregion

        public void GetLiveOrganizations()
        {
            var orgsList = _organizationService.GetAll();
            Organizations = new ObservableCollection<OrganizationDTO>(orgsList);
        }

        #region Validation
        public static int Errors { get; set; }
        public bool CanSave(object obj)
        {
            return Errors == 0;

        }

        public static int LineErrors { get; set; }
        public bool CanSaveLine()
        {
            return LineErrors == 0;

        }
        #endregion
    }


}
