using System;
using System.Collections.Generic;
using AMStock.Core;
using AMStock.Core.Models;

namespace AMStock.Service.Interfaces
{
    public interface ICpoService : IDisposable
    {
        IEnumerable<CpoDTO> GetAll(SearchCriteria<CpoDTO> criteria = null);
        CpoDTO Find(string cpoId);
        CpoDTO GetByName(string displayName);
        string InsertOrUpdate(CpoDTO cpo);
        string Disable(CpoDTO cpo);
        int Delete(string cpoId);
    }
}