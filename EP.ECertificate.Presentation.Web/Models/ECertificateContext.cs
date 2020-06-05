using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace EP.ECertificate.Presentation.Web.Models
{
    public class ECertificateContext: DbContext 
    {
        public ECertificateContext() : base("DefaultConnection")
        {

        }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Placeholder> Placeholders { get; set; }

        }
    }
