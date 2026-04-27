using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nexus.Business.Abstract;
using Nexus.Core.Entities;
using Nexus.Data;
using Microsoft.EntityFrameworkCore;

namespace Nexus.Business.Concrete
{
    public class DeviceManager(NexusDbContext context) : IDeviceService
    {
        public async Task<List<Device>> GetAllDevicesAsync()
        {
            return await context.Devices.ToListAsync();
        }
        public async Task AddDeviceAsync(Device device)
        {
            if (!string.IsNullOrEmpty(device.IPAddress))
            {
                await context.Devices.AddAsync(device);
                await context.SaveChangesAsync();
            }
        }
        public DeviceStatus CheckDeviceHealth(Device device)
        {
            var timeDifference = DateTime.UtcNow - device.LastSeen;

            if (!device.IsOnline) return DeviceStatus.Critical; 
            if (timeDifference.TotalMinutes > 30) return DeviceStatus.Warning;

            return DeviceStatus.Healthy;
        }
    }
}
