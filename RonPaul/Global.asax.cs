using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Raven.Client;
using Raven.Client.Embedded;

namespace RonPaul
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
            routes.MapRoute(
                "Twitter", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Twitter", action = "Preview", id = UrlParameter.Optional } // Parameter defaults
            );

            //routes.MapRoute(
            //    "TwitterPreview", // Route name
            //    "{controller}/{action}/{id}", // URL with parameters
            //    new { controller = "Home", action = "TwitterPreviewCallback", id = UrlParameter.Optional } // Parameter defaults
            //);
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            DocumentStore = new EmbeddableDocumentStore { DataDirectory = @"App_Data\ravendb", UseEmbeddedHttpServer = true };
            DocumentStore.Initialize();

            Raven.Client.Indexes.IndexCreation.CreateIndexes(typeof(Models.UniqueTwitterBadgeTypesIndex).Assembly, DocumentStore);

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        public static IDocumentStore DocumentStore
        {
            get;
            private set;
        }
    }
}