using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using AMStock.Core;
using AMStock.Core.Common;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.Service;
using AMStock.Service.Interfaces;
using AMStock.Web.Models;

namespace AMStock.Web.Controllers
{
    [Authorize]
    public class OnHandInventoryController : Controller
    {
        private static IItemQuantityService _itemQuantityService;

        public OnHandInventoryController()
        {
            CleanUp();
            _itemQuantityService = new ItemQuantityService();
        }

        public static void CleanUp()
        {
            if (_itemQuantityService != null)
                _itemQuantityService.Dispose();
        }

        public ActionResult Index(string searchText, int? page, int? pageSize, int? ptype, string categoryId, string filterByQuantity, string warehouseId)
        {
            try
            {
                FilterByQuantityList();
                var criteria = new SearchCriteria<ItemQuantityDTO>()
                {
                    CurrentUserId = Singleton.User.UserId
                };

                #region Search Text

                if (!string.IsNullOrEmpty(searchText))
                {
                    ViewData["ItemSearch"] = "search results for '<strong style='font-size:14px;'>" + searchText + "</strong>'";
                    ViewData["SearchText"] = searchText;
                    ViewData["ItemSearchHidden"] = "visible";
                }
                else
                {
                    ViewData["ItemSearch"] = string.Empty;
                    ViewData["SearchText"] = string.Empty;
                    ViewData["ItemSearchHidden"] = "hidden";
                }

                if (!string.IsNullOrEmpty(searchText))
                    criteria.FiList.Add(
                        i => i.Item.ItemCode.Contains(searchText) || i.Item.DisplayName.Contains(searchText));

                #endregion

                #region Filter By Warehouse

                int wareId;
                if (int.TryParse(warehouseId, out wareId) && wareId != 0 && wareId != -1)
                {
                    criteria.SelectedWarehouseId = wareId;
                    ViewData["WarehouseId"] = wareId;
                }
                else
                {
                    ViewData["WarehouseId"] = string.Empty;
                }

                #endregion

                #region Filter By Category

                int catId;
                if (int.TryParse(categoryId, out catId) && catId != 0 && catId != -1)
                {
                    ViewData["CategoryId"] = categoryId;
                    criteria.FiList.Add(i => i.Item.CategoryId == catId);
                }
                else
                {
                    ViewData["CategoryId"] = "0";
                }

                #endregion

                #region Filter By Quantity

                int filByQty;
                if (int.TryParse(categoryId, out filByQty) && filByQty != 0)
                {
                    ViewData["FilterByQuantity"] = filterByQuantity;

                    switch (filByQty)
                    {
                        case 1:
                            criteria.FiList.Add(i => i.QuantityOnHand == 0);
                            break;
                        case 2:
                            criteria.FiList.Add(i => i.QuantityOnHand > 0);
                            break;
                        case 3:
                            criteria.FiList.Add(i => i.QuantityOnHand < i.Item.SafeQuantity);
                            break;
                        case 4:
                            criteria.FiList.Add(i => i.QuantityOnHand >= i.Item.SafeQuantity);
                            break;
                    }
                }
                else
                {
                    ViewData["FilterByQuantity"] = "0";
                }

                #endregion

                #region Paging

                if (page != null && ptype != null && pageSize != null)
                {
                    criteria.Page = (int)(page + ptype);
                    criteria.PageSize = (int)pageSize;

                    if (criteria.Page < 1)
                        criteria.Page = 1;
                }
                else
                {
                    criteria.Page = 1;
                    criteria.PageSize = 25;
                }

                if (criteria.Page == 1)
                    ViewData["prevDisabled"] = "none";
                ViewData["Page"] = criteria.Page;
                ViewData["PageSize"] = criteria.PageSize;

                #endregion

                var totalCount = 0;
                var itemQts = _itemQuantityService.GetAll(criteria, out totalCount).ToList();
                @ViewData["totalNumber"] = totalCount;

                #region Paging

                var pages = HelperUtility.GetPages(totalCount, criteria.PageSize);
                ViewData["totalPages"] = pages;
                if (pages == 0)
                {
                    criteria.Page = 0;
                    ViewData["Page"] = 0;
                }
                if (criteria.Page == pages)
                    ViewData["nextDisabled"] = "none";

                #endregion

                #region For Serial Number

                var sNo = (criteria.Page - 1) * criteria.PageSize + 1;
                foreach (var businessPartnerDTO in itemQts)
                {
                    businessPartnerDTO.SerialNumber = sNo;
                    sNo++;
                }

                #endregion

                if (Request.IsAjaxRequest())
                {
                    //Thread.Sleep(3000);
                    return PartialView("_onHandInventory", itemQts);
                }
                return View(itemQts);
            }
            catch
            {
                return HttpNotFound();
            }
        }

