using System.ComponentModel.DataAnnotations;
using AMStock.Core.Models;

namespace AMStock.WPF.Models
{
    public class ImportModel : EntityBase
    {
        [Required]
        public string FileName
        {
            get { return GetValue(() => FileName); }
            set { SetValue(() => FileName, value); }
        }
        [Required]
        public string SheetName
        {
            get { return GetValue(() => SheetName); }
            set { SetValue(() => SheetName, value); }
        }
    }
}