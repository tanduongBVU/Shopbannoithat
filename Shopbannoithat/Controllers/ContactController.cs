using Microsoft.AspNetCore.Mvc;
using Shopbannoithat.Services;

namespace Shopbannoithat.Controllers
{
    public class ContactController : Controller
    {
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;

        public ContactController(EmailService emailService, IConfiguration config)
        {
            _emailService = emailService;
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Send(string name, string phone,
                                              string email, string subject, string message)
        {
            try
            {
                var adminEmail = _config["EmailSettings:SenderEmail"];

                // Email gửi cho Admin
                var adminBody = $@"
                    <div style='font-family:Arial; padding:20px; background:#f8f5f0;'>
                        <div style='max-width:600px; margin:auto; background:#fff;
                                    border-radius:12px; overflow:hidden;
                                    box-shadow:0 4px 20px rgba(0,0,0,0.1);'>
                            <div style='background:#3a2a1a; color:#fff; padding:24px;'>
                                <h2 style='margin:0;'>📩 Tin nhắn liên hệ mới</h2>
                            </div>
                            <div style='padding:28px;'>
                                <table style='width:100%; border-collapse:collapse;'>
                                    <tr>
                                        <td style='padding:10px; font-weight:bold; color:#6b5a4a; width:140px;'>Họ tên:</td>
                                        <td style='padding:10px; color:#2c1e10;'>{name}</td>
                                    </tr>
                                    <tr style='background:#f8f5f0;'>
                                        <td style='padding:10px; font-weight:bold; color:#6b5a4a;'>Email:</td>
                                        <td style='padding:10px; color:#2c1e10;'>{email}</td>
                                    </tr>
                                    <tr>
                                        <td style='padding:10px; font-weight:bold; color:#6b5a4a;'>SĐT:</td>
                                        <td style='padding:10px; color:#2c1e10;'>{phone ?? "Không có"}</td>
                                    </tr>
                                    <tr style='background:#f8f5f0;'>
                                        <td style='padding:10px; font-weight:bold; color:#6b5a4a;'>Chủ đề:</td>
                                        <td style='padding:10px; color:#2c1e10;'>{subject ?? "Không có"}</td>
                                    </tr>
                                    <tr>
                                        <td style='padding:10px; font-weight:bold; color:#6b5a4a; vertical-align:top;'>Nội dung:</td>
                                        <td style='padding:10px; color:#2c1e10;'>{message}</td>
                                    </tr>
                                </table>
                            </div>
                            <div style='background:#f8f5f0; padding:16px; text-align:center;
                                        color:#9e8672; font-size:13px;'>
                                © 2026 ShopNoiThat — Tin nhắn từ trang Liên hệ
                            </div>
                        </div>
                    </div>";

                await _emailService.SendAsync(adminEmail,
                    $"[ShopNoiThat] Liên hệ mới từ {name}", adminBody);

                // Email xác nhận gửi cho khách
                var customerBody = $@"
                    <div style='font-family:Arial; padding:20px; background:#f8f5f0;'>
                        <div style='max-width:600px; margin:auto; background:#fff;
                                    border-radius:12px; overflow:hidden;
                                    box-shadow:0 4px 20px rgba(0,0,0,0.1);'>
                            <div style='background:#3a2a1a; color:#fff; padding:24px;'>
                                <h2 style='margin:0;'>🪑 ShopNoiThat</h2>
                            </div>
                            <div style='padding:28px;'>
                                <h3 style='color:#2c1e10;'>Xin chào {name}!</h3>
                                <p style='color:#6b5a4a; line-height:1.8;'>
                                    Chúng tôi đã nhận được tin nhắn của bạn và sẽ
                                    phản hồi trong vòng <strong>24 giờ làm việc</strong>.
                                </p>
                                <div style='background:#f8f5f0; border-radius:10px;
                                            padding:18px; margin:20px 0;
                                            border-left:4px solid #e05c2a;'>
                                    <p style='margin:0; color:#6b5a4a; font-size:14px;'>
                                        <strong>Nội dung bạn gửi:</strong><br/>{message}
                                    </p>
                                </div>
                                <p style='color:#6b5a4a;'>
                                    Nếu cần hỗ trợ khẩn cấp, vui lòng gọi hotline:
                                    <strong style='color:#e05c2a;'>0903 884 358</strong>
                                </p>
                            </div>
                            <div style='background:#3a2a1a; padding:16px; text-align:center; color:#a89888; font-size:13px;'>
                                © 2026 ShopNoiThat. All rights reserved.
                            </div>
                        </div>
                    </div>";

                await _emailService.SendAsync(email,
                    "ShopNoiThat — Xác nhận đã nhận tin nhắn của bạn", customerBody);

                TempData["Success"] = "true";
            }
            catch
            {
                TempData["Error"] = "true";
            }

            return RedirectToAction("Index");
        }
    }
}
