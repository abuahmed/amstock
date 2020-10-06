#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using AMStock.Core;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.DAL;
using AMStock.Repository;
using AMStock.Repository.Interfaces;
using AMStock.Service;
using AMStock.Service.Interfaces;
using WebMatrix.WebData;

#endregion

namespace AMStock.WPF.ViewModel
{
    public class UserViewModel : ViewModelBase
    {
        #region Fields

        private static IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private ObservableCollection<UserDTO> _users;
        private UserDTO _selectedUser;
        private ICommand _addRoleViewCommand;
        private RoleDTO _selectedRole, _selectedRoleToAdd;
        private ObservableCollection<RoleDTO> _selectedRoles;
        private ICommand _saveUserViewCommand, _addNewUserViewCommand, _deleteUserViewCommand;
        private ICommand _closeUserViewCommand;
        private bool _editCommandVisibility, _isBlocked;

        #endregion

        #region Constructor

        public UserViewModel()
        {
            CleanUp();
            _unitOfWork = new UnitOfWork(DbContextUtil.GetDbContextInstance());
            _userService = new UserService();

            GetWarehouses();
            //SelectedUser = new UserDTO();
            //SelectedRole = new RoleDTO();
            //SelectedRoleToAdd = new RoleDTO();

            //Users = new ObservableCollection<UserDTO>();
            //Roles = new ObservableCollection<RoleDTO>();
            //SelectedRoles = new ObservableCollection<RoleDTO>();
            //FilteredRoles = new ObservableCollection<RoleDTO>();

            GetLiveRoles();
            GetLiveUsers();

            EditCommandVisibility = false;
            NewPasswordExpandibility = false;
        }

        public static void CleanUp()
        {
            if (_unitOfWork != null)
                _unitOfWork.Dispose();
        }

        #endregion

        #region Properties

        public bool EditCommandVisibility
        {
            get { return _editCommandVisibility; }
            set
            {
                _editCommandVisibility = value;
                RaisePropertyChanged<bool>(() => EditCommandVisibility);
            }
        }

        public bool IsBlocked
        {
            get { return _isBlocked; }
            set
            {
                _isBlocked = value;
                RaisePropertyChanged<bool>(() => IsBlocked);
                SelectedUser.Status = IsBlocked ? UserTypes.Blocked : UserTypes.Active;
            }
        }

