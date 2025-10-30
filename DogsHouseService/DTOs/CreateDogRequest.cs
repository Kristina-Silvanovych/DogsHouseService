namespace DogsHouseService.DTOs
{
    public class CreateDogRequest
    {
        public string Name { get; set; } = null!;
        public string Color { get; set; } = null!;
        public int Tail_length { get; set; }
        public int Weight { get; set; }
    }
}
