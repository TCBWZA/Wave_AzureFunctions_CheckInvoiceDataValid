# Azure Functions vs HTTPClient-Based API Architecture

## Overview

This document explains the key architectural differences between **Azure Functions** (serverless) and traditional **HTTPClient-based Web APIs** (ASP.NET Core), highlighting when to use each approach.

---

## Azure Functions (Current Implementation)

### What Are Azure Functions?

Azure Functions is a **serverless compute service** that allows you to run event-driven code without managing infrastructure. Functions scale automatically and you only pay for the compute time you consume.

### Architecture Characteristics

#### 1. **Function-as-a-Service (FaaS)**
```csharp
// Each function is an independent, self-contained endpoint
[Function("CreateCustomer")]
public async Task<IActionResult> CreateCustomer(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "customers")] HttpRequestData req)
{
    // Function logic
}
```

**Key Points:**
- Each function is a separate unit of deployment
- Functions can have different triggers (HTTP, Timer, Queue, etc.)
- No application-level startup or configuration for individual functions
- Functions share the same host runtime but are logically independent

#### 2. **Serverless Execution Model**
- **Cold Start**: First request may have latency (100ms-2s) as function instance spins up
- **Auto-scaling**: Azure automatically scales based on load (0 to thousands of instances)
- **Stateless**: Each invocation is independent; no in-memory state between calls
- **Pay-per-execution**: Billed only for actual execution time (not idle time)

#### 3. **Deployment & Hosting**
```powershell
# From SampleRolloutScript.ps1
# Creates a serverless function app
az functionapp create `
  --name $FunctionAppName `
  --resource-group $ResourceGroupName `
  --storage-account $StorageAccountName `
  --consumption-plan-location $Location `
  --runtime dotnet-isolated `
  --functions-version 4
```

**Characteristics:**
- No web server to manage (IIS, Kestrel runs internally but is abstracted)
- Consumption plan = serverless (auto-scale, pay-per-use)
- Premium/Dedicated plans available for always-on scenarios
- Requires Azure Storage for function metadata and state

#### 4. **Request/Response Model**
```csharp
// Isolated worker process model (.NET 8)
public async Task<IActionResult> CreateCustomer(
    [HttpTrigger(...)] HttpRequestData req)  // HttpRequestData (not HttpRequest)
{
    // Manual deserialization
    var body = await new StreamReader(req.Body).ReadToEndAsync();
    var dto = JsonSerializer.Deserialize<CreateCustomerDto>(body);
    
    // Return IActionResult
    return new OkObjectResult(new { customer = dto });
}
```

**Key Differences:**
- Uses `HttpRequestData` instead of `HttpRequest` (isolated model)
- Manual JSON deserialization (no built-in model binding)
- Returns `IActionResult` but through a different pipeline
- No middleware pipeline like ASP.NET Core
- Authentication via function keys or Azure AD

#### 5. **Configuration & Settings**
```json
// local.settings.json - Function-specific configuration
{
    "Values": {
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
    }
}
```

```json
// host.json - Global function host configuration
{
    "version": "2.0",
    "logging": { ... }
}
```

**Characteristics:**
- Two-level configuration: `host.json` (global) and `local.settings.json` (app settings)
- No `appsettings.json` or `Startup.cs` in isolated model
- Environment variables via Azure Portal or CLI

---

## HTTPClient-Based Web API (Traditional ASP.NET Core)

### What Is an HTTPClient-Based API?

A traditional **ASP.NET Core Web API** is a web application hosted on a web server (Kestrel/IIS) that handles HTTP requests through a middleware pipeline. HTTPClient refers to the client-side component used to consume such APIs.

### Architecture Characteristics

#### 1. **Monolithic Web Application**
```csharp
// Program.cs - Single application entry point
var builder = WebApplication.CreateBuilder(args);

// Add services to DI container
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

var app = builder.Build();

// Configure middleware pipeline
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

**Key Points:**
- Single application with multiple controllers/endpoints
- Centralized configuration and dependency injection
- Shared middleware pipeline for all requests
- Application lifecycle (Startup, Configuration, Shutdown)

#### 2. **Always-On Execution Model**
```csharp
// Controller with dependency injection
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;
    
    public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto dto)
    {
        // Business logic
    }
}
```

**Characteristics:**
- **Always running**: Application starts once and stays running
- **No cold starts**: First request is fast (application is already warm)
- **In-memory state**: Can maintain caches, connection pools, background services
- **Fixed capacity**: Scales by adding more instances (manual or auto-scale rules)
- **Pay for reserved capacity**: Billed for compute time whether handling requests or idle

#### 3. **Deployment & Hosting**
```powershell
# Traditional web app deployment
az webapp create `
  --name mywebapp `
  --resource-group mygroup `
  --plan myappserviceplan `
  --runtime "DOTNET:8.0"

