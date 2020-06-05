using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace EP.ECertificate.Presentation.Web.Models
{
    public class PlaceholderValue
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ValueId { get; set; }
        public Placeholder Placeholder { get; set; }
        public string Value { get; set; }
    }
}
