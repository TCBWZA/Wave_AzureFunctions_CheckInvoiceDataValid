# Azure Functions - Invoice Validation Example

> **Educational Example Code for Module 7**  
> This repository contains example code demonstrating Azure Functions with .NET 8 for students learning serverless development.

## Overview

This project demonstrates a complete Azure Functions application built with **C# 12** and **.NET 8** using the **isolated worker process model**. It implements a RESTful API for managing invoices, customers, and telephone numbers with comprehensive validation.

**Module 7 Learning Objectives:**
- Understanding serverless architecture with Azure Functions
- Implementing HTTP-triggered functions with .NET 8
- Working with Data Transfer Objects (DTOs) and validation
- Deploying to Azure using PowerShell scripts
- Configuring Azure Functions with `host.json` and `local.settings.json`

## Project Structure

```
CheckInvoiceDataValid/
+-- CheckInvoiceDataValid/              # Main function app project
    +-- CustomerFunctions.cs             # ✅ Customer CRUD (DataAnnotations validation)
    +-- CustomerFunctionsWithFluentValidation.cs  # ✅ Customer CRUD (FluentValidation)
    +-- InvoiceFunctions.cs              # ⚠️ TO BE CREATED BY STUDENTS (Exercise 1)
    +-- TelephoneNumberFunctions.cs      # ⚠️ TO BE CREATED BY STUDENTS (Exercise 2)
    +-- Validators/                      # FluentValidation validators
        +-- CreateCustomerValidator.cs   # ✅ Customer creation validation rules
        +-- UpdateCustomerValidator.cs   # ✅ Customer update validation rules
    +-- DTOs/                            # Data Transfer Objects
        +-- CustomerDto.cs               # Complete with Create/Update variants
        +-- InvoiceDto.cs                # Complete with Create/Update variants
        +-- TelephoneNumberDto.cs        # Complete with Create/Update variants
    +-- host.json                        # Global function configuration
    +-- local.settings.json              # Local development settings
    +-- CheckInvoiceDataValid.csproj     # Project file (includes FluentValidation)
    +-- Program.cs                       # Application entry point (DI setup)
+-- SampleRolloutScript.txt             # Deployment script template (rename to .ps1 to use)
+-- test-validation-approaches.txt      # Test script template (rename to .ps1 to use)
+-- instructions.md                      # Detailed setup and deployment guide
+-- architecture-comparison.md           # Azure Functions vs Web API comparison
+-- fluentvalidation-guide.md           # FluentValidation tutorial
+-- isolated-worker-model-explained.md  # Isolated worker model explanation
+-- README.md                            # This file
```

**Note:** The DTOs for all entities (Customer, Invoice, TelephoneNumber) are complete and available for use in your implementations.

## Features Implemented

### Customer Functions (`CustomerFunctions.cs`) - ✅ Reference Implementation

This is the **complete reference implementation** for students to learn from:

- **CreateCustomer** - `POST /api/customers`
- **UpdateCustomer** - `PUT /api/customers/{id}`
- **GetCustomer** - `GET /api/customers/{id}`
- **GetAllCustomers** - `GET /api/customers`
- **DeleteCustomer** - `DELETE /api/customers/{id}`

### Student Exercises - ⚠️ To Be Implemented

The following functions are **intentionally left for students to implement** as learning exercises:

#### 1. Invoice Functions (Exercise: Create `InvoiceFunctions.cs`)
- **CreateInvoice** - `POST /api/invoices`
- **UpdateInvoice** - `PUT /api/invoices/{id}`
- **GetInvoice** - `GET /api/invoices/{id}` (optional)
- **GetAllInvoices** - `GET /api/invoices` (optional)
- **DeleteInvoice** - `DELETE /api/invoices/{id}` (optional)

**Learning Goal:** Practice creating HTTP-triggered functions following the pattern in `CustomerFunctions.cs`.

**Hints:**
- Use `CreateInvoiceDto` and `UpdateInvoiceDto` from the DTOs folder
- Follow the same validation pattern as CustomerFunctions
- Invoice DTOs include: InvoiceNumber, InvoiceDate, DueDate, CustomerId, TotalAmount

