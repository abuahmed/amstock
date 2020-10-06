using System.ComponentModel;
using System.Windows;
using AMStock.WPF.ViewModel;
using GalaSoft.MvvmLight.Messaging;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();
        }

        private void wdwSpashScreen_Loaded(object sender, RoutedEventArgs e)
        {            
            Messenger.Default.Send<object>(sender);
            Messenger.Reset();
        }

        private void SplashScreen_OnClosing(object sender, CancelEventArgs e)
        {
            SplashScreenViewModel.CleanUp();
        }
    }
}
