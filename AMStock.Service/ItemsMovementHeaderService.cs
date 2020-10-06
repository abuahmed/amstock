using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AMStock.Core;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.DAL;
using AMStock.DAL.Interfaces;
using AMStock.Repository;
using AMStock.Repository.Interfaces;
using AMStock.Service.Interfaces;

namespace AMStock.Service
{
    public class ItemsMovementHeaderService : IItemsMovementHeaderService
    {
        #region Fields
        private IDbContext _iDbContext;
        private IUnitOfWork _unitOfWork;
        private IRepository<ItemsMovementHeaderDTO> _imHeaderRepository;
        private IRepository<ItemsMovementLineDTO> _imLineRepository;
        private readonly bool _disposeWhenDone;
        #endregion

        #region Constructor
        public ItemsMovementHeaderService()
        {
            InitializeContext();
        }
        public ItemsMovementHeaderService(bool disposeWhenDone)
        {
            _disposeWhenDone = disposeWhenDone;
            InitializeContext();
        }
        public ItemsMovementHeaderService(IDbContext iDbContext)
        {
            _iDbContext = iDbContext;
            _imHeaderRepository = new Repository<ItemsMovementHeaderDTO>(_iDbContext);
            _imLineRepository = new Repository<ItemsMovementLineDTO>(_iDbContext);
            _unitOfWork = new UnitOfWork(_iDbContext);
        }
        public void InitializeContext()
        {
            _iDbContext = DbContextUtil.GetDbContextInstance();
            _imHeaderRepository = new Repository<ItemsMovementHeaderDTO>(_iDbContext);
            _imLineRepository = new Repository<ItemsMovementLineDTO>(_iDbContext);
            _unitOfWork = new UnitOfWork(_iDbContext);
        }

        #endregion

        #region Common Methods

        public IRepositoryQuery<ItemsMovementHeaderDTO> Get()
        {
            var imList = _imHeaderRepository
                .Query()
                .Include(a => a.ItemsMovementLines, a => a.FromWarehouse, a => a.ToWarehouse)
                .Filter(a => !string.IsNullOrEmpty(a.MovementNumber))
                .OrderBy(q => q.OrderByDescending(c => c.MovementDate));
            return imList;
        }

        public IEnumerable<ItemsMovementHeaderDTO> GetAll(SearchCriteria<ItemsMovementHeaderDTO> criteria = null)
        {
            IEnumerable<ItemsMovementHeaderDTO> imList = new List<ItemsMovementHeaderDTO>();
            try
            {
                if (criteria != null && criteria.CurrentUserId != -1)
                {
                    var pdto = Get();

                    foreach (var cri in criteria.FiList)
                    {
                        pdto.FilterList(cri);
                    }

                    #region By Duration

                    if (criteria.BeginingDate != null)
                    {
                        var beginDate = new DateTime(criteria.BeginingDate.Value.Year, criteria.BeginingDate.Value.Month,
                            criteria.BeginingDate.Value.Day, 0, 0, 0);
                        pdto.FilterList(p => p.MovementDate >= beginDate);
                    }

                    if (criteria.EndingDate != null)
                    {
                        var endDate = new DateTime(criteria.EndingDate.Value.Year, criteria.EndingDate.Value.Month,
                            criteria.EndingDate.Value.Day, 23, 59, 59);
                        pdto.FilterList(p => p.MovementDate <= endDate);
                    }

                    #endregion

                    IList<ItemsMovementHeaderDTO> pdtoList;
                    if (criteria.Page != 0 && criteria.PageSize != 0)
                    {
                        int totalCount;
                        pdtoList = pdto.GetPage(criteria.Page, criteria.PageSize, out totalCount).ToList();
                    }
                    else
                        pdtoList = pdto.GetList().ToList();

                    imList = imList.Concat(pdtoList).ToList();

                }
                else
                    imList = Get().Get().ToList();

            }
            finally
            {
                Dispose(_disposeWhenDone);
            }

            return imList;
        }

