using KeyVault.DataLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeyVault.Controllers
{
    [Authorize]
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
                BackendServices = true
            };
        }
    }
}
