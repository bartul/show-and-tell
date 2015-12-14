using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vulpine.Model;

namespace Vulpine.DataExtraction
{
    public static class Pages
    {
        public static Extraction Home(this Extraction extraction)
        {
            var requestTask = extraction.HttpClient.GetStringAsync("/");
            requestTask.Wait();

            extraction.Page = new HtmlInfo(requestTask.Result);
            return extraction;
        }
        public static Extraction Login(this Extraction extraction, string username, string password)
        {
            var postBuilder = new LoginPostBuilder();
            postBuilder.SetViewState(extraction.Page.ViewState());
            postBuilder.SetEventValidation(extraction.Page.EventValidation());
            postBuilder.SetLoginData(username, password);

            var post = new Post(extraction.HttpClient);

            var requestTask = post.ExecuteAsStringAsync(postBuilder.ToBytes(), "/");
            requestTask.Wait();

            extraction.Page = new HtmlInfo(requestTask.Result);
            return extraction;
        }
        public static Extraction ReportsMain(this Extraction extraction)
        {
            var requestTask = extraction.HttpClient.GetStringAsync("/Izvjesca.aspx");
            requestTask.Wait();

            extraction.Page = new HtmlInfo(requestTask.Result);
            return extraction;
        }
        public static Extraction AuthorizationsTab(this Extraction extraction)
        {
            var postBuilder = new PostStringBuilder(PostStringBuilder.MainTabsStaticSet);
            postBuilder.SetViewState(extraction.Page.ViewState());
            postBuilder.SetEventValidation(extraction.Page.EventValidation());

            var postContent = postBuilder.ToBytes();
            var post = new Post(extraction.HttpClient);

            var requestTask = post.ExecuteAsStringAsync(postContent, "/Izvjesca.aspx");
            requestTask.Wait();

            extraction.PartialPage = new HtmlInfo(requestTask.Result);
            return extraction;
        }

        public static Extraction LimitsTab(this Extraction extraction)
        {
            var postBuilder = new LimitsTabPostBuilder();
            postBuilder.SetViewState(extraction.Page.ViewState());
            postBuilder.SetEventValidation(extraction.Page.EventValidation());

            var postContent = postBuilder.ToBytes();
            var post = new Post(extraction.HttpClient);

            var requestTask = post.ExecuteAsStringAsync(postContent, "/Izvjesca.aspx");
            requestTask.Wait();

            extraction.PartialPage = new HtmlInfo(requestTask.Result);
            return extraction;
        }

        public static Extraction AuthorizationsPage(this Extraction extraction, DateTime startDate, int pageIndex)
        {
            var postBuilder = new AuthorizationsPagesPostBuilder();
            postBuilder.SetViewState(extraction.Page.ViewState());
            postBuilder.SetEventValidation(extraction.Page.EventValidation());
            postBuilder.SetCallbackState(extraction.PartialPage.AuthorizationPages_CallBackState());
            postBuilder.SetInputState(extraction.PartialPage.AuthorizationPages_InputState());
            postBuilder.SetTimeFilter(startDate);

            postBuilder.SetPageIndex(pageIndex);

            var postContent = postBuilder.ToBytes();
            var post = new Post(extraction.HttpClient);

            var requestTask = post.ExecuteAsStringAsync(postContent, "/Izvjesca.aspx");
            requestTask.Wait();

            extraction.PartialPage = new HtmlInfo(requestTask.Result);
            return extraction;
        }

        
        public static IEnumerable<string> AllAuthorizationPagesFrom(this Extraction extraction, DateTime startDate)
        {
            var pages = new List<string>();
            var treshold = 500;

            for (int i = 0; i < treshold; i++)
            {
                var postBuilder = new AuthorizationsPagesPostBuilder();
                postBuilder.SetViewState(extraction.Page.ViewState());
                postBuilder.SetEventValidation(extraction.Page.EventValidation());
                postBuilder.SetCallbackState(extraction.PartialPage.AuthorizationPages_CallBackState());
                postBuilder.SetInputState(extraction.PartialPage.AuthorizationPages_InputState());
                postBuilder.SetTimeFilter(startDate);

                postBuilder.SetPageIndex(i);

                var postContent = postBuilder.ToBytes();
                var post = new Post(extraction.HttpClient);

                var requestTask = post.ExecuteAsStringAsync(postContent, "/Izvjesca.aspx");
                requestTask.Wait();

                pages.Add(requestTask.Result);

                var partialPage = new HtmlInfo(requestTask.Result);
                extraction.PartialPage = partialPage;

                if (partialPage.AuthorizationPages_IsLastPage) break;
            }

            return pages;
        }
    }
}
