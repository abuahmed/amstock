#region MyRegion
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AMStock.Core;
using AMStock.Core.Common;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using AMStock.Service;
using AMStock.Service.Interfaces;
using AMStock.WPF.Models;
using AMStock.WPF.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
#endregion

namespace AMStock.WPF.ViewModel
{
    public class ItemDetailViewModel : ViewModelBase
    {
        #region Fields

        private static IItemService _itemService;
        private static IItemQuantityService _itemQuantityService;
        private IEnumerable<ItemDTO> _itemsList;
        private ObservableCollection<ItemDTO> _items;
        private ObservableCollection<TransactionLineDTO> _transactionLines;
        private ItemDTO _currentItem;
        private ItemQuantityDTO _selectedItemQuantity, _selectedItemQuantityOld;
        private WarehouseDTO _selectedWarehouse;
        private TransactionHeaderDTO _selectedTransaction;
        private TransactionLineModel _transactionLine;
        private ICommand _saveItemViewCommand, _closeItemViewCommand,_itemCodeTextChangedEvent,
            _addNewCategoryCommand, _addNewUomCommand;
        private ObservableCollection<CategoryDTO> _categories, _unitOfMeasure;
        private string _quantityEditVisibility, _itemsListVisibility;
        private decimal _currentQuantity, _itemPreviousQty;
        private decimal? _transactionQuantity;
        #endregion

        #region Constructor
        public ItemDetailViewModel()
        {
            CleanUp();
            _itemService = new ItemService();
            _itemQuantityService = new ItemQuantityService();

            CheckRoles();

            LoadItems();
            LoadCategories();
            SelectedCategory = Categories.FirstOrDefault();
            LoadUoMs();
            SelectedUnitOfMeasure = UnitOfMeasures.FirstOrDefault();

            Messenger.Default.Register<ItemQuantityDTO>(this, (message) =>
            {
                if (message != null)
                {
                    SelectedItemQuantityOld = _itemQuantityService.Find(message.Id.ToString()) ??
                                              new ItemQuantityDTO()
                    {
                        QuantityOnHand = 0,
                        ItemId = message.ItemId,
                        WarehouseId = message.WarehouseId
                    };
                }

            });

            Messenger.Default.Register<WarehouseDTO>(this, (message) =>
            {
                SelectedWarehouse = message;
            });

            Messenger.Default.Register<ObservableCollection<TransactionLineDTO>>(this, (message) =>
            {
                TransactionLines = message;
            });
            Messenger.Default.Register<TransactionHeaderDTO>(this, (message) =>
            {
                SelectedTransaction = message;
            });


            if (_currentItem == null)
            {
                _currentItem = GetNewCurrentItem();
                _itemPreviousQty = 0;
            }
            QuantityEditVisibility = "Collapsed";
            ItemsListVisibility = "Collapsed";
            
        }

        public static void CleanUp()
        {
            if (_itemService != null)
                _itemService.Dispose();
            if (_itemQuantityService != null)
                _itemQuantityService.Dispose();
        }
        #endregion

        #region Properties
        public ObservableCollection<ItemDTO> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged<ObservableCollection<ItemDTO>>(() => Items);

