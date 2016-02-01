using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Libreria;
using System.Net.Http.Formatting;
using SOLEMPMobile.Models;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security;

namespace SOLEMPMobile.Controllers
{
    [Authorize]
    [RoutePrefix("api/Users")]
    public class UsersController : ApiController
    {

        DBFHelper dbf = new DBFHelper(Properties.Settings.Default.CaminoComun);

        #region AllUsers
        [HttpGet]
        [Route("AllUsers")]
        public HttpResponseMessage AllUsers()
        {
            var resp = new HttpResponseMessage()
            {
                Content = new StringContent(dbf.getUsers())
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return resp;
        }
        #endregion

        #region getUser(string userName, string Password)
        [HttpPost]
        [Route("getUser")]
        public HttpResponseMessage getUser(LoginData login)
        {
            var userName = login.userName;
            var Password = login.Password;
            var resp = new HttpResponseMessage()
            {
                Content = new StringContent(dbf.getUser(userName, Password))
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return resp;
        }
        #endregion

        #region getAllUserInfo(string userName)
        [HttpPost]
        [Route("getAllUserInfo")]
        public HttpResponseMessage getAllUserInfo(User userID)
        {
            var userName = userID.userName;
            var resp = new HttpResponseMessage()
            {
                Content = new StringContent(dbf.getAllUserInfo(userName))
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return resp;
        }
        #endregion

        #region getUserProfileByCompanyID(string userName, string Password)
        [HttpPost ]
        [Route("getUserProfileByCompanyID")]
        public HttpResponseMessage getUserProfileByCompanyID(CompanyData companyData)
        {
            var resp = new HttpResponseMessage()
            {
                Content = new StringContent(dbf.getUserProfileByCompanyID(companyData.userName, companyData.companyID))
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return resp;
        }
        #endregion

        #region getDataForMainScreen(string userName, string companyID)
        [HttpPost]
        [Route("getDataForMainScreen")]
        public HttpResponseMessage getDataForMainScreen(CompanyData companyData)
        {
            var resp = new HttpResponseMessage()
            {
                Content = new StringContent(dbf.getDataForMainScreen(companyData.userName, companyData.companyID))
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return resp;
        }
        #endregion

        #region getProgPagByStatus(string status, string companyID)
        [HttpPost]
        [Route("getProgPagByStatus")]
        public HttpResponseMessage getProgPagByStatus(StatusData statusData)
        {
            var resp = new HttpResponseMessage()
            {
                Content = new StringContent(dbf.getProgPagByStatus(statusData.status, statusData.companyID))
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return resp;
        }
        #endregion

        #region listProgPagByStatus(string status, string companyID)
        [HttpPost]
        [Route("listProgPagByStatus")]
        public HttpResponseMessage listProgPagByStatus(StatusData statusData)
        {
            var resp = new HttpResponseMessage()
            {
                Content = new StringContent(dbf.listProgPagByStatus(statusData.status, statusData.companyID))
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return resp;
        }
        #endregion

        #region listDetailProPagByID(string idProgPag, string companyID)
        [HttpPost]
        [Route("listDetailProPagByID")]
        public HttpResponseMessage listDetailProPagByID(DetailProgPagData detailData)
        {
            var resp = new HttpResponseMessage()
            {
                Content = new StringContent(dbf.listDetailProPagByID(detailData.idProgPag, detailData.companyID))
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return resp;
        }
        #endregion

        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }






        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

    }
}
