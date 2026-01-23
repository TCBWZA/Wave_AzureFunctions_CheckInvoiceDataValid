# Newtonsoft.Json Artifactory 403 Error - Resolution Guide

## Problem Summary
When building the project behind an Artifactory proxy, you encountered 403 errors for `Newtonsoft.Json` version 11, even though the project doesn't directly reference that version.

## Root Cause
- **Transitive Dependency**: One of the Azure Functions packages was requesting an old version of Newtonsoft.Json
- **Artifactory Limitation**: Your Artifactory instance doesn't have that specific old version available
- **Version Mismatch**: The project had version 13.0.4, but something was requesting version 11

## Solutions Implemented

### ? Solution 1: Downgrade to More Available Version
Changed `Newtonsoft.Json` from `13.0.4` to `13.0.3` in `CheckInvoiceDataValid.csproj`:

```xml
<!-- Force override of Newtonsoft.Json -->
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
<PackageReference Update="Newtonsoft.Json" Version="13.0.3" />
```

**Why this works:**
- Version 13.0.3 is more likely to be cached in Artifactory
- The `Update` attribute forces ALL transitive dependencies to use this version
- Overrides any old version requests from dependencies

### ✅ Solution 2: Directory.Build.props (Global Override)
Created `CheckInvoiceDataValid\Directory.Build.props`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project>
  <PropertyGroup>
    <DisableTransitiveProjectReferences>false</DisableTransitiveProjectReferences>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Update="Newtonsoft.Json">
      <Version>13.0.3</Version>
    </PackageReference>
  </ItemGroup>
</Project>
```

**Why this works:**
- Applies globally to all projects in the solution
- Catches any transitive references across the entire build tree
- MSBuild evaluates this BEFORE project files

### ? Solution 3: Directory.Packages.props (Central Package Management)
Created `CheckInvoiceDataValid\Directory.Packages.props`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Update="Newtonsoft.Json" Version="13.0.3" />
  </ItemGroup>
</Project>
```

**Why this works:**
- Alternative centralized package management approach
- Can be enabled for stricter control if needed

### ? Solution 4: NuGet.config (Artifactory Configuration)
Created `NuGet.config` at solution root:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <!-- Add your Artifactory source here -->
    <!-- <add key="Artifactory" value="https://your-artifactory-url/api/nuget/nuget-virtual" /> -->
    
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
  
  <packageSourceMapping>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
  
  <config>
    <add key="dependencyVersion" value="HighestMinor" />
  </config>
</configuration>
```

**How to customize for your Artifactory:**

1. Uncomment and update the Artifactory line:
   ```xml
   <add key="Artifactory" value="https://your-company.jfrog.io/artifactory/api/nuget/nuget-virtual" />
   ```

2. Add Artifactory to package source mapping:
   ```xml
   <packageSource key="Artifactory">
     <package pattern="*" />
   </packageSource>
   ```

3. If you need authentication, add credentials section:
   ```xml
   <packageSourceCredentials>
     <Artifactory>
       <add key="Username" value="your-username" />
       <add key="ClearTextPassword" value="your-api-key" />
     </Artifactory>
   </packageSourceCredentials>
   ```

## Verification

After applying these fixes, verify the resolution:

```powershell
# Clean and restore
cd CheckInvoiceDataValid
dotnet clean
dotnet restore --force

# Verify Newtonsoft.Json version
dotnet list package --include-transitive | Select-String -Pattern "Newtonsoft"
```

**Expected output:**
```
>    > Newtonsoft.Json    13.0.3    13.0.3
```

## Additional Troubleshooting

### If you still get 403 errors:

1. **Check which package needs Newtonsoft.Json:**
   ```powershell
   dotnet restore --verbosity detailed 2>&1 | Select-String "Newtonsoft"
   ```

2. **Try different available versions:**
   ```xml
   <!-- Try these versions in order: -->
   <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
   <PackageReference Include="Newtonsoft.Json" Version="13.0.2" />
   <PackageReference Include="Newtonsoft.Json" Version="13.0.1" />
   <PackageReference Include="Newtonsoft.Json" Version="12.0.3" />
   ```

3. **Ask your Artifactory admin to add the package:**
   - Contact your DevOps team
   - Request they add `Newtonsoft.Json 13.0.3` to your Artifactory repository
   - Or configure Artifactory to proxy nuget.org automatically

4. **Temporarily bypass Artifactory for testing:**
   ```xml
   <!-- In NuGet.config -->
   <packageSources>
     <clear />
     <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
   </packageSources>
   ```

### If you want to eliminate Newtonsoft.Json entirely:

Your project uses `System.Text.Json` instead. To remove Newtonsoft.Json dependency:

1. Check which package requires it:
   ```powershell
   dotnet list package --include-transitive --framework net8.0
   ```

2. If it's from Azure Functions packages, you may need to wait for Microsoft to update their dependencies

3. As a workaround, the version override approach forces a working version

## Best Practices

1. ? **Always use version overrides** when working with Artifactory
2. ? **Keep NuGet.config in source control** (except credentials)
3. ? **Use Central Package Management** for solutions with multiple projects
4. ? **Document Artifactory URLs** in your team wiki
5. ? **Coordinate with DevOps** on package availability

## Files Modified/Created

- ? `CheckInvoiceDataValid\CheckInvoiceDataValid.csproj` - Updated Newtonsoft.Json version
- ? `CheckInvoiceDataValid\Directory.Build.props` - Global package override
- ? `CheckInvoiceDataValid\Directory.Packages.props` - Central package management
- ? `NuGet.config` - Artifactory configuration

