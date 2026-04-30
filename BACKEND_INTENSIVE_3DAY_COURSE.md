# 🚀 Backend Development - Intensive 3-Day Course
## From Zero to Building APIs

**Duration**: 3 days (6-8 hours per day)  
**Level**: Beginner → Intermediate  
**Based on**: NexusHub Real Project  
**Goal**: Build production-ready REST APIs

---

# **DAY 1: FUNDAMENTALS & DATABASE (6-8 hours)**

## **Session 1.1: ASP.NET Core & Dependency Injection (90 min)**

### What is ASP.NET Core?
- Web framework for building APIs
- Built on .NET 8 (latest, fastest)
- Middleware pipeline architecture

### Your Project Structure
```
Nexus.Api/           ← Controllers (HTTP layer)
Nexus.Business/      ← Services (Business logic)
Nexus.Data/          ← DbContext (Database)
Nexus.Core/          ← Entities (Models)
```

### Dependency Injection Explained
```csharp
// Program.cs - Register service
builder.Services.AddScoped<IDeviceService, DeviceManager>();

// Controller - Automatically injected
public class DeviceController(IDeviceService deviceService)
{
    // Use it directly - no "new" needed!
}
```

**Why?** Loose coupling, easier testing, cleaner code

### 🎯 **EXERCISE 1** (20 min)
1. Open `Program.cs`
2. Find where `IDeviceService` is registered
3. Open `DeviceController` - see how it's injected via constructor
4. **Challenge**: Add a second service registration and use it in controller

---

## **Session 1.2: HTTP & REST Basics (60 min)**

### The 5 HTTP Methods You'll Use

| Method | Use | Example |
|--------|-----|---------|
| **GET** | Read data | `GET /api/devices` |
| **POST** | Create | `POST /api/devices` (body: new device) |
| **PUT** | Update | `PUT /api/devices` (body: updated device) |
| **DELETE** | Remove | `DELETE /api/devices/5` |
| **PATCH** | Partial update | `PATCH /api/devices/5` |

### Your DeviceController
```csharp
[ApiController]
[Route("api/[controller]")]
public class DeviceController(IDeviceService deviceService)
{
    [HttpGet]
    public async Task<ActionResult<List<Device>>> Get()
    {
        return Ok(await deviceService.GetAllDevicesAsync());
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Device>> Get(int id)
    {
        var device = await deviceService.GetDeviceByIdAsync(id);
        if (device == null) return NotFound();
        return Ok(device);
    }
    
    [HttpPost]
    public async Task<ActionResult<Device>> Post([FromBody] Device device)
    {
        await deviceService.AddDeviceAsync(device);
        return Ok(device);
    }
}
```

### HTTP Status Codes
- **200** = Success ✅
- **201** = Created ✅
- **400** = Bad request (client error) ❌
- **404** = Not found ❌
- **500** = Server error ❌

### 🎯 **EXERCISE 2** (15 min)
1. Open **Postman** or **Thunder Client** (VS Code extension)
2. Test: `GET http://localhost:5000/api/devices`
3. Test: `POST /api/devices` with JSON body: `{"name":"Router","ipAddress":"192.168.1.1"}`
4. Observe status codes

---

## **Session 1.3: Async/Await (45 min)**

### The Problem
```csharp
// ❌ BLOCKS thread - BAD
public List<Device> GetAllDevices()
{
    return context.Devices.ToList();  // Thread waits here
}
```

### The Solution
```csharp
// ✅ ASYNC - GOOD
public async Task<List<Device>> GetAllDevicesAsync()
{
    return await context.Devices.ToListAsync();  // Thread freed up
}

// Usage
var devices = await deviceService.GetAllDevicesAsync();
```

### Key Rules
1. **Always use async for I/O** (database, network, files)
2. **Never use `.Result` or `.Wait()`** → Causes deadlocks
3. **Always `await`** → Don't call async without await

### 🎯 **EXERCISE 3** (10 min)
In `DeviceManager.cs`, find all methods. Verify they're async. Look at the pattern:
- Method name ends with `Async`
- Returns `Task<T>` not `T`
- Uses `await` on database calls

**Result**: Your API can handle 1000s of concurrent requests!

---

## **Session 1.4: Entity Framework Core & CRUD (120 min)**

