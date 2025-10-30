using DogsHouseService.Data;
using Microsoft.EntityFrameworkCore;

namespace DogsHouseService.Services
{
    public class DogRepository : IDogRepository
    {
        private readonly DogsDbContext _db;
        public DogRepository(DogsDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Dog dog, CancellationToken ct)
        {
            _db.Dogs.Add(dog);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<int> CountAsync(CancellationToken ct) => await _db.Dogs.CountAsync(ct);

        public async Task<List<Dog>> GetAllAsync(string? attribute, string? order, int pageNumber, int pageSize, CancellationToken ct)
        {
            IQueryable<Dog> q = _db.Dogs.AsNoTracking();

            if (!string.IsNullOrEmpty(attribute))
            {
                bool desc = string.Equals(order, "desc", StringComparison.OrdinalIgnoreCase);
                q = attribute.ToLower() switch
                {
                    "name" => desc ? q.OrderByDescending(d => d.Name) : q.OrderBy(d => d.Name),
                    "color" => desc ? q.OrderByDescending(d => d.Color) : q.OrderBy(d => d.Color),
                    "tail_length" => desc ? q.OrderByDescending(d => d.TailLength) : q.OrderBy(d => d.TailLength),
                    "weight" => desc ? q.OrderByDescending(d => d.Weight) : q.OrderBy(d => d.Weight),
                    _ => q
                };
            }

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            q = q.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            return await q.ToListAsync(ct);
        }

        public async Task<Dog?> GetByNameAsync(string name, CancellationToken ct)
        {
            return await _db.Dogs.FirstOrDefaultAsync(d => d.Name == name, ct);
        }
    }
}