                if (Items != null && Items.Count > 0)
                    ItemsListVisibility = "Visible";
                else
                    ItemsListVisibility = "Collapsed";
            }
        }
        public IEnumerable<ItemDTO> ItemsList
        {
            get { return _itemsList; }
            set
            {
                _itemsList = value;
                RaisePropertyChanged<IEnumerable<ItemDTO>>(() => ItemsList);
            }
        }
        public ItemDTO CurrentItem
        {
            get { return _currentItem; }
            set
            {
                _currentItem = value;
                RaisePropertyChanged<ItemDTO>(() => CurrentItem);
                if (CurrentItem != null)
                {
                    SelectedCategory = Categories.FirstOrDefault(c => c.Id == CurrentItem.CategoryId);
                    SelectedUnitOfMeasure = UnitOfMeasures.FirstOrDefault(c => c.Id == CurrentItem.UnitOfMeasureId);
                }
            }
        }
        public ItemQuantityDTO SelectedItemQuantityOld
        {
            get { return _selectedItemQuantityOld; }
            set
            {
                _selectedItemQuantityOld = value;
                RaisePropertyChanged<ItemQuantityDTO>(() => SelectedItemQuantityOld);
                if (SelectedItemQuantityOld != null)
                {
                    SelectedItemQuantity = SelectedItemQuantityOld;
                }
            }
        }
        public ItemQuantityDTO SelectedItemQuantity
        {
            get { return _selectedItemQuantity; }
            set
            {
                _selectedItemQuantity = value;
                RaisePropertyChanged<ItemQuantityDTO>(() => SelectedItemQuantity);
                if (SelectedItemQuantity != null)
                {
                    _itemPreviousQty = SelectedItemQuantity.QuantityOnHand;
                    CurrentItem = _itemService.Find(SelectedItemQuantity.ItemId.ToString());
                    CurrentQuantity = SelectedItemQuantity.QuantityOnHand;
                }
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
                    //if (SelectedWarehouse.DisplayName != "All")
                    //{
                    //    QuantityEditVisibility = "Visible";
                    //}
                }
            }
        }
        public TransactionHeaderDTO SelectedTransaction
        {
            get { return _selectedTransaction; }
            set
            {
                _selectedTransaction = value;
                RaisePropertyChanged<TransactionHeaderDTO>(() => SelectedTransaction);
                QuantityEditVisibility = "Collapsed";
                if (SelectedTransaction != null)
                {
                    QuantityEditVisibility = "Visible";
                    TransactionLine=new TransactionLineModel();
                }
            }
        }
        public ObservableCollection<TransactionLineDTO> TransactionLines
        {
            get { return _transactionLines; }
            set
            {
                _transactionLines = value;
                RaisePropertyChanged<ObservableCollection<TransactionLineDTO>>(() => TransactionLines);

                if (TransactionLines != null)
                {
                    
                }
            }
        }

        public TransactionLineModel TransactionLine
        {
            get { return _transactionLine; }
            set
            {
                _transactionLine = value;
                RaisePropertyChanged<TransactionLineModel>(() => TransactionLine);
            }
        }
        public string QuantityEditVisibility
        {
            get { return _quantityEditVisibility; }
            set
            {
                _quantityEditVisibility = value;
                RaisePropertyChanged<string>(() => QuantityEditVisibility);
            }
        }
        public string ItemsListVisibility
        {
            get { return _itemsListVisibility; }
            set
            {
                _itemsListVisibility = value;
                RaisePropertyChanged<string>(() => ItemsListVisibility);
            }
        }
        public decimal CurrentQuantity
        {
            get { return _currentQuantity; }
            set
            {
                _currentQuantity = value;
                RaisePropertyChanged<decimal>(() => CurrentQuantity);
            }
        }
        //public decimal? TransactionQuantity
        //{
        //    get { return _transactionQuantity; }
        //    set
        //    {
        //        _transactionQuantity = value;
        //        RaisePropertyChanged<decimal?>(() => TransactionQuantity);
        //    }
        //}

        #endregion

        #region Commands
        public ICommand SaveCloseItemViewCommand
        {
            get { return _saveItemViewCommand ?? (_saveItemViewCommand = new RelayCommand<Object>(ExecuteSaveItemViewCommand, CanSave)); }
        }
        private void ExecuteSaveItemViewCommand(object obj)
        {
            try
            {
                CurrentItem.CategoryId = SelectedCategory.Id;
                CurrentItem.UnitOfMeasureId = SelectedUnitOfMeasure.Id;

                var stat = _itemService.InsertOrUpdate(CurrentItem);
                if (stat == string.Empty)
                {
                    if (SelectedTransaction != null)
                    {
                        //if (TransactionQuantity == null || (TransactionQuantity!=null && TransactionQuantity<=0))
                        //{
                        //    MessageBox.Show("Quantity Can't Be Empty and Less than or equalzero");
                        //    return;
                        //}
                             
                        var selectedTransactionLine = new TransactionLineDTO()
                        {
                            TransactionId = SelectedTransaction.Id
                        };

                        if (SelectedTransaction.Id == 0)
                            selectedTransactionLine.Transaction = SelectedTransaction;

                        selectedTransactionLine.ItemId = CurrentItem.Id;
                        selectedTransactionLine.Unit = (decimal) TransactionLine.UnitQuantity;// TransactionQuantity;
                        selectedTransactionLine.EachPrice = 0;
                        stat = new TransactionService(true).InsertOrUpdateChild(selectedTransactionLine);
                        if (string.IsNullOrEmpty(stat))
                        {
                            selectedTransactionLine.Item=new ItemDTO()
                            {
                                ItemCode = CurrentItem.ItemCode,
                                Id=CurrentItem.Id
                            };
                            TransactionLines.Add(selectedTransactionLine);
                        }
                            
                    }

                    #region Change Item Qty after adding a new PI

                    //if (QuantityEditVisibility != null && QuantityEditVisibility == "Visible" &&
                    //    _itemPreviousQty != CurrentQuantity && SelectedWarehouse != null && SelectedWarehouse.Id != -1)
                    //{
                    //    var itemQty = new ItemQuantityDTO
                    //    {
                    //        WarehouseId = SelectedWarehouse.Id,
                    //        ItemId = CurrentItem.Id,
                    //        QuantityOnHand = CurrentQuantity
                    //    };
                    //    var stat2 = _itemQuantityService.InsertOrUpdate(itemQty, true);

                    //    if (stat2 == string.Empty)
                    //        CloseWindow(obj);
                    //    else
                    //        MessageBox.Show(
                    //            "item detail saved successfully but updating quantity failed, try again..." +
                    //            Environment.NewLine + stat2, "save error", MessageBoxButton.OK, MessageBoxImage.Error);
                    //}
                    //else
                    //{
                    //    CloseWindow(obj);
                    //}
                    #endregion

                    CurrentItem = GetNewCurrentItem();
                    TransactionLine = new TransactionLineModel();
                    ItemsListVisibility = "Collapsed";
                    _itemPreviousQty = 0;
                }
                else
                    MessageBox.Show("Got Problem while saving item, try again..." + Environment.NewLine + stat, "save error", MessageBoxButton.OK,
                       MessageBoxImage.Error);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Problem saving Item..." +
                            Environment.NewLine + exception.Message +
                            Environment.NewLine + exception.InnerException);
            }
        }

        public ICommand CloseItemViewCommand
        {
            get
            {
                return _closeItemViewCommand ?? (_closeItemViewCommand = new RelayCommand<Object>(CloseWindow));
            }
        }
        private void CloseWindow(object obj)
        {
            if (obj != null)
            {
                var window = obj as Window;
                if (window != null)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
        }

        public ICommand ItemCodeTextChangedEvent
        {
            get
            {
                return _itemCodeTextChangedEvent ?? (_itemCodeTextChangedEvent = new RelayCommand<Object>(ItemCodeChanged));
            }
        }
        private void ItemCodeChanged(object obj)
        {
            var text = obj as TextBox;
            var itemsList = ItemsList.Where(it => text != null && it.ItemCode.ToLower().Contains(text.Text.ToLower())).ToList();
            Items = new ObservableCollection<ItemDTO>(itemsList);
        }
        #endregion

        public ItemDTO GetNewCurrentItem()
        {
            var currentItem = new ItemDTO
            {
                ItemCode = _itemService.GetItemCode(),
                TotalCurrentQty = 0,
                PurchasePrice = 0,
                SellPrice = 0,
                SafeQuantity = 10,
                CategoryId = Categories.FirstOrDefault().Id,// SelectedCategory.Id,
                UnitOfMeasureId =UnitOfMeasures.FirstOrDefault().Id//SelectedUnitOfMeasure.Id
            };

            return currentItem;
        }

        public void LoadItems()
        {
           ItemsList = new ItemService(true).GetAll().OrderBy(i => i.Id).ToList();
           Items = new ObservableCollection<ItemDTO>(ItemsList);
        }

        #region Categories
        public void LoadCategories()
        {
            var criteria = new SearchCriteria<CategoryDTO>();
            criteria.FiList.Add(c => c.NameType == NameTypes.Category);
            IEnumerable<CategoryDTO> categoriesList = new CategoryService(true)
                .GetAll(criteria)
                .ToList();

            Categories = new ObservableCollection<CategoryDTO>(categoriesList);
        }

        private CategoryDTO _selectedCategory;
        public CategoryDTO SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                _selectedCategory = value;
                RaisePropertyChanged<CategoryDTO>(() => SelectedCategory);
            }
        }
        public ObservableCollection<CategoryDTO> Categories
        {
            get { return _categories; }
            set
            {
                _categories = value;
                RaisePropertyChanged<ObservableCollection<CategoryDTO>>(() => Categories);
            }
        }
        public ICommand AddNewCategoryCommand
        {
            get
            {
                return _addNewCategoryCommand ?? (_addNewCategoryCommand = new RelayCommand(ExcuteAddNewCategoryCommand));
            }
        }
        private void ExcuteAddNewCategoryCommand()
        {
            var category = new Categories(NameTypes.Category);
            category.ShowDialog();
            var dialogueResult = category.DialogResult;
            if (dialogueResult != null && (bool)dialogueResult)
            {
                LoadCategories();//should also get the latest updates in each row
                SelectedCategory = Categories.FirstOrDefault(c => c.DisplayName == category.TxtCategoryName.Text);
                if (SelectedCategory != null) CurrentItem.CategoryId = SelectedCategory.Id;
            }
        }
        #endregion

        #region Unit Of Measures
        private CategoryDTO _selectedUnitOfMeasure;
        public CategoryDTO SelectedUnitOfMeasure
        {
            get { return _selectedUnitOfMeasure; }
            set
            {
                _selectedUnitOfMeasure = value;
                RaisePropertyChanged<CategoryDTO>(() => SelectedUnitOfMeasure);
            }
        }

        public void LoadUoMs()
        {
            var criteria = new SearchCriteria<CategoryDTO>();
            criteria.FiList.Add(c => c.NameType == NameTypes.UnitMeasure);
            IEnumerable<CategoryDTO> categoriesList = new CategoryService(true)
                .GetAll(criteria)
                .OrderBy(i => i.Id)
                .ToList();

            UnitOfMeasures = new ObservableCollection<CategoryDTO>(categoriesList);
        }
        public ObservableCollection<CategoryDTO> UnitOfMeasures
        {
            get { return _unitOfMeasure; }
            set
            {
                _unitOfMeasure = value;
                RaisePropertyChanged<ObservableCollection<CategoryDTO>>(() => UnitOfMeasures);
            }
        }
        public ICommand AddNewUomCommand
        {
            get
            {
                return _addNewUomCommand ?? (_addNewUomCommand = new RelayCommand(ExcuteAddNewUomCommand));
            }
        }
        private void ExcuteAddNewUomCommand()
        {
            var category = new Categories(NameTypes.UnitMeasure);
            category.ShowDialog();
            var dialogueResult = category.DialogResult;
            if (dialogueResult != null && (bool)dialogueResult)
            {
                LoadUoMs();
                SelectedUnitOfMeasure = UnitOfMeasures.FirstOrDefault(c => c.DisplayName == category.TxtCategoryName.Text);
                if (SelectedUnitOfMeasure != null) CurrentItem.UnitOfMeasureId = SelectedUnitOfMeasure.Id;
            }
        }
        #endregion

        #region Validation
        public static int Errors { get; set; }
        public bool CanSave(object parameter)
        {
            return Errors == 0;
        }
        #endregion

        #region Previlege Visibility
        private UserRolesModel _userRoles;
       


        public UserRolesModel UserRoles
        {
            get { return _userRoles; }
            set
            {
                _userRoles = value;
                RaisePropertyChanged<UserRolesModel>(() => UserRoles);
            }
        }

        private void CheckRoles()
        {
            UserRoles = Singleton.UserRoles;
        }

        #endregion
    }
}
