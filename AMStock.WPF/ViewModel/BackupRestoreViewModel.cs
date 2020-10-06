#region Using
using System;
using System.Windows;
using AMStock.DAL;
using AMStock.Repository;
//using Microsoft.SqlServer.Management.Smo;
//using Microsoft.SqlServer.Management.Common;
using System.IO;
using System.Windows.Forms;
using GalaSoft.MvvmLight;
using AMStock.Core;
using AMStock.Core.Enumerations;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using MessageBox = System.Windows.Forms.MessageBox;
using AMStock.Repository.Interfaces; 
#endregion


namespace AMStock.WPF.ViewModel
{
    public class BackupRestoreViewModel : ViewModelBase
    {
        #region Fields
        //private static Server _server;
        public static string ServerName = ".";
        private static IUnitOfWork _unitOfWork;
        #endregion

        #region Constructor
        public BackupRestoreViewModel()
        {
            CleanUp();
            _unitOfWork = new UnitOfWork(DbContextUtil.GetDbContextInstance());
            if (Singleton.Edition == AMStockEdition.ServerEdition)
            {
                //var serverConnection = new ServerConnection(ServerName)
                //{
                //    LoginSecure = false,
                //    Login = "sa",
                //    Password = "amihan"
                //};
                //_server = new Server(serverConnection);
            }
            ProgressBarVisibility = "Collapsed";
            CommandsEnability = true;
            Messenger.Default.Register<object>(this, (message) =>
            {
                MainWindow = message;
            });
        }
        public static void CleanUp()
        {
            if (_unitOfWork != null)
                _unitOfWork.Dispose();
        }
        #endregion

        #region Properties

        private object _mainWindow;
        public object MainWindow
        {
            get { return _mainWindow; }
            set
            {
                _mainWindow = value;
                RaisePropertyChanged<object>(() => MainWindow);
            }
        }

        private string _progressBarVisibility;
        public string ProgressBarVisibility
        {
            get { return _progressBarVisibility; }
            set
            {
                _progressBarVisibility = value;
                RaisePropertyChanged<string>(() => ProgressBarVisibility);
            }
        }


        private bool _commandsEnability;
        public bool CommandsEnability
        {
            get { return _commandsEnability; }
            set
            {
                _commandsEnability = value;
                RaisePropertyChanged<bool>(() => this.CommandsEnability);
            }
        }

        private string _fileLocation;
        public string FileLocation
        {
            get { return _fileLocation; }
            set
            {
                _fileLocation = value;
                RaisePropertyChanged<string>(() => FileLocation);
            }
        }

        #endregion

        //#region Commands
        //private ICommand _backupCommand, _restoreCommand, _closeWindowCommand;

        //public ICommand BackupCommand
        //{
        //    get
        //    {
        //        return _backupCommand ?? (_backupCommand = new RelayCommand<Object>(BackUp));
        //    }
        //}
        //public ICommand RestoreCommand
        //{
        //    get
        //    {
        //        return _restoreCommand ?? (_restoreCommand = new RelayCommand<Object>(Restore));
        //    }
        //}

        //public ICommand CloseWindowCommand
        //{
        //    get
        //    {
        //        return _closeWindowCommand ?? (_closeWindowCommand = new RelayCommand<Object>(CloseWindow));
        //    }
        //}

        //private void CloseWindow(object obj)
        //{
        //    if (obj == null) return;
        //    var window = obj as Window;
        //    if (window != null)
        //    {
        //        window.Close();
        //    }
        //}

        //private void BackUp(object obj)
        //{
        //    ProgressBarVisibility = "Visible";
        //    if (Singleton.Edition == AMStockEdition.ServerEdition)
        //    {
        //        BackupSqlServerData(obj);
        //    }
        //    else
        //    {
        //        BackupCompactData(obj);
        //    }
        //    ProgressBarVisibility = "Collapsed";
        //}
        //private void Restore(object obj)
        //{
        //    ProgressBarVisibility = "Visible";
        //    if (Singleton.Edition == AMStockEdition.ServerEdition)
        //    {
        //        RestoreSqlServerData();
        //    }
        //    else
        //    {
        //        RestoreCompactData(obj);
        //    }
        //    ProgressBarVisibility = "Collapsed";
        //}
        //#endregion

        //#region SQLCE
        //public void BackupCompactData(object obj)
        //{
        //    try
        //    {
        //        var folder = new FolderBrowserDialog();
        //        if (folder.ShowDialog() != DialogResult.OK) return;
        //        FileLocation = folder.SelectedPath;
        //        try
        //        {
        //            var sourcefileName = Singleton.SqlceFileName;
        //            var sourceFile = new FileInfo(sourcefileName);

        //            var destFileName = "AMStockDb_" + DateTime.Now.ToLongDateString() + "_Backup.sdf";
        //            var destinationFile = Path.Combine(folder.SelectedPath, destFileName);

        //            var destFile = new FileInfo(destinationFile);
        //            if (destFile.Exists)
        //                File.Delete(destFile.FullName);

