namespace AMStock.Web.Models
{
    public class TransactionViewModel
    {
        public TransactionViewModel()
        {

        }
        public TransactionViewModel(string date, int noofitems, decimal amount)
        {
            DateOf = date;
            NoOfItems = noofitems;
            Amount = amount;
        }

        public string DateOf { get; set; }
        public int NoOfItems { get; set; }
        public decimal Amount { get; set; }
        public string Color { get; set; }
    }
}