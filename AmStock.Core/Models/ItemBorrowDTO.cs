using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AMStock.Core.Enumerations;

namespace AMStock.Core.Models
{
    public class ItemBorrowDTO : CommonFieldsA
    {
        public ItemBorrowTypes ItemBorrowType
        {
            get { return GetValue(() => ItemBorrowType); }
            set { SetValue(() => ItemBorrowType, value); }
        }

        [ForeignKey("Warehouse")]
        public int WarehouseId { get; set; }
        public WarehouseDTO Warehouse
        {
            get { return GetValue(() => Warehouse); }
            set { SetValue(() => Warehouse, value); }
        }

        [ForeignKey("Item")]
        public int ItemId { get; set; }
        public ItemDTO Item
        {
            get { return GetValue(() => Item); }
            set { SetValue(() => Item, value); }
        }

        [Required]
        public DateTime ItemBorrowDate
        {
            get { return GetValue(() => ItemBorrowDate); }
            set { SetValue(() => ItemBorrowDate, value); }
        }
        [Required]
        [MaxLength(255, ErrorMessage = "Exceeded 255 letters")]
        public string ShopName
        {
            get { return GetValue(() => ShopName); }
            set { SetValue(() => ShopName, value); }
        }
        [Required]
        [MaxLength(50, ErrorMessage = "Exceeded 50 letters")]
        public string PersonName
        {
            get { return GetValue(() => PersonName); }
            set { SetValue(() => PersonName, value); }
        }
        [Required]
        [Range(1,100000)]
        public int Quantity
        {
            get { return GetValue(() => Quantity); }
            set { SetValue(() => Quantity, value); }
        }
        public string Remarks
        {
            get { return GetValue(() => Remarks); }
            set { SetValue(() => Remarks, value); }
        }

        public int QuantityReturned
        {
            get { return GetValue(() => QuantityReturned); }
            set { SetValue(() => QuantityReturned, value); }
        }
        public DateTime? LastReturnedDate
        {
            get { return GetValue(() => LastReturnedDate); }
            set { SetValue(() => LastReturnedDate, value); }
        }
        public string ReturnRemarks
        {
            get { return GetValue(() => ReturnRemarks); }
            set { SetValue(() => ReturnRemarks, value); }
        }

        [NotMapped]
        public string ItemBorrowDateString
        {
            get
            {
                return ItemBorrowDate.ToString("dd-MM-yyyy");
            }
        }
        [NotMapped]
        public string ReturnCompleted
        {
            get
            {
                if (QuantityLeft == 0)
                {
                    return "Fully Returned";
                }
                else //if (QuantityReturned < Quantity)
                {
                    return "Not Returned";
                }
                //else
                //    return "Partially Returned";
            }
            set { SetValue(() => ReturnCompleted, value); }
        }
        [NotMapped]
        public int QuantityLeft
        {
            get { return Quantity - QuantityReturned; }
            set { SetValue(() => QuantityLeft, value); }
        }
        
    }
}