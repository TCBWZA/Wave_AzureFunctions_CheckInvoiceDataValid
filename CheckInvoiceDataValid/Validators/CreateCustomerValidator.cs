using EfCoreLab.DTOs;
using FluentValidation;

namespace EfCoreLab.Validators
{
    /// <summary>
    /// FluentValidation validator for CreateCustomerDto.
    /// Demonstrates an alternative to DataAnnotations validation.
    /// </summary>
    public class CreateCustomerValidator : AbstractValidator<CreateCustomerDto>
    {
        public CreateCustomerValidator()
        {
            // Name validation
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required.")
                .MaximumLength(200)
                .WithMessage("Name cannot exceed 200 characters.")
                .MinimumLength(2)
                .WithMessage("Name must be at least 2 characters long.");

            // Email validation
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Invalid email address format.")
                .MaximumLength(200)
                .WithMessage("Email cannot exceed 200 characters.");

            // Advanced FluentValidation features (optional examples)
            
            // Example: Ensure name doesn't contain numbers
            RuleFor(x => x.Name)
                .Matches(@"^[a-zA-Z\s\-\.]+$")
                .WithMessage("Name can only contain letters, spaces, hyphens, and periods.")
                .When(x => !string.IsNullOrWhiteSpace(x.Name));

            // Example: Ensure email domain is not from temporary email services
            RuleFor(x => x.Email)
                .Must(email => !IsTemporaryEmail(email))
                .WithMessage("Temporary email addresses are not allowed.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));
        }

        private bool IsTemporaryEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var tempDomains = new[] { "tempmail.com", "throwaway.email", "guerrillamail.com" };
            var domain = email.Split('@').LastOrDefault()?.ToLower();
            
            return domain != null && tempDomains.Contains(domain);
        }
    }
}
