using DogsHouseService.DTOs;
using FluentValidation;

namespace DogsHouseService.Validators
{
    public class CreateDogRequestValidator : AbstractValidator<CreateDogRequest>
    {
        public CreateDogRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
            RuleFor(x => x.Color).NotEmpty().WithMessage("Color is required.");
            RuleFor(x => x.Tail_length).GreaterThanOrEqualTo(0).WithMessage("Tail_length must be a non-negative integer.");
            RuleFor(x => x.Weight).GreaterThan(0).WithMessage("Weight must be greater than 0.");
        }
    }
}
