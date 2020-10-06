using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AMStock.Core;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;

namespace AMStock.Service.Interfaces
{
    public interface IBusinessPartnerService : IDisposable
    {
        IEnumerable<BusinessPartnerDTO> GetAll(SearchCriteria<BusinessPartnerDTO> criteria=null);
        IEnumerable<BusinessPartnerDTO> GetAll(SearchCriteria<BusinessPartnerDTO> criteria, out int totalCount);
        BusinessPartnerDTO Find(string businessPartnerId);
        BusinessPartnerDTO GetByName(string displayName);
        string InsertOrUpdate(BusinessPartnerDTO businessPartner);
        string Disable(BusinessPartnerDTO businessPartner);
        int Delete(string businessPartnerId);
        //string GetBusinessPartnerCode(BusinessPartnerTypes businessPartnerType);

        Task<IEnumerable<BusinessPartnerDTO>> GetAsync();
        Task<IEnumerable<BusinessPartnerDTO>> FindAsync(string id);
        Task<string> InsertOrUpdateAync(BusinessPartnerDTO businessPartner);
    }
}