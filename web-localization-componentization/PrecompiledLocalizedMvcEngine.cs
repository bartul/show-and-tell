using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Web;
using System.Web.Compilation;
using System.Web.Mvc;
using System.Web.WebPages;

namespace Web.Infrastructure
{
    public class PrecompiledLocalizedMvcEngine : RazorViewEngine, IVirtualPathFactory
    {
        private readonly IDictionary<string, Type> _mappings;
        private readonly string _baseVirtualPath;
        private readonly Lazy<DateTime> _assemblyLastWriteTime;

        // Localization
        // format is ":ViewCacheEntry:{cacheType}:{prefix}:{name}:{controllerName}:{culture}:"
        private const string _cacheKeyFormat = ":ViewCacheEntry:{0}:{1}:{2}:{3}:{4}:";
        private const string _cacheKeyPrefix_Master = "Master";
        private const string _cacheKeyPrefix_Partial = "Partial";
        private const string _cacheKeyPrefix_View = "View";
        private static readonly string[] _emptyLocations = new string[0];
        public string[] LocalizedViewLocationFormats { get; set; }
        public string[] LocalizedMasterLocationFormats { get; set; }
        protected string[] LocalizedPartialViewLocationFormats { get; set; }

        public PrecompiledLocalizedMvcEngine(Assembly assembly)
            : this(assembly, null) { }
        public PrecompiledLocalizedMvcEngine(Assembly assembly, string baseVirtualPath)
        {
            _assemblyLastWriteTime = new Lazy<DateTime>(() => assembly.GetLastWriteTimeUtc(fallback: DateTime.MaxValue));
            _baseVirtualPath = NormalizeBaseVirtualPath(baseVirtualPath);

            base.AreaViewLocationFormats = new[] {
                "~/Areas/{2}/Views/{1}/{0}.cshtml", 
                "~/Areas/{2}/Views/Shared/{0}.cshtml", 
            };

            base.AreaMasterLocationFormats = new[] {
                "~/Areas/{2}/Views/{1}/{0}.cshtml", 
                "~/Areas/{2}/Views/Shared/{0}.cshtml", 
            };

            base.AreaPartialViewLocationFormats = new[] {
                "~/Areas/{2}/Views/{1}/{0}.cshtml", 
                "~/Areas/{2}/Views/Shared/{0}.cshtml", 
            };
            base.ViewLocationFormats = new[] {
                "~/Views/{1}/{0}.cshtml", 
                "~/Views/Shared/{0}.cshtml", 
            };
            base.MasterLocationFormats = new[] {
                "~/Views/{1}/{0}.cshtml", 
                "~/Views/Shared/{0}.cshtml", 
            };
            base.PartialViewLocationFormats = new[] {
                "~/Views/{1}/{0}.cshtml", 
                "~/Views/Shared/{0}.cshtml", 
            };
            base.FileExtensions = new[] {
                "cshtml", 
            };

            _mappings = (from type in assembly.GetTypes()
                         where typeof(WebPageRenderingBase).IsAssignableFrom(type)
                         let pageVirtualPath = type.GetCustomAttributes(inherit: false).OfType<PageVirtualPathAttribute>().FirstOrDefault()
                         where pageVirtualPath != null
                         select new KeyValuePair<string, Type>(CombineVirtualPaths(_baseVirtualPath, pageVirtualPath.VirtualPath), type)
                         ).ToDictionary(t => t.Key, t => t.Value, StringComparer.OrdinalIgnoreCase);
            this.ViewLocationCache = new PrecompiledViewLocationCache(assembly.FullName, this.ViewLocationCache);

            // Localization
            LocalizedViewLocationFormats = new[] {
                "~/Views/{2}/{1}.{0}.cshtml",
                "~/Views/{2}/{1}.{0}.vbhtml",
                "~/Views/Shared/{1}.{0}.cshtml",
                "~/Views/Shared/{1}.{0}.vbhtml"
            };
            MasterLocationFormats = new[] {
                "~/Views/{2}/{1}.{0}.cshtml",
                "~/Views/{2}/{1}.{0}.vbhtml",
                "~/Views/Shared/{1}.{0}.cshtml",
                "~/Views/Shared/{1}.{0}.vbhtml"
            };
            LocalizedPartialViewLocationFormats = new[] {
                "~/Views/{2}/{1}.{0}.cshtml",
                "~/Views/{2}/{1}.{0}.vbhtml",
                "~/Views/Shared/{1}.{0}.cshtml",
                "~/Views/Shared/{1}.{0}.vbhtml"
            };

        }

