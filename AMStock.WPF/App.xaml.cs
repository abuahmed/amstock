using System;
using System.Windows;
using System.Windows.Threading;
using AMStock.Core;
using AMStock.Service;
using WebMatrix.WebData;
using SplashScreen = AMStock.WPF.Views.SplashScreen;

namespace AMStock.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //private const int MINIMUM_SPLASH_TIME = 5500; // Miliseconds
        //private const int SPLASH_FADE_TIME = 500;     // Miliseconds
        public bool DoHandle { get; set; }

        public App()
        {
            System.Windows.FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var splashScreen = new SplashScreen();
            splashScreen.Show();

            //AMStock.WPF.ViewModel.SplashScreenViewModel splashViewModel = );
            //splashScreen.IsLoaded
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (DoHandle)
            {
                MessageBox.Show(e.Exception.Message, "Exception Caught", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            }
            else
            {
                MessageBox.Show("Application is going to close! ", "Uncaght Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = false;
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        public void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex != null)
                MessageBox.Show(ex.Message, "Uncaught Thread Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public class CustomMembershipProvider : SimpleMembershipProvider
    {
        //public override bool ValidateUser(string username, string password)
        //{
        //    //var userService = new UserService(true);
        //    //var user = userService.Login(username, password);
        //    //return user != null;
        //    return base.ValidateUser(username, password);
        //}
    }
}
