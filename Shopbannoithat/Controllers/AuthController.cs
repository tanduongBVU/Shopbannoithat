using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Shopbannoithat.Data;
using Shopbannoithat.Models;

namespace Shopbannoithat.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            // ❗ check validation
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Users
                .FirstOrDefault(x => x.Email == model.Email && x.Password == model.Password);

            if (user == null)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
                return View(model);
            }

            // lưu session
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", user.Email);

            // chuyển trang
            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            else if (user.Role == "Staff")
            {
                return RedirectToAction("Index", "Order", new { area = "Staff" });
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _context.Users
                    .FirstOrDefault(x => x.Email == model.Email);

                if (existingUser != null)
                {
                    ViewBag.Error = "Email đã tồn tại";
                    return View(model);
                }

                var user = new User
                {
                    Email = model.Email,
                    Password = model.Password,
                    Phone = model.Phone,
                    Role = "Customer"
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("Login", "Auth");
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();   // xóa session
            return RedirectToAction("Login", "Auth");  // quay về trang login
        }


        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Users
                .FirstOrDefault(x => x.Email == model.Email && x.Phone == model.Phone);

            if (user == null)
            {
                // ❗ cách chuẩn (gắn lỗi vào form)
                ModelState.AddModelError("", "Email hoặc số điện thoại không đúng");
                return View(model);
            }

            user.Password = model.NewPassword;
            _context.SaveChanges();

            TempData["Success"] = "Đổi mật khẩu thành công";
            return RedirectToAction("Login");
        }
    }
}
