using System;
using System.Collections.Generic;
using AMStock.Core;
using AMStock.Core.Models;

namespace AMStock.Service.Interfaces
{
    public interface IPaymentService : IDisposable
    {
        IEnumerable<PaymentDTO> GetAll(SearchCriteria<PaymentDTO> criteria = null);
        PaymentDTO Find(string paymentId);
        PaymentDTO GetByName(string displayName);
        string InsertOrUpdate(PaymentDTO payment);
        string Disable(PaymentDTO payment);
        int Delete(string paymentId);

        string PostPayments(TransactionHeaderDTO tran, PaymentDTO payment,
            PaymentModel selectedPaymentModel, CheckDTO selectedCheck);
    }
}