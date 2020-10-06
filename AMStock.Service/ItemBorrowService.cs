using System;
using System.Collections.Generic;
using System.Linq;
using AMStock.Core;
using AMStock.Core.Models;
using AMStock.DAL;
using AMStock.Repository;
using AMStock.Repository.Interfaces;
using AMStock.Service.Interfaces;

namespace AMStock.Service
{
    public class ItemBorrowService : IItemBorrowService
    {
        #region Fields
        private IUnitOfWork _unitOfWork;
        private IRepository<ItemBorrowDTO> _itemBorrowRepository;
        private readonly bool _disposeWhenDone;
        #endregion

        #region Constructor

        public ItemBorrowService()
        {
            InitializeDbContext();
        }
        public ItemBorrowService(bool disposeWhenDone)
        {
            _disposeWhenDone = disposeWhenDone;
            InitializeDbContext();
        }
        public void InitializeDbContext()
        {
            var iDbContext = DbContextUtil.GetDbContextInstance();
            _itemBorrowRepository = new Repository<ItemBorrowDTO>(iDbContext);
            _unitOfWork = new UnitOfWork(iDbContext);
        }
        #endregion

        #region Common Methods
        public IRepositoryQuery<ItemBorrowDTO> Get()
        {
            var piList = _itemBorrowRepository
                .Query()
                .Include(i => i.Item, i => i.Item.Category, i => i.Item.UnitOfMeasure, i => i.Warehouse)
                .Filter(a => !string.IsNullOrEmpty(a.PersonName))
                .OrderBy(q => q.OrderBy(c => c.Id));
            return piList;
        }
        
        public IEnumerable<ItemBorrowDTO> GetAll(SearchCriteria<ItemBorrowDTO> criteria = null)
        {
            IEnumerable<ItemBorrowDTO> catList = new List<ItemBorrowDTO>();
            try
            {
                if (criteria != null && criteria.CurrentUserId != -1)
                {
                    var warehouseList = new WarehouseService(true)
                        .GetWarehousesPrevilegedToUser(criteria.CurrentUserId).ToList();
                    if (criteria.SelectedWarehouseId != null)
                        warehouseList = warehouseList.Where(w => w.Id == criteria.SelectedWarehouseId).ToList();

                    foreach (var warehouse in warehouseList.Where(w => w.Id != -1))
                    {
                        var pdto = Get();

                        foreach (var cri in criteria.FiList)
                        {
                            pdto.FilterList(cri);
                        }

                        #region By Warehouse
                        var warehouse1 = warehouse;
                        pdto.FilterList(p => p.WarehouseId == warehouse1.Id);
                        #endregion

                        #region By Duration

                        if (criteria.BeginingDate != null)
                        {
                            var beginDate = new DateTime(criteria.BeginingDate.Value.Year, criteria.BeginingDate.Value.Month,
                                criteria.BeginingDate.Value.Day, 0, 0, 0);
                            pdto.FilterList(p => p.ItemBorrowDate >= beginDate);
                        }

                        if (criteria.EndingDate != null)
                        {
                            var endDate = new DateTime(criteria.EndingDate.Value.Year, criteria.EndingDate.Value.Month,
                                criteria.EndingDate.Value.Day, 23, 59, 59);
                            pdto.FilterList(p => p.ItemBorrowDate <= endDate);
                        }


                        #endregion
                        
                        IList<ItemBorrowDTO> pdtoList;
                        if (criteria.Page != 0 && criteria.PageSize != 0)
                        {
                            int totalCount;
                            pdtoList = pdto.GetPage(criteria.Page, criteria.PageSize, out totalCount).ToList();
                        }
                        else
                            pdtoList = pdto.GetList().ToList();

                        catList = catList.Concat(pdtoList).ToList();
                    }
                }
                else
                    catList = Get().Get().ToList();
            }
            finally
            {
                Dispose(_disposeWhenDone);
            }

            return catList;
        }

        public ItemBorrowDTO Find(string itemBorrowId)
        {
            return _itemBorrowRepository.FindById(Convert.ToInt32(itemBorrowId));
        }

        public ItemBorrowDTO GetByName(string displayName)
        {
            var cat = _itemBorrowRepository
                .Query()
                .Filter(c => c.PersonName == displayName)
                .Get().FirstOrDefault();
            return cat;
        }

        public string InsertOrUpdate(ItemBorrowDTO itemBorrow)
        {
            try
            {
                var validate = Validate(itemBorrow);
                if (!string.IsNullOrEmpty(validate))
                    return validate;

                if (ObjectExists(itemBorrow))
                    return GenericMessages.DatabaseErrorRecordAlreadyExists;

                _itemBorrowRepository.InsertUpdate(itemBorrow);
                _unitOfWork.Commit();
                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public string Disable(ItemBorrowDTO itemBorrow)
        {
            if (itemBorrow == null)
                return GenericMessages.ObjectIsNull;

            string stat;
            try
            {

                _itemBorrowRepository.Update(itemBorrow);
                _unitOfWork.Commit();
                stat = string.Empty;

            }
            catch (Exception exception)
            {
                stat = exception.Message;
            }
            return stat;
        }

        public int Delete(string itemBorrowId)
        {
            try
            {
                _itemBorrowRepository.Delete(itemBorrowId);
                _unitOfWork.Commit();
                return 0;
            }
            catch (Exception exception)
            {
                return -1;
            }
        }

        public bool ObjectExists(ItemBorrowDTO itemBorrow)
        {
            return false;
        }

        public string Validate(ItemBorrowDTO itemBorrow)
        {
            if (null == itemBorrow)
                return GenericMessages.ObjectIsNull;

            if (String.IsNullOrEmpty(itemBorrow.PersonName))
                return itemBorrow.PersonName + " " + GenericMessages.StringIsNullOrEmpty;

            if (String.IsNullOrEmpty(itemBorrow.ShopName))
                return itemBorrow.ShopName + " " + GenericMessages.StringIsNullOrEmpty;

            if (itemBorrow.Quantity < 1 || itemBorrow.Quantity > 100000)
                return itemBorrow.Quantity + " Quantity is above allowed limit";
            return string.Empty;
        }
        #endregion

        #region Disposing
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWork.Dispose();
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}