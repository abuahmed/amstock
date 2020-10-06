using System;
using System.Windows;
using System.Windows.Input;
using AMStock.Core.Models;
using AMStock.Service;
using AMStock.Service.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace AMStock.WPF.ViewModel
{
    public class ClientViewModel : ViewModelBase
    {
        #region Fields
        private static IClientService _clientService;
        private ClientDTO _selectedClient;
        private ICommand _saveClientViewCommand;
        #endregion

        #region Constructor

        public ClientViewModel()
        {
            CleanUp();
            _clientService = new ClientService();

            SelectedClient = _clientService.GetClient() ?? new ClientDTO()
            {
                Address = new AddressDTO()
                {
                    Country = "Ethiopia",
                    City = "Addis Abeba"
                }
            };
        }

        public static void CleanUp()
        {
            if (_clientService != null)
                _clientService.Dispose();
        }

        #endregion

        #region Properties

        public ClientDTO SelectedClient
        {
            get { return _selectedClient; }
            set
            {
                _selectedClient = value;
                RaisePropertyChanged<ClientDTO>(() => SelectedClient);
            }
        }

        #endregion

        #region Commands
        public ICommand SaveClientViewCommand
        {
            get { return _saveClientViewCommand ?? (_saveClientViewCommand = new RelayCommand<Object>(ExecuteSaveClientViewCommand, CanSave)); }
        }
        private void ExecuteSaveClientViewCommand(object obj)
        {
            try
            {
                if (SelectedClient != null && _clientService.InsertOrUpdate(SelectedClient) == string.Empty)
                    CloseWindow(obj);
                else
                    MessageBox.Show("Got Problem while saving, try again...", "error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.InnerException.Message + Environment.NewLine + exception.Message, "error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
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