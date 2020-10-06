using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using AMStock.Core;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.DAL;
using AMStock.DAL.Interfaces;
using AMStock.Repository;
using AMStock.Repository.Interfaces;
using AutoMapper;

namespace AMStock.SyncEngine
{
    public class SyncTask
    {
        #region Fields
        private System.Timers.Timer _monitorTimer;
        private const int MonitorTimerDelay = 6000; //60 second
        private UnitOfWork _unitOfWork;
        private UnitOfWorkServer _unitOfWorkServer;
        #endregion

        #region Constructor
        public SyncTask()
        {
            //Singleton.Edition = AMStockEdition.ServerEdition;
            //Singleton.SqlceFileName = "AMStockDb";

            /**Uncomment below for Compact Edition*/
            Singleton.Edition = AMStockEdition.CompactEdition;
            var path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\AMStock\\";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var pathfile = Path.Combine(path, "AMStockDbMedinaLatest_2.sdf");
            Singleton.SqlceFileName = pathfile;

            _unitOfWork = new UnitOfWork(new DbContextFactory().Create());
            _unitOfWorkServer = new UnitOfWorkServer(new ServerDbContextFactory().Create());

            Setting = _unitOfWork.Repository<SettingDTO>().Query().Get().FirstOrDefault();

            if (Setting != null)
            {
                LastFromServerSyncDate = Setting.LastFromServerSyncDate != null ? (DateTime)Setting.LastFromServerSyncDate : DateTime.Now.AddYears(-1);
                LastToServerSyncDate = Setting.LastToServerSyncDate != null ? (DateTime)Setting.LastToServerSyncDate : DateTime.Now.AddYears(-1);
            }
            else
            {
                LastFromServerSyncDate = DateTime.Now.AddYears(-1);
                LastToServerSyncDate = DateTime.Now.AddYears(-1);
            }
        }
        public static DateTime LastToServerSyncDate { get; set; }
        public static DateTime LastFromServerSyncDate { get; set; }

        public static DateTime LastServerSyncDate { get; set; }
        public static SettingDTO Setting { get; set; }
        #endregion

        private void Initialize()
        {
            _monitorTimer = new System.Timers.Timer(MonitorTimerDelay);
            _monitorTimer.Elapsed += OnMonitorTimerElapsed;
        }
        public void Start()
        {
            try
            {
                Initialize();
                _monitorTimer.Enabled = true;
            }
            catch (Exception ex)
            {
                ServiceLog.Log(ex.Message);
            }
        }
        private void OnMonitorTimerElapsed(object source, System.Timers.ElapsedEventArgs e)
        {
            _monitorTimer.Enabled = false;
            Sync();
            _monitorTimer.Enabled = true;
        }
        public IUnitOfWork GetNewUow(IUnitOfWork uom)
        {
            uom.Dispose();
            _unitOfWorkServer = new UnitOfWorkServer(new ServerDbContextFactory().Create());
            return _unitOfWorkServer;
        }
        public void Sync()
        {
            IUnitOfWork sourceUom = _unitOfWork;
            IUnitOfWork destUom = _unitOfWorkServer;

            LastServerSyncDate = LastToServerSyncDate;

            Setting.LastToServerSyncDate = DateTime.Now;

            ServiceLog.Log(" - Sync To Server started");//TO SERVER
            try
            {
                var client = destUom.Repository<ProductActivationDTO>().FindById(1);
                if (client == null)
                {
                    //SyncGenerals(sourceUom, destUom);
                }

                if (!SyncAddresss(sourceUom, destUom)) return;
                if (!SyncCategories(sourceUom, destUom)) return;

                if (!SyncClients(sourceUom, destUom)) return;
                destUom = GetNewUow(destUom);
                if (!SyncOrganizations(sourceUom, destUom)) return;
                destUom = GetNewUow(destUom);
                if (!SyncWarehouses(sourceUom, destUom)) return;
                destUom = GetNewUow(destUom);
                if (!SyncBusinessPartners(sourceUom, destUom)) return;
                destUom = GetNewUow(destUom);
                if (!SyncFinancialAccounts(sourceUom, destUom)) return;
                destUom = GetNewUow(destUom);

                if (!SyncItems(sourceUom, destUom)) return;
                destUom = GetNewUow(destUom);
                if (!SyncItemQuantities(sourceUom, destUom)) return;

                if (!SyncTransactionHeaders(sourceUom, destUom)) return;
                destUom = GetNewUow(destUom);
                if (!SyncTransactionLines(sourceUom, destUom)) return;

                if (!SyncPhysicalInventoryHeaders(sourceUom, destUom)) return;
                destUom = GetNewUow(destUom);
                if (!SyncPhysicalInventoryLines(sourceUom, destUom)) return;

                if (!SyncCpos(sourceUom, destUom)) return;
                if (!SyncItemBorrows(sourceUom, destUom)) return;

                //if (!SyncUsers(sourceUom, destUom)) return;
                //if (!SyncRoles(sourceUom, destUom)) return; destUom = GetNewUow(destUom);
                //if (!SyncUsersInRoles(sourceUom, destUom)) return;

                if (!SyncChecks(sourceUom, destUom)) return;
                //if (!SyncClearances(sourceUom, destUom)) return;
                if (!SyncPayments(sourceUom, destUom)) return;

                _unitOfWork.Repository<SettingDTO>().SimpleUpdate(Setting);
                _unitOfWork.Commit();

                ServiceLog.Log(" - Sync To Server Completed");
            }
            catch (Exception ex)
            {
                ServiceLog.Log(" - Sync To Server Failed :-" +
                               Environment.NewLine + ex.Message +
                               Environment.NewLine + ex.InnerException);
            }
            finally
            {
                _unitOfWork.Dispose();
            }
        }

