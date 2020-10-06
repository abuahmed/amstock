using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace AMStock.Core.Models
{
    public class OrganizationDTO : CommonFieldsD
    {
        public OrganizationDTO()
        {
            Warehouses = new List<WarehouseDTO>();
        }

        [ForeignKey("Client")]
        public int ClientId { get; set; }
        public ClientDTO Client
        {
            get { return GetValue(() => Client); }
            set { SetValue(() => Client, value); }
        }

        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        [DisplayName("TIN Number")]
        [RegularExpression("^[0-9]{10}$", ErrorMessage = "Invalid Tax No, Must be 10 digit")]
        public string TinNumber
        {
            get { return GetValue(() => TinNumber); }
            set { SetValue(() => TinNumber, value); }
        }
        
        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        [DisplayName("VAT Number")]
        [RegularExpression("^[0-9]{10}$", ErrorMessage = "Invalid VAT No, Must be 10 digit")]
        public string VatNumber
        {
            get { return GetValue(() => VatNumber); }
            set { SetValue(() => VatNumber, value); }
        }
        
        [NotMapped]
        public string NoOfWarehouses
        {
            get
            {
                return Warehouses.Count(l => l.Enabled).ToString();
            }
            set { SetValue(() => NoOfWarehouses, value); }
        }
        
        public IList<WarehouseDTO> Warehouses
        {
            get { return GetValue(() => Warehouses); }
            set { SetValue(() => Warehouses, value); }
        }
    }
}