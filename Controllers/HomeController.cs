using FinalProject.Helpers;
using FinalProject.Models;
using FinalProject.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Security.Claims;

namespace FinalProject.Controllers
{
    [Authorize]
    public class HomeController(ConnectDb db, ILogger<HomeController> logger/*, IMemoryCache memoryCache, SendEmailHelper sendEmailHelper*/) : Controller
    {
        private readonly ConnectDb _db = db;
        private readonly ILogger<HomeController> _logger = logger;
        //private readonly IMemoryCache _memoryCache = memoryCache;
        //private readonly SendEmailHelper _sendEmailHelper = sendEmailHelper;

        private async Task<bool> CheckWarehouseAccessRights(Guid employeeId, string warehouseCode)
        {
            return await (from ew in _db.EmployeeWarehouse
                          where ew.EmployeeId == employeeId
                          && ew.Warehouse!.WarehouseCode == warehouseCode
                          && ew.Approval == true
                          select ew).AnyAsync();
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var employee = await (from e in _db.Employee
                                          join pos in _db.Position on e.PositionId equals pos.PositionId
                                          where e.Username == model.Username
                                          select new
                                          {
                                              e.EmployeeId,
                                              e.EmployeeName,
                                              e.Username,
                                              e.Salt,
                                              e.Password,
                                              pos.PositionName,
                                              e.Email
                                          }).FirstOrDefaultAsync();

                    if (employee != null)
                    {
                        var storedSalt = Convert.FromBase64String(employee.Salt);
                        var storedPassword = employee.Password;

                        var isPasswordValid = GeneratePassword.VerifyPassword(model.Password, storedSalt, storedPassword);

                        if (isPasswordValid)
                        {
                            var hasAcess = await CheckWarehouseAccessRights(employee.EmployeeId, model.WarehouseCode);

                            if (!hasAcess)
                            {
                                ModelState.AddModelError(string.Empty, "คุณไม่มีสิทธิ์เข้าถึงคลังสินค้านี้");
                                return View(model);
                            }

                            var claims = new List<Claim>
                            {
                                new(ClaimTypes.NameIdentifier, employee.EmployeeId.ToString()),
                                new(ClaimTypes.Name, employee.EmployeeName.ToString()),
                                new(ClaimTypes.Role, employee.PositionName.ToString()),
                                new(ClaimTypes.Email, employee.Email),
                                new("WarehouseCode", model.WarehouseCode),
                            };

                            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                            var authProperties = new AuthenticationProperties
                            {
                                IsPersistent = model.RememberMe,
                                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(14) : DateTimeOffset.UtcNow.AddMinutes(30)
                            };

                            await HttpContext.SignInAsync(CookieAuthenticationDefaults
                                .AuthenticationScheme, new ClaimsPrincipal(claimsIdentity),
                                authProperties);

                            return RedirectToAction("Index", "Home");
                        }
                    }
                }

                ModelState.AddModelError(string.Empty, "รหัสผู้ใช้หรือรหัสผ่านไม่ถูกต้อง หรือคุณไม่มีสิทธิ์เข้าถึงคลังสินค้านี้");
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดในการเข้าระบบ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการเข้าระบบ");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Home");
        }

        //[AllowAnonymous]
        //public IActionResult ForgotPassword()
        //{
        //    return View();
        //}
        //[AllowAnonymous]
        //[HttpPost]
        //public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = await (from e in _db.Employee
        //                          where e.Email == model.Email
        //                          select new
        //                          {
        //                              e.EmployeeName,
        //                              e.Email
        //                          }).FirstOrDefaultAsync();

        //        if (user != null)
        //        {
        //            var token = Guid.NewGuid().ToString();

        //            _memoryCache.Set(token, user.Email, TimeSpan.FromMinutes(30));

        //            var resetLink = Url.Action("ResetPassword", "Home",
        //                new { token, email = user.Email }, 
        //                protocol: HttpContext.Request.Scheme);

        //            await _sendEmailHelper.SendEmailReretPassword(model.Email, user.EmployeeName, resetLink!);

        //            TempData["Success"] = "ลิงก์สำหรับการรีเซ็ตรหัสผ่านได้ถูกส่งไปยังอีเมลของคุณ";
        //            return RedirectToAction("Login", "Home");
        //        }
        //    }

        //    ModelState.AddModelError(string.Empty, "ไม่พบอีเมลนี้ในระบบ");
        //    return View(model);
        //}

        //[AllowAnonymous]
        //public IActionResult ResetPassword(string token, string email)
        //{
        //    if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
        //    {
        //        return RedirectToAction("Login", "Home");
        //    }

        //    var model = new ResetPasswordViewModel { Token = token, Email = email };
        //    return View(model);
        //}
        //[AllowAnonymous]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = await _db.Employee.Where(x => x.Email == model.Email).FirstOrDefaultAsync();

        //        if (user != null)
        //        {
        //            var isValidToken = _memoryCache.TryGetValue(model.Token!, out string? cachedEmail);

        //            if (isValidToken || cachedEmail == model.Email)
        //            {
        //                var newSalt = GeneratePassword.GenerateSalt();

        //                user.Salt = Convert.ToBase64String(newSalt);
        //                user.Password = GeneratePassword.HashPassword(model.Password!, newSalt);
        //                user.ModifiedDate = DateTimeOffset.Now;

        //                _db.Employee.Update(user);
        //                await _db.SaveChangesAsync();
        //                _memoryCache.Remove(model.Token!);
        //                TempData["Success"] = "รหัสผ่านของคุณได้ถูกเปลี่ยนเรียบร้อยแล้ว";
        //                return RedirectToAction("Login", "Account");
        //            }
        //        }
        //    }

        //    return View(model);
        //}

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
