using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using AMStock.Core.Enumerations;
using AMStock.Core.CustomValidationAttributes;
using AMStock.Core.Extensions;

namespace AMStock.Core.Models
{
    public class PhysicalInventoryHeaderDTO : CommonFieldsA
    {
        public PhysicalInventoryHeaderDTO()
        {
            PhysicalInventoryLines = new HashSet<PhysicalInventoryLineDTO>();
        }
        [Required]
        [MaxLength(50, ErrorMessage = "PhysicalInventoryNumber exceeded 50 letters")]
        [ExcludeChar("/.,!@#$%", ErrorMessage = "PhysicalInventoryNumber contains invalid letters")]
        public string PhysicalInventoryNumber
        {
            get { return GetValue(() => PhysicalInventoryNumber); }
            set { SetValue(() => PhysicalInventoryNumber, value); }
        }

        [ForeignKey("Warehouse")]
        public int WarehouseId { get; set; }
        public WarehouseDTO Warehouse
        {
            get { return GetValue(() => Warehouse); }
            set { SetValue(() => Warehouse, value); }
        }

        public DateTime PhysicalInventoryDate
        {
            get { return GetValue(() => PhysicalInventoryDate); }
            set { SetValue(() => PhysicalInventoryDate, value); }
        }

        public PhysicalInventoryStatus Status
        {
            get { return GetValue(() => Status); }
            set { SetValue(() => Status, value); }
        }

        [NotMapped]
        public int CountLines
        {
            get
            {
                return PhysicalInventoryLines.Count(l => l.Enabled);
            }
            set
            {
                SetValue(() => CountLines, value);
            }
        }
        [NotMapped]
        public bool ShowLines
        {
            get
            {
                return PhysicalInventoryLines.Count(li => li.PhysicalInventoryLineType == PhysicalInventoryLineTypes.AfterPi && li.Enabled) > 0;
            }
            set
            {
                SetValue(() => ShowLines, value);
            }
        }

        //[NotMapped]
        //public string PiType
        //{
        //    get
        //    {
        //        return EnumUtil.GetEnumDesc()
        //    }
        //    set
        //    {
        //        SetValue(() => PiType, value);
        //    }
        //}

        [NotMapped]
        public string PhysicalInventoryDateString
        {
            get
            {
                return PhysicalInventoryDate.ToString("dd-MM-yyyy");
            }
        }

        public ICollection<PhysicalInventoryLineDTO> PhysicalInventoryLines
        {
            get { return GetValue(() => PhysicalInventoryLines); }
            set
            {
                SetValue(() => PhysicalInventoryLines, value);
            }
        }
    }
}
