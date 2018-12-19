using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ipsi.Models;
using Ipsi.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Ipsi.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace Ipsi.Controllers
{
    public class AuthController : Controller
    {
        private LoginService loginService;

        public AuthController()
        {
            loginService = new LoginService();
        }
        public async Task<IActionResult> Login()
        {
            LoginInfo loginInfo = new LoginInfo();

            loginInfo.Id = IpsiDataConfig.UserID;
            loginInfo.Password = IpsiDataConfig.UserPassword;

            loginInfo.APIKey = IpsiDataConfig.ApiKey;
            loginInfo.isEmployeeLogin = true;

            var result = await this.loginService.Login(loginInfo);

            CookieOptions option = new CookieOptions();

            option.Expires = DateTime.Now.AddMinutes(30);


            var data = this.GetPrincipalFromExpiredToken(result.JwtToken);

            var claimsIdentity = new ClaimsIdentity(
            data.Claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));


            if (User.Identity.IsAuthenticated)
            {
                Response.Cookies.Append("JwtToken", result.JwtToken, option);
                Response.Cookies.Append("RefreshToken", result.RefreshToken, option);
                Response.Cookies.Append("UserType", result.UserType.ToString(), option);
                Response.Cookies.Append("UserId", result.UserId, option);
                Response.Cookies.Append("UserName", result.UserName, option);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginInfo loginInfo)
        {
            var result = await this.loginService.Login(loginInfo);

            var data = this.GetPrincipalFromExpiredToken(result.JwtToken);

            var claimsIdentity = new ClaimsIdentity(
            data.Claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));




            return View();
        }

        [Authorize]
        public IActionResult UserInfo()
        {
            var Name = User.Identity.Name;
            var authenticationType = User.Identity.AuthenticationType;
            var isAuthenticated = User.Identity.IsAuthenticated;

            CookieOptions option = new CookieOptions();

            option.Expires = DateTime.Now.AddMinutes(30);

            Response.Cookies.Append("RefreshToken", "testetset", option);

            string RefreshToken = "None";

            if (Request.Cookies["RefreshToken"] != null)
            {
                RefreshToken = Request.Cookies["RefreshToken"].ToString();
            }
            string UserName = "None";
            if (isAuthenticated)
            {
                UserName = User.Claims.SingleOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
            }




            ViewData["Message"] = string.Format("Name : {0}, authenticationType : {1}, isAuthenticated : {2}, Name : {3}, RefreshToken : {4}",
            Name, authenticationType, isAuthenticated, UserName, RefreshToken);//

            // User.Identity.Name

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            foreach (var cookieKey in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookieKey);
            }

            ViewData["Message"] = "로그아웃 되었습니다.";
            return View();
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(IpsiDataConfig.JwtKey)),
                ValidateLifetime = true //here we are saying that we don't care about the token's expiration date
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}