using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
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
    public class BusinessPartnerController : Controller
    {
        private readonly IBusinessPartnerService _businessPartnerService;

        public BusinessPartnerController()
        {
            _businessPartnerService = new BusinessPartnerService();
        }

        public ActionResult Index(string type, string searchText, int? page, int? pageSize, int? ptype, string creditStatus, string warehouseId)
        {
            try
            {
                FilterByCredit();
                var bpType = (BusinessPartnerTypes)Convert.ToInt32(type);
                ViewData["BussinessPartnerType"] = bpType.ToString();
                ViewData["BPType"] = type;

                var criteria = new SearchCriteria<BusinessPartnerDTO>();

                criteria.FiList.Add(b => b.BusinessPartnerType == bpType);

                #region Search Text
                if (!string.IsNullOrEmpty(searchText))
                {
                    ViewData["BusinessPartnerSearch"] = "search results for '" + searchText + "'";
                    ViewData["SearchText"] = searchText;
                }
                else
                {
                    ViewData["BusinessPartnerSearch"] = string.Empty;
                    ViewData["SearchText"] = string.Empty;
                }

                if (!string.IsNullOrEmpty(searchText))
                    criteria.FiList.Add(bp => bp.Code.Contains(searchText) || bp.DisplayName.Contains(searchText));
                #endregion

                #region Filter By Credit Status
                int credStatus;
                if (int.TryParse(creditStatus, out credStatus) && credStatus != 0 && credStatus != -1)
                {
                    ViewData["CreditStat"] = creditStatus;
                    criteria.PaymentListType = credStatus;
                }
                else
                {
                    ViewData["CreditStat"] = "-1";
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
                var bps = _businessPartnerService.GetAll(criteria, out totalCount);
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
                foreach (var businessPartnerDTO in bps)
                {
                    businessPartnerDTO.SerialNumber = sNo;
                    sNo++;
                }
                #endregion

                if (Request.IsAjaxRequest())
                {
                    //Thread.Sleep(3000);
                    return PartialView("_businessPartner", bps);
                }
                return View(bps);
            }
            catch
            {
                return HttpNotFound("Problem getting transactions list");
            }
        }

        //var quantityFilterList = Singleton.WarehousesList.Select(warehouseDTO => new SelectListItem
        //             {
        //                 Value = warehouseDTO.Id.ToString(),
        //                 Text = warehouseDTO.DisplayName
        //             }).ToList();

        //             var cats = new SelectList(quantityFilterList, "value", "text");
        //             ViewData["WarehouseList"] = cats;
        //
        // GET: /BusinessPartner/Details/5
        //public async Task<ActionResult> Details(int id)
        //{
        //    var businessPartner = await _businessPartnerService.FindAsync(id.ToString());
        //    if (businessPartner == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(businessPartner.FirstOrDefault());
        //}

        public ActionResult Details(int id, string type)
        {
            ViewData["BPType"] = type;
            var criteria = new SearchCriteria<BusinessPartnerDTO>()
            {
                CurrentUserId = Singleton.User.UserId
            };
            criteria.FiList.Add(a => a.Id == id);
            var bps = _businessPartnerService.GetAll(criteria).ToList();

            var businessPartner = bps.FirstOrDefault();
            if (businessPartner == null)
            {
                return HttpNotFound();
            }
            return View(businessPartner);
        }

        public ActionResult CreditLimit(int id)
        {
            var criteria = new SearchCriteria<BusinessPartnerDTO>()
           {
               CurrentUserId = Singleton.User.UserId
           };
            criteria.FiList.Add(a => a.Id == id);
            var bps = _businessPartnerService.GetAll(criteria).ToList();

            var businessPartner = bps.FirstOrDefault();
            if (businessPartner == null)
            {
                return HttpNotFound();
            }
            return PartialView(businessPartner);
        }

        public JsonResult SaveCreditLimit(int bpId, decimal creditLimit, int maxNoCreditTransactions)
        {
            //Thread.Sleep(3000);
            if (ModelState.IsValid)
            {
                var criteria = new SearchCriteria<BusinessPartnerDTO>()
                {
                    CurrentUserId = Singleton.User.UserId
                };
                criteria.FiList.Add(b => b.Id == bpId);
                var bp = _businessPartnerService.GetAll(criteria).FirstOrDefault();
                if (bp != null)
                {
                    if (bp.Id == 1)
                        return Json(false);
                    bp.CreditLimit = creditLimit;
                    bp.MaxNoCreditTransactions = maxNoCreditTransactions;
                    var stat = _businessPartnerService.InsertOrUpdate(bp);
                    if (string.IsNullOrEmpty(stat))
                        return Json(true);
                    //return Json(new { result = "Successfully updated the credit limit!", id = bp.Id, amount = bp.CreditLimit, number = bp.MaxNoCreditTransactions });
                    else
                        return Json(false);

                }
                else
                    return Json(false);
            }
            return Json(false);
        }

        public void FilterByCredit()
        {
            var creditFilterList = new List<SelectListItem>
            {
                new SelectListItem {Value = "0", Text = "All"},
                new SelectListItem {Value = "1", Text = "With Credit"},
                new SelectListItem {Value = "2", Text = "No Credit"}
            };
            var credits = new SelectList(creditFilterList, "value", "text");
            @ViewData["CreditFilter"] = credits;
        }

    }
}
