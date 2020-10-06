using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AMStock.Core.Common;
using GalaSoft.MvvmLight.Messaging;
using AMStock.Core.Enumerations;
using AMStock.Core.Extensions;
using AMStock.Core.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using AMStock.Service;
using AMStock.WPF.Views;

namespace AMStock.WPF.ViewModel
{
    public class AddressViewModel : ViewModelBase
    {
        #region Fields
        private AddressDTO _selectedAddress;
        private string _headerText;
        private ICommand _saveAddressViewCommand, _closeAddressViewCommand, _resetAddressViewCommand;

        #endregion

        #region Constructor
        public AddressViewModel()
        {
            Messenger.Default.Register<AddressDTO>(this, (message) =>
            {
                SelectedAddress = message;
            });
        }

        #endregion

        #region Public Properties
        public AddressDTO SelectedAddress
        {
            get { return _selectedAddress; }
            set
            {
                _selectedAddress = value;
                RaisePropertyChanged<AddressDTO>(() => SelectedAddress);
                if (SelectedAddress != null)
                {
                    LoadCategories();

                    if (Cities != null && SelectedAddress.City != null)
                        SelectedCity = Cities.FirstOrDefault(c => c.Display.Equals(SelectedAddress.City));
                    if (SubCities != null && SelectedAddress.SubCity != null)
                        SelectedSubCity = SubCities.FirstOrDefault(c => c.Display.Equals(SelectedAddress.SubCity));
                }
            }
        }
        public string HeaderText
        {
            get { return _headerText; }
            set
            {
                _headerText = value;
                RaisePropertyChanged<string>(() => HeaderText);
            }
        }

        #endregion

        #region Commands
        public ICommand SaveAddressViewCommand
        {
            get { return _saveAddressViewCommand ?? (_saveAddressViewCommand = new RelayCommand<Object>(SaveAddress, CanSave)); }
        }
        private void SaveAddress(object obj)
        {
            try
            {
                if (SelectedCity != null)
                    SelectedAddress.City = SelectedCity.Display;

                if (SelectedSubCity != null)
                    SelectedAddress.SubCity = SelectedSubCity.Display;

                CloseWindow(obj);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't save"
                                  + Environment.NewLine + exception.Message, "Can't save", MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }
        }

        public ICommand ResetAddressViewCommand
        {
            get { return _resetAddressViewCommand ?? (_resetAddressViewCommand = new RelayCommand(ResetAddress)); }
        }
        private void ResetAddress()
        {
            SelectedAddress = new AddressDTO
            {
                Country = EnumUtil.GetEnumDesc(CountryList.Ethiopia),
                City = EnumUtil.GetEnumDesc(CityList.AddisAbeba)
            };
        }

        public ICommand CloseAddressViewCommand
        {
            get { return _closeAddressViewCommand ?? (_closeAddressViewCommand = new RelayCommand<Object>(CloseWindow)); }
        }
        public void CloseWindow(object obj)
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

        #endregion
        
        #region Open List Commands
        private ListDataItem _selectedCity, _selectedSubCity;
        private ObservableCollection<ListDataItem> _cities, _subCities;
        private ICommand _subCityListViewCommand, _cityListEnglishViewCommand;

        public ICommand CityListEnglishViewCommand
        {
            get
            {
                return _cityListEnglishViewCommand ??
                       (_cityListEnglishViewCommand = new RelayCommand(ExcuteCityListEnglishViewCommand));
            }
        }
        public void ExcuteCityListEnglishViewCommand()
        {
            var listWindow = new Categories(NameTypes.City);

            listWindow.ShowDialog();
            if (listWindow.DialogResult != null && (bool)listWindow.DialogResult)
            {
                LoadCities();
                SelectedCity = Cities.FirstOrDefault(c => c.Display.Equals(listWindow.TxtCategoryName.Text));
            }
        }

        public ICommand SubCityListViewCommand
        {
            get
            {
                return _subCityListViewCommand ?? (_subCityListViewCommand = new RelayCommand(ExcuteSubCityListViewCommand));
            }
        }
        public void ExcuteSubCityListViewCommand()
        {
            var listWindow = new Categories(NameTypes.SubCity);
            listWindow.ShowDialog();
            if (listWindow.DialogResult != null && (bool)listWindow.DialogResult)
            {
                LoadSubCities();
                SelectedSubCity = SubCities.FirstOrDefault(c => c.Display.Equals(listWindow.TxtCategoryName.Text));
            }
        }

        public void LoadCategories()
        {
            LoadCities();
            LoadSubCities();
        }

        public ListDataItem SelectedSubCity
        {
            get { return _selectedSubCity; }
            set
            {
                _selectedSubCity = value;
                RaisePropertyChanged<ListDataItem>(() => this.SelectedSubCity);
            }
        }
        public ObservableCollection<ListDataItem> SubCities
        {
            get { return _subCities; }
            set
            {
                _subCities = value;
                RaisePropertyChanged<ObservableCollection<ListDataItem>>(() => this.SubCities);
            }
        }
        public void LoadSubCities()
        {
            SubCities = new ObservableCollection<ListDataItem>();
            SelectedSubCity = new ListDataItem();

            IEnumerable<string> citiesAmharicList = new CategoryService(true)
                .GetAll()
                .Where(l => l.NameType == NameTypes.SubCity)
                .Select(l => l.DisplayName).Distinct().ToList();

            int i = 0;
            foreach (var item in citiesAmharicList)
            {
                var dataItem = new ListDataItem
                {
                    Display = item,
                    Value = i
                };
                SubCities.Add(dataItem);
                i++;
            }
        }

        public ListDataItem SelectedCity
        {
            get { return _selectedCity; }
            set
            {
                _selectedCity = value;
                RaisePropertyChanged<ListDataItem>(() => this.SelectedCity);
            }
        }
        public ObservableCollection<ListDataItem> Cities
        {
            get { return _cities; }
            set
            {
                _cities = value;
                RaisePropertyChanged<ObservableCollection<ListDataItem>>(() => this.Cities);
            }
        }
        public void LoadCities()
        {
            Cities = new ObservableCollection<ListDataItem>();
            SelectedCity = new ListDataItem();

            IEnumerable<string> citiesList = new CategoryService(true)
                .GetAll()
                .Where(l => l.NameType == NameTypes.City)
                .Select(l => l.DisplayName).Distinct().ToList();

            int i = 0;
            foreach (var item in citiesList)
            {
                var dataItem = new ListDataItem
                {
                    Display = item,
                    Value = i
                };
                Cities.Add(dataItem);
                i++;
            }
            //Cities.OrderBy(t => t.Display);
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
