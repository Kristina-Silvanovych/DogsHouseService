using System.Net;
using System.Net.Http.Json;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using DogsHouseService.DTOs;
using FluentAssertions;

namespace DogsHouseService.Tests
{
    public class DogsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public DogsControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Ping_Returns_Version()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("/ping");
            resp.EnsureSuccessStatusCode();
            var content = await resp.Content.ReadAsStringAsync();
            content.Trim('"').Trim().Should().Be("Dogshouseservice.Version1.0.1");
        }

        [Fact]
        public async Task GetDogs_Returns_List()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("/dogs");
            resp.EnsureSuccessStatusCode();
            var dogs = await resp.Content.ReadFromJsonAsync<DogDto[]>();
            dogs.Should().NotBeNull();
            dogs!.Length.Should().BeGreaterThanOrEqualTo(2);
        }

        [Fact]
        public async Task CreateDog_Returns_Conflict_For_DuplicateName()
        {
            var client = _factory.CreateClient();
            var request = new CreateDogRequest
            {
                Name = "Neo",
                Color = "red",
                Tail_length = 22,
                Weight = 32
            };
            var response = await client.PostAsJsonAsync("/dog", request);
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task CreateDog_Returns_BadRequest_For_NegativeTailLength()
        {
            var client = _factory.CreateClient();
            var request = new CreateDogRequest
            {
                Name = "NewDog",
                Color = "blue",
                Tail_length = -5,
                Weight = 10
            };
            var response = await client.PostAsJsonAsync("/dog", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateDog_Returns_BadRequest_For_InvalidModel()
        {
            var client = _factory.CreateClient();
            var invalidRequest = new CreateDogRequest
            {
                Name = "",
                Color = "red",
                Tail_length = 5,
                Weight = 10
            };

            var response = await client.PostAsJsonAsync("/dog", invalidRequest);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateDog_Can_Add_NewDog()
        {
            var client = _factory.CreateClient();

            var request = new CreateDogRequest
            {
                Name = $"DoggyTest_{Guid.NewGuid()}",
                Color = "green",
                Tail_length = 10,
                Weight = 5
            };

            var response = await client.PostAsJsonAsync("/dog", request);

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var dog = await response.Content.ReadFromJsonAsync<DogDto>();
            dog.Should().NotBeNull();
            dog!.Name.Should().Be(request.Name);
        }

        [Fact]
        public async Task GetDogs_Sorting_And_Pagination_Works()
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("/dogs?attribute=weight&order=desc&pageNumber=1&pageSize=1");
            resp.EnsureSuccessStatusCode();
            var dogs = await resp.Content.ReadFromJsonAsync<DogDto[]>();
            dogs.Should().NotBeNull();
            dogs!.Length.Should().Be(1);
            dogs[0].Weight.Should().BeGreaterThanOrEqualTo(dogs[0].Tail_length); 
        }

        [Fact]
        public async Task RateLimiting_Returns_429_When_Exceeded()
        {
            var client = _factory.CreateClient();
            int success = 0, tooMany = 0;

            for (int i = 0; i < 20; i++)
            {
                var resp = await client.GetAsync("/ping");
                if (resp.StatusCode == HttpStatusCode.OK)
                    success++;
                else if (resp.StatusCode == (HttpStatusCode)429)
                    tooMany++;
            }

            success.Should().BeLessThanOrEqualTo(10);
            tooMany.Should().BeGreaterThan(0);
        }
    }
}
