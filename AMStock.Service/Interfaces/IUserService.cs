using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AMStock.Core;
using AMStock.Core.Models;

namespace AMStock.Service.Interfaces
{
    public interface IUserService : IDisposable
    {
        IEnumerable<UserDTO> GetAll();
        IEnumerable<RoleDTO> GetAllRoles();
        UserDTO GetUser(int userId);
        UserDTO GetByName(string displayName);
        string InsertOrUpdate(UserDTO user);
        string Disable(UserDTO user);
        int Delete(UserDTO user);
    }
}