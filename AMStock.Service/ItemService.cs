using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using AMStock.Core;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.DAL;
using AMStock.Repository;
using AMStock.Repository.Interfaces;
using AMStock.Service.Interfaces;

namespace AMStock.Service
{
    public class ItemService : IItemService
    {
        #region Fields
        private IUnitOfWork _unitOfWork;
        private IRepository<ItemDTO> _itemRepository;
        private readonly bool _disposeWhenDone;
        #endregion

        #region Constructor
        public ItemService()
        {
            InitializeDbContext();
        }

        public ItemService(bool disposeWhenDone)
        {
            _disposeWhenDone = disposeWhenDone;
            InitializeDbContext();
        }

        public void InitializeDbContext()
        {
            var iDbContext = DbContextUtil.GetDbContextInstance();
            _itemRepository = new Repository<ItemDTO>(iDbContext);
            _unitOfWork = new UnitOfWork(iDbContext);
        }

        #endregion

        #region Common Methods
        public IRepositoryQuery<ItemDTO> Get()
        {
            var piList = _itemRepository
              .Query()
              .Include(a => a.Category).Include(a => a.UnitOfMeasure)//.Include(a => a.ItemQuantities)
              //.Filter(a => !string.IsNullOrEmpty(a.DisplayName))
              .OrderBy(q => q.OrderBy(c => c.Id));
            return piList;
        }

        public IEnumerable<ItemDTO> GetAll(SearchCriteria<ItemDTO> criteria = null)
        {
            IEnumerable<ItemDTO> piList;// = new List<ItemDTO>();
            try
            {
                if (criteria != null)
                {
                    var pdto = Get();

                    foreach (var cri in criteria.FiList)
                    {
                        pdto.FilterList(cri);
                    }

                    IList<ItemDTO> pdtoList;
                    if (criteria.Page != 0 && criteria.PageSize != 0)
                    {
                        int totalCount;
                        pdtoList = pdto.GetPage(criteria.Page, criteria.PageSize, out totalCount).ToList();
                    }
                    else
                        pdtoList = pdto.GetList().ToList();

                    piList = pdtoList.ToList();

                }
                else
                {
                    piList = Get().Get().OrderBy(i => i.Id).ToList();
                }

            }
            finally
            {
                Dispose(_disposeWhenDone);
            }
            return piList;
        }

        public ItemDTO Find(string itemId)
        {
            var orgDto = _itemRepository.FindById(Convert.ToInt32(itemId));
            if (_disposeWhenDone)
                Dispose();
            return orgDto;
        }

        public ItemDTO GetByName(string displayName)
        {
            var cat = _itemRepository.Query().Filter(c => c.DisplayName == displayName).Get().FirstOrDefault();
            return cat;
        }

        public string InsertOrUpdate(ItemDTO item)
        {
            try
            {
                var validate = Validate(item);
                if (!string.IsNullOrEmpty(validate))
                    return validate;

                if (ObjectExists(item))
                    return GenericMessages.DatabaseErrorRecordAlreadyExists;

                _itemRepository.InsertUpdate(item);

                _unitOfWork.Commit();
                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public string Disable(ItemDTO item)
        {
            if (item == null)
                return GenericMessages.ObjectIsNull;

            if (_unitOfWork.Repository<ItemQuantityDTO>().Query().Get().Any(i => i.ItemId == item.Id) ||
                _unitOfWork.Repository<PhysicalInventoryLineDTO>().Query().Get().Any(i => i.ItemId == item.Id) ||
                _unitOfWork.Repository<TransactionLineDTO>().Query().Get().Any(i => i.ItemId == item.Id) ||
                _unitOfWork.Repository<ItemBorrowDTO>().Query().Get().Any(i => i.ItemId == item.Id))
            {
                return "Can't delete the item, it is already in use...";
            }

            string stat;
            try
            {
                _itemRepository.Update(item);
                _unitOfWork.Commit();
                stat = string.Empty;
            }
            catch (Exception exception)
            {
                stat = exception.Message;
            }
            return stat;
        }

        public int Delete(string itemId)
        {
            try
            {
                _itemRepository.Delete(itemId);
                _unitOfWork.Commit();
                return 0;
            }
            catch (Exception exception)
            {
                return -1;
            }
        }

        public bool ObjectExists(ItemDTO item)
        {
            var objectExists = false;
            var iDbContext = DbContextUtil.GetDbContextInstance();
            try
            {
                var catRepository = new Repository<ItemDTO>(iDbContext);
                var catExists = catRepository.Query().Filter(bp =>bp.ItemCode == item.ItemCode && bp.Id != item.Id)
                        .Get()
                    .FirstOrDefault();

                if (catExists != null)
                    objectExists = true;
            }
            finally
            {
                iDbContext.Dispose();
            }

            return objectExists;
        }

        public string Validate(ItemDTO item)
        {
            if (null == item)
                return GenericMessages.ObjectIsNull;

            //if (String.IsNullOrEmpty(item.DisplayName))
            //    return item.DisplayName + " " + GenericMessages.StringIsNullOrEmpty;

            //if (item.DisplayName.Length > 255)
            //    return item.DisplayName + " can not be more than 255 characters ";

            if (String.IsNullOrEmpty(item.ItemCode))
                return item.ItemCode + " " + GenericMessages.StringIsNullOrEmpty;

            if (item.ItemCode.Length > 50)
                return item.ItemCode + " can not be more than 50 characters ";

            return string.Empty;
        }

        #endregion

        #region Private Methods
        public string GetItemCode()
        {
            try
            {
                return string.Empty;
                //var prefix = "I";
                //if (new ClientService(true).GetClient().HasOnlineAccess)
                //    prefix = Singleton.Edition == AMStockEdition.OnlineEdition ? prefix + "S" : prefix + "L";//S=Server L=Local

                //var lastNum = 0;

                //var lastItem = new ItemService(true).GetLastItemIncludingDeleted();

                //if (lastItem != null)
                //{
                //    //int.TryParse(lastItem.ItemCode.Substring(2), out lastNum);
                //    lastNum = lastItem.Id;
                //}

                //lastNum = lastNum + 1 + 100000;
                //return prefix + lastNum.ToString(CultureInfo.InvariantCulture).Substring(1);

            }
            catch
            {
                MessageBox.Show("Problem getting item number, try again...");
                return null;// prefix + (SelectedWarehouse.WarehouseNumber * 100000).ToString() + "0001";
            }
        }
        public ItemDTO GetLastItemIncludingDeleted()
        {
            ItemDTO piList;
            try
            {
                piList = Get()
                    .Get(1)
                    .OrderByDescending(i => i.Id)
                    .FirstOrDefault();
            }
            finally
            {
                Dispose(_disposeWhenDone);
            }

            return piList;
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