        #region Sync Methods
        public bool SyncGenerals(IUnitOfWork sourceUnitOfWork, IUnitOfWork destinationUnitOfWork)
        {
            ServiceLog.Log(" - General Sync Started");

            try
            {
                var prd = sourceUnitOfWork.Repository<ProductActivationDTO>().FindById(1);
                destinationUnitOfWork.Repository<ProductActivationDTO>().CrudByRowGuid(prd);

                var set = sourceUnitOfWork.Repository<SettingDTO>().FindById(1);
                destinationUnitOfWork.Repository<SettingDTO>().CrudByRowGuid(set);

                destinationUnitOfWork.Commit();

            }
            catch (Exception ex)
            {

                ServiceLog.Log(" - General Sync Failed :-" +
                                               Environment.NewLine + ex.Message +
                                               Environment.NewLine + ex.InnerException);
                return false;
            }
            ServiceLog.Log(" - General Sync Completed");
            return true;
        }
        public bool SyncClients(IUnitOfWork sourceUnitOfWork, IUnitOfWork destinationUnitOfWork)
        {
            ServiceLog.Log(" - Client Sync Started");

            try
            {
                #region Sync Clients
                var sourceList = sourceUnitOfWork.Repository<ClientDTO>().Query()
                    .Include(a => a.Address).Filter(a => a.DateLastModified > LastServerSyncDate)
                    .Get()
                    .ToList();
                //.Where(a => a.DateLastModified > LastServerSyncDate)

                if (sourceList.Any())
                {
                    var destAddresses = destinationUnitOfWork.Repository<AddressDTO>().Query()
                    .Get()
                    .ToList();

                    var destList = destinationUnitOfWork.Repository<ClientDTO>().Query()
                        .Include(a => a.Address)
                        .Get()
                        .ToList();

                    foreach (var clientDTO in sourceList)
                    {
                        var clients = destList.FirstOrDefault(i => i.RowGuid == clientDTO.RowGuid);

                        var clientId = 0;
                        if (clients == null)
                            clients = new ClientDTO();
                        else
                            clientId = clients.Id;

                        Mapper.Reset();
                        Mapper.CreateMap<ClientDTO, ClientDTO>();
                        clients = Mapper.Map(clientDTO, clients);
                        clients.Id = clientId;

                        #region Foreign Keys

                        var categoryDTO = destAddresses.FirstOrDefault(c => c.RowGuid == clientDTO.Address.RowGuid);
                        {
                            clients.Address = categoryDTO;
                            clients.AddressId = categoryDTO != null ? categoryDTO.Id : 1;
                        }

                        #endregion

                        destinationUnitOfWork.Repository<ClientDTO>().InsertUpdate(clients);
                    }

                    destinationUnitOfWork.Commit();
                }

                #endregion
            }
            catch (Exception ex)
            {

                ServiceLog.Log(" - Client Sync Failed :-" +
                                               Environment.NewLine + ex.Message +
                                               Environment.NewLine + ex.InnerException);
                return false;
            }
            ServiceLog.Log(" - Client Sync Completed");
            return true;
        }
        public bool SyncOrganizations(IUnitOfWork sourceUnitOfWork, IUnitOfWork destinationUnitOfWork)
        {
            ServiceLog.Log(" - Organization Sync Started");

            try
            {
                #region Sync Organizations
                var sourceList = sourceUnitOfWork.Repository<OrganizationDTO>().Query()
                    .Include(i => i.Address, i => i.Client)
                    .Filter(a => a.DateLastModified > LastServerSyncDate).Get().ToList();

                if (sourceList.Any())
                {
                    var destClients = destinationUnitOfWork.Repository<ClientDTO>().Query().Get().ToList();
                    var destAddresses = destinationUnitOfWork.Repository<AddressDTO>().Query().Get().ToList();

                    var destList = destinationUnitOfWork.Repository<OrganizationDTO>().Query()
                        .Include(i => i.Address, i => i.Client).Get().ToList();

                    foreach (var orgDTO in sourceList)
                    {
                        var orgs = destList.FirstOrDefault(i => i.RowGuid == orgDTO.RowGuid);

                        var orgId = 0;
                        if (orgs == null)
                            orgs = new OrganizationDTO();
                        else
                            orgId = orgs.Id;

                        Mapper.Reset();
                        Mapper.CreateMap<OrganizationDTO, OrganizationDTO>();
                        orgs = Mapper.Map(orgDTO, orgs);
                        orgs.Id = orgId;

                        #region Foreign Keys

                        var addrDTO = destAddresses.FirstOrDefault(c => c.RowGuid == orgDTO.Address.RowGuid);
                        {
                            orgs.Address = addrDTO;
                            orgs.AddressId = addrDTO != null ? addrDTO.Id : 1;
                        }
                        var clientDTO = destClients.FirstOrDefault(c => c.RowGuid == orgDTO.Client.RowGuid);
                        {
                            orgs.Client = clientDTO;
                            orgs.ClientId = clientDTO != null ? clientDTO.Id : 1;
                        }

                        #endregion

                        destinationUnitOfWork.Repository<OrganizationDTO>().InsertUpdate(orgs);
                    }

                    destinationUnitOfWork.Commit();
                }

                #endregion
            }
            catch (Exception ex)
            {

                ServiceLog.Log(" - Organization Sync Failed :-" +
                                               Environment.NewLine + ex.Message +
                                               Environment.NewLine + ex.InnerException);
                return false;
            }
            ServiceLog.Log(" - Organization Sync Completed");
            return true;
        }
        public bool SyncWarehouses(IUnitOfWork sourceUnitOfWork, IUnitOfWork destinationUnitOfWork)
        {
            ServiceLog.Log(" - Warehouse Sync Started");
            try
            {
                #region Sync Warehouses
                var sourceList = sourceUnitOfWork.Repository<WarehouseDTO>().Query()
                                   .Include(i => i.Address, i => i.Organization)
                                   .Filter(a => a.DateLastModified > LastServerSyncDate)
                                   .Get().ToList();

                if (sourceList.Any())
                {
                    var destOrgs = destinationUnitOfWork.Repository<OrganizationDTO>().Query().Get().ToList();
                    var destAddresses = destinationUnitOfWork.Repository<AddressDTO>().Query().Get().ToList();

                    var destList = destinationUnitOfWork.Repository<WarehouseDTO>().Query()
                        .Include(i => i.Address, i => i.Organization).Get().ToList();

                    foreach (var warehouseDTO in sourceList)
                    {
                        var warehouses = destList.FirstOrDefault(i => i.RowGuid == warehouseDTO.RowGuid);

                        var warehouseId = 0;
                        if (warehouses == null)
                            warehouses = new WarehouseDTO();
                        else
                            warehouseId = warehouses.Id;

                        Mapper.Reset();
                        Mapper.CreateMap<WarehouseDTO, WarehouseDTO>();
                        warehouses = Mapper.Map(warehouseDTO, warehouses);
                        warehouses.Id = warehouseId;

                        #region Foreign Keys

                        var categoryDTO = destAddresses.FirstOrDefault(c => c.RowGuid == warehouseDTO.Address.RowGuid);
                        {
                            warehouses.Address = categoryDTO;
                            warehouses.AddressId = categoryDTO != null ? categoryDTO.Id : 1;
                        }
                        var orgDTO = destOrgs.FirstOrDefault(c => c.RowGuid == warehouseDTO.Organization.RowGuid);
                        {
                            warehouses.Organization = orgDTO;
                            warehouses.OrganizationId = orgDTO != null ? orgDTO.Id : 1;
                        }

                        #endregion

                        destinationUnitOfWork.Repository<WarehouseDTO>().InsertUpdate(warehouses);
                    }

                    destinationUnitOfWork.Commit();
                }

                #endregion
            }
            catch (Exception ex)
            {

                ServiceLog.Log(" - Warehouse Sync Failed :-" +
                                               Environment.NewLine + ex.Message +
                                               Environment.NewLine + ex.InnerException);
                return false;
            }
            ServiceLog.Log(" - Warehouse Sync Completed");
            return true;
        }
        public bool SyncBusinessPartners(IUnitOfWork sourceUnitOfWork, IUnitOfWork destinationUnitOfWork)
        {
            ServiceLog.Log(" - BusinessPartner Sync Started");

            try
            {
                #region Sync BusinessPartners
                var sourceList = sourceUnitOfWork.Repository<BusinessPartnerDTO>().Query()
                    .Include(i => i.Address)
                    .Filter(a => a.DateLastModified > LastServerSyncDate).Get().ToList();

                if (sourceList.Any())
                {
                    var destAddresses = destinationUnitOfWork.Repository<AddressDTO>().Query().Get().ToList();

                    var destList = destinationUnitOfWork.Repository<BusinessPartnerDTO>().Query()
                        .Include(i => i.Address).Get().ToList();

                    foreach (var businessPartnerDTO in sourceList)
                    {
                        var businessPartners = destList.FirstOrDefault(i => i.RowGuid == businessPartnerDTO.RowGuid);

                        var businessPartnerId = 0;
                        if (businessPartners == null)
                            businessPartners = new BusinessPartnerDTO();
                        else
                            businessPartnerId = businessPartners.Id;

                        Mapper.Reset();
                        Mapper.CreateMap<BusinessPartnerDTO, BusinessPartnerDTO>();
                        businessPartners = Mapper.Map(businessPartnerDTO, businessPartners);
                        businessPartners.Id = businessPartnerId;

                        #region Foreign Keys

                        var categoryDTO =
                            destAddresses.FirstOrDefault(c => c.RowGuid == businessPartnerDTO.Address.RowGuid);
                        {
                            businessPartners.Address = categoryDTO;
                            businessPartners.AddressId = categoryDTO != null ? categoryDTO.Id : 1;
                        }

                        #endregion

                        destinationUnitOfWork.Repository<BusinessPartnerDTO>().InsertUpdate(businessPartners);
                    }

                    destinationUnitOfWork.Commit();
                }

                #endregion
            }
            catch (Exception ex)
            {

                ServiceLog.Log(" - BusinessPartner Sync Failed :-" +
                                               Environment.NewLine + ex.Message +
                                               Environment.NewLine + ex.InnerException);
                return false;
            }
            ServiceLog.Log(" - BusinessPartner Sync Completed");
            return true;
        }
        public bool SyncFinancialAccounts(IUnitOfWork sourceUnitOfWork, IUnitOfWork destinationUnitOfWork)
        {
            ServiceLog.Log(" - FinancialAccount Sync Started");

            try
            {
                #region Sync FinancialAccounts
                var sourceList = sourceUnitOfWork.Repository<FinancialAccountDTO>().Query()
                    .Include(i => i.BusinessPartner, i => i.Warehouse)
                    .Filter(a => a.DateLastModified > LastServerSyncDate).Get().ToList();
                if (sourceList.Any())
                {
                    var destBps = destinationUnitOfWork.Repository<BusinessPartnerDTO>().Query().Get().ToList();
                    var destWarehouses = destinationUnitOfWork.Repository<WarehouseDTO>().Query().Get().ToList();

                    var destList = destinationUnitOfWork.Repository<FinancialAccountDTO>().Query()
                        .Include(i => i.BusinessPartner, i => i.Warehouse).Get().ToList();

                    foreach (var financialAccountDTO in sourceList)
                    {
                        var financialAccounts = destList.FirstOrDefault(i => i.RowGuid == financialAccountDTO.RowGuid);

                        var financialAccountsInRoleId = 0;
                        if (financialAccounts == null)
                            financialAccounts = new FinancialAccountDTO();
                        else
                            financialAccountsInRoleId = financialAccounts.Id;

                        Mapper.Reset();
                        Mapper.CreateMap<FinancialAccountDTO, FinancialAccountDTO>();
                        financialAccounts = Mapper.Map(financialAccountDTO, financialAccounts);
                        financialAccounts.Id = financialAccountsInRoleId;

                        #region Foreign Keys

                        if (financialAccountDTO.BusinessPartner != null)
                        {
                            var bpDTO = destBps.FirstOrDefault(c => c.RowGuid == financialAccountDTO.BusinessPartner.RowGuid);
                            {
                                financialAccounts.BusinessPartner = bpDTO;
                                financialAccounts.BusinessPartnerId = bpDTO != null ? bpDTO.Id : 1;
                            }
                        }

                        if (financialAccountDTO.Warehouse != null)
                        {
                            var warehouseDTO =
                             destWarehouses.FirstOrDefault(c => c.RowGuid == financialAccountDTO.Warehouse.RowGuid);
                            {
                                financialAccounts.Warehouse = warehouseDTO;
                                financialAccounts.WarehouseId = warehouseDTO != null ? warehouseDTO.Id : 1;
                            }
                        }


                        #endregion

                        destinationUnitOfWork.Repository<FinancialAccountDTO>().InsertUpdate(financialAccounts);
                    }

                    destinationUnitOfWork.Commit();
                }

                #endregion
            }
            catch (Exception ex)
            {

                ServiceLog.Log(" - FinancialAccount Sync Failed :-" +
                                               Environment.NewLine + ex.Message +
                                               Environment.NewLine + ex.InnerException);
                return false;
            }
            ServiceLog.Log(" - FinancialAccount Sync Completed");
            return true;
        }

