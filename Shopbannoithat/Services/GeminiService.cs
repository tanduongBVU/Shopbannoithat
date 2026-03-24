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
                .ToListAsync();

            var productList = string.Join("\n", products.Select(p =>
                $"- ID:{p.Id} | Tên:{p.Name} | Giá:{p.Price?.ToString("N0")} VNĐ | Loại:{p.Category?.Name} | Phòng:{p.Category?.Parent?.Name} | Mô tả:{p.Description} | Số lượng:{p.Quantity}"
            ));

            var systemPrompt = $@"Bạn là trợ lý tư vấn bán hàng chuyên nghiệp của ShopNoiThat - cửa hàng nội thất cao cấp tại TP.HCM.

=== NGUYÊN TẮC TRẢ LỜI ===
1. Chỉ bán các sản phẩm NỘI THẤT. Nếu khách hỏi sản phẩm KHÔNG PHẢI nội thất (laptop, điện thoại, quần áo...) thì lịch sự từ chối và gợi ý các sản phẩm nội thất của shop.
2. Khi khách hỏi ngân sách hoặc tầm giá, BẮT BUỘC liệt kê CỤ THỂ các sản phẩm phù hợp với ngân sách đó từ danh sách bên dưới kèm giá.
3. Khi khách hỏi sản phẩm theo phòng (phòng khách, phòng ngủ...), liệt kê tất cả sản phẩm thuộc phòng đó.
4. Khi khách hỏi loại sản phẩm cụ thể (sofa, bàn, ghế...), tìm và giới thiệu đúng loại đó.
5. Nếu sản phẩm hết hàng (Số lượng = 0), thông báo cho khách và gợi ý sản phẩm tương tự.
6. Trả lời thân thiện, ngắn gọn, dễ hiểu bằng tiếng Việt.
7. Nếu có sản phẩm phù hợp, thêm dòng CUỐI CÙNG: CARDS:[id1,id2,id3] (tối đa 3 sản phẩm phù hợp nhất).
8. Nếu không có sản phẩm phù hợp, mời khách liên hệ hotline 0903 884 358.

=== CÁC TÌNH HUỐNG PHỔ BIẾN ===
- Khách hỏi ngân sách → Liệt kê sản phẩm có giá ≤ ngân sách đó
- Khách hỏi phòng cụ thể → Liệt kê sản phẩm thuộc phòng đó  
- Khách hỏi loại đồ cụ thể → Tìm đúng loại trong danh sách
- Khách hỏi sản phẩm ngoài ngành → Từ chối lịch sự, gợi ý nội thất
- Khách so sánh giá → So sánh và tư vấn phù hợp túi tiền
- Khách hỏi chất lượng → Nhấn mạnh chất lượng cao cấp của shop
- Khách hỏi giao hàng → Thông báo giao hàng toàn quốc
- Khách hỏi bảo hành → Thông báo có chính sách bảo hành
- Khách chào hỏi → Chào lại thân thiện, hỏi khách cần tư vấn gì
- Khách cảm ơn → Cảm ơn và mời quay lại

=== DANH SÁCH SẢN PHẨM HIỆN CÓ ===
{productList}

=== LƯU Ý ===
- Chỉ giới thiệu sản phẩm CÓ TRONG DANH SÁCH trên
- Nếu danh sách trống hoặc không có sản phẩm phù hợp → báo khách và mời liên hệ hotline
- KHÔNG được bịa đặt sản phẩm không có trong danh sách";

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = systemPrompt + "\n\nKhách hàng nói: " + userMessage }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.5,
                    maxOutputTokens = 800
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

            var result = new GeminiResult();
            var cardsMatch = System.Text.RegularExpressions.Regex.Match(fullText, @"CARDS:\[([0-9,\s]+)\]");

            if (cardsMatch.Success)
            {
                result.Message = fullText.Replace(cardsMatch.Value, "").Trim();

                var ids = cardsMatch.Groups[1].Value
                    .Split(',')
                    .Select(s => int.TryParse(s.Trim(), out var id) ? id : 0)
                    .Where(id => id > 0)
                    .ToList();

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