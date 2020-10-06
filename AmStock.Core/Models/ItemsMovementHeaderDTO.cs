using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AMStock.Core.Enumerations;

namespace AMStock.Core.Models
{
    public class ItemsMovementHeaderDTO : CommonFieldsA
    {
        public ItemsMovementHeaderDTO()
        {
            MovementDate = DateTime.Now;
            ItemsMovementLines=new HashSet<ItemsMovementLineDTO>();
        }

        [Required]
        [DisplayName("SearchKey")]
        [MaxLength(30, ErrorMessage = "Exceeded 30 letters")]
        public string MovementNumber
        {
            get { return GetValue(() => MovementNumber); }
            set { SetValue(() => MovementNumber, value); }
        }

        [ForeignKey("FromWarehouse")]
        public int FromWarehouseId { get; set; }
        public WarehouseDTO FromWarehouse
        {
            get { return GetValue(() => FromWarehouse); }
            set { SetValue(() => FromWarehouse, value); }
        }

        [ForeignKey("ToWarehouse")]
        public int ToWarehouseId { get; set; }
        public WarehouseDTO ToWarehouse
        {
            get { return GetValue(() => ToWarehouse); }
            set { SetValue(() => ToWarehouse, value); }
        }

        [DisplayName("On Date")]
        public DateTime MovementDate
        {
            get { return GetValue(() => MovementDate); }
            set { SetValue(() => MovementDate, value); }
        }

        [DisplayName("Movement Status")]
        public TransactionStatus Status
        {
            get { return GetValue(() => Status); }
            set { SetValue(() => Status, value); }
        }

        [NotMapped]
        public int CountLines
        {
            get
            {
                return ItemsMovementLines.Count(l => l.Enabled);
            }
            set
            {
                SetValue(() => CountLines, value);
            }
        }

        [NotMapped]
        public string MovementDateString
        {
            get
            {
                return MovementDate.ToString("dd-MM-yyyy");
            }
        }

        public ICollection<ItemsMovementLineDTO> ItemsMovementLines
        {
            get { return GetValue(() => ItemsMovementLines); }
            set
            {
                SetValue(() => ItemsMovementLines, value);
            }
        }
    }
}