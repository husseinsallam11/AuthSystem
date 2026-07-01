using Auth.Application.DTOs;
using FluentValidation;

namespace Auth.API.Validators
{
    public class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
    {
        public RefreshTokenDtoValidator() 
        {
            RuleFor(x => x.token).NotEmpty();
            RuleFor(x => x.refreshToken).NotEmpty();
        }
    }
}
