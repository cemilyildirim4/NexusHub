using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nexus.Core.Entities;

namespace Nexus.Business.Abstract
{
    public interface IDeviceService 
    {
        Task<List<Device>> GetAllDevicesAsync();
        Task AddDeviceAsync(Device device);
        DeviceStatus CheckDeviceHealth(Device device);
        Task<Device?> GetDeviceByIdAsync(int id);
        Task<bool> UpdateDeviceAsync(Device device);
        Task<bool> DeleteDeviceAsync(int id);
    }
}