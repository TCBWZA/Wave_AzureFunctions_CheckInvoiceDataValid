# FluentValidation in Azure Functions - Student Guide

## Overview

This project demonstrates **two validation approaches** for Azure Functions:

1. **DataAnnotations** (used in `CustomerFunctions.cs`) - Simple, attribute-based validation
2. **FluentValidation** (used in `CustomerFunctionsWithFluentValidation.cs`) - Advanced, code-based validation

Both approaches achieve the same goal but with different strengths and use cases.

---

## What is FluentValidation?

FluentValidation is a popular .NET library for building strongly-typed validation rules using a fluent interface. Instead of decorating your classes with attributes, you write validation logic in separate validator classes.

### Key Benefits

- **Separation of Concerns**: Validation logic is separate from DTOs
- **More Powerful**: Complex conditional logic, custom validators, async validation
- **Better Testing**: Validators can be unit tested independently
- **Reusability**: Validators can inherit and compose rules
- **Clear Error Messages**: Fine-grained control over error messages

---

## DataAnnotations vs FluentValidation

### DataAnnotations Example (Current Implementation)

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

**Pros:**
- Simple and declarative
- Built into .NET
- Good for basic validation
- Easy to understand for beginners

**Cons:**
- Limited to what attributes support
- Hard to do complex conditional logic
- Tightly couples validation to DTOs
- Difficult to test in isolation

### FluentValidation Example (New Implementation)

```csharp
public class CreateCustomerValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(200)
            .WithMessage("Name cannot exceed 200 characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email address format.");
    }
}
```

**Pros:**
- Very flexible and powerful
- Easy to write complex conditional logic
- Separate validation concerns from models
- Easier to unit test
- Better code reuse through inheritance

**Cons:**
- Requires additional NuGet package
- Slightly more verbose
- Requires dependency injection setup
- Learning curve for beginners

---

## Implementation in This Project

### 1. Validators (`Validators/` folder)

#### CreateCustomerValidator.cs
```csharp
public class CreateCustomerValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerValidator()
    {
        // Basic validation rules
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(200)
            .WithMessage("Name cannot exceed 200 characters.")
            .MinimumLength(2)
            .WithMessage("Name must be at least 2 characters long.");

        // Advanced: Custom validation logic
        RuleFor(x => x.Name)
            .Matches(@"^[a-zA-Z\s\-\.]+$")
            .WithMessage("Name can only contain letters, spaces, hyphens, and periods.")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        // Advanced: Custom validator method
        RuleFor(x => x.Email)
            .Must(email => !IsTemporaryEmail(email))
            .WithMessage("Temporary email addresses are not allowed.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }

    private bool IsTemporaryEmail(string email)
    {
        var tempDomains = new[] { "tempmail.com", "throwaway.email" };
        var domain = email.Split('@').LastOrDefault()?.ToLower();
        return domain != null && tempDomains.Contains(domain);
    }
}
```

**Learning Points:**
- Each validation rule is a method call
- `.WithMessage()` customizes error messages
- `.When()` adds conditional logic
- `.Must()` allows custom validation functions

### 2. Dependency Injection (`Program.cs`)

```csharp
// Register FluentValidation validators
builder.Services.AddScoped<IValidator<CreateCustomerDto>, CreateCustomerValidator>();
builder.Services.AddScoped<IValidator<UpdateCustomerDto>, UpdateCustomerValidator>();
```

**Why Scoped?**
- Validators are created per request
- Thread-safe for Azure Functions
- Efficient memory usage

### 3. Using in Functions (`CustomerFunctionsWithFluentValidation.cs`)

```csharp
public class CustomerFunctionsWithFluentValidation
{
    private readonly IValidator<CreateCustomerDto> _createCustomerValidator;

    // Inject validator through constructor
    public CustomerFunctionsWithFluentValidation(
        IValidator<CreateCustomerDto> createCustomerValidator)
    {
        _createCustomerValidator = createCustomerValidator;
    }

    [Function("CreateCustomerFluentValidation")]
    public async Task<IActionResult> CreateCustomer(...)
    {
        // Validate using FluentValidation
        var validationResult = await _createCustomerValidator.ValidateAsync(dto);
        
        if (!validationResult.IsValid)
        {
            // Convert to dictionary format for consistent API response
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            return new BadRequestObjectResult(new { errors });
        }

        // Validation passed, continue with business logic
        return new OkObjectResult(new { message = "Success", customer = dto });
    }
}
```

