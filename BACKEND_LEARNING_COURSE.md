# Backend Development Learning Course
## Based on NexusHub Project

A comprehensive guide to learn backend development using technologies and patterns from your real-world project.

---

## 📚 Course Overview

This course covers backend fundamentals through advanced patterns using:
- **ASP.NET Core 8** (.NET 8)
- **Entity Framework Core** (ORM)
- **PostgreSQL** (Database)
- **Layered Architecture**
- **RESTful API Design**

**Duration**: 8-10 weeks (self-paced)
**Prerequisites**: C# basics, HTTP concepts

---

## 🎯 Learning Path

```
Foundation (Week 1-2)
    ↓
Database & ORM (Week 3-4)
    ↓
API Development (Week 5-6)
    ↓
Architecture & Patterns (Week 7-8)
    ↓
Production Practices (Week 9-10)
```

---

# **MODULE 1: FOUNDATION (Weeks 1-2)**

## **Lesson 1.1: ASP.NET Core Basics**

### What You'll Learn
- What is ASP.NET Core?
- Project structure and templates
- Request-Response lifecycle
- Middleware pipeline

### Key Concepts from Your Project
Your `Program.cs` shows the core pipeline:

```csharp
var app = builder.Build();

// Middleware pipeline (executed in order)
app.UseExceptionHandler();     // Error handling
app.UseCors("AllowAll");       // Cross-origin requests
app.UseHttpsRedirection();     // Security
app.UseAuthorization();        // Authentication/Authorization
app.MapControllers();          // Route handlers

app.Run();
```

**Middleware** = Software that processes HTTP requests/responses

### Hands-On Exercise
1. Create a simple Hello World endpoint
2. Add a custom middleware
3. Observe the execution order

---

## **Lesson 1.2: Dependency Injection (DI)**

### Why It Matters
Instead of `new DeviceManager()`, ASP.NET Core manages object creation.

### Example from Your Code
```csharp
// Register service in Program.cs
builder.Services.AddScoped<IDeviceService, DeviceManager>();

// Use in controller (injected automatically)
public class DeviceController(IDeviceService deviceService)
{
    // deviceService is ready to use
    public async Task<ActionResult> GetDevices()
    {
        var devices = await deviceService.GetAllDevicesAsync();
    }
}
```

### Key Points
- **Scoped**: New instance per request
- **Singleton**: Same instance for app lifetime
- **Transient**: New instance every time

### Exercise
Why does your `DeviceManager` accept `NexusDbContext` in constructor? (DI principle!)

---

## **Lesson 1.3: HTTP Methods & REST Basics**

### REST Principles
Your `DeviceController` follows REST:

| HTTP Method | Purpose | Example |
|---|---|---|
| **GET** | Retrieve data | `GET /api/devices` |
| **POST** | Create data | `POST /api/devices` (with body) |
| **PUT** | Update data | `PUT /api/devices` (with body) |
| **DELETE** | Remove data | `DELETE /api/devices/5` |

### Your Implementation
```csharp
[ApiController]
[Route("api/[controller]")]
public class DeviceController(IDeviceService deviceService)
{
    [HttpGet]
    public async Task<ActionResult<List<Device>>> Get()
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Device>> Get(int id)
    
    [HttpPost]
    public async Task<ActionResult<Device>> Post([FromBody] Device device)
    
    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Device device)
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
}
```

### Exercise
Design endpoints for a `User` resource (GET all, GET by ID, POST, PUT, DELETE)

---

## **Lesson 1.4: Async/Await Pattern**

### Why Async?
Non-blocking operations = better performance

### Your Code
```csharp
// Async method - returns Task
public async Task<List<Device>> GetAllDevicesAsync()
{
    // await = "wait here, but release thread"
    return await context.Devices.ToListAsync();
}

// In controller
public async Task<ActionResult<List<Device>>> Get()
{
    // This runs asynchronously
    var devices = await deviceService.GetAllDevicesAsync();
    return Ok(devices);
}
```

### Key Rule
- **Always use async** for I/O (database, network, file)
- Never use `.Result` or `.Wait()` (causes deadlocks!)

