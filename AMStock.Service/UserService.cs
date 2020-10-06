using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AMStock.Core;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.Core.Models.Interfaces;
using AMStock.DAL;
using AMStock.DAL.Interfaces;
using AMStock.Repository;
using AMStock.Repository.Interfaces;
using AMStock.Service.Interfaces;

namespace AMStock.Service
{
    public class UserService : IUserService
    {
        #region Fields
        private IDbContext _iDbContext;
        private readonly bool _disposeWhenDone;
        #endregion

        #region Constructor
        public UserService()
        {
            InitializeDbContext();
        }
        public UserService(bool disposeWhenDone)
        {
            _disposeWhenDone = disposeWhenDone;
            InitializeDbContext();
        }
        public void InitializeDbContext()
        {
            _iDbContext = DbContextUtil.GetDbContextInstance();
        }
        #endregion

        #region Common Methods
        public UserDTO GetUser(int userId)
        {
            var user = _iDbContext.Set<UserDTO>().Include("Roles").Include("Warehouse").Include("Organization").FirstOrDefault(u => u.UserId == userId && u.Enabled == true);
            var role = _iDbContext.Set<UsersInRoles>().Include("User").Include("Role").Where(u => u.UserId == user.UserId).ToList();
            if (_disposeWhenDone)
                Dispose();
            return user;
        }

        public UserDTO GetByName(string displayName)
        {
            var user = _iDbContext.Set<UserDTO>().Include("Roles").Include("Warehouse").Include("Organization").FirstOrDefault(u => u.UserName == displayName && u.Enabled == true);
            var role = _iDbContext.Set<UsersInRoles>().Include("User").Include("Role").Where(u => u.UserId == user.UserId).ToList();
            if (_disposeWhenDone)
                Dispose();
            return user;
        }

        public IEnumerable<UserDTO> GetAll()
        {
            IEnumerable<UserDTO> piList;
            try
            {
                piList = _iDbContext.Set<UserDTO>().Include("Roles").Include("Warehouse").Include("Organization")
                    .Where(u => u.Enabled == true).ToList();

                #region For Eager Loading
                foreach (var userDTO in piList)
                {
                    var role = _iDbContext.Set<UsersInRoles>()
                        .Include("User")
                        .Include("Role")
                        .Where(u => u.UserId == userDTO.UserId)
                        .ToList();
                }
                #endregion
            }
            finally
            {
                Dispose(_disposeWhenDone);
            }
            return piList;
        }

        public IEnumerable<RoleDTO> GetAllRoles()
        {
            return _iDbContext.Set<RoleDTO>().Include("Users").ToList();
        }
        public IEnumerable<MembershipDTO> GetAllMemberShips()
        {
            return _iDbContext.Set<MembershipDTO>().ToList();
        }
        public IEnumerable<UsersInRoles> GetAllUsersInRoles()
        {
            return _iDbContext.Set<UsersInRoles>().ToList();
        }
        public string InsertOrUpdate(UserDTO user)
        {
            try
            {
                //WebSecurity.CreateUserAndAccount("medina", "pa12345", new { Status = 0, Enabled = true });

                var validate = Validate(user);
                if (!string.IsNullOrEmpty(validate))
                    return validate;

                if (ObjectExists(user))
                    return GenericMessages.DatabaseErrorRecordAlreadyExists;

                if (user.UserId == 0)
                {
                    _iDbContext.Set<UserDTO>().Add(user);
                }
                else
                {
                    _iDbContext.Set<UserDTO>().Add(user);
                    _iDbContext.Entry(user).State = EntityState.Modified;
                }
                _iDbContext.SaveChanges();

                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public string Disable(UserDTO user)
        {
            if (user == null)
                return GenericMessages.ObjectIsNull;

            string stat;
            try
            {
                _iDbContext.Set<UserDTO>().Add(user);
                _iDbContext.Entry(user).State = EntityState.Modified;
                _iDbContext.SaveChanges();

                stat = string.Empty;
            }
            catch (Exception exception)
            {
                stat = exception.Message;
            }
            return stat;
        }

        public int Delete(UserDTO user)
        {
            try
            {
                ((IObjectState)user).ObjectState = ObjectState.Deleted;
                _iDbContext.Set<UserDTO>().Attach(user);
                _iDbContext.Set<UserDTO>().Remove(user);
                _iDbContext.SaveChanges();
                return 0;
            }
            catch (Exception exception)
            {
                return -1;
            }
        }

        public bool ObjectExists(UserDTO user)
        {
            var objectExists = false;
            var dbContextInstance = DbContextUtil.GetDbContextInstance();
            try
            {
                var catExists = dbContextInstance.Set<UserDTO>().Include("Roles")
                    .FirstOrDefault(bp => bp.UserName == user.UserName && bp.UserId != user.UserId);
                if (catExists != null)
                    objectExists = true;
            }
            finally
            {
                dbContextInstance.Dispose();
            }

            return objectExists;
        }

        public string Validate(UserDTO user)
        {
            if (null == user)
                return GenericMessages.ObjectIsNull;

            //if (user.Warehouse == null)
            //    return "Warehouse " + GenericMessages.ObjectIsNull;

            //if (user.OrganizationId == 0 || new OrganizationService(true).Find(user.OrganizationId.ToString()) == null)
            //    return "Organization is null/disabled ";

            if (String.IsNullOrEmpty(user.UserName))
                return user.UserName + " " + GenericMessages.StringIsNullOrEmpty;

            if (user.UserName.Length > 50)
                return user.UserName + " can not be more than 50 characters ";

            return string.Empty;
        }
        #endregion

        #region Disposing
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                _iDbContext.Dispose();
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}