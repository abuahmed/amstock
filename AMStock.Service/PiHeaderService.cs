#region usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AMStock.Core;
using AMStock.Core.Enumerations;
using AMStock.Core.Extensions;
using AMStock.Core.Models;
using AMStock.DAL;
using AMStock.DAL.Interfaces;
using AMStock.Repository;
using AMStock.Repository.Interfaces;
using AMStock.Service.Interfaces;

#endregion

namespace AMStock.Service
{
    public class PiHeaderService : IPiHeaderService
    {
        #region Fields
        private IDbContext _iDbContext;
        private IUnitOfWork _unitOfWork;
        private IRepository<PhysicalInventoryHeaderDTO> _piHeaderRepository;
        private IRepository<PhysicalInventoryLineDTO> _piLineRepository;
        private readonly bool _disposeWhenDone;
        #endregion

        #region Constructor
        public PiHeaderService()
        {
            InitializeContext();
        }
        public PiHeaderService(bool disposeWhenDone)
        {
            _disposeWhenDone = disposeWhenDone;
            InitializeContext();
        }
        public PiHeaderService(IDbContext iDbContext)
        {
            _iDbContext = iDbContext;
            _piHeaderRepository = new Repository<PhysicalInventoryHeaderDTO>(_iDbContext);
            _piLineRepository = new Repository<PhysicalInventoryLineDTO>(_iDbContext);
            _unitOfWork = new UnitOfWork(_iDbContext);
        }
        public void InitializeContext()
        {
            _iDbContext = DbContextUtil.GetDbContextInstance();
            _piHeaderRepository = new Repository<PhysicalInventoryHeaderDTO>(_iDbContext);
            _piLineRepository = new Repository<PhysicalInventoryLineDTO>(_iDbContext);
            _unitOfWork = new UnitOfWork(_iDbContext);
        }

        #endregion

        #region Common Methods
        public IRepositoryQuery<PhysicalInventoryHeaderDTO> Get()
        {
            var piList = _piHeaderRepository
              .Query()
              .Include(a => a.PhysicalInventoryLines, a => a.Warehouse)
              .Filter(a => !string.IsNullOrEmpty(a.PhysicalInventoryNumber))
              .OrderBy(q => q.OrderByDescending(c => c.Id));
            return piList;
        }
        
        public IEnumerable<PhysicalInventoryHeaderDTO> GetAll(SearchCriteria<PhysicalInventoryHeaderDTO> criteria = null)
        {
            IEnumerable<PhysicalInventoryHeaderDTO> piList = new List<PhysicalInventoryHeaderDTO>();
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
                            pdto.FilterList(p => p.PhysicalInventoryDate >= beginDate);
                        }

                        if (criteria.EndingDate != null)
                        {
                            var endDate = new DateTime(criteria.EndingDate.Value.Year, criteria.EndingDate.Value.Month,
                                criteria.EndingDate.Value.Day, 23, 59, 59);
                            pdto.FilterList(p => p.PhysicalInventoryDate <= endDate);
                        }

                        #endregion

                        IList<PhysicalInventoryHeaderDTO> pdtoList;
                        if (criteria.Page != 0 && criteria.PageSize != 0)
                        {
                            int totalCount;
                            pdtoList = pdto.GetPage(criteria.Page, criteria.PageSize, out totalCount).ToList();
                        }
                        else
                            pdtoList = pdto.GetList().ToList();

                        