#### 2. Telephone Number Functions (Exercise: Create `TelephoneNumberFunctions.cs`)
- **CreateTelephoneNumber** - `POST /api/telephone-numbers`
- **UpdateTelephoneNumber** - `PUT /api/telephone-numbers/{id}`
- **GetTelephoneNumber** - `GET /api/telephone-numbers/{id}` (optional)
- **GetCustomerTelephoneNumbers** - `GET /api/customers/{customerId}/telephone-numbers` (optional)
- **DeleteTelephoneNumber** - `DELETE /api/telephone-numbers/{id}` (optional)

**Learning Goal:** Practice route parameters and validation with complex DTOs.

**Hints:**
- Use `CreateTelephoneNumberDto` and `UpdateTelephoneNumberDto`
- Type validation: Must be "Mobile", "Work", or "DirectDial"
- The nested route requires capturing `customerId` from the path

## Key Learning Concepts

### 1. Serverless Architecture
```csharp
[Function("CreateCustomer")]
public async Task<IActionResult> CreateCustomer(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "customers")] HttpRequestData req)
{
    // Serverless function triggered by HTTP request
    // Scales automatically, pay-per-execution
}
```

**Learning Points:**
- Functions as independent deployment units
- Auto-scaling and pay-per-execution pricing
- Cold starts and warm instances
- Isolated worker process model (.NET 8)

### 2. Data Validation with DataAnnotations

```csharp
public class CreateCustomerDto
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string Email { get; set; } = string.Empty;
}
```

**Learning Points:**
- Declarative validation with attributes
- Custom error messages
- Regular expressions for pattern matching
- Manual validation in isolated worker model

**See also:** `CustomerFunctions.cs` for implementation

### 2b. Alternative: FluentValidation (Advanced)

```csharp
public class CreateCustomerValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address format.");
    }
}
```

**Learning Points:**
- Code-based validation (more flexible than attributes)
- Separation of validation logic from DTOs
- Complex conditional rules
- Async validation support
- Better testability

**See also:** 
- `CustomerFunctionsWithFluentValidation.cs` for implementation
- `Validators/CreateCustomerValidator.cs` for validator class
- `fluentvalidation-guide.md` for complete tutorial

### 3. Configuration Management
```json
// host.json - Runtime configuration
{
    "version": "2.0",
    "logging": {
        "applicationInsights": {
            "samplingSettings": {
                "isEnabled": true
            }
        }
    }
}
```

```json
// local.settings.json - Application settings
{
    "Values": {
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
    }
}
```

**Learning Points:**
- Two-tier configuration (host.json and local.settings.json)
- Environment-specific settings
- Application Insights integration

### 4. Deployment Automation
```powershell
# SampleRolloutScript.txt
.\SampleRolloutScript.ps1 -Environment dev -DryRun
```

**Learning Points:**
- Infrastructure as Code with PowerShell
- Environment-specific deployments (dev/test/prod)
- Dry-run mode for safe testing
- Azure CLI integration

## Getting Started

### Prerequisites

