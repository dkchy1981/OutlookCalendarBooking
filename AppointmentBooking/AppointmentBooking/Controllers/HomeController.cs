﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using System.DirectoryServices.Protocols;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.DirectoryServices;
using System.Web.Security;
using System.Security.Principal;

namespace AppointmentBooking.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                return View();
            }
            else
            {
                return Logout();
            }
        }


        [HttpGet]
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