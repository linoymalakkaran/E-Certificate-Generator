using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace EP.ECertificate.Presentation.Web.Models
{
    public class Job
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int JobId { get; set; }

        public string Name { get; set; }

        public Template Template { get; set; }

        public string FileName { get; set; }

        [Display(Name = "Employee ID Reference")]
        public string EmployeeIDReference { get; set; }

        [Display(Name = "Training Reference")]
        public string TrainingReference { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Created On")]
        public DateTime CreatedOn { get; set; }

    }
}