        public bool PreemptPhysicalFiles
        {
            get;
            set;
        }
        public bool UsePhysicalViewsIfNewer
        {
            get;
            set;
        }

        // Localization
        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            if (controllerContext == null)
                throw new ArgumentNullException("controllerContext");

            if (String.IsNullOrEmpty(partialViewName))
                throw new ArgumentException("Parameter partialViewName is null or empty.", "partialViewName");

            string[] searched;
            var controllerName = controllerContext.RouteData.GetRequiredString("controller");
            var partialPath = _GetPath(controllerContext, LocalizedPartialViewLocationFormats, partialViewName, controllerName, _cacheKeyPrefix_Partial, useCache, out searched);

            if (String.IsNullOrEmpty(partialPath))
            {
                var baseRes = base.FindPartialView(controllerContext, partialViewName, useCache);

                if (baseRes.View != null)
                    return baseRes;

                return new ViewEngineResult(searched.Union(baseRes.SearchedLocations));
            }

            return new ViewEngineResult(CreatePartialView(controllerContext, partialPath), this);
        }
        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            if (controllerContext == null)
                throw new ArgumentNullException("controllerContext");

            if (String.IsNullOrEmpty(viewName))
                throw new ArgumentException("Parameter viewName is null or empty.", "viewName");

            string[] viewLocationsSearched;
            string[] masterLocationsSearched;

            var controllerName = controllerContext.RouteData.GetRequiredString("controller");
            var viewPath = _GetPath(controllerContext, LocalizedViewLocationFormats, viewName, controllerName, _cacheKeyPrefix_View, useCache, out viewLocationsSearched);
            var masterPath = _GetPath(controllerContext, LocalizedMasterLocationFormats, masterName, controllerName, _cacheKeyPrefix_Master, useCache, out masterLocationsSearched);

            if (String.IsNullOrEmpty(viewPath) || (String.IsNullOrEmpty(masterPath) && !String.IsNullOrEmpty(masterName)))
            {
                var baseRes = base.FindView(controllerContext, viewName, masterName, useCache);

                if (baseRes.View != null)
                    return baseRes;

                return new ViewEngineResult(viewLocationsSearched.Union(masterLocationsSearched).Union(baseRes.SearchedLocations));
            }

