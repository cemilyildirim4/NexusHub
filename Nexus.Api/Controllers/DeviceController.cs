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
        /// <summary>
        /// Sistemde kayıtlı olan tüm cihazları sağlık durumlarıyla birlikte listeler.
        /// </summary>
        /// <returns>Cihaz listesini ve HTTP 200 kodunu döner.</returns>
        [HttpGet]   
        public async Task<ActionResult> Get() => Ok(await deviceService.GetAllDevicesAsync());

        [HttpPost]
        public async Task<IActionResult> Post(Device device)
        {
            await deviceService.AddDeviceAsync(device);
            var healthStatus = deviceService.CheckDeviceHealth(device);
            return Ok(new { Device = device, Status = healthStatus });
        }
        // Belirli bir ID'ye göre cihaz getir: GET /api/devices/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var device = await deviceService.GetDeviceByIdAsync(id);
            if (device == null) return NotFound();
            return Ok(device);
        }

        //Cihaz güncelle: PUT /api/devices/5
        [HttpPut]
        public async Task<IActionResult> Put(Device device)
        {
            var updated = await deviceService.UpdateDeviceAsync(device);
            if (!updated) return NotFound();
            return Ok(new { message = "Device updated successfully." });
        }

        //Cihaz sil:DELETE /api/devices/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await deviceService.DeleteDeviceAsync(id);
            if (!deleted) return NotFound();
            return Ok(new { message = "Device deleted successfully." });
        }
    }
}