### Exercise
Convert this sync code to async:
```csharp
public List<Device> GetAllDevices()
{
    return context.Devices.ToList();
}
```

---

# **MODULE 2: DATABASE & ORM (Weeks 3-4)**

## **Lesson 2.1: Entity Framework Core Basics**

### What is EF Core?
ORM (Object-Relational Mapper) = write C# instead of SQL

### Your Setup
```csharp
// Program.cs - Register DbContext
builder.Services.AddDbContext<NexusDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
```

### Entity Model
```csharp
// Nexus.Core/Entities/Device.cs
public class Device
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string IPAddress { get; set; }
    public bool IsOnline { get; set; }
    public DateTime LastSeen { get; set; }
    
    [NotMapped]  // Not stored in database
    public DeviceStatus Status { get; set; }
}
```

### DbContext
```csharp
public class NexusDbContext : DbContext
{
    public DbSet<Device> Devices { get; set; }
    
    public NexusDbContext(DbContextOptions<NexusDbContext> options)
        : base(options) { }
}
```

---

## **Lesson 2.2: CRUD Operations**

### Create (C)
```csharp
public async Task AddDeviceAsync(Device device)
{
    if (string.IsNullOrEmpty(device.IPAddress))
        throw new ArgumentException("IPAddress cannot be null or empty.");
    
    await context.Devices.AddAsync(device);
    await context.SaveChangesAsync();  // Commit to database
}
```

### Read (R)
```csharp
public async Task<List<Device>> GetAllDevicesAsync()
{
    return await context.Devices.ToListAsync();
}

public async Task<Device?> GetDeviceByIdAsync(int id)
{
    return await context.Devices.FindAsync(id);
}
```

### Update (U)
```csharp
public async Task<bool> UpdateDeviceAsync(Device device)
{
    var existing = await context.Devices.FindAsync(device.Id);
    if (existing == null) return false;
    
    existing.Name = device.Name;
    existing.IsOnline = device.IsOnline;
    // ... update other fields
    
    await context.SaveChangesAsync();
    return true;
}
```

### Delete (D)
```csharp
public async Task<bool> DeleteDeviceAsync(int id)
{
    var device = await context.Devices.FindAsync(id);
    if (device == null) return false;
    
    context.Devices.Remove(device);
    await context.SaveChangesAsync();
    return true;
}
```

### Exercise
Implement CRUD for a new `User` entity

---

## **Lesson 2.3: Migrations**

### What are Migrations?
Version control for your database schema

### Commands
```powershell
# Create migration
dotnet ef migrations add InitialCreate

# Apply migration
dotnet ef database update

# Rollback migration
dotnet ef migrations remove
```

### When to Use
- Add new table
- Add/modify columns
- Change constraints

### Exercise
Create a migration adding a `Description` field to `Device`

---

## **Lesson 2.4: Data Relationships**

### One-to-Many Example
```csharp
// Device has many Logs
public class Device
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<DeviceLog> Logs { get; set; }
}

public class DeviceLog
{
    public int Id { get; set; }
    public int DeviceId { get; set; }  // Foreign key
    public Device Device { get; set; }  // Navigation property
    public string Message { get; set; }
}
```

### Query with Include
```csharp
var deviceWithLogs = await context.Devices
    .Include(d => d.Logs)  // Eager loading
    .FirstOrDefaultAsync(d => d.Id == id);
```

### Exercise
Add `DeviceLog` entity and create CRUD operations

---

# **MODULE 3: API DEVELOPMENT (Weeks 5-6)**

## **Lesson 3.1: Controllers & Routing**

### Controller Basics
```csharp
[ApiController]
[Route("api/[controller]")]  // Becomes /api/devices
public class DeviceController : ControllerBase
{
    // [HttpGet] routes to: GET /api/devices
    [HttpGet]
    public async Task<ActionResult<List<Device>>> Get()
    
    // [HttpGet("{id}")] routes to: GET /api/devices/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Device>> Get(int id)
}
```

