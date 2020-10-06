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
    public class OrganizationService : IOrganizationService
    {
        #region Fields
        private IUnitOfWork _unitOfWork;
        private IRepository<OrganizationDTO> _organizationRepository;
        private readonly bool _disposeWhenDone;
        #endregion

        #region Constructor
        public OrganizationService()
        {
           InitializeDbContext();
        }

        public OrganizationService(bool disposeWhenDone)
        {
            _disposeWhenDone = disposeWhenDone;
            InitializeDbContext();
        }

        public void InitializeDbContext()
        {
            var iDbContext = DbContextUtil.GetDbContextInstance();
            _organizationRepository = new Repository<OrganizationDTO>(iDbContext);
            _unitOfWork = new UnitOfWork(iDbContext);
        }

        #endregion

        #region Common Methods
        public IRepositoryQuery<OrganizationDTO> Get()
        {
            var piList = _organizationRepository
                .Query()
                .Include(a => a.Address, a => a.Warehouses)
                .Filter(a => !string.IsNullOrEmpty(a.DisplayName))
                .OrderBy(q => q.OrderBy(c => c.DisplayName));
            return piList;
        }

        public IEnumerable<OrganizationDTO> GetAll(SearchCriteria<OrganizationDTO> criteria = null)
        {
            IEnumerable<OrganizationDTO> piList;
            try
            {
                piList = Get().Get().ToList();
            }
            finally
            {
                Dispose(_disposeWhenDone);
            }

            return piList;
          
        }

        public OrganizationDTO Find(string organizationId)
        {
            var orgDto = _organizationRepository.FindById(Convert.ToInt32(organizationId));
            if(_disposeWhenDone)
                Dispose();
            return orgDto;
        }

        public OrganizationDTO GetByName(string displayName)
        {
            var orgDto = _organizationRepository.Query().Filter(c => c.DisplayName == displayName).Get().FirstOrDefault();
            if (_disposeWhenDone)
                Dispose();
            return orgDto;
        }

        public string InsertOrUpdate(OrganizationDTO organization)
        {
            try
            {
                var validate = Validate(organization);
                if (!string.IsNullOrEmpty(validate))
                    return validate;

                if (ObjectExists(organization))
                    return GenericMessages.DatabaseErrorRecordAlreadyExists;

                _organizationRepository.InsertUpdate(organization);
                _unitOfWork.Commit();
                return string.Empty;
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        public string Disable(OrganizationDTO organization)
        {
            if (organization == null)
                return GenericMessages.ObjectIsNull;

            string stat;
            try
            {
                _organizationRepository.Update(organization);
                _unitOfWork.Commit();
                stat = string.Empty;
            }
            catch (Exception exception)
            {
                stat = exception.Message;
            }
            return stat;
        }

        public int Delete(string organizationId)
        {
            try
            {
                _organizationRepository.Delete(organizationId);
                _unitOfWork.Commit();
                return 0;
            }
            catch (Exception exception)
            {
                return -1;
            }
        }

        public bool ObjectExists(OrganizationDTO organization)
        {
            var objectExists = false;
            var iDbContext = DbContextUtil.GetDbContextInstance();
            try
            {
                var catRepository = new Repository<OrganizationDTO>(iDbContext);
                var catExists = catRepository.Query()
                    .Filter(bp => bp.DisplayName == organization.DisplayName && bp.Id != organization.Id)
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

        public string Validate(OrganizationDTO organization)
        {
            if (null == organization)
                return GenericMessages.ObjectIsNull;

            if (organization.Address == null)
                return "Address " + GenericMessages.ObjectIsNull;

            if (organization.ClientId == 0)
                return "Client Data is null ";

            if (String.IsNullOrEmpty(organization.DisplayName))
                return organization.DisplayName + " " + GenericMessages.StringIsNullOrEmpty;

            if (organization.DisplayName.Length > 255)
                return organization.DisplayName + " can not be more than 255 characters ";

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