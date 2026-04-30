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
            if (string.IsNullOrEmpty(device.IPAddress))
            {
                throw new ArgumentException("IPAddress cannot be null or empty.");
            }
            await context.Devices.AddAsync(device);
            await context.SaveChangesAsync();
        }
        public DeviceStatus CheckDeviceHealth(Device device)    
        {
            var timeDifference = DateTime.UtcNow - device.LastSeen;
            
            if (!device.IsOnline) return DeviceStatus.Critical; 
            if (timeDifference.TotalMinutes > 30) return DeviceStatus.Warning;

            return DeviceStatus.Healthy;
        }

        public async Task<Device?> GetDeviceByIdAsync(int id)
        {
            var device = await context.Devices.FindAsync(id);
            if(device != null)
            {
                device.Status = CheckDeviceHealth(device);
            }
            return device;  
        }

        public async Task<bool> UpdateDeviceAsync(Device device)
        {
            var existingDevice = await context.Devices.FindAsync(device.Id);
            if (existingDevice == null)
            {
                return false;
            }

            existingDevice.Name = device.Name;
            existingDevice.IPAddress = device.IPAddress;
            existingDevice.IsOnline = device.IsOnline;
            existingDevice.LastSeen = device.LastSeen;

            await context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> DeleteDeviceAsync(int id)
        {
            var device = await context.Devices.FindAsync(id);
            if (device == null)
            {
                return false;
            }

            context.Devices.Remove(device);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