        public bool SyncAddresss(IUnitOfWork sourceUnitOfWork, IUnitOfWork destinationUnitOfWork)
        {
            ServiceLog.Log(" - Address Sync Started");
            try
            {
                var addrs = sourceUnitOfWork.Repository<AddressDTO>().Query()
                    .Filter(a => a.DateLastModified > LastServerSyncDate).Get().ToList();
                foreach (var addr in addrs)
                {
                    destinationUnitOfWork.Repository<AddressDTO>().CrudByRowGuid(addr);
                }
                destinationUnitOfWork.Commit();
            }
            catch (Exception ex)
            {

                ServiceLog.Log(" - Address Sync Failed :-" +
                                               Environment.NewLine + ex.Message +
                                               Environment.NewLine + ex.InnerException);
                return false;
            }
            ServiceLog.Log(" - Address Sync Completed");
            return true;
        }
        public bool SyncCategories(IUnitOfWork sourceUnitOfWork, IUnitOfWork destinationUnitOfWork)
        {
            ServiceLog.Log(" - Category Sync Started");
            try
            {
                var cats = sourceUnitOfWork.Repository<CategoryDTO>().Query()
                    .Filter(a => a.DateLastModified > LastServerSyncDate).Get().ToList();
                foreach (var cat in cats)
                {
                    destinationUnitOfWork.Repository<CategoryDTO>().CrudByRowGuid(cat);
                }
                destinationUnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                ServiceLog.Log(" - Category Sync Failed :-" +
                                               Environment.NewLine + ex.Message +
                                               Environment.NewLine + ex.InnerException);
                return false;
            }
            ServiceLog.Log(" - Category Sync Completed");
            return true;
        }

