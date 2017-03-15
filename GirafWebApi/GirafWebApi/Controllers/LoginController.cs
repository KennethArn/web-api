using System;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;

namespace GirafWebApi.Controllers
{
    [Route("[controller]")]
    public class LoginController : Controller
    {
        public LoginController()
        {
        }
        public class Credentials {
            public string Username {get; set;}
            public string Password {get; set;}
        }

        [HttpPost]
        public async Task <IActionResult> Post([FromBody] Credentials credentials)
        {
            string u = credentials.Username;
            string p = credentials.Password;
            
            // discover endpoints from metadata
            var disco = await DiscoveryClient.GetAsync("http://localhost:5001");

            // request token
            var tokenClient = new TokenClient(disco.TokenEndpoint, "mvc", "secret");
            var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync(u, p, "api1");
            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return Unauthorized();
            }
            return Ok(tokenResponse.Json);
        }
    }
}