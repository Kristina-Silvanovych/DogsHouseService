using DogsHouseService.Data;

namespace DogsHouseService.Services
{
    public interface IDogRepository
    {
        Task<List<Dog>> GetAllAsync(string? attribute, string? order, int pageNumber, int pageSize, CancellationToken ct);
        Task<int> CountAsync(CancellationToken ct);
        Task<Dog?> GetByNameAsync(string name, CancellationToken ct);
        Task AddAsync(Dog dog, CancellationToken ct);
    }
}
