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
using System.Drawing.Drawing2D;
using System.Configuration;

namespace RonPaul.Controllers
{
    public class TwitterController : Controller
    {
        private const string CUSTOMER_KEY = "TwitterCustomerKey";
        private const string CUSTOMER_SECRET = "TwitterCustomerSecret";
        private const string TWITTER_PREVIEW_IMAGE = "twitterPreviewImage";

        public enum BadgeType
        {
            BasicBlack = 0,
            BasicRed = 1,
            ItsTime = 2
        }

        [HttpPost]
        [Authorize]
        public ActionResult UpdatePicture()
        {
            byte[] imageBytes = (byte[])Session[TWITTER_PREVIEW_IMAGE];
            using (IDocumentSession session = MvcApplication.DocumentStore.OpenSession())
            {
                Models.TwitterAccountModel model = GetTwitterAccountModel(session, User.Identity.Name);
                TwitterResponse<TwitterUser> response = TwitterAccount.UpdateProfileImage(GetTwitterUserTokens(model), imageBytes);
            }
            return Redirect("/home/thankyou");
        }

        [Authorize]
        public ActionResult Preview(BadgeType previewType = BadgeType.BasicBlack)
        {
            if (User.Identity.IsAuthenticated)
            {
                using (IDocumentSession session = MvcApplication.DocumentStore.OpenSession())
                {
                    Models.TwitterAccountModel model = GetTwitterAccountModel(session, User.Identity.Name);
                    ViewBag.UserName = model.UserName;

                    TwitterUser twitterUser = TwitterUser.Show(GetTwitterUserTokens(model), decimal.Parse(model.TwitterUserId)).ResponseObject;
                    string originalProfileImagePath = GetBiggerProfilePictureURL(twitterUser.ProfileImageLocation);

                    ViewBag.OriginalProfileImageLocation = originalProfileImagePath;
                    ViewBag.ImagePreviewType = previewType;
                }
            }

            return View();
        }

        [HttpGet]
        public ActionResult InitiatePreview(BadgeType previewType)
        {
            OAuthTokens tokens = new OAuthTokens();

            string twitterLoginCallbackUrl = new Uri(Request.Url, string.Concat("/twitter/callback?previewType=", previewType)).ToString();
            OAuthTokenResponse response = OAuthUtility.GetRequestToken(ConfigurationManager.AppSettings[CUSTOMER_KEY], ConfigurationManager.AppSettings[CUSTOMER_SECRET], twitterLoginCallbackUrl);

            Uri twitterLoginUrl = OAuthUtility.BuildAuthorizationUri(response.Token, false);

            if (User.Identity.IsAuthenticated)
            {
                FormsAuthentication.SignOut();
                Session.Abandon();
            }

            return Redirect(twitterLoginUrl.ToString());
        }

        [HttpGet]
        [Authorize]
        public FileContentResult PreviewImage(BadgeType previewType)
        {
            Models.TwitterAccountModel user = null;
            using (IDocumentSession session = MvcApplication.DocumentStore.OpenSession())
            {
                user = GetTwitterAccountModel(session, User.Identity.Name);
            }
            if (user == null)
                return null; // TODO:

            ViewBag.UserName = user.UserName;
            TwitterUser twitterUser = TwitterUser.Show(
                    GetTwitterUserTokens(user),
                    decimal.Parse(user.TwitterUserId)).ResponseObject;

            string originalProfileImagePath = GetReasonablySmallProfilePictureURL(twitterUser.ProfileImageLocation);

            FileContentResult result = null;
            switch (previewType)
            {
                case BadgeType.BasicBlack:
                    result = GetBasicBlackPreviewImageResult(originalProfileImagePath);
                    break;
                case BadgeType.BasicRed:
                    result = GetBasicRedPreviewImageResult(originalProfileImagePath);
                    break;
                case BadgeType.ItsTime:
                    result = new FileContentResult(System.IO.File.ReadAllBytes(Server.MapPath("~/Images/restore-full.jpg")), "image/jpg");
                    break;
            }

            Session[TWITTER_PREVIEW_IMAGE] = result.FileContents;
            return result;
        }

