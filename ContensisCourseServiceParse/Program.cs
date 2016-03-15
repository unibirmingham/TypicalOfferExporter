using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ServiceStack.Text;

namespace ContensisCourseServiceParse
{
    public class CourseItem
    {
        public string RegistryCode { get; set; }
        public string TypicalOffer { get; set; }

    }

    internal class Program
    {
        private const string ServiceAddress = "http://www.birmingham.ac.uk/web_services/CourseService.svc/json/courses";
        private static readonly string PathDesktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        private static readonly string FilePath = PathDesktop + "\\typicalOfferExport.csv";

        private static void Main(string[] args)
        {
            var response = GET(ServiceAddress);

            var results = JsonSerializer.DeserializeFromString<IEnumerable<CourseItem>>(response);

            IEnumerable<CourseItem> finalParse = null;

            if (results != null && results.Skip(0).Any())
            {
                finalParse = results.Where(n => !string.IsNullOrEmpty(n.RegistryCode) && !string.IsNullOrEmpty(n.TypicalOffer));
            }

            if (finalParse != null && finalParse.Skip(0).Any())
            {
                GenerateCsv(finalParse);
            }
        }

        public static void GenerateCsv(IEnumerable<CourseItem> items)
        {
            if (!File.Exists(FilePath))
            {
                File.Create(FilePath).Close();
            }
            const string delimter = ",";

            var output = items.Select(n => new[] {n.RegistryCode, n.TypicalOffer}).ToList();

            var length = output.Count;

            using (TextWriter writer = File.CreateText(FilePath))
            {
                for (var i = 0; i < length; i++)
                {
                    writer.WriteLine(string.Join(delimter, output[i]));
                }
            }
        }

        static string GET(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                var response = request.GetResponse();
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        var reader = new StreamReader(responseStream, Encoding.UTF8);
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                var errorResponse = ex.Response;
                using (var responseStream = errorResponse.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        var reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                        return reader.ReadToEnd();


                    }
                    // log errorText
                }
            }

            return "The responseStream object is null.";
        }
    }
}
