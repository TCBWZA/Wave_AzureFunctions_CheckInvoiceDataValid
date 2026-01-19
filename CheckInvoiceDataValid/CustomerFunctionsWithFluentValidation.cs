using EfCoreLab.DTOs;
using EfCoreLab.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EfCoreLab.Functions
{
    /// <summary>
    /// Alternative implementation of CustomerFunctions using FluentValidation.
    /// This demonstrates a different validation approach compared to DataAnnotations.
    /// 
    /// COMPARISON FOR STUDENTS:
    /// - DataAnnotations: Declarative, attribute-based, simpler for basic scenarios
    /// - FluentValidation: Code-based, more flexible, better for complex rules
    /// </summary>
    public class CustomerFunctionsWithFluentValidation
    {
        private readonly ILogger<CustomerFunctionsWithFluentValidation> _logger;
        private readonly IValidator<CreateCustomerDto> _createCustomerValidator;
        private readonly IValidator<UpdateCustomerDto> _updateCustomerValidator;

        // Constructor with dependency injection for validators
        public CustomerFunctionsWithFluentValidation(
            ILogger<CustomerFunctionsWithFluentValidation> logger,
            IValidator<CreateCustomerDto> createCustomerValidator,
            IValidator<UpdateCustomerDto> updateCustomerValidator)
        {
            _logger = logger;
            _createCustomerValidator = createCustomerValidator;
            _updateCustomerValidator = updateCustomerValidator;
        }

        [Function("CreateCustomerFluentValidation")]
        public async Task<IActionResult> CreateCustomer(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "customers-fluent")] HttpRequestData req)
        {
            _logger.LogInformation("CreateCustomer (FluentValidation) called.");

            // Deserialize request body
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

            // FluentValidation approach
            var validationResult = await _createCustomerValidator.ValidateAsync(dto);
            
            if (!validationResult.IsValid)
            {
                // Convert FluentValidation errors to a dictionary format
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                return new BadRequestObjectResult(new { errors });
            }

            // TODO: persist customer (e.g., EF Core) and return created resource
            return new OkObjectResult(new 
            { 
                message = "CreateCustomer validation passed (using FluentValidation).", 
                customer = dto 
            });
        }

        [Function("UpdateCustomerFluentValidation")]
        public async Task<IActionResult> UpdateCustomer(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "customers-fluent/{id:long}")] HttpRequestData req,
            long id)
        {
            _logger.LogInformation("UpdateCustomer (FluentValidation) called for id {Id}.", id);

            // Deserialize request body
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            UpdateCustomerDto? dto;
            try
            {
                dto = JsonSerializer.Deserialize<UpdateCustomerDto>(body, new JsonSerializerOptions
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

            // FluentValidation approach
            var validationResult = await _updateCustomerValidator.ValidateAsync(dto);
            
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                return new BadRequestObjectResult(new { errors });
            }

            // TODO: load existing customer by id, apply updates, save changes
            return new OkObjectResult(new 
            { 
                message = "UpdateCustomer validation passed (using FluentValidation).", 
                id, 
                customer = dto 
            });
        }

        // Note: GetCustomer, GetAllCustomers, and DeleteCustomer remain the same as the DataAnnotations version
        // They don't require validation since they don't accept request bodies
    }
}
