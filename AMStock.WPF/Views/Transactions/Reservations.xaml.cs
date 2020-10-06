using System.Windows;
using System.Windows.Controls;
using AMStock.Core.Models;
using AMStock.WPF.ViewModel;
using GalaSoft.MvvmLight.Messaging;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for Reservations.xaml
    /// </summary>
    public partial class Reservations : Window
    {
        public Reservations()
        {
            ReservationsViewModel.Errors = 0;
            InitializeComponent();
        }
        public Reservations(ItemQuantityDTO itemQty)
        {
            ReservationsViewModel.Errors = 0;
            InitializeComponent();
            Messenger.Default.Send<ItemQuantityDTO>(itemQty);
        }
        private void Validation_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.Action == ValidationErrorEventAction.Added) ReservationsViewModel.Errors += 1;
            if (e.Action == ValidationErrorEventAction.Removed) ReservationsViewModel.Errors -= 1;
        }

        private void WdwReservations_Loaded(object sender, RoutedEventArgs e)
        {
            TxtQtyReserve.Focus();
        }

        private void Reservations_OnUnloaded(object sender, RoutedEventArgs e)
        {
            ReservationsViewModel.CleanUp();
        }
    }
}
