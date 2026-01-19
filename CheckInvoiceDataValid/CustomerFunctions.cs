using EfCoreLab.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EfCoreLab.Functions
{
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

            var validationErrors = ValidateModel(dto);
            if (validationErrors.Any())
            {
                return new BadRequestObjectResult(new { errors = validationErrors });
            }

            return new OkObjectResult(new { message = "CreateCustomer validation passed.", customer = dto });
        }

        [Function("UpdateCustomer")]
        public async Task<IActionResult> UpdateCustomer(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "customers/{id:long}")] HttpRequestData req,
            long id)
        {
            _logger.LogInformation("UpdateCustomer called for id {Id}.", id);

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

            var validationErrors = ValidateModel(dto);
            if (validationErrors.Any())
            {
                return new BadRequestObjectResult(new { errors = validationErrors });
            }

            return new OkObjectResult(new { message = "UpdateCustomer validation passed.", id, customer = dto });
        }

        [Function("GetCustomer")]
        public IActionResult GetCustomer(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "customers/{id:long}")] HttpRequestData req,
            long id)
        {
            _logger.LogInformation("GetCustomer called for id {Id}.", id);

            return new OkObjectResult(new { message = "GetCustomer called.", id });
        }

        [Function("GetAllCustomers")]
        public IActionResult GetAllCustomers(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "customers")] HttpRequestData req)
        {
            _logger.LogInformation("GetAllCustomers called.");

            return new OkObjectResult(new { message = "GetAllCustomers called.", customers = new List<CustomerDto>() });
        }

        [Function("DeleteCustomer")]
        public IActionResult DeleteCustomer(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "customers/{id:long}")] HttpRequestData req,
            long id)
        {
            _logger.LogInformation("DeleteCustomer called for id {Id}.", id);

            return new OkObjectResult(new { message = "DeleteCustomer called.", id });
        }

        private static IDictionary<string, string[]> ValidateModel(object model)
        {
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);

            var errors = results
                .SelectMany(r =>
                    r.MemberNames.Any()
                        ? r.MemberNames.Select(m => new { Member = m, Message = r.ErrorMessage ?? string.Empty })
                        : new[] { new { Member = string.Empty, Message = r.ErrorMessage ?? string.Empty } })
                .GroupBy(x => x.Member)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Message).ToArray());

            return errors;
        }
    }
}
