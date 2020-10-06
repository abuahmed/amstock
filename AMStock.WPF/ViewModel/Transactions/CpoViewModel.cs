using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AMStock.Core;
using AMStock.Core.Common;
using AMStock.Core.Models;
using AMStock.Service;
using AMStock.Service.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace AMStock.WPF.ViewModel
{
    public class CpoViewModel : ViewModelBase
    {
        #region Fields
        private static ICpoService _cpoService;
        private CpoDTO _selectedCpo;
        private IEnumerable<CpoDTO> _cpoList;
        private ObservableCollection<CpoDTO> _cpos;
        private int _totalNumberOfCpos;
        private string _totalValueOfCpos, _totalLeftValueOfCpos;
        private bool _addNewCpoCommandVisibility, _saveCpoCommandVisibility;
        private ICommand _refreshWindowCommand;
        private ICommand _addNewCpoCommand, _saveCpoCommand, _deleteCpoCommand;

        #endregion

        #region Constructor
        public CpoViewModel()
        {
            FilterStartDate = DateTime.Now.AddYears(-2);
            FilterEndDate = DateTime.Now.AddYears(1);

            FillCpoTypesCombo();

            GetWarehouses();

            Load();
        }

        public void Load()
        {
            CleanUp();
            _cpoService = new CpoService();

            if (Warehouses != null && Warehouses.Any())
            {
                if (SelectedWarehouse == null)
                    SelectedWarehouse = Warehouses.FirstOrDefault(w => w.IsDefault) ?? Warehouses.FirstOrDefault();
                else
                    SelectedWarehouse = SelectedWarehouse;
            }
        }

        public static void CleanUp()
        {
            if (_cpoService != null)
                _cpoService.Dispose();
        }

        public ICommand RefreshWindowCommand
        {
            get
            {
                return _refreshWindowCommand ?? (_refreshWindowCommand = new RelayCommand(Load));
            }
        }
        #endregion

        #region Public Properties
        public int TotalNumberOfCpos
        {
            get { return _totalNumberOfCpos; }
            set
            {
                _totalNumberOfCpos = value;
                RaisePropertyChanged<int>(() => TotalNumberOfCpos);
            }
        }
        public string TotalValueOfCpos
        {
            get { return _totalValueOfCpos; }
            set
            {
                _totalValueOfCpos = value;
                RaisePropertyChanged<string>(() => TotalValueOfCpos);
            }
        }
        public string TotalLeftValueOfCpos
        {
            get { return _totalLeftValueOfCpos; }
            set
            {
                _totalLeftValueOfCpos = value;
                RaisePropertyChanged<string>(() => TotalLeftValueOfCpos);
            }
        }
        public bool AddNewCpoCommandVisibility
        {
            get { return _addNewCpoCommandVisibility; }
            set
            {
                _addNewCpoCommandVisibility = value;
                RaisePropertyChanged<bool>(() => AddNewCpoCommandVisibility);
            }
        }
        public bool SaveCpoCommandVisibility
        {
            get { return _saveCpoCommandVisibility; }
            set
            {
                _saveCpoCommandVisibility = value;
                RaisePropertyChanged<bool>(() => SaveCpoCommandVisibility);
            }
        }

        public CpoDTO SelectedCpo
        {
            get { return _selectedCpo; }
            set
            {
                _selectedCpo = value;
                RaisePropertyChanged<CpoDTO>(() => SelectedCpo);
            }
        }
        public ObservableCollection<CpoDTO> Cpos
        {
            get { return _cpos; }
            set
            {
                _cpos = value;
                RaisePropertyChanged<ObservableCollection<CpoDTO>>(() => Cpos);

                if (Cpos != null)
                {
                    //if (Cpos.Count > 0)
                    //    SelectedCpo = Cpos.FirstOrDefault();
                    //else
                        ExcuteAddNewCpoCommand();

                    TotalNumberOfCpos = Cpos.Count;
                    TotalValueOfCpos = Cpos.Sum(cp => cp.Amount).ToString();
                    TotalLeftValueOfCpos = "";

                    try
                    {
                        var criteria = new SearchCriteria<BankGuaranteeDTO>()
                        {
                            CurrentUserId = Singleton.User.UserId,
                        };
                        var sum = new BankGuaranteeService(true).GetAll(criteria).Sum(bg => bg.GuaranteedAmount);
                        if (sum != null)
                        {
                            var totalGuaranteed = (decimal) sum;
                            totalGuaranteed = (decimal) (totalGuaranteed - Cpos.Where(c=>!c.IsReturned).Sum(cp => cp.Amount));

                            TotalLeftValueOfCpos = totalGuaranteed.ToString("C");
                        }
                    }
                    catch
                    {}
                }
            }
        }
        public IEnumerable<CpoDTO> CpoList
        {
            get { return _cpoList; }
            set
            {
                _cpoList = value;
                RaisePropertyChanged<IEnumerable<CpoDTO>>(() => CpoList);
            }
        }
        #endregion

        #region Commands
        public ICommand AddNewCpoCommand
        {
            get
            {
                return _addNewCpoCommand ?? (_addNewCpoCommand = new RelayCommand(ExcuteAddNewCpoCommand));
            }
        }
        private void ExcuteAddNewCpoCommand()
        {
            if (SelectedWarehouse != null && SelectedWarehouse.Id != -1)
            {
                SelectedCpo = new CpoDTO
                {
                    PreparedDate = DateTime.Now,
                    IsReturned = false,
                    WarehouseId = SelectedWarehouse.Id
                };
                AddNewCpoCommandVisibility = true;
                SaveCpoCommandVisibility = true;
            }
        }

        public ICommand SaveCpoCommand
        {
            get
            {
                return _saveCpoCommand ?? (_saveCpoCommand = new RelayCommand(ExcuteSaveCpoCommand, CanSave));
            }
        }
        private void ExcuteSaveCpoCommand()
        {
            try
            {
                //var newObject = SelectedCpo.Id;

                var stat = _cpoService.InsertOrUpdate(SelectedCpo);
                if (stat != string.Empty)
                    MessageBox.Show("Can't save"
                                    + Environment.NewLine + stat, "Can't save", MessageBoxButton.OK,
                        MessageBoxImage.Error);

                else Load();
                    //if (newObject == 0)
                    //Cpos.Insert(0, SelectedCpo);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't save"
                                  + Environment.NewLine + exception.Message, "Can't save", MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }
        }

        public ICommand DeleteCpoCommand
        {
            get
            {
                return _deleteCpoCommand ?? (_deleteCpoCommand = new RelayCommand(ExcuteDeleteCpoCommand, CanSave));
            }
        }
        private void ExcuteDeleteCpoCommand()
        {
            if (MessageBox.Show("Are you Sure You want to Delete this Cpo?", "Delete Cpo", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    if (SelectedCpo != null)
                    {
                        SelectedCpo.Enabled = false;
                        var stat = _cpoService.Disable(SelectedCpo);
                        if (stat == string.Empty)
                        {
                            Cpos.Remove(SelectedCpo);
                            GetLiveCpos();
                        }
                        else
                        {
                            MessageBox.Show("Can't Delete, may be the data is already in use..."
                                            + Environment.NewLine + stat, "Can't Delete",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Can't Delete, may be the data is already in use..."
                         + Environment.NewLine + ex.Message + Environment.NewLine + ex.InnerException, "Can't Delete",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        #endregion

        public void GetLiveCpos()
        {
            CpoList = new List<CpoDTO>();
            var criteria = new SearchCriteria<CpoDTO>()
            {
                CurrentUserId = Singleton.User.UserId,
                BeginingDate = FilterStartDate,
                EndingDate = FilterEndDate
            };

            if (SelectedWarehouse != null && SelectedWarehouse.Id != -1)
                criteria.SelectedWarehouseId = SelectedWarehouse.Id;

            if (!string.IsNullOrEmpty(FilterByPerson))
                criteria.FiList.Add(pi => pi.ToCompany.Contains(FilterByPerson));

            if (!string.IsNullOrEmpty(FilterByReason))
                criteria.FiList.Add(pi => pi.Number.Contains(FilterByReason));

            if (SelectedCpoType != null)
                criteria.TransactionType = SelectedCpoType.Value;

            CpoList = _cpoService.GetAll(criteria);

            Cpos = new ObservableCollection<CpoDTO>(CpoList.OrderByDescending(p => p.Id));
        }

        #region Warehouse
        private IEnumerable<WarehouseDTO> _warehouses;
        private WarehouseDTO _selectedWarehouse;

        public IEnumerable<WarehouseDTO> Warehouses
        {
            get { return _warehouses; }
            set
            {
                _warehouses = value;
                RaisePropertyChanged<IEnumerable<WarehouseDTO>>(() => Warehouses);
            }
        }
        public WarehouseDTO SelectedWarehouse
        {
            get { return _selectedWarehouse; }
            set
            {
                _selectedWarehouse = value;
                RaisePropertyChanged<WarehouseDTO>(() => SelectedWarehouse);
                if (SelectedWarehouse != null)
                {
                    AddNewCpoCommandVisibility = SelectedWarehouse.Id != -1;
                    SaveCpoCommandVisibility = SelectedWarehouse.Id != -1;
                    GetLiveCpos();
                }
            }
        }
        public void GetWarehouses()
        {
            Warehouses = Singleton.WarehousesList;
        }
        #endregion

        #region Filter Header

        private DateTime _filterStartDate, _filterEndDate;
        private string _filterByPerson, _filterByReason;
        private List<ListDataItem> _cpoTypes;
        private ListDataItem _selectedCpoType;
        public DateTime FilterStartDate
        {
            get { return _filterStartDate; }
            set
            {
                _filterStartDate = value;
                RaisePropertyChanged<DateTime>(() => FilterStartDate);
            }
        }
        public DateTime FilterEndDate
        {
            get { return _filterEndDate; }
            set
            {
                _filterEndDate = value;
                RaisePropertyChanged<DateTime>(() => FilterEndDate);
            }
        }

        public string FilterByPerson
        {
            get { return _filterByPerson; }
            set
            {
                _filterByPerson = value;
                RaisePropertyChanged<string>(() => FilterByPerson);
                GetLiveCpos();
            }
        }
        public string FilterByReason
        {
            get { return _filterByReason; }
            set
            {
                _filterByReason = value;
                RaisePropertyChanged<string>(() => FilterByReason);
                GetLiveCpos();
            }
        }

        public List<ListDataItem> CpoTypes
        {
            get { return _cpoTypes; }
            set
            {
                _cpoTypes = value;
                RaisePropertyChanged<List<ListDataItem>>(() => CpoTypes);
            }
        }
        public ListDataItem SelectedCpoType
        {
            get { return _selectedCpoType; }
            set
            {
                _selectedCpoType = value;
                RaisePropertyChanged<ListDataItem>(() => SelectedCpoType);
                GetLiveCpos();

            }
        }
        private void FillCpoTypesCombo()
        {
            CpoTypes = new List<ListDataItem>
            {
                new ListDataItem {Display = "All", Value = 0},
                new ListDataItem {Display = "Not-Returned", Value = 1},
                new ListDataItem {Display = "Returned", Value = 2}
            };
        }

        #endregion

        #region Validation
        public static int Errors { get; set; }
        public bool CanSave()
        {
            return Errors == 0;
        }
        #endregion
    }
}
