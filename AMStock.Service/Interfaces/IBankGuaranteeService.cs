using System;
using System.Collections.Generic;
using AMStock.Core;
using AMStock.Core.Models;

namespace AMStock.Service.Interfaces
{
    public interface IBankGuaranteeService : IDisposable
    {
        IEnumerable<BankGuaranteeDTO> GetAll(SearchCriteria<BankGuaranteeDTO> criteria = null);
        BankGuaranteeDTO Find(string financialAccountId);
        BankGuaranteeDTO GetByName(string displayName);
        string InsertOrUpdate(BankGuaranteeDTO financialAccount);
        string Disable(BankGuaranteeDTO financialAccount);
        int Delete(string financialAccountId);
    }
}