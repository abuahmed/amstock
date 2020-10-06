using System;
using System.Collections.Generic;
using AMStock.Core;
using AMStock.Core.Models;

namespace AMStock.Service.Interfaces
{
    public interface IOrganizationService : IDisposable
    {
        IEnumerable<OrganizationDTO> GetAll(SearchCriteria<OrganizationDTO> criteria = null);
        OrganizationDTO Find(string organizationId);
        OrganizationDTO GetByName(string displayName);
        string InsertOrUpdate(OrganizationDTO organization);
        string Disable(OrganizationDTO organization);
        int Delete(string organizationId);
    }
}