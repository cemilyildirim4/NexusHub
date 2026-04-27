using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;
using Nexus.Data;
using Nexus.Core.Entities;
using Nexus.Business.Abstract;

namespace Nexus.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController(IDeviceService deviceService ) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> Get() => Ok(await deviceService.GetAllDevicesAsync());

        [HttpPost]
        public async Task<IActionResult> Post(Device device)
        {
            await deviceService.AddDeviceAsync(device);
            var healthStatus = deviceService.CheckDeviceHealth(device);
            return Ok(new { Device = device, Status = healthStatus });
        }
    }
}