---

## Testing Both Approaches

### Test DataAnnotations Version (Original)

```powershell
# Valid customer
$body = @{
    name = "John Doe"
    email = "john.doe@example.com"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:7071/api/customers" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

### Test FluentValidation Version (New)

```powershell
# Valid customer (using FluentValidation endpoint)
$body = @{
    name = "Jane Smith"
    email = "jane.smith@example.com"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:7071/api/customers-fluent" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

**Notice:** The route is different (`customers-fluent`) to avoid conflicts.

### Test Validation Errors

```powershell
# Test invalid data
$body = @{
    name = "A"  # Too short (< 2 characters)
    email = "invalid-email"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:7071/api/customers-fluent" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

**Expected Response:**
```json
{
  "errors": {
    "Name": [
      "Name must be at least 2 characters long."
    ],
    "Email": [
      "Invalid email address format."
    ]
  }
}
```

### Test Advanced Validation

```powershell
# Test temporary email blocking (FluentValidation only)
$body = @{
    name = "Test User"
    email = "test@tempmail.com"  # Blocked by FluentValidation
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:7071/api/customers-fluent" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

**Expected Response:**
```json
{
  "errors": {
    "Email": [
      "Temporary email addresses are not allowed."
    ]
  }
}
```

---

## Advanced FluentValidation Features

### 1. Conditional Validation

```csharp
RuleFor(x => x.Name)
    .NotEqual(x => x.Email.Split('@')[0])
    .WithMessage("Name should not be the same as email username.")
    .When(x => !string.IsNullOrWhiteSpace(x.Email) && x.Email.Contains('@'));
```

**Explanation:** Only validates if email is not empty and contains '@'.

### 2. Custom Validators

```csharp
RuleFor(x => x.Email)
    .Must(email => !IsTemporaryEmail(email))
    .WithMessage("Temporary email addresses are not allowed.");

private bool IsTemporaryEmail(string email)
{
    var tempDomains = new[] { "tempmail.com", "throwaway.email" };
    var domain = email.Split('@').LastOrDefault()?.ToLower();
    return domain != null && tempDomains.Contains(domain);
}
```

**Explanation:** Uses a custom method to implement complex logic.

### 3. Async Validation (Example)

```csharp
public class CreateCustomerValidator : AbstractValidator<CreateCustomerDto>
{
    private readonly ICustomerRepository _repository;

    public CreateCustomerValidator(ICustomerRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.Email)
            .MustAsync(async (email, cancellation) => 
            {
                var exists = await _repository.EmailExistsAsync(email);
                return !exists;
            })
            .WithMessage("Email address is already in use.");
    }
}
```

**Explanation:** Validates against database (requires repository injection).

### 4. Rule Sets

```csharp
public class CreateCustomerValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerValidator()
    {
        // Default rules
        RuleFor(x => x.Name).NotEmpty();
        
        // Additional rules for "Strict" rule set
        RuleSet("Strict", () =>
        {
            RuleFor(x => x.Name)
                .MinimumLength(5)
                .WithMessage("In strict mode, name must be at least 5 characters.");
        });
    }
}

// Usage in function
var validationResult = await _validator.ValidateAsync(dto, options => 
{
    options.IncludeRuleSets("Strict");
});
```

### 5. Inheritance and Composition

```csharp
public class BaseCustomerValidator<T> : AbstractValidator<T> where T : class
{
    protected void SetupEmailRules(Func<T, string> emailSelector)
    {
        RuleFor(emailSelector)
            .NotEmpty()
            .EmailAddress();
    }
}

public class CreateCustomerValidator : BaseCustomerValidator<CreateCustomerDto>
{
    public CreateCustomerValidator()
    {
        SetupEmailRules(x => x.Email);
        // Add more rules specific to Create
    }
}
```

---

## When to Use Which Approach?

### Use DataAnnotations When:
- Simple validation rules (required, length, range)
- Quick prototyping or learning basics
- Validation logic is straightforward
- You want minimal dependencies
- Team is more familiar with attributes

### Use FluentValidation When:
- Complex conditional validation
- Cross-property validation
- Need to validate against database/external services
- Want to unit test validation logic
- Building enterprise applications with complex rules
- Need better code organization and reusability

---

## Student Exercises

### Exercise 1: Add FluentValidation to Invoice Functions
**Objective:** Practice creating FluentValidation validators

**Task:** 
1. Create `InvoiceValidator.cs` in the `Validators/` folder
2. Add validation rules for `CreateInvoiceDto`:
   - InvoiceNumber: Required, max 50 characters
   - InvoiceDate: Required, not in future
   - DueDate: Required, must be after InvoiceDate
   - CustomerId: Required, > 0
   - TotalAmount: Required, >= 0

**Hints:**
```csharp
RuleFor(x => x.DueDate)
    .GreaterThan(x => x.InvoiceDate)
    .WithMessage("Due date must be after invoice date.");
```

### Exercise 2: Add Async Email Uniqueness Check
**Objective:** Learn async validation

**Task:** Add a rule that checks if email already exists (simulate with in-memory list)

**Hints:**
```csharp
RuleFor(x => x.Email)
    .MustAsync(async (email, cancellation) =>
    {
        await Task.Delay(10); // Simulate DB call
        return !_existingEmails.Contains(email);
    })
    .WithMessage("Email already exists.");
```

### Exercise 3: Compare Validation Performance
**Objective:** Understand performance implications

**Task:** 
1. Create 1000 test customers
2. Validate using DataAnnotations
3. Validate using FluentValidation
4. Compare execution time

---

## Common Mistakes and Solutions

### Mistake 1: Forgetting to Register Validators

**Wrong:**
```csharp
// Program.cs - Validator not registered
builder.Build().Run();
```

**Correct:**
```csharp
builder.Services.AddScoped<IValidator<CreateCustomerDto>, CreateCustomerValidator>();
```

### Mistake 2: Not Handling Validation Results

**Wrong:**
```csharp
var validationResult = await _validator.ValidateAsync(dto);
// Continue without checking validationResult.IsValid
```

**Correct:**
```csharp
var validationResult = await _validator.ValidateAsync(dto);
if (!validationResult.IsValid)
{
    return new BadRequestObjectResult(new { errors = ... });
}
```

### Mistake 3: Using Wrong Validator Lifetime

**Wrong:**
```csharp
builder.Services.AddSingleton<IValidator<CreateCustomerDto>, CreateCustomerValidator>();
// Singleton can cause issues if validator has state
```

**Correct:**
```csharp
builder.Services.AddScoped<IValidator<CreateCustomerDto>, CreateCustomerValidator>();
// Scoped is safe for stateless validators
```

---

## Resources

### Official Documentation
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [Built-in Validators](https://docs.fluentvalidation.net/en/latest/built-in-validators.html)
- [Custom Validators](https://docs.fluentvalidation.net/en/latest/custom-validators.html)

### Comparison Articles
- [DataAnnotations vs FluentValidation](https://docs.fluentvalidation.net/en/latest/aspnet.html)
- [When to use which approach](https://stackoverflow.com/questions/5518057/fluentvalidation-vs-data-annotations)

---

## Summary

| Feature | DataAnnotations | FluentValidation |
|---------|----------------|------------------|
| **Setup** | No setup needed | Requires DI registration |
| **Syntax** | Attributes on properties | Fluent API in validator class |
| **Complexity** | Simple rules only | Complex conditional logic |
| **Testing** | Hard to test in isolation | Easy to unit test |
| **Reusability** | Limited | Excellent (inheritance, composition) |
| **Async Support** | No | Yes |
| **Performance** | Slightly faster | Slightly slower (negligible) |
| **Learning Curve** | Easy | Moderate |
| **Best For** | Simple apps, prototypes | Enterprise apps, complex rules |

**Recommendation:** Start with DataAnnotations for learning, move to FluentValidation as requirements grow more complex.

---

**Module 7 Learning Goal:** Understand both validation approaches and know when to use each one! ??
