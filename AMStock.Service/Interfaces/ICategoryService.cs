using System;
using System.Collections.Generic;
using AMStock.Core;
using AMStock.Core.Models;

namespace AMStock.Service.Interfaces
{
    public interface ICategoryService : IDisposable
    {
        IEnumerable<CategoryDTO> GetAll(SearchCriteria<CategoryDTO> criteria = null);
        CategoryDTO Find(string categoryId);
        CategoryDTO GetByName(string displayName);
        string InsertOrUpdate(CategoryDTO category);
        string Disable(CategoryDTO category);
        int Delete(string categoryId);
    }
}