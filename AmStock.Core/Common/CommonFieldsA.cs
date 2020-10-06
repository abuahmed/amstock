using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using AMStock.Core.Models;

namespace AMStock.Core
{
    public class CommonFieldsA : EntityBase
    {
        [NotMapped]
        [DisplayName("No.")]
        public int SerialNumber { get; set; }
    }
}
