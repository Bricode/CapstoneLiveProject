using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CapstoneProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        public string temp = "";
        private static HttpClient client = new HttpClient();
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        public static async void TestFetch()
        {            
            try
            {
                using (HttpResponseMessage res = await client.GetAsync("https://animechan.vercel.app/api/random"))
                    {
                        using (HttpContent content = res.Content)
                        {
                            var data = await content.ReadAsStringAsync();
                            if(data != null)
                            {
                                Console.WriteLine(data);
                                
                            }
                            else
                            {
                                Console.WriteLine("Error");
                            
                            }
                        }
                    }
                
            } catch(Exception exception)
            {
                Console.WriteLine(exception);
                
            }
        }
        [HttpGet][HttpPost]
        public IEnumerable<WeatherForecast> Get()
        {
            Console.WriteLine();
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {                
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
