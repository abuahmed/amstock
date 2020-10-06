using System.ComponentModel.DataAnnotations;

//using AMStock.Core.Repository;
//using AMStock.Core.Repository.Interfaces;

namespace AMStock.Core.CustomValidationAttributes
{
    public class Unqiue : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //var contains= CustomerViewModel.SharedViewModel().Customers.Select(x => x.Id).Contains(int.Parse(value.ToString()));

            //if (contains)
            //    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            //else
                return ValidationResult.Success;
        }
    }
    public class UniqueCustomer : ValidationAttribute
    {
        string id ="0";
        public UniqueCustomer(string _id)
        {
            id = _id;
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //var unitOfWork = ServiceLocator.Current.GetInstance<IUnitOfWork>();
            //IUnitOfWork unitOfWork = new UnitOfWork(new AMStock.DAL.DbContextFactory().Create());
            //string val = value.ToString();
            //CustomerDTO customer = unitOfWork.Repository<CustomerDTO>().GetAll().Where(c => c.CustomerCode == val).FirstOrDefault();
            ////var contains= CustomerViewModel.SharedViewModel().Customers.Select(x => x.Id).Contains(int.Parse(value.ToString()));

            //if (customer!=null)
            //    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            //else
            return ValidationResult.Success;
        }
    } 
}