        public bool SyncItems(IUnitOfWork sourceUnitOfWork, IUnitOfWork destinationUnitOfWork)
        {
            ServiceLog.Log(" - Item Sync Started");

            try
            {
                #region Sync Items

                var sourceList = sourceUnitOfWork.Repository<ItemDTO>().Query()
                    .Include(i => i.Category, i => i.UnitOfMeasure)
                    .Filter(a => a.DateLastModified > LastServerSyncDate).Get().ToList();
                if (sourceList.Any())
                {
                    var destCats = destinationUnitOfWork.Repository<CategoryDTO>().Query().Get().ToList();
                    var destList = destinationUnitOfWork.Repository<ItemDTO>().Query()
                        .Include(i => i.Category, i => i.UnitOfMeasure).Get().ToList();

                    foreach (var itemDTO in sourceList)
                    {
                        var items = destList.FirstOrDefault(i => i.RowGuid == itemDTO.RowGuid);

                        var itemId = 0;
                        if (items == null)
                            items = new ItemDTO();
                        else
                            itemId = items.Id;

                        Mapper.Reset();
                        Mapper.CreateMap<ItemDTO, ItemDTO>();
                        items = Mapper.Map(itemDTO, items);
                        items.Id = itemId;

                        #region Foreign Keys

                        var categoryDTO = destCats.FirstOrDefault(c => c.RowGuid == itemDTO.Category.RowGuid);
                        {
                            items.Category = categoryDTO;
                            items.CategoryId = categoryDTO != null ? categoryDTO.Id : 1;
                        }
                        var unitOfMeasureDto = destCats.FirstOrDefault(c => c.RowGuid == itemDTO.UnitOfMeasure.RowGuid);
                        {
                            items.UnitOfMeasure = unitOfMeasureDto;
                            items.UnitOfMeasureId = unitOfMeasureDto != null ? unitOfMeasureDto.Id : 2;
                        }

                        #endregion

                        destinationUnitOfWork.Repository<ItemDTO>().InsertUpdate(items);
                    }

                    destinationUnitOfWork.Commit();
                }

                #endregion
            }
            catch (Exception ex)
            {

                ServiceLog.Log(" - Item Sync Failed :-" +
                                               Environment.NewLine + ex.Message +
                                               Environment.NewLine + ex.InnerException);
                return false;
            }
            ServiceLog.Log(" - Item Sync Completed");
            return true;
        }
        public bool SyncItemQuantities(IUnitOfWork sourceUnitOfWork, IUnitOfWork destinationUnitOfWork)
        {
            ServiceLog.Log(" - ItemQuantity Sync Started");

            try
            {
                #region Sync ItemQuantitys

                var sourceList = sourceUnitOfWork.Repository<ItemQuantityDTO>().Query()
                    .Include(i => i.Item, i => i.Warehouse)
                    .Filter(a => a.DateLastModified > LastServerSyncDate).Get().ToList();
                if (sourceList.Any())
                {
                    var destitems = destinationUnitOfWork.Repository<ItemDTO>().Query().Get().ToList();
                    var destWarehouses = destinationUnitOfWork.Repository<WarehouseDTO>().Query().Get().ToList();

                    var destList = destinationUnitOfWork.Repository<ItemQuantityDTO>().Query()
                        .Include(i => i.Item, i => i.Warehouse).Get().ToList();

                    foreach (var itemQuantityDTO in sourceList)
                    {
                        var itemQuantitys = destList.FirstOrDefault(i => i.RowGuid == itemQuantityDTO.RowGuid);

                        var itemQuantityId = 0;
                        if (itemQuantitys == null)
                            itemQuantitys = new ItemQuantityDTO();
                        else
                            itemQuantityId = itemQuantitys.Id;

                        Mapper.Reset();
                        Mapper.CreateMap<ItemQuantityDTO, ItemQuantityDTO>();
                        itemQuantitys = Mapper.Map(itemQuantityDTO, itemQuantitys);
                        itemQuantitys.Id = itemQuantityId;

                        #region Foreign Keys

                        var itemDTO = destitems.FirstOrDefault(c => c.RowGuid == itemQuantityDTO.Item.RowGuid);
                        {
                            itemQuantitys.Item = itemDTO;
                            itemQuantitys.ItemId = itemDTO != null ? itemDTO.Id : 1;
                        }
                        var warehouseDTO =
                            destWarehouses.FirstOrDefault(c => c.RowGuid == itemQuantityDTO.Warehouse.RowGuid);
                        {
                            itemQuantitys.Warehouse = warehouseDTO;
                            itemQuantitys.WarehouseId = warehouseDTO != null ? warehouseDTO.Id : 1;
                        }

                        #endregion

                        destinationUnitOfWork.Repository<ItemQuantityDTO>().InsertUpdate(itemQuantitys);
                    }

                    destinationUnitOfWork.Commit();
                }

                #endregion
            }
            catch (Exception ex)
            {

                ServiceLog.Log(" - ItemQuantity Sync Failed :-" +
                                               Environment.NewLine + ex.Message +
                                               Environment.NewLine + ex.InnerException);
                return false;
            }
            ServiceLog.Log(" - ItemQuantity Sync Completed");
            return true;
        }

        public bool SyncTransactionHeaders(IUnitOfWork sourceUnitOfWork, IUnitOfWork destinationUnitOfWork)
        {
            ServiceLog.Log(" - TransactionHeader Sync Started");

            try
            {
                #region Sync TransactionHeaders
                var sourceList = sourceUnitOfWork.Repository<TransactionHeaderDTO>().Query()
                    .Include(i => i.Warehouse, i => i.BusinessPartner)
                    .Filter(a => a.DateLastModified > LastServerSyncDate).Get().ToList();
                if (sourceList.Any())
                {
                    var destBps = destinationUnitOfWork.Repository<BusinessPartnerDTO>().Query().Get().ToList();
                    var destWarehouses = destinationUnitOfWork.Repository<WarehouseDTO>().Query().Get().ToList();

                    var destList = destinationUnitOfWork.Repository<TransactionHeaderDTO>().Query()
                        .Include(i => i.Warehouse, i => i.BusinessPartner).Get().ToList();

                    foreach (var transactionHeaderDTO in sourceList)
                    {
                        var transactionHeaders = destList.FirstOrDefault(i => i.RowGuid == transactionHeaderDTO.RowGuid);

                        var transactionHeadersInRoleId = 0;
                        if (transactionHeaders == null)
                            transactionHeaders = new TransactionHeaderDTO();
                        else
                            transactionHeadersInRoleId = transactionHeaders.Id;

                        Mapper.Reset();
                        Mapper.CreateMap<TransactionHeaderDTO, TransactionHeaderDTO>();
                        transactionHeaders = Mapper.Map(transactionHeaderDTO, transactionHeaders);
                        transactionHeaders.Id = transactionHeadersInRoleId;

                        #region Foreign Keys

                        var warehouseDTO =
                            destWarehouses.FirstOrDefault(c => c.RowGuid == transactionHeaderDTO.Warehouse.RowGuid);
                        {
                            transactionHeaders.Warehouse = warehouseDTO;
                            transactionHeaders.WarehouseId = warehouseDTO != null ? warehouseDTO.Id : 1;
                        }
                        var bpDTO =
                            destBps.FirstOrDefault(c => c.RowGuid == transactionHeaderDTO.BusinessPartner.RowGuid);
                        {
                            transactionHeaders.BusinessPartner = bpDTO;
                            transactionHeaders.BusinessPartnerId = bpDTO != null ? bpDTO.Id : 1;
                        }

                        #endregion

                        destinationUnitOfWork.Repository<TransactionHeaderDTO>().InsertUpdate(transactionHeaders);
                    }

                    destinationUnitOfWork.Commit();
                }

                #endregion
            }
            catch (Exception ex)
            {

                ServiceLog.Log(" - TransactionHeader Sync Failed :-" +
                                               Environment.NewLine + ex.Message +
                                               Environment.NewLine + ex.InnerException);
                return false;
            }
            ServiceLog.Log(" - TransactionHeader Sync Completed");
            return true;
        }
        public bool SyncTransactionLines(IUnitOfWork sourceUnitOfWork, IUnitOfWork destinationUnitOfWork)
        {
            ServiceLog.Log(" - TransactionLine Sync Started");

            try
            {
                #region Sync TransactionLines
                var sourceList = sourceUnitOfWork.Repository<TransactionLineDTO>().Query()
                    .Include(i => i.Item, i => i.Transaction)
                    .Filter(a => a.DateLastModified > LastServerSyncDate).Get().ToList();
                if (sourceList.Any())
                {
                    var destitems = destinationUnitOfWork.Repository<ItemDTO>().Query().Get().ToList();
                    var destHeaders = destinationUnitOfWork.Repository<TransactionHeaderDTO>().Query().Get().ToList();

                    var destList = destinationUnitOfWork.Repository<TransactionLineDTO>().Query()
                        .Include(i => i.Item, i => i.Transaction).Get().ToList();

                    foreach (var transactionLineDTO in sourceList)
                    {
                        var transactionLines = destList.FirstOrDefault(i => i.RowGuid == transactionLineDTO.RowGuid);

                        var transactionLinesInRoleId = 0;
                        if (transactionLines == null)
                            transactionLines = new TransactionLineDTO();
                        else
                            transactionLinesInRoleId = transactionLines.Id;

                        Mapper.Reset();
                        Mapper.CreateMap<TransactionLineDTO, TransactionLineDTO>();
                        transactionLines = Mapper.Map(transactionLineDTO, transactionLines);
                        transactionLines.Id = transactionLinesInRoleId;

                        #region Foreign Keys

                        var itemDTO = destitems.FirstOrDefault(c => c.RowGuid == transactionLineDTO.Item.RowGuid);
                        {
                            transactionLines.Item = itemDTO;
                            transactionLines.ItemId = itemDTO != null ? itemDTO.Id : 1;
                        }
                        var transactionDTO =
                            destHeaders.FirstOrDefault(c => c.RowGuid == transactionLineDTO.Transaction.RowGuid);
                        {
                            transactionLines.Transaction = transactionDTO;
                            transactionLines.TransactionId = transactionDTO != null ? transactionDTO.Id : 1;
                        }

                        #endregion

                        destinationUnitOfWork.Repository<TransactionLineDTO>().InsertUpdate(transactionLines);
                    }

                    destinationUnitOfWork.Commit();
                }

                #endregion
            }
            catch (Exception ex)
            {

                ServiceLog.Log(" - TransactionLine Sync Failed :-" +
                                               Environment.NewLine + ex.Message +
                                               Environment.NewLine + ex.InnerException);
                return false;
            }
            ServiceLog.Log(" - TransactionLine Sync Completed");
            return true;
        }