# Requires an App Service Plan (pre-provisioned capacity)
az appservice plan create `
  --name myappserviceplan `
  --resource-group mygroup `
  --sku B1  # Basic, Standard, Premium tiers
```

**Characteristics:**
- Hosted on App Service, VMs, or containers
- Requires App Service Plan with fixed pricing tier
- Web server (Kestrel) is explicitly managed
- No external storage dependency

#### 4. **Request/Response Model**
```csharp
[HttpPost]
public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CreateCustomerDto dto)
{
    // Built-in model binding and validation
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }
    
    // Business logic with injected services
    var customer = await _customerService.CreateAsync(dto);
    
    // Return typed result
    return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
}
```

**Key Differences:**
- Uses standard `HttpRequest` and `HttpResponse`
- **Automatic model binding** from JSON to DTO
- **Automatic validation** via `[FromBody]` and DataAnnotations
- Rich middleware pipeline (authentication, CORS, compression, etc.)
- Built-in dependency injection throughout the pipeline

#### 5. **Configuration & Settings**
```json
// appsettings.json - Hierarchical configuration
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=..."
  },
  "AppSettings": {
    "MaxPageSize": 100
  }
}
```

```csharp
// Startup configuration
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));

// Environment-specific files
// appsettings.Development.json
// appsettings.Production.json
```

**Characteristics:**
- Rich configuration system with providers (JSON, Environment, User Secrets, Key Vault)
- `IConfiguration` available throughout application
- Options pattern for strongly-typed settings
- Environment-specific configuration files

---

## Side-by-Side Comparison

| Feature | Azure Functions | ASP.NET Core Web API |
|---------|----------------|---------------------|
| **Execution Model** | Serverless, event-driven | Always-on, request-response |
| **Scaling** | Automatic (0-∞ instances) | Manual or auto-scale rules |
| **Cold Start** | Yes (100ms-2s first request) | No (app is always warm) |
| **Pricing** | Pay-per-execution + storage | Pay for reserved capacity (App Service Plan) |
| **State Management** | Stateless (external storage required) | Stateful (in-memory caching supported) |
| **Deployment Unit** | Individual functions | Entire application |
| **Dependency Injection** | Function-level (limited scope) | Application-level (full framework support) |
| **Middleware** | Limited (Functions middleware) | Rich pipeline (20+ built-in middleware) |
| **Model Binding** | Manual deserialization | Automatic via `[FromBody]`, etc. |
| **Validation** | Manual (Validator.TryValidateObject) | Automatic (ModelState) |
| **Request Type** | `HttpRequestData` (isolated model) | `HttpRequest` |
| **Authentication** | Function keys, Azure AD, EasyAuth | JWT, Cookies, OAuth2, custom middleware |
| **Configuration** | `host.json`, `local.settings.json` | `appsettings.json`, Options pattern |
| **Background Jobs** | Timer triggers, Queue triggers | Hosted Services, Hangfire, Quartz |
| **Logging** | Application Insights (default) | ILogger + providers (Console, AppInsights, etc.) |
| **Database Connections** | Per-function instance (use pooling) | Shared connection pool across app |
| **WebSockets** | Not supported in Consumption plan | Fully supported |
| **Best For** | Event-driven, sporadic workloads | Consistent traffic, complex workflows |

---

## Code Examples from Current Project

### Azure Functions Approach (Current)