### What is EF Core?
ORM = Write C# instead of SQL
```csharp
// C# way (EF Core)
var devices = await context.Devices
    .Where(d => d.IsOnline)
    .ToListAsync();

// SQL way (no fun)
SELECT * FROM Devices WHERE IsOnline = true;
```

### Your Database Model
```csharp
public class Device
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string IPAddress { get; set; }
    public bool IsOnline { get; set; }
    public DateTime LastSeen { get; set; }
    
    [NotMapped]
    public DeviceStatus Status { get; set; }  // Not in database
}
```

### CRUD Operations

#### CREATE
```csharp
public async Task AddDeviceAsync(Device device)
{
    if (string.IsNullOrEmpty(device.IPAddress))
        throw new ArgumentException("IP required");
    
    await context.Devices.AddAsync(device);
    await context.SaveChangesAsync();  // ← Commit
}
```

#### READ
```csharp
// All
public async Task<List<Device>> GetAllDevicesAsync()
{
    return await context.Devices.ToListAsync();
}

// Single
public async Task<Device?> GetDeviceByIdAsync(int id)
{
    return await context.Devices.FindAsync(id);
}
```

#### UPDATE
```csharp
public async Task<bool> UpdateDeviceAsync(Device device)
{
    var existing = await context.Devices.FindAsync(device.Id);
    if (existing == null) return false;
    
    existing.Name = device.Name;
    existing.IsOnline = device.IsOnline;
    
    await context.SaveChangesAsync();
    return true;
}
```

#### DELETE
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

### 🎯 **EXERCISE 4** (30 min)
**Build a complete CRUD for a new entity: `Location`**

1. Create `Location.cs` in `Nexus.Core/Entities/`:
```csharp
public class Location
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string City { get; set; }
    public int DeviceCount { get; set; }
}
```

2. Add to `NexusDbContext.cs`:
```csharp
public DbSet<Location> Locations { get; set; }
```

3. Create `Nexus.Business/Abstract/ILocationService.cs`:
```csharp
public interface ILocationService
{
    Task<List<Location>> GetAllAsync();
    Task<Location?> GetByIdAsync(int id);
    Task AddAsync(Location location);
    Task<bool> UpdateAsync(Location location);
    Task<bool> DeleteAsync(int id);
}
```

4. Implement `Nexus.Business/Concrete/LocationManager.cs` (copy DeviceManager pattern)

5. Create `Nexus.Api/Controllers/LocationController.cs` (copy DeviceController pattern)

6. Register in `Program.cs`:
```csharp
builder.Services.AddScoped<ILocationService, LocationManager>();
```

7. **TEST** with Postman:
   - GET /api/locations
   - POST /api/locations (new location)
   - PUT /api/locations (update)
   - DELETE /api/locations/1

**Bonus**: Create migration and update database:
```powershell
dotnet ef migrations add AddLocations
dotnet ef database update
```

---

# **DAY 2: API DEVELOPMENT & ARCHITECTURE (6-8 hours)**

## **Session 2.1: Input Validation & Error Handling (90 min)**

### Your Current Validation
```csharp
public async Task AddDeviceAsync(Device device)
{
    if (string.IsNullOrEmpty(device.IPAddress))
        throw new ArgumentException("IPAddress cannot be null or empty.");
    // ...
}
```

### Better: Data Annotations
```csharp
public class Device
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; }
    
    [Required]
    [RegularExpression(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")]
    public string IPAddress { get; set; }
    
    [Range(0, 100)]
    public int SignalStrength { get; set; }
}
```

### Best: Global Exception Handler
```csharp
// Already in your project!
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
        
        await context.Response.WriteAsJsonAsync(new
        {
            message = exception.Message,
            statusCode = context.Response.StatusCode
        });
        return true;
    }
}
```

### Controller Response
```csharp
[HttpPost]
public async Task<ActionResult<Device>> Post([FromBody] Device device)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
    
    try
    {
        await deviceService.AddDeviceAsync(device);
        return Ok(device);  // 200
    }
    catch (ArgumentException)
    {
        return BadRequest();  // 400
    }
}
```

### 🎯 **EXERCISE 5** (20 min)
1. Add data annotations to `Device` model
2. Add validation to `DeviceController.Post()`
3. Test invalid data in Postman → See validation errors
4. Test valid data → Success

---

