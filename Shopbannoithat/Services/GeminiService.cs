using System.Text;
using System.Text.Json;
using Shopbannoithat.Data;
using Microsoft.EntityFrameworkCore;

namespace Shopbannoithat.Services
{
    public class ProductCard
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Price { get; set; }
        public string Image { get; set; }
    }

    public class GeminiResult
    {
        public string Message { get; set; }
        public List<ProductCard> Cards { get; set; } = new();
    }

    public class GeminiService
    {
        private readonly string _apiKey;
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        public GeminiService(IConfiguration config, ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _apiKey = config["Gemini:ApiKey"] ?? "";
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<GeminiResult> AskAsync(string userMessage)
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .ThenInclude(c => c.Parent)
                .Take(50)
                .ToListAsync();

            var productList = string.Join("\n", products.Select(p =>
                $"- ID:{p.Id} | {p.Name} | Giá: {p.Price?.ToString("N0")} VNĐ | Loại: {p.Category?.Name} | Phòng: {p.Category?.Parent?.Name} | Mô tả: {p.Description}"
            ));

            var systemPrompt = $@"Bạn là trợ lý tư vấn bán hàng của ShopNoiThat - cửa hàng nội thất cao cấp tại TP.HCM.
Nhiệm vụ của bạn là tư vấn, giới thiệu sản phẩm nội thất cho khách hàng một cách thân thiện và chuyên nghiệp.
Trả lời bằng tiếng Việt, ngắn gọn, dễ hiểu.
Nếu khách hỏi về sản phẩm, hãy giới thiệu sản phẩm phù hợp.
Nếu có sản phẩm phù hợp, hãy thêm dòng cuối cùng theo định dạng: CARDS:[id1,id2,id3] (tối đa 3 sản phẩm).
Nếu không có sản phẩm phù hợp, hãy tư vấn chung và mời khách liên hệ hotline 0903 884 358.

DANH SÁCH SẢN PHẨM HIỆN CÓ:
{productList}";

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = systemPrompt + "\n\nKhách hàng hỏi: " + userMessage }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 500
                }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
            for (int i = 0; i < 3; i++)
            {
                response = await _httpClient.PostAsync(url, content);
                if (response.StatusCode != System.Net.HttpStatusCode.TooManyRequests)
                    break;
                await Task.Delay(2000);
            }

            var responseJson = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseJson);
            var fullText = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "";

            // Tách CARDS ra khỏi message
            var result = new GeminiResult();
            var cardsMatch = System.Text.RegularExpressions.Regex.Match(fullText, @"CARDS:\[([0-9,]+)\]");

            if (cardsMatch.Success)
            {
                // Xóa dòng CARDS khỏi message
                result.Message = fullText.Replace(cardsMatch.Value, "").Trim();

                // Lấy danh sách ID
                var ids = cardsMatch.Groups[1].Value
                    .Split(',')
                    .Select(s => int.TryParse(s.Trim(), out var id) ? id : 0)
                    .Where(id => id > 0)
                    .ToList();

                // Lấy thông tin sản phẩm
                var cardProducts = await _context.Products
                    .Where(p => ids.Contains(p.Id))
                    .ToListAsync();

                result.Cards = ids
                    .Select(id => cardProducts.FirstOrDefault(p => p.Id == id))
                    .Where(p => p != null)
                    .Select(p => new ProductCard
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price?.ToString("N0") + " ₫",
                        Image = p.Image
                    })
                    .ToList();
            }
            else
            {
                result.Message = fullText;
            }

            return result;
        }
    }
}