        public ItemsMovementHeaderDTO Find(string imHeaderId)
        {
            return _imHeaderRepository.FindById(imHeaderId);
        }

        public string InsertOrUpdate(ItemsMovementHeaderDTO imHeader)
        {
            try
            {
                var validate = Validate(imHeader);
                if (!string.IsNullOrEmpty(validate))
                    return validate;

                if (ObjectExists(imHeader))
                    return GenericMessages.DatabaseErrorRecordAlreadyExists;

                _imHeaderRepository.InsertUpdate(imHeader);
                _unitOfWork.Commit();
                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public string Disable(ItemsMovementHeaderDTO imHeader)
        {
            if (imHeader == null)
                return GenericMessages.ObjectIsNull;

            string stat;
            try
            {
                _imHeaderRepository.Update(imHeader);
                _unitOfWork.Commit();
                stat = string.Empty;
            }
            catch (Exception exception)
            {
                stat = exception.Message;
            }
            return stat;
        }

        public int Delete(string imHeaderId)
        {
            try
            {
                _imHeaderRepository.Delete(imHeaderId);
                _unitOfWork.Commit();
                return 0;
            }
            catch (Exception exception)
            {
                return -1;
            }
        }

        public bool ObjectExists(ItemsMovementHeaderDTO imHeader)
        {
            var objectExists = false;
            var iDbContext = DbContextUtil.GetDbContextInstance();
            try
            {
                var catRepository = new Repository<ItemsMovementHeaderDTO>(iDbContext);
                var catExists = catRepository.Query()
                    .Filter(bp => bp.MovementNumber == imHeader.MovementNumber && bp.Id != imHeader.Id)
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

        public string Validate(ItemsMovementHeaderDTO imHeader)
        {
            if (null == imHeader)
                return GenericMessages.ObjectIsNull;

            if (imHeader.FromWarehouseId == -1)
                return "From Warehouse " + GenericMessages.ObjectIsNull;

            if (imHeader.ToWarehouseId == -1)
                return "To Warehouse " + GenericMessages.ObjectIsNull;

            if (String.IsNullOrEmpty(imHeader.MovementNumber))
                return imHeader.MovementNumber + " " + GenericMessages.StringIsNullOrEmpty;

            if (imHeader.MovementNumber.Length > 50)
                return imHeader.MovementNumber + " can not be more than 50 characters ";

            return string.Empty;
        }

        #endregion

        #region Private Methods

        public string Post(ItemsMovementHeaderDTO iMHeader)
        {
            try
            {
                var imHeader = this.Get().Filter(im => im.Id == iMHeader.Id).Get().FirstOrDefault();
                //For Eager Loading
                var items =
                    _imLineRepository.Query()
                        .Include(l => l.Item)
                        .Filter(l => l.ItemsMovementHeaderId == imHeader.Id)
                        .Get();
                if (imHeader != null)
                {
                    if (imHeader.ItemsMovementLines.Count == 0)
                        return "No Items To Post, Add Item First....";

                    var itemsQuantityRepository = new Repository<ItemQuantityDTO>(_iDbContext);

                    var validate = Validate(imHeader);
                    if (!string.IsNullOrEmpty(validate))
                        return validate;

                    foreach (var line in imHeader.ItemsMovementLines.Where(p => p.Enabled))
                    {
                        var itFrQty = new ItemQuantityService(_iDbContext)
                            .GetByCriteria(imHeader.FromWarehouseId, line.ItemId);

                        itFrQty.QuantityOnHand = itFrQty.QuantityOnHand - (int)line.MovedQuantity;
                        if (itFrQty.QuantityOnHand < 0)
                            return "Less than 0 stock level found for item " + line.Item.DisplayName;
                        itemsQuantityRepository.InsertUpdate(itFrQty);

                        var itToQty = new ItemQuantityService(_iDbContext)
                            .GetByCriteria(imHeader.ToWarehouseId, line.ItemId) ?? new ItemQuantityDTO
                            {
                                WarehouseId = imHeader.ToWarehouseId,
                                ItemId = line.ItemId,
                                QuantityOnHand = 0
                            };

                        itToQty.QuantityOnHand = itToQty.QuantityOnHand + (int)line.MovedQuantity;

                        itemsQuantityRepository.InsertUpdate(itToQty);
                    }


                    imHeader.Status = TransactionStatus.Posted;
                    _imHeaderRepository.Update(imHeader);

                    _unitOfWork.Commit();

                    return string.Empty;
                }
                return "can't Find Items Movement...";
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public string UnPost(ItemsMovementHeaderDTO iMHeader)
        {
            try
            {
                var imHeader = this.Get().Filter(im => im.Id == iMHeader.Id).Get().FirstOrDefault();
                //For Eager Loading
                var items =
                    _imLineRepository.Query()
                        .Include(l => l.Item)
                        .Filter(l => l.ItemsMovementHeaderId == imHeader.Id)
                        .Get();

                if (imHeader != null)
                {
                    if (imHeader.ItemsMovementLines.Count == 0)
                        return "No Items To UnPost, Add Item First....";

                    var itemsQuantityRepository = new Repository<ItemQuantityDTO>(_iDbContext);

                    var validate = Validate(imHeader);
                    if (!string.IsNullOrEmpty(validate))
                        return validate;

                    foreach (var line in imHeader.ItemsMovementLines.Where(p => p.Enabled))
                    {
                        var itFrQty = new ItemQuantityService(_iDbContext)
                            .GetByCriteria(imHeader.FromWarehouseId, line.ItemId);

                        itFrQty.QuantityOnHand = itFrQty.QuantityOnHand + (int)line.MovedQuantity;

                        itemsQuantityRepository.InsertUpdate(itFrQty);

                        var itToQty = new ItemQuantityService(_iDbContext)
                            .GetByCriteria(imHeader.ToWarehouseId, line.ItemId);

                        itToQty.QuantityOnHand = itToQty.QuantityOnHand - (int)line.MovedQuantity;
                        if (itFrQty.QuantityOnHand < 0)
                            return "Less than 0 stock level found for item " + line.Item.DisplayName;
                        itemsQuantityRepository.InsertUpdate(itToQty);
                    }


                    imHeader.Status = TransactionStatus.Draft;
                    _imHeaderRepository.Update(imHeader);

                    _unitOfWork.Commit();

                    return string.Empty;
                }
                return "can't Find Items Movement...";
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public string GetNewMovementNumber(int warehouseId, bool disposeWhenDone)
        {
            var imNum = "IM" + Guid.NewGuid().ToString().Substring(0, 4);// "IM0001";
            try
            {


            }
            catch
            {
                MessageBox.Show("Problem getting transaction number, try again...");
                return null;
            }
            finally
            {
                if (disposeWhenDone)
                    _unitOfWork.Dispose();
            }

            return imNum;
        }
        #endregion

        #region Child Methods
        public IEnumerable<ItemsMovementLineDTO> GetChilds(int parentId, bool disposeWhenDone)
        {
            IEnumerable<ItemsMovementLineDTO> imList;
            try
            {
                imList = _imLineRepository
                    .Query()
                    .Include(a => a.ItemsMovementHeader, a => a.Item)
                    .Get()
                    .OrderBy(i => i.Id).ToList();
                if (parentId != 0)
                    imList = imList.Where(l => l.ItemsMovementHeaderId == parentId).ToList();
            }
            finally
            {
                Dispose(disposeWhenDone);
            }

            return imList;

        }

        public IRepositoryQuery<ItemsMovementLineDTO> GetChildsQuery()
        {
            var imList = _imLineRepository
                .Query()
                .Include(i => i.Item, s => s.ItemsMovementHeader, s => s.ItemsMovementHeader.FromWarehouse, s => s.ItemsMovementHeader.ToWarehouse)
                .OrderBy(q => q.OrderByDescending(c => c.Id));
            return imList;
        }

        public IEnumerable<ItemsMovementLineDTO> GetAllChilds(SearchCriteria<ItemsMovementLineDTO> criteria = null)
        {
            IEnumerable<ItemsMovementLineDTO> imList = new List<ItemsMovementLineDTO>();
            try
            {
                if (criteria != null && criteria.CurrentUserId != -1)
                {
                    var pdto = GetChildsQuery();

                    foreach (var cri in criteria.FiList)
                    {
                        pdto.FilterList(cri);
                    }

                    #region By Duration

                    if (criteria.BeginingDate != null)
                    {
                        var beginDate = new DateTime(criteria.BeginingDate.Value.Year, criteria.BeginingDate.Value.Month,
                            criteria.BeginingDate.Value.Day, 0, 0, 0);
                        pdto.FilterList(p => p.ItemsMovementHeader.MovementDate >= beginDate);
                    }

                    if (criteria.EndingDate != null)
                    {
                        var endDate = new DateTime(criteria.EndingDate.Value.Year, criteria.EndingDate.Value.Month,
                            criteria.EndingDate.Value.Day, 23, 59, 59);
                        pdto.FilterList(p => p.ItemsMovementHeader.MovementDate <= endDate);
                    }

                    #endregion

                    IList<ItemsMovementLineDTO> pdtoList;
                    if (criteria.Page != 0 && criteria.PageSize != 0)
                    {
                        int totalCount;
                        pdtoList = pdto.GetPage(criteria.Page, criteria.PageSize, out totalCount).ToList();
                    }
                    else
                        pdtoList = pdto.GetList().ToList();

                    imList = imList.Concat(pdtoList).ToList();
                }
                else
                    imList = GetChildsQuery().Get().ToList();


            }
            finally
            {
                Dispose(_disposeWhenDone);
            }

            return imList;

        }

        public string InsertOrUpdateChild(ItemsMovementLineDTO imLine)
        {
            try
            {
                var validate = ValidateChild(imLine);
                if (!string.IsNullOrEmpty(validate))
                    return validate;

                if (ObjectExistsChild(imLine))
                    return GenericMessages.DatabaseErrorRecordAlreadyExists;

                _imLineRepository.InsertUpdate(imLine);
                _unitOfWork.Commit();
                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public string DisableChild(ItemsMovementLineDTO imLine)
        {
            if (imLine == null || imLine.Id == 0 || imLine.ItemsMovementHeader == null ||
                imLine.ItemsMovementHeader.Status != TransactionStatus.Draft)
            {
                return "First choose Item to delete...";
            }

            string stat;
            try
            {
                _imLineRepository.Update(imLine);
                _unitOfWork.Commit();
                stat = string.Empty;
            }
            catch (Exception exception)
            {
                stat = exception.Message;
            }
            return stat;
        }

        public bool ObjectExistsChild(ItemsMovementLineDTO imLine)
        {
            var objectExists = false;
            //var iDbContext = DbContextUtil.GetDbContextInstance();
            //try
            //{
            //    var catRepository = new Repository<ItemsMovementLineDTO>(iDbContext);
            //    var catExists = catRepository.GetAll()
            //        .FirstOrDefault(bp => bp.ItemsMovementId == imLine.ItemsMovementId && bp.ItemId == imLine.ItemId && bp.Id != imLine.Id);
            //    if (catExists != null)
            //        objectExists = true;
            //}
            //finally
            //{
            //    iDbContext.Dispose();
            //}

            return objectExists;
        }

        public string ValidateChild(ItemsMovementLineDTO imLine)
        {
            if (null == imLine)
                return GenericMessages.ObjectIsNull;

            if (imLine.ItemsMovementHeader == null)
                return "Items Movement Header " + GenericMessages.ObjectIsNull;

            if (imLine.ItemId == 0)
                return "No item is found in the items movement line";

            if (imLine.MovedQuantity < 0)
                return imLine.MovedQuantity + " can not be less than 0 ";

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