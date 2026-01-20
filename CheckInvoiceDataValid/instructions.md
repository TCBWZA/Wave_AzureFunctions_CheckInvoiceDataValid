# Invoice Functions - Setup and Deployment Instructions

## Overview
This project contains Azure Functions (isolated worker process model) for managing invoices with validation capabilities using .NET 8.

## Prerequisites

### Required Tools
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure Functions Core Tools v4](https://learn.microsoft.com/azure/azure-functions/functions-run-local)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (recommended) or [VS Code with Azure Functions extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions)
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) (for Azure deployment)

### Verify Installation
```powershell
# Check .NET version
dotnet --version

# Check Azure Functions Core Tools
func --version

# Check Azure CLI
az --version
```

### Azure Resources (for deployment)
- Active Azure subscription
- Azure Function App (isolated worker process, .NET 8)

## Project Structure

```
CheckInvoiceDataValid/
+-- CheckInvoiceDataValid/
    +-- CustomerFunctions.cs             # Customer CRUD functions
    +-- CustomerFunctionsWithFluentValidation.cs  # Customer CRUD with FluentValidation
    +-- Validators/                      # FluentValidation validators
        +-- CreateCustomerValidator.cs
        +-- UpdateCustomerValidator.cs
    +-- DTOs/
        +-- CustomerDto.cs               # Customer DTOs with Create/Update variants
        +-- InvoiceDto.cs                # Invoice DTOs with Create/Update variants
        +-- TelephoneNumberDto.cs        # TelephoneNumber DTOs with Create/Update variants
    +-- CheckInvoiceDataValid.csproj
    +-- host.json                        # Function host configuration
    +-- local.settings.json              # Local development settings (gitignored)
    +-- Program.cs                       # Entry point and service configuration
+-- CheckInvoiceDataValid.slnx           # Solution file
+-- SampleRolloutScript.txt              # Deployment script template
+-- test-validation-approaches.txt       # Test script template
+-- instructions.md                      # This file
```

## Running Locally

### Option 1: Visual Studio 2022 (Recommended)

1. **Open the solution**
   - Open `CheckInvoiceDataValid.sln` in Visual Studio 2022

2. **Configure local settings**
   - Ensure `local.settings.json` exists in the `CheckInvoiceDataValid` project folder
   - If it doesn't exist, create it (see Configuration section below)

3. **Set the startup project**
   - Right-click the `CheckInvoiceDataValid` project in Solution Explorer
   - Select **Set as Startup Project**

4. **Start debugging**
   - Press **F5** or click **Debug > Start Debugging**
   - The Azure Functions runtime will start in a console window
   - Endpoint URLs will be displayed in the console output

5. **Expected output**
```
Azure Functions Core Tools
Core Tools Version:       4.x.xxxx
Function Runtime Version: 4.x.xxxxx
   
Functions:
   
CreateCustomer: [POST] http://localhost:7071/api/customers
UpdateCustomer: [PUT] http://localhost:7071/api/customers/{id:long}
GetCustomer: [GET] http://localhost:7071/api/customers/{id:long}
GetAllCustomers: [GET] http://localhost:7071/api/customers
DeleteCustomer: [DELETE] http://localhost:7071/api/customers/{id:long}
   
CreateCustomerFluentValidation: [POST] http://localhost:7071/api/customers-fluent
UpdateCustomerFluentValidation: [PUT] http://localhost:7071/api/customers-fluent/{id:long}
```

### Option 2: Visual Studio Code

1. **Open the workspace folder**
   ```powershell
   cd D:\Users\tbw_\source\repos\CheckInvoiceDataValid
   code .
   ```

2. **Install Azure Functions extension**
   - Install the Azure Functions extension for VS Code

3. **Configure local settings**
   - Ensure `local.settings.json` exists in the project folder

4. **Start the function**
   - Press **F5** or use the command palette: **Azure Functions: Start Debugging**

### Option 3: Command Line

1. **Navigate to the project directory**
   ```powershell
   cd D:\Users\tbw_\source\repos\CheckInvoiceDataValid\CheckInvoiceDataValid
   ```

2. **Restore dependencies**
   ```powershell
   dotnet restore
   ```

3. **Build the project**
   ```powershell
   dotnet build
   ```

4. **Start the Azure Functions host**
   ```powershell
   func start
   ```
   
   Or use the dotnet CLI:
   ```powershell
   dotnet run
   ```

5. **Functions will be available at**
   - `http://localhost:7071/api/invoices` (POST - CreateInvoice)
   - `http://localhost:7071/api/invoices/{id}` (PUT - UpdateInvoice)

## Configuration

### local.settings.json

Create a `local.settings.json` file in the `CheckInvoiceDataValid` project directory:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  }
}
```

**Note**: This file is excluded from source control by default. Each developer needs to create their own.

### Optional: Add Database Connection (for future EF Core implementation)

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=InvoiceDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

## Testing Locally

### Using PowerShell

#### Create Invoice
```powershell
$body = @{
    customerName = "Acme Corporation"
    amount = 1500.00
    dueDate = "2026-02-15"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:7071/api/invoices" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

#### Update Invoice
```powershell
$body = @{
    customerName = "Acme Corp Updated"
    amount = 2000.00
    dueDate = "2026-03-01"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:7071/api/invoices/123" `
    -Method PUT `
    -Body $body `
    -ContentType "application/json"
```

### Using Windows Command Prompt with curl

#### Create Invoice
```cmd
curl -X POST http://localhost:7071/api/invoices ^
  -H "Content-Type: application/json" ^
  -d "{\"customerName\": \"Acme Corporation\", \"amount\": 1500.00, \"dueDate\": \"2026-02-15\"}"
```

#### Update Invoice
```cmd
curl -X PUT http://localhost:7071/api/invoices/123 ^
  -H "Content-Type: application/json" ^
  -d "{\"customerName\": \"Acme Corp Updated\", \"amount\": 2000.00, \"dueDate\": \"2026-03-01\"}"
```

### Using Postman or Insomnia

1. **Create Invoice**
   - Method: `POST`
   - URL: `http://localhost:7071/api/invoices`
   - Headers: `Content-Type: application/json`
   - Body (raw JSON):
     ```json
     {
       "customerName": "Acme Corporation",
       "amount": 1500.00,
       "dueDate": "2026-02-15"
     }
     ```

2. **Update Invoice**
   - Method: `PUT`
   - URL: `http://localhost:7071/api/invoices/123`
   - Headers: `Content-Type: application/json`
   - Body (raw JSON):
     ```json
     {
       "customerName": "Acme Corp Updated",
       "amount": 2000.00,
       "dueDate": "2026-03-01"
     }
     ```

### Expected Responses

**Success Response:**
```json
{
  "message": "CreateInvoice validation passed.",
  "invoice": {
    "customerName": "Acme Corporation",
    "amount": 1500.00,
    "dueDate": "2026-02-15"
  }
}
```

**Validation Error Response:**
```json
{
  "errors": {
    "CustomerName": ["The CustomerName field is required."],
    "Amount": ["The Amount field must be greater than 0."]
  }
}
```

## Deploying to Azure

### Step 1: Login to Azure

```powershell
az login
```

### Step 2: Create Azure Resources

```powershell
# Set variables (customize these)
$RESOURCE_GROUP = "rg-invoice-functions"
$LOCATION = "eastus"
$STORAGE_ACCOUNT = "stinvoicefunc$(Get-Random -Maximum 9999)"
$FUNCTION_APP = "func-invoice-api-$(Get-Random -Maximum 9999)"

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create storage account
az storage account create `
  --name $STORAGE_ACCOUNT `
  --resource-group $RESOURCE_GROUP `
  --location $LOCATION `
  --sku Standard_LRS

# Create Function App (isolated worker, .NET 8)
az functionapp create `
  --name $FUNCTION_APP `
  --resource-group $RESOURCE_GROUP `
  --storage-account $STORAGE_ACCOUNT `
  --consumption-plan-location $LOCATION `
  --runtime dotnet-isolated `
  --runtime-version 8 `
  --functions-version 4 `
  --os-type Windows
```

**For Linux:**
```powershell
az functionapp create `
  --name $FUNCTION_APP `
  --resource-group $RESOURCE_GROUP `
  --storage-account $STORAGE_ACCOUNT `
  --consumption-plan-location $LOCATION `
  --runtime dotnet-isolated `
  --runtime-version 8 `
  --functions-version 4 `
  --os-type Linux
```

### Step 3: Deploy from Visual Studio

1. **Right-click the project**
   - In Solution Explorer, right-click `CheckInvoiceDataValid` project
   - Select **Publish...**

2. **Configure publish target**
   - Target: **Azure**
   - Specific target: **Azure Function App (Windows)** or **(Linux)**
   - Click **Next**

3. **Select or create Function App**
   - Sign in to your Azure account
   - Select your subscription
   - Choose your existing Function App (created in Step 2)
   - Or click **Create new** to create one through the wizard
   - Click **Finish**

4. **Publish**
   - Click **Publish** button
   - Wait for deployment to complete
   - The output window will show deployment progress

### Step 4: Deploy from Command Line

```powershell
# Navigate to project directory
cd D:\Users\tbw_\source\repos\CheckInvoiceDataValid\CheckInvoiceDataValid

# Build in Release mode
dotnet build --configuration Release

# Publish the project
dotnet publish --configuration Release --output ./publish

# Deploy to Azure using Azure Functions Core Tools
func azure functionapp publish $FUNCTION_APP
```

### Step 5: Verify Deployment

```powershell
# List functions in the deployed app
az functionapp function list --name $FUNCTION_APP --resource-group $RESOURCE_GROUP

# Get function app URL
az functionapp show --name $FUNCTION_APP --resource-group $RESOURCE_GROUP --query "defaultHostName" -o tsv
```

## Azure Configuration

### Get Function Key for Authentication

#### Using Azure Portal:
1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to your Function App
3. Go to **Functions > CreateInvoice** (or any function)
4. Click **Function Keys**
5. Copy the **default** key value

#### Using Azure CLI:
```powershell
# Get function keys
az functionapp function keys list `
  --name $FUNCTION_APP `
  --resource-group $RESOURCE_GROUP `
  --function-name CreateInvoice
```

### Configure Application Settings

Set environment-specific settings in the Azure Portal:

1. Navigate to your Function App in Azure Portal
2. Go to **Settings > Configuration**
3. Under **Application settings**, add/verify:
   - `FUNCTIONS_WORKER_RUNTIME`: `dotnet-isolated`
   - Add any connection strings or app-specific settings

Or use Azure CLI:
```powershell
az functionapp config appsettings set `
  --name $FUNCTION_APP `
  --resource-group $RESOURCE_GROUP `
  --settings "MyCustomSetting=MyValue"
```

## Testing in Azure

Replace placeholders with your actual values:
- `<FUNCTION_APP>`: Your Function App name
- `<FUNCTION_KEY>`: Your function key (from Step 5)

### Using PowerShell

```powershell
$functionApp = "<FUNCTION_APP>"
$functionKey = "<FUNCTION_KEY>"

# Create Invoice
$body = @{
    customerName = "Acme Corporation"
    amount = 1500.00
    dueDate = "2026-02-15"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://$functionApp.azurewebsites.net/api/invoices?code=$functionKey" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"

# Update Invoice
$body = @{
    customerName = "Acme Corp Updated"
    amount = 2000.00
    dueDate = "2026-03-01"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://$functionApp.azurewebsites.net/api/invoices/123?code=$functionKey" `
    -Method PUT `
    -Body $body `
    -ContentType "application/json"
```

### Using Windows Command Prompt with curl

```cmd
# Create Invoice
curl -X POST "https://<FUNCTION_APP>.azurewebsites.net/api/invoices?code=<FUNCTION_KEY>" ^
  -H "Content-Type: application/json" ^
  -d "{\"customerName\": \"Acme Corporation\", \"amount\": 1500.00, \"dueDate\": \"2026-02-15\"}"

# Update Invoice
curl -X PUT "https://<FUNCTION_APP>.azurewebsites.net/api/invoices/123?code=<FUNCTION_KEY>" ^
  -H "Content-Type: application/json" ^
  -d "{\"customerName\": \"Acme Corp Updated\", \"amount\": 2000.00, \"dueDate\": \"2026-03-01\"}"
```

## Monitoring and Debugging

### View Logs in Azure Portal

1. Navigate to your Function App in Azure Portal
2. Go to **Functions > Monitor** for specific function execution history
3. Use **Log stream** under **Monitoring** for real-time logs
4. Access **Application Insights** for detailed telemetry

### Enable Application Insights (Recommended)

```powershell
# Create Application Insights
az monitor app-insights component create `
  --app $FUNCTION_APP-insights `
  --location $LOCATION `
  --resource-group $RESOURCE_GROUP

# Get instrumentation key
$INSTRUMENTATION_KEY = az monitor app-insights component show `
  --app $FUNCTION_APP-insights `
  --resource-group $RESOURCE_GROUP `
  --query "instrumentationKey" -o tsv

# Configure Function App to use Application Insights
az functionapp config appsettings set `
  --name $FUNCTION_APP `
  --resource-group $RESOURCE_GROUP `
  --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$INSTRUMENTATION_KEY"
```

### View Logs Locally

When running locally, logs appear in:
- Visual Studio: **Output** window (select "Azure Functions" from dropdown)
- VS Code: **Terminal** panel
- Command line: Console output

## Troubleshooting

### Common Local Issues

**Issue**: "No job functions found"
- **Solution**: Ensure the project builds successfully
- Verify `[Function]` attributes are present on methods
- Check that the project references `Microsoft.Azure.Functions.Worker.Sdk`

**Issue**: Storage emulator errors
- **Solution**: 
  - Install [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite)
  - Or use a real Azure Storage connection string in `local.settings.json`
  ```json
  "AzureWebJobsStorage": "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net"
  ```

**Issue**: Port 7071 already in use
- **Solution**: 
  - Stop other Azure Functions instances
  - Or change the port in `local.settings.json`:
  ```json
  "Host": {
    "LocalHttpPort": 7072
  }
  ```

### Common Azure Deployment Issues

**Issue**: Functions not appearing after deployment
- **Solution**: 
  - Verify `FUNCTIONS_WORKER_RUNTIME` is set to `dotnet-isolated`
  - Ensure runtime version is set to .NET 8
  - Check deployment logs in Azure Portal

**Issue**: 500 errors in Azure
- **Solution**: 
  - Check Application Insights or Log Stream for detailed errors
  - Verify all required configuration settings are set
  - Ensure dependencies are correctly deployed

**Issue**: Authentication errors (401)
- **Solution**: 
  - Verify the function key is correct
  - Check that `AuthorizationLevel` is set appropriately
  - For testing, temporarily change to `AuthorizationLevel.Anonymous`

## API Documentation

### CreateInvoice

**Endpoint**: `POST /api/invoices`

**Request Body**:
```json
{
  "customerName": "string (required)",
  "amount": "number (required, > 0)",
  "dueDate": "date (required)"
}
```

**Response**: 200 OK
```json
{
  "message": "CreateInvoice validation passed.",
  "invoice": { ... }
}
```

### UpdateInvoice

**Endpoint**: `PUT /api/invoices/{id}`

**Path Parameters**:
- `id`: Invoice ID (long/integer)

**Request Body**:
```json
{
  "customerName": "string (required)",
  "amount": "number (required, > 0)",
  "dueDate": "date (required)"
}
```

**Response**: 200 OK
```json
{
  "message": "UpdateInvoice validation passed.",
  "id": 123,
  "invoice": { ... }
}
```

## Resources

- [Azure Functions .NET Isolated Worker Documentation](https://learn.microsoft.com/azure/azure-functions/dotnet-isolated-process-guide)
- [Azure Functions Best Practices](https://learn.microsoft.com/azure/azure-functions/functions-best-practices)
- [Azure Functions HTTP Trigger](https://learn.microsoft.com/azure/azure-functions/functions-bindings-http-webhook-trigger)
- [.NET 8 Documentation](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-8)
- [Data Annotations in .NET](https://learn.microsoft.com/dotnet/api/system.componentmodel.dataannotations)

## Support

For issues or questions:
- Check Azure Functions documentation
- Review Application Insights logs
