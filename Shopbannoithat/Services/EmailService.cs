using MailKit.Net.Smtp;
using MimeKit;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Shopbannoithat.Models;
using Microsoft.Extensions.Options;

namespace Shopbannoithat.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
            QuestPDF.Settings.License = LicenseType.Community;   // Dùng miễn phí cho bài tập
        }

        public async Task SendOrderInvoiceAsync(Order order)
        {
            var pdfBytes = GenerateInvoicePdf(order);

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            email.To.Add(new MailboxAddress(order.Name, order.UserId));
            email.Subject = $"Xác nhận đơn hàng #{order.Id} từ ShopNoiThat";

            var builder = new BodyBuilder
            {
                HtmlBody = $"<h2>Cảm ơn bạn đã đặt hàng, {order.Name}!</h2>" +
                           $"<p>Đơn hàng #{order.Id} ghi nhận lúc {order.OrderPlaced:dd/MM/yyyy HH:mm}.</p>" +
                           $"<p>ShopNoiThat sẽ liên hệ và giao hàng sớm nhất.</p>"
            };

            builder.Attachments.Add($"hoa-don-{order.Id}.pdf", pdfBytes, new ContentType("application", "pdf"));

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        private byte[] GenerateInvoicePdf(Order order)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    // Header
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Text("ShopNoiThat").FontSize(26).SemiBold().FontColor(Colors.Blue.Darken2);
                        row.RelativeItem().AlignRight().Text($"Đơn hàng #{order.Id}").FontSize(14).SemiBold();
                    });

                    page.Content().PaddingTop(20).Column(col =>
                    {
                        col.Item().Text($"Cảm ơn bạn đã đặt hàng, {order.Name}!").FontSize(16).SemiBold().FontColor(Colors.Purple.Darken2);
                        col.Item().Text($"Đơn hàng #{order.Id} ghi nhận lúc {order.OrderPlaced:dd/MM/yyyy HH:mm}.").FontSize(12);

                        col.Item().PaddingTop(15).Text("Thông tin giao hàng:").SemiBold();
                        col.Item().Text(order.Name);
                        col.Item().Text($"{order.Address1}, {order.Address2}");
                        col.Item().Text(order.City);

                        // ================== BẢNG SẢN PHẨM ==================
                        col.Item().PaddingTop(25).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);     // STT
                                columns.RelativeColumn(5);      // Tên sản phẩm
                                columns.ConstantColumn(70);     // Số lượng
                                columns.ConstantColumn(120);    // Đơn giá
                            });

                            // Header bảng
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("STT").SemiBold();
                                header.Cell().Element(CellStyle).Text("Sản phẩm").SemiBold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Số lượng").SemiBold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Đơn giá").SemiBold();
                            });

                            int stt = 1;
                            foreach (var detail in order.OrderDetails)
                            {
                                string productName = detail.Product?.Name ?? $"Sản phẩm {detail.ProductId}";

                                if (!string.IsNullOrEmpty(detail.VariantSize) ||
                                    !string.IsNullOrEmpty(detail.VariantColor) ||
                                    !string.IsNullOrEmpty(detail.VariantMaterial))
                                {
                                    productName += " (";
                                    if (!string.IsNullOrEmpty(detail.VariantSize)) productName += detail.VariantSize + " ";
                                    if (!string.IsNullOrEmpty(detail.VariantColor)) productName += detail.VariantColor + " ";
                                    if (!string.IsNullOrEmpty(detail.VariantMaterial)) productName += detail.VariantMaterial;
                                    productName = productName.Trim() + ")";
                                }

                                table.Cell().Element(CellStyle).Text(stt++.ToString());
                                table.Cell().Element(CellStyle).Text(productName);
                                table.Cell().Element(CellStyle).AlignRight().Text(detail.Quantity.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text($"{detail.Price:N0} đ");
                            }

                            // ================== DÒNG TỔNG TIỀN (đã sửa lỗi ColumnSpan) ==================
                            table.Cell()                                     // ô trống cho STT
                                .ColumnSpan(3)
                                .Element(CellStyle)
                                .AlignRight()
                                .Text("Tổng thanh toán:")
                                .SemiBold()
                                .FontSize(13);

                            table.Cell().Element(CellStyle)                    // ô tổng tiền
                                .AlignRight()
                                .Text($"{order.TotalPrice:N0} đ")
                                .FontColor(Colors.Red.Medium)
                                .SemiBold()
                                .FontSize(13);
                        });
                    });

                    // Footer
                    page.Footer().AlignCenter().Text("ShopNoiThat - Cảm ơn bạn đã tin tưởng và ủng hộ!").FontSize(10).Light();
                });
            });

            return document.GeneratePdf();
        }

        private static IContainer CellStyle(IContainer container) =>
            container.Border(1).BorderColor(Colors.Grey.Medium).Padding(8);
    }
}