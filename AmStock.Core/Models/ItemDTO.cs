using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using AMStock.Core.CustomValidationAttributes;
using AMStock.Core.Enumerations;

namespace AMStock.Core.Models
{
    public class ItemDTO : CommonFieldsB2
    {
        public ItemDTO()
        {
            ItemQuantities = new HashSet<ItemQuantityDTO>();
            PhysicalInventoryLines = new HashSet<PhysicalInventoryLineDTO>();
            TransactionLines = new HashSet<TransactionLineDTO>();
            ItemsMovementLines = new HashSet<ItemsMovementLineDTO>();
        }

        //[ExcludeChar("/.,!@#$%", ErrorMessage = "Item Code contains invalid letters")]
        //[Unqiue(ErrorMessage = "There Exists Item with the same Code Number!")]
        [Index(IsUnique = true)]
        [Required, Display(Name = "Item Code"), MaxLength(50, ErrorMessage = "Item Code exceeded 50 letters")]
        public string ItemCode
        {
            get { return GetValue(() => ItemCode); }
            set { SetValue(() => ItemCode, value); }
        }

        [ForeignKey("Category")]
        public int? CategoryId
        {
            get { return GetValue(() => CategoryId); }
            set { SetValue(() => CategoryId, value); }
        }
        public CategoryDTO Category
        {
            get { return GetValue(() => Category); }
            set { SetValue(() => Category, value); }
        }

        [ForeignKey("UnitOfMeasure")]
        public int? UnitOfMeasureId
        {
            get { return GetValue(() => UnitOfMeasureId); }
            set { SetValue(() => UnitOfMeasureId, value); }
        }
        public CategoryDTO UnitOfMeasure
        {
            get { return GetValue(() => UnitOfMeasure); }
            set { SetValue(() => UnitOfMeasure, value); }
        }
        
        //Will have sum current quantity of all stores
        public decimal TotalCurrentQty 
        {
            get { return ItemQuantities.Sum(it => it.QuantityOnHand); }
            set { SetValue(() => TotalCurrentQty, value); }
        }

        [DisplayName("Purchase Price")]
        public decimal PurchasePrice
        {
            get { return GetValue(() => PurchasePrice); }
            set { SetValue(() => PurchasePrice, value); }
        }

        [Display(Name = "Sale Price")]
        public decimal SellPrice
        {
            get { return GetValue(() => SellPrice); }
            set { SetValue(() => SellPrice, value); }
        }

        public int? SafeQuantity
        {
            get { return GetValue(() => SafeQuantity); }
            set { SetValue(() => SafeQuantity, value); }
        }
        public int? WarningQuantity
        {
            get { return GetValue(() => WarningQuantity); }
            set { SetValue(() => WarningQuantity, value); }
        }
        public float? Discount
        {
            get { return GetValue(() => Discount); }
            set { SetValue(() => Discount, value); }
        }

        public byte? ItemImage
        {
            get { return GetValue(() => ItemImage); }
            set { SetValue(() => ItemImage, value); }
        }

        public ItemTypes ItemType
        {
            get { return GetValue(() => ItemType); }
            set { SetValue(() => ItemType, value); }
        }

        [NotMapped]
        public string ItemDetail
        {
            get
            {
                var it = ItemCode;// + " | " + DisplayName;
                if (!String.IsNullOrEmpty(DisplayName))
                    it = it + " | " + DisplayName;
                if (Category != null && !Category.DisplayName.ToLower().Contains("no"))
                {
                    it = it + " | " + Category.DisplayName;
                }
                if (!string.IsNullOrEmpty(Description))
                    it = it + " | " + Description;
                return it;

            }
            set { SetValue(() => ItemDetail, value); }
        }
        [NotMapped]
        public string CategoryString
        {
            get
            {
                return Category != null ? Category.DisplayName : "";
            }
            set { SetValue(() => CategoryString, value); }
        }
        [NotMapped]
        public string UomString
        {
            get
            {
                return UnitOfMeasure != null ? UnitOfMeasure.DisplayName : "";
            }
            set { SetValue(() => UomString, value); }
        }
        [NotMapped]
        public decimal TotalPhysicalInventoryLines
        {
            get { return PhysicalInventoryLines.Sum(l => l.CountedQty); }
            set { SetValue(() => TotalPhysicalInventoryLines, value); }
        }

        public ICollection<ItemQuantityDTO> ItemQuantities
        {
            get { return GetValue(() => ItemQuantities); }
            set { SetValue(() => ItemQuantities, value); }
        }
        public ICollection<PhysicalInventoryLineDTO> PhysicalInventoryLines
        {
            get { return GetValue(() => PhysicalInventoryLines); }
            set { SetValue(() => PhysicalInventoryLines, value); }
        }
        public ICollection<TransactionLineDTO> TransactionLines
        {
            get { return GetValue(() => TransactionLines); }
            set { SetValue(() => TransactionLines, value); }
        }
        public ICollection<ItemsMovementLineDTO> ItemsMovementLines
        {
            get { return GetValue(() => ItemsMovementLines); }
            set { SetValue(() => ItemsMovementLines, value); }
        }
        public ICollection<ItemBorrowDTO> ItemBorrows
        {
            get { return GetValue(() => ItemBorrows); }
            set { SetValue(() => ItemBorrows, value); }
        }
    }
}
