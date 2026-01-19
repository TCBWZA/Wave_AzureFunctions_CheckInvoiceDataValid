using EfCoreLab.DTOs;
using FluentValidation;

namespace EfCoreLab.Validators
{
    /// <summary>
    /// FluentValidation validator for UpdateCustomerDto.
    /// Demonstrates validation rules for update operations.
    /// </summary>
    public class UpdateCustomerValidator : AbstractValidator<UpdateCustomerDto>
    {
        public UpdateCustomerValidator()
        {
            // Name validation (same rules as Create)
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required.")
                .MaximumLength(200)
                .WithMessage("Name cannot exceed 200 characters.")
                .MinimumLength(2)
                .WithMessage("Name must be at least 2 characters long.");

            // Email validation (same rules as Create)
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Invalid email address format.")
                .MaximumLength(200)
                .WithMessage("Email cannot exceed 200 characters.");

            // Demonstrate conditional validation
            RuleFor(x => x.Name)
                .Must((dto, name) => 
                {
                    if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains('@'))
                        return true;
                    
                    var emailUsername = dto.Email.Split('@')[0];
                    return !name.Equals(emailUsername, StringComparison.OrdinalIgnoreCase);
                })
                .WithMessage("Name should not be the same as email username.");
        }
    }
}