        public bool SyncPhysicalInventoryHeaders(IUnitOfWork sourceUnitOfWork, IUnitOfWork destinationUnitOfWork)
        {
            ServiceLog.Log(" - PhysicalInventoryHeader Sync Started");

            try
            {
                #region Sync PhysicalInventoryHeaders
                var sourceList = sourceUnitOfWork.Repository<PhysicalInventoryHeaderDTO>().Query()
                    .Include(i => i.Warehouse)
                    .Filter(a => a.DateLastModified > LastServerSyncDate).Get().ToList();
                if (sourceList.Any())
                {
                    var destWarehouses = destinationUnitOfWork.Repository<WarehouseDTO>().Query().Get().ToList();

                    var destList = destinationUnitOfWork.Repository<PhysicalInventoryHeaderDTO>().Query()
                        .Include(i => i.Warehouse).Get().ToList();

                    foreach (var piHeaderDTO in sourceList)
                    {
                        var piHeaders = destList.FirstOrDefault(i => i.RowGuid == piHeaderDTO.RowGuid);

                        var piHeadersInRoleId = 0;
                        if (piHeaders == null)
                            piHeaders = new PhysicalInventoryHeaderDTO();
                        else
                            piHeadersInRoleId = piHeaders.Id;

                        Mapper.Reset();
                        Mapper.CreateMap<PhysicalInventoryHeaderDTO, PhysicalInventoryHeaderDTO>();
                        piHeaders = Mapper.Map(piHeaderDTO, piHeaders);
                        piHeaders.Id = piHeadersInRoleId;

                        #region Foreign Keys

                        var warehouseDTO = destWarehouses.FirstOrDefault(c => c.RowGuid == piHeaderDTO.Warehouse.RowGuid);
                        {
                            piHeaders.Warehouse = warehouseDTO;
                            piHeaders.WarehouseId = warehouseDTO != null ? warehouseDTO.Id : 1;
                        }

                        #endregion

                        destinationUnitOfWork.Repository<PhysicalInventoryHeaderDTO>().InsertUpdate(piHeaders);
                    }

                    destinationUnitOfWork.Commit();
                }

                #endregion
            }
            catch (Exception ex)
            {

                ServiceLog.Log(" - PhysicalInventoryHeader Sync Failed :-" +
                                               Environment.NewLine + ex.Message +
                                               Environment.NewLine + ex.InnerException);
                return false;
            }
            ServiceLog.Log(" - PhysicalInventoryHeader Sync Completed");
            return true;
        }
        public bool SyncPhysicalInventoryLines(IUnitOfWork sourceUnitOfWork, IUnitOfWork destinationUnitOfWork)
        {
            ServiceLog.Log(" - PhysicalInventoryLine Sync Started");

            try
            {
                #region Sync PhysicalInventoryLines

                var sourceList = sourceUnitOfWork.Repository<PhysicalInventoryLineDTO>().Query()
                    .Include(i => i.Item, i => i.PhysicalInventory)
                    .Filter(a => a.DateLastModified > LastServerSyncDate).Get().ToList();
                if (sourceList.Any())
                {
                    var destitems = destinationUnitOfWork.Repository<ItemDTO>().Query().Get().ToList();
                    var destHeaders = destinationUnitOfWork.Repository<PhysicalInventoryHeaderDTO>().Query().Get().ToList();
                    var destList = destinationUnitOfWork.Repository<PhysicalInventoryLineDTO>().Query()
                        .Include(i => i.Item, i => i.PhysicalInventory).Get().ToList();

                    foreach (var piLineDTO in sourceList)
                    {
                        var piLines = destList.FirstOrDefault(i => i.RowGuid == piLineDTO.RowGuid);

                        var piLinesInRoleId = 0;
                        if (piLines == null)
                            piLines = new PhysicalInventoryLineDTO();
                        else
                            piLinesInRoleId = piLines.Id;

                        Mapper.Reset();
                        Mapper.CreateMap<PhysicalInventoryLineDTO, PhysicalInventoryLineDTO>();
                        piLines = Mapper.Map(piLineDTO, piLines);
                        piLines.Id = piLinesInRoleId;

                        #region Foreign Keys

                        var itemDTO = destitems.FirstOrDefault(c => c.RowGuid == piLineDTO.Item.RowGuid);
                        {
                            piLines.Item = itemDTO;
                            piLines.ItemId = itemDTO != null ? itemDTO.Id : 1;
                        }
                        var piDTO = destHeaders.FirstOrDefault(c => c.RowGuid == piLineDTO.PhysicalInventory.RowGuid);
                        {
                            piLines.PhysicalInventory = piDTO;
                            piLines.PhysicalInventoryId = piDTO != null ? piDTO.Id : 1;
                        }

                        #endregion

                        destinationUnitOfWork.Repository<PhysicalInventoryLineDTO>().InsertUpdate(piLines);
                    }

                    destinationUnitOfWork.Commit();
                }

                #endregion
            }
            catch (Exception ex)
            {

                ServiceLog.Log(" - PhysicalInventoryLine Sync Failed :-" +
                                               Environment.NewLine + ex.Message +
                                               Environment.NewLine + ex.InnerException);
                return false;
            }
            ServiceLog.Log(" - PhysicalInventoryLine Sync Completed");
            return true;
        }

