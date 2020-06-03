using Microsoft.AspNetCore.Mvc;

namespace KeyVault.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        public object IsHealthy()
        {
            return new
            {
                DataSource = true,
                BackendServices = true
            };
        }
    }
}
