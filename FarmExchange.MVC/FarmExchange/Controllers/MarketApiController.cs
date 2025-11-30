using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace FarmExchange.Controllers
{
    [Route("api/market")]
    [ApiController]
    public class MarketApiController : Controller
    {
        [HttpGet("demand")]
        public IActionResult GetMarketDemand()
        {
            // Simulated data for Philippine crops
            var marketData = new List<object>
            {
                new {
                    Name = "Rice (Palay)",
                    DemandLevel = "High",
                    Trend = "Rising",
                    AveragePrice = 19.50,
                    Unit = "kg",
                    Description = "Staple food, consistently high demand nationwide."
                },
                new {
                    Name = "Yellow Corn",
                    DemandLevel = "High",
                    Trend = "Stable",
                    AveragePrice = 15.25,
                    Unit = "kg",
                    Description = "High demand for livestock feed production."
                },
                new {
                    Name = "Coconut (Mature)",
                    DemandLevel = "Medium",
                    Trend = "Stable",
                    AveragePrice = 12.00,
                    Unit = "pc",
                    Description = "Steady export and industrial processing demand."
                },
                new {
                    Name = "Banana (Cavendish)",
                    DemandLevel = "High",
                    Trend = "Rising",
                    AveragePrice = 35.00,
                    Unit = "kg",
                    Description = "Top export crop with strong local consumption."
                },
                new {
                    Name = "Mango (Carabao)",
                    DemandLevel = "Medium",
                    Trend = "Rising",
                    AveragePrice = 85.00,
                    Unit = "kg",
                    Description = "Seasonal peak approaching, prices increasing."
                },
                new {
                    Name = "Onion (Red)",
                    DemandLevel = "High",
                    Trend = "Volatile",
                    AveragePrice = 120.00,
                    Unit = "kg",
                    Description = "Currently facing supply shortage in key regions."
                }
            };

            return Json(marketData);
        }
    }
}
