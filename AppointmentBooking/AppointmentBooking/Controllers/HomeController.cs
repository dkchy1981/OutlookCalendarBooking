using System;
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

namespace AppointmentBooking.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            string strDomain = "baroda";
            string strUserId = "dc";
            string strPassword = "password*991234#1";
            NetworkCredential _objNetWorkC = new NetworkCredential(strUserId, strPassword, strDomain);
            if (LogonValid(strUserId, strDomain, strPassword))
            {
                string str = "sasa"; 
            }


            return View();
        }

        public static bool LogonValid(string userName, string domain, string password)
        {
            DirectoryEntry de = new DirectoryEntry(null, domain +
              "\\" + userName, password);
            try
            {
                object o = de.NativeObject;
                DirectorySearcher ds = new DirectorySearcher(de);
                ds.Filter = "samaccountname=" + userName;
                ds.PropertiesToLoad.Add("cn");
                SearchResult sr = ds.FindOne();
                if (sr == null) throw new Exception();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool AuthenticateAndGetUserDataFromAD(string strusername, string strDomain, string strPassword)
        {
            string strRootDN = string.Empty;
            DirectoryEntry objDseSearchRoot = null, objDseUserEntry = null;
            DirectorySearcher objDseSearcher = null;
            SearchResultCollection objResults = null;
            string strLDAPPath = string.Empty;
            try
            {
                /* Give LDAP Server IP along with OU
                 * e.g : LDAP://29.29.29.29:389/DC=YourDomain,DC=com"
                 */
                strLDAPPath = "ldap://192.168.0.251:389/OU=sfw staff,CN=baroda,CN=sfwltd,CN=co,CN=uk";
                string strDomainname = strDomain;
                objDseSearchRoot = new DirectoryEntry(strLDAPPath, strDomainname + "\\" + strusername, strPassword, AuthenticationTypes.None);
                strRootDN = objDseSearchRoot.Properties["defaultNamingContext"].Value as string;
                objDseSearcher = new DirectorySearcher(objDseSearchRoot);
                objDseSearcher.SearchScope = SearchScope.Subtree;
                objDseSearcher.CacheResults = false;

                objResults = objDseSearcher.FindAll();
                if (objResults.Count > 0)
                {
                    objDseUserEntry = objResults[0].GetDirectoryEntry();
                }
                if (objDseUserEntry == null)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return false; ;
            }
            finally
            {
                //Dipose Object Over Here
            }
            return true;
        }

    }
}