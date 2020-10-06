using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AMStock.Core;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.DAL;
using AMStock.DAL.Interfaces;
using AMStock.Repository;
using AMStock.Repository.Interfaces;
using AMStock.Service.Interfaces;

namespace AMStock.Service
{
    public class BusinessPartnerService : IBusinessPartnerService
    {
        #region Fields
        private IUnitOfWork _unitOfWork;
        private IRepository<BusinessPartnerDTO> _businessPartnerRepository;
        private readonly bool _disposeWhenDone;
        private IDbContext _iDbContext;
        #endregion

        #region Constructor
        public BusinessPartnerService()
        {
            InitializeDbContext();
        }

        public BusinessPartnerService(bool disposeWhenDone)
        {
            _disposeWhenDone = disposeWhenDone;
            InitializeDbContext();
        }

        public BusinessPartnerService(IDbContext iDbContext)
        {
            InitializeDbContext(iDbContext);
        }

        public void InitializeDbContext(IDbContext iDbContext = null)
        {
            _iDbContext = iDbContext;
            if (iDbContext == null)
                _iDbContext = DbContextUtil.GetDbContextInstance();
            _businessPartnerRepository = new Repository<BusinessPartnerDTO>(_iDbContext);
            _unitOfWork = new UnitOfWork(_iDbContext);
        }

        #endregion

        #region Common Methods
        public IRepositoryQuery<BusinessPartnerDTO> Get()
        {//, a => a.TransactionHeaders
            var piList = _businessPartnerRepository
                .Query()
                .Include(a => a.Addresses, a => a.FinancialAccounts)
                .Filter(a => !string.IsNullOrEmpty(a.DisplayName))
                .OrderBy(q => q.OrderBy(c => c.DisplayName).ThenBy(c => c.Code));
            return piList;
        }

        public IEnumerable<BusinessPartnerDTO> GetAll(SearchCriteria<BusinessPartnerDTO> criteria = null)
        {
            int totalCount = 0;
            return this.GetAll(criteria, out totalCount);
        }

        public IEnumerable<BusinessPartnerDTO> GetAll(SearchCriteria<BusinessPartnerDTO> criteria, out int totalCount)//, 
        {
            totalCount = 0;
            IEnumerable<BusinessPartnerDTO> piList = new List<BusinessPartnerDTO>();
            try
            {
                if (criteria != null)
                {
                    var pdto = Get();

                    foreach (var cri in criteria.FiList)
                    {
                        pdto.FilterList(cri);
                    }

                    #region By Transaction Type
                    if (criteria.TransactionType != -1)
                        switch ((TransactionTypes)criteria.TransactionType)
                        {
                            case TransactionTypes.Sale:
                                pdto.FilterList(bp => bp.BusinessPartnerType == BusinessPartnerTypes.Customer);
                                break;
                            case TransactionTypes.Purchase:
                                pdto.FilterList(bp => bp.BusinessPartnerType == BusinessPartnerTypes.Supplier);
                                break;
                        }
                    #endregion

                    IList<BusinessPartnerDTO> pdtoList;
                    if (criteria.Page != 0 && criteria.PageSize != 0 && criteria.PaymentListType == -1)
                    {
                        int totalCount2;
                        pdtoList = pdto.GetPage(criteria.Page, criteria.PageSize, out totalCount2).ToList();
                        totalCount = totalCount2;
                    }
                    else
                    {
                        pdtoList = pdto.GetList().ToList();
                        totalCount = pdtoList.Count;
                    }

                    piList = pdtoList.ToList();
                }
                else
                    piList = Get().Get().OrderBy(i => i.Id).ToList();


                //#region For Eager Loading Childs
                //foreach (var businessPartnerDTO in piList)
                //{
                //    var transactionHeaders = (ICollection<TransactionHeaderDTO>)new TransactionService(_iDbContext)
                //        .GetAll(new SearchCriteria<TransactionHeaderDTO>
                //        {
                //            BusinessPartnerId = businessPartnerDTO.Id,
                //            CurrentUserId = Singleton.User.UserId
                //        });
                //}
                //#endregion

                #region By Payment List Types (Since TotalCredit is n't in the table can't use Linq)


                if (criteria != null && criteria.PaymentListType != -1)
                {
                    var b = piList.ToList().Count;
                    switch (criteria.PaymentListType)
                    {
                        case 1:
                            piList = piList.Where(s => s.TotalCredit > 0).ToList();
                            break;
                        case 2:
                            piList = piList.Where(s => s.TotalCredit == 0).ToList();
                            break;

                    }
                    var c = piList.ToList().Count;
                    totalCount = totalCount - (b - c);

                    if (criteria.Page != 0 && criteria.PageSize != 0)
                    {
                        piList = piList.Skip(criteria.PageSize * (criteria.Page - 1)).Take(criteria.PageSize).ToList();
                    }
                }
                #endregion
            }
            finally
            {
                Dispose(_disposeWhenDone);
            }
            return piList;
        }