        private FileContentResult GetBasicRedPreviewImageResult(string originalProfileImagePath)
        {
            using (Bitmap original = LoadImageFromURL(originalProfileImagePath))
            {
                using (Graphics graphics = Graphics.FromImage(original))
                {

                    // resize image to 128x128
                    int scaleHeight = 128;
                    int scaleWidth = 128;
                    int boxHeight = 41;
                    using (Bitmap newImage = new Bitmap(scaleWidth, scaleHeight))
                    {
                        using (Graphics gr = Graphics.FromImage(newImage))
                        {
                            gr.SmoothingMode = SmoothingMode.AntiAlias;
                            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            gr.DrawImage(original, new Rectangle(0, 0, scaleWidth, scaleHeight));

                            gr.FillRectangle(new SolidBrush(Color.FromArgb(204, 0, 0)), 0, newImage.Height - boxHeight, newImage.Width, newImage.Height);

                            using (Font voteFont = new Font("Arial Black", 20, FontStyle.Regular, GraphicsUnit.Pixel))
                            {
                                gr.DrawString("vote", voteFont, Brushes.WhiteSmoke,
                                    new PointF(newImage.Width / 2f, newImage.Height - boxHeight - 4), // add some padding
                                    new StringFormat() { Alignment = StringAlignment.Center });
                            }

                            using (Font rpFont = new Font("Arial Black", 20, FontStyle.Regular, GraphicsUnit.Pixel))
                            {
                                gr.DrawString("RON PAUL", rpFont, Brushes.WhiteSmoke,
                                    new PointF(newImage.Width / 2f, newImage.Height - 25), // add some padding
                                    new StringFormat() { Alignment = StringAlignment.Center });
                            }
                        }

                        using (MemoryStream outStream = new MemoryStream())
                        {
                            newImage.Save(outStream, System.Drawing.Imaging.ImageFormat.Png);
                            byte[] image = outStream.ToArray();
                            return new FileContentResult(image, "image/png");
                        }
                    }

                }
            }
        }

        private FileContentResult GetBasicBlackPreviewImageResult(string originalProfileImagePath)
        {
            using (Bitmap original = LoadImageFromURL(originalProfileImagePath))
            {
                using (Graphics graphics = Graphics.FromImage(original))
                {

                    // resize image to 128x128
                    int scaleHeight = 128;
                    int scaleWidth = 128;
                    int boxHeight = 41;
                    using (Bitmap newImage = new Bitmap(scaleWidth, scaleHeight))
                    {
                        using (Graphics gr = Graphics.FromImage(newImage))
                        {
                            gr.SmoothingMode = SmoothingMode.AntiAlias;
                            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            gr.DrawImage(original, new Rectangle(0, 0, scaleWidth, scaleHeight));

                            gr.FillRectangle(new SolidBrush(Color.Black), 0, newImage.Height - boxHeight, newImage.Width, newImage.Height);

                            using (Font voteFont = new Font("Arial Black", 20, FontStyle.Regular, GraphicsUnit.Pixel))
                            {
                                gr.DrawString("vote", voteFont, Brushes.WhiteSmoke,
                                    new PointF(newImage.Width / 2f, newImage.Height - boxHeight - 4), // add some padding
                                    new StringFormat() { Alignment = StringAlignment.Center });
                            }

                            using (Font rpFont = new Font("Arial Black", 20, FontStyle.Regular, GraphicsUnit.Pixel))
                            {
                                gr.DrawString("RON PAUL", rpFont, Brushes.WhiteSmoke,
                                    new PointF(newImage.Width / 2f, newImage.Height - 25), // add some padding
                                    new StringFormat() { Alignment = StringAlignment.Center });
                            }
                        }

                        using (MemoryStream outStream = new MemoryStream())
                        {
                            newImage.Save(outStream, System.Drawing.Imaging.ImageFormat.Png);
                            byte[] image = outStream.ToArray();
                            return new FileContentResult(image, "image/png");
                        }
                    }

                }
            }
        }

        [HttpGet]
        public ActionResult Callback(string oAuth_Token, string oAuth_Verifier, BadgeType previewType)
        {
            if (string.IsNullOrEmpty(oAuth_Token) || string.IsNullOrEmpty(oAuth_Verifier))
            {
                return new HttpNotFoundResult("todo");
            }

            OAuthTokenResponse tokenResponse = OAuthUtility.GetAccessToken(ConfigurationManager.AppSettings[CUSTOMER_KEY], ConfigurationManager.AppSettings[CUSTOMER_SECRET], oAuth_Token, oAuth_Verifier);

            using (IDocumentSession session = MvcApplication.DocumentStore.OpenSession())
            {
                Models.TwitterAccountModel user = GetTwitterAccountModel(session, tokenResponse.UserId.ToString());

                if (user != null)
                {
                    user.BadgeType = previewType;
                    session.SaveChanges();
                }
                else
                {
                    user = new Models.TwitterAccountModel()
                    {
                        TwitterUserId = tokenResponse.UserId.ToString(),
                        UserName = tokenResponse.ScreenName,
                        TwitterAccessKey = tokenResponse.Token,
                        TwitterAccessSecret = tokenResponse.TokenSecret,
                        BadgeType = previewType
                    };
                    OAuthTokens tokens = GetTwitterUserTokens(user);
                    user.TwitterOriginalProfilePictureURL =
                        GetBiggerProfilePictureURL(TwitterUser.Show(tokenResponse.UserId).ResponseObject.ProfileImageLocation);
                    session.Store(user);

                    session.SaveChanges();
                }

                FormsAuthentication.SetAuthCookie(tokenResponse.UserId.ToString(), false);
            }

            return Redirect(new Uri(Request.Url, string.Concat("/twitter/preview?previewType=", previewType)).ToString());
        }

        public Bitmap LoadImageFromURL(string fromURL)
        {
            const int BUFFER = 4096;

            WebRequest myRequest = WebRequest.Create(fromURL);
            WebResponse myResponse = myRequest.GetResponse();
            using (Stream inStream = myResponse.GetResponseStream())
            {
                using (BinaryReader br = new BinaryReader(inStream))
                {
                    using (MemoryStream memstream = new MemoryStream())
                    {
                        byte[] bytebuffer = new byte[BUFFER];
                        int bytesRead = 0;
                        while ((bytesRead = br.Read(bytebuffer, 0, BUFFER)) > 0)
                        {
                            memstream.Write(bytebuffer, 0, bytesRead);
                        }
                        return new Bitmap(memstream);
                    }
                }
            }
        }

        private Models.TwitterAccountModel GetTwitterAccountModel(IDocumentSession session, string twitterUserId)
        {
            return (from t in session.Query<Models.TwitterAccountModel>()
                              .Customize(q => q.WaitForNonStaleResultsAsOfLastWrite())
                    where t.TwitterUserId == twitterUserId
                    select t).FirstOrDefault();
        }

        private OAuthTokens GetTwitterUserTokens(Models.TwitterAccountModel user)
        {
            return new OAuthTokens()
                    {
                        AccessToken = user.TwitterAccessKey,
                        AccessTokenSecret = user.TwitterAccessSecret,
                        ConsumerKey = ConfigurationManager.AppSettings[CUSTOMER_KEY],
                        ConsumerSecret = ConfigurationManager.AppSettings[CUSTOMER_SECRET]
                    };
        }

        private string GetReasonablySmallProfilePictureURL(string originalImageURL)
        {
            int imgIndex = originalImageURL.LastIndexOf("_normal");
            return string.Format("{0}_reasonably_small{1}",
                    originalImageURL.Substring(0, imgIndex),
                    imgIndex + "_normal".Length + 1 == originalImageURL.Length ?
                        string.Empty :
                        originalImageURL.Substring(imgIndex + "_normal".Length));
        }

        private string GetBiggerProfilePictureURL(string originalImageURL)
        {
            int imgIndex = originalImageURL.LastIndexOf("_normal");
            return string.Format("{0}_bigger{1}",
                    originalImageURL.Substring(0, imgIndex),
                    imgIndex + "_normal".Length + 1 == originalImageURL.Length ?
                        string.Empty :
                        originalImageURL.Substring(imgIndex + "_normal".Length));
        }
    }
}
