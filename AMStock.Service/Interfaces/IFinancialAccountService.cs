using System;
using System.Collections.Generic;
using AMStock.Core;
using AMStock.Core.Models;

namespace AMStock.Service.Interfaces
{
    public interface IFinancialAccountService : IDisposable
    {
        IEnumerable<FinancialAccountDTO> GetAll(SearchCriteria<FinancialAccountDTO> criteria = null);
        FinancialAccountDTO Find(string financialAccountId);
        FinancialAccountDTO GetByName(string displayName);
        string InsertOrUpdate(FinancialAccountDTO financialAccount);
        string Disable(FinancialAccountDTO financialAccount);
        int Delete(string financialAccountId);
    }
}