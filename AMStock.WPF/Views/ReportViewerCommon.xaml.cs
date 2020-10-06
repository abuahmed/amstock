using System.Windows;
using CrystalDecisions.CrystalReports.Engine;
using GalaSoft.MvvmLight.Messaging;


namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for ReportViewerCommon.xaml
    /// </summary>
    public partial class ReportViewerCommon : Window
    {
        public ReportViewerCommon()
        {
            InitializeComponent();
        }

        readonly ReportDocument _reportDocument;
        public ReportViewerCommon(ReportDocument report)
        {
            InitializeComponent();
            _reportDocument = report;
            Messenger.Default.Send<ReportDocument>(_reportDocument);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CrvReportViewer.ViewerCore.ReportSource = _reportDocument;
        }

    }
}