## **Session 2.2: CORS & Security (60 min)**

### The CORS Problem
Frontend on `http://localhost:3000` can't call API on `http://localhost:5000` (different origin)

### Your Solution
```csharp
// Program.cs
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

app.UseCors("AllowAll");  // MUST be before MapControllers!
```

### Production CORS (Restrictive)
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production",
        builder =>
        {
            builder.WithOrigins("https://yourdomain.com")  // Only your frontend
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();  // For cookies/auth
        });
});
```

### Important: Middleware Order
```csharp
app.UseExceptionHandler();     // 1st - Handle errors
app.UseCors("AllowAll");       // 2nd - CORS
app.UseHttpsRedirection();     // 3rd - Security
app.UseAuthorization();        // 4th - Auth
app.MapControllers();          // 5th - Routes
```

**Order matters!** CORS must come early.

### 🎯 **EXERCISE 6** (15 min)
1. Change CORS policy to restrict to `https://localhost:3000` only
2. Test from allowed origin → Works
3. Test from different origin → Blocked
4. Observe CORS errors in browser console

---

## **Session 2.3: Layered Architecture Pattern (90 min)**

### Your Architecture
```
HTTP Request
    ↓
[DeviceController]          ← Handles HTTP, validates input
    ↓ calls
[DeviceManager]             ← Business logic, calculations
    ↓ uses
[NexusDbContext]            ← Database operations
    ↓ writes to
[PostgreSQL Database]       ← Data storage
```

### Why Layered?
✅ **Separation of Concerns** - Each layer has one job  
✅ **Testability** - Easy to mock layers  
✅ **Reusability** - Service can be used by multiple controllers  
✅ **Maintainability** - Easy to find and fix bugs  

### Layer Responsibilities

#### **Presentation Layer (Controllers)**
- Accept HTTP requests
- Validate input
- Return HTTP responses
- DON'T: Query database directly, do complex calculations

```csharp
[HttpPost]
public async Task<ActionResult<Device>> Post([FromBody] Device device)
{
    // ✅ Validate input
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
    
    // ✅ Call service
    await deviceService.AddDeviceAsync(device);
    
    // ✅ Return response
    return Ok(device);
}
```

#### **Business Logic Layer (Services)**
- Implement business rules
- Perform calculations
- Coordinate multiple operations
- DON'T: Know about HTTP, controllers, or SQL

```csharp
public DeviceStatus CheckDeviceHealth(Device device)
{
    // ✅ Business logic
    var timeDifference = DateTime.UtcNow - device.LastSeen;
    
    if (!device.IsOnline) return DeviceStatus.Critical;
    if (timeDifference.TotalMinutes > 30) return DeviceStatus.Warning;
    return DeviceStatus.Healthy;
}
```

#### **Data Access Layer (DbContext)**
- Database queries
- CRUD operations
- DON'T: Business logic, calculations

```csharp
public async Task<List<Device>> GetAllDevicesAsync()
{
    return await context.Devices.ToListAsync();
}
```

#### **Domain Layer (Entities)**
- Data models
- Enums, constants
- DON'T: Database access, HTTP, business logic

```csharp
public class Device
{
    public int Id { get; set; }
    public string Name { get; set; }
    // ... just data
}
```

### 🎯 **EXERCISE 7** (30 min)
**Identify layers in your project:**

1. Find 3 things in `DeviceController` that belong in controller layer
2. Find 3 things in `DeviceManager` that belong in business layer
3. Find 3 things in `NexusDbContext` that belong in data layer
4. Verify separation - move any misplaced code

---

## **Session 2.4: Service Pattern (60 min)**

### Why Interfaces?
```csharp
// ❌ Tightly coupled
public class DeviceController
{
    private DeviceManager manager = new DeviceManager();  // Hard dependency
}

// ✅ Loosely coupled
public class DeviceController(IDeviceService service)  // Interface dependency
{
    // Easy to swap implementations, easy to test
}
```

