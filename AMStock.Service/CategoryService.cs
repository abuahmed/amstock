using System;
using System.Collections.Generic;
using System.Linq;
using AMStock.Core;
using AMStock.Core.Models;
using AMStock.DAL;
using AMStock.Repository;
using AMStock.Repository.Interfaces;
using AMStock.Service.Interfaces;

namespace AMStock.Service
{
    public class CategoryService : ICategoryService
    {
        #region Fields
        private IUnitOfWork _unitOfWork;
        private IRepository<CategoryDTO> _categoryRepository;
        private readonly bool _disposeWhenDone;
        #endregion

        #region Constructor
        public CategoryService()
        {
            InitializeDbContext();
        }
        public CategoryService(bool disposeWhenDone)
        {
            _disposeWhenDone = disposeWhenDone;
            InitializeDbContext();
        }

        public void InitializeDbContext()
        {
            var iDbContext = DbContextUtil.GetDbContextInstance();
            _categoryRepository = new Repository<CategoryDTO>(iDbContext);
            _unitOfWork = new UnitOfWork(iDbContext);
        }
        #endregion

        #region Common Methods
        public IRepositoryQuery<CategoryDTO> Get()
        {
            var piList = _categoryRepository
                .Query()
                .Filter(a => !string.IsNullOrEmpty(a.DisplayName))
                .OrderBy(q => q.OrderBy(c => c.Id));
            return piList;
        }
        
        public IEnumerable<CategoryDTO> GetAll(SearchCriteria<CategoryDTO> criteria = null)
        {
            IEnumerable<CategoryDTO> catList = new List<CategoryDTO>();
            try
            {
                if (criteria != null)
                {
                    var pdto = Get();

                    foreach (var cri in criteria.FiList)
                    {
                        pdto.FilterList(cri);
                    }

                    IList<CategoryDTO> pdtoList;
                    if (criteria.Page != 0 && criteria.PageSize != 0)
                    {
                        int totalCount;
                        pdtoList = pdto.GetPage(criteria.Page, criteria.PageSize, out totalCount).ToList();
                    }
                    else
                        pdtoList = pdto.GetList().ToList();

                    catList = pdtoList.ToList();
                }
                else
                {
                    catList = Get().Get().ToList();
                }

            }
            finally
            {
                Dispose(_disposeWhenDone);
            }

            return catList;
        }
        
        public CategoryDTO Find(string categoryId)
        {
            return _categoryRepository.FindById(Convert.ToInt32(categoryId));
        }

        public CategoryDTO GetByName(string displayName)
        {
            var cat = _categoryRepository
                .Query()
                .Filter(c => c.DisplayName == displayName)
                .Get().FirstOrDefault();
            return cat;
        }

        public string InsertOrUpdate(CategoryDTO category)
        {
            try
            {
                var validate = Validate(category);
                if (!string.IsNullOrEmpty(validate))
                    return validate;

                if (ObjectExists(category))
                    return GenericMessages.DatabaseErrorRecordAlreadyExists;

                _categoryRepository.InsertUpdate(category);
                _unitOfWork.Commit();
                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public string Disable(CategoryDTO category)
        {
            if (category == null)
                return GenericMessages.ObjectIsNull;

            var catId = category.Id;
            string stat;
            var iDbContext = DbContextUtil.GetDbContextInstance();
            try
            {
                var items = new Repository<ItemDTO>(iDbContext).Query().Get();
                if (items.Any(i => i.CategoryId == catId || i.UnitOfMeasureId == catId))
                {
                    stat = GenericMessages.DatabaseErrorRecordAlreadyInUse;
                }
                else
                {
                    _categoryRepository.Update(category);
                    _unitOfWork.Commit();
                    stat = string.Empty;
                }
            }
            catch (Exception exception)
            {
                stat = exception.Message;
            }
            finally
            {
                iDbContext.Dispose();
            }
            return stat;
        }

        public int Delete(string categoryId)
        {
            try
            {
                _categoryRepository.Delete(categoryId);
                _unitOfWork.Commit();
                return 0;
            }
            catch (Exception exception)
            {
                return -1;
            }
        }

        public bool ObjectExists(CategoryDTO category)
        {
            var objectExists = false;
            var iDbContext = DbContextUtil.GetDbContextInstance();
            try
            {
                var catRepository = new Repository<CategoryDTO>(iDbContext);
                var catExists = catRepository.Query()
                    .Filter(bp => bp.DisplayName == category.DisplayName && bp.Id != category.Id)
                    .Get()
                    .FirstOrDefault();
                if (catExists != null)
                    objectExists = true;
            }
            finally
            {
                iDbContext.Dispose();
            }

            return objectExists;
        }

        public string Validate(CategoryDTO category)
        {
            if (null == category)
                return GenericMessages.ObjectIsNull;

            if (String.IsNullOrEmpty(category.DisplayName))
                return category.DisplayName + " " + GenericMessages.StringIsNullOrEmpty;

            if (category.DisplayName.Length > 255)
                return category.DisplayName + " can not be more than 255 characters ";

            return string.Empty;
        }

        #endregion

        #region Disposing
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWork.Dispose();
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}