```csharp
// CustomerFunctions.cs
public class CustomerFunctions
{
    private readonly ILogger<CustomerFunctions> _logger;

    public CustomerFunctions(ILogger<CustomerFunctions> logger)
    {
        _logger = logger;
    }

    [Function("CreateCustomer")]
    public async Task<IActionResult> CreateCustomer(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "customers")] HttpRequestData req)
    {
        _logger.LogInformation("CreateCustomer called.");

        // Manual deserialization
        var body = await new StreamReader(req.Body).ReadToEndAsync();
        CreateCustomerDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<CreateCustomerDto>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Invalid JSON.");
            return new BadRequestObjectResult(new { error = "Invalid JSON payload." });
        }

        if (dto == null)
        {
            return new BadRequestObjectResult(new { error = "Request body is empty or could not be deserialized." });
        }

        // Manual validation
        var validationErrors = ValidateModel(dto);
        if (validationErrors.Any())
        {
            return new BadRequestObjectResult(new { errors = validationErrors });
        }

        return new OkObjectResult(new { message = "CreateCustomer validation passed.", customer = dto });
    }

    // Helper method for validation (not built-in)
    private static IDictionary<string, string[]> ValidateModel(object model)
    {
        var context = new ValidationContext(model, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        // ... error mapping
    }
}
```

### Equivalent ASP.NET Core Web API Approach

```csharp
// CustomersController.cs (hypothetical)
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CreateCustomerDto dto)
    {
        _logger.LogInformation("CreateCustomer called.");

        // Automatic model binding and validation
        // If validation fails, BadRequest is returned automatically
        // No need for manual deserialization or validation

        var customer = await _customerService.CreateAsync(dto);
        
        return CreatedAtAction(
            nameof(GetCustomer), 
            new { id = customer.Id }, 
            customer);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(long id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        
        if (customer == null)
        {
            return NotFound();
        }
        
        return customer;
    }
}

// Program.cs - Single startup
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ICustomerService, CustomerService>();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();
app.Run();
```

---

## When to Use Azure Functions

### Ideal Use Cases

1. **Event-Driven Processing**
   - File upload triggers (process images, parse CSV)
   - Queue message processing (background jobs)
   - Timer-based tasks (scheduled cleanups, reports)

2. **Microservices with Sporadic Traffic**
   - Webhook handlers
   - API endpoints with unpredictable load
   - Integration endpoints between systems

3. **Cost-Sensitive Scenarios**
   - Development/test environments (scale to zero when not in use)
   - Low-traffic APIs (pay only for actual requests)
   - Burst workloads (scale up, then down automatically)

4. **Rapid Prototyping**
   - Quick API development without infrastructure setup
   - POC/MVPs with minimal DevOps overhead

### Example from Current Project
```powershell
# From SampleRolloutScript.ps1
# Deploy to dev environment with minimal cost
.\SampleRolloutScript.txt -Environment dev

# Function app scales to zero when not in use
# No charges when idle (except minimal storage cost)
```

---

## When to Use ASP.NET Core Web API

### Ideal Use Cases

1. **High-Throughput, Consistent Traffic**
   - Public-facing APIs with steady request rate
   - Customer portals with 24/7 usage
   - APIs requiring predictable latency (no cold starts)

2. **Complex Application Logic**
   - Multi-layer architecture (Controllers, Services, Repositories)
   - Shared services across multiple endpoints
   - Background services and hosted workers

3. **Rich Middleware Requirements**
   - Custom authentication/authorization
   - Request/response transformation
   - CORS with complex policies
   - Rate limiting, caching strategies

4. **Stateful Operations**
   - In-memory caching (IMemoryCache)
   - SignalR for real-time communication
   - Session management

5. **Database-Heavy Applications**
   - Entity Framework Core with migrations
   - Shared DbContext across requests
   - Connection pooling optimization

### Example Architecture
```csharp
// Layered architecture with Web API
InvoiceApi/
  +-- Controllers/
      +-- CustomersController.cs
      +-- InvoicesController.cs
      +-- TelephoneNumbersController.cs
  +-- Services/
      +-- ICustomerService.cs
      +-- CustomerService.cs
  +-- Repositories/
      +-- ICustomerRepository.cs
      +-- CustomerRepository.cs
  +-- Data/
      +-- AppDbContext.cs
  +-- DTOs/
      +-- CustomerDto.cs
  +-- Program.cs
```

---

## HTTPClient: The Consumer Side

### What Is HTTPClient?

`HttpClient` is a .NET class used to **consume** HTTP APIs (both Azure Functions and Web APIs). It's not an alternative to Azure Functions or Web APIs—it's how you call them.

### Example: Calling Azure Functions with HTTPClient

