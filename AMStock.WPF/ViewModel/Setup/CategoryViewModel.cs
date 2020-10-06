using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AMStock.Core.Enumerations;
using AMStock.Core.Extensions;
using AMStock.Core.Models;
using AMStock.Service;
using AMStock.Service.Interfaces;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace AMStock.WPF.ViewModel
{
    public class CategoryViewModel : ViewModelBase
    {
        #region Fields
        private static ICategoryService _categoryService;
        private ICommand _saveCategoryViewCommand, _closeCategoryViewCommand, _deleteCategoryViewCommand, _addNewCategoryCommand;
        private ObservableCollection<CategoryDTO> _categories;
        private CategoryDTO _selectedCategory;
        private NameTypes _categoryType;
        private string _inputLanguage, _headerText;
        #endregion

        #region Constructor
        public CategoryViewModel()
        {
            CleanUp();
            _categoryService = new CategoryService();
            Messenger.Default.Register<NameTypes>(this, (message) =>
            {
                CategoryType = message;
            });
        }
        public static void CleanUp()
        {
            if (_categoryService != null)
                _categoryService.Dispose();
        }
        #endregion

        #region Properties
        public NameTypes CategoryType
        {
            get { return _categoryType; }
            set
            {
                _categoryType = value;
                RaisePropertyChanged<NameTypes>(() => CategoryType);

                HeaderText = new EnumerationExtension(NameTypes.Category.GetType()).GetDescription(CategoryType);
                LoadCategories();

                InputLanguage = HeaderText.ToLower().Contains("amh") ? "am-ET" : "en-US";
            }
        }

        public string InputLanguage
        {
            get { return _inputLanguage; }
            set
            {
                _inputLanguage = value;
                RaisePropertyChanged<string>(() => this.InputLanguage);
            }
        }
        public string HeaderText
        {
            get { return _headerText; }
            set
            {
                _headerText = value;
                RaisePropertyChanged<string>(() => this.HeaderText);
            }
        }

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
                ExcuteAddNewCategoryCommand();
            }
        }
        #endregion

        #region Commands
        public ICommand AddNewCategoryCommand
        {
            get
            {
                return _addNewCategoryCommand ?? (_addNewCategoryCommand = new RelayCommand(ExcuteAddNewCategoryCommand));
            }
        }
        private void ExcuteAddNewCategoryCommand()
        {
            SelectedCategory = new CategoryDTO
            {
                NameType = CategoryType
            };
        }

        public ICommand SaveItemViewCommand
        {
            get { return _saveCategoryViewCommand ?? (_saveCategoryViewCommand = new RelayCommand<Object>(ExecuteSaveItemViewCommand, CanSave)); }
        }
        private void ExecuteSaveItemViewCommand(object obj)
        {
            if (SaveCategory(obj))
            {
                if (obj != null)
                    CloseWindow(obj);
            }
        }
        public bool SaveCategory(object obj)
        {
            try
            {
                var isNewObject = SelectedCategory.Id;
                var stat = _categoryService.InsertOrUpdate(SelectedCategory);

                if (string.IsNullOrEmpty(stat))
                {
                    if (isNewObject == 0)
                    {
                        Categories.Insert(0, SelectedCategory);
                    }
                    return true;
                }

                MessageBox.Show(stat);
                return false;
            }
            catch
            {
                return false;
            }
        }

        public ICommand DeleteCategoryViewCommand
        {
            get { return _deleteCategoryViewCommand ?? (_deleteCategoryViewCommand = new RelayCommand<Object>(ExecuteDeleteListViewCommand, CanSave)); }
        }
        private void ExecuteDeleteListViewCommand(object obj)
        {
            if (MessageBox.Show("Are you Sure You want to Delete this Category?", "Delete Category",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                SelectedCategory.Enabled = false;
                var stat = _categoryService.Disable(SelectedCategory);

                if (string.IsNullOrEmpty(stat))
                    Categories.Remove(SelectedCategory);
                else
                    MessageBox.Show("Can't delete " + CategoryType.ToString() + ", may be the " + stat, "Can't Delete",
                        MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public ICommand CloseItemViewCommand
        {
            get
            {
                return _closeCategoryViewCommand ?? (_closeCategoryViewCommand = new RelayCommand<Object>(ExecuteCloseListViewCommand));
            }
        }
        private void ExecuteCloseListViewCommand(object obj)
        {
            CloseWindow(obj);
        }
        public void CloseWindow(object obj)
        {
            if (obj == null) return;
            var window = obj as Window;
            if (window == null) return;
            window.DialogResult = true;
            window.Close();
        }
        #endregion

        #region Load Categories
        public void LoadCategories()
        {
            var catList = _categoryService.GetAll();
            var categoriesList = catList.Where(c => c.NameType == CategoryType).ToList();
            Categories = new ObservableCollection<CategoryDTO>(categoriesList);
        }
        #endregion

        #region Validation
        public static int Errors { get; set; }
        public bool CanSave(object parameter)
        {
            return Errors == 0;
        }
        #endregion

    }
}
