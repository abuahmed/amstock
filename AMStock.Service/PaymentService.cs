using System;
using System.Collections.Generic;
using System.Linq;
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
    public class PaymentService : IPaymentService
    {
        #region Fields

        private IDbContext _iDbContext;
        private IUnitOfWork _unitOfWork;
        private IRepository<PaymentDTO> _paymentRepository;
        private IRepository<BusinessPartnerDTO> _businessPartnerRepository;
        private IRepository<TransactionHeaderDTO> _transactionRepository;
        private IRepository<ItemQuantityDTO> _itemsQuantityRepository;
        private readonly bool _disposeWhenDone;
        #endregion

        #region Constructor
        public PaymentService()
        {
            InitializeDbContext();
        }
        public PaymentService(IDbContext iDbContext)
        {
            _iDbContext = iDbContext;
            _unitOfWork = new UnitOfWork(_iDbContext);
            _paymentRepository = _unitOfWork.Repository<PaymentDTO>();
            _businessPartnerRepository = _unitOfWork.Repository<BusinessPartnerDTO>();
            _transactionRepository = _unitOfWork.Repository<TransactionHeaderDTO>();
            _itemsQuantityRepository = _unitOfWork.Repository<ItemQuantityDTO>();
        }
        public PaymentService(bool disposeWhenDone)
        {
            _disposeWhenDone = disposeWhenDone;
            InitializeDbContext();
        }

        public void InitializeDbContext()
        {
            _iDbContext = DbContextUtil.GetDbContextInstance();
            _unitOfWork = new UnitOfWork(_iDbContext);
            _paymentRepository = _unitOfWork.Repository<PaymentDTO>();
            _businessPartnerRepository = _unitOfWork.Repository<BusinessPartnerDTO>();
            _transactionRepository = _unitOfWork.Repository<TransactionHeaderDTO>();
            _itemsQuantityRepository = _unitOfWork.Repository<ItemQuantityDTO>();
        }

        #endregion

        #region Common Methods
        public IRepositoryQuery<PaymentDTO> Get()
        {
            var piList = _paymentRepository
                .Query()
                .Include(c => c.Warehouse,
                         c => c.Transaction,
                         c => c.Transaction.BusinessPartner,
                         c => c.Transaction.Payments,
                         s => s.Transaction.TransactionLines,
                         c => c.Check,
                         c => c.Check.CustomerBankAccount,
                         c => c.Clearance)
                .OrderBy(q => q.OrderBy(c => c.Id));
            return piList;

            //c => c.Clearance.DepositedBy,
            //c => c.Clearance.ClearedBy
        }

        public IEnumerable<PaymentDTO> GetAll(SearchCriteria<PaymentDTO> criteria = null)
        {
            IEnumerable<PaymentDTO> piList = new List<PaymentDTO>();
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
                            pdto.FilterList(p => p.PaymentDate >= beginDate);
                        }

                        if (criteria.EndingDate != null)
                        {
                            var endDate = new DateTime(criteria.EndingDate.Value.Year, criteria.EndingDate.Value.Month,
                                criteria.EndingDate.Value.Day, 23, 59, 59);
                            pdto.FilterList(p => p.PaymentDate <= endDate);
                        }

                        #endregion

                        #region By Transaction Type
                        if (criteria.TransactionType != -1)
                        {
                            switch ((TransactionTypes)criteria.TransactionType)
                            {
                                case TransactionTypes.Sale:
                                    {
                                        pdto.FilterList(p => p.PaymentType == PaymentTypes.Sale);
                                    }
                                    break;
                                case TransactionTypes.Purchase:
                                    {
                                        pdto.FilterList(p => p.PaymentType == PaymentTypes.Purchase);
                                    }
                                    break;
                            }
                        }
                        #endregion

                        #region By Payment List Types
                        if (criteria.PaymentListType != -1)
                        {
                            switch ((PaymentListTypes)criteria.PaymentListType)
                            {
                                case PaymentListTypes.All:
                                    break;
                                case PaymentListTypes.Cleared:
                                    pdto.FilterList(p => p.Status == PaymentStatus.Cleared);
                                    break;
                                case PaymentListTypes.NotCleared:
                                    pdto.FilterList(p => p.Status == PaymentStatus.NotCleared);
                                    break;
                                case PaymentListTypes.NotClearedandOverdue:
                                    pdto.FilterList(p => p.Status == PaymentStatus.NotCleared && (p.DueDate != null && p.DueDate > DateTime.Now));
                                    break;
                                case PaymentListTypes.NotDeposited:
                                    pdto.FilterList(p => p.Status == PaymentStatus.NotDeposited && p.PaymentMethod == PaymentMethods.Cash);
                                    break;
                                case PaymentListTypes.DepositedNotCleared:
                                    pdto.FilterList(p => p.Status == PaymentStatus.NotCleared && p.PaymentMethod == PaymentMethods.Cash);
                                    break;
                                case PaymentListTypes.DepositedCleared:
                                    pdto.FilterList(p => p.Status == PaymentStatus.Cleared && p.PaymentMethod == PaymentMethods.Cash);
                                    break;
                                case PaymentListTypes.CreditNotCleared:
                                    pdto.FilterList(p => p.Status == PaymentStatus.NotCleared && p.PaymentMethod == PaymentMethods.Credit);
                                    break;
                                case PaymentListTypes.CheckNotCleared:
                                    pdto.FilterList(p => p.Status == PaymentStatus.NotCleared && p.PaymentMethod == PaymentMethods.Check);
                                    break;
                                case PaymentListTypes.CheckCleared:
                                    pdto.FilterList(p => p.Status == PaymentStatus.Cleared && p.PaymentMethod == PaymentMethods.Check);
                                    break;
                            }
                        }
                        #endregion

                        #region By Payment Method
                        if (criteria.PaymentMethodType != -1)
                        {
                            switch ((PaymentMethods)criteria.PaymentMethodType)
                            {
                                case PaymentMethods.Cash:
                                    pdto.FilterList(p => p.PaymentMethod == PaymentMethods.Cash);
                                    break;
                                case PaymentMethods.Credit:
                                    pdto.FilterList(p => p.PaymentMethod == PaymentMethods.Credit);
                                    break;
                                case PaymentMethods.Check:
                                    pdto.FilterList(p => p.PaymentMethod == PaymentMethods.Check);
                                    break;
                            }
                        }
                        #endregion

                        #region By Payment Type
                        if (criteria.PaymentType != -1)
                        {
                            switch (criteria.PaymentType)
                            {
                                case 2:
                                    pdto.FilterList(p => p.PaymentType == PaymentTypes.CashOut);
                                    break;
                                case 5:
                                    pdto.FilterList(p => p.PaymentType == PaymentTypes.CashIn);
                                    break;
                            }
                        }
                        #endregion

                        piList = piList.Concat(pdto.GetList().ToList());
                    }
                }
                else
                {
                    piList = Get().Get().ToList();
                }

            }
            finally
            {
                Dispose(_disposeWhenDone);
            }

            return piList;

        }

        public PaymentDTO Find(string paymentId)
        {
            var orgDto = _paymentRepository.FindById(Convert.ToInt32(paymentId));
            if (_disposeWhenDone)
                Dispose();
            return orgDto;
        }

        public PaymentDTO GetByName(string displayName)
        {
            var cat = _paymentRepository.Query().Filter(c => c.PersonName == displayName).Get().FirstOrDefault();
            return cat;
        }

        public string InsertOrUpdate(PaymentDTO payment)
        {
            try
            {
                var validate = Validate(payment);
                if (!string.IsNullOrEmpty(validate))
                    return validate;

                if (ObjectExists(payment))
                    return GenericMessages.DatabaseErrorRecordAlreadyExists + Environment.NewLine +
                           "With the same Name/Tin No. Exists";

                _paymentRepository.InsertUpdate(payment);
                _unitOfWork.Commit();
                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public string Disable(PaymentDTO payment)
        {

            if (payment == null)
                return GenericMessages.ObjectIsNull;

            string stat;
            try
            {
                _paymentRepository.Update(payment);
                _unitOfWork.Commit();
                stat = string.Empty;
            }
            catch (Exception exception)
            {
                stat = exception.Message;
            }
            return stat;
        }

        public int Delete(string paymentId)
        {
            try
            {
                _paymentRepository.Delete(paymentId);
                _unitOfWork.Commit();
                return 0;
            }
            catch (Exception exception)
            {
                return -1;
            }
        }

        public bool ObjectExists(PaymentDTO payment)
        {
            var objectExists = false;
            //var iDbContext = DbContextUtil.GetDbContextInstance();
            //try
            //{
            //    var catRepository = new Repository<PaymentDTO>(iDbContext);
            //    var catExists = catRepository.GetAll()
            //        .FirstOrDefault(bp => (bp.DisplayName == payment.DisplayName ||
            //        (!string.IsNullOrEmpty(bp.TinNumber) && bp.TinNumber == payment.TinNumber)) &&
            //        bp.Id != payment.Id);


            //    if (catExists != null)
            //        objectExists = true;
            //}
            //finally
            //{
            //    iDbContext.Dispose();
            //}

            return objectExists;
        }

        public string Validate(PaymentDTO payment)
        {
            if (null == payment)
                return GenericMessages.ObjectIsNull;

            if (payment.Warehouse == null)
                return "Warehouse " + GenericMessages.ObjectIsNull;

            if (String.IsNullOrEmpty(payment.Reason))
                return payment.Reason + " " + GenericMessages.StringIsNullOrEmpty;

            if (String.IsNullOrEmpty(payment.PersonName))
                return payment.PersonName + " " + GenericMessages.StringIsNullOrEmpty;

            return string.Empty;
        }

        #endregion

        #region Private Methods
        public string PostPayments(TransactionHeaderDTO transaction2, PaymentDTO payment2, PaymentModel paymentModel, CheckDTO selectedCheck)
        {
            string stat = string.Empty;
            try
            {
                var transaction =
                    _transactionRepository
                        .Query()
                        .Include(t => t.Warehouse, t => t.BusinessPartner, t => t.TransactionLines, t => t.Payments)
                        .Filter(t => t.Id == transaction2.Id)
                        .Get()
                        .FirstOrDefault();

                if (transaction == null) return "Empty transaction";

                var bp = _businessPartnerRepository
                    .Query()
                    .Filter(b => b.Id == transaction.BusinessPartnerId)
                    .Get()
                    .FirstOrDefault();
                if (bp == null) return "No Business Partner";

                PaymentDTO cashPayment = null;
                if (paymentModel.Amount > 0) //always cash payment
                {
                    #region Cash Payment

                    cashPayment = GetNewPayment(transaction, paymentModel.PaymentDate);
                    if (cashPayment != null)
                    {
                        cashPayment.PaymentMethod = PaymentMethods.Cash;
                        cashPayment.Status = PaymentStatus.NotDeposited;
                        cashPayment.Amount = paymentModel.Amount - paymentModel.Change;
                        if (!Singleton.Setting.HandleBankTransaction)
                            cashPayment.Status = PaymentStatus.Cleared;

                        _paymentRepository.Insert(cashPayment);

                        if (payment2 != null)
                        {
                            bp.TotalCredits = bp.TotalCredits - cashPayment.Amount;
                            if (payment2.Amount - cashPayment.Amount == 0)
                                bp.TotalNoOfCreditTransactions = bp.TotalNoOfCreditTransactions - 1;
                            _businessPartnerRepository.InsertUpdate(bp);
                        }
                    }

                    #endregion
                }

                //Commented because is always cash payment
                if (paymentModel.Amount < paymentModel.AmountRequired)
                {
                    if (selectedCheck != null && selectedCheck.CheckNumber.Length > 0)
                    {
                        #region Check Payment

                        var checkPayment = GetNewPayment(transaction, paymentModel.PaymentDate);
                        if (checkPayment != null)
                        {
                            checkPayment.Check = selectedCheck;
                            checkPayment.PaymentMethod = PaymentMethods.Check;
                            checkPayment.Amount = paymentModel.AmountRequired - paymentModel.Amount;
                            //piList.Add(checkPayment);
                            _paymentRepository.Insert(checkPayment);

                            if (payment2 == null)
                            {
                                bp.TotalCredits = bp.TotalCredits + checkPayment.Amount;
                                bp.TotalNoOfCreditTransactions = bp.TotalNoOfCreditTransactions + 1;
                                _businessPartnerRepository.InsertUpdate(bp);
                            }
                        }

                        #endregion
                    }
                    else if (transaction.BusinessPartner.AllowCreditsWithoutCheck)
                    {
                        #region Credit Payment

                        var creditPayment = GetNewPayment(transaction, paymentModel.PaymentDate);
                        if (creditPayment != null)
                        {
                            creditPayment.PaymentMethod = PaymentMethods.Credit;
                            creditPayment.DueDate = DateTime.Now.AddDays(transaction.BusinessPartner.PaymentTerm);
                            creditPayment.Amount = paymentModel.AmountRequired - paymentModel.Amount;
                            _paymentRepository.Insert(creditPayment);

                            if (payment2 == null)
                            {
                                bp.TotalCredits = bp.TotalCredits + creditPayment.Amount;
                                bp.TotalNoOfCreditTransactions = bp.TotalNoOfCreditTransactions + 1;
                                _businessPartnerRepository.InsertUpdate(bp);
                            }
                        }

                        #endregion
                    }
                    else
                    {
                        stat = "Got problem while getting check detail, Can't take credit or problem on check ..";
                        if (cashPayment != null)
                            _paymentRepository.Delete(cashPayment);
                    }
                }
                if (string.IsNullOrEmpty(stat))
                {
                    if (payment2 == null)
                    {
                        // Update Inventory
                        var iqList = new ItemQuantityService(_iDbContext).UpdateInventoryByTransaction(transaction);
                        foreach (var itemQuantityDTO in iqList)
                        {
                            _itemsQuantityRepository.InsertUpdate(itemQuantityDTO);
                        }

                        //Update transaction
                        transaction.Status = TransactionStatus.Posted;
                        _transactionRepository.InsertUpdate(transaction);

                        _unitOfWork.Commit();
                    }
                    else
                    {
                        var payment = _paymentRepository.FindById(payment2.Id);

                        payment.Enabled = false;
                        _paymentRepository.Update(payment);

                        _unitOfWork.Commit();
                    }
                }
            }
            catch (Exception exception)
            {
                stat = exception.Message;
            }
            finally
            {
                _iDbContext.Dispose();
            }
            return stat;
        }
        public PaymentDTO GetNewPayment(TransactionHeaderDTO selectedTransaction, DateTime paymentDate)
        {
            if (selectedTransaction != null)
            {
                return new PaymentDTO
                {
                    //PaymentMethod = PaymentMethods.Check,
                    Status = PaymentStatus.NotCleared,
                    PaymentDate = paymentDate,

                    TransactionId = selectedTransaction.Id,
                    WarehouseId = selectedTransaction.WarehouseId,
                    PersonName = selectedTransaction.BusinessPartnerId.ToString(),

                    PaymentType = selectedTransaction.TransactionType == TransactionTypes.Sale ? PaymentTypes.Sale : PaymentTypes.Purchase,
                    Reason = selectedTransaction.TransactionType.ToString() + "-" +
                             selectedTransaction.TransactionNumber + "-" +
                             selectedTransaction.TransactionDateString
                };
            }
            return null;
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