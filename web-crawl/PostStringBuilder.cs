using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Vulpine.DataExtraction
{
    public class PostStringBuilder
    {
        public PostStringBuilder(Dictionary<string, string> staticPostValues)
        {
            _staticValues = staticPostValues;
        }
        private Dictionary<string, string> _staticValues;

        public void SetViewState(string value)
        {
            _viewStateValue = value;
        }
        private string _viewStateValue;
        public void SetEventValidation(string value)
        {
            _eventValidationValue = value;
        }
        private string _eventValidationValue;

        public override string ToString()
        {
            var builder = new StringBuilder();

            var keys = _staticValues.Keys;

            for (int i = 0; i < _staticValues.Count; i++)
            {
                var key = keys.ElementAt(i);
                var value = _staticValues[key];

                if (i != 0) builder.Append("&");
                builder.AppendFormat("{0}={1}", key, WebUtility.UrlEncode(value));
            }

            builder.AppendFormat("&{0}={1}", "__VIEWSTATE", _viewStateValue);
            builder.AppendFormat("&{0}={1}", "__EVENTVALIDATION", _eventValidationValue);

            return builder.ToString();
        }
        public byte[] ToBytes()
        {
            return Encoding.UTF8.GetBytes(this.ToString());
        }


        public static readonly Dictionary<string, string> MainTabsStaticSet = new Dictionary<string, string>
        {
                    { "__EVENTTARGET", "" },
                    { "__EVENTARGUMENT", "" },
                    { "ctl00$ContentPlaceHolder1$hfSifra", "N016365" },
                    { "ctl00$ContentPlaceHolder1$hfKorisnik", "" },
                    { "ctl00_ContentPlaceHolder1_rpGrayPanel_cb1_VI", "4135:N016365:       :0" },
                    { "ctl00$ContentPlaceHolder1$rpGrayPanel$cb1", "FOXY d.o.o.                                       " },
                    { "ctl00_ContentPlaceHolder1_rpGrayPanel_cb1_DDDWS", "0:0:-1:-10000:-10000:0:-10000:-10000:1:0:0:0" },
                    { "ctl00$ContentPlaceHolder1$rpGrayPanel$cb1$DDD$L", "4135:N016365:       :0" },
                    { "ctl00_ContentPlaceHolder1_pc1ATI", "7" },
                    { "ctl00_ContentPlaceHolder1_pc1_ucIzvPocetna1_ASPxPopupControl1WS", "0:0:-1:-10000:-10000:0:400px:270px:1:0:0:0" },
                    { "__CALLBACKID", "ctl00$ContentPlaceHolder1$pc1" },
                    { "__CALLBACKPARAM", "c0:7" },
        };
    }
    public class LoginPostBuilder : PostStringBuilder
    {
        public LoginPostBuilder()
            : base(LoginPostStaticSet)
        {

        }

        public void SetLoginData(string username, string password)
        {
            _username = username;
            _password = password;
        }
        private string _username;
        private string _password;

        public override string ToString()
        {
            var builder = new StringBuilder(base.ToString());

            builder.AppendFormat("&{0}={1}", "ctl00$ContentPlaceHolder1$ucLogin1$Login1$UserName", _username);
            builder.AppendFormat("&{0}={1}", "ctl00$ContentPlaceHolder1$ucLogin1$Login1$Password", _password);

            return builder.ToString();
        }

        internal static readonly Dictionary<string, string> LoginPostStaticSet = new Dictionary<string, string>
        {
            { "ctl00$ContentPlaceHolder1$ucLogin1$Login1$LoginButton", "Prijava" },
        };
    }
    public class LimitsTabPostBuilder : PostStringBuilder {
        public LimitsTabPostBuilder()
            : base(LimitsTabStaticSet)
        {

        }

        internal static readonly Dictionary<string, string> LimitsTabStaticSet = new Dictionary<string, string>
        {
                    { "__EVENTTARGET", "" },
                    { "__EVENTARGUMENT", "" },
                    { "ctl00$ContentPlaceHolder1$hfSifra", "N016365" },
                    { "ctl00$ContentPlaceHolder1$hfKorisnik", "" },
                    { "ctl00_ContentPlaceHolder1_rpGrayPanel_cb1_VI", "4135:N016365:       :0" },
                    { "ctl00$ContentPlaceHolder1$rpGrayPanel$cb1", "FOXY d.o.o.                                       " },
                    { "ctl00_ContentPlaceHolder1_rpGrayPanel_cb1_DDDWS", "0:0:-1:-10000:-10000:0:-10000:-10000:1:0:0:0" },
                    { "ctl00$ContentPlaceHolder1$rpGrayPanel$cb1$DDD$L", "4135:N016365:       :0" },
                    { "ctl00_ContentPlaceHolder1_pc1ATI", "6" },
                    { "ctl00_ContentPlaceHolder1_pc1_ucIzvPocetna1_ASPxPopupControl1WS", "0:0:-1:-10000:-10000:0:400px:270px:1:0:0:0" },
                    { "DXScript", "1_142,1_80,1_135,1_128,1_133,1_119,1_98,1_105,1_97,1_77,1_126,1_100,1_136" },
                    { "__CALLBACKID", "ctl00$ContentPlaceHolder1$pc1" },
                    { "__CALLBACKPARAM", "c0:6" },
        };
    }
    public class AuthorizationsPagesPostBuilder : PostStringBuilder
    {
        public AuthorizationsPagesPostBuilder()
            : base(AuthorizationPagesStaticSet)
        {

        }

        public void SetCallbackState(string value)
        {
            _callbackState = value;
        }
        private string _callbackState;

        public void SetInputState(string value)
        {
            _inputState = value;
        }
        private string _inputState;

        public void SetPageIndex(int value)
        {
            _pageIndex = value;
        }
        private int _pageIndex;

        public void SetTimeFilter(DateTime value)
        {
            _timeFilter = value;
        }
        private DateTime? _timeFilter;

        public override string ToString()
        {
            var builder = new StringBuilder(base.ToString());

            if (!string.IsNullOrEmpty(_callbackState)) builder.AppendFormat("&{0}={1}", "ctl00$ContentPlaceHolder1$pc1$ctl100$gwAutorizacije$CallbackState", _callbackState);

            if (string.IsNullOrEmpty(_inputState))
            {
                builder.AppendFormat("&{0}={1}", "ctl00$ContentPlaceHolder1$pc1$ctl100$gwAutorizacije$DXKVInput", "[]");
            }
            else
            {
                builder.AppendFormat("&{0}={1}", "ctl00$ContentPlaceHolder1$pc1$ctl100$gwAutorizacije$DXKVInput", WebUtility.UrlEncode(_inputState));
            }

            if (_pageIndex == 0)
            {
                builder.AppendFormat("&{0}={1}", "__CALLBACKPARAM", WebUtility.UrlEncode("c0:KV|2;[];GB|9;7|REFRESH;"));
            }
            else
            {
                builder.AppendFormat("&{0}={1}", "__CALLBACKPARAM", WebUtility.UrlEncode(string.Format("c0:KV|148;{0};GB|20;12|PAGERONCLICK3|PN{1};", _inputState, _pageIndex)));
            }

            if (_timeFilter.HasValue)
            {
                var jsTime = ConvertToJavacriptTimestamp(_timeFilter.Value);

                builder.AppendFormat("&{0}={1}", "ctl00_ContentPlaceHolder1_pc1_ctl100_dAutorizacije_Raw", WebUtility.UrlEncode(jsTime.ToString()));
                builder.AppendFormat("&{0}={1}", "ctl00$ContentPlaceHolder1$pc1$ctl100$dAutorizacije", WebUtility.UrlEncode(_timeFilter.Value.ToShortDateString()));
                builder.AppendFormat("&{0}={1}", "ctl00$ContentPlaceHolder1$pc1$ctl100$hf_au$I", WebUtility.UrlEncode(string.Format("12|#|date|6|13|{0}#", jsTime.ToString())));
            }

            return builder.ToString();
        }

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static long ConvertToJavacriptTimestamp(DateTime value)
        {
            TimeSpan elapsedTime = value - Epoch;
            return (long)elapsedTime.TotalMilliseconds;
        }

        internal static readonly Dictionary<string, string> AuthorizationPagesStaticSet = new Dictionary<string, string>
        {
                    { "__EVENTTARGET", "" },
                    { "__EVENTARGUMENT", "" },
                    { "ctl00$ContentPlaceHolder1$hfSifra", "N016365" },
                    { "ctl00$ContentPlaceHolder1$hfKorisnik", "" },
                    { "ctl00_ContentPlaceHolder1_rpGrayPanel_cb1_VI", "4135:N016365:       :0" },
                    { "ctl00$ContentPlaceHolder1$rpGrayPanel$cb1", "FOXY d.o.o.                                       " },
                    { "ctl00_ContentPlaceHolder1_rpGrayPanel_cb1_DDDWS", "0:0:-1:-10000:-10000:0:-10000:-10000:1:0:0:0" },
                    { "ctl00$ContentPlaceHolder1$rpGrayPanel$cb1$DDD$L", "4135:N016365:       :0" },
                    { "ctl00_ContentPlaceHolder1_pc1ATI", "7" },
                    { "ctl00_ContentPlaceHolder1_pc1_ucIzvPocetna1_ASPxPopupControl1WS", "0:0:-1:-10000:-10000:0:400px:270px:1:0:0:0" },
                    { "ctl00_ContentPlaceHolder1_pc1_ctl100_dAutorizacije_DDDWS", "0:0:12000:213:278:1:235:239:1:0:0:0" },
                    { "ctl00_ContentPlaceHolder1_pc1_ctl100_dAutorizacije_DDD_C_FNPWS", "0:0:-1:-10000:-10000:0:0px:-10000:1:0:0:0" },
                    { "ctl00$ContentPlaceHolder1$pc1$ctl100$dAutorizacije$DDD$C", "01/18/2014:01/01/2014" },
                    { "ctl00$ContentPlaceHolder1$pc1$ctl100$gwAutorizacije$DXFREditorcol1", "" },
                    { "ctl00$ContentPlaceHolder1$pc1$ctl100$gwAutorizacije$DXFREditorcol2", "" },
                    { "ctl00_ContentPlaceHolder1_pc1_ctl100_gwAutorizacije_DXFREditorcol3_Raw", "N" },
                    { "ctl00$ContentPlaceHolder1$pc1$ctl100$gwAutorizacije$DXFREditorcol3", "" },
                    { "ctl00_ContentPlaceHolder1_pc1_ctl100_gwAutorizacije_DXFREditorcol3_DDDWS", "0:0:-1:-10000:-10000:0:-10000:-10000:1:0:0:0" },
                    { "ctl00_ContentPlaceHolder1_pc1_ctl100_gwAutorizacije_DXFREditorcol3_DDD_C_FNPWS", "0:0:-1:-10000:-10000:0:0px:-10000:1:0:0:0" },
                    { "ctl00$ContentPlaceHolder1$pc1$ctl100$gwAutorizacije$DXFREditorcol3$DDD$C", "12/29/2013" },
                    { "ctl00$ContentPlaceHolder1$pc1$ctl100$gwAutorizacije$DXFREditorcol4", "" },
                    { "ctl00$ContentPlaceHolder1$pc1$ctl100$gwAutorizacije$DXSelInput", "" },
                    { "DXScript", "1_142,1_80,1_135,1_128,1_133,1_119,1_98,1_105,1_97,1_77,1_126,1_100,1_136,1_84,1_92,1_90,1_113,1_115" },
                    { "__CALLBACKID", "ctl00$ContentPlaceHolder1$pc1$ctl100$gwAutorizacije" },
        };

        
    }
}
