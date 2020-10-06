using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using AMStock.Core;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.DAL;
using AMStock.Repository;
using AMStock.Repository.Interfaces;
using AMStock.Service.Interfaces;

namespace AMStock.Service
{
    public class WarehouseService : IWarehouseService
    {
        #region Fields
        private IUnitOfWork _unitOfWork;
        private IRepository<WarehouseDTO> _warehouseRepository;
        private readonly bool _disposeWhenDone;
        #endregion

        #region Constructor
        public WarehouseService()
        {
            InitializeDbContext();
        }

        public WarehouseService(bool disposeWhenDone)
        {
            _disposeWhenDone = disposeWhenDone;
            InitializeDbContext();
        }

        public void InitializeDbContext()
        {
            var iDbContext = DbContextUtil.GetDbContextInstance();
            _warehouseRepository = new Repository<WarehouseDTO>(iDbContext);
            _unitOfWork = new UnitOfWork(iDbContext);
        }

        #endregion

        #region Common Methods

        public IRepositoryQuery<WarehouseDTO> Get()
        {
            var piList = _warehouseRepository
                .Query()
                .Include(a => a.Address, a => a.FinancialAccounts, a => a.Organization)
                .Filter(a => !string.IsNullOrEmpty(a.DisplayName))
                .OrderBy(q => q.OrderBy(c => c.DisplayName));
            return piList;
        }

        public IEnumerable<WarehouseDTO> GetAll(SearchCriteria<WarehouseDTO> criteria = null)
        {
            IEnumerable<WarehouseDTO> piList;// = new List<BusinessPartnerDTO>();
            try
            {
                if (criteria != null)
                {
                    var pdto = Get();

                    foreach (var cri in criteria.FiList)
                    {
                        pdto.FilterList(cri);
                    }

                    IList<WarehouseDTO> pdtoList;
                    if (criteria.Page != 0 && criteria.PageSize != 0)
                    {
                        int totalCount;
                        pdtoList = pdto.GetPage(criteria.Page, criteria.PageSize, out totalCount).ToList();
                    }
                    else
                        pdtoList = pdto.GetList().ToList();

                    piList = pdtoList.ToList();

                }
                else
                {
                    piList = Get().Get().OrderBy(i => i.Id).ToList();
                }

            }
            finally
            {
                Dispose(_disposeWhenDone);
            }
            return piList;
        }
        
        public WarehouseDTO Find(string warehouseId)
        {
            var orgDto = _warehouseRepository.FindById(Convert.ToInt32(warehouseId));
            if (_disposeWhenDone)
                Dispose();
            return orgDto;
        }

        public WarehouseDTO GetByName(string displayName)
        {
            var cat = _warehouseRepository.Query().Filter(c => c.DisplayName == displayName).Get().FirstOrDefault();
            return cat;
        }

        public string InsertOrUpdate(WarehouseDTO warehouse)
        {
            try
            {
                var validate = Validate(warehouse);
                if (!string.IsNullOrEmpty(validate))
                    return validate;

                if (ObjectExists(warehouse))
                    return GenericMessages.DatabaseErrorRecordAlreadyExists;

                _warehouseRepository.InsertUpdate(warehouse);
                _unitOfWork.Commit();
                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public string Disable(WarehouseDTO warehouse)
        {
            if (warehouse == null)
                return GenericMessages.ObjectIsNull;

            string stat;
            try
            {
                _warehouseRepository.Update(warehouse);
                _unitOfWork.Commit();
                stat = string.Empty;
            }
            catch (Exception exception)
            {
                stat = exception.Message;
            }
            return stat;
        }

        public int Delete(string warehouseId)
        {
            try
            {
                _warehouseRepository.Delete(warehouseId);
                _unitOfWork.Commit();
                return 0;
            }
            catch (Exception exception)
            {
                return -1;
            }
        }

        public bool ObjectExists(WarehouseDTO warehouse)
        {
            var objectExists = false;
            var iDbContext = DbContextUtil.GetDbContextInstance();
            try
            {
                var catRepository = new Repository<WarehouseDTO>(iDbContext);
                var catExists = catRepository.Query()
                    .Filter(bp => bp.DisplayName == warehouse.DisplayName && bp.Id != warehouse.Id).Get()
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

        public string Validate(WarehouseDTO warehouse)
        {
            if (null == warehouse)
                return GenericMessages.ObjectIsNull;

            if (warehouse.Address == null)
                return "Address " + GenericMessages.ObjectIsNull;

            if (warehouse.OrganizationId == 0 || new OrganizationService(true).Find(warehouse.OrganizationId.ToString()) == null)
                return "Organization is null/disabled ";

            if (String.IsNullOrEmpty(warehouse.DisplayName))
                return warehouse.DisplayName + " " + GenericMessages.StringIsNullOrEmpty;

            if (warehouse.DisplayName.Length > 255)
                return warehouse.DisplayName + " can not be more than 255 characters ";

            return string.Empty;
        }

        #endregion

        public IEnumerable<WarehouseDTO> GetWarehousesPrevilegedToUser(int userId)
        {
            IEnumerable<WarehouseDTO> warehouseList = new List<WarehouseDTO>();

            //var user = new UserService(true).Find(userId.ToString());

            //if (user != null)
            //{
            //    warehouseList = GetAll();

            //    if (user.WarehouseId != null)
            //        warehouseList = warehouseList.Where(w => w.Id == user.WarehouseId).ToList();

            //    if (user.OrganizationId != null)
            //        warehouseList = warehouseList.Where(w => w.OrganizationId == user.Organization.Id).ToList();
            //}

            warehouseList = GetAll();
            return warehouseList.ToList();
        }

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