            return new ViewEngineResult(CreateView(controllerContext, viewPath, masterPath), this);
        }
        private string _GetPath(ControllerContext controllerContext, string[] locations, string name, string controllerName, string cacheKeyPrefix, bool useCache, out string[] searchedLocations)
        {
            searchedLocations = _emptyLocations;

            if (String.IsNullOrEmpty(name))
                return String.Empty;

            if (_IsSpecificPath(name))
                return String.Empty;

            var cacheKey = _CreateCacheKey(cacheKeyPrefix, name, controllerName);

            if (useCache)
            {
                var path = ViewLocationCache.GetViewLocation(controllerContext.HttpContext, cacheKey);
                if (string.IsNullOrWhiteSpace(path) == false) return path;
            }

            return _GetPathFromGeneralName(controllerContext, locations, name, controllerName, cacheKey, ref searchedLocations);
        }
        private static bool _IsSpecificPath(string name)
        {
            char c = name[0];
            return (c == '~' || c == '/');
        }
        private string _CreateCacheKey(string prefix, string name, string controllerName)
        {
            return String.Format(CultureInfo.InvariantCulture, _cacheKeyFormat,
                GetType().AssemblyQualifiedName, prefix, name, controllerName, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
        }
        private string _GetPathFromGeneralName(ControllerContext controllerContext, string[] locations, string name, string controllerName, string cacheKey, ref string[] searchedLocations)
        {
            var result = String.Empty;
            searchedLocations = new string[locations.Length];

            for (int i = 0; i < locations.Length; i++)
            {
                var location = locations[i];
                var virtualPath = string.Format(CultureInfo.InvariantCulture, location, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, name, controllerName);

                if (this.FileExists(controllerContext, virtualPath))
                {
                    searchedLocations = _emptyLocations;
                    result = virtualPath;
                    ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, cacheKey, result);
                    break;
                }

                searchedLocations[i] = virtualPath;
            }

            return result;
        }


        protected override bool FileExists(ControllerContext controllerContext, string virtualPath)
        {
            if (UsePhysicalViewsIfNewer && IsPhysicalFileNewer(virtualPath))
            {
                // If the physical file on disk is newer and the user's opted in this behavior, serve it instead.
                return false;
            }
            return Exists(virtualPath) || base.FileExists(controllerContext, virtualPath);
        }
        protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath)
        {
            var precompiledView = CreateViewInternal(partialPath, masterPath: null, runViewStartPages: false);
            if (precompiledView != null) return precompiledView;
            return base.CreatePartialView(controllerContext, partialPath);
        }
        protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath)
        {
            var precompiledView = CreateViewInternal(viewPath, masterPath, runViewStartPages: true);
            if (precompiledView != null) return precompiledView;
            return base.CreateView(controllerContext, viewPath, masterPath);
        }
        private IView CreateViewInternal(string viewPath, string masterPath, bool runViewStartPages)
        {
            Type type;
            if (_mappings.TryGetValue(viewPath, out type))
            {
                return new PrecompiledMvcView(viewPath, masterPath, type, runViewStartPages, base.FileExtensions);
            }
            return null;
        }

        public object CreateInstance(string virtualPath)
        {
            Type type;

            if (!PreemptPhysicalFiles && VirtualPathProvider.FileExists(virtualPath))
            {
                // If we aren't pre-empting physical files, use the BuildManager to create _ViewStart instances if the file exists on disk. 
                return BuildManager.CreateInstanceFromVirtualPath(virtualPath, typeof(WebPageRenderingBase));
            }

            if (UsePhysicalViewsIfNewer && IsPhysicalFileNewer(virtualPath))
            {
                // If the physical file on disk is newer and the user's opted in this behavior, serve it instead.
                return BuildManager.CreateInstanceFromVirtualPath(virtualPath, typeof(WebViewPage));
            }

            if (_mappings.TryGetValue(virtualPath, out type))
            {
                if (DependencyResolver.Current != null)
                {
                    return DependencyResolver.Current.GetService(type);
                }
                return Activator.CreateInstance(type);
            }
            return null;
        }
        public bool Exists(string virtualPath)
        {
            return _mappings.ContainsKey(virtualPath);
        }
        private bool IsPhysicalFileNewer(string virtualPath)
        {
            if (virtualPath.StartsWith(_baseVirtualPath ?? String.Empty, StringComparison.Ordinal))
            {
                // If a base virtual path is specified, we should remove it as a prefix. Everything that follows should map to a view file on disk.
                if (!String.IsNullOrEmpty(_baseVirtualPath))
                {
                    virtualPath = '~' + virtualPath.Substring(_baseVirtualPath.Length);
                }

                string path = HttpContext.Current.Request.MapPath(virtualPath);
                return File.Exists(path) && File.GetLastWriteTimeUtc(path) > _assemblyLastWriteTime.Value;
            }
            return false;
        }
        private static string NormalizeBaseVirtualPath(string virtualPath)
        {
            if (!String.IsNullOrEmpty(virtualPath))
            {
                // For a virtual path to combine properly, it needs to start with a ~/ and end with a /.
                if (!virtualPath.StartsWith("~/", StringComparison.Ordinal))
                {
                    virtualPath = "~/" + virtualPath;
                }
                if (!virtualPath.EndsWith("/", StringComparison.Ordinal))
                {
                    virtualPath += "/";
                }
            }
            return virtualPath;
        }
        private static string CombineVirtualPaths(string baseVirtualPath, string virtualPath)
        {
            if (!String.IsNullOrEmpty(baseVirtualPath))
            {
                return VirtualPathUtility.Combine(baseVirtualPath, virtualPath.Substring(2));
            }
            return virtualPath;
        }
    }

    public class PrecompiledViewLocationCache : IViewLocationCache
    {
        private readonly string _assemblyName;
        private readonly IViewLocationCache _innerCache;

        public PrecompiledViewLocationCache(string assemblyName, IViewLocationCache innerCache)
        {
            _assemblyName = assemblyName;
            _innerCache = innerCache;
        }

        public string GetViewLocation(HttpContextBase httpContext, string key)
        {
            key = _assemblyName + "::" + key;
            return _innerCache.GetViewLocation(httpContext, key);
        }

        public void InsertViewLocation(HttpContextBase httpContext, string key, string virtualPath)
        {
            key = _assemblyName + "::" + key;
            _innerCache.InsertViewLocation(httpContext, key, virtualPath);
        }
    }
    public class PrecompiledMvcView : IView
    {
        private static readonly Action<WebViewPage, string> _overriddenLayoutSetter = CreateOverriddenLayoutSetterDelegate();
        private readonly Type _type;
        private readonly string _virtualPath;
        private readonly string _masterPath;


        public PrecompiledMvcView(string virtualPath, Type type, bool runViewStartPages, IEnumerable<string> fileExtension)
            : this(virtualPath, null, type, runViewStartPages, fileExtension)
        {
        }

        public PrecompiledMvcView(string virtualPath, string masterPath, Type type, bool runViewStartPages, IEnumerable<string> fileExtension)
        {
            _type = type;
            _virtualPath = virtualPath;
            _masterPath = masterPath;
            RunViewStartPages = runViewStartPages;
            ViewStartFileExtensions = fileExtension;
        }

        public bool RunViewStartPages
        {
            get;
            private set;
        }

        public IEnumerable<string> ViewStartFileExtensions
        {
            get;
            private set;
        }

        public void Render(ViewContext viewContext, TextWriter writer)
        {
            object instance = null;
            if (DependencyResolver.Current != null)
            {
                var viewPageActivator = DependencyResolver.Current.GetService<IViewPageActivator>();
                if (viewPageActivator != null)
                    instance = viewPageActivator.Create(viewContext.Controller.ControllerContext, _type);
                else
                    instance = DependencyResolver.Current.GetService(_type);
            }
            if (instance == null)
                instance = Activator.CreateInstance(_type);

            WebViewPage webViewPage = instance as WebViewPage;

            if (webViewPage == null)
            {
                throw new InvalidOperationException("Invalid view type");
            }

            if (!String.IsNullOrEmpty(_masterPath))
            {
                _overriddenLayoutSetter(webViewPage, _masterPath);
            }
            webViewPage.VirtualPath = _virtualPath;
            webViewPage.ViewContext = viewContext;
            webViewPage.ViewData = viewContext.ViewData;
            webViewPage.InitHelpers();

            WebPageRenderingBase startPage = null;
            if (this.RunViewStartPages)
            {
                startPage = StartPage.GetStartPage(webViewPage, "_ViewStart", ViewStartFileExtensions);
            }

            var pageContext = new WebPageContext(viewContext.HttpContext, webViewPage, null);
            webViewPage.ExecutePageHierarchy(pageContext, writer, startPage);
        }

        // Unfortunately, the only way to override the default layout with a custom layout from a
        // ViewResult, without introducing a new subclass, is by setting the WebViewPage internal
        // property OverridenLayoutPath [sic].
        // This method makes use of reflection for creating a property setter in the form of a
        // delegate. The latter is used to improve performance, compared to invoking the MethodInfo
        // instance directly, without sacrificing maintainability.
        private static Action<WebViewPage, string> CreateOverriddenLayoutSetterDelegate()
        {
            PropertyInfo property = typeof(WebViewPage).GetProperty("OverridenLayoutPath",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (property == null)
            {
                throw new NotSupportedException("The WebViewPage internal property \"OverridenLayoutPath\" does not exist, probably due to an unsupported run-time version.");
            }

            MethodInfo setter = property.GetSetMethod(nonPublic: true);
            if (setter == null)
            {
                throw new NotSupportedException("The WebViewPage internal property \"OverridenLayoutPath\" exists but is missing a set method, probably due to an unsupported run-time version.");
            }

            return (Action<WebViewPage, string>)Delegate.CreateDelegate(typeof(Action<WebViewPage, string>), setter, throwOnBindFailure: true);
        }
    }
    internal static class AssemblyExtensions
    {
        public static DateTime GetLastWriteTimeUtc(this Assembly assembly, DateTime fallback)
        {
            string assemblyLocation = null;
            try
            {
                assemblyLocation = assembly.Location;
            }
            catch (SecurityException)
            {
                // In partial trust we may not be able to read assembly.Location. In which case, we'll try looking at assembly.CodeBase
                Uri uri;
                if (!String.IsNullOrEmpty(assembly.CodeBase) && Uri.TryCreate(assembly.CodeBase, UriKind.Absolute, out uri) && uri.IsFile)
                {
                    assemblyLocation = uri.LocalPath;
                }
            }

            if (String.IsNullOrEmpty(assemblyLocation))
            {
                // If we are unable to read the filename, return fallback value.
                return fallback;
            }

            DateTime timestamp;
            try
            {
                timestamp = File.GetLastWriteTimeUtc(assemblyLocation);
                if (timestamp.Year == 1601)
                {
                    // 1601 is returned if GetLastWriteTimeUtc for some reason cannot read the timestamp.
                    timestamp = fallback;
                }
            }
            catch (UnauthorizedAccessException)
            {
                timestamp = fallback;
            }
            catch (PathTooLongException)
            {
                timestamp = fallback;
            }
            catch (NotSupportedException)
            {
                timestamp = fallback;
            }
            return timestamp;
        }
    }
}