        public bool SyncCpos(IUnitOfWork sourceUnitOfWork, IUnitOfWork destinationUnitOfWork)
        {
            ServiceLog.Log(" - Cpo Sync Started");

            try
            {
                #region Sync Cpos
                var sourceList = sourceUnitOfWork.Repository<CpoDTO>().Query()
                    .Include(i => i.Warehouse).Get().ToList();
                if (sourceList.Any())
                {
                    var destWarehouses = destinationUnitOfWork.Repository<WarehouseDTO>().Query().Get().ToList();

                    var destList = destinationUnitOfWork.Repository<CpoDTO>().Query()
                        .Include(i => i.Warehouse)
                        .Filter(a => a.DateLastModified > LastServerSyncDate).Get().ToList();

                    foreach (var cpoDTO in sourceList)
                    {
                        var cpos = destList.FirstOrDefault(i => i.RowGuid == cpoDTO.RowGuid);

                        var cposInRoleId = 0;
                        if (cpos == null)
                            cpos = new CpoDTO();
                        else
                            cposInRoleId = cpos.Id;

                        Mapper.Reset();
                        Mapper.CreateMap<CpoDTO, CpoDTO>();
                        cpos = Mapper.Map(cpoDTO, cpos);
                        cpos.Id = cposInRoleId;

                        #region Foreign Keys

                        var warehouseDTO = destWarehouses.FirstOrDefault(c => c.RowGuid == cpoDTO.Warehouse.RowGuid);
                        {
                            cpos.Warehouse = warehouseDTO;
                            cpos.WarehouseId = warehouseDTO != null ? warehouseDTO.Id : 1;
                        }

                        #endregion

                        destinationUnitOfWork.Repository<CpoDTO>().InsertUpdate(cpos);
                    }

                    destinationUnitOfWork.Commit();
                }

                #endregion
            }
            catch (Exception ex)
            {

                ServiceLog.Log(" - Cpo Sync Failed :-" +
                                               Environment.NewLine + ex.Message +
                                               Environment.NewLine + ex.InnerException);
                return false;
            }
            ServiceLog.Log(" - Cpo Sync Completed");
            return true;
        }
        public bool SyncItemBorrows(IUnitOfWork sourceUnitOfWork, IUnitOfWork destinationUnitOfWork)
        {
            ServiceLog.Log(" - ItemBorrow Sync Started");

            try
            {
                #region Sync ItemBorrows
                var sourceList = sourceUnitOfWork.Repository<ItemBorrowDTO>().Query()
                    .Include(i => i.Item, i => i.Warehouse)
                    .Filter(a => a.DateLastModified > LastServerSyncDate).Get().ToList();
                if (sourceList.Any())
                {
                    var destitems = destinationUnitOfWork.Repository<ItemDTO>().Query().Get().ToList();
                    var destWarehouses = destinationUnitOfWork.Repository<WarehouseDTO>().Query().Get().ToList();

                    var destList = destinationUnitOfWork.Repository<ItemBorrowDTO>().Query()
                        .Include(i => i.Item, i => i.Warehouse).Get().ToList();

                    foreach (var itemBorrowDTO in sourceList)
                    {
                        var itemBorrows = destList.FirstOrDefault(i => i.RowGuid == itemBorrowDTO.RowGuid);

                        var itemBorrowsInRoleId = 0;
                        if (itemBorrows == null)
                            itemBorrows = new ItemBorrowDTO();
                        else
                            itemBorrowsInRoleId = itemBorrows.Id;

                        Mapper.Reset();
                        Mapper.CreateMap<ItemBorrowDTO, ItemBorrowDTO>();
                        itemBorrows = Mapper.Map(itemBorrowDTO, itemBorrows);
                        itemBorrows.Id = itemBorrowsInRoleId;

                        #region Foreign Keys

                        var itemDTO = destitems.FirstOrDefault(c => c.RowGuid == itemBorrowDTO.Item.RowGuid);
                        {
                            itemBorrows.Item = itemDTO;
                            itemBorrows.ItemId = itemDTO != null ? itemDTO.Id : 1;
                        }
                        var warehouseDTO =
                            destWarehouses.FirstOrDefault(c => c.RowGuid == itemBorrowDTO.Warehouse.RowGuid);
                        {
                            itemBorrows.Warehouse = warehouseDTO;
                            itemBorrows.WarehouseId = warehouseDTO != null ? warehouseDTO.Id : 1;
                        }

                        #endregion

                        destinationUnitOfWork.Repository<ItemBorrowDTO>().InsertUpdate(itemBorrows);
                    }

                    destinationUnitOfWork.Commit();
                }

                #endregion
            }
            catch (Exception ex)
            {

                ServiceLog.Log(" - ItemBorrow Sync Failed :-" +
                                               Environment.NewLine + ex.Message +
                                               Environment.NewLine + ex.InnerException);
                return false;
            }
            ServiceLog.Log(" - ItemBorrow Sync Completed");
            return true;
        }

        //public bool SyncUsers(IUnitOfWork sourceUnitOfWork, IUnitOfWork destinationUnitOfWork)
        //{
        //    ServiceLog.Log(" - User Sync Started");

        //    try
        //    {
        //        #region Sync Users

        //        var sourceList = sourceUnitOfWork.Repository<UserDTO>().Query()
        //            .Include(i => i.Organization, i => i.Warehouse)
        //            .Filter(a => a.DateLastModified > LastServerSyncDate).Get().ToList();
        //        if (sourceList.Any())
        //        {
        //            var destOrgs = destinationUnitOfWork.Repository<OrganizationDTO>().Query().Get().ToList();
        //            var destWarehouses = destinationUnitOfWork.Repository<WarehouseDTO>().Query().Get().ToList();

        //            var destList = destinationUnitOfWork.Repository<UserDTO>().Query()
        //                .Include(i => i.Organization, i => i.Warehouse).Get().ToList();

        //            foreach (var usersInRoleDTO in sourceList)
        //            {
        //                var usersInRoles = destList.FirstOrDefault(i => i.RowGuid == usersInRoleDTO.RowGuid);

        //                var usersInRoleId = 0;
        //                if (usersInRoles == null)
        //                    usersInRoles = new UserDTO();
        //                else
        //                    usersInRoleId = usersInRoles.Id;

        //                Mapper.Reset();
        //                Mapper.CreateMap<UserDTO, UserDTO>();
        //                usersInRoles = Mapper.Map(usersInRoleDTO, usersInRoles);
        //                usersInRoles.Id = usersInRoleId;

