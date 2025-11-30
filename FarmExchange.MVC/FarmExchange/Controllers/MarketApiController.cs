using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FarmExchange.Controllers
{
    [Route("api/market")]
    [ApiController]
    public class MarketApiController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string API_KEY = "YOUR_API_KEY"; // Placeholder for the user to replace
        private const string BASE_URL = "https://api.apifarmer.com/v0/commodities";

        public MarketApiController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("demand")]
        public async Task<IActionResult> GetMarketDemand()
        {
            List<CommodityData> commodities = new List<CommodityData>();

            try
            {
                var client = _httpClientFactory.CreateClient();
                // We use a short timeout so the page doesn't hang if the API is unreachable or key is invalid
                client.Timeout = TimeSpan.FromSeconds(3);

                var response = await client.GetAsync($"{BASE_URL}?api-key={API_KEY}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiFarmerResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (apiResponse?.Data != null)
                    {
                        commodities = apiResponse.Data;
                    }
                }
                else
                {
                    // Fallback if API fails (e.g. invalid key)
                    commodities = GetSimulatedData();
                }
            }
            catch
            {
                // Fallback on network error
                commodities = GetSimulatedData();
            }

            // Transform to the format expected by the frontend
            var result = commodities.Select(c => new
            {
                Name = c.Commodity,
                DemandLevel = DetermineDemand(c), // Logic to infer demand
                Trend = DetermineTrend(c.DailyChange),
                AveragePrice = c.Price,
                Unit = c.Unit ?? "kg",
                Description = $"Market Price: {c.Price} {c.Currency}"
            }).ToList();

            return Json(result);
        }

        private List<CommodityData> GetSimulatedData()
        {
            return new List<CommodityData>
            {
                new CommodityData { Commodity = "Rice (Palay)", Price = 19.50, DailyChange = 0.5, Unit = "kg", Currency = "PHP" },
                new CommodityData { Commodity = "Corn (Yellow)", Price = 15.25, DailyChange = 0.0, Unit = "kg", Currency = "PHP" },
                new CommodityData { Commodity = "Coconut", Price = 12.00, DailyChange = -0.1, Unit = "pc", Currency = "PHP" },
                new CommodityData { Commodity = "Banana", Price = 35.00, DailyChange = 1.2, Unit = "kg", Currency = "PHP" },
                new CommodityData { Commodity = "Mango", Price = 85.00, DailyChange = 5.0, Unit = "kg", Currency = "PHP" },
                new CommodityData { Commodity = "Onion", Price = 120.00, DailyChange = -15.0, Unit = "kg", Currency = "PHP" }
            };
        }

        private string DetermineTrend(double? change)
        {
            if (!change.HasValue) return "Stable";
            if (change > 0) return "Rising";
            if (change < 0) return "Falling";
            return "Stable";
        }

        private string DetermineDemand(CommodityData c)
        {
            // Simple heuristic: if price is rising, demand is likely high
            if (c.DailyChange > 0) return "High";
            if (c.DailyChange < 0) return "Medium";
            return "High"; // Default to High for stable staples
        }

        // Internal classes to map the external API response
        public class ApiFarmerResponse
        {
            [JsonPropertyName("data")]
            public List<CommodityData> Data { get; set; }
        }

        public class CommodityData
        {
            [JsonPropertyName("commodity")]
            public string Commodity { get; set; }

            [JsonPropertyName("price")]
            public double Price { get; set; }

            [JsonPropertyName("daily_change")]
            public double? DailyChange { get; set; }

            [JsonPropertyName("unit")]
            public string Unit { get; set; }

            [JsonPropertyName("currency")]
            public string Currency { get; set; }
        }
    }
}
