using System;
using System.Collections.Generic;
using AMStock.Core;
using AMStock.Core.Models;

namespace AMStock.Service.Interfaces
{
    public interface IItemBorrowService : IDisposable
    {
        IEnumerable<ItemBorrowDTO> GetAll(SearchCriteria<ItemBorrowDTO> criteria = null);
        ItemBorrowDTO Find(string itemBorrowId);
        ItemBorrowDTO GetByName(string displayName);
        string InsertOrUpdate(ItemBorrowDTO itemBorrow);
        string Disable(ItemBorrowDTO itemBorrow);
        int Delete(string itemBorrowId);
    }
}