        //                #region Foreign Keys

        //                if (usersInRoleDTO.Organization != null)
        //                {
        //                    var orgDTO = destOrgs.FirstOrDefault(c => c.RowGuid == usersInRoleDTO.Organization.RowGuid);
        //                    {
        //                        usersInRoles.Organization = orgDTO;
        //                        usersInRoles.OrganizationId = orgDTO != null ? orgDTO.Id : 1;
        //                    }
        //                }
        //                if (usersInRoleDTO.Warehouse != null)
        //                {
        //                    var warehouseDTO =
        //                        destWarehouses.FirstOrDefault(c => c.RowGuid == usersInRoleDTO.Warehouse.RowGuid);
        //                    {
        //                        usersInRoles.Warehouse = warehouseDTO;
        //                        usersInRoles.WarehouseId = warehouseDTO != null ? warehouseDTO.Id : 1;
        //                    }
        //                }

        //                #endregion

        //                destinationUnitOfWork.Repository<UserDTO>().InsertUpdate(usersInRoles);
        //            }

        //            destinationUnitOfWork.Commit();
        //        }

        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {

        //        ServiceLog.Log(" - User Sync Failed :-" +
        //                                       Environment.NewLine + ex.Message +
        //                                       Environment.NewLine + ex.InnerException);
        //        return false;
        //    }
        //    ServiceLog.Log(" - User Sync Completed");
        //    return true;
        //}
        //public bool SyncRoles(IUnitOfWork sourceUnitOfWork, IUnitOfWork destinationUnitOfWork)
        //{
        //    ServiceLog.Log(" - Role Sync Started");
        //    try
        //    {
        //        var roles = sourceUnitOfWork.Repository<RoleDTO>().Query()
        //            .Filter(a => a.DateLastModified > LastServerSyncDate).Get().ToList();
        //        foreach (var role in roles)
        //        {
        //            destinationUnitOfWork.Repository<RoleDTO>().CrudByRowGuid(role);
        //        }
        //        destinationUnitOfWork.Commit();
        //    }
        //    catch (Exception ex)
        //    {

        //        ServiceLog.Log(" - Role Sync Failed :-" +
        //                                       Environment.NewLine + ex.Message +
        //                                       Environment.NewLine + ex.InnerException);
        //        return false;
        //    }
        //    ServiceLog.Log(" - Role Sync Completed");
        //    return true;
        //}
        //public bool SyncUsersInRoles(IUnitOfWork sourceUnitOfWork, IUnitOfWork destinationUnitOfWork)
        //{
        //    ServiceLog.Log(" - UsersInRole Sync Started");
            
        //    try
        //    {
        //        #region Sync UsersInRoles

        //        var sourceList = sourceUnitOfWork.Repository<UsersInRoles>().Query()
        //            .Include(i => i.User, i => i.Role)
        //            .Filter(a => a.DateLastModified > LastServerSyncDate).Get().ToList();
        //        if (sourceList.Any())
        //        {
        //            var destUsers = destinationUnitOfWork.Repository<UserDTO>().Query().Get().ToList();
        //            var destRoles = destinationUnitOfWork.Repository<RoleDTO>().Query().Get().ToList();

        //            var destList = destinationUnitOfWork.Repository<UsersInRoles>().Query()
        //                .Include(i => i.User, i => i.Role).Get().ToList();

        //            foreach (var usersInRolesDTO in sourceList)
        //            {
        //                var users = destList.FirstOrDefault(i => i.RowGuid == usersInRolesDTO.RowGuid);

        //                var userId = 0;
        //                if (users == null)
        //                    users = new UsersInRoles();
        //                else
        //                    userId = users.Id;

        //                Mapper.Reset();
        //                Mapper.CreateMap<UsersInRoles, UsersInRoles>();
        //                users = Mapper.Map(usersInRolesDTO, users);
        //                users.Id = userId;

        //                #region Foreign Keys

        //                var userDTO = destUsers.FirstOrDefault(c => c.RowGuid == usersInRolesDTO.User.RowGuid);
        //                {
        //                    users.User = userDTO;
        //                    users.UserId = userDTO != null ? userDTO.Id : 1;
        //                }
        //                var roleDTO = destRoles.FirstOrDefault(c => c.RowGuid == usersInRolesDTO.Role.RowGuid);
        //                {
        //                    users.Role = roleDTO;
        //                    users.RoleId = roleDTO != null ? roleDTO.Id : 1;
        //                }

        //                #endregion

        //                destinationUnitOfWork.Repository<UsersInRoles>().InsertUpdate(users);
        //            }

        //            destinationUnitOfWork.Commit();
        //        }

        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {

        //        ServiceLog.Log(" - UsersInRole Sync Failed :-" +
        //                                       Environment.NewLine + ex.Message +
        //                                       Environment.NewLine + ex.InnerException);
        //        return false;
        //    }
        //    ServiceLog.Log(" - UsersInRole Sync Completed");
        //    return true;
        //}

        public bool SyncChecks(IUnitOfWork sourceUnitOfWork, IUnitOfWork destinationUnitOfWork)
        {
            ServiceLog.Log(" - Check Sync Started");

            try
            {
                #region Sync Checks
                var sourceList = sourceUnitOfWork.Repository<CheckDTO>().Query()
                    .Include(i => i.CustomerBankAccount, i => i.ClientBankAccount)
                    .Filter(a => a.DateLastModified > LastServerSyncDate).Get().ToList();
                if (sourceList.Any())
                {
                    var destAccounts = destinationUnitOfWork.Repository<FinancialAccountDTO>().Query().Get().ToList();

                    var destList = destinationUnitOfWork.Repository<CheckDTO>().Query()
                        .Include(i => i.CustomerBankAccount, i => i.ClientBankAccount).Get().ToList();

                    foreach (var checkDTO in sourceList)
                    {
                        var checks = destList.FirstOrDefault(i => i.RowGuid == checkDTO.RowGuid);

                        var checkId = 0;
                        if (checks == null)
                            checks = new CheckDTO();
                        else
                            checkId = checks.Id;

                        Mapper.Reset();
                        Mapper.CreateMap<CheckDTO, CheckDTO>();
                        checks = Mapper.Map(checkDTO, checks);
                        checks.Id = checkId;

                        #region Foreign Keys

                        var custAccountDTO =
                            destAccounts.FirstOrDefault(c => c.RowGuid == checkDTO.CustomerBankAccount.RowGuid);
                        {
                            checks.CustomerBankAccount = custAccountDTO;
                            checks.CustomerBankAccountId = custAccountDTO != null ? custAccountDTO.Id : 1;
                        }
                        var clientAccountDto =
                            destAccounts.FirstOrDefault(c => c.RowGuid == checkDTO.ClientBankAccount.RowGuid);
                        {
                            checks.ClientBankAccount = clientAccountDto;
                            checks.ClientBankAccountId = clientAccountDto != null ? clientAccountDto.Id : 2;
                        }

                        #endregion

                        destinationUnitOfWork.Repository<CheckDTO>().InsertUpdate(checks);
                    }

                    destinationUnitOfWork.Commit();
                }

                #endregion
            }
            catch (Exception ex)
            {

                ServiceLog.Log(" - Check Sync Failed :-" +
                                               Environment.NewLine + ex.Message +
                                               Environment.NewLine + ex.InnerException);
                return false;
            }
            ServiceLog.Log(" - Check Sync Completed");
            return true;
        }
        //public bool SyncClearances(IUnitOfWork sourceUnitOfWork, IUnitOfWork destinationUnitOfWork)
        //{
        //    ServiceLog.Log(" - PaymentClearance Sync Started");

        //    try
        //    {
        //        #region Sync PaymentClearances
        //        var sourceList = sourceUnitOfWork.Repository<PaymentClearanceDTO>().Query()
        //            .Include(i => i.ClientAccount)//, i => i.DepositedBy, i => i.ClearedBy
        //            .Filter(a => a.DateLastModified > LastServerSyncDate).Get().ToList();
        //        if (sourceList.Any())
        //        {
        //            var destUsers = destinationUnitOfWork.Repository<UserDTO>().Query().Get().ToList();
        //            var destAccounts = destinationUnitOfWork.Repository<FinancialAccountDTO>().Query().Get().ToList();

        //            var destList = destinationUnitOfWork.Repository<PaymentClearanceDTO>().Query()
        //                .Include(i => i.ClientAccount).Get().ToList();//, i => i.DepositedBy, i => i.ClearedBy

        //            foreach (var paymentClearanceDTO in sourceList)
        //            {
        //                var clearances = destList.FirstOrDefault(i => i.RowGuid == paymentClearanceDTO.RowGuid);

        //                var checkId = 0;
        //                if (clearances == null)
        //                    clearances = new PaymentClearanceDTO();
        //                else
        //                    checkId = clearances.Id;

        //                Mapper.Reset();
        //                Mapper.CreateMap<PaymentClearanceDTO, PaymentClearanceDTO>();
        //                clearances = Mapper.Map(paymentClearanceDTO, clearances);
        //                clearances.Id = checkId;

        //                #region Foreign Keys

        //                //var depositedByDTO =
        //                //    destUsers.FirstOrDefault(c => c.RowGuid == paymentClearanceDTO.DepositedBy.RowGuid);
        //                //{
        //                //    clearances.DepositedBy = depositedByDTO;
        //                //    clearances.DepositedById = depositedByDTO != null ? depositedByDTO.Id : 1;
        //                //}
        //                //var clearedByDto =
        //                //    destUsers.FirstOrDefault(c => c.RowGuid == paymentClearanceDTO.ClearedBy.RowGuid);
        //                //{
        //                //    clearances.ClearedBy = clearedByDto;
        //                //    clearances.ClearedById = clearedByDto != null ? clearedByDto.Id : 2;
        //                //}
        //                var clientAccountDTO =
        //                    destAccounts.FirstOrDefault(c => c.RowGuid == paymentClearanceDTO.ClientAccount.RowGuid);
        //                {
        //                    clearances.ClientAccount = clientAccountDTO;
        //                    clearances.ClientAccountId = clientAccountDTO != null ? clientAccountDTO.Id : 1;
        //                }

        //                #endregion

        //                destinationUnitOfWork.Repository<PaymentClearanceDTO>().InsertUpdate(clearances);
        //            }

        //            destinationUnitOfWork.Commit();
        //        }

        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {

        //        ServiceLog.Log(" - PaymentClearance Sync Failed :-" +
        //                                       Environment.NewLine + ex.Message +
        //                                       Environment.NewLine + ex.InnerException);
        //        return false;
        //    }
        //    ServiceLog.Log(" - PaymentClearance Sync Completed");
        //    return true;
        //}
        public bool SyncPayments(IUnitOfWork sourceUnitOfWork, IUnitOfWork destinationUnitOfWork)
        {
            ServiceLog.Log(" - Payment Sync Started");

            try
            {
                #region Sync Payments

                var sourceList = sourceUnitOfWork.Repository<PaymentDTO>().Query()
                    .Include(i => i.Warehouse, i => i.Transaction, i => i.Clearance, i => i.Check)
                    .Filter(a => a.DateLastModified > LastServerSyncDate).Get().ToList();
                if (sourceList.Any())
                {
                    var destWarehouses = destinationUnitOfWork.Repository<WarehouseDTO>().Query().Get().ToList();
                    var destHeaders = destinationUnitOfWork.Repository<TransactionHeaderDTO>().Query().Get().ToList();
                    var destclearances = destinationUnitOfWork.Repository<PaymentClearanceDTO>().Query().Get().ToList();
                    var destChecks = destinationUnitOfWork.Repository<CheckDTO>().Query().Get().ToList();

                    var destList = destinationUnitOfWork.Repository<PaymentDTO>().Query()
                        .Include(i => i.Warehouse, i => i.Transaction, i => i.Clearance, i => i.Check).Get()
                        .ToList();

                    foreach (var paymentDTO in sourceList)
                    {
                        var payments = destList.FirstOrDefault(i => i.RowGuid == paymentDTO.RowGuid);

                        var paymentId = 0;
                        if (payments == null)
                            payments = new PaymentDTO();
                        else
                            paymentId = payments.Id;

                        Mapper.Reset();
                        Mapper.CreateMap<PaymentDTO, PaymentDTO>();
                        payments = Mapper.Map(paymentDTO, payments);
                        payments.Id = paymentId;

                        #region Foreign Keys

                        var warehouseDTO = destWarehouses.FirstOrDefault(c => c.RowGuid == paymentDTO.Warehouse.RowGuid);
                        {
                            payments.Warehouse = warehouseDTO;
                            payments.WarehouseId = warehouseDTO != null ? warehouseDTO.Id : 1;
                        }
                        if (paymentDTO.Transaction != null)
                        {
                            var transactionDTO =
                                destHeaders.FirstOrDefault(c => c.RowGuid == paymentDTO.Transaction.RowGuid);
                            {
                                payments.Transaction = transactionDTO;
                                payments.TransactionId = transactionDTO != null ? transactionDTO.Id : 1;
                            }
                        }
                        if (paymentDTO.Clearance != null)
                        {
                            var clrDTO = destclearances.FirstOrDefault(c => c.RowGuid == paymentDTO.Clearance.RowGuid);
                            {
                                payments.Clearance = clrDTO;
                                payments.ClearanceId = clrDTO != null ? clrDTO.Id : 1;
                            }
                        }
                        if (paymentDTO.Check != null)
                        {
                            var checkDto = destChecks.FirstOrDefault(c => c.RowGuid == paymentDTO.Check.RowGuid);
                            {
                                payments.Check = checkDto;
                                payments.CheckId = checkDto != null ? checkDto.Id : 2;
                            }
                        }

                        #endregion

                        destinationUnitOfWork.Repository<PaymentDTO>().InsertUpdate(payments);
                    }

                    destinationUnitOfWork.Commit();
                }

                #endregion
            }
            catch (Exception ex)
            {

                ServiceLog.Log(" - Payment Sync Failed :-" +
                                               Environment.NewLine + ex.Message +
                                               Environment.NewLine + ex.InnerException);
                return false;
            }
            ServiceLog.Log(" - Payment Sync Completed");
            return true;
        }
        #endregion

    }
}