        public BusinessPartnerDTO Find(string businessPartnerId)
        {
            var bpId = Convert.ToInt32(businessPartnerId);
            var bpDto = Get().Filter(b => b.Id == bpId).Get().FirstOrDefault();
            if (_disposeWhenDone)
                Dispose();
            return bpDto;
        }

        public BusinessPartnerDTO GetByName(string displayName)
        {
            var bp = Get()
                .Filter(b => b.DisplayName == displayName)
                .Get()
                .FirstOrDefault();
            return bp;
        }

        public string InsertOrUpdate(BusinessPartnerDTO businessPartner)
        {
            try
            {
                var validate = Validate(businessPartner);
                if (!string.IsNullOrEmpty(validate))
                    return validate;

                if (ObjectExists(businessPartner))
                    return GenericMessages.DatabaseErrorRecordAlreadyExists + Environment.NewLine +
                           "with the same Name/Tin No. Exists";

                _businessPartnerRepository.InsertUpdate(businessPartner);
                _unitOfWork.Commit();

                //Generate Business Partner Code
                if (string.IsNullOrEmpty(businessPartner.Code))
                    return GetBusinessPartnerCode(businessPartner);

                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public string Disable(BusinessPartnerDTO businessPartner)
        {
            if (_unitOfWork.Repository<TransactionHeaderDTO>().Query().Get().Any(i => i.BusinessPartnerId == businessPartner.Id) ||
                _unitOfWork.Repository<FinancialAccountDTO>().Query().Get().Any(i => i.BusinessPartnerId == businessPartner.Id))
            {
                return "Can't delete the item, it is already in use...";
            }

            if (businessPartner == null)
                return GenericMessages.ObjectIsNull;

            string stat;
            try
            {
                _businessPartnerRepository.Update(businessPartner);
                _unitOfWork.Commit();
                stat = string.Empty;
            }
            catch (Exception exception)
            {
                stat = exception.Message;
            }
            return stat;
        }

        public int Delete(string businessPartnerId)
        {
            try
            {
                _businessPartnerRepository.Delete(businessPartnerId);
                _unitOfWork.Commit();
                return 0;
            }
            catch (Exception exception)
            {
                return -1;
            }
        }

        public bool ObjectExists(BusinessPartnerDTO businessPartner)
        {
            var objectExists = false;
            var iDbContext = DbContextUtil.GetDbContextInstance();
            try
            {
                var catRepository = new Repository<BusinessPartnerDTO>(iDbContext);
                var catExists = catRepository.Query()
                    .Filter(bp => (bp.DisplayName == businessPartner.DisplayName ||
                    (!string.IsNullOrEmpty(bp.TinNumber) && bp.TinNumber == businessPartner.TinNumber)) &&
                    bp.Id != businessPartner.Id)
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

        public string Validate(BusinessPartnerDTO businessPartner)
        {
            if (null == businessPartner)
                return GenericMessages.ObjectIsNull;

            if (String.IsNullOrEmpty(businessPartner.DisplayName))
                return businessPartner.DisplayName + " " + GenericMessages.StringIsNullOrEmpty;

            if (businessPartner.DisplayName.Length > 255)
                return businessPartner.DisplayName + " can not be more than 255 characters ";

            if (!string.IsNullOrEmpty(businessPartner.Code) && businessPartner.Code.Length > 50)
                return businessPartner.Code + " can not be more than 50 characters ";

            return string.Empty;
        }

        #endregion

        #region Asynchronous Methods
        public Task<IEnumerable<BusinessPartnerDTO>> GetAsync()
        {
            return _businessPartnerRepository.Query().GetAsync();
        }
        public Task<IEnumerable<BusinessPartnerDTO>> FindAsync(string id)
        {
            return Get().Filter(b => b.Id == Convert.ToInt32(id)).GetAsync();
            //return _businessPartnerRepository.Get.FirstIncludingChildsAsync(Convert.ToInt32(id), a => a.Address, a => a.FinancialAccounts);
        }
        public async Task<string> InsertOrUpdateAync(BusinessPartnerDTO businessPartner)
        {
            try
            {
                var validate = Validate(businessPartner);
                if (!string.IsNullOrEmpty(validate))
                    return validate;

                if (ObjectExists(businessPartner))
                    return GenericMessages.DatabaseErrorRecordAlreadyExists + Environment.NewLine +
                           "With the same Name/Tin No. Exists";

                _businessPartnerRepository.InsertUpdate(businessPartner);
                await _unitOfWork.CommitAync();
                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
        #endregion

        #region Private Methods
        public string GetBusinessPartnerCode(BusinessPartnerDTO businessPartner)
        {
            var bpCode = "";
            try
            {
                var prefix = businessPartner.BusinessPartnerType.ToString().Substring(0, 1);
                var code = 10000 + businessPartner.Id;
                businessPartner.Code = prefix + code.ToString(CultureInfo.InvariantCulture).Substring(1);
                _businessPartnerRepository.InsertUpdate(businessPartner);
                _unitOfWork.Commit();
            }
            catch (Exception exception)
            {
                bpCode = "Problem Getting Partner Number, try again..." + Environment.NewLine + exception.InnerException.Message;
            }
            return bpCode;
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