Students should have installed:
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure Functions Core Tools v4](https://learn.microsoft.com/azure/azure-functions/functions-run-local)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) (for deployment)
- [Git](https://git-scm.com/) (for cloning the repository)

### Quick Start

1. **Clone the repository**
   ```powershell
   git clone https://github.com/TCBWZA/Wave_AzureFunctions_CheckInvoiceDataValid.git
   cd Wave_AzureFunctions_CheckInvoiceDataValid
   ```

2. **Create local settings**
   
   Create a `local.settings.json` file in the `CheckInvoiceDataValid` folder:
   ```json
   {
       "IsEncrypted": false,
       "Values": {
           "AzureWebJobsStorage": "UseDevelopmentStorage=true",
           "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
       }
   }
   ```

3. **Run locally (Visual Studio)**
- Open `CheckInvoiceDataValid.slnx` in Visual Studio 2022
- Press **F5** to start debugging
- Functions will be available at `http://localhost:7071`

4. **Run locally (Command Line)**
   ```powershell
   cd CheckInvoiceDataValid
   func start
   ```

5. **Test an endpoint**
   ```powershell
   # Create a customer
   $body = @{
       name = "John Doe"
       email = "john.doe@example.com"
   } | ConvertTo-Json

   Invoke-RestMethod -Uri "http://localhost:7071/api/customers" `
       -Method POST `
       -Body $body `
       -ContentType "application/json"
   ```

## Testing the Functions

### Example: Create Customer (Reference Implementation)

This example uses the **completed CustomerFunctions** as a reference. Students should test their Invoice and Telephone implementations similarly.

**Request:**
```powershell
$body = @{
    name = "Acme Corporation"
    email = "info@acme.com"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:7071/api/customers" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

**Response:**
```json
{
  "message": "CreateCustomer validation passed.",
  "customer": {
    "name": "Acme Corporation",
    "email": "info@acme.com"
  }
}
```

### Example: Test Validation Errors

**Request with invalid data:**
```powershell
$body = @{
    name = ""
    email = "invalid-email"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:7071/api/customers" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

**Expected Response:**
```json
{
  "errors": {
    "Name": ["The Name field is required."],
    "Email": ["Invalid email address format."]
  }
}
```

### Student Exercise: Test Your Invoice Implementation

After completing Exercise 1, test your `CreateInvoice` function:

**Request:**
```powershell
$body = @{
    invoiceNumber = "INV-2024-001"
    invoiceDate = "2024-01-15"
    dueDate = "2024-02-15"
    customerId = 1
    totalAmount = 1500.50
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:7071/api/invoices" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

### Student Exercise: Test Your Telephone Number Implementation

After completing Exercise 2, test your `CreateTelephoneNumber` function:

**Request:**
```powershell
$body = @{
    customerId = 1
    type = "Mobile"
    number = "+1-555-0123"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:7071/api/telephone-numbers" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

**Expected Response:**
```json
{
  "message": "CreateTelephoneNumber validation passed.",
  "telephoneNumber": {
    "customerId": 1,
    "type": "Mobile",
    "number": "+1-555-0123"
  }
}
```

## Deploying to Azure

### Option 1: Dry Run (Recommended for Learning)

Test the deployment script without making actual changes:

```powershell
.\SampleRolloutScript.ps1 -Environment dev -DryRun
```

This shows what commands would be executed without actually running them.

### Option 2: Deploy to Development Environment

```powershell
.\SampleRolloutScript.ps1 -Environment dev
```

This creates:
- Resource Group: `rg-func-dev`
- Storage Account: `funcdev####`
- Function App: `funcapp-dev-####`

### Option 3: Manual Deployment

Follow the detailed instructions in [`instructions.md`](instructions.md) for step-by-step deployment guidance.

## Additional Documentation

### For Students

- **[instructions.md](instructions.md)** - Complete setup and deployment guide
  - Running locally (Visual Studio, VS Code, Command Line)
  - Configuration files explained
  - Azure deployment step-by-step
  - Testing in Azure
  - Troubleshooting common issues

- **[architecture-comparison.md](architecture-comparison.md)** - Azure Functions vs ASP.NET Core Web API
  - Architecture differences
  - When to use each approach
  - Code comparisons
  - HTTPClient usage (consuming APIs)

- **[fluentvalidation-guide.md](fluentvalidation-guide.md)** - FluentValidation Tutorial
  - DataAnnotations vs FluentValidation comparison
  - Implementation examples with CustomerFunctions
  - Advanced validation techniques
  - When to use each approach
  - Student exercises

- **[isolated-worker-model-explained.md](isolated-worker-model-explained.md)** - Isolated Worker Model Explained
  - What is the isolated worker model
  - In-process vs isolated worker comparison
  - Why this project uses .NET 8
  - Code differences and examples
  - Benefits and trade-offs
  - Package cleanup recommendations

### Commented Configuration Files

Both configuration files include detailed comments explaining each setting:
- `host.json` - Runtime behavior configuration
- `local.settings.json` - Application settings

### Code Examples

- **`CustomerFunctions.cs`** - Reference implementation using **DataAnnotations** validation
- **`CustomerFunctionsWithFluentValidation.cs`** - Alternative implementation using **FluentValidation**
- **`Validators/`** folder - FluentValidation validator classes

## Learning Exercises

### Exercise 1: Implement Invoice Functions (Required)
**Objective:** Practice creating HTTP-triggered functions by implementing the complete invoice management API

**Task:** Create a new file `InvoiceFunctions.cs` with the following functions:
1. `CreateInvoice` - POST endpoint with validation
2. `UpdateInvoice` - PUT endpoint with ID parameter
3. `GetInvoice` - GET endpoint to retrieve by ID (bonus)
4. `GetAllInvoices` - GET endpoint to list all (bonus)
5. `DeleteInvoice` - DELETE endpoint (bonus)

**Hints:**
- Copy the structure from `CustomerFunctions.cs` as a starting point
- Use `CreateInvoiceDto` and `UpdateInvoiceDto` from the DTOs folder
- Don't forget the `ValidateModel` helper method
- Invoice validation includes: InvoiceNumber, InvoiceDate, DueDate, CustomerId, TotalAmount
- Test with PowerShell after implementation

**Expected Routes:**
```
POST   /api/invoices
PUT    /api/invoices/{id}
GET    /api/invoices/{id}
GET    /api/invoices
DELETE /api/invoices/{id}
```

### Exercise 2: Implement Telephone Number Functions (Required)
**Objective:** Practice route parameters and complex validation patterns

**Task:** Create a new file `TelephoneNumberFunctions.cs` with telephone number CRUD operations.

**Hints:**
- Type must be validated against: "Mobile", "Work", or "DirectDial"
- Implement the nested route: `GET /api/customers/{customerId}/telephone-numbers`
- Use `[HttpTrigger(AuthorizationLevel.Function, "get", Route = "customers/{customerId:long}/telephone-numbers")]`
- The `RegularExpression` attribute is already on the DTO

**Validation Requirements:**
- CustomerId: Required, must be > 0
- Type: Required, must be "Mobile", "Work", or "DirectDial"
- Number: Required, max 50 characters

### Exercise 3: Add Enhanced Validation
**Objective:** Extend DataAnnotations validation with custom patterns

**Task:** Add phone number format validation to `TelephoneNumberDto`.

**Hints:**
- Add `[RegularExpression(@"^\+?[1-9]\d{1,14}$", ErrorMessage = "Invalid phone number format (E.164).")]` to Number property
- Or use simpler pattern: `@"^[\d\s\-\+\(\)]+$"` for basic format
- Test with valid and invalid phone numbers

### Exercise 4: Add a New Function
**Objective:** Practice creating HTTP-triggered functions from scratch

**Task:** Add a `GetAllTelephoneNumbers` function that returns all telephone numbers (not filtered by customer).

**Hints:**
- Use `[HttpTrigger(AuthorizationLevel.Function, "get", Route = "telephone-numbers")]`
- Return an `OkObjectResult` with an empty list (no database yet)
- Update the route to avoid conflicts with existing routes

### Exercise 5: Deploy with Environments
**Objective:** Learn infrastructure as code

**Task:** Deploy the same function app to dev, test, and prod environments.
(Requires the renaming of the SampleRolloutScript.txt to SampleRolloutScript.ps1)

**Commands:**
```powershell
.\SampleRolloutScript.ps1 -Environment dev -DryRun
.\SampleRolloutScript.ps1 -Environment test -DryRun
.\SampleRolloutScript.ps1 -Environment prod -DryRun
```

Observe how resource names differ per environment.

### Exercise 4: Add Application Insights
**Objective:** Learn monitoring and diagnostics

**Task:** Enable Application Insights and view telemetry.

**Hints:**
- Follow the "Enable Application Insights" section in `instructions.md`
- View Live Metrics in Azure Portal
- Check execution logs

## Common Issues for Students

### Issue: "No job functions found"
**Solution:** Ensure you're running from the project directory containing the `.csproj` file.

### Issue: Port 7071 already in use
**Solution:** Another Functions instance is running. Stop it or use a different port in `local.settings.json`.

### Issue: Storage emulator errors
**Solution:** Install [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite) for local storage emulation.

### Issue: Functions not appearing in Azure after deployment
**Solution:** Verify `FUNCTIONS_WORKER_RUNTIME` is set to `dotnet-isolated` in Azure Portal configuration.

### Issue: CORS Errors (Cross-Origin Resource Sharing)

**Symptoms:**
- Browser console shows: `"Access to fetch at '...' from origin '...' has been blocked by CORS policy"`
- Error: `"No 'Access-Control-Allow-Origin' header is present on the requested resource"`
- Functions work in Postman/PowerShell but fail in web browsers

**What is CORS?**

CORS is a security feature in browsers that prevents web pages from making requests to a different domain than the one serving the page. For example:
- Your web app runs on `http://localhost:3000` (React/Angular/Vue app)
- Your Azure Function runs on `http://localhost:7071`
- Browser blocks the request because the origins don't match

**Solution 1: Configure CORS Locally**

Add CORS configuration to `host.json`:

```json
{
    "version": "2.0",
    "logging": { ... },
    "extensions": {
        "http": {
            "routePrefix": "api",
            "cors": {
                "allowedOrigins": [
                    "http://localhost:3000",
                    "http://localhost:4200",
                    "http://localhost:5173"
                ],
                "supportCredentials": false
            }
        }
    }
}
```

**During development**, you can allow all origins (NOT recommended for production):

```json
{
    "extensions": {
        "http": {
            "cors": {
                "allowedOrigins": ["*"]
            }
        }
    }
}
```

**Solution 2: Configure CORS in Azure Portal**

1. Navigate to your Function App in [Azure Portal](https://portal.azure.com)
2. Go to **Settings > CORS**
3. Add allowed origins:
   - For development: `http://localhost:3000`
   - For production: `https://your-app.azurewebsites.net`
4. Optionally check **Enable Access-Control-Allow-Credentials**
5. Click **Save**

**Solution 3: Configure CORS with Azure CLI**

```powershell
# Allow specific origins
az functionapp cors add `
  --name $FunctionAppName `
  --resource-group $ResourceGroupName `
  --allowed-origins "https://your-app.com"

# Allow multiple origins
az functionapp cors add `
  --name $FunctionAppName `
  --resource-group $ResourceGroupName `
  --allowed-origins "https://app1.com" "https://app2.com"

# View current CORS settings
az functionapp cors show `
  --name $FunctionAppName `
  --resource-group $ResourceGroupName

# Remove all CORS settings
az functionapp cors remove `
  --name $FunctionAppName `
  --resource-group $ResourceGroupName `
  --allowed-origins "https://app.com"
```

**Solution 4: Testing with a Simple HTML Page**

Create `test.html` to test CORS locally:

```html
<!DOCTYPE html>
<html>
<head>
    <title>Test Azure Functions</title>
</head>
<body>
    <h1>Test Azure Functions CORS</h1>
    <button onclick="testFunction()">Test CreateCustomer</button>
    <pre id="result"></pre>

    <script>
        async function testFunction() {
            try {
                const response = await fetch('http://localhost:7071/api/customers', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        name: 'Test Customer',
                        email: 'test@example.com'
                    })
                });
                
                const data = await response.json();
                document.getElementById('result').textContent = JSON.stringify(data, null, 2);
            } catch (error) {
                document.getElementById('result').textContent = 'Error: ' + error.message;
            }
        }
    </script>
</body>
</html>
```

Open this file in a browser (e.g., `file:///C:/path/to/test.html`) and click the button. If you see a CORS error, apply Solution 1 above.

**CORS Best Practices for Students:**

1. **Development:**
   - Use `allowedOrigins: ["*"]` in `host.json` for quick testing
   - Or specify exact localhost ports: `["http://localhost:3000"]`

2. **Production:**
   - **NEVER** use `"*"` in production
   - Specify exact domains: `["https://myapp.azurewebsites.net"]`
   - Use environment-specific configuration

3. **Credentials:**
   - Set `supportCredentials: true` only if you need cookies/authentication headers
   - Requires exact origin (cannot use `"*"`)

4. **Preflight Requests:**
   - Browsers send OPTIONS requests before actual requests
   - Azure Functions handle this automatically when CORS is configured
   - If you see OPTIONS requests failing, check CORS settings

**Common CORS Mistakes:**

```json
// WRONG - Missing protocol
"allowedOrigins": ["localhost:3000"]

// CORRECT - Include protocol
"allowedOrigins": ["http://localhost:3000"]

// WRONG - Trailing slash
"allowedOrigins": ["http://localhost:3000/"]

// CORRECT - No trailing slash
"allowedOrigins": ["http://localhost:3000"]

// WRONG - Using * with credentials
"allowedOrigins": ["*"],
"supportCredentials": true

// CORRECT - Specific origin with credentials
"allowedOrigins": ["http://localhost:3000"],
"supportCredentials": true
```

**Troubleshooting CORS:**

1. **Check browser console** for exact error message
2. **Verify origins match exactly** (protocol, domain, port)
3. **Restart the function host** after changing `host.json`
4. **Clear browser cache** and hard reload (Ctrl+Shift+R)
5. **Test with Postman first** to confirm function works (Postman ignores CORS)
6. **Check Azure Portal CORS settings** if deployed to Azure

**Alternative: Azure Functions Proxies (Legacy)**

For complex scenarios, consider using Azure API Management or reverse proxies, but these are beyond Module 7 scope.

## Module 7 Concepts Covered

- [x] **Azure Functions Basics**
  - Function triggers and bindings
  - HTTP triggers
  - Authorization levels
  
- [x] **Isolated Worker Process Model**
  - .NET 8 support
  - `HttpRequestData` vs `HttpRequest`
  - Manual JSON deserialization

- [x] **.NET 8 Features**
  - C# 12 syntax
  - Modern project structure
  - Dependency injection

- [x] **RESTful API Design**
  - CRUD operations
  - Route parameters
  - HTTP methods (GET, POST, PUT, DELETE)

- [x] **Validation**
- DataAnnotations (attribute-based)
- FluentValidation (code-based alternative)
- Custom error messages
- Manual validation in functions
- Async validation patterns

- [x] **Configuration**
  - `host.json` for runtime settings
  - `local.settings.json` for app settings
  - Environment-specific configuration

- [x] **Deployment**
  - Azure CLI commands
  - PowerShell automation
  - Resource naming conventions
  - Environment strategies (dev/test/prod)

## Important Notes for Students

### This is Example Code

- **Purpose:** Educational demonstration of Azure Functions concepts
- **Not Production-Ready:** Missing database persistence, authentication, error handling, and tests
- **Learning Focus:** Understanding serverless architecture and Azure Functions basics

### TODO Items in Code

Throughout the code, you'll see comments like:
```csharp
// TODO: persist customer (e.g., EF Core) and return created resource
```

These indicate areas where students can expand the functionality:
1. Add Entity Framework Core for database access
2. Implement proper authentication (Azure AD, JWT)
3. Add comprehensive error handling
4. Write unit and integration tests
5. Implement proper logging and monitoring

### Best Practices Not Yet Implemented

This example prioritizes **learning fundamentals** over production patterns. In a real project, you would add:
- Repository pattern for data access
- Service layer for business logic
- Proper exception handling and logging
- Unit tests and integration tests
- CI/CD pipelines
- Security best practices (Key Vault, Managed Identity)

## Next Steps After Module 7

Once you understand the basics from this example:

1. **Add Database Persistence**
   - Install `Microsoft.EntityFrameworkCore.SqlServer`
   - Create a `DbContext`
   - Implement repositories

2. **Implement Authentication**
   - Azure AD integration
   - JWT token validation
   - Function-level authorization

3. **Add Testing**
   - Unit tests for validation logic
   - Integration tests for functions
   - Use `Microsoft.Azure.Functions.Worker.TestFunctions`

4. **Set Up CI/CD**
   - GitHub Actions workflow
   - Automated testing
   - Staged deployments

5. **Explore Other Triggers**
   - Timer triggers for scheduled jobs
   - Queue triggers for async processing
   - Blob triggers for file processing

## Resources for Students

### Official Documentation
- [Azure Functions Developer Guide](https://learn.microsoft.com/azure/azure-functions/functions-reference)
- [Azure Functions .NET Isolated Worker](https://learn.microsoft.com/azure/azure-functions/dotnet-isolated-process-guide)
- [.NET 8 Documentation](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-8)

### Tutorials
- [Create your first function in Azure](https://learn.microsoft.com/azure/azure-functions/create-first-function-vs-code-csharp)
- [Azure Functions HTTP trigger](https://learn.microsoft.com/azure/azure-functions/functions-bindings-http-webhook-trigger)
- [Local development with Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local)

### Related Topics
- [Data Annotations in .NET](https://learn.microsoft.com/dotnet/api/system.componentmodel.dataannotations)
- [Azure CLI Reference](https://learn.microsoft.com/cli/azure/functionapp)
- [PowerShell Scripting Basics](https://learn.microsoft.com/powershell/scripting/overview)

## Support

For questions or issues:
1. Check the troubleshooting sections in [`instructions.md`](instructions.md)
2. Review Azure Functions documentation
3. Ask your instructor or teaching assistant
4. Check Azure Portal logs for deployed functions

