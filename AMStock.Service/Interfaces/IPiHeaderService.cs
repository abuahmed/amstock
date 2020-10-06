using System;
using System.Collections.Generic;
using AMStock.Core;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;

namespace AMStock.Service.Interfaces
{
    public interface IPiHeaderService : IDisposable
    {
        IEnumerable<PhysicalInventoryHeaderDTO> GetAll(SearchCriteria<PhysicalInventoryHeaderDTO> criteria = null);
        PhysicalInventoryHeaderDTO Find(string piHeaderId);
        string InsertOrUpdate(PhysicalInventoryHeaderDTO piHeader);
        string Post(PhysicalInventoryHeaderDTO piHeader);
        string Disable(PhysicalInventoryHeaderDTO piHeader);
        int Delete(string piHeaderId);

        PhysicalInventoryLineDTO GetNewPiLine(int selectedWarehouse, int selectedItem,
            decimal countedQty, PhysicalInventoryLineTypes lineType);

        string GetNewPhysicalInventoryNumber(int selectedWarehouse, bool disposeWhenDone);

        IEnumerable<PhysicalInventoryLineDTO> GetChilds(int parentId, bool disposeWhenDone);
        string InsertOrUpdateChild(PhysicalInventoryLineDTO piLine);
        string DisableChild(PhysicalInventoryLineDTO piLine);
    }
}