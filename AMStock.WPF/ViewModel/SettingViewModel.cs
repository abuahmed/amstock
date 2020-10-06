using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using AMStock.Core.Common;
using AMStock.Core.Enumerations;
using AMStock.DAL;
using AMStock.Repository;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using AMStock.Core;
using AMStock.Core.Models;
using AMStock.Repository.Interfaces;
using MessageBox = System.Windows.MessageBox;

namespace AMStock.WPF.ViewModel
{
    public class SettingViewModel : ViewModelBase
    {
        #region Fields
        private static IUnitOfWork _unitOfWork;
        private SettingDTO _currentSetting;
        private TaxTypes _selectedTaxType;
        private ICommand _saveSettingViewCommand;
        private ICommand _closeWindowCommand;

        #endregion

        #region Constructor
        public SettingViewModel()
        {
            CleanUp();
            _unitOfWork = new UnitOfWork(DbContextUtil.GetDbContextInstance());
            CheckRoles();
            CurrentSetting = _unitOfWork.Repository<SettingDTO>().Query().Get().FirstOrDefault();
        }
        public static void CleanUp()
        {
            if (_unitOfWork != null)
                _unitOfWork.Dispose();
        }
        #endregion

        #region Properties
        public SettingDTO CurrentSetting
        {
            get { return _currentSetting; }
            set
            {
                _currentSetting = value;
                RaisePropertyChanged<SettingDTO>(() => CurrentSetting);
                if (CurrentSetting != null)
                {
                    //SelectedTaxType = CurrentSetting.TaxType;
                }
            }
        }
        public TaxTypes SelectedTaxType
        {
            get { return _selectedTaxType; }
            set
            {
                _selectedTaxType = value;
                RaisePropertyChanged<TaxTypes>(() => SelectedTaxType);

            }
        }
        #endregion

        #region Commands
        public ICommand SaveSettingCommand
        {
            get { return _saveSettingViewCommand ?? (_saveSettingViewCommand = new RelayCommand<Object>(ExecuteSaveSettingViewCommand, CanSave)); }
        }
        private void ExecuteSaveSettingViewCommand(object obj)
        {
            try
            {
                CurrentSetting.DateLastModified = DateTime.Now;
                CurrentSetting.ModifiedByUserId = Singleton.User.UserId;

                //CurrentSetting.TaxType = SelectedTaxType;
                _unitOfWork.Repository<SettingDTO>().Update(CurrentSetting);
                _unitOfWork.Commit();
                System.Windows.Forms.MessageBox.Show(
                                "You have to restart AMSTOCK to see the new changes made...",
                                "Options Saved",
                                MessageBoxButtons.OK, 
                                MessageBoxIcon.Information);
                CloseWindow(obj);
            }
            catch
            {
                MessageBox.Show("Can't Save Setting!");
            }
        }
        public ICommand CloseWindowCommand
        {
            get
            {
                return _closeWindowCommand ?? (_closeWindowCommand = new RelayCommand<Object>(CloseWindow));
            }
        }
        public void CloseWindow(object obj)
        {
            if (obj == null) return;
            var window = obj as System.Windows.Window;
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

        #endregion

        #region Previlege Visibility
        private UserRolesModel _userRoles;

        public UserRolesModel UserRoles
        {
            get { return _userRoles; }
            set
            {
                _userRoles = value;
                RaisePropertyChanged<UserRolesModel>(() => UserRoles);
            }
        }

        private void CheckRoles()
        {
            UserRoles = Singleton.UserRoles;
        }

        #endregion
    }
}