        public ActionResult GetItemTransactions(int? page, int? pageSize, int? ptype, int transactionType, int itemId)
        {
            return PartialView("PartialTemplates/ItemTransactionHistory", GetTransactionLines(page, pageSize, ptype, (TransactionTypes)transactionType, itemId));
        }

        public ActionResult Details(int itemId, string warehouseId)
        {
            var wareId = Convert.ToInt32(warehouseId);
            var criteria = new SearchCriteria<ItemQuantityDTO>()
            {
                CurrentUserId = Singleton.User.UserId
            };

            criteria.FiList.Add(a => a.ItemId == itemId && a.WarehouseId == wareId);

            var itemQts = _itemQuantityService.GetAll(criteria).ToList();

            var itemQty = itemQts.FirstOrDefault();

            if (itemQty == null)
            {
                return HttpNotFound();
            }
            ViewData["ItemId"] = itemQty.ItemId;

            ViewData["SalesLines"] = GetTransactionLines(1, 20, null, TransactionTypes.Sale, itemQty.ItemId);

            ViewData["PurchaseLines"] = GetTransactionLines(1, 20, null, TransactionTypes.Purchase, itemQty.ItemId);

            return View(itemQty);
        }

        public TransactionLineHeader GetTransactionLines(int? page, int? pageSize, int? ptype, TransactionTypes transactionType, int itemId)
        {
            ViewData["ItemId"] = itemId;
            ViewData["bpType" + Convert.ToInt32(transactionType).ToString()] = transactionType == TransactionTypes.Sale ? "Customer" : "Supplier";
            var criteria = new SearchCriteria<TransactionLineDTO>()
            {
                CurrentUserId = Singleton.User.UserId,
                BeginingDate = DateTime.Now.AddMonths(-12),
                EndingDate = DateTime.Now
            };

            criteria.FiList.Add(sa => sa.Transaction.TransactionType == transactionType &&
                                 sa.Transaction.Status != TransactionStatus.Order &&
                                 sa.ItemId == itemId);

            #region Paging

            if (page != null && ptype != null && pageSize != null)
            {
                criteria.Page = (int)(page + ptype);
                criteria.PageSize = (int)pageSize;

                if (criteria.Page < 1)
                    criteria.Page = 1;
            }
            else
            {
                criteria.Page = 1;
                criteria.PageSize = 25;
            }

            if (criteria.Page == 1)
                ViewData["prevDisabled" + Convert.ToInt32(transactionType).ToString()] = "none";
            ViewData["Page" + Convert.ToInt32(transactionType).ToString()] = criteria.Page;
            ViewData["PageSize" + Convert.ToInt32(transactionType).ToString()] = criteria.PageSize;

            #endregion

            int totalCount;
            var transactionpLinesList = new TransactionService().GetAllChilds(criteria, out totalCount).ToList();

            @ViewData["totalNumber" + Convert.ToInt32(transactionType).ToString()] = totalCount;

            #region Paging

            var pages = HelperUtility.GetPages(totalCount, criteria.PageSize);
            ViewData["totalPages" + Convert.ToInt32(transactionType).ToString()] = pages;
            if (pages == 0)
            {
                criteria.Page = 0;
                ViewData["Page" + Convert.ToInt32(transactionType).ToString()] = 0;
            }
            if (criteria.Page == pages)
                ViewData["nextDisabled" + Convert.ToInt32(transactionType).ToString()] = "none";

            #endregion

            var linesTransactionList = transactionpLinesList.Select(salesLineDto => new TransactionLineDetail
            {
                TransactionDate = salesLineDto.Transaction.TransactionDate,
                TransactionId = salesLineDto.Transaction.Id,
                TransactionNumber = salesLineDto.Transaction.TransactionNumber,
                DisplayName = salesLineDto.Transaction.BusinessPartner.DisplayName,
                BusinessPartnerId = salesLineDto.Transaction.BusinessPartner.Id,
                ItemCode = salesLineDto.Item.ItemCode,
                ItemDisplayName = salesLineDto.Item.DisplayName,
                Unit = salesLineDto.Unit,
                EachPrice = salesLineDto.EachPrice,
                LinePrice = salesLineDto.LinePrice,
                PurchasePrice = salesLineDto.Item.PurchasePrice,
                WarehouseName = salesLineDto.Transaction.Warehouse.DisplayName
            }).ToList();

            var sNo = (criteria.Page - 1) * criteria.PageSize + 1;
            foreach (var transactionLineDTO in linesTransactionList)
            {
                transactionLineDTO.SerialNumber = sNo;
                sNo++;
            }
            var linesTransactionListHeader = new TransactionLineHeader()
            {
                TransactionType = (int)transactionType,
                TransactionLineDetails = linesTransactionList
            };
            return linesTransactionListHeader;
        }

