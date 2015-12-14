using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SomeBoundedContext.Web.Components;


namespace Web.Testing
{
    public class _TestingController : Controller
    {
        

        // GET: /_Testing/Index
        public ActionResult Index()
        {
            var model = new ComponentIndexViewModel
            {
                ContextName = ComponentCaseSet.ContextName,
                Components = ComponentCaseSet.Components().Select(_componentSelector),
                CultureMenu = new CultureMenuPartialViewModel
                {
                    CurrentCulture = Localization.GetCurrentLanguage(),
                    SupportedCultures = Localization.GetSupportedLanguages(),
                },
            };

            SetCurrentThreadCulture();

            return View(model);
        }

        // GET: /_Testing/ComponentCases?name=ComponentName
        public ActionResult ComponentCases(string name)
        {
            SetCurrentThreadCulture();
            return View(
                ComponentCaseSet.Components()
                .Select(_componentSelector)
                .Where(item => item.Name == name)
                .Single()
            );
        }

        // GET: /_Testing/SetCulture?culture=en
        public ActionResult SetCulture(string culture)
        {
            Localization.SetCurrentLanguage(culture);

            SetCurrentThreadCulture();

            var url = Request.UrlReferrer != null ? Request.UrlReferrer.AbsoluteUri : "/";
            return Redirect(url);
        }


        private Func<IComponentMetadata, ComponentViewModel> _componentSelector = (item =>
            new ComponentViewModel
            {
                Name = item.Contract.Name.Replace("ComponentModel", ""),
                ExternalResources = item.ExternalResources,
                Interface = item.Contract.GetProperties().Select(prop => string.Format("{0} {1}", _typeCaption(prop.PropertyType), prop.Name)).ToArray(),
                HasAnyCase = ComponentCaseSet.Cases(item).Any(),
                HasMultipleCases = ComponentCaseSet.Cases(item).Count() > 1,
                Cases = ComponentCaseSet.Cases(item),
                SpotlightCase = ComponentCaseSet.Cases(item).FirstOrDefault(),
                CultureMenu = new CultureMenuPartialViewModel {
                    CurrentCulture = Localization.GetCurrentLanguage(),
                    SupportedCultures = Localization.GetSupportedLanguages(),
                },
            }
        );
        private static Func<Type, string> _typeCaption = (type =>
        {
            if (!type.IsGenericType) return type.Name;
            return string.Format(
                "{0}<{1}>", 
                type.Name.Split('`').First(), 
                string.Join(", ", type.GetGenericArguments().Select(arg => _typeCaption(arg)).ToArray())
            );
        });

        public void SetCurrentThreadCulture()
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = Localization.GetCultureInfoFromCurrent();
                System.Threading.Thread.CurrentThread.CurrentUICulture = Localization.GetCultureInfoFromCurrent();
            }

            catch (Exception ex) { }
        }
    }
    public class CultureMenuPartialViewModel
    {
        public string CurrentCulture { get; set; }
        public IEnumerable<string> SupportedCultures { get; set; }
    }
    public class ComponentIndexViewModel
    {
        public string ContextName { get; set; }
        public string[] Groups { get; set; }
        public IEnumerable<ComponentViewModel> Components { get; set; }
        public CultureMenuPartialViewModel CultureMenu { get; set; }
    }
    public class ComponentViewModel
    {
        public string Name { get; set; }
        public string Group { get; set; }
        public string Description { get; set; }
        public string[] ExternalResources { get; set; }
        public string[] Interface { get; set; }

        public bool HasAnyCase { get; set; }
        public bool HasMultipleCases { get; set; }
        public IEnumerable<IComponentCase> Cases { get; set; }
        public IComponentCase SpotlightCase { get; set; }

        public CultureMenuPartialViewModel CultureMenu { get; set; }
    }

    public interface IComponentCase
    {
        Type Contract { get; }
        string Name { get; }
        object Model { get; }
    }
    public class ComponentCase<T> : IComponentCase
    {
        Type IComponentCase.Contract { get { return typeof(T); } }
        object IComponentCase.Model { get { return this.Model; } }
        public string Name { get; set; }

        public T Model { get; set; }
    }

}
namespace SomeBoundedContext.Web.Testing
{    
    public class ComponentCaseSet
    {
        public static string ContextName { get { return "Some Bounded Context"; } }

        private static IEnumerable<IComponentCase> _GetCases()
        {
            yield return new ComponentCase<SomeImportantListComponentModel> { Name = "Single page list", Model = new SomeImportantListComponentModel { SomeId = 23 } };
            yield return new ComponentCase<SomeImportantListComponentModel> { Name = "Multi page list", Model = new SomeImportantListComponentModel { SomeId = 121 } };
            yield return new ComponentCase<SomeImportantListComponentModel> { Name = "Empty list", Model = new SomeImportantListComponentModel { SomeId = 0 } };

        }
        private static IEnumerable<IComponentCase> _cases = _GetCases();

        public static IEnumerable<ComponentCase<T>> Cases<T>()
        {
            return _cases.Where(item => item.Contract is T).Cast<ComponentCase<T>>();
        }
    }
}