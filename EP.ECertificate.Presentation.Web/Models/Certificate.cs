using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EP.ECertificate.Presentation.Web.Models
{
    public class Certificate
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int CertificateId { get; set; }
        public string EmployeeID { get; set; }
        public string Training { get; set; }
        public string Filename
        {
            get
            {
                //return String.Format("CT-{0}-{1}-{2}.pdf", CertificateId , Training, EmployeeID);
                return String.Format("CT-{0}.pdf", CertificateId);
            }
        }
        public List<PlaceholderValue> PlaceholderValues { get; set; }
        public Job Job { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }


    }
}