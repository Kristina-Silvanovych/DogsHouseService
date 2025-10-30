using System.ComponentModel.DataAnnotations;

namespace DogsHouseService.Data
{
    public class Dog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Color { get; set; } = null!;

        public int TailLength { get; set; }
        public int Weight { get; set; }
    }
}
