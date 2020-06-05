using EP.ECertificate.Presentation.Web.Models;
using LinqToExcel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Web;

namespace EP.ECertificate.Presentation.Web.Utility
{
    public class FileManager
    {

        public static string SaveFile(HttpPostedFileBase file, FileType fileType)
        {
            return SaveFile(file, fileType.ToString());

        }

        public static string SaveFile(HttpPostedFileBase file, string directory)
        {
            if (file != null && file.ContentLength > 0)
            {
                var fileName = GetUniqueFileName(file.FileName, directory);
                file.SaveAs(fileName);
                return Path.GetFileName(fileName);
            }
            throw new FileNotFoundException();

        }


        private static string GetUniqueFileName(string fileName, string directory)
        {
            var fileWithPath = String.Empty;
            var newFileName = fileName;

            int i = 0;

            do
            {
                fileWithPath = GetFileNameWithFullPath(newFileName, directory);
                newFileName = Path.GetFileNameWithoutExtension(fileName) + "-" + i + Path.GetExtension(fileName);
                i++;
            }
            while (File.Exists(fileWithPath));
            return GetFileNameWithFullPath(newFileName, directory);
        }


        public static byte[] LoadFile(string fileName, string directory)
        {
            if (!String.IsNullOrEmpty(fileName) || !String.IsNullOrEmpty(directory))
            {
                var path = GetFileNameWithFullPath(fileName, directory);
                return File.ReadAllBytes(path);
            }
            throw new FileNotFoundException();

        }

        public static bool CheckExtension(HttpPostedFileBase file, string[] formats)
        {
            if (file != null && formats.Contains(Path.GetExtension(file.FileName)))
                return true;
            return false;
        }

        public static string GetFileNameWithFullPath(string fileName, FileType fileType)
        {
            var path = GetFileNameWithFullPath(fileName, fileType.ToString());
            return path;
        }

        public static string GetHttpPath(string fileName, FileType fileType)
        {
            var path = Path.Combine("~\\App_Data\\", fileType.ToString(), fileName);
            return path;
        }


        public static string GetFileNameWithFullPath(string rawName, string directory)
        {
            // campaign
            var directoryPath = HttpContext.Current.Server.MapPath("~\\App_Data\\" + directory);
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            var fileName = Path.Combine(directoryPath, Path.GetFileName(rawName));
            return fileName;
        }


        public static string GetCertificateFilePath(Job job, Certificate certificate)
        {

            AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);

            // http://pinvoke.net/default.aspx/advapi32/LogonUser.html    
            IntPtr userToken = IntPtr.Zero;
            bool success = LogonUser(
                ConfigurationManager.AppSettings["CertificateUsername"],
                ConfigurationManager.AppSettings["CertificateDomain"],
                ConfigurationManager.AppSettings["CertificatePassword"], 
                2, 0, out userToken);

            WindowsIdentity identity = new WindowsIdentity(userToken);

            WindowsImpersonationContext context = identity.Impersonate();
            var directory = String.Format(ConfigurationManager.AppSettings["CertificatePath"], job.Name, certificate.Training, certificate.EmployeeID);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            return Path.Combine(directory, Path.GetFileName(certificate.Filename));
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUser(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out IntPtr phToken
            );





    }
}