                        piList = piList.Concat(pdtoList).ToList();
                    }
                }
                else
                    piList = Get().Get().ToList();
                
            }
            finally
            {
                Dispose(_disposeWhenDone);
            }

            return piList;

        }

        public PhysicalInventoryHeaderDTO Find(string piHeaderId)
        {
            return _piHeaderRepository.FindById(piHeaderId);
        }

        public string InsertOrUpdate(PhysicalInventoryHeaderDTO piHeader)
        {
            try
            {
                var validate = Validate(piHeader);
                if (!string.IsNullOrEmpty(validate))
                    return validate;

                if (ObjectExists(piHeader))
                    return GenericMessages.DatabaseErrorRecordAlreadyExists;

                _piHeaderRepository.InsertUpdate(piHeader);
                _unitOfWork.Commit();
                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public string Disable(PhysicalInventoryHeaderDTO piHeader)
        {
            if (piHeader == null)
                return GenericMessages.ObjectIsNull;

            string stat;
            try
            {
                _piHeaderRepository.Update(piHeader);
                _unitOfWork.Commit();
                stat = string.Empty;
            }
            catch (Exception exception)
            {
                stat = exception.Message;
            }
            return stat;
        }

        public int Delete(string piHeaderId)
        {
            try
            {
                _piHeaderRepository.Delete(piHeaderId);
                _unitOfWork.Commit();
                return 0;
            }
            catch (Exception exception)
            {
                return -1;
            }
        }

        public bool ObjectExists(PhysicalInventoryHeaderDTO piHeader)
        {
            var objectExists = false;
            var iDbContext = DbContextUtil.GetDbContextInstance();
            try
            {
                var catRepository = new Repository<PhysicalInventoryHeaderDTO>(iDbContext);
                var catExists = catRepository.Query()
                    .Filter(bp => bp.PhysicalInventoryNumber == piHeader.PhysicalInventoryNumber && bp.Id != piHeader.Id)
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

        public string Validate(PhysicalInventoryHeaderDTO piHeader)
        {
            if (null == piHeader)
                return GenericMessages.ObjectIsNull;

            if (piHeader.Warehouse == null)
                return "Warehouse " + GenericMessages.ObjectIsNull;

            if (String.IsNullOrEmpty(piHeader.PhysicalInventoryNumber))
                return piHeader.PhysicalInventoryNumber + " " + GenericMessages.StringIsNullOrEmpty;

            if (piHeader.PhysicalInventoryNumber.Length > 50)
                return piHeader.PhysicalInventoryNumber + " can not be more than 50 characters ";

            return string.Empty;
        }

        #endregion

        #region Private Methods

        public string Post(PhysicalInventoryHeaderDTO piHeader)
        {
            try
            {
                var itemsQuantityRepository = new Repository<ItemQuantityDTO>(_iDbContext);
                if (piHeader.PhysicalInventoryLines.Count == 0)
                    return "No Items To Post, Add Item First....";

                var validate = Validate(piHeader);
                if (!string.IsNullOrEmpty(validate))
                    return validate;

                foreach (var line in piHeader.PhysicalInventoryLines.Where(p => p.Enabled))
                {
                    ////var itemQty = new ItemQuantityDTO
                    ////{
                    ////    WarehouseId = piHeader.WarehouseId,
                    ////    ItemId = line.ItemId,
                    ////    QuantityOnHand = line.CountedQty
                    ////};
                    //itemQty.QuantityOnHand = line.ExpectedQty;
                    var itQty = new ItemQuantityService(_iDbContext).GetByCriteria(piHeader.WarehouseId, line.ItemId);
                    itQty.QuantityOnHand = line.CountedQty;

                    itemsQuantityRepository.InsertUpdate(itQty);
                }


                piHeader.Status = PhysicalInventoryStatus.Posted;
                _piHeaderRepository.Update(piHeader);

                _unitOfWork.Commit();

                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public string UnPost(PhysicalInventoryHeaderDTO piHeader)
        {
            try
            {
                var itemsQuantityRepository = new Repository<ItemQuantityDTO>(_iDbContext);
                if (piHeader.PhysicalInventoryLines.Count == 0)
                    return "No Items To Post, Add Item First....";

                var validate = Validate(piHeader);
                if (!string.IsNullOrEmpty(validate))
                    return validate;

                foreach (var line in piHeader.PhysicalInventoryLines.Where(p => p.Enabled))
                {
                    var itQty = new ItemQuantityService(_iDbContext).GetByCriteria(piHeader.WarehouseId, line.ItemId);
                    if (line.CountedQty >= itQty.QuantityOnHand)
                    {
                        //donothing
                    }
                    if (line.CountedQty < itQty.QuantityOnHand)
                    {
                        itemsQuantityRepository = new Repository<ItemQuantityDTO>(_iDbContext);
                        return "Can't Unpost";
                    }

                    itQty.QuantityOnHand = line.ExpectedQty;
                    itemsQuantityRepository.InsertUpdate(itQty);
                }


                piHeader.Status = PhysicalInventoryStatus.Draft;
                _piHeaderRepository.Update(piHeader);

                _unitOfWork.Commit();

                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public PhysicalInventoryLineDTO GetNewPiLine(int warehouseId, int itemId,
                                                     decimal countedQty, PhysicalInventoryLineTypes lineType)
        {
            var line = new PhysicalInventoryLineDTO();

            try
            {
                var selectedItemQuantity = new ItemQuantityService(false).GetByCriteria(warehouseId, itemId);

                //var selectedPhysicalInventory = GetListOf(false)
                //.FirstOrDefault(pi => pi.WarehouseId == warehouseId &&
                //                      pi.PhysicalInventoryDate.Date == DateTime.Now.Date) ??
                var selectedPhysicalInventory = new PhysicalInventoryHeaderDTO
                                                {
                                                    PhysicalInventoryDate = DateTime.Now,
                                                    Status = PhysicalInventoryStatus.Posted,
                                                    WarehouseId = warehouseId,
                                                    PhysicalInventoryNumber = GetNewPhysicalInventoryNumber(warehouseId, false)
                                                };

                line.PhysicalInventory = selectedPhysicalInventory;
                line.ItemId = itemId;
                line.CountedQty = countedQty;
                line.ExpectedQty = selectedItemQuantity != null ? selectedItemQuantity.QuantityOnHand : 0;
                line.PhysicalInventoryLineType = lineType;

            }
            catch
            {
                MessageBox.Show("Problem getting new pi, try again...");
                return null;
            }
            finally
            {
                //_unitOfWork.Dispose();
            }

            return line;
        }
        public string GetNewPhysicalInventoryNumber(int warehouseId, bool disposeWhenDone)
        {
            var piNum = "";
            try
            {
                var selectedWarehouse = new WarehouseService(true).Find(warehouseId.ToString());
                var prefix = "PI";
                var clDto = new ClientService(true).GetClient();

                if (clDto != null && clDto.HasOnlineAccess)
                    prefix = Singleton.Edition == AMStockEdition.OnlineEdition ? prefix + "S" : prefix + "L";//S=Server L=Local

                var lastNum = 0;

                var lastTransaction = Get().Filter(t => t.WarehouseId == selectedWarehouse.Id)
                    .Get()
                    .OrderByDescending(i => i.Id)
                    .FirstOrDefault();

                if (lastTransaction != null)
                {
                    int.TryParse(lastTransaction.PhysicalInventoryNumber.Substring(5), out lastNum);
                }

                lastNum = lastNum + 1 + 1000000;

                var subLength = 1;
                if (clDto != null && clDto.Organizations.Count > 1)
                    subLength = 0;

                piNum = prefix +
                       selectedWarehouse.WarehouseNumber.ToString().Substring(subLength) +
                       lastNum.ToString().Substring(1);

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

            return piNum;
        }
        #endregion

        #region Child Methods
        public IEnumerable<PhysicalInventoryLineDTO> GetChilds(int parentId, bool disposeWhenDone)
        {
            IEnumerable<PhysicalInventoryLineDTO> piList;
            try
            {
                piList = _piLineRepository
                    .Query()
                    .Include(a => a.PhysicalInventory, a => a.Item)
                    .Get()
                    .OrderBy(i => i.Id).ToList();
                if (parentId != 0)
                    piList = piList.Where(l => l.PhysicalInventoryId == parentId).ToList();
            }
            finally
            {
                Dispose(disposeWhenDone);
            }

            return piList;

        }

        public IRepositoryQuery<PhysicalInventoryLineDTO> GetChildsQuery()
        {
            var piList = _piLineRepository
              .Query()
              .Include(i => i.Item, s => s.PhysicalInventory, s => s.PhysicalInventory.Warehouse)
              .OrderBy(q => q.OrderByDescending(c => c.Id));
            return piList;
        }

        public IEnumerable<PhysicalInventoryLineDTO> GetAllChilds(SearchCriteria<PhysicalInventoryLineDTO> criteria = null)
        {
            IEnumerable<PhysicalInventoryLineDTO> piList = new List<PhysicalInventoryLineDTO>();
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
                        var pdto = GetChildsQuery();

                        foreach (var cri in criteria.FiList)
                        {
                            pdto.FilterList(cri);
                        }

                        #region By Warehouse
                        var warehouse1 = warehouse;
                        pdto.FilterList(p => p.PhysicalInventory.WarehouseId == warehouse1.Id);
                        #endregion

                        #region By Duration

                        if (criteria.BeginingDate != null)
                        {
                            var beginDate = new DateTime(criteria.BeginingDate.Value.Year, criteria.BeginingDate.Value.Month,
                                criteria.BeginingDate.Value.Day, 0, 0, 0);
                            pdto.FilterList(p => p.PhysicalInventory.PhysicalInventoryDate >= beginDate);
                        }

                        if (criteria.EndingDate != null)
                        {
                            var endDate = new DateTime(criteria.EndingDate.Value.Year, criteria.EndingDate.Value.Month,
                                criteria.EndingDate.Value.Day, 23, 59, 59);
                            pdto.FilterList(p => p.PhysicalInventory.PhysicalInventoryDate <= endDate);
                        }

                        #endregion

                        IList<PhysicalInventoryLineDTO> pdtoList;
                        if (criteria.Page != 0 && criteria.PageSize != 0)
                        {
                            int totalCount;
                            pdtoList = pdto.GetPage(criteria.Page, criteria.PageSize, out totalCount).ToList();
                        }
                        else
                            pdtoList = pdto.GetList().ToList();

                        piList = piList.Concat(pdtoList).ToList();
                    }
                }
                else
                    piList = GetChildsQuery().Get().ToList();


            }
            finally
            {
                Dispose(_disposeWhenDone);
            }

            return piList;

        }

        public string InsertOrUpdateChild(PhysicalInventoryLineDTO piLine)
        {
            try
            {
                var validate = ValidateChild(piLine);
                if (!string.IsNullOrEmpty(validate))
                    return validate;

                if (ObjectExistsChild(piLine))
                    return GenericMessages.DatabaseErrorRecordAlreadyExists;

                _piLineRepository.InsertUpdate(piLine);
                _unitOfWork.Commit();
                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public string DisableChild(PhysicalInventoryLineDTO piLine)
        {
            if (piLine == null || piLine.Id == 0 || piLine.PhysicalInventory == null ||
                piLine.PhysicalInventory.Status != PhysicalInventoryStatus.Draft)
            {
                return "First choose Item to delete...";
            }

            string stat;
            try
            {
                _piLineRepository.Update(piLine);
                _unitOfWork.Commit();
                stat = string.Empty;
            }
            catch (Exception exception)
            {
                stat = exception.Message;
            }
            return stat;
        }

        public bool ObjectExistsChild(PhysicalInventoryLineDTO piLine)
        {
            var objectExists = false;
            //var iDbContext = DbContextUtil.GetDbContextInstance();
            //try
            //{
            //    var catRepository = new Repository<PhysicalInventoryLineDTO>(iDbContext);
            //    var catExists = catRepository.GetAll()
            //        .FirstOrDefault(bp => bp.PhysicalInventoryId == piLine.PhysicalInventoryId && bp.ItemId == piLine.ItemId && bp.Id != piLine.Id);
            //    if (catExists != null)
            //        objectExists = true;
            //}
            //finally
            //{
            //    iDbContext.Dispose();
            //}

            return objectExists;
        }

        public string ValidateChild(PhysicalInventoryLineDTO piLine)
        {
            if (null == piLine)
                return GenericMessages.ObjectIsNull;

            if (piLine.PhysicalInventory == null)
                return "PhysicalInventory " + GenericMessages.ObjectIsNull;

            if (piLine.ItemId == 0)
                return "No item is found in the physical inventory line";

            if (piLine.CountedQty < 0)
                return piLine.CountedQty + " can not be less than 0 ";

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
