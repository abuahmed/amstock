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
using AMStock.WPF.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using AMStock.Core.Enumerations;

namespace AMStock.WPF.ViewModel
{
    public class ItemViewModel : ViewModelBase
    {
        #region Fields
        private IEnumerable<ItemDTO> _itemsList;
        private ObservableCollection<ItemDTO> _items;
        private ItemDTO _selectedItem;
        private static IItemService _itemService;
        private ICommand _addNewItemViewCommand, _saveItemViewCommand, _deleteItemViewCommand,
                         _addNewCategoryCommand, _addNewUomCommand, _importItemViewCommand, _refreshListCommand;
        private string _searchText, _noOfItems;
        #endregion

        #region Constructor
        public ItemViewModel()
        {
            Load();
        }

        public void Load()
        {
            CleanUp();
            _itemService = new ItemService();
            CheckRoles();
            LoadCategories();
            LoadUoMs();
            GetLiveItems();
        }
        public static void CleanUp()
        {
            if (_itemService != null)
                _itemService.Dispose();
        }
        #endregion

        #region Public Properties

        public IEnumerable<ItemDTO> ItemsList
        {
            get { return _itemsList; }
            set
            {
                _itemsList = value;
                RaisePropertyChanged<IEnumerable<ItemDTO>>(() => ItemsList);
            }
        }
        public ObservableCollection<ItemDTO> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged<ObservableCollection<ItemDTO>>(() => Items);
                NoOfItems = "Total No. of Items = " + Items.Count().ToString("n0");
                if (Items.Any())
                    SelectedItem = Items.FirstOrDefault();
                else
                    AddNewItem();
            }
        }
        public ItemDTO SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                RaisePropertyChanged<ItemDTO>(() => SelectedItem);
                if (SelectedItem != null)
                {
                    SelectedCategory = Categories.FirstOrDefault(c => c.Id == SelectedItem.CategoryId);
                    SelectedUnitOfMeasure = UnitOfMeasures.FirstOrDefault(c => c.Id == SelectedItem.UnitOfMeasureId);

                    //SelectedCategory = SelectedItem.Category;
                    //SelectedUnitOfMeasure = SelectedItem.UnitOfMeasure;
                }

            }
        }
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                RaisePropertyChanged<string>(() => SearchText);

                if (SearchText != "")
                {
                    Items = new ObservableCollection<ItemDTO>
                        (ItemsList.Where(c => c.DisplayName.ToLower().Contains(value.ToLower()) ||
                        c.ItemCode.ToLower().Contains(value.ToLower())).ToList());
                }
                else
                {
                    Items = new ObservableCollection<ItemDTO>(ItemsList);
                }
            }
        }
        public string NoOfItems
        {
            get { return _noOfItems; }
            set
            {
                _noOfItems = value;
                RaisePropertyChanged<string>(() => NoOfItems);

            }
        }
        #endregion

        #region Commands
        public ICommand AddNewItemViewCommand
        {
            get { return _addNewItemViewCommand ?? (_addNewItemViewCommand = new RelayCommand(AddNewItem)); }
        }
        private void AddNewItem()
        {
            SelectedItem = new ItemDTO
            {
                ItemCode = _itemService.GetItemCode(),
                TotalCurrentQty = 0,
                SafeQuantity = 10,
                //Category = Categories.FirstOrDefault(),
                //UnitOfMeasure = UnitOfMeasures.FirstOrDefault()
            };

            SelectedCategory = Categories.FirstOrDefault();
            SelectedUnitOfMeasure = UnitOfMeasures.FirstOrDefault();
        }


        public ICommand SaveItemViewCommand
        {
            get { return _saveItemViewCommand ?? (_saveItemViewCommand = new RelayCommand(SaveItem)); }
        }
        private void SaveItem()
        {
            try
            {
                var newObject = SelectedItem.Id;

                SelectedItem.CategoryId = SelectedCategory.Id;
                SelectedItem.UnitOfMeasureId = SelectedUnitOfMeasure.Id;

                var stat = _itemService.InsertOrUpdate(SelectedItem);
                if (stat == string.Empty)
                {
                    var it = _itemService.GetAll().FirstOrDefault(i => i.Id == SelectedItem.Id);
                    if (it != null)
                    {
                        SelectedItem.Category = it.Category;
                        SelectedItem.UnitOfMeasure = it.UnitOfMeasure;
                    }

                    if (newObject == 0)
                        Items.Insert(0, SelectedItem);
                }
                else
                {
                    MessageBox.Show("Got Problem while saving item, try again..." + Environment.NewLine + stat,
                        "save error", MessageBoxButton.OK,
                       MessageBoxImage.Error);
                }

            }
            catch (Exception exception)
            {
                MessageBox.Show("Problem saving Item..." +
                            Environment.NewLine + exception.Message +
                            Environment.NewLine + exception.InnerException);
            }
        }

        public ICommand DeleteItemViewCommand
        {
            get { return _deleteItemViewCommand ?? (_deleteItemViewCommand = new RelayCommand(DeleteItem)); }
        }
        private void DeleteItem()
        {
            if (MessageBox.Show("Are you Sure You want to Delete this Item?", "Delete Item", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    SelectedItem.Enabled = false;
                    var stat = _itemService.Disable(SelectedItem);
                    if (stat == string.Empty)
                    {
                        Items.Remove(SelectedItem);
                        Items = Items;
                    }
                    else
                    {
                        MessageBox.Show("Can't delete the item, may be it is already in use..."
                        + Environment.NewLine + stat, "Can't Delete", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Can't delete the item, may be it is already in use..."
                        + Environment.NewLine + ex.Message + Environment.NewLine + ex.InnerException,
                        "Can't Delete", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public ICommand ImportItemViewCommand
        {
            get
            {
                return _importItemViewCommand ?? (_importItemViewCommand = new RelayCommand(ImportItem));
            }
        }
        private void ImportItem()
        {
            var importItemsWindow = new ImportItems();
            importItemsWindow.ShowDialog();
        }

        public ICommand RefreshListCommand
        {
            get
            {
                return _refreshListCommand ?? (_refreshListCommand = new RelayCommand(Load));
            }
        }
        #endregion

        #region Load Items
        private void GetLiveItems()
        {
            ItemsList = _itemService.GetAll().OrderBy(i => i.DisplayName).ToList();
            Items = new ObservableCollection<ItemDTO>(ItemsList);

            if (Items.Count > 0)
                SelectedItem = Items.FirstOrDefault();
            else
                AddNewItem();
        }
        #endregion

        #region Categories
        private ObservableCollection<CategoryDTO> _categories, _categoriesForSearch, _unitOfMeasure;
        public void LoadCategories()
        {
            IEnumerable<CategoryDTO> categoriesList = new CategoryService(true).GetAll()
                .Where(c => c.NameType == NameTypes.Category).ToList();

            Categories = new ObservableCollection<CategoryDTO>(categoriesList);

            IList<CategoryDTO> catForSearch = categoriesList.ToList();
            if (catForSearch.Count > 1)
                catForSearch.Insert(0, new CategoryDTO()
                {
                    DisplayName = "All"
                });
            CategoriesForSearch = new ObservableCollection<CategoryDTO>(catForSearch);
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
                if (SelectedCategory != null) SelectedItem.CategoryId = SelectedCategory.Id;
            }
        }

        public ObservableCollection<CategoryDTO> CategoriesForSearch
        {
            get { return _categoriesForSearch; }
            set
            {
                _categoriesForSearch = value;
                RaisePropertyChanged<ObservableCollection<CategoryDTO>>(() => CategoriesForSearch);
            }
        }
        private CategoryDTO _selectedCategoryForSearch;
        public CategoryDTO SelectedCategoryForSearch
        {
            get { return _selectedCategoryForSearch; }
            set
            {
                _selectedCategoryForSearch = value;
                RaisePropertyChanged<CategoryDTO>(() => SelectedCategoryForSearch);

                if (SelectedCategoryForSearch == null) return;
                if (SelectedCategoryForSearch.DisplayName == "All")
                {
                    GetLiveItems();
                    return;
                }
                var itemsList = ItemsList.Where(iq => SelectedCategoryForSearch != null && (iq.Category != null && iq.Category.DisplayName == SelectedCategoryForSearch.DisplayName));
                Items = new ObservableCollection<ItemDTO>(itemsList);
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
            IEnumerable<CategoryDTO> categoriesList = new CategoryService(true).GetAll()
                .Where(c => c.NameType == NameTypes.UnitMeasure).OrderBy(i => i.Id).ToList();

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
                if (SelectedUnitOfMeasure != null) SelectedItem.UnitOfMeasureId = SelectedUnitOfMeasure.Id;
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
