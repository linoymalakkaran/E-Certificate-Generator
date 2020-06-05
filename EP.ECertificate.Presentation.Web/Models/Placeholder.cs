using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace EP.ECertificate.Presentation.Web.Models
{
    public class Placeholder
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int PlaceholderId { get; set; }
        public string FieldName { get; set; }
        public decimal PositionX { get; set; }
        public decimal PositionY { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public Alignment Alignment { get; set; }
        public decimal FontSize { get; set; }
        public string FontFamily { get; set; }
        public string Color { get; set; }
    }

}
