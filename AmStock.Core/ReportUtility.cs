using System;

namespace AMStock.Core
{
    public static class ReportUtility
    {
        public static string getEthCalendar(DateTime gregordate, bool longformat)
        {

            var nextisleapyear = DateTime.IsLeapYear(gregordate.Year + 1);


            var lastdate = new DateTime(gregordate.Year, 9, 10);

            if (nextisleapyear)
                lastdate = new DateTime(gregordate.Year, 9, 11);//will be 11 if the next year is a leap year

            var difference = lastdate.DayOfYear;
            int dayx = 0, monthx = 0, yearx = 2000;
            if (gregordate.DayOfYear > difference) //is in between meskerem 1 and tahasas 22
            {
                difference = gregordate.DayOfYear - difference;

                dayx = difference % 30;
                monthx = difference / 30;
                yearx = gregordate.Year - 7;

            }
            else //is in between tahasas 22 and meskerem 1
            {
                int yearlength = 365;
                if (nextisleapyear)
                    yearlength = 366;
                difference = gregordate.DayOfYear + (yearlength - difference);//will be 366 if the next year is a leap year

                dayx = difference % 30;
                monthx = difference / 30;
                yearx = gregordate.Year - 8;
            }

            string date = dayx.ToString();
            string month = getAmhMonth(monthx);
            if (dayx == 0)
            {
                month = getAmhMonth(monthx - 1);
                monthx = monthx - 1;
                dayx = 30;
            }
            string days = dayx.ToString();
            string months = (monthx + 1).ToString();
            if (dayx < 10)
                days = "0" + dayx;
            if ((monthx + 1) < 10)
                months = "0" + (monthx + 1).ToString();
            //return month + " " + dayx.ToString() + " " + yearx.ToString() + " -- " + dayx.ToString() + "/" + (monthx + 1).ToString() + "/" + yearx.ToString();

            if (longformat)
                return getAmhMonth(monthx) + " " + days + " " + yearx.ToString();
            else
                return days + "" + months + "" + yearx.ToString();
        }
        public static string getEthCalendarFormated(DateTime gregordate, string separator)
        {
            string amhCalender = getEthCalendar(gregordate, false);
            return amhCalender.Substring(0, 2) + separator + amhCalender.Substring(2, 2) + separator + amhCalender.Substring(4);
        }
        public static DateTime getGregorCalendar(int ethyear, int ethmonth, int ethday)
        {
            int yearr = ethyear;
            var begindate = new DateTime(yearr, 9, 11);
            int noOfDays = (ethmonth - 1) * 30 + ethday;

            if (noOfDays <= 112)
            {
                begindate = DateTime.IsLeapYear(yearr + 8) ? new DateTime(yearr + 7, 9, 11) : new DateTime(yearr + 7, 9, 10);
            }
            else
            {
                begindate = DateTime.IsLeapYear(yearr + 7) ? new DateTime(yearr + 7, 9, 11) : new DateTime(yearr + 7, 9, 10);
            }

            return begindate.AddDays(noOfDays);

        }
        public static string getAmhMonth(int month)
        {
            string[] amhmonths = { "መስከረም", "ጥቅምት", "ሕዳር", "ታህሳስ", "ጥር", "የካቲት", "መጋቢት", "ሚያዚያ", "ግንቦት", "ሰኔ", "ሐምሌ", "ነሃሴ", "ጳጉሜ" };
            return amhmonths[month];

        }
        public static string getEngMonth(int month)
        {
            string[] amhmonths = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            return amhmonths[month];
        }
        //ReportDocument crReportDocument;
        //public void DirectPrinter(ReportDocument cReport)
        //{
        //    System.Windows.Forms.PrintDialog printDialog = new System.Windows.Forms.PrintDialog();
        //    System.Drawing.Printing.PrintDocument printDocument = new System.Drawing.Printing.PrintDocument();

        //    printDialog.Document = printDocument;
        //    printDialog.AllowSomePages = true;
        //    printDialog.AllowCurrentPage = true;


        //    System.Windows.Forms.DialogResult dialogue = printDialog.ShowDialog();
        //    if (dialogue == System.Windows.Forms.DialogResult.OK)
        //    {
        //        int nCopy = printDocument.PrinterSettings.Copies;
        //        int sPage = printDocument.PrinterSettings.FromPage;
        //        int ePage = printDocument.PrinterSettings.ToPage;
        //        string PrinterName = printDocument.PrinterSettings.PrinterName;
        //        crReportDocument = new ReportDocument();
        //        crReportDocument = cReport;
        //        try
        //        {
        //            crReportDocument.PrintOptions.PrinterName = PrinterName;
        //            crReportDocument.Refresh();
        //            crReportDocument.PrintToPrinter(nCopy, false, sPage, ePage);
        //        }
        //        catch
        //        {
        //            MessageBox.Show("Error Printing Document");
        //        }
        //    }
        //}

    }
}
