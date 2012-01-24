using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twitterizer;
using System.Web.Security;
using Raven.Client;
using System.Net;
using System.Drawing;
using System.IO;

namespace RonPaul.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            GetTwitterUserCounts();

            return View();
        }

        public ActionResult ThankYou()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index");

            GetTwitterUserCounts();

            return View();
        }

        /// <summary>
        /// Sets twitter user counts onto the ViewBag
        /// </summary>
        private void GetTwitterUserCounts()
        {
            using (IDocumentSession session = MvcApplication.DocumentStore.OpenSession())
            {
                Dictionary<TwitterController.BadgeType, int> twitterBadgeTypeCounts =
                    session.Query<Models.UniqueTwitterBadgeTypesResult, Models.UniqueTwitterBadgeTypesIndex>()
                    .ToDictionary(k => k.BadgeType, v => v.Count);

                ViewBag.BasicBlackUserCount = twitterBadgeTypeCounts.ContainsKey(TwitterController.BadgeType.BasicBlack) ?
                    twitterBadgeTypeCounts[TwitterController.BadgeType.BasicBlack] : 0;
                ViewBag.BasicRedUserCount = twitterBadgeTypeCounts.ContainsKey(TwitterController.BadgeType.BasicRed) ?
                    twitterBadgeTypeCounts[TwitterController.BadgeType.BasicRed] : 0;
                ViewBag.ItsTimeUserCount = twitterBadgeTypeCounts.ContainsKey(TwitterController.BadgeType.ItsTime) ?
                    twitterBadgeTypeCounts[TwitterController.BadgeType.ItsTime] : 0;

                ViewBag.TotalUsers = twitterBadgeTypeCounts.Sum(b => b.Value);
            }
        }
    }
}
