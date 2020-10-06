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
    public class CpoService : ICpoService
    {
        #region Fields
        private IUnitOfWork _unitOfWork;
        private IRepository<CpoDTO> _cpoRepository;
        private readonly bool _disposeWhenDone;
        #endregion

        #region Constructor
        public CpoService()
        {
            InitializeDbContext();
        }
        public CpoService(bool disposeWhenDone)
        {
            _disposeWhenDone = disposeWhenDone;
            InitializeDbContext();
        }
        public void InitializeDbContext()
        {
            var iDbContext = DbContextUtil.GetDbContextInstance();
            _cpoRepository = new Repository<CpoDTO>(iDbContext);
            _unitOfWork = new UnitOfWork(iDbContext);
        }

        #endregion

        #region Common Methods

        public IRepositoryQuery<CpoDTO> Get()
        {
            var piList = _cpoRepository
                .Query()
                .OrderBy(q => q.OrderBy(c => c.Id));
            return piList;
        }

        public IEnumerable<CpoDTO> GetAll(SearchCriteria<CpoDTO> criteria = null)
        {
            IEnumerable<CpoDTO> catList = new List<CpoDTO>();
            try
            {
                if (criteria != null && criteria.CurrentUserId != -1)
                {
                    var warehouseList = new WarehouseService(true)
                        .GetWarehousesPrevilegedToUser(criteria.CurrentUserId).ToList();
                    if (criteria.SelectedWarehouseId != null)
                        warehouseList = warehouseList.Where(w => w.Id == criteria.SelectedWarehouseId).ToList();

                    foreach (var warehouse in warehouseList.Where(w => w.Id != -1))
                    {
                        var pdto = Get();

                        foreach (var cri in criteria.FiList)
                        {
                            pdto.FilterList(cri);
                        }

                        #region By Warehouse
                        var warehouse1 = warehouse;
                        pdto.FilterList(p => p.WarehouseId == warehouse1.Id);
                        #endregion

                        #region By Duration

                        if (criteria.BeginingDate != null)
                        {
                            var beginDate = new DateTime(criteria.BeginingDate.Value.Year, criteria.BeginingDate.Value.Month,
                                criteria.BeginingDate.Value.Day, 0, 0, 0);
                            pdto.FilterList(p => p.PreparedDate >= beginDate);
                        }

                        if (criteria.EndingDate != null)
                        {
                            var endDate = new DateTime(criteria.EndingDate.Value.Year, criteria.EndingDate.Value.Month,
                                criteria.EndingDate.Value.Day, 23, 59, 59);
                            pdto.FilterList(p => p.PreparedDate <= endDate);
                        }


                        #endregion

                        #region By Cpo Type
                        if (criteria.TransactionType != -1)
                            switch (criteria.TransactionType)
                            {
                                case 0:
                                    break;
                                case 1:
                                    pdto.FilterList(p => !p.IsReturned);
                                    break;
                                case 2:
                                    pdto.FilterList(p => p.IsReturned);
                                    break;
                            }
                        #endregion

                        IList<CpoDTO> pdtoList;
                        if (criteria.Page != 0 && criteria.PageSize != 0)
                        {
                            int totalCount;
                            pdtoList = pdto.GetPage(criteria.Page, criteria.PageSize, out totalCount).ToList();
                        }
                        else
                            pdtoList = pdto.GetList().ToList();

                        catList = catList.Concat(pdtoList).ToList();
                    }
                }
                else
                    catList = Get().Get().ToList();
            }
            finally
            {
                Dispose(_disposeWhenDone);
            }

            return catList;
        }

        public CpoDTO Find(string cpoId)
        {
            return _cpoRepository.FindById(Convert.ToInt32(cpoId));
        }

        public CpoDTO GetByName(string displayName)
        {
            var cat = _cpoRepository.Query().Filter(c => c.Number == displayName).Get().FirstOrDefault();
            return cat;
        }

        public string InsertOrUpdate(CpoDTO cpo)
        {
            try
            {
                var validate = Validate(cpo);
                if (!string.IsNullOrEmpty(validate))
                    return validate;

                if (ObjectExists(cpo))
                    return GenericMessages.DatabaseErrorRecordAlreadyExists;

                _cpoRepository.InsertUpdate(cpo);
                _unitOfWork.Commit();
                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public string Disable(CpoDTO cpo)
        {
            if (cpo == null)
                return GenericMessages.ObjectIsNull;

            string stat;
            try
            {
                _cpoRepository.Update(cpo);
                _unitOfWork.Commit();
                stat = string.Empty;
            }
            catch (Exception exception)
            {
                stat = exception.Message;
            }
            return stat;
        }

        public int Delete(string cpoId)
        {
            try
            {
                _cpoRepository.Delete(cpoId);
                _unitOfWork.Commit();
                return 0;
            }
            catch (Exception exception)
            {
                return -1;
            }
        }

        public bool ObjectExists(CpoDTO cpo)
        {
            var objectExists = false;
            var iDbContext = DbContextUtil.GetDbContextInstance();
            try
            {
                var catRepository = new Repository<CpoDTO>(iDbContext);
                var catExists = catRepository.Query()
                    .Filter(bp => bp.Number == cpo.Number && bp.Id != cpo.Id)
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

        public string Validate(CpoDTO cpo)
        {
            if (null == cpo)
                return GenericMessages.ObjectIsNull;

            if (String.IsNullOrEmpty(cpo.Number))
                return cpo.Number + " " + GenericMessages.StringIsNullOrEmpty;

            if (String.IsNullOrEmpty(cpo.ToCompany))
                return cpo.ToCompany + " " + GenericMessages.StringIsNullOrEmpty;

            if (cpo.Amount < 1 || cpo.Amount > 1000000)
                return " cpo Amount is above allowed limit";

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