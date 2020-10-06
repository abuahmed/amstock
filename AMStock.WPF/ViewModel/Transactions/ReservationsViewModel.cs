using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AMStock.Core.Models;
using AMStock.Service;
using AMStock.Service.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace AMStock.WPF.ViewModel
{
    public class ReservationsViewModel : ViewModelBase
    {
        #region Fields

        private static IItemQuantityService _itemQuantityService;
        private ItemQuantityDTO _itemQuantityDto, _itemQuantityDtoOld;
        private ICommand _savePaymentViewCommand;
        #endregion

        #region Constructor
        public ReservationsViewModel()
        {
            CleanUp();
            _itemQuantityService = new ItemQuantityService(false);
            Messenger.Default.Register<ItemQuantityDTO>(this, (message) =>
            {
                CurrentItemQuantityOld = _itemQuantityService.Find(message.Id.ToString(CultureInfo.InvariantCulture));
            });

        }
        public static void CleanUp()
        {
            if (_itemQuantityService != null)
                _itemQuantityService.Dispose();
        }
        #endregion

        #region Properties

        public ItemQuantityDTO CurrentItemQuantityOld
        {
            get { return _itemQuantityDtoOld; }
            set
            {
                _itemQuantityDtoOld = value;
                RaisePropertyChanged<ItemQuantityDTO>(() => CurrentItemQuantityOld);
                if (CurrentItemQuantityOld != null)
                {
                    CurrentItemQuantity = _itemQuantityService.GetAll()
                        .FirstOrDefault(i => i.Id == CurrentItemQuantityOld.Id);
                }
            }
        }
        public ItemQuantityDTO CurrentItemQuantity
        {
            get { return _itemQuantityDto; }
            set
            {
                _itemQuantityDto = value;
                RaisePropertyChanged<ItemQuantityDTO>(() => CurrentItemQuantity);
                if (CurrentItemQuantity != null)
                {
                    if (CurrentItemQuantity.ReservedOnDate == null)
                        CurrentItemQuantity.ReservedOnDate = DateTime.Now;
                }
            }
        }

        #endregion

        #region Commands

        public ICommand SaveReservationCommand
        {
            get { return _savePaymentViewCommand ?? (_savePaymentViewCommand = new RelayCommand<Object>(ExecuteSaveReservationViewCommand, CanSave)); }
        }
        private bool SaveReservation()
        {
            try
            {
                if (CurrentItemQuantity.QuantityReserved > CurrentItemQuantity.QuantityOnHand)
                    CurrentItemQuantity.QuantityReserved = CurrentItemQuantity.QuantityOnHand;

                CurrentItemQuantity.QuantityOnHand = CurrentItemQuantity.QuantityOnHand -
                                                     CurrentItemQuantity.QuantityReserved;
                var stat = _itemQuantityService.InsertOrUpdate(CurrentItemQuantity, false);

                return stat == string.Empty;

            }
            catch (Exception exception)
            {
                MessageBox.Show("Got Problem while reserving, try again..." + Environment.NewLine + exception.Message,
                    "Reserve Problem", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private void ExecuteSaveReservationViewCommand(object obj)
        {
            if (SaveReservation())
                CloseWindow(obj);
            else
                MessageBox.Show("Got Problem while reserving, try again...", "Reserve Problem", MessageBoxButton.OK,
                    MessageBoxImage.Error);

        }

        private ICommand _closePaymentViewCommand;
        public ICommand ClosePaymentViewCommand
        {
            get
            {
                return _closePaymentViewCommand ?? (_closePaymentViewCommand = new RelayCommand<Object>(CloseWindow));
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

        #endregion

        #region Validation
        public static int Errors { get; set; }
        public bool CanSave(object obj)
        {
            return Errors == 0;
        }
        #endregion
    }
}
