using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AMStock.Core;
using GalaSoft.MvvmLight.Messaging;
using AMStock.Core.Enumerations;
using AMStock.Core.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using AMStock.Service;
using AMStock.WPF.Views;

namespace AMStock.WPF.ViewModel
{
    public class ContactViewModel : ViewModelBase
    {
        #region Fields
        private ContactDTO _selectedContact;
        private ObservableCollection<AddressDTO> _businessPartnerContactAddress;
        private ICommand _saveContactViewCommand, _closeContactViewCommand,
                         _resetContactViewCommand, _businessPartnerContactAddressViewCommand;

        #endregion

        #region Constructor
        public ContactViewModel()
        {
            Messenger.Default.Register<ContactDTO>(this, (message) =>
            {
                SelectedContact = message;
            });
        }

        #endregion

        #region Public Properties
        public ContactDTO SelectedContact
        {
            get { return _selectedContact; }
            set
            {
                _selectedContact = value;
                RaisePropertyChanged<ContactDTO>(() => SelectedContact);
                if (SelectedContact != null)
                {
                    LoadCategories();
                    if (SelectedContact.PositionId != null && SelectedContact.PositionId != 0)
                        SelectedPosition = Positions.FirstOrDefault(c => c.Id == SelectedContact.PositionId);
                    if (SelectedContact.TitleId != null && SelectedContact.TitleId != 0)
                        SelectedTitle = Titles.FirstOrDefault(c => c.Id == SelectedContact.TitleId);
                    if (SelectedContact.Address != null)
                        BusinessPartnerContactAddress = new ObservableCollection<AddressDTO>
                        {
                            SelectedContact.Address
                        };
                }
            }
        }
      
        public ObservableCollection<AddressDTO> BusinessPartnerContactAddress
        {
            get { return _businessPartnerContactAddress; }
            set
            {
                _businessPartnerContactAddress = value;
                RaisePropertyChanged<ObservableCollection<AddressDTO>>(() => BusinessPartnerContactAddress);
            }
        }

        #endregion

        #region Commands

        public ICommand BusinessPartnerContactAddressViewCommand
        {
            get { return _businessPartnerContactAddressViewCommand ?? (_businessPartnerContactAddressViewCommand = new RelayCommand(ExcuteBusinessPartnerContactAddress)); }
        }
        public void ExcuteBusinessPartnerContactAddress()
        {
            var businessPartnerAddress = new AddressEntry(SelectedContact.Address);
            businessPartnerAddress.ShowDialog();

            var dialogueResult = businessPartnerAddress.DialogResult;
            if (dialogueResult != null && (bool)dialogueResult)
            {
                //
            }
        }

        public ICommand SaveContactViewCommand
        {
            get { return _saveContactViewCommand ?? (_saveContactViewCommand = new RelayCommand<Object>(SaveContact, CanSave)); }
        }
        private void SaveContact(object obj)
        {
            try
            {
                if (SelectedTitle != null)
                    SelectedContact.TitleId = SelectedTitle.Id;
                if (SelectedPosition != null)
                    SelectedContact.PositionId = SelectedPosition.Id;

                CloseWindow(obj);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't save"
                                  + Environment.NewLine + exception.Message, "Can't save", MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }
        }

        public ICommand ResetContactViewCommand
        {
            get { return _resetContactViewCommand ?? (_resetContactViewCommand = new RelayCommand(ResetContact)); }
        }
        private void ResetContact()
        {
            SelectedContact = new ContactDTO();
        }

        public ICommand CloseContactViewCommand
        {
            get { return _closeContactViewCommand ?? (_closeContactViewCommand = new RelayCommand<Object>(CloseWindow)); }
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
        private CategoryDTO _selectedTitle, _selectedPosition;
        private ObservableCollection<CategoryDTO> _positions, _titles;
        private ICommand _positionListViewCommand, _titleListViewCommand;

        public void LoadCategories()
        {
            LoadTitles();
            LoadPositions();
        }

        public CategoryDTO SelectedTitle
        {
            get { return _selectedTitle; }
            set
            {
                _selectedTitle = value;
                RaisePropertyChanged<CategoryDTO>(() => this.SelectedTitle);
            }
        }
        public ObservableCollection<CategoryDTO> Titles
        {
            get { return _positions; }
            set
            {
                _positions = value;
                RaisePropertyChanged<ObservableCollection<CategoryDTO>>(() => this.Titles);
            }
        }
        public void LoadTitles()
        {
            Titles = new ObservableCollection<CategoryDTO>();
            SelectedTitle = new CategoryDTO();

            var criteria = new SearchCriteria<CategoryDTO>();
            criteria.FiList.Add(c => c.NameType == NameTypes.Title);
            IEnumerable<CategoryDTO> titlesList = new CategoryService(true)
                .GetAll(criteria)
                .ToList();

            Titles = new ObservableCollection<CategoryDTO>(titlesList);
        }
        public ICommand TitleListViewCommand
        {
            get
            {
                return _titleListViewCommand ??
                       (_titleListViewCommand = new RelayCommand(ExcuteTitleListViewCommand));
            }
        }
        public void ExcuteTitleListViewCommand()
        {
            var listWindow = new Categories(NameTypes.Title);

            listWindow.ShowDialog();
            if (listWindow.DialogResult != null && (bool)listWindow.DialogResult)
            {
                LoadTitles();
                SelectedTitle = Titles.FirstOrDefault(c => c.DisplayName == listWindow.TxtCategoryName.Text);
                if (SelectedTitle != null) SelectedContact.TitleId = SelectedTitle.Id;
            }
        }

        public CategoryDTO SelectedPosition
        {
            get { return _selectedPosition; }
            set
            {
                _selectedPosition = value;
                RaisePropertyChanged<CategoryDTO>(() => this.SelectedPosition);
            }
        }
        public ObservableCollection<CategoryDTO> Positions
        {
            get { return _titles; }
            set
            {
                _titles = value;
                RaisePropertyChanged<ObservableCollection<CategoryDTO>>(() => this.Positions);
            }
        }
        public void LoadPositions()
        {
            Positions = new ObservableCollection<CategoryDTO>();
            SelectedPosition = new CategoryDTO();

            var criteria = new SearchCriteria<CategoryDTO>();
            criteria.FiList.Add(c => c.NameType == NameTypes.Position);
            IEnumerable<CategoryDTO> positionsList = new CategoryService(true)
                .GetAll(criteria)
                .ToList();

            Positions = new ObservableCollection<CategoryDTO>(positionsList);
        }
        public ICommand PositionListViewCommand
        {
            get
            {
                return _positionListViewCommand ?? (_positionListViewCommand = new RelayCommand(ExcutePositionListViewCommand));
            }
        }
        public void ExcutePositionListViewCommand()
        {
            var listWindow = new Categories(NameTypes.Position);
            listWindow.ShowDialog();
            if (listWindow.DialogResult != null && (bool)listWindow.DialogResult)
            {
                LoadPositions();
                SelectedPosition = Positions.FirstOrDefault(c => c.DisplayName == listWindow.TxtCategoryName.Text);
                if (SelectedPosition != null) SelectedContact.PositionId = SelectedPosition.Id;
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

    }
}