        //            if (sourceFile.Exists)
        //            {
        //                File.Copy(sourceFile.FullName, destinationFile);
        //                System.Windows.MessageBox.Show("Database Backup taken Successfully " + Environment.NewLine +
        //                    "You can get the file here:" + destinationFile, "Backup Success", MessageBoxButton.OK, MessageBoxImage.Information);
        //                CloseWindow(obj);
        //            }
        //            else
        //                MessageBox.Show("Can't Find the source file to copy to!" + Environment.NewLine + "tray again later...");
        //        }
        //        catch
        //        {
        //            MessageBox.Show("Can't Backup database file!" + Environment.NewLine + "tray again later...");
        //        }
        //    }
        //    catch
        //    {
        //        MessageBox.Show("Can't Backup database file!" + Environment.NewLine + "tray again later...");
        //    }
        //}
        //public void RestoreCompactData(object obj)
        //{
        //    try
        //    {
        //        var open = new OpenFileDialog { Filter = "Backup Files(*.sdf;)|*.sdf" };
        //        if (open.ShowDialog() == DialogResult.OK)
        //        {
        //            FileLocation = open.FileName;
        //            try
        //            {
        //                var destinationFile = Singleton.SqlceFileName;
        //                var destFile = new FileInfo(destinationFile);
        //                if (destFile.Exists)
        //                {
        //                    var path = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "\\OnefaceBACKUP" + "\\";
        //                    if (!Directory.Exists(path))
        //                        Directory.CreateDirectory(path);
        //                    var pathfile = Path.Combine(path, "AMStockDb_" + DateTime.Now.Ticks.ToString() + ".sdf");

        //                    File.Move(destFile.FullName, pathfile);
        //                }

        //                var sourcefilePath = open.FileName;
        //                var fi = new FileInfo(sourcefilePath);

        //                if (!fi.Exists) return;
        //                File.Copy(sourcefilePath, destinationFile);
        //                MessageBox.Show("Database Restored Successfully!" + Environment.NewLine +
        //                                "The System will be closed, you have to reopen the system to see the new restored data!",
        //                    "Restore Success",
        //                    MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                CloseWindow(obj);
        //                CloseWindow(MainWindow);
        //            }
        //            catch { }
        //        }
        //    }
        //    catch { }
        //}
        //#endregion

        //#region SQLServer
        //public void BackupSqlServerData(object obj)
        //{
        //    try
        //    {
        //        var folder = new FolderBrowserDialog();
        //        if (folder.ShowDialog() == DialogResult.OK)
        //        {
        //            FileLocation = folder.SelectedPath;
        //            if (_server != null)
        //            {
        //                try
        //                {
        //                    //this.Cursor = Cursors.WaitCursor;
        //                    var bkpDatabase = new Backup { Action = BackupActionType.Database, Database = "AMStockDb" };
        //                    var bkpDevice = new BackupDeviceItem(folder.SelectedPath + "\\" + DateTime.Now.ToLongDateString() + "_Backup.bak", DeviceType.File);
        //                    bkpDatabase.Devices.Add(bkpDevice);
        //                    bkpDatabase.SqlBackup(_server);
        //                    System.Windows.MessageBox.Show("Bakup of Database " + " successfully created", 
        //                        "Backup Success", MessageBoxButton.OK, MessageBoxImage.Information);
        //                    CloseWindow(obj);
        //                }
        //                catch (Exception x)
        //                {
        //                    MessageBox.Show("ERROR: An error ocurred while backing up DataBase" + x, "Backup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //                }
        //                finally
        //                {
        //                    //this.Cursor = Cursors.Arrow;
        //                }
        //            }
        //            else
        //            {
        //                MessageBox.Show("ERROR: A connection to a SQL server was not established.", "Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //                //this.Cursor = Cursors.Arrow;
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw new ApplicationException("Failed loading image");
        //    }

        //}
        //public void RestoreSqlServerData()
        //{
        //    try
        //    {
        //        var open = new OpenFileDialog { Filter = "Backup Files(*.bak;)|*.bak" };
        //        if (open.ShowDialog() != DialogResult.OK) return;
        //        FileLocation = open.FileName;
        //        if (_server != null)
        //        {
        //            try
        //            {
        //                //this.Cursor = Cursors.WaitCursor;                            
        //                var rstDatabase = new Restore { Action = RestoreActionType.Database, Database = "AMStockDb" };
        //                var bkpDevice = new BackupDeviceItem(open.FileName, DeviceType.File);
        //                rstDatabase.Devices.Add(bkpDevice);
        //                rstDatabase.ReplaceDatabase = true;
        //                rstDatabase.Restart = true;
        //                rstDatabase.SqlRestore(_server);
        //                MessageBox.Show("The Database is" + " succefully restored", "Server", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                //Close Main Window
        //            }
        //            catch (Exception)
        //            {
        //                MessageBox.Show("ERROR: An error ocurred while restoring the database", "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            }
        //            finally
        //            {
        //                //this.Cursor = Cursors.Arrow;
        //            }
        //        }
        //        else
        //        {
        //            MessageBox.Show("ERROR: A connection to a SQL server was not established.", "Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            //this.Cursor = Cursors.Arrow;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw new ApplicationException("Failed loading backup file");
        //    }
        //}
        //#endregion
        
    }
}