### Your Pattern
```csharp
// Interface - Contract
public interface IDeviceService
{
    Task<List<Device>> GetAllDevicesAsync();
    Task AddDeviceAsync(Device device);
    Task<Device?> GetDeviceByIdAsync(int id);
    Task<bool> UpdateDeviceAsync(Device device);
    Task<bool> DeleteDeviceAsync(int id);
    DeviceStatus CheckDeviceHealth(Device device);
}

// Implementation - Actual logic
public class DeviceManager(NexusDbContext context) : IDeviceService
{
    public async Task<List<Device>> GetAllDevicesAsync()
    {
        return await context.Devices.ToListAsync();
    }
    // ... rest of implementation
}

// Registration
builder.Services.AddScoped<IDeviceService, DeviceManager>();

// Usage
public class DeviceController(IDeviceService deviceService)
{
    // deviceService is already injected!
}
```

### Benefits
- **Testable**: Mock `IDeviceService` in unit tests
- **Swappable**: Change implementation without changing controller
- **Extensible**: Add new implementations later

### 🎯 **EXERCISE 8** (20 min)
1. Look at `IDeviceService` interface
2. Look at `DeviceManager` implementation
3. Look at `DeviceController` usage
4. Verify all methods from interface are implemented
5. Test that everything works

---

# **DAY 3: PRODUCTION & DEPLOYMENT (6-8 hours)**

## **Session 3.1: Configuration & Environment (60 min)**

### Your appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=NexusDb;Username=postgres;Password=1234"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Debug"
    }
  }
}
```

### Environment-Specific Settings
```
appsettings.json              (Development)
appsettings.Production.json   (Production)
appsettings.Staging.json      (Staging)
```

### Program.cs Usage
```csharp
builder.Services.AddDbContext<NexusDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
```

### Development vs Production
```csharp
// Use different configs based on environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();              // Show Swagger UI in dev
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();     // Force HTTPS in prod
}