        public void FilterByQuantityList()
        {
            var quantityFilterList = new List<SelectListItem>
            {
                new SelectListItem {Value = "0", Text = "All"},
                new SelectListItem {Value = "1", Text = "Zero Quantity"},
                new SelectListItem {Value = "2", Text = "Above Zero Quantity"},
                new SelectListItem {Value = "3", Text = "Below Safe Quantity"},
                new SelectListItem {Value = "4", Text = "Above Safe Quantity"}
            };
            var cats = new SelectList(quantityFilterList, "value", "text");
            @ViewData["QuantityFilter"] = cats;
        }

        #region Reservations
        public ActionResult ReserveItem(int itemId, string warehouseId)
        {
            var wareId = Convert.ToInt32(warehouseId);
            var criteria = new SearchCriteria<ItemQuantityDTO>()
            {
                CurrentUserId = Singleton.User.UserId
            };

            criteria.FiList.Add(a => a.ItemId == itemId && a.WarehouseId == wareId);

            var itemQts = _itemQuantityService.GetAll(criteria).ToList();

            var itemQty = itemQts.FirstOrDefault();

            if (itemQty == null)
            {
                return HttpNotFound();
            }
            return PartialView(itemQty);
        }
        public JsonResult SaveItemReservation(int iqId, int quantityReserved, int soQuantityReserved, string onDate)
        {
            if (ModelState.IsValid)
            {
                var criteria = new SearchCriteria<ItemQuantityDTO>()
                {
                    CurrentUserId = Singleton.User.UserId
                };
                criteria.FiList.Add(b => b.Id == iqId);
                var iq = _itemQuantityService.GetAll(criteria).FirstOrDefault();
                if (iq != null)
                {
                    var prevreserved = iq.QuantityReserved;
                    iq.QuantityReserved = quantityReserved;
                    iq.ReservedOnDate = Convert.ToDateTime(onDate);

                    if (iq.QuantityReserved > iq.QuantityOnHand)
                        iq.QuantityReserved = iq.QuantityOnHand;
                    iq.QuantityOnHand = iq.QuantityOnHand + (prevreserved - quantityReserved);

                    var stat = _itemQuantityService.InsertOrUpdate(iq, false);

                    if (string.IsNullOrEmpty(stat))
                        return Json(true);
                    else
                        return Json(false);

                }
                return Json(false);
            }
            return Json(false);
        }
        #endregion

        public ActionResult _ItemSalesHistory(int itemId)
        {
            var transactions = GetTransactionLines(null, null, null, TransactionTypes.Sale, itemId);

            //IList<TransactionLineHeader> dailySales = new List<TransactionLineHeader>();

            //foreach (var tran in transactions.TransactionLineDetails)
            //{
            //    dailySales.Add(new TransactionViewModel(tran.TransactionDate.ToShortDateString(), tran.CountLines, tran.TotalCost));
            //}

            return Json(transactions.TransactionLineDetails);
        }
    }
}
