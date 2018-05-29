using AppointmentBooking.Models;
using System.Web.Mvc;
using System;
using System.Net.Http;
using System.Threading.Tasks;

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

                    Task<HttpResponseMessage> response = client.PostAsJsonAsync<UserLoginInfo>("http://localhost/APIForCalandarOperations/api/Calendar/ValidateUser", userInfo);
                    response.Wait();
                    if (response.Result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
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
    }
}
