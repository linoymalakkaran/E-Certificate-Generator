using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EP.ECertificate.Presentation.Web.Models
{
    public class CustomizationFont
    {
        public CustomizationFont()
        {

        }

        public CustomizationFont(string familyName, string fileName)
        {
            FileName = fileName;
            FamilyName = familyName;
        }

        public string FileName { get; set; }
        public string FamilyName { get; set; }
    }
}