// Always use exception handler
app.UseExceptionHandler();
```

### 🎯 **EXERCISE 9** (15 min)
1. Create `appsettings.Production.json`
2. Add production connection string (change Host to production server)
3. Add production CORS policy (restrict to your domain only)
4. Test locally with `ASPNETCORE_ENVIRONMENT=Production`

---

## **Session 3.2: Logging & Monitoring (60 min)**

### Built-in Logging
```csharp
public class DeviceManager(
    NexusDbContext context,
    ILogger<DeviceManager> logger) : IDeviceService
{
    public async Task AddDeviceAsync(Device device)
    {
        logger.LogInformation("Adding device: {DeviceName}", device.Name);
        
        if (string.IsNullOrEmpty(device.IPAddress))
        {
            logger.LogWarning("Invalid IP address for device: {DeviceName}", device.Name);
            throw new ArgumentException("IP required");
        }
        
        try
        {
            await context.Devices.AddAsync(device);
            await context.SaveChangesAsync();
            logger.LogInformation("Device {Id} added successfully", device.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding device {DeviceName}", device.Name);
            throw;
        }
    }
}
```

### Log Levels (Low to High Severity)
- **Debug** - Diagnostic info (variable values)
- **Information** - Important events (user login, device added)
- **Warning** - Something unexpected (retry attempt)
- **Error** - Error occurred but app continues
- **Critical** - Application failure

### What to Log
✅ DO:
- Important operations (device added, deleted)
- Errors with full details
- Performance metrics
- User actions

❌ DON'T:
- Every line of code (spam)
- Passwords or sensitive data
- Excessive database queries

### 🎯 **EXERCISE 10** (15 min)
1. Add `ILogger<DeviceManager>` to constructor
2. Add logging to each CRUD method:
   - Information when operation starts
   - Warning if validation fails
   - Information on success
   - Error if exception occurs
3. Run and check Debug Output window

---

## **Session 3.3: Unit Testing Basics (90 min)**

### Test Structure
```csharp
[TestClass]
public class DeviceServiceTests
{
    private NexusDbContext _context;
    private DeviceManager _service;
    
    [TestInitialize]  // Runs before each test
    public void Setup()
    {
        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<NexusDbContext>()
            .UseInMemoryDatabase("TestDb_" + Guid.NewGuid())
            .Options;
        
        _context = new NexusDbContext(options);
        _service = new DeviceManager(_context, new FakeLogger());
    }
    
    [TestMethod]
    public async Task AddDevice_ValidDevice_Success()
    {
        // Arrange - Setup test data
        var device = new Device 
        { 
            Name = "TestRouter", 
            IPAddress = "192.168.1.1",
            IsOnline = true,
            LastSeen = DateTime.UtcNow
        };
        
        // Act - Perform the action
        await _service.AddDeviceAsync(device);
        
        // Assert - Verify results
        var savedDevice = await _context.Devices.FirstOrDefaultAsync();
        Assert.IsNotNull(savedDevice);
        Assert.AreEqual("TestRouter", savedDevice.Name);
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task AddDevice_NullIP_Throws()
    {
        var device = new Device { Name = "Router", IPAddress = null };
        await _service.AddDeviceAsync(device);
        // Should throw - test passes
    }
    
    [TestMethod]
    public async Task UpdateDevice_NonExistent_ReturnsFalse()
    {
        var device = new Device { Id = 999, Name = "NotExists" };
        var result = await _service.UpdateDeviceAsync(device);
        Assert.IsFalse(result);
    }
    
    [TestMethod]
    public async Task CheckDeviceHealth_NotOnline_ReturnsCritical()
    {
        var device = new Device { IsOnline = false, LastSeen = DateTime.UtcNow };
        var health = _service.CheckDeviceHealth(device);
        Assert.AreEqual(DeviceStatus.Critical, health);
    }
}
```

### Test Pattern: Arrange-Act-Assert
1. **Arrange** - Setup test conditions
2. **Act** - Call method being tested
3. **Assert** - Verify results

### 🎯 **EXERCISE 11** (40 min)
**Write unit tests for DeviceManager:**

Create `Nexus.Tests/DeviceManagerTests.cs`:

1. **Test GetAllDevicesAsync**
   - Add 3 devices
   - Call GetAllDevicesAsync
   - Assert returns 3 devices

2. **Test AddDeviceAsync - Success**
   - Create valid device
   - Add it
   - Verify it's in database

3. **Test AddDeviceAsync - ValidationFails**
   - Create device with null IP
   - Assert throws ArgumentException

4. **Test DeleteDeviceAsync - Success**
   - Add device
   - Delete it
   - Verify it's gone

5. **Test CheckDeviceHealth - AllStates**
   - Device offline → Critical
   - Device online, not checked 30 min → Warning
   - Device online, recent → Healthy

**Setup project first:**
```powershell
dotnet new mstest -n Nexus.Tests
cd Nexus.Tests
dotnet add reference ../Nexus.Business/Nexus.Business.csproj
dotnet add reference ../Nexus.Core/Nexus.Core.csproj
dotnet add reference ../Nexus.Data/Nexus.Data.csproj
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

---

## **Session 3.4: Build & Deploy (90 min)**

### Step 1: Verify Everything Works
```powershell
# Build project
dotnet build

# Run tests
dotnet test

# Run project
dotnet run
```

Visit `https://localhost:7001/swagger` → Should see all endpoints

### Step 2: Database Migration
```powershell
# Create migration
dotnet ef migrations add FinalMigration -p Nexus.Data

# Apply to database
dotnet ef database update -p Nexus.Data
```

### Step 3: Build Release
```powershell
# Clean build for production
dotnet clean
dotnet build -c Release

# Check for warnings/errors
# Fix any issues
```

### Step 4: Publish
```powershell
# Create publishable package
dotnet publish -c Release -o ./publish
```

Output in `./publish` folder contains everything needed to run the app.

### Step 5: Deploy Options

#### Option A: Local IIS
1. Copy `./publish` folder to IIS server
2. Create IIS Application Site
3. Point to published folder
4. Start site

#### Option B: Docker
1. Create `Dockerfile`:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY ./publish .
ENTRYPOINT ["dotnet", "Nexus.Api.dll"]
```

2. Build image:
```powershell
docker build -t nexushub:latest .
```

3. Run container:
```powershell
docker run -p 8000:80 nexushub:latest
```

#### Option C: Azure App Service
1. Install Azure CLI
2. Create resource group:
```powershell
az group create --name NexusHub --location eastus
```

3. Create App Service:
```powershell
az appservice plan create --name NexusHubPlan --resource-group NexusHub --sku B1 --is-linux
az webapp create --resource-group NexusHub --plan NexusHubPlan --name nexushub-api --runtime "DOTNETCORE|8.0"
```

4. Deploy:
```powershell
az webapp deployment source config-zip --resource-group NexusHub --name nexushub-api --src publish.zip
```

### 🎯 **EXERCISE 12: DEPLOY YOUR PROJECT** (40 min)

**Choose one:**

**Option A - Local Testing (Easiest - 10 min)**
1. `dotnet build` - Verify no errors
2. `dotnet run` - Start API
3. Open Swagger UI
4. Test all endpoints work

**Option B - Docker (Better - 20 min)**
1. Follow Docker setup above
2. `docker build -t nexushub:latest .`
3. `docker run -p 8000:80 nexushub:latest`
4. Test at `http://localhost:8000/swagger`

**Option C - Azure (Production - 30 min)**
1. Create Azure account (free tier available)
2. Follow Azure App Service setup
3. Deploy your project
4. Test at `https://nexushub-api.azurewebsites.net/swagger`

---

# **FINAL PROJECT: Build Something Real (2 hours)**

## **Your Task**
Extend NexusHub with a new feature:

### **Option 1: Device Alerts**
- Add `Alert` entity (id, deviceId, severity, message, createdDate)
- Create `IAlertService` interface
- Implement `AlertManager`
- Create `AlertController` with CRUD
- Add unit tests
- Alert is created when device becomes offline

### **Option 2: Device History**
- Add `DeviceHistory` entity (id, deviceId, statusBefore, statusAfter, changedDate)
- Track every device status change
- Create service to log status changes
- Controller to query history for a device
- Add unit tests

### **Option 3: Device Groups**
- Add `Group` entity (id, name, description)
- Add relationship: Group → Many Devices
- Create `IGroupService` interface
- CRUD operations for groups
- Endpoint: GET /api/groups/{id}/devices (all devices in group)
- Add unit tests

### **Option 4: Device Statistics**
- Create endpoint: GET /api/devices/statistics
- Returns: Total devices, Online count, Offline count, Average health
- Create `IStatisticsService`
- Add unit tests

---

## **Deliverables**
1. ✅ New Entity created
2. ✅ Service Interface created
3. ✅ Service Implementation created
4. ✅ Controller with endpoints created
5. ✅ Database migration created & applied
6. ✅ 3+ Unit tests written
7. ✅ Endpoints tested in Swagger/Postman
8. ✅ Code committed to Git

---

## **Git Commit**
```powershell
git add .
git commit -m "feat: add [feature name] - [description]"
git push origin main
```

---

# **SUMMARY - What You Learned**

## Day 1: Foundations
✅ ASP.NET Core & Dependency Injection  
✅ HTTP Methods & REST APIs  
✅ Async/Await for performance  
✅ Entity Framework Core CRUD  
✅ Database Migrations  

## Day 2: APIs
✅ Input Validation & Error Handling  
✅ CORS & Security  
✅ Layered Architecture  
✅ Service Pattern with Interfaces  
✅ Separation of Concerns  

## Day 3: Production
✅ Configuration & Environments  
✅ Logging & Monitoring  
✅ Unit Testing  
✅ Building & Publishing  
✅ Deployment (Local/Docker/Azure)  

---

# **Next Steps After Course**

1. **Week 1**: Complete all exercises
2. **Week 2**: Build the final project
3. **Week 3**: Deploy to production
4. **Week 4+**: 
   - Add authentication (JWT tokens)
   - Add authorization (roles)
   - Add caching (Redis)
   - Add background jobs (Hangfire)
   - Add real-time updates (SignalR)

---

# **Quick Reference**

### Key Files in Your Project
```
Program.cs                              - Configuration & DI
Controllers/DeviceController.cs         - HTTP layer
Business/Abstract/IDeviceService.cs     - Contract
Business/Concrete/DeviceManager.cs      - Logic
Data/NexusDbContext.cs                  - Database
Core/Entities/Device.cs                 - Models
appsettings.json                        - Configuration
```

### Common Commands
```powershell
dotnet build                            # Build
dotnet run                              # Run locally
dotnet test                             # Run tests
dotnet ef migrations add Name           # Create migration
dotnet ef database update               # Apply migration
dotnet publish -c Release -o ./publish  # Publish
```

### Important URLs (Local)
```
API: https://localhost:7001
Swagger UI: https://localhost:7001/swagger
```

---

**🎉 You're ready to build APIs!**

**Course Duration**: 3 intensive days  
**Based on**: Real NexusHub project  
**Your Next Move**: Start the exercises today!
