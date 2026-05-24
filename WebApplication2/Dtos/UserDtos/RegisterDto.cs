using FluentValidation;

namespace WebApplication2.Dtos.UserDtos
{
    public class RegisterDto
    {
        public string FullName { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Confirmpassword { get; set; } = null!;
    }
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator() 
        {
            RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required");


            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username is required");


            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required");


            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters");
               

            RuleFor(x => x.Confirmpassword)
                .NotEmpty().WithMessage("Confirm password is required")
                .Equal(x => x.Password).WithMessage("Passwords do not match");
        }
    }
}