### Custom Routes
```csharp
[HttpGet("healthy")]  // GET /api/devices/healthy
public async Task<ActionResult<List<Device>>> GetHealthyDevices()

[HttpPost("bulk")]    // POST /api/devices/bulk
public async Task<IActionResult> AddBulkDevices([FromBody] List<Device> devices)
```

### Exercise
Add route: `GET /api/devices/online` that returns only online devices

---

## **Lesson 3.2: Input Validation**

### Your Implementation
```csharp
public async Task AddDeviceAsync(Device device)
{
    if (string.IsNullOrEmpty(device.IPAddress))
        throw new ArgumentException("IPAddress cannot be null or empty.");
    
    await context.Devices.AddAsync(device);
    await context.SaveChangesAsync();
}
```

### Data Annotations
```csharp
public class Device
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; }
    
    [Required]
    [RegularExpression(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")]
    public string IPAddress { get; set; }
}
```

### Validate in Controller
```csharp
[HttpPost]
public async Task<ActionResult<Device>> Post([FromBody] Device device)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
    
    await deviceService.AddDeviceAsync(device);
    return Ok(device);
}
```

### Exercise
Add validation: IP must be valid format, Name length 3-50

---

## **Lesson 3.3: HTTP Status Codes**

### Common Status Codes
| Code | Meaning | Example |
|---|---|---|
| **200** | Success | Device retrieved |
| **201** | Created | New device added |
| **204** | No Content | Deleted successfully |
| **400** | Bad Request | Invalid input |
| **404** | Not Found | Device doesn't exist |
| **500** | Server Error | Database connection failed |

### Your Code
```csharp
[HttpPost]
public async Task<ActionResult<Device>> Post([FromBody] Device device)
{
    await deviceService.AddDeviceAsync(device);
    return Ok(device);  // 200 OK
}

[HttpGet("{id}")]
public async Task<ActionResult<Device>> Get(int id)
{
    var device = await deviceService.GetDeviceByIdAsync(id);
    if (device == null)
        return NotFound();  // 404 Not Found
    return Ok(device);      // 200 OK
}

[HttpDelete("{id}")]
public async Task<IActionResult> Delete(int id)
{
    var success = await deviceService.DeleteDeviceAsync(id);
    if (!success)
        return NotFound();   // 404
    return Ok(new { message = "Device deleted" });  // 200
}
```

### Exercise
Implement proper status codes for all endpoints

---

## **Lesson 3.4: Exception Handling**

### Your Global Handler
```csharp
// Nexus.Api/Handlers/GlobalExceptionHandler.cs
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, 
        CancellationToken cancellationToken)
    {
        context.Response.ContentType = "application/json";
        
        if (exception is ArgumentException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
        
        var errorResponse = new ErrorResponse
        {
            Message = exception.Message,
            StatusCode = context.Response.StatusCode
        };
        
        await context.Response.WriteAsJsonAsync(errorResponse);
        return true;
    }
}
```

### Register in Program.cs
```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
app.UseExceptionHandler();
```

### Benefits
- Consistent error format
- Centralized handling
- Prevents stack traces in responses

### Exercise
Add custom exception `InvalidDeviceException` and handle it

---

## **Lesson 3.5: CORS Configuration**

### Why CORS?
Browsers block cross-origin requests by default (security feature)

### Your Setup
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()   // Any domain
                   .AllowAnyMethod()   // GET, POST, etc
                   .AllowAnyHeader();  // Any headers
        });
});

// Apply middleware (MUST be before MapControllers)
app.UseCors("AllowAll");
```

### Production-Ready CORS
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production",
        builder =>
        {
            builder.WithOrigins("https://yourdomain.com")
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();  // Allow cookies
        });
});
```

### Exercise
Create restrictive CORS policy for specific domains

---

# **MODULE 4: ARCHITECTURE & PATTERNS (Weeks 7-8)**

## **Lesson 4.1: Layered Architecture**

### Your Project Structure
```
Nexus.Api/              (Presentation Layer)
├── Controllers/        → Handle HTTP requests
├── Program.cs          → Configuration
└── Handlers/           → Exception handling

Nexus.Business/         (Business Logic Layer)
├── Abstract/           → Interfaces (IDeviceService)
└── Concrete/           → Implementations (DeviceManager)

Nexus.Data/             (Data Access Layer)
└── NexusDbContext.cs   → Database context

Nexus.Core/             (Domain Layer)
└── Entities/           → Device.cs, DeviceStatus.cs
```

### Benefits
✅ Separation of Concerns  
✅ Testability  
✅ Reusability  
✅ Maintainability  

### Data Flow
```
Request
   ↓
Controller (validates input)
   ↓
Service/Manager (business logic)
   ↓
DbContext (database operations)
   ↓
Database
```

### Exercise
Add new `LogService` to business layer following the pattern

---

## **Lesson 4.2: Service Pattern (Repository Alternative)**

### Interface
```csharp
// Nexus.Business/Abstract/IDeviceService.cs
public interface IDeviceService
{
    Task<List<Device>> GetAllDevicesAsync();
    Task AddDeviceAsync(Device device);
    Task<Device?> GetDeviceByIdAsync(int id);
    Task<bool> UpdateDeviceAsync(Device device);
    Task<bool> DeleteDeviceAsync(int id);
    DeviceStatus CheckDeviceHealth(Device device);
}
```

### Implementation
```csharp
public class DeviceManager(NexusDbContext context) : IDeviceService
{
    // All CRUD operations here
}
```

### Usage in Controller
```csharp
public class DeviceController(IDeviceService deviceService)
{
    [HttpGet]
    public async Task<ActionResult<List<Device>>> Get()
    {
        return Ok(await deviceService.GetAllDevicesAsync());
    }
}
```

### Why Interfaces?
- Loose coupling
- Easy to test (mock implementation)
- Can swap implementations

### Exercise
Create `ILogService` interface and `LogManager` implementation

---

## **Lesson 4.3: Business Logic Example**

### Your Health Check Logic
```csharp
public DeviceStatus CheckDeviceHealth(Device device)
{
    var timeDifference = DateTime.UtcNow - device.LastSeen;
    
    if (!device.IsOnline) 
        return DeviceStatus.Critical;  // Not online
    
    if (timeDifference.TotalMinutes > 30) 
        return DeviceStatus.Warning;   // Hasn't checked in 30min
    
    return DeviceStatus.Healthy;       // All good
}
```

### Enum
```csharp
public enum DeviceStatus
{
    Healthy,
    Warning,
    Critical
}
```

### Used in Controller
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<Device>> Get(int id)
{
    var device = await deviceService.GetDeviceByIdAsync(id);
    if (device != null)
    {
        device.Status = deviceService.CheckDeviceHealth(device);
    }
    return Ok(device);
}
```

### Exercise
Add business logic: `CalculateDeviceUptime()` method

---

## **Lesson 4.4: Error Handling Best Practices**

### Your Validation
```csharp
// ✅ Throw exception for invalid input
public async Task AddDeviceAsync(Device device)
{
    if (string.IsNullOrEmpty(device.IPAddress))
        throw new ArgumentException("IPAddress cannot be null or empty.");
    
    await context.Devices.AddAsync(device);
    await context.SaveChangesAsync();
}

// ✅ Return bool for expected failure
public async Task<bool> UpdateDeviceAsync(Device device)
{
    var existing = await context.Devices.FindAsync(device.Id);
    if (existing == null)
        return false;  // Not found is expected
    
    // Update...
    return true;
}
```

### Controller Response
```csharp
[HttpPut]
public async Task<IActionResult> Put([FromBody] Device device)
{
    var success = await deviceService.UpdateDeviceAsync(device);
    if (!success)
        return NotFound();  // 404
    
    return Ok(new { message = "Device updated" });  // 200
}
```

### Principle
- **Exceptions** = Unexpected errors (network, database down)
- **Return values** = Expected outcomes (not found)

---

# **MODULE 5: PRODUCTION PRACTICES (Weeks 9-10)**

## **Lesson 5.1: Configuration Management**

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=NexusDb;Username=postgres;Password=1234"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Program.cs Usage
```csharp
builder.Services.AddDbContext<NexusDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
```

### Environment-Specific Settings
```
appsettings.json          (Development default)
appsettings.Production.json (Production overrides)
appsettings.Staging.json  (Staging overrides)
```

### Exercise
Create `appsettings.Production.json` with secure database settings

---

## **Lesson 5.2: Logging**

### Built-in Logging
```csharp
public class DeviceManager(
    NexusDbContext context,
    ILogger<DeviceManager> logger) : IDeviceService
{
    public async Task AddDeviceAsync(Device device)
    {
        logger.LogInformation("Adding device: {DeviceName}", device.Name);
        
        try
        {
            if (string.IsNullOrEmpty(device.IPAddress))
                throw new ArgumentException("IPAddress cannot be null.");
            
            await context.Devices.AddAsync(device);
            await context.SaveChangesAsync();
            
            logger.LogInformation("Device added successfully: {DeviceId}", device.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding device");
            throw;
        }
    }
}
```

### Log Levels
- **Critical**: Application failure
- **Error**: Error occurred but app continues
- **Warning**: Potentially harmful
- **Information**: Important events
- **Debug**: Diagnostic info

### Exercise
Add logging throughout DeviceManager

---

## **Lesson 5.3: Unit Testing**

### Test Structure
```csharp
[TestClass]
public class DeviceServiceTests
{
    private NexusDbContext _context;
    private DeviceManager _service;
    
    [TestInitialize]
    public void Setup()
    {
        // Create in-memory database
        var options = new DbContextOptionsBuilder<NexusDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;
        
        _context = new NexusDbContext(options);
        _service = new DeviceManager(_context);
    }
    
    [TestMethod]
    public async Task AddDevice_ValidDevice_Success()
    {
        // Arrange
        var device = new Device 
        { 
            Name = "Router", 
            IPAddress = "192.168.1.1" 
        };
        
        // Act
        await _service.AddDeviceAsync(device);
        
        // Assert
        Assert.AreEqual(1, await _context.Devices.CountAsync());
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task AddDevice_NullIP_Throws()
    {
        var device = new Device { Name = "Router", IPAddress = null };
        await _service.AddDeviceAsync(device);
    }
}
```

### Exercise
Write unit tests for all DeviceManager methods

---

## **Lesson 5.4: Security Best Practices**

### HTTPS
```csharp
// Program.cs - enabled by default
app.UseHttpsRedirection();
```

### Authentication (Future Module)
```csharp
// Future: Add JWT tokens
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* config */ });
```

### Secrets Management
```csharp
// Never hardcode passwords!
// Use User Secrets in development
var password = builder.Configuration["Secrets:DbPassword"];
```

### CORS Security
```csharp
// NOT for production!
builder.AllowAnyOrigin()

// Production - specific domain
builder.WithOrigins("https://yourdomain.com")
```

### Exercise
Implement proper CORS for specific domain

---

## **Lesson 5.5: API Documentation with Swagger**

### Your Setup
```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

### Document Endpoints
```csharp
[HttpGet("{id}")]
/// <summary>
/// Get device by ID
/// </summary>
/// <param name="id">Device ID</param>
/// <returns>Device details including health status</returns>
/// <response code="200">Device found</response>
/// <response code="404">Device not found</response>
public async Task<ActionResult<Device>> Get(int id)
{
    // ...
}
```

### Access
- Development: `https://localhost:7001/swagger`
- Shows all endpoints
- Try endpoints directly

### Exercise
Add XML documentation comments to all endpoints

---

# **MODULE 6: ADVANCED TOPICS**

## **Lesson 6.1: Pagination**

### Implementation
```csharp
public class PaginatedResult<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public async Task<PaginatedResult<Device>> GetDevicesPaginatedAsync(int pageNumber, int pageSize)
{
    var query = context.Devices;
    var totalCount = await query.CountAsync();
    
    var items = await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return new PaginatedResult<Device>
    {
        Items = items,
        TotalCount = totalCount,
        PageNumber = pageNumber,
        PageSize = pageSize
    };
}
```

### Controller
```csharp
[HttpGet("paginated")]
public async Task<ActionResult<PaginatedResult<Device>>> GetPaginated(int pageNumber = 1, int pageSize = 10)
{
    return Ok(await deviceService.GetDevicesPaginatedAsync(pageNumber, pageSize));
}
```

