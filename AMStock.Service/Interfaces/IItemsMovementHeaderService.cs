using System;
using System.Collections.Generic;
using AMStock.Core;
using AMStock.Core.Models;

namespace AMStock.Service.Interfaces
{
    public interface IItemsMovementHeaderService : IDisposable
    {
        IEnumerable<ItemsMovementHeaderDTO> GetAll(SearchCriteria<ItemsMovementHeaderDTO> criteria = null);
        ItemsMovementHeaderDTO Find(string imHeaderId);
        string InsertOrUpdate(ItemsMovementHeaderDTO imHeader);
        string Post(ItemsMovementHeaderDTO imHeader);
        string UnPost(ItemsMovementHeaderDTO imHeader);
        string Disable(ItemsMovementHeaderDTO imHeader);
        int Delete(string imHeaderId);

        //ItemsMovementLineDTO GetNewItemsMovementLine(int selectedWarehouse, int selectedItem,
        //    int countedQty);

        string GetNewMovementNumber(int selectedWarehouse, bool disposeWhenDone);

        IEnumerable<ItemsMovementLineDTO> GetChilds(int parentId, bool disposeWhenDone);
        string InsertOrUpdateChild(ItemsMovementLineDTO imLine);
        string DisableChild(ItemsMovementLineDTO imLine);
    }
}