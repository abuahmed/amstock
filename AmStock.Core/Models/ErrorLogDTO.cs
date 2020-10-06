using System;
using System.ComponentModel.DataAnnotations;

namespace AMStock.Core.Models
{
    public class ErrorLogDTO : CommonFieldsA
    {
        [Required]
        public DateTime ErrorTime
        {
            get { return GetValue(() => ErrorTime); }
            set { SetValue(() => ErrorTime, value); }
        }

        public int ErrorNumber
        {
            get { return GetValue(() => ErrorNumber); }
            set { SetValue(() => ErrorNumber, value); }
        }

        public int ErrorSeverity
        {
            get { return GetValue(() => ErrorSeverity); }
            set { SetValue(() => ErrorSeverity, value); }
        }

        public int ErrorState
        {
            get { return GetValue(() => ErrorState); }
            set { SetValue(() => ErrorState, value); }
        }

        [StringLength(255)]
        public string ErrorProcedure
        {
            get { return GetValue(() => ErrorProcedure); }
            set { SetValue(() => ErrorProcedure, value); }
        }

        public int ErrorLine
        {
            get { return GetValue(() => ErrorLine); }
            set { SetValue(() => ErrorLine, value); }
        }

        [StringLength(4000)]
        public string ErrorMessage
        {
            get { return GetValue(() => ErrorMessage); }
            set { SetValue(() => ErrorMessage, value); }
        }
    }
}