using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.SqlServer;
using System.Configuration;
using System.Text;
using System.Web;
using System.IO;
using System.Data.SqlClient;
using Microsoft.Extensions.Options;
using Xero.NetStandard.OAuth2.Models;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Token;
using Xero.NetStandard.OAuth2.Api;

namespace CapstoneProject.Controllers{


    [ApiController]
    [Route("[controller]")]
    public class TestController : Controller
    {
        
        private readonly IOptions<XeroConfiguration> XeroConfig;
        public string temp = "";
        private static HttpClient client = new HttpClient();
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public TestController( IOptions<XeroConfiguration> XeroConfig)
        {            
            this.XeroConfig = XeroConfig;
        }

        public static async void TestFetch()
        {
            var xeroToken = TokenUtilities.GetStoredToken();            
            string accessToken = xeroToken.AccessToken;
            Guid tenantId = TokenUtilities.GetCurrentTenantId();
            string xeroTenantId;
            if (xeroToken.Tenants.Any((t) => t.TenantId == tenantId))
            {
                xeroTenantId = tenantId.ToString();
            }
            else
            {
                var id = xeroToken.Tenants.First().TenantId;
                xeroTenantId = id.ToString();
                TokenUtilities.StoreTenantId(id);
            }
            var AccountingApi = new AccountingApi();
            var response = await AccountingApi.GetInvoicesAsync(accessToken, xeroTenantId);
            Console.WriteLine(response);
        }
        [HttpGet]
        [HttpPost]
        public string Get()
        {
            var rng = new Random();
            
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

                using (StreamReader sr = new StreamReader("userSecrets.txt"))
                {
                    builder.UserID = sr.ReadLine();
                    builder.Password = sr.ReadLine();
                    builder.InitialCatalog = sr.ReadLine();
                    builder.DataSource = sr.ReadLine();
                    sr.Close();
                }

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    Console.WriteLine("\nQuery data example:");
                    Console.WriteLine("=========================================\n");

                    connection.Open();
                    string sqlWriteTest = "select testEntry, testEntry from test;";
                    string sql = "SELECT * FROM xero_data";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                Console.WriteLine(reader.GetString(1));

                                return reader.GetString(1);
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());

                return e.ToString();
            }

            return "failed";


        }
    }
}
