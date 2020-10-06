using System.Windows.Input;
using AMStock.Core.Common;
using AMStock.WPF.Views;
using CrystalDecisions.CrystalReports.Engine;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Command;

namespace AMStock.WPF.ViewModel
{
    public class ReportViewerViewModel : ViewModelBase
    {
        #region Fields
        private ReportDocument _reportToView; 
        private EmailDTO _emailDetail;
        private string _sendEmailCommandVisibility;
        #endregion

        #region Constructor
        public ReportViewerViewModel()
        {
            Messenger.Default.Register<ReportDocument>(this, (message) =>
            {
                ReportToView = message;
            });
            SendEmailCommandVisibility = "Collapsed";
        } 
        #endregion

        #region Public Properties
        public EmailDTO EmailDetail
        {
            get { return _emailDetail; }
            set
            {
                _emailDetail = value;
                RaisePropertyChanged<EmailDTO>(() => EmailDetail);
            }
        }
        public ReportDocument ReportToView
        {
            get { return _reportToView; }
            set
            {
                _reportToView = value;
                RaisePropertyChanged<ReportDocument>(() => ReportToView);
            }
        }  
        public string SendEmailCommandVisibility
        {
            get { return _sendEmailCommandVisibility; }
            set
            {
                _sendEmailCommandVisibility = value;
                RaisePropertyChanged<string>(() => SendEmailCommandVisibility);
            }
        }
        #endregion

        #region Commands
       
        private ICommand _sendEmailCommand;
        public ICommand SendEmailCommand
        {
            get
            {
                return _sendEmailCommand ?? (_sendEmailCommand = new RelayCommand(ExcuteSendEmailCommand));
            }
        }

        private void ExcuteSendEmailCommand()
        {
            var sendEmail = new SendEmail(ReportToView,EmailDetail);
            sendEmail.ShowDialog();
        }



        #endregion
    }
}
