using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EP.ECertificate.Presentation.Web.Models
{
    public class Template
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int TemplateId { get; set; }

        public string Name { get; set; }
        [DisplayName("Background File")]
        public string Filename { get; set; }
        public List<Placeholder> Placeholders { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}