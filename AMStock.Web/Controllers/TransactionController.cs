using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using AMStock.Core;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.Service;
using AMStock.Service.Interfaces;
using AMStock.Web.Models;

namespace AMStock.Web.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;
        private IEnumerable<TransactionHeaderDTO> listOfCharts;
        public TransactionController()
        {
            _transactionService = new TransactionService();
        }

        public ActionResult Index(string type, string searchText, int? page, int? pageSize, int? ptype,
            string transactionType, string paymentStatus, string warehouseId,
            string beginDate, string endDate)
        {
            try
            {
                FilterLists();
                var tranType = (TransactionTypes)Convert.ToInt32(type);
                ViewData["TransactionType"] = tranType.ToString();
                ViewData["TranType"] = type;
                ViewData["BussinessPartnerType"] = tranType == TransactionTypes.Sale ? "Customer" : "Supplier";
                ViewData["BPType"] = tranType == TransactionTypes.Sale ? "1" : "2";

                var criteria = new SearchCriteria<TransactionHeaderDTO>()
                {
                    CurrentUserId = Singleton.User.UserId
                };

                criteria.FiList.Add(p => p.TransactionType == tranType);

                #region Filter By Duration
                if (!string.IsNullOrEmpty(beginDate))
                {
                    DateTime begDate;
                    if (DateTime.TryParse(beginDate, out begDate))
                        criteria.BeginingDate = begDate;
                    ViewData["BeginDate"] = beginDate;
                }
                else
                {
                    ViewData["BeginDate"] = "0";
                }

                if (!string.IsNullOrEmpty(endDate))
                {
                    DateTime enDate;
                    if (DateTime.TryParse(endDate, out enDate))
                        criteria.EndingDate = enDate;
                    ViewData["EndDate"] = endDate;
                }
                else
                {
                    ViewData["EndDate"] = "0";
                }
                #endregion

                #region Search Text
                if (!string.IsNullOrEmpty(searchText))
                {
                    ViewData["TransactionSearch"] = "search results for '" + searchText + "'";
                    ViewData["SearchText"] = searchText;
                }
                else
                {
                    ViewData["TransactionSearch"] = string.Empty;
                    ViewData["SearchText"] = string.Empty;
                }

                if (!string.IsNullOrEmpty(searchText))
                    criteria.FiList.Add(bp => bp.TransactionNumber.Contains(searchText) ||
                                        bp.BusinessPartner.DisplayName.Contains(searchText) ||
                                        bp.BusinessPartner.TinNumber.Contains(searchText));
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

                #region Filter By Status
                int tranStatus;
                if (int.TryParse(transactionType, out tranStatus) && tranStatus != 0 && tranStatus != -1)
                {
                    ViewData["TransactionStat"] = transactionType;
                    var tranStat = (TransactionStatus)tranStatus;
                    criteria.FiList.Add(i => i.Status == tranStat);
                }
                else
                {
                    ViewData["TransactionStat"] = "0";
                }
                #endregion

                #region Filter By Payment Status
                int paymStatus;
                if (int.TryParse(paymentStatus, out paymStatus) && paymStatus != 0 && paymStatus != -1)
                {
                    ViewData["PaymentStat"] = transactionType;
                    criteria.PaymentListType = paymStatus;
                }
                else
                {
                    ViewData["PaymentStat"] = "0";
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

                int totalCount;
                var transactions = _transactionService.GetAll(criteria, out totalCount);
                @ViewData["totalNumber"] = totalCount;
                
                listOfCharts = transactions.ToList();

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
                foreach (var transactionDTO in transactions)
                {
                    transactionDTO.SerialNumber = sNo;
                    sNo++;
                }
                #endregion

                
                if (Request.IsAjaxRequest())
                {
                    //Thread.Sleep(3000);
                    return PartialView("_transaction", transactions);
                }
                return View(transactions);
            }
            catch
            {
                return HttpNotFound("Problem getting transactions list");
            }
        }

        public ActionResult Details(int id, string type)
        {
            ViewData["TranType"] = type;
            ViewData["BusinessPartnerType"] = type == "1" ? "Customer" : "Supplier";
            var criteria = new SearchCriteria<TransactionHeaderDTO>()
            {
                CurrentUserId = Singleton.User.UserId
            };
            criteria.FiList.Add(a => a.Id == id);
            var itemQts = _transactionService.GetAll(criteria).ToList();


            var transaction = itemQts.FirstOrDefault();

            if (transaction == null)
            {
                return HttpNotFound();
            }

            #region Transaction Lines


            var salesCriteria = new SearchCriteria<TransactionLineDTO>()
            {
                CurrentUserId = Singleton.User.UserId,
                BeginingDate = DateTime.Now.AddMonths(-12),
                EndingDate = DateTime.Now
            };
            salesCriteria.FiList.Add(sa => sa.TransactionId == transaction.Id);
            int totalCount;
            var tranLines = _transactionService.GetAllChilds(salesCriteria, out totalCount);// transaction.TransactionLines.ToList();

            var sNo = 1;
            foreach (var transactionDTO in tranLines)
            {
                transactionDTO.SerialNumber = sNo;
                sNo++;
            }
            ViewData["TranLines"] = tranLines;
            #endregion

            #region Payment Lines
            sNo = 1;
            var paymentLines = transaction.Payments.ToList();
            foreach (var paymentDTO in paymentLines)
            {
                paymentDTO.SerialNumber = sNo;
                sNo++;
            }
            ViewData["PaymentLines"] = paymentLines;
            #endregion

            return View(transaction);
        }

        public void FilterLists()
        {
            var transactionStatusFilterList = new List<SelectListItem>
            {
                new SelectListItem {Value = "-1", Text = "All"},
                new SelectListItem {Value = Convert.ToInt32(TransactionStatus.Order).ToString(), Text = TransactionStatus.Order.ToString()},
                new SelectListItem {Value = Convert.ToInt32(TransactionStatus.Posted).ToString(), Text = TransactionStatus.Posted.ToString()}
            };
            var cats = new SelectList(transactionStatusFilterList, "value", "text");
            @ViewData["TransactionStatus"] = cats;

            var paymentStatusFilterList = new List<SelectListItem>
            {
                new SelectListItem {Value = "-1", Text = "All"},
                new SelectListItem {Value = "2", Text = PaymentListTypes.NotCleared.ToString()},
                new SelectListItem {Value = "3", Text = PaymentListTypes.Cleared.ToString()}
            };
            var ptypes = new SelectList(paymentStatusFilterList, "value", "text");
            @ViewData["PaymentType"] = ptypes;
        }

        public ActionResult _DailySales()
        {
            var criteria = new SearchCriteria<TransactionHeaderDTO>()
            {
                CurrentUserId = Singleton.User.UserId
            };

            criteria.FiList.Add(p => p.TransactionType == TransactionTypes.Sale);
            int totalCount;
            var transactions = _transactionService.GetAll(criteria, out totalCount);

            IList<TransactionViewModel> dailySales = new List<TransactionViewModel>();

            foreach (var tran in transactions)
            {
                var tranvm = new TransactionViewModel(tran.TransactionDate.ToShortDateString(), tran.CountLines,
                    tran.TotalCost);
                int cindex = tran.Id%2+1;
                tranvm.Color = resolutionColors[cindex];
                dailySales.Add(tranvm);
            }
            
            return Json(dailySales);
        }

        private Dictionary<int, string> resolutionColors = new Dictionary<int, string>() { 
            {1,"#ccc"},
            {2,"#c00"}
        };
    }
}
