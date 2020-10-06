using System;
using System.Collections.Generic;
using AMStock.Core;
using AMStock.Core.Models;

namespace AMStock.Service.Interfaces
{
    public interface IWarehouseService : IDisposable
    {
        IEnumerable<WarehouseDTO> GetAll(SearchCriteria<WarehouseDTO> criteria = null);
        WarehouseDTO Find(string warehouseId);
        WarehouseDTO GetByName(string displayName);
        string InsertOrUpdate(WarehouseDTO warehouse);
        string Disable(WarehouseDTO warehouse);
        int Delete(string warehouseId);
        IEnumerable<WarehouseDTO> GetWarehousesPrevilegedToUser(int userId);
    }
}