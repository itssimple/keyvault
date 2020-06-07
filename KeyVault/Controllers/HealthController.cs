using KeyVault.DataLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeyVault.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly StorageLayer _storageLayer;

        public HealthController(StorageLayer storageLayer)
        {
            _storageLayer = storageLayer;
        }
        public object IsHealthy()
        {
            return new
            {
                BackendServices = true,
                Certificate = Request.Headers["X-SSL-CERT"].ToString()
            };
        }

        [Authorize]
        [Route("whoami")]
        public object WhoAmI()
        {
            return User.Identity;
        }
    }
}
