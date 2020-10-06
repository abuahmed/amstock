using System;
using System.ComponentModel.DataAnnotations;

using System.Management;
using System.ComponentModel.DataAnnotations.Schema;
using AMStock.Core.Enumerations;

namespace AMStock.Core.Models
{
    public class ProductActivationDTO : CommonFieldsA
    {
        public ProductActivationDTO()
        {
            BiosSn = Get_BIOS_SN();
        }

        [Required]
        [StringLength(150)]
        public string ProductKey
        {
            get { return GetValue(() => ProductKey); }
            set { SetValue(() => ProductKey, value); }
        }

        [Required]
        [StringLength(150)]
        public string LicensedTo
        {
            get { return GetValue(() => LicensedTo); }
            set { SetValue(() => LicensedTo, value); }
        }

        [Required]
        public int WarehouseId
        {
            get { return GetValue(() => WarehouseId); }
            set { SetValue(() => WarehouseId, value); }
        }
        
        public AMStockEdition Edition
        {
            get { return GetValue(() => Edition); }
            set { SetValue(() => Edition, value); }
        }

        [Required]
        public DateTime ActivatedDate
        {
            get { return GetValue(() => ActivatedDate); }
            set { SetValue(() => ActivatedDate, value); }
        }

        [Required]
        public DateTime ExpirationDate
        {
            get { return GetValue(() => ExpirationDate); }
            set { SetValue(() => ExpirationDate, value); }
        }

        [Required]
        [StringLength(150)]
        public string RegisteredBiosSn
        {
            get { return GetValue(() => RegisteredBiosSn); }
            set { SetValue(() => RegisteredBiosSn, value); }
        }

        [Required]
        [NotMapped]
        public string BiosSn
        {
            get { return GetValue(() => BiosSn); }
            set { SetValue(() => BiosSn, value); }
        }

        /// <summary>
        /// It discovers the BIOS serial number
        /// </summary>
        public static string Get_BIOS_SN()
        {
            var biossn = string.Empty;

            //return Environment.MachineName;
            var searcher = new ManagementObjectSearcher("select SerialNumber from WIN32_BIOS");
            var result = searcher.Get();

            foreach (var o in result)
            {
                var obj = (ManagementObject)o;
                if (obj["SerialNumber"] != null)
                    biossn = obj["SerialNumber"].ToString();
            }

            result.Dispose();
            searcher.Dispose();

            return biossn;

        }
    }
}
