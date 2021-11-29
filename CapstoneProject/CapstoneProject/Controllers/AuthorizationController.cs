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
using Newtonsoft.Json;

namespace CapstoneProject.Controllers{
    [Route("auth/")]
    public class AuthorizationController : Controller
    {        
        private readonly IOptions<XeroConfiguration> XeroConfig;
        private readonly IHttpClientFactory clientFactory;
        private readonly ILogger<AuthorizationController> _logger;

        // Get /Authorization/
        public AuthorizationController(ILogger<AuthorizationController> logger, IHttpClientFactory httpClientFactory, IOptions<XeroConfiguration> XeroConfig)
        {
            _logger = logger;
            this.XeroConfig = XeroConfig;
            this.clientFactory = httpClientFactory;
        }

        
        [HttpGet("fetch/")]
        public IActionResult Index()
        {
            Console.WriteLine("test");
            var client = new XeroClient(XeroConfig.Value, clientFactory);
            var clientState = Guid.NewGuid().ToString();
            TokenUtilities.StoreState(clientState);
            Console.WriteLine("testting");
            return Redirect(client.BuildLoginUri());
        }

        [HttpGet("callback/")]
        public async Task<ActionResult> Callback(string code, string state)
        {
            var clientState = TokenUtilities.GetCurrentState();
            Console.WriteLine(state);
            Console.WriteLine(clientState);            

            var client = new XeroClient(XeroConfig.Value, clientFactory);
            var xeroToken = (XeroOAuth2Token)await client.RequestAccessTokenAsync(code);                     

            List<Tenant> tenants = await client.GetConnectionsAsync(xeroToken);

            Tenant firstTenant = tenants[0];

            TokenUtilities.StoreToken(xeroToken);
            
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
            var output = JsonConvert.SerializeObject(response._Invoices);
            Console.WriteLine( response._Invoices);
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
                connection.Open();
                string sqlWriteTest = "select * from test;";
                string createNewTable = "CREATE TABLE xero_data(id int PRIMARY KEY IDENTITY(1,1 ), incoming_data char(100));";                
                string writeData = "Insert xero_data (stored_data) values('"+output+"');";
                string getData = "select * from xero_data;";
                using (SqlCommand command = new SqlCommand(writeData, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
                return Redirect("https://localhost:5001/display");
        }
        
        [HttpGet("logout/")]
        public async Task<ActionResult> Disconnect()
        {
            var client = new XeroClient(XeroConfig.Value, clientFactory);

            var xeroToken = TokenUtilities.GetStoredToken();
            var utcTimeNow = DateTime.UtcNow;

            if (utcTimeNow > xeroToken.ExpiresAtUtc)
            {
                xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
                TokenUtilities.StoreToken(xeroToken);
            }

            string accessToken = xeroToken.AccessToken;
            Tenant xeroTenant = xeroToken.Tenants[0];

            await client.DeleteConnectionAsync(xeroToken, xeroTenant);

            TokenUtilities.DestroyToken();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet("session/")]
        public async Task<ActionResult> Session()
        {
            if (TokenUtilities.TokenExists())
            {
                var xeroToken = TokenUtilities.GetStoredToken();
                return Json(xeroToken);
            }
            else
            {
                return Json("Error");
            }
        }
    }
}

