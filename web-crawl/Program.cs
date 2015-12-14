using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using DataExtraction;
using System.Diagnostics;

namespace Vulpine
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            var authorizations =
            Extraction.Start("https://kartica.ina.hr")
                .Home()
                .Login("user", "pwd")
                .ReportsMain()
                .AuthorizationsTab()
                .AllAuthorizationPagesFrom(new DateTime(2013, 10, 1))
                .AsCardAuthorizations();

            Console.WriteLine("Done.");
        }
    }
}
