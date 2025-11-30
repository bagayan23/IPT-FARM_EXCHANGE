using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace FarmExchange.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarketApiController : ControllerBase
    {
        [HttpGet("demand")]
        public IActionResult GetInDemandCrops()
        {
            var crops = new List<object>
            {
                new { Name = "Rice (Palay)", Demand = "High", PriceTrend = "Stable", ImageUrl = "https://images.unsplash.com/photo-1536617621255-b46c653696f8?auto=format&fit=crop&w=300&q=80" },
                new { Name = "Corn (Mais)", Demand = "Medium", PriceTrend = "Up", ImageUrl = "https://images.unsplash.com/photo-1551754655-cd27e38d2076?auto=format&fit=crop&w=300&q=80" },
                new { Name = "Coconut (Niyog)", Demand = "High", PriceTrend = "Down", ImageUrl = "https://images.unsplash.com/photo-1550549405-24d142142e05?auto=format&fit=crop&w=300&q=80" },
                new { Name = "Mango (Carabao)", Demand = "High", PriceTrend = "Up", ImageUrl = "https://images.unsplash.com/photo-1553279768-865429fa0078?auto=format&fit=crop&w=300&q=80" },
                new { Name = "Banana (Lakatan)", Demand = "Very High", PriceTrend = "Stable", ImageUrl = "https://images.unsplash.com/photo-1528825871115-3581a5387919?auto=format&fit=crop&w=300&q=80" },
                new { Name = "Onion (Red)", Demand = "Extreme", PriceTrend = "Volatile", ImageUrl = "https://images.unsplash.com/photo-1618512496248-a07fe83aa8cb?auto=format&fit=crop&w=300&q=80" }
            };

            return Ok(crops);
        }
    }
}
