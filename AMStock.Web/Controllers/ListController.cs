using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AMStock.Core;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.Service;
using Kendo.Mvc.Extensions;

namespace AMStock.Web.Controllers
{
    public class ListController : Controller
    {
        public ActionResult GetWarehouses()
        {
            IEnumerable<WarehouseDTO> categories = Singleton.WarehousesList;
            IList<ListViewModel> wareVms = categories.Select(warehouseDTO => new ListViewModel()
            {
                Name = warehouseDTO.DisplayName,
                Id = warehouseDTO.Id
            }).ToList();

            if (wareVms.Count > 1)
            {
                wareVms.Insert(0, new ListViewModel()
                {
                    Name = "Show All",
                    Id = -1
                });
            }


            return Json(wareVms, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetBps(string bpType)
        {
            var businessPartnerType = (BusinessPartnerTypes)Convert.ToInt32(bpType);
            var criteria = new SearchCriteria<BusinessPartnerDTO>();
            criteria.FiList.Add(b => b.BusinessPartnerType == businessPartnerType);

            IEnumerable<BusinessPartnerDTO> bps = new BusinessPartnerService(true)
                .GetAll(criteria);
            IList<ListViewModel> bpVms = bps.Select(bpDTO => new ListViewModel()
            {
                Name = bpDTO.DisplayName,
                Id = bpDTO.Id
            }).ToList();

            return Json(bpVms, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetCategories(string catType)
        {
            var categoryType = (NameTypes)Convert.ToInt32(catType);
            var criteria = new SearchCriteria<CategoryDTO>();
            criteria.FiList.Add(b => b.NameType == categoryType);

            IList<CategoryDTO> categories = new List<CategoryDTO>();
            categories.AddRange(new CategoryService(true).GetAll(criteria));
            categories.Insert(0, new CategoryDTO()
            {
                Id = -1,
                DisplayName = "Show All"
            });

            return Json(categories, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetItems()
        {
            IEnumerable<ItemDTO> items = new ItemService(true).GetAll();
            IList<ListViewModel> wareVms = items.Select(itemDTO => new ListViewModel()
            {
                Name = itemDTO.ItemDetail,
                Id = itemDTO.Id
            }).ToList();

            return Json(wareVms, JsonRequestBehavior.AllowGet);
        }

    }
    public class ListViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
