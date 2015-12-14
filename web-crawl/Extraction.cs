using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Vulpine.DataExtraction
{
    public class Extraction
    {
        private Extraction(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public HttpClient HttpClient { get { return _httpClient; } }
        private HttpClient _httpClient;

        public HtmlInfo Page { get; set; }
        public HtmlInfo PartialPage { get; set; }

        public static Extraction Start(HttpClient httpClient)
        {
            return new Extraction(httpClient);
        }
        public static Extraction Start()
        {
            var httpClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = true, UseCookies = true });
            httpClient.DefaultRequestHeaders.Add("Host", "kartica.ina.hr");
            httpClient.BaseAddress = new Uri("https://kartica.ina.hr");

            return Extraction.Start(httpClient);
        }
    }

    public static class ExtractionExtensions
    {
        public static Extraction DoNothing(this Extraction extraction)
        {
            return extraction;
        }
        public static void End(this Extraction extraction)
        {
            extraction.HttpClient.Dispose();
        }
        public static Extraction FlushOnDesktop(this Extraction extraction, string extractName)
        {
            File.WriteAllText(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                            string.Format("{0}.Page.html", extractName)),
                extraction.Page.Html);

            if (extraction.PartialPage != null)
            {
                File.WriteAllText(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                                string.Format("{0}.PartialPage.html", extractName)),
                    extraction.PartialPage.Html);
            }
            return extraction;
        }

        public static Extraction Login(this Extraction extraction)
        {
            return extraction;
        }
    }
}
