using Microsoft.AspNetCore.Mvc;
using DogsHouseService.Services;
using DogsHouseService.Mappings;
using DogsHouseService.DTOs;
using FluentValidation;
using FluentValidation.Results;

namespace DogsHouseService.Controllers
{
    [ApiController]
    public class DogsController : ControllerBase
    {
        private readonly IDogRepository _repo;
        private readonly IValidator<CreateDogRequest> _validator;

        public DogsController(IDogRepository repo, IValidator<CreateDogRequest> validator)
        {
            _repo = repo;
            _validator = validator;
        }

        [HttpGet]
        [Route("dogs")]
        public async Task<IActionResult> GetDogs([FromQuery] string? attribute, [FromQuery] string? order,
                                                 [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100,
                                                 CancellationToken ct = default)
        {
            var dogs = await _repo.GetAllAsync(attribute, order, pageNumber, pageSize, ct);
            var dtos = dogs.Select(d => d.ToDto());
            return Ok(dtos);
        }

        [HttpPost]
        [Route("dog")]
        public async Task<IActionResult> CreateDog([FromBody] CreateDogRequest request, CancellationToken ct = default)
        {
            ValidationResult vr = await _validator.ValidateAsync(request, ct);
            if (!vr.IsValid)
            {
                return BadRequest(new { errors = vr.Errors.Select(e => e.ErrorMessage) });
            }

            var existing = await _repo.GetByNameAsync(request.Name, ct);
            if (existing != null)
            {
                return Conflict(new { message = $"Dog with name '{request.Name}' already exists." });
            }

            if (request.Tail_length < 0)
            {
                return BadRequest(new { message = "Tail_length must be non-negative." });
            }

            var entity = request.ToEntity();
            try
            {
                await _repo.AddAsync(entity, ct);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Could not create dog.", detail = ex.Message });
            }

            var dto = entity.ToDto();
            return CreatedAtAction(nameof(GetDogs), new { name = dto.Name }, dto);
        }
    }
}
