using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EP.ECertificate.Presentation.Web.Models;
using System.IO;
using EP.ECertificate.Presentation.Web.Utility;
using System.Configuration;
using Elmah;

namespace EP.ECertificate.Presentation.Web.Controllers
{
    [HandleError]
    public class TemplateController : Controller
    {
        private ECertificateContext db = new ECertificateContext();

        //
        // GET: /Template/

        public ActionResult Index()
        {
            return View(db.Templates.ToList());
        }

        //
        // GET: /Template/Details/5

        public ActionResult Details(int id = 0)
        {
            Template template = db.Templates.Include("Placeholders").FirstOrDefault(p => p.TemplateId == id);
            if (template == null)
            {
                return HttpNotFound();
            }
            return View(template);
        }

        //
        // GET: /Template/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Template/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Template template)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    template.Placeholders = MapPlaceHolder();
                    if (Request.Files.Count > 0)
                    {
                        var file = Request.Files[0];
                        var fileName = FileManager.SaveFile(file, FileType.Template);
                        template.Filename = fileName;
                    }

                    template.CreatedOn = DateTime.Now;
                    template.CreatedBy = User.Identity.Name;
                    db.Templates.Add(template);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(template);
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                ViewBag.Error = ex.Message;
                return View(template);
            }
           
        }

        //
        // GET: /Template/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Template template = db.Templates.Include("Placeholders").FirstOrDefault(p => p.TemplateId == id);
            if (template == null)
            {
                return HttpNotFound();
            }
            IncludeFontDDL();
            return View(template);
        }

        //
        // POST: /Template/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Template template)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Template oldTemplate = db.Templates.Include("Placeholders").FirstOrDefault(p => p.TemplateId == template.TemplateId);
                    db.Entry(oldTemplate).State = EntityState.Modified;

                    //var oldPlaceholders = oldTemplate.Placeholders; // REMOVING THIS WOULD CAUSE ERROR DUE TO LINK TO CERTIFICATES GENERATED
                    //foreach (var placeholder in oldPlaceholders.ToList())
                    //    db.Placeholders.Remove(placeholder);

                    var placeHolders = MapPlaceHolder();
                    oldTemplate.Name = template.Name;
                    oldTemplate.Placeholders = placeHolders;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(template);
            }
            catch (Exception ex)
            {
                ErrorSignal.FromCurrentContext().Raise(ex);
                ViewBag.Error = ex.Message;
                return View(template);
            }
        }


        private List<Placeholder> MapPlaceHolder()
        {
            if (Request["PositionX"] != null)
            {
                var placeHolders = new List<Placeholder>();
                var idArray = Request["PlaceholderId"].Split(',');
                var fieldNameArray = Request["FieldName"].Split(',');
                var positionXArray = Request["PositionX"].Split(',');
                var positionYArray = Request["PositionY"].Split(',');
                var widthArray = Request["Width"].Split(',');
                var heightArray = Request["Height"].Split(',');
                var fontSizeArray = Request["FontSize"].Split(',');
                var fontFamilyArray = Request["FontFamily"].Split(',');
                var colorArray = Request["Color"].Split(',');
                //var alignmentArray = Request["Alignment"].Split(',');


                var length = Request["PositionX"].Split(',').Count();
                for (int i = 0; i < length; i++)
                {
                    placeHolders.Add(new Placeholder
                    {
                        PlaceholderId = Convert.ToInt32(idArray[i]),
                        FieldName = fieldNameArray[i],
                        PositionX = Convert.ToDecimal(positionXArray[i]),
                        PositionY = Convert.ToDecimal(positionYArray[i]),
                        Width = Convert.ToDecimal(widthArray[i]),
                        Height = Convert.ToDecimal(heightArray[i]),
                        FontSize = Convert.ToDecimal(fontSizeArray[i]),
                        FontFamily = fontFamilyArray[i],
                        Color = colorArray[i],
                        //Alignment = (Alignment)Enum.Parse(typeof(Alignment), alignmentArray[i])
                    });

                }
                return placeHolders;
            }
            else
                throw new Exception("Placeholders not added");
        }


        //
        // GET: /Template/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Template template = db.Templates.Find(id);
            if (template == null)
            {
                return HttpNotFound();
            }
            return View(template);
        }

        //
        // POST: /Template/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Template template = db.Templates.Find(id);
            db.Templates.Remove(template);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }


        public ActionResult AddPlaceHolder()
        {
            IncludeFontDDL();
            return PartialView(new Placeholder { FontSize = 30, Color = "000000" });
        }

        private void IncludeFontDDL()
        {
            var fontPath = ConfigurationManager.AppSettings["FontPath"];
            var path = Server.MapPath(String.Format(fontPath));
            var files = System.IO.Directory.GetFiles(path, "*.ttf").Select(p => Path.GetFileName(p));

            ViewBag.FontDDL = new SelectList(files);
        }

    }
}