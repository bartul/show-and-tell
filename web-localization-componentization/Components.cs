using System;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.WebPages;

namespace SomeBoundedContext.Web.Components
{
    [PageVirtualPathAttribute("~/Views/Shared/SomeImportantListComponent.cshtml")]
    public class SomeImportantListComponent : WebViewPage<dynamic>
    {
        public override void Execute() { Write(Html.Action("SomeImportantList", "SomeBoundedContext", Model)); } 
    }
    
    public class SomeImportantListComponentModel() 
    { 
        public int SomeId { get; set; }
    }
}

namespace SomeBoundedContext.Web
{
    public partial class SomeBoundedContextController : Controller
    {
        [Import]
        public IQueryService QueryService { get; set; }
        
        public PartialViewResult SomeImportantList(SomeImportantListComponentModel componentModel)
        {
            var model = new SomeImportantListViewModel()
            {
                Items = QueryService.ItemsByParentId(componentModel.SomeId),
            };
            return PartialView(model);
        }
    }
}