        public UserDTO SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                _selectedUser = value;
                RaisePropertyChanged<UserDTO>(() => SelectedUser);
                if (SelectedUser != null && SelectedUser.UserId != 0)
                {
                    SelectedWarehouse = SelectedUser.Warehouse != null ? Warehouses.FirstOrDefault(w => w.Id == SelectedUser.WarehouseId) : Warehouses.FirstOrDefault(w => w.Id == -1);
                    
                    IList<RoleDTO> selectedRolesList = SelectedUser.Roles.Select(userroles => userroles.Role).ToList();
                    SelectedRoles = new ObservableCollection<RoleDTO>(selectedRolesList);
                    try
                    {
                        FilteredRoles = new ObservableCollection<RoleDTO>(Roles.Except(SelectedRoles));
                    }
                    catch
                    {
                    }

                    EditCommandVisibility = true;
                }
                else
                {
                    EditCommandVisibility = false;
                }
            }
        }

        public ObservableCollection<UserDTO> Users
        {
            get { return _users; }
            set
            {
                _users = value;
                RaisePropertyChanged<ObservableCollection<UserDTO>>(() => Users);

                if (Users.Count > 0)
                    SelectedUser = Users.FirstOrDefault();
                else
                    ExecuteAddNewUserViewCommand();
            }
        }

        #endregion

        #region Commands

        public ICommand AddNewUserViewCommand
        {
            get
            {
                return _addNewUserViewCommand ??
                       (_addNewUserViewCommand = new RelayCommand(ExecuteAddNewUserViewCommand));
            }
        }

        private void ExecuteAddNewUserViewCommand()
        {
            SelectedUser = new UserDTO
            {
                Status = UserTypes.Waiting,
                NewPassword = new Random().Next(123456, 134567).ToString()
            };

            SelectedRoles = new ObservableCollection<RoleDTO>();
            GetLiveRoles();
            FilteredRoles = new ObservableCollection<RoleDTO>(Roles.ToList()); //.Skip(5)

            AllRolesChecked = true;
            NewPasswordExpandibility = true;
            AddRoleEnability = false;
            RemoveRoleEnability = false;
        }

        public ICommand SaveUserViewCommand
        {
            get
            {
                return _saveUserViewCommand ??
                       (_saveUserViewCommand = new RelayCommand(ExecuteSaveUserViewCommand, CanSave));
            }
        }

        private void ExecuteSaveUserViewCommand()
        {
            try
            {
                if (Users.Any(u => u.UserName == SelectedUser.UserName
                                   && u.UserId != SelectedUser.UserId))
                {
                    MessageBox.Show("There exist user with same username, try another username... ");
                    return;
                }
                SelectedUser.Roles = new List<UsersInRoles>();
                foreach (var role in SelectedRoles)
                {
                    SelectedUser.Roles.Add(new UsersInRoles {UserId = SelectedUser.UserId, Role = role});
                }

                SelectedUser.DateLastModified = DateTime.Now;

                if (SelectedWarehouse != null && SelectedWarehouse.Id != -1)
                    SelectedUser.WarehouseId = SelectedWarehouse.Id;
                else
                    SelectedUser.Warehouse = null;
                

                if (SelectedUser.UserId == 0)
                {
                    SelectedUser.DateRecordCreated = DateTime.Now;
                    if (!string.IsNullOrEmpty(SelectedUser.NewPassword))
                    {
                        SelectedUser.Password = SelectedUser.NewPassword;
                        SelectedUser.ConfirmPassword = SelectedUser.NewPassword;
                    }
                    WebSecurity.CreateUserAndAccount(SelectedUser.UserName, SelectedUser.Password,
                        new
                        {
                            Status = 1,
                            Enabled = true,
                            RowGuid = Guid.NewGuid(),
                            Email = SelectedUser.Email ?? "",
                            CreatedByUserId = Singleton.User.UserId,
                            DateRecordCreated = DateTime.Now,
                            ModifiedByUserId = Singleton.User.UserId,
                            DateLastModified = DateTime.Now
                        });
                    SelectedUser.UserId = WebSecurity.GetUserId(SelectedUser.UserName);
                    var membershipDTO =
                        new UserService().GetAllMemberShips().FirstOrDefault(m => m.UserId == SelectedUser.UserId);

                    try
                    {
                        using (var dbCon = DbContextUtil.GetDbContextInstance())
                        {
                            dbCon.Set<MembershipDTO>().Add(membershipDTO);
                            dbCon.Entry(membershipDTO).State = EntityState.Modified;
                            dbCon.SaveChanges();
                            //dbCon.Dispose();
                        }
                    }
                    catch
                    {
                    }
                }
                _userService.InsertOrUpdate(SelectedUser);

                GetLiveUsers();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public ICommand DeleteUserViewCommand
        {
            get
            {
                return _deleteUserViewCommand ??
                       (_deleteUserViewCommand = new RelayCommand(ExecuteDeleteUserViewCommand));
            }
        }

        private void ExecuteDeleteUserViewCommand()
        {
            try
            {
                if (
                    MessageBox.Show("Are you Sure You want to Delete the user?", "Delete User",
                        MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    SelectedUser.Enabled = false;
                    _userService.Disable(SelectedUser);
                    //_unitOfWork.Repository<UserDTO>().Update(SelectedUser);
                    //_unitOfWork.Commit();

                    GetLiveUsers();
                }
            }
            catch
            {
            }
        }

        public ICommand CloseUserViewCommand
        {
            get
            {
                return _closeUserViewCommand ??
                       (_closeUserViewCommand = new RelayCommand<Object>(ExecuteCloseUserViewCommand));
            }
        }

        private void ExecuteCloseUserViewCommand(object obj)
        {
            try
            {
                CloseWindow(obj);
            }
            catch
            {
            }
        }

        private void CloseWindow(object obj)
        {
            if (obj == null) return;
            var window = obj as Window;
            if (window == null) return;
            window.DialogResult = true;
            window.Close();
        }

        #endregion

        #region Roles

        private ObservableCollection<RoleDTO> _roles, _filteredRoles;
        private bool _addRoleEnability, _removeRoleEnability, _allRolesChecked;

        public RoleDTO SelectedRole
        {
            get { return _selectedRole; }
            set
            {
                _selectedRole = value;
                RaisePropertyChanged<RoleDTO>(() => SelectedRole);
                RemoveRoleEnability = SelectedRole != null;
            }
        }

        public RoleDTO SelectedRoleToAdd
        {
            get { return _selectedRoleToAdd; }
            set
            {
                _selectedRoleToAdd = value;
                RaisePropertyChanged<RoleDTO>(() => SelectedRoleToAdd);

                AddRoleEnability = SelectedRoleToAdd != null;
            }
        }

        public ObservableCollection<RoleDTO> SelectedRoles
        {
            get { return _selectedRoles; }
            set
            {
                _selectedRoles = value;
                RaisePropertyChanged<ObservableCollection<RoleDTO>>(() => SelectedRoles);
            }
        }

        public ObservableCollection<RoleDTO> Roles
        {
            get { return _roles; }
            set
            {
                _roles = value;
                RaisePropertyChanged<ObservableCollection<RoleDTO>>(() => Roles);
            }
        }

        public ObservableCollection<RoleDTO> FilteredRoles
        {
            get { return _filteredRoles; }
            set
            {
                _filteredRoles = value;
                RaisePropertyChanged<ObservableCollection<RoleDTO>>(() => FilteredRoles);
            }
        }

        public bool AddRoleEnability
        {
            get { return _addRoleEnability; }
            set
            {
                _addRoleEnability = value;
                RaisePropertyChanged<bool>(() => AddRoleEnability);
            }
        }

        public bool RemoveRoleEnability
        {
            get { return _removeRoleEnability; }
            set
            {
                _removeRoleEnability = value;
                RaisePropertyChanged<bool>(() => RemoveRoleEnability);
            }
        }

        public bool AllRolesChecked
        {
            get { return _allRolesChecked; }
            set
            {
                _allRolesChecked = value;
                RaisePropertyChanged<bool>(() => AllRolesChecked);

                try
                {
                    if (AllRolesChecked)
                    {
                        SelectedRoles = new ObservableCollection<RoleDTO>(Roles);
                        FilteredRoles = new ObservableCollection<RoleDTO>();
                    }
                    else
                    {
                        SelectedRoles = new ObservableCollection<RoleDTO>();
                        FilteredRoles = new ObservableCollection<RoleDTO>(Roles.Except(SelectedRoles));
                    }
                }
                catch
                {
                    MessageBox.Show("Can't Remove Role");
                }
            }
        }

        public ICommand AddRoleViewCommand
        {
            get
            {
                return _addRoleViewCommand ?? (_addRoleViewCommand = new RelayCommand(ExcuteAddRoleViewCommand, CanSave));
            }
        }

        private void ExcuteAddRoleViewCommand()
        {
            try
            {
                SelectedRoles.Add(SelectedRoleToAdd);
                FilteredRoles = new ObservableCollection<RoleDTO>(Roles.Except(SelectedRoles));
            }
            catch
            {
                MessageBox.Show("Can't Save Role");
            }
        }

        public ICommand RemoveRoleViewCommand
        {
            get
            {
                return _deleteRoleViewCommand ??
                       (_deleteRoleViewCommand = new RelayCommand(ExecuteRemoveRoleViewCommand));
            }
        }

        private void ExecuteRemoveRoleViewCommand()
        {
            try
            {
                SelectedRoles.Remove(SelectedRole);
                FilteredRoles = new ObservableCollection<RoleDTO>(Roles.Except(SelectedRoles));
            }
            catch
            {
                MessageBox.Show("Can't Remove Role");
            }
        }

        #endregion

        #region GetNewPassword

        private ICommand _getNewPassword;

        public ICommand GetNewPassword
        {
            get { return _getNewPassword ?? (_getNewPassword = new RelayCommand(ExcuteGetNewPassword)); }
        }

        private void ExcuteGetNewPassword()
        {
            try
            {
                var newPassword = new Random().Next(123456, 134567).ToString();
                SelectedUser.NewPassword = newPassword;

                var token = WebSecurity.GeneratePasswordResetToken(SelectedUser.UserName);
                WebSecurity.ResetPassword(token, newPassword);
                MessageBox.Show("Password Reset Successfully!");
            }
            catch
            {
                MessageBox.Show("Problem on Resetting Password!");
            }
        }


        private bool _newPasswordExpandibility;
        private ICommand _deleteRoleViewCommand;

        public bool NewPasswordExpandibility
        {
            get { return _newPasswordExpandibility; }
            set
            {
                _newPasswordExpandibility = value;
                RaisePropertyChanged<bool>(() => NewPasswordExpandibility);
            }
        }

        #endregion

        #region Load Users Roles

        private void GetLiveUsers()
        {
            Users = new ObservableCollection<UserDTO>(_userService.GetAll().Where(u => u.UserId > 2).ToList()); //
        }

        private void GetLiveRoles()
        {
            Roles = new ObservableCollection<RoleDTO>(_userService.GetAllRoles().ToList().OrderBy(i => i.RoleId));
            //Roles = new ObservableCollection<RoleDTO>(Roles);//Roles.Skip(5)
        }

        #endregion

        #region Warehouse

        private IEnumerable<WarehouseDTO> _warehouses;
        private WarehouseDTO _selectedWarehouse;

        public IEnumerable<WarehouseDTO> Warehouses
        {
            get { return _warehouses; }
            set
            {
                _warehouses = value;
                RaisePropertyChanged<IEnumerable<WarehouseDTO>>(() => Warehouses);
            }
        }

        public WarehouseDTO SelectedWarehouse
        {
            get { return _selectedWarehouse; }
            set
            {
                _selectedWarehouse = value;
                RaisePropertyChanged<WarehouseDTO>(() => SelectedWarehouse);
            }
        }

        public void GetWarehouses()
        {
            IList<WarehouseDTO> warehouselist = _unitOfWork.Repository<WarehouseDTO>()
                .Query()
                .Get()
                .OrderByDescending(w => w.Id).ToList();
            if (warehouselist.Count > 1)
                warehouselist.Insert(0, new WarehouseDTO
                {
                    DisplayName = "All",
                    Id = -1
                });
            Warehouses = warehouselist;
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