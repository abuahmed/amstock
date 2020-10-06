using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using AMStock.DAL;
using AMStock.Repository;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using AMStock.Core;
using AMStock.Core.Models;
using AMStock.Repository.Interfaces;
using AMStock.WPF.Views;
using AMStock.Core.Enumerations;

namespace AMStock.WPF.ViewModel
{
    public class SplashScreenViewModel : ViewModelBase
    {
        #region Fields
        private static IUnitOfWork _unitOfWork;
        private object _splashWindow;
        bool _login, _activations;
        private string _licensedTo;
        #endregion

        #region Constructor
        public SplashScreenViewModel()
        {
            CleanUp();
            _unitOfWork = new UnitOfWork(DbContextUtil.GetDbContextInstance()); 

            Messenger.Default.Register<object>(this, (message) =>
            {
                SplashWindow = message;
            });
        }
        public static void CleanUp()
        {
            if (_unitOfWork != null)
                _unitOfWork.Dispose();
        }
        #endregion

        #region Properties
        
        public object SplashWindow
        {
            get { return _splashWindow; }
            set
            {
                _splashWindow = value;
                RaisePropertyChanged<object>(() => SplashWindow);
                if (SplashWindow != null)
                {
                    //var maxdt = new DateTime(2019, 5, 10);
                    //var daysleft = maxdt.Subtract(DateTime.Now).Days;
                    //if (daysleft < 0 || daysleft > 18)
                    //    MessageBox.Show("Software Error", "Software Error");
                    //else
                        CheckActivation();
                }
            }
        }
        public string LicensedTo
        {
            get { return _licensedTo; }
            set
            {
                _licensedTo = value;
                RaisePropertyChanged<string>(() => LicensedTo);
            }
        }
        #endregion

        #region Actions
        private void CheckActivation()
        {
            var worker = new BackgroundWorker();
            worker.DoWork += DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }
        private void DoWork(object sender, DoWorkEventArgs e)
        {
            //_login = true;
            try
            {
                
                var activation = _unitOfWork.Repository<ProductActivationDTO>().Query().Get().FirstOrDefault();
                if (activation == null)
                {
                    //new Activations().Show();
                    _activations = true;
                }
                else
                {
                    LicensedTo = activation.LicensedTo;
                    Thread.Sleep(1000);//To show the License to whom it belongs
                    if (activation.RegisteredBiosSn.Contains(new ProductActivationDTO().BiosSn))
                    {
                        //new Login().Show();
                        Singleton.ProductActivation = activation;
                        _login = true;
                    }
                    else
                    {

                        //if (MessageBox.Show(
                        //            "The Product has already been activated on another computer, Do you want to reset the Key",
                        //            "Activation Key Problem", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                        //{
                        //    _unitOfWork.Repository<ProductActivationDTO>().Delete(activation);
                        //    _unitOfWork.Commit();
                        //    //new Activations().Show();
                        //    _activations = true;
                        //}
                        _activations = true;
                    }
                }
            }
            catch
            {
                MessageBox.Show(
                    Singleton.Edition == AMStockEdition.ServerEdition
                        ? "Problem opening amstock, may be the server computer or the network not working properly! try again later.."
                        : "Problem opening amstock! try again later..",
                    "Error Opening", MessageBoxButton.OK, MessageBoxImage.Error);
                CloseWindow(SplashWindow);
            }
        }
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if (_login)
            {
                if (_unitOfWork.Repository<ClientDTO>().Query().Get().FirstOrDefault() == null)
                {
                    MessageBox.Show("The server is not yet ready for work, contact your administrator...");
                    CloseWindow(SplashWindow);
                }
                new Login().Show();
            }
            else if (_activations)
                new Activations().Show();
            CloseWindow(SplashWindow);
        }

        private ICommand _closeWindowCommand;
        public ICommand CloseWindowCommand
        {
            get
            {
                return _closeWindowCommand ?? (_closeWindowCommand = new RelayCommand<Object>(CloseWindow));
            }
        }
    
        private void CloseWindow(object obj)
        {
            if (obj == null) return;
            var window = obj as Window;
            if (window == null) return;
            window.Close();
        }
        #endregion
    }
}