```csharp
public class InvoiceApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _functionKey;

    public InvoiceApiClient(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _functionKey = config["FunctionApp:FunctionKey"];
        _httpClient.BaseAddress = new Uri(config["FunctionApp:BaseUrl"]);
    }

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto)
    {
        // Add function key to request
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/customers")
        {
            Content = JsonContent.Create(dto)
        };
        request.Headers.Add("x-functions-key", _functionKey);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CustomerDto>();
    }
}
```

### Usage in Dependency Injection

```csharp
// Program.cs (in a consuming application)
builder.Services.AddHttpClient<InvoiceApiClient>(client =>
{
    client.BaseAddress = new Uri("https://funcapp-prod-1234.azurewebsites.net");
    client.DefaultRequestHeaders.Add("x-functions-key", functionKey);
});
```

---

## Hybrid Approach: Functions + Web API

Many organizations use **both** patterns:

```
┌─────────────────────────────────────────────┐
│  Public API (ASP.NET Core Web API)          │
│  - Customer-facing endpoints                 │
│  - Complex business logic                    │
│  - Always-on, low latency                    │
└─────────────────────────────────────────────┘
                    │
                    │ Publishes events to
                    ▼
┌─────────────────────────────────────────────┐
│  Azure Functions (Event Processing)         │
│  - Invoice processing (Queue trigger)       │
│  - Email notifications (Queue trigger)      │
│  - Report generation (Timer trigger)        │
│  - Data export (Storage trigger)            │
└─────────────────────────────────────────────┘
```

### Example Workflow
1. **Web API**: Receives customer invoice request
2. **Web API**: Validates and saves to database
3. **Web API**: Publishes message to Azure Queue
4. **Azure Function**: Queue trigger processes invoice
5. **Azure Function**: Generates PDF
6. **Azure Function**: Sends email notification

---

## Migration Considerations

### From Azure Functions to Web API

**Reasons to migrate:**
- Eliminate cold starts for user-facing endpoints
- Need complex middleware or authentication
- Require in-memory caching or state
- High, consistent request volume makes Web API more cost-effective

**Changes required:**
```csharp
// Before (Azure Functions)
[Function("CreateCustomer")]
public async Task<IActionResult> CreateCustomer(
    [HttpTrigger(...)] HttpRequestData req)

// After (Web API)
[HttpPost]
public async Task<ActionResult<CustomerDto>> CreateCustomer(
    [FromBody] CreateCustomerDto dto)
```

### From Web API to Azure Functions

**Reasons to migrate:**
- Reduce costs for low-traffic APIs
- Need event-driven processing (queues, timers)
- Want automatic scaling without configuration
- Simplify infrastructure management

**Changes required:**
```csharp
// Before (Web API)
public CustomersController(ICustomerService service)

// After (Azure Functions)
public CustomerFunctions(ILogger<CustomerFunctions> logger)
// Note: Services require manual setup in isolated model
```

---

## Summary

| Aspect | Azure Functions | ASP.NET Core Web API |
|--------|----------------|---------------------|
| **Architecture** | Serverless, function-based | Monolithic, controller-based |
| **Best For** | Event-driven, variable load | Consistent traffic, complex apps |
| **Developer Experience** | More manual work (validation, binding) | Rich framework features |
| **Cost Model** | Pay-per-execution | Pay-per-capacity |
| **Cold Starts** | Yes (mitigated with Premium plan) | No |
| **Scalability** | Automatic (infinite scale) | Manual/auto-scale configuration |
| **State** | Stateless (use external storage) | Can maintain in-memory state |

## Recommendation for This Project

**Current State**: Azure Functions with isolated worker model (.NET 8)

**Good fit for:**
- Invoice validation APIs (intermittent usage)
- Customer and telephone number management (CRUD operations)
- Future event processing (invoice generation, notifications)

**Consider Web API if:**
- Adding complex business logic layers
- Implementing Entity Framework Core with migrations
- Requiring real-time features (SignalR)
- Cold starts become problematic for users

**Optimal Approach**: Start with Azure Functions (current implementation), migrate specific high-traffic endpoints to Web API if needed, and use Functions for background processing.

---

## Resources

- [Azure Functions Documentation](https://learn.microsoft.com/azure/azure-functions/)
- [ASP.NET Core Web API Documentation](https://learn.microsoft.com/aspnet/core/web-api/)
- [HttpClient Best Practices](https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient-guidelines)
- [Choosing Between Azure Functions and Web Apps](https://learn.microsoft.com/azure/architecture/guide/technology-choices/compute-decision-tree)
