using Elmah;
using EP.ECertificate.Presentation.Web.Models;
using EP.ECertificate.Presentation.Web.Utility;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace EP.ECertificate.Presentation.Web.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {

        private ECertificateContext db = new ECertificateContext();

        //
        // GET: /Template/

        #region Methods
        public ActionResult Index()
        {

            ViewBag.Templates = new SelectList(db.Templates.ToList(), "TemplateId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(Job job)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var file = Request.Files[0];
                    if (FileManager.CheckExtension(file, new string[] { ".xls", ".csv" }))
                    {
                        //Include for getting related tables
                        var template = db.Templates.Include("Placeholders").Where(p => p.TemplateId == job.Template.TemplateId).FirstOrDefault();

                        job.Template = template;
                        job.FileName = FileManager.SaveFile(file, FileType.Data);
                        job.CreatedBy = User.Identity.Name;
                        job.CreatedOn = DateTime.Now;
                        db.Jobs.Add(job);

                        var excelFileManager = new ExcelFileManager(job.FileName);

                        var excelHeaders = excelFileManager.GetExcelHeaders();
                        var fieldNames = job.Template.Placeholders.Select(p => p.FieldName);

                        ValidatePlaceHolders(job, excelHeaders);
                        ValidateCertificateReference(job, excelHeaders);

                        var rows = excelFileManager.GetExcelRows();
                        var count = rows.Count();

                        List<Certificate> newCertificates = new List<Certificate>();
                        for (int i = 0; i < count; i++)
                        {
                            var certificate = CreateCertificate(job, rows, i);
                            newCertificates.Add(certificate);
                            db.Certificates.Add(certificate);
                        }
                        db.SaveChanges();
                        CreateCertificateFiles(job, template, newCertificates);
                        transaction.Commit();
                    }
                    else
                    {
                        throw new Exception("Wrong file type used. Please use XLS or CSV format.");
                    }

                    return RedirectToAction("details", new { id = job.JobId });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ErrorSignal.FromCurrentContext().Raise(ex);
                    ViewBag.Error = ex.Message;
                    ViewBag.Templates = new SelectList(db.Templates.ToList(), "TemplateId", "Name");
                    return View(job);
                }
            }

        }

        public ActionResult List()
        {
            var lastMonthDate = DateTime.Now.AddDays(-30);
            return View(db.Jobs.Where(p=>p.CreatedOn > lastMonthDate).ToList());
        }

        public ActionResult DownloadCertificate(int id)
        {
            var certificate = db.Certificates.Include("Job").FirstOrDefault(p => p.CertificateId == id);
            var job = db.Certificates.FirstOrDefault(p => p.CertificateId == id);

            return File(FileManager.GetCertificateFilePath(certificate.Job, certificate), "application/pdf", certificate.Filename);
        }

        public ActionResult Report(int id)
        {
            var job = db.Jobs.FirstOrDefault(p => p.JobId == id);
            var certificates = db.Certificates.Where(p => p.Job.JobId == id);
            StringBuilder sb = new StringBuilder();
            foreach (var certificate in certificates)
            {
                var directory = String.Format(@"\{0}\{1}\{2}", job.Name, certificate.Training, certificate.EmployeeID);
                sb.AppendLine(String.Format("\"{0}\"\t\"{1}\"\t\"{2}\"\t\"{3}\"", certificate.EmployeeID, certificate.Filename, certificate.Training, directory));
            }
            return File(new System.Text.UTF8Encoding().GetBytes(sb.ToString()), "text/csv", job.Name + ".csv");
        }

        public ActionResult Details(int id)
        {
            ViewBag.Job = db.Jobs.FirstOrDefault(p => p.JobId == id);
            return View(db.Certificates.Where(p => p.Job.JobId == id));
        }

        #endregion

        #region Utilities
        private Certificate CreateCertificate(Job job, LinqToExcel.Row[] rows, int i)
        {
            Certificate certificate = new Certificate();
            certificate.PlaceholderValues = new List<PlaceholderValue>();

            var row = rows[i];
            foreach (var placeholder in job.Template.Placeholders)
            {
                var cellValue = row[placeholder.FieldName].Value;
                var value = cellValue.GetType() == typeof(DateTime) ? ((DateTime)cellValue).ToString("dd-MM-yyyy") : cellValue.ToString();
                certificate.PlaceholderValues.Add(new PlaceholderValue { Value = value, Placeholder = placeholder });

            }

            certificate.Job = job;
            certificate.EmployeeID = ((string)row[job.EmployeeIDReference]).Trim();
            certificate.Training = ((string)row[job.TrainingReference]).Trim();
            certificate.CreatedBy = User.Identity.Name;
            certificate.CreatedOn = DateTime.Now;
            return certificate;

        }

        private static void ValidateCertificateReference(Job job, List<string> excelHeaders)
        {
            //Validation for Certificate Reference
            if (!String.IsNullOrEmpty(job.EmployeeIDReference))
            {
                if (!excelHeaders.Contains(job.EmployeeIDReference))
                {
                    throw new Exception(job.EmployeeIDReference + " reference not in excel file");
                }
            }

            if (!String.IsNullOrEmpty(job.TrainingReference))
            {
                if (!excelHeaders.Contains(job.TrainingReference))
                {
                    throw new Exception(job.TrainingReference + " reference not in excel file");
                }
            }
        }

        private static void ValidatePlaceHolders(Job job, List<string> excelHeaders)
        {
            //Validation for the Headers and Placeholders
            foreach (var placeholder in job.Template.Placeholders)
            {
                if (!excelHeaders.Contains(placeholder.FieldName))
                {
                    throw new Exception(String.Format("The placeholder '{0}' of the template '{1}' is not in the excel file's header", placeholder.FieldName, job.Template.Name));
                }

            }
        }

        private void CreateCertificateFiles(Job job, Template template, List<Certificate> newCertificates)
        {

            //NOTE : If the FontSize is larger than the area allotted. Text won't appear.

            // Create a reader object to read the template's size
            PdfReader reader = new PdfReader(FileManager.GetFileNameWithFullPath(template.Filename, FileType.Template));
            Rectangle size = reader.GetPageSizeWithRotation(1);


            var pdfWidth = size.Width;
            var pdfHeight = size.Height;
            foreach (var certificate in newCertificates)
            {
                var document = new Document(new Rectangle(pdfWidth, pdfHeight), 0f, 0f, 0f, 0f);
                var path = FileManager.GetCertificateFilePath(certificate.Job, certificate);
                var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
                var writer = PdfWriter.GetInstance(document, stream);
                document.Open();
                var contentByte = writer.DirectContent;
                PdfImportedPage page = writer.GetImportedPage(reader, 1);
                contentByte.AddTemplate(page, 0, 0);

                // Text are always to layered on the most top, so it will be added last.
                foreach (var text in certificate.PlaceholderValues)
                {
                    var columnText = new ColumnText(contentByte);
                    float llx, lly, urx, ury;
                    llx = ConvertCmToPoints(text.Placeholder.PositionX);
                    lly = pdfHeight - ConvertCmToPoints(text.Placeholder.PositionY) - ConvertCmToPoints(text.Placeholder.Height);
                    urx = llx + ConvertCmToPoints(text.Placeholder.Width);
                    ury = lly + ConvertCmToPoints(text.Placeholder.Height);
                    Rectangle rectangle = new Rectangle(llx, lly, urx, ury);

                    var fontPath = ConfigurationManager.AppSettings["FontPath"];
                    var baseFont = BaseFont.CreateFont(Server.MapPath(String.Format(fontPath + "{0}", text.Placeholder.FontFamily)), BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    Font font = new Font(baseFont);
                    font.Color = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#" + text.Placeholder.Color));
                    //font.SetFamily(text.Placeholder.FontFamily);
                    font.Size = (float)text.Placeholder.FontSize;
                    columnText.SetSimpleColumn(rectangle);

                    // Run Direction would cause the text in PDF to be as garbage.
                    const string regex_match_arabic_hebrew = @"[\u0600-\u06FF,\u0590-\u05FF]+";
                    if (Regex.IsMatch(text.Value, regex_match_arabic_hebrew, RegexOptions.IgnoreCase))
                    {
                        columnText.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    }


                    var chunk = new Chunk(text.Value, font);
                    chunk.setLineHeight((float)text.Placeholder.FontSize);
                    columnText.AddText(chunk);
                    columnText.Alignment = PdfContentByte.ALIGN_CENTER;
                    columnText.Go(false);
                }


                document.Close();
            }

        }

        // NOT USED
        private List<CustomizationFont> GetCustomizationFonts()
        {

            var fonts = new List<CustomizationFont> { 
                new CustomizationFont("BaskervilleRegular", "baskervilleef-webfont"),
                new CustomizationFont("BernhardModern", "bernhmo-webfont"),
                new CustomizationFont("BlackJackRegular", "blackjack-webfont"), 
                new CustomizationFont("BlairMdITCTTMedium", "blairmedium-webfont"), 
                new CustomizationFont("CaitlinAGAGIPlain", "caitlin-webfont"), 
                new CustomizationFont("CatchupNormal", "catchup-webfont"), 
                new CustomizationFont("CACFuturaCasualBoldRegular", "futuracasualbold-webfont"), 
                new CustomizationFont("ClassicRoman", "atclassic-webfont"), 
                new CustomizationFont("DaisyLauRegular", "daisylau-webfont"), 
                new CustomizationFont("ErinBRegular", "erinb-webfont"), 
                new CustomizationFont("FiddlestixOutline", "fiddlestix-webfont"), 
                new CustomizationFont("FlingCom", "flingcom-webfont"), 
                new CustomizationFont("FuturaCEBookDemiBook", "futuracebook-webfont"), 
                new CustomizationFont("Gravura", "gravura-webfont"), 
                new CustomizationFont("HandwritingRegular", "handwriting-webfont"), 
                new CustomizationFont("HucklebuckRegular", "hucklebuck-webfont"), 
                new CustomizationFont("KanonBRegular", "kanonb-webfont"), 
                new CustomizationFont("LadyScriptRegular","ladyscript-webfont") , 
                new CustomizationFont("LaskoMedium", "lasko-webfont"), 
                new CustomizationFont("LazyDazeAGIRegular", "lazydazeagi-webfont"), 
                new CustomizationFont("LovebirdsRegular", "lovebirds-webfont"), 
                new CustomizationFont("MigrateITCTTRegular", "migrateitc-webfont"), 
                new CustomizationFont("MilliRegular", "milli-webfont"), 
                new CustomizationFont("MonolineScriptMTItalic", "monolinescriptmt-webfont"), 
                new CustomizationFont("OttoMaticSansRegular", "ottomatsan-webfont"), 
                new CustomizationFont("CACPinaforeRegular", "pinafore-webfont"), 
                new CustomizationFont("PristinaCom", "pristinacom-webfont"), 
                new CustomizationFont("SweetPeaRegular", "sweetpea-webfont"),
                new CustomizationFont("VelvetScriptRegular", "velvetscript-webfont"), 
                new CustomizationFont("WhimseyRegular", "whimsey-webfont"), 
                new CustomizationFont("WendyMediumRegular", "wendymedium-webfont"), 
                new CustomizationFont("WildBillBold", "wildbill-webfont"), 
                new CustomizationFont("AGIserifRegular", "agiserif-webfont"), 
                new CustomizationFont("CobaltNormal", "cobalt-normal-font-webfont"), 
                new CustomizationFont("ChiselNormal", "chisel-normal-font-webfont"), 
                new CustomizationFont("BernhartNormal", "bernhart-normal-font-webfont"), 
                new CustomizationFont("AmazeNormal", "amaze-normal-font-webfont"), 
                new CustomizationFont("AGITypewriterRegular", "agitypewriter-webfont"), 
                new CustomizationFont("BriskExtendedNormal", "brisk-webfont"), 
                new CustomizationFont("CafeNoireRegular", "cafenoire-webfont"), 
                new CustomizationFont("ChiselCondensedNormal", "chiselnormal-webfont"), 
                new CustomizationFont("FrancisGothicNormal", "francisgothic-webfont"), 
                new CustomizationFont("GeoNormal", "geo-webfont"), 
                new CustomizationFont("FrancisGothicReducedNormal", "francisgothiccond-webfont"), 
                new CustomizationFont("FusiNormal", "fusi-webfont"), 
                new CustomizationFont("VogelWideNormal","vogel-webfont")
            };
            return fonts;

        }

        private float ConvertCmToPoints(decimal cmUnit)
        {
            return Utilities.MillimetersToPoints((float)cmUnit * 10);
        } 
        #endregion
    }
}
