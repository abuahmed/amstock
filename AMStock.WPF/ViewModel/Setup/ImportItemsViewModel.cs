using System.Data.OleDb;
using System.Windows.Forms;
using System.Windows.Input;
using AMStock.Core.Models;
using AMStock.DAL;
using AMStock.Repository;
using AMStock.Repository.Interfaces;
using AMStock.WPF.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace AMStock.WPF.ViewModel
{
    public class ImportItemsViewModel : ViewModelBase
    {
        #region Fileds
        private static IUnitOfWork _unitOfWork;
        private ICommand _browseFileCommand, _importCommand;
        private ImportModel _selectedImportModel;

        #endregion

        #region Constructor
        public ImportItemsViewModel()
        {
            CleanUp();
            _unitOfWork = new UnitOfWork(DbContextUtil.GetDbContextInstance());
            SelectedImportModel = new ImportModel();
        }
        public static void CleanUp()
        {
            if (_unitOfWork != null)
                _unitOfWork.Dispose();
        }
        #endregion

        #region Public Properties

        public ImportModel SelectedImportModel
        {
            get { return _selectedImportModel; }
            set
            {
                _selectedImportModel = value;
                RaisePropertyChanged<ImportModel>(() => SelectedImportModel);
            }
        }

        #endregion

        #region Commands
        public ICommand BrowseFileCommand
        {
            get
            {
                return _browseFileCommand ?? (_browseFileCommand = new RelayCommand(ExecuteBrowseFile));
            }
        }
        private void ExecuteBrowseFile()
        {
            var open = new OpenFileDialog { Filter = "Excel Files(*.xls; *.xlsx; *.csv;)|*.xls; *.xlsx; *.csv; " };
            if (open.ShowDialog() == DialogResult.OK)
            {
                SelectedImportModel.FileName = open.FileName;
            }
        }

        public ICommand ImportCommand
        {
            get { return _importCommand ?? (_importCommand = new RelayCommand(ExecuteImportCommand, CanSave)); }
        }
        private void ExecuteImportCommand()
        {
            try
            {
                //var oconn = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                //    SelectedImportModel.FileName +
                //    ";Extended Properties=Excel 8.0");
                var oconn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source='" +
                                        SelectedImportModel.FileName +
                                        "';Extended Properties= 'Excel 8.0;HDR=Yes;IMEX=1'");//
                var ocmd = new OleDbCommand("select AccountID,Type,AccountDescription from [" +
                    SelectedImportModel.SheetName + "$]", oconn);
                oconn.Open();

                var odr = ocmd.ExecuteReader();

                if (odr != null)
                {
                    odr.Read();//Skip first Row
                    while (odr.Read())
                    {
                        var item = new ItemDTO()
                        {
                            ItemCode = odr.GetString(0),
                            DisplayName = odr.GetString(1),
                            Description = odr.GetString(2),
                            PurchasePrice = odr.GetDecimal(3),
                            SellPrice = odr.GetDecimal(4),
                        };
                    }
                }
            }
            catch
            {
            }
        }
        #endregion

        #region Validation
        public static int Errors { get; set; }
        public bool CanSave()
        {
            return Errors == 0;
        }
        #endregion
    }
}