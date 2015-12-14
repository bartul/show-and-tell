using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataExtraction
{
    public class HtmlInfo
    {
        public HtmlInfo(string html)
        {
            _html = html;
        }

        public string Html { get { return _html; } }
        private readonly string _html;

        public string GetElementValue(string elementId)
        {
            string flag = string.Format("id=\"{0}\" value=\"", elementId);

            int i = _html.IndexOf(flag) + flag.Length;
            int j = _html.IndexOf("\"", i);

            return Uri.EscapeDataString(_html.Substring(i, j - i));
        }

        public string ViewState()
        {
            return this.GetElementValue("__VIEWSTATE");
        }
        public string EventValidation()
        {
            return this.GetElementValue("__EVENTVALIDATION");
        }

        public string AuthorizationPages_CallBackState()
        {
            return this.GetElementValue("ctl00_ContentPlaceHolder1_pc1_ctl100_gwAutorizacije_CallbackState");
        }
        public string AuthorizationPages_InputState()
        {
            return WebUtility.UrlDecode(this.GetElementValue("ctl00_ContentPlaceHolder1_pc1_ctl100_gwAutorizacije_DXKVInput")).Replace("&#39;", "'");
        }
        public bool AuthorizationPages_IsLastPage
        {
            get
            {
                return
                    (_html.IndexOf("<img class=\"dxWeb_pNextDisabled\" src=\"/DXR.axd?r=1_9-Rcd37\" alt=\"Slijedeći\" />") != -1)
                    || (_html.IndexOf("<td class=\"dxpSummary\" style=\"white-space:nowrap;\">") == -1); // <td class="dxpSummary" style="white-space:nowrap;">
            }
        }

    }
}
