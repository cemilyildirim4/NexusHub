using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Nexus.Core.Entities;

namespace Nexus.Data
{
    public class    NexusDbContext : DbContext
    {
        public NexusDbContext(DbContextOptions<NexusDbContext> options) : base(options)
        {

        }
        public DbSet<Device> Devices { get; set; }
    }
}
