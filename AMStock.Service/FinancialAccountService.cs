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
    public class FinancialAccountService : IFinancialAccountService
    {
        #region Fields
        private IUnitOfWork _unitOfWork;
        private IRepository<FinancialAccountDTO> _financialAccountRepository;
        private readonly bool _disposeWhenDone;
        #endregion

        #region Constructor

        public FinancialAccountService()
        {
            InitializeDbContext();
        }
        public FinancialAccountService(bool disposeWhenDone)
        {
            _disposeWhenDone = disposeWhenDone;
            InitializeDbContext();
        }
        public void InitializeDbContext()
        {
            var iDbContext = DbContextUtil.GetDbContextInstance();
            _financialAccountRepository = new Repository<FinancialAccountDTO>(iDbContext);
            _unitOfWork = new UnitOfWork(iDbContext);
        }

        #endregion

        #region Common Methods
        public IRepositoryQuery<FinancialAccountDTO> Get()
        {
            var piList = _financialAccountRepository
                .Query()
                .Include(a => a.Warehouse)
                .Filter(a => !string.IsNullOrEmpty(a.AccountNumber))
                .OrderBy(q => q.OrderBy(c => c.Id));
            return piList;
        }
        
        public IEnumerable<FinancialAccountDTO> GetAll(SearchCriteria<FinancialAccountDTO> criteria = null)
        {
            IEnumerable<FinancialAccountDTO> accountList = new List<FinancialAccountDTO>();
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
                        
                        IList<FinancialAccountDTO> pdtoList;
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
        
        public FinancialAccountDTO Find(string financialAccountId)
        {
            return _financialAccountRepository.FindById(Convert.ToInt32(financialAccountId));
        }

        public FinancialAccountDTO GetByName(string accountNumber)
        {
            var cat = _financialAccountRepository
                .Query()
                .Filter(c => c.AccountNumber == accountNumber)
                .Get().FirstOrDefault();
            return cat;
        }

        public string InsertOrUpdate(FinancialAccountDTO financialAccount)
        {
            try
            {
                var validate = Validate(financialAccount);
                if (!string.IsNullOrEmpty(validate))
                    return validate;

                if (ObjectExists(financialAccount))
                    return GenericMessages.DatabaseErrorRecordAlreadyExists;

                _financialAccountRepository.InsertUpdate(financialAccount);
                _unitOfWork.Commit();
                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public string Disable(FinancialAccountDTO financialAccount)
        {
            if (financialAccount == null)
                return GenericMessages.ObjectIsNull;

            string stat;
            try
            {
                _financialAccountRepository.Update(financialAccount);
                _unitOfWork.Commit();
                stat = string.Empty;
            }
            catch (Exception exception)
            {
                stat = exception.Message;
            }
            return stat;
        }

        public int Delete(string financialAccountId)
        {
            try
            {
                _financialAccountRepository.Delete(financialAccountId);
                _unitOfWork.Commit();
                return 0;
            }
            catch (Exception exception)
            {
                return -1;
            }
        }

        public bool ObjectExists(FinancialAccountDTO financialAccount)
        {
            var objectExists = false;
            var iDbContext = DbContextUtil.GetDbContextInstance();
            try
            {
                var catRepository = new Repository<FinancialAccountDTO>(iDbContext);
                var catExists = catRepository
                    .Query()
                    .Filter(bp => bp.BankName == financialAccount.BankName && bp.AccountNumber == financialAccount.AccountNumber && bp.Id != financialAccount.Id)
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

        public string Validate(FinancialAccountDTO financialAccount)
        {
            if (null == financialAccount)
                return GenericMessages.ObjectIsNull;

            if (financialAccount.WarehouseId != 0 && new WarehouseService(true).Find(financialAccount.WarehouseId.ToString()) == null)
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