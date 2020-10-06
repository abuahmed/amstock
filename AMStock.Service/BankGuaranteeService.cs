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
    public class BankGuaranteeService : IBankGuaranteeService
    {
        #region Fields
        private IUnitOfWork _unitOfWork;
        private IRepository<BankGuaranteeDTO> _bankGuaranteeRepository;
        private readonly bool _disposeWhenDone;
        #endregion

        #region Constructor

        public BankGuaranteeService()
        {
            InitializeDbContext();
        }
        public BankGuaranteeService(bool disposeWhenDone)
        {
            _disposeWhenDone = disposeWhenDone;
            InitializeDbContext();
        }
        public void InitializeDbContext()
        {
            var iDbContext = DbContextUtil.GetDbContextInstance();
            _bankGuaranteeRepository = new Repository<BankGuaranteeDTO>(iDbContext);
            _unitOfWork = new UnitOfWork(iDbContext);
        }

        #endregion

        #region Common Methods
        public IRepositoryQuery<BankGuaranteeDTO> Get()
        {
            var piList = _bankGuaranteeRepository
                .Query()
                .Include(a => a.Warehouse)
                .Filter(a => !string.IsNullOrEmpty(a.AccountNumber))
                .OrderBy(q => q.OrderBy(c => c.Id));
            return piList;
        }

        public IEnumerable<BankGuaranteeDTO> GetAll(SearchCriteria<BankGuaranteeDTO> criteria = null)
        {
            IEnumerable<BankGuaranteeDTO> accountList = new List<BankGuaranteeDTO>();
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

                        IList<BankGuaranteeDTO> pdtoList;
                        if (criteria.Page != 0 && criteria.PageSize != 0)
                        {
                            int totalCount;
                            pdtoList = pdto.GetPage(criteria.Page, criteria.PageSize, out totalCount).ToList();
                        }
                        else
                            pdtoList = pdto.GetList().ToList();

                        accountList = accountList.Concat(pdtoList).ToList();
                    }
                }
                else
                    accountList = Get().Get().ToList();
            }
            finally
            {
                Dispose(_disposeWhenDone);
            }

            return accountList;
        }

        public BankGuaranteeDTO Find(string bankGuaranteeId)
        {
            return _bankGuaranteeRepository.FindById(Convert.ToInt32(bankGuaranteeId));
        }

        public BankGuaranteeDTO GetByName(string accountNumber)
        {
            var cat = _bankGuaranteeRepository
                .Query()
                .Filter(c => c.AccountNumber == accountNumber)
                .Get().FirstOrDefault();
            return cat;
        }

        public string InsertOrUpdate(BankGuaranteeDTO bankGuarantee)
        {
            try
            {
                var validate = Validate(bankGuarantee);
                if (!string.IsNullOrEmpty(validate))
                    return validate;

                if (ObjectExists(bankGuarantee))
                    return GenericMessages.DatabaseErrorRecordAlreadyExists;

                _bankGuaranteeRepository.InsertUpdate(bankGuarantee);
                _unitOfWork.Commit();
                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public string Disable(BankGuaranteeDTO bankGuarantee)
        {
            if (bankGuarantee == null)
                return GenericMessages.ObjectIsNull;

            string stat;
            try
            {
                _bankGuaranteeRepository.Update(bankGuarantee);
                _unitOfWork.Commit();
                stat = string.Empty;
            }
            catch (Exception exception)
            {
                stat = exception.Message;
            }
            return stat;
        }

        public int Delete(string bankGuaranteeId)
        {
            try
            {
                _bankGuaranteeRepository.Delete(bankGuaranteeId);
                _unitOfWork.Commit();
                return 0;
            }
            catch (Exception exception)
            {
                return -1;
            }
        }

        public bool ObjectExists(BankGuaranteeDTO bankGuarantee)
        {
            var objectExists = false;
            //var iDbContext = DbContextUtil.GetDbContextInstance();
            //try
            //{
            //    var catRepository = new Repository<BankGuaranteeDTO>(iDbContext);
            //    var catExists = catRepository
            //        .Query()
            //        .Filter(bp => bp.BankName == bankGuarantee.BankName && bp.AccountNumber == bankGuarantee.AccountNumber && bp.Id != bankGuarantee.Id)
            //        .Get()
            //        .FirstOrDefault();
            //    if (catExists != null)
            //        objectExists = true;
            //}
            //finally
            //{
            //    iDbContext.Dispose();
            //}

            return objectExists;
        }

        public string Validate(BankGuaranteeDTO bankGuarantee)
        {
            if (null == bankGuarantee)
                return GenericMessages.ObjectIsNull;

            if (bankGuarantee.GuaranteedAmount == null || (bankGuarantee.GuaranteedAmount != null && bankGuarantee.GuaranteedAmount<=0))
                return "Guaranteed Amount Can't be Empty/Zero";

            if (bankGuarantee.WarehouseId != 0 && new WarehouseService(true).Find(bankGuarantee.WarehouseId.ToString()) == null)
                return "Warehouse is null/disabled ";

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