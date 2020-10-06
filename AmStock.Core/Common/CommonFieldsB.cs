using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMStock.Core
{
    public class CommonFieldsB : CommonFieldsA
    {
        [Required(ErrorMessage = "Name Is Required")]
        [MaxLength(255, ErrorMessage = "Name Exceeded 255 letters")]
        [DisplayName("Name")]
        public string DisplayName
        {
            get { return GetValue(() => DisplayName); }
            set { SetValue(() => DisplayName, value); }
        }

        [MaxLength(255, ErrorMessage = "Name Exceeded 255 letters")]
        [DisplayName("Description")]
        public string Description
        {
            get { return GetValue(() => Description); }
            set { SetValue(() => Description, value); }
        }

        [NotMapped]
        public Uri PhotoPath { get; set; }
    }
    public class CommonFieldsB2 : CommonFieldsA
    {
        //[Required(ErrorMessage = "Name Is Required")]
        [MaxLength(255, ErrorMessage = "Name Exceeded 255 letters")]
        [DisplayName("Name")]
        public string DisplayName
        {
            get { return GetValue(() => DisplayName); }
            set { SetValue(() => DisplayName, value); }
        }

        [MaxLength(255, ErrorMessage = "Name Exceeded 255 letters")]
        [DisplayName("Description")]
        public string Description
        {
            get { return GetValue(() => Description); }
            set { SetValue(() => Description, value); }
        }

        [NotMapped]
        public Uri PhotoPath { get; set; }
    }
}
