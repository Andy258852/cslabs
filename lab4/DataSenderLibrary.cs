using System;
using System.IO;
using System.Net;
using System.Text;
using XmlGeneratorLibrary;

namespace DataSenderLibrary
{
    public class DataSender
    {
        public NetworkCredential Credentials { get; set; }
        public string GloabalAddress { get; set; }

        public DataSender(string address, string login, string password)
        {
            GloabalAddress = address;
            Credentials = new NetworkCredential(login, password);
        }

        public void Record(object input, DateTime dateTime)
        {
            string address = GloabalAddress;
            address += DateTime.Now.ToString("yyyy") + "/";
            if (!DirectoryExists(address))
            {
                MakeDirectory(address);
            }
            address += DateTime.Now.ToString("MM") + "/";
            if (!DirectoryExists(address))
            {
                MakeDirectory(address);
            }
            address += DateTime.Now.ToString("dd") + "/";
            if (!DirectoryExists(address))
            {
                MakeDirectory(address);
            }
            string tempadd = address;
            address += "Instance__" + dateTime.ToString("yyyy_MM_dd_HH_mm_ss") + ".xml";
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(address);
            request.Credentials = Credentials;
            request.Method = WebRequestMethods.Ftp.UploadFile;
            XmlGenerator xmlGenerator = new XmlGenerator("");
            string xmlstr = xmlGenerator.GetXmlString(input);
            Upload(request, xmlstr);

            address = tempadd;
            address += "Instance__" + dateTime.ToString("yyyy_MM_dd_HH_mm_ss") + ".xsd";
            FtpWebRequest request1 = (FtpWebRequest)WebRequest.Create(address);
            request1.Credentials = Credentials;
            request1.Method = WebRequestMethods.Ftp.UploadFile;
            Upload(request1, xmlGenerator.GetXsd(xmlstr));
        }

        private void Upload(FtpWebRequest request, string input)
        {
            using (Stream requestStream = request.GetRequestStream())
            {
                byte[] arr = Encoding.UTF8.GetBytes(input);
                requestStream.Write(arr, 0, arr.Length);
            }
        }

        private void MakeDirectory(string address)
        {
            FtpWebRequest request1 = (FtpWebRequest)WebRequest.Create(address);
            request1.Credentials = Credentials;
            request1.Method = WebRequestMethods.Ftp.MakeDirectory;
            GetResponse(request1);
        }

        private bool DirectoryExists(string dirPath)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(dirPath);
                request.Credentials = Credentials;
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                GetResponse(request);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void GetResponse(FtpWebRequest request)
        {
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse()) ;
        }
    }
}