---

## **Lesson 6.2: Filtering & Sorting**

### Filter Online Devices
```csharp
public async Task<List<Device>> GetOnlineDevicesAsync()
{
    return await context.Devices
        .Where(d => d.IsOnline)
        .ToListAsync();
}
```

### Sort by LastSeen
```csharp
public async Task<List<Device>> GetDevicesSortedAsync(string sortBy = "name")
{
    return sortBy.ToLower() switch
    {
        "lastseen" => await context.Devices
            .OrderByDescending(d => d.LastSeen)
            .ToListAsync(),
        "name" => await context.Devices
            .OrderBy(d => d.Name)
            .ToListAsync(),
        _ => await context.Devices.ToListAsync()
    };
}
```

---

## **Lesson 6.3: Database Transactions**

### Atomic Operations
```csharp
public async Task TransferDeviceAsync(int deviceId, int fromUserId, int toUserId)
{
    using var transaction = await context.Database.BeginTransactionAsync();
    try
    {
        var device = await context.Devices.FindAsync(deviceId);
        device.OwnerId = toUserId;
        
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

---

# **PROJECT EXERCISES**

## **Exercise 1: Add User Management**
Create a `User` entity and service:
- Users can own devices
- One-to-Many relationship
- Full CRUD operations

## **Exercise 2: Add Device Logs**
Create a `DeviceLog` entity:
- Track every device state change
- Store timestamp, old status, new status
- Query logs for a specific device

## **Exercise 3: Add Device Groups**
Create ability to group devices:
- Users can organize devices into groups
- Query all devices in a group
- Get health status of group

## **Exercise 4: Add Authentication**
(Future module topic):
- Users must login
- Only see their own devices
- Roles: Admin, User

## **Exercise 5: Performance Optimization**
- Add caching for frequently queried devices
- Implement database indexing on IPAddress
- Use AsNoTracking() for read-only queries

---

# **DEPLOYMENT GUIDE**

## **Step 1: Prepare Database**
```powershell
dotnet ef database update
```

## **Step 2: Build Release**
```powershell
dotnet build -c Release
```

## **Step 3: Publish**
```powershell
dotnet publish -c Release -o ./publish
```

## **Step 4: Deploy to Azure/Docker/IIS**
- Azure App Service
- Docker container
- Windows IIS

---

# **DEBUGGING TIPS**

1. **Database Connection Issues**
   - Check connection string in appsettings.json
   - Verify PostgreSQL is running
   - Check firewall rules

2. **CORS Errors**
   - Verify `app.UseCors()` is before `app.MapControllers()`
   - Check frontend URL matches CORS policy
   - Use browser DevTools Network tab

3. **404 Errors**
   - Verify route name matches controller/action
   - Check HTTP method (GET vs POST)
   - Confirm parameter names match

4. **500 Errors**
   - Check Output window for exception details
   - Verify database is accessible
   - Look at GlobalExceptionHandler logs

---

# **LEARNING RESOURCES**

### Official Documentation
- [Microsoft Docs - ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core)
- [PostgreSQL Documentation](https://www.postgresql.org/docs)

### Videos
- Traversy Media: ASP.NET Core REST API
- Microsoft: ASP.NET Core for Beginners
- Kudvenkat: C# Complete Tutorial

### Practice
- Build CRUD API from scratch
- Add authentication to your project
- Deploy to cloud (Azure)

---

# **SUMMARY**

You now understand:
✅ ASP.NET Core fundamentals  
✅ Database design with Entity Framework Core  
✅ REST API design principles  
✅ Layered architecture patterns  
✅ Exception handling and validation  
✅ CORS and security basics  
✅ Testing and logging  
✅ Deployment practices  

**Next Steps:**
1. Complete all exercises
2. Deploy your project
3. Add authentication
4. Explore advanced topics (caching, background jobs, microservices)

---

**Created**: Based on NexusHub project  
**Level**: Beginner → Intermediate  
**Time**: 8-10 weeks self-paced
