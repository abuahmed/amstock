using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AMStock.Core;
using AMStock.Core.Enumerations;
using AMStock.Core.Extensions;
using AMStock.Core.Models;
using AMStock.DAL;
using AMStock.Repository;
using AMStock.Repository.Interfaces;
using AMStock.WPF.Reports.DataSets;
using AMStock.WPF.Reports.Transactions;
using AMStock.WPF.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace AMStock.WPF.ViewModel
{
    public class AttachmentEntryViewModel : ViewModelBase
    {
        #region Fields
        private static IUnitOfWork _unitOfWork;
        private TransactionHeaderDTO _selectedTransaction, _selectedTransactionOld;
        private IEnumerable<TransactionLineDTO> _transactionLines;
        private BusinessPartnerDTO _selectedBusinessPartner;
        private ObservableCollection<BusinessPartnerDTO> _businessPartners;
        private ICommand _addCheckCommand;
        private ICommand _addNewBusinessPartnerCommand;
        private string _paymentMethod;

        #endregion

        #region Constructor
        public AttachmentEntryViewModel()
        {
            CleanUp();
            _unitOfWork = new UnitOfWork(DbContextUtil.GetDbContextInstance());

            LoadBusinessPartners();

            Messenger.Default.Register<TransactionHeaderDTO>(this, (message) =>
            {
                SelectedTransactionOld = message;
            });
        }
        public static void CleanUp()
        {
            if (_unitOfWork != null)
                _unitOfWork.Dispose();
        }
        #endregion

        #region Properties

        public TransactionHeaderDTO SelectedTransactionOld
        {
            get { return _selectedTransactionOld; }
            set
            {
                _selectedTransactionOld = value;
                RaisePropertyChanged<TransactionHeaderDTO>(() => SelectedTransactionOld);

                if (SelectedTransactionOld != null)
                {
                    SelectedTransaction =
                           _unitOfWork.Repository<TransactionHeaderDTO>().Query()
                          .Include(p => p.BusinessPartner, p => p.BusinessPartner.Addresses,
                                           p => p.Warehouse, p => p.Warehouse.Address,
                                           p => p.Payments,
                                           p => p.TransactionLines)
                                           .Filter(t => t.Id == SelectedTransactionOld.Id)
                                           .Get()
                                           .FirstOrDefault();

                    TransactionLines = _unitOfWork.Repository<TransactionLineDTO>().Query()
                      .Include(a => a.Item, a => a.Item.Category, a => a.Item.UnitOfMeasure)
                      .Filter(t => t.TransactionId == SelectedTransactionOld.Id).Get();
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
                if (SelectedTransaction != null)
                {
                    if (BusinessPartners != null)
                        SelectedBusinessPartner =
                            BusinessPartners.FirstOrDefault(bp => bp.Id == SelectedTransaction.BusinessPartnerId);
                    PaymentMethod = SelectedTransaction.PaymentCompleted == "Cleared" ? "CASH" : "CREDIT";
                }
            }
        }
        public IEnumerable<TransactionLineDTO> TransactionLines
        {
            get { return _transactionLines; }
            set
            {
                _transactionLines = value;
                RaisePropertyChanged<IEnumerable<TransactionLineDTO>>(() => TransactionLines);
            }
        }

        public string PaymentMethod
        {
            get { return _paymentMethod; }
            set
            {
                _paymentMethod = value;
                RaisePropertyChanged<string>(() => PaymentMethod);
            }
        }
        #endregion

        #region Commands
        public ICommand AddAttachmentCommand
        {
            get { return _addCheckCommand ?? (_addCheckCommand = new RelayCommand<Object>(ExecuteAddCheckCommand, CanSave)); }
        }
        private void ExecuteAddCheckCommand(object obj)
        {
            try
            {
                if (SelectedTransaction != null && SelectedBusinessPartner != null)
                {
                    if (!string.IsNullOrEmpty(SelectedTransaction.FiscalNumber))
                    {
                        SelectedTransaction.BusinessPartnerId = SelectedBusinessPartner.Id;

                        _unitOfWork.Repository<TransactionHeaderDTO>().Update(SelectedTransaction);
                        _unitOfWork.Commit();

                        var myReport = new SingleTransaction();
                        myReport.SetDataSource(GetListDataSet());

                        var report = new ReportViewerCommon(myReport);
                        report.Show();

                        CloseWindow(obj);
                    }
                    else
                    {
                        MessageBox.Show("Fiscal Number can't be empty!");
                    }
                }
            }
            catch
            {
                MessageBox.Show("Got problem while getting Attachment!", "Attachment Problem");
            }
        }

        public TransactionDataSet GetListDataSet()
        {
            var myDataSet = new TransactionDataSet();

            try
            {
                #region Fields
                var brCode = new BarcodeProcess();
                var tranNumberbarcode = ImageToByteArray(brCode.GetBarcode(SelectedTransaction.TransactionNumber, 320, 40, true), ImageFormat.Bmp);

                var tax = Convert.ToDecimal((SelectedTransaction.TotalCost * (Singleton.Setting.TaxPercent * (decimal)0.01)).ToString("N2"));

                #endregion

                #region Header
                myDataSet.TransactionHeader.Rows.Add(
                        SelectedTransaction.FiscalNumber,
                        tranNumberbarcode,
                        SelectedTransaction.Warehouse.DisplayName,
                        SelectedTransaction.BusinessPartner.DisplayName,
                        SelectedTransaction.BusinessPartner.TinNumber,
                        SelectedTransaction.BusinessPartner.VatNumber,
                        SelectedTransaction.TransactionDateString + "(" +
                               ReportUtility.getEthCalendarFormated(SelectedTransaction.TransactionDate, "/") + ")",
                        SelectedTransaction.Status,
                        SelectedTransaction.TotalCost,
                        EnumUtil.GetEnumDesc(Singleton.Setting.TaxType) + " (" + Singleton.Setting.TaxPercent + "%)",
                        tax,
                        SelectedTransaction.TotalCost + tax,
                        "linknumber1"
                        );
                #endregion

                #region Client Address

                myDataSet.ClientDetail.Rows.Add(
                    SelectedTransaction.Warehouse.Header,
                    SelectedTransaction.Warehouse.Footer,
                    SelectedTransaction.Warehouse.Address.StreetAddress,
                    SelectedTransaction.Warehouse.Address.SubCity,
                    SelectedTransaction.Warehouse.Address.Kebele,
                    SelectedTransaction.Warehouse.Address.HouseNumber,
                    SelectedTransaction.Warehouse.Address.Telephone,
                    SelectedTransaction.Warehouse.Address.Mobile,
                    SelectedTransaction.Warehouse.Address.Fax,
                    SelectedTransaction.Warehouse.Address.PrimaryEmail,
                    SelectedTransaction.Warehouse.Address.AlternateEmail,
                    SelectedTransaction.Warehouse != null ? SelectedTransaction.Warehouse.TinNumber : "",
                    SelectedTransaction.Warehouse != null ? SelectedTransaction.Warehouse.VatNumber : "",
                    PaymentMethod, PaymentMethod, "", "linknumber1");
                #endregion

                #region BPAddress
                myDataSet.BPAddress.Rows.Add(
                    SelectedTransaction.BusinessPartner.Addresses.FirstOrDefault().Address.StreetAddress,
                    SelectedTransaction.BusinessPartner.Addresses.FirstOrDefault().Address.SubCity,
                    SelectedTransaction.BusinessPartner.Addresses.FirstOrDefault().Address.Kebele,
                    SelectedTransaction.BusinessPartner.Addresses.FirstOrDefault().Address.HouseNumber,
                    SelectedTransaction.BusinessPartner.Addresses.FirstOrDefault().Address.Telephone,
                    SelectedTransaction.BusinessPartner.Addresses.FirstOrDefault().Address.Mobile,
                    SelectedTransaction.BusinessPartner.Addresses.FirstOrDefault().Address.Fax,
                    SelectedTransaction.BusinessPartner.Addresses.FirstOrDefault().Address.PrimaryEmail,
                    SelectedTransaction.BusinessPartner.Addresses.FirstOrDefault().Address.AlternateEmail,
                    "linknumber1");
                #endregion

                #region Lines
                var serNo = 1;
                foreach (var line in TransactionLines)
                {
                    myDataSet.TransactionLine.Rows.Add(
                        serNo,
                        line.Item.ItemCode,
                        string.IsNullOrEmpty(line.Item.Description) ? line.Item.DisplayName : line.Item.Description,
                        "",
                        line.Item.Category.DisplayName,
                        line.Item.UnitOfMeasure.DisplayName,
                        line.EachPrice,
                        line.Unit,
                        line.LinePrice,
                        0,
                        "linknumber1");

                    serNo++;
                }
                #endregion
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't get data for the report"
                                  + Environment.NewLine + exception.Message, "Can't get data", MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }

            return myDataSet;
        }

        public byte[] ImageToByteArray(Image imageIn, ImageFormat format)
        {
            var ms = new MemoryStream();
            imageIn.Save(ms, format);
            return ms.ToArray();
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

        #region BusinessPartners
        public void LoadBusinessPartners()
        {
            var businessPartnersList = _unitOfWork.Repository<BusinessPartnerDTO>()
                .Query()
                .Include(a => a.Addresses)
                .Filter(f => f.BusinessPartnerType == BusinessPartnerTypes.Customer)
                .Get()
                .OrderBy(i => i.Id).ToList();

            BusinessPartners = new ObservableCollection<BusinessPartnerDTO>(businessPartnersList);
        }

        public ObservableCollection<BusinessPartnerDTO> BusinessPartners
        {
            get { return _businessPartners; }
            set
            {
                _businessPartners = value;
                RaisePropertyChanged<ObservableCollection<BusinessPartnerDTO>>(() => BusinessPartners);
            }
        }
        public BusinessPartnerDTO SelectedBusinessPartner
        {
            get { return _selectedBusinessPartner; }
            set
            {
                _selectedBusinessPartner = value;
                RaisePropertyChanged<BusinessPartnerDTO>(() => SelectedBusinessPartner);
            }
        }

        public ICommand AddNewBusinessPartnerCommand
        {
            get
            {
                return _addNewBusinessPartnerCommand ?? (_addNewBusinessPartnerCommand = new RelayCommand(ExcuteAddNewBusinessPartnerCommand));
            }
        }
        private void ExcuteAddNewBusinessPartnerCommand()
        {
            try
            {
                var bPDetailWindow = new BusinessPartnerDetail(BusinessPartnerTypes.Customer);
                bPDetailWindow.ShowDialog();
                var dialogueResult = bPDetailWindow.DialogResult;
                if (dialogueResult != null && (bool)dialogueResult)
                {
                    LoadBusinessPartners();
                    SelectedBusinessPartner = BusinessPartners.LastOrDefault();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Can't add customer"
                                  + Environment.NewLine + exception.Message, "Can't add customer", MessageBoxButton.OK,
                      MessageBoxImage.Error);
            }
        }
        #endregion

        #region Validation
        public static int Errors { get; set; }
        public bool CanSave(object obj)
        {
            return Errors == 0;
        }
        #endregion
    }
}