using DogsHouseService.Data;
using DogsHouseService.DTOs;

namespace DogsHouseService.Mappings
{
    public static class DogMapping
    {
        public static DogDto ToDto(this Dog dog)
        {
            var color = dog.Color.Replace(" & ", "&").Replace(" &", "&").Replace("& ", "&");
            return new DogDto
            {
                Name = dog.Name,
                Color = color,
                Tail_length = dog.TailLength,
                Weight = dog.Weight
            };
        }

        public static Dog ToEntity(this CreateDogRequest req)
        {
            return new Dog
            {
                Name = req.Name,
                Color = req.Color,
                TailLength = req.Tail_length,
                Weight = req.Weight
            };
        }
    }
}
