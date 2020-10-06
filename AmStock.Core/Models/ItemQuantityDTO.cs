using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace AMStock.Core.Models
{
    public class ItemQuantityDTO : CommonFieldsA
    {
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

        [DisplayName("Qty. On Hand")]
        public decimal QuantityOnHand
        {
            get { return GetValue(() => QuantityOnHand); }
            set { SetValue(() => QuantityOnHand, value); }
        }
        [NotMapped]
        public string QuantityOnHandString
        {
            get { return QuantityOnHand.ToString("N2"); }
            set { SetValue(() => QuantityOnHandString, value); }
        }

        public decimal QuantityPurchased
        {
            get { return GetValue(() => QuantityPurchased); }
            set { SetValue(() => QuantityPurchased, value); }
        }
        [NotMapped]
        public string QuantityPurchasedString
        {
            get { return QuantityPurchased.ToString("N0"); }
            set { SetValue(() => QuantityPurchasedString, value); }
        }

        public decimal QuantitySold
        {
            get { return GetValue(() => QuantitySold); }
            set { SetValue(() => QuantitySold, value); }
        }
        [NotMapped]
        public string QuantitySoldString
        {
            get { return QuantitySold.ToString("N0"); }
            set { SetValue(() => QuantitySoldString, value); }
        }
        public decimal QuantityOnOrder
        {
            get { return GetValue(() => QuantityOnOrder); }
            set { SetValue(() => QuantityOnOrder, value); }
        }

        public decimal QuantityOnDelivery
        {
            get { return TotalGoodsMovementLines; }
            set { SetValue(() => QuantityOnDelivery, value); }
        }

        [Range(0, 10000)]
        public decimal QuantityReserved
        {
            get { return GetValue(() => QuantityReserved); }
            set { SetValue(() => QuantityReserved, value); }
        }

        public decimal QuantityAvailable
        {
            get
            {
                //if (QuantityOnOrder == null)
                //    QuantityOnOrder = 0;
                //if (QuantityReserved == null)
                //    QuantityReserved = 0;
                return QuantityOnHand + QuantityReserved;
            }
            set { SetValue(() => QuantityAvailable, value); }
        }

        [Range(0, 10000)]
        public decimal MaxCustomerQuantity
        {
            get { return GetValue(() => MaxCustomerQuantity); }
            set { SetValue(() => MaxCustomerQuantity, value); }
        }

        public DateTime? ReservedOnDate
        {
            get { return GetValue(() => ReservedOnDate); }
            set { SetValue(() => ReservedOnDate, value); }
        }

        public int? ReservedByUserId
        {
            get { return GetValue(() => ReservedByUserId); }
            set { SetValue(() => ReservedByUserId, value); }
        }

        [NotMapped]
        public decimal Difference
        {
            get
            {
                if (Item != null && Item.SafeQuantity != null) return QuantityOnHand - (int)Item.SafeQuantity;
                return -1;
            }
            set { SetValue(() => Difference, value); }

        }

        [NotMapped, DisplayName("Total Price")]
        public decimal TotalPrice
        {
            get
            {
                if (Item != null) return QuantityOnHand * Item.SellPrice;
                return 0;
            }
            set { SetValue(() => TotalPrice, value); }
        }

        [NotMapped, DisplayName("Total Price")]
        public string TotalPriceString
        {
            get { return TotalPrice.ToString("N"); }
            set { SetValue(() => TotalPriceString, value); }
        }

        [NotMapped]
        public decimal TotalPurchasePrice
        {
            get
            {
                if (Item != null) return QuantityOnHand * Item.PurchasePrice;
                return 0;
            }
            set { SetValue(() => TotalPurchasePrice, value); }
        }
        [NotMapped, DisplayName("Total Purchase")]
        public string TotalPurchaseString
        {
            get { return TotalPurchasePrice.ToString("N"); }
            set { SetValue(() => TotalPurchaseString, value); }
        }
        [NotMapped]
        public decimal TotalPhysicalInventoryLines
        {
            get
            {
                if (Item != null)
                    return Item.PhysicalInventoryLines
                        .Where(w => w.PhysicalInventory != null && w.PhysicalInventory.Warehouse == Warehouse)
                        .Sum(l => l.CountedQty);
                return 0;
            }
            set { SetValue(() => TotalPhysicalInventoryLines, value); }
        }

        [NotMapped]
        public int TotalGoodsMovementLines
        {
            get
            {
                //return 0;//
                var sum = 0;
                //foreach (var w in Item.GoodsMovementLines)
                //{
                //  if (w.GoodsMovement.Warehouse != null && (w.GoodsMovement != null && w.GoodsMovement.Warehouse == Warehouse)) sum += w.QuantityLeft;
                //}
                return sum;
            }
            set { SetValue(() => TotalGoodsMovementLines, value); }
        }

    }
}
