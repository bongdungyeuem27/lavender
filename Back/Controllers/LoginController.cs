using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Back.ModelDTO;
using Back.Models;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Cors;
using Back.Common;
using Microsoft.EntityFrameworkCore;
using Back.Services;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace Back.Controllers
{
    //[EnableCors(origins: "*", headers: "accept,content-type,origin,x-my-header", methods: "*")]
    [ApiController]
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender sendMailService;
        private readonly lavenderContext lavenderContext;

        public LoginController(ILogger<LoginController> logger, lavenderContext lavenderContext, IConfiguration configuration, IEmailSender sendMailService)
        {
            _logger = logger;
            _configuration = configuration;
            this.lavenderContext = lavenderContext;
            this.sendMailService = sendMailService; 
        }

        [Route("/login")]
        [HttpPost]
        public async Task<IActionResult> LoginKhachhangAsync(LoginForm loginForm)
        {
            if (loginForm.loaitaikhoan.Equals("khachhang"))
            {
                var taikhoan = await (from t in lavenderContext.Taikhoankhachhang
                                      where t.Username.Equals(loginForm.username)
                                      && t.Password.Equals(loginForm.password)
                                      select t).FirstOrDefaultAsync();
                if (taikhoan == null) return Unauthorized();

                var khachhang = await (from x in lavenderContext.Khachhang
                                       where x.Makhachhang == taikhoan.Makhachhang
                                       select x).FirstOrDefaultAsync();
                if (khachhang == null) return StatusCode(401);
                var token = MyTokenHandler.GenerateAccessToken(loginForm.username, MyTokenHandler.GUEST, _configuration);

                if (loginForm.savelogin)
                {
                    var refreshtoken = MyTokenHandler.GenerateRefreshToken(loginForm.username, MyTokenHandler.GUEST, _configuration);
                    Khachhangdangnhap khachhangdangnhap = new Khachhangdangnhap();
                    khachhangdangnhap.Refreshtoken = refreshtoken;
                    khachhangdangnhap.Makhachhang = khachhang.Makhachhang;
                    await lavenderContext.AddAsync(khachhangdangnhap);
                    await lavenderContext.SaveChangesAsync();
                    return StatusCode(200, Json(new { token = token, refreshtoken = refreshtoken, makhachhang = khachhang.Makhachhang }));
                }


                
                return StatusCode(200, Json(new { token = token, makhachhang = khachhang.Makhachhang }));
            }
            else if (loginForm.loaitaikhoan.Equals("nhanvien"))
            {
                var taikhoan = await (from t in lavenderContext.Taikhoannhanvien
                                      where t.Username.Equals(loginForm.username)
                                      && t.Password.Equals(loginForm.password)
                                      select t).FirstOrDefaultAsync();
                if (taikhoan == null) return Unauthorized();


                var nhanvien = await (from x in lavenderContext.Nhanvien
                                       where x.Manhanvien == taikhoan.Manhanvien
                                       select x).FirstOrDefaultAsync();
                if (nhanvien == null) return StatusCode(401);

                string role = MyTokenHandler.STAFF;
                if (nhanvien.Chucvu.Equals("Giám đốc") || nhanvien.Chucvu.Equals("Quản lý")) role = MyTokenHandler.ADMINISTRATOR;
                var token = MyTokenHandler.GenerateAccessToken(loginForm.username, role, _configuration);

                if (loginForm.savelogin)
                {
                    var refreshtoken = MyTokenHandler.GenerateRefreshToken(loginForm.username, role, _configuration);
                    Nhanviendangnhap nhanviendangnhap = new Nhanviendangnhap();
                    nhanviendangnhap.Refreshtoken = refreshtoken;
                    nhanviendangnhap.Manhanvien = nhanvien.Manhanvien;
                    await lavenderContext.AddAsync(nhanviendangnhap);
                    await lavenderContext.SaveChangesAsync();
                    return StatusCode(200, Json(new { token = token, refreshtoken = refreshtoken, manhanvien = nhanviendangnhap.Manhanvien }));
                }

                return StatusCode(200, Json(new { token = token, manhanvien = nhanvien.Manhanvien }));
            }
            return StatusCode(204);
        }

        [Route("/refresh-token")]
        [HttpGet]
        //[Authorize(Policy = "Bearer")]
        public async Task<IActionResult> RefreshToken(string refreshtoken)
        {
            Dictionary<string, string> payload = MyTokenHandler.TokenPayloadHandler(refreshtoken);
            if (payload["role"].Equals(MyTokenHandler.GUEST))
            {
                if (payload.IsNullOrEmpty()) return StatusCode(401);
                var taikhoan = await (from x in lavenderContext.Taikhoankhachhang
                                      where x.Username.Equals(payload["unique_name"])
                                      select x).FirstOrDefaultAsync();
                if (taikhoan == null) return StatusCode(401);

                var khachhangdangnhap = await (from x in lavenderContext.Khachhangdangnhap
                                               where x.Refreshtoken.Equals(refreshtoken)
                                               select x).FirstOrDefaultAsync();
                if (khachhangdangnhap == null) return StatusCode(401);

                var newrefreshtoken = MyTokenHandler.GenerateRefreshToken(taikhoan.Username, MyTokenHandler.GUEST, _configuration);

                var newkhachhangdangnhap = new Khachhangdangnhap();
                newkhachhangdangnhap.Refreshtoken = newrefreshtoken;
                newkhachhangdangnhap.Makhachhang = khachhangdangnhap.Makhachhang;
                lavenderContext.Remove(khachhangdangnhap);

                await lavenderContext.AddAsync(newkhachhangdangnhap);
                await lavenderContext.SaveChangesAsync();

                var token = MyTokenHandler.GenerateAccessToken(taikhoan.Username, MyTokenHandler.GUEST, _configuration);
                return StatusCode(200, Json(new { token = token, refreshtoken = newrefreshtoken, makhachhang = taikhoan.Makhachhang }));
            }
            else if (payload["role"].Equals(MyTokenHandler.ADMINISTRATOR)|| payload["role"].Equals(MyTokenHandler.STAFF))
            {
                if (payload.IsNullOrEmpty()) return StatusCode(401);
                var taikhoan = await (from x in lavenderContext.Taikhoannhanvien
                                      where x.Username.Equals(payload["unique_name"])
                                      select x).FirstOrDefaultAsync();
                if (taikhoan == null) return StatusCode(401);

                var nhanviendangnhap = await (from x in lavenderContext.Nhanviendangnhap
                                               where x.Refreshtoken.Equals(refreshtoken)
                                               select x).FirstOrDefaultAsync();
                if (nhanviendangnhap == null) return StatusCode(401);

                var newrefreshtoken = MyTokenHandler.GenerateRefreshToken(taikhoan.Username, payload["role"], _configuration);

                var newnhanviendangnhap = new Nhanviendangnhap();
                newnhanviendangnhap.Refreshtoken = newrefreshtoken;
                newnhanviendangnhap.Manhanvien = nhanviendangnhap.Manhanvien;
                lavenderContext.Remove(nhanviendangnhap);

                await lavenderContext.AddAsync(newnhanviendangnhap);
                await lavenderContext.SaveChangesAsync();

                var token = MyTokenHandler.GenerateAccessToken(taikhoan.Username, payload["role"], _configuration);
                return StatusCode(200, Json(new { token = token, refreshtoken = newrefreshtoken, manhanvien = taikhoan.Manhanvien }));
            }
            return StatusCode(204);
        }

        [Route("/test")]
        [AllowAnonymous]
        public async Task<IActionResult> test()
        {
            //await sendMailService.SendEmailAsync("khanhlemaiduy123@gmail.com", "test", "a");
            //CookieOptions option = new CookieOptions
            //{
            //    Expires = DateTime.Now.AddHours(4),
            //    Path = "/",
            //    HttpOnly = false,

            //};

            //HttpContext.Session.SetString("khanhzum", "a");
            //var sessionValue = HttpContext.Request.Cookies["token"];
            //Console.WriteLine("session" + sessionValue);


            //CookieOptions option = new CookieOptions
            //{
            //    Expires = DateTime.Now.AddHours(4),
            //    Path = "/",
            //    HttpOnly = false,

            //};
            //Response.Cookies.Append("token", "aa", option);
            //Console.WriteLine("cookie"+HttpContext.Request.Cookies["token"]);
            Response.Cookies.Append("token", "aa");
            var listcokie = HttpContext.Request.Cookies.Select((header) => $"{header.Key}: {header.Value}");
            Console.WriteLine("listcooke" + listcokie.ToString());
            Console.WriteLine("cookie" + HttpContext.Request.Cookies["token"]);
            return StatusCode(200);
        }

        [Route("/logout")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout(JsonElement json)
        {
            if (json.GetString("loaitaikhoan").Equals("khachhang"))
            {
                int makhachhang = int.Parse(json.GetString("ma"));
                string refreshtoken = json.GetString("refreshtoken");

                var khachhangdangnhap = await (from x in lavenderContext.Khachhangdangnhap
                                               where x.Makhachhang == makhachhang
                                               && x.Refreshtoken.Equals(refreshtoken)
                                               select x).FirstOrDefaultAsync();
                if (khachhangdangnhap == null) return StatusCode(404);
                lavenderContext.Remove(khachhangdangnhap);
                await lavenderContext.SaveChangesAsync();
                return StatusCode(200, Json(makhachhang));
            }
            else if (json.GetString("loaitaikhoan").Equals("nhanvien"))
            {
                int manhanvien = int.Parse(json.GetString("ma"));
                string refreshtoken = json.GetString("refreshtoken");

                var nhanviendangnhap = await (from x in lavenderContext.Nhanviendangnhap
                                               where x.Manhanvien == manhanvien
                                               && x.Refreshtoken.Equals(refreshtoken)
                                               select x).FirstOrDefaultAsync();
                if (nhanviendangnhap == null) return StatusCode(404);
                lavenderContext.Remove(nhanviendangnhap);
                await lavenderContext.SaveChangesAsync();
                return StatusCode(200, Json(nhanviendangnhap));
            }
            return StatusCode(204);


        }
    }
}
