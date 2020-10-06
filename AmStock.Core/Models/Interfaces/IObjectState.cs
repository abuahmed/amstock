using System.ComponentModel.DataAnnotations.Schema;


namespace AMStock.Core.Models.Interfaces
{
    public interface IObjectState
    {
        [NotMapped]
        ObjectState ObjectState { get; set; }
    }
}
