using System.ComponentModel.DataAnnotations.Schema;

namespace AMStock.Core.Models
{
    public class TransactionLineDTO : TransactionLine
    {
        [ForeignKey("Transaction")]
        public int TransactionId { get; set; }
        public TransactionHeaderDTO Transaction
        {
            get { return GetValue(() => Transaction); }
            set { SetValue(() => Transaction, value); }
        }
    }
}
