using System;
using System.ComponentModel.DataAnnotations;

namespace AMStock.Core.Models
{
    public class BuildVersionDTO : CommonFieldsA
    {
        [Required]
        [StringLength(25)]
        public string DatabaseVersion
        {
            get { return GetValue(() => DatabaseVersion); }
            set { SetValue(() => DatabaseVersion, value); }
        }
        
        [Required]
        public DateTime VersionDate
        {
            get { return GetValue(() => VersionDate); }
            set { SetValue(() => VersionDate, value); }
        }
    }
}