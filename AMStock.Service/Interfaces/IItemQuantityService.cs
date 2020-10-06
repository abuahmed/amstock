using System;
using System.Collections.Generic;
using AMStock.Core;
using AMStock.Core.Models;

namespace AMStock.Service.Interfaces
{
    public interface IItemQuantityService : IDisposable
    {
        IEnumerable<ItemQuantityDTO> GetAll(SearchCriteria<ItemQuantityDTO> criteria = null);
        IEnumerable<ItemQuantityDTO> GetAll(SearchCriteria<ItemQuantityDTO> criteria, out int totalCount);
        ItemQuantityDTO Find(string itemQuantityId);
        ItemQuantityDTO GetByName(string displayName);
        string InsertOrUpdate(ItemQuantityDTO itemQuantity, bool insertPi);
        ItemQuantityDTO InsertOrUpdate(ItemQuantityDTO itemQuantity);
        string Disable(ItemQuantityDTO itemQuantity);
        int Delete(string itemQuantityId);
    }
}