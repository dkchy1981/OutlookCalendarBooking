using AppointmentBooking.Models;
using System.Web.Mvc;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Security;
using System.Security.Principal;
using System.Configuration;

namespace AppointmentBooking.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Login()
        {
            ViewBag.Message = "";
            return View();
        }

        // POST: Login/Create
        [HttpPost]
        public ActionResult ValidateUser(FormCollection collection)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View();

                using (var client = new HttpClient())
                {
                    UserLoginInfo userInfo = new UserLoginInfo();
                    userInfo.userName = collection["userName"];
                    userInfo.password = collection["password"];
                    string apiURL=ConfigurationManager.AppSettings["APIRefenenceURL"];

                    Task<HttpResponseMessage> response = client.PostAsJsonAsync<UserLoginInfo>(apiURL+"ValidateUser", userInfo);
                    response.Wait();
                    if (response.Result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Session["UserName"] = userInfo.userName;
                        FormsAuthentication.SignOut();
                        FormsAuthentication.SetAuthCookie(userInfo.userName, true);
                        return Redirect("~/Home/Index");
                    }
                    else
                    {
                        ModelState.AddModelError("FullName", "Invalid Credentials");
                        return View("Login");
                    }
                }
            }
            catch
            {
                return View("Login");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            try
            {
                // First we clean the authentication ticket like always  
                //required NameSpace: using System.Web.Security;  
                FormsAuthentication.SignOut();

                // Second we clear the principal to ensure the user does not retain any authentication  
                //required NameSpace: using System.Security.Principal;  
                HttpContext.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);

                Session.Clear();
                System.Web.HttpContext.Current.Session.RemoveAll();

                // Last we redirect to a controller/action that requires authentication to ensure a redirect takes place  
                // this clears the Request.IsAuthenticated flag since this triggers a new request  
                return RedirectToLocal();
            }
            catch
            {
                throw;
            }
        }

        private ActionResult RedirectToLocal(string returnURL = "")
        {
            try
            {
                // If the return url starts with a slash "/" we assume it belongs to our site  
                // so we will redirect to this "action"  
                if (!string.IsNullOrWhiteSpace(returnURL) && Url.IsLocalUrl(returnURL))
                    return Redirect(returnURL);

                // If we cannot verify if the url is local to our host we redirect to a default location  
                return RedirectToAction("Login", "Login");
            }
            catch
            {
                throw;
            }
        }
    }
}
