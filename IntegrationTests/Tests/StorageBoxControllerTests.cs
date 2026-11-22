using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using MyWebAPI.Documents;
using MyWebAPI.Dtos;
using MyWebAPI.Models;

namespace IntegrationTests.Tests;

public class StorageBoxControllerTests : BaseIntegrationTest
{
    public StorageBoxControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_ReturnsSeededBoxes()
    {
        var response = await Client.GetAsync("/api/StorageBox");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var boxes = await response.Content.ReadFromJsonAsync<List<StorageBoxResponseDto>>();
        Assert.NotNull(boxes);
        Assert.Equal(3, boxes.Count);
    }

    [Fact]
    public async Task GetById_ReturnsCorrectBox()
    {
        var list = await Client.GetFromJsonAsync<List<StorageBoxResponseDto>>("/api/StorageBox");
        var expected = list!.First();

        var response = await Client.GetAsync($"/api/StorageBox/{expected.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var box = await response.Content.ReadFromJsonAsync<StorageBoxResponseDto>();
        Assert.NotNull(box);
        Assert.Equal(expected.Id, box.Id);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_ForNonExistingId()
    {
        var fakeId = ObjectId.GenerateNewId().ToString();

        var response = await Client.GetAsync($"/api/StorageBox/{fakeId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAvailable_ReturnsOnlyAvailableMatchingBoxes()
    {
        var response = await Client.GetAsync("/api/StorageBox/available?type=Heating&minVolume=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var boxes = await response.Content.ReadFromJsonAsync<List<StorageBoxResponseDto>>();
        Assert.NotNull(boxes);
        Assert.Single(boxes);
    }

    [Fact]
    public async Task Create_ValidDto_ReturnsCreatedAndSavesToDatabase()
    {
        var dto = new StorageBoxCreateDto
        {
            Square = 12.5,
            Volume = 18.0,
            Type = StorageBoxType.Heating,
            IsDisinfected = true,
            MonthlyPrice = 2100m
        };

        var response = await Client.PostAsJsonAsync("/api/StorageBox", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<StorageBoxResponseDto>();
        Assert.NotNull(created);
        Assert.False(string.IsNullOrWhiteSpace(created.Id));

        var db = Services.GetRequiredService<IMongoDatabase>();
        var count = await db.GetCollection<StorageBoxDocument>(DatabaseSeeder.CollectionName)
                            .CountDocumentsAsync(FilterDefinition<StorageBoxDocument>.Empty);
        Assert.Equal(4, count);
    }

    [Fact]
    public async Task Create_InvalidDto_ReturnsBadRequest()
    {
        var invalidDto = new StorageBoxCreateDto
        {
            Square = 0,
            Volume = 500,
            Type = StorageBoxType.Heating,
            IsDisinfected = false,
            MonthlyPrice = -100m
        };

        var response = await Client.PostAsJsonAsync("/api/StorageBox", invalidDto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task FullUpdate_ValidDto_ReturnsOkAndUpdatesDatabase()
    {
        var list = await Client.GetFromJsonAsync<List<StorageBoxResponseDto>>("/api/StorageBox");
        var targetId = list!.First().Id;

        var updateDto = new StorageBoxCreateDto
        {
            Square = 30.0,
            Volume = 28.0,
            Type = StorageBoxType.Unheating,
            IsDisinfected = false,
            MonthlyPrice = 4500m
        };

        var response = await Client.PutAsJsonAsync($"/api/StorageBox/{targetId}", updateDto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var db = Services.GetRequiredService<IMongoDatabase>();
        var updatedInDb = await db.GetCollection<StorageBoxDocument>(DatabaseSeeder.CollectionName)
                                  .Find(x => x.Id == targetId)
                                  .FirstOrDefaultAsync();
        Assert.NotNull(updatedInDb);
        Assert.Equal(30.0, updatedInDb.Square);
        Assert.Equal(4500m, updatedInDb.MonthlyPrice);
    }

    [Fact]
    public async Task FullUpdate_NonExistingId_ReturnsNotFound()
    {
        var fakeId = ObjectId.GenerateNewId().ToString();
        var updateDto = new StorageBoxCreateDto
        {
            Square = 10.0,
            Volume = 10.0,
            Type = StorageBoxType.Heating,
            IsDisinfected = true,
            MonthlyPrice = 1000m
        };

        var response = await Client.PutAsJsonAsync($"/api/StorageBox/{fakeId}", updateDto);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PartialUpdate_ValidDto_UpdatesOnlySpecifiedFields()
    {
        var list = await Client.GetFromJsonAsync<List<StorageBoxResponseDto>>("/api/StorageBox");
        var targetId = list!.First().Id;

        var patchDto = new StorageBoxPatchDto
        {
            MonthlyPrice = 1999m
        };

        var response = await Client.PatchAsJsonAsync($"/api/StorageBox/{targetId}", patchDto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var db = Services.GetRequiredService<IMongoDatabase>();
        var updatedInDb = await db.GetCollection<StorageBoxDocument>(DatabaseSeeder.CollectionName)
                                  .Find(x => x.Id == targetId)
                                  .FirstOrDefaultAsync();
        Assert.NotNull(updatedInDb);
        Assert.Equal(1999m, updatedInDb.MonthlyPrice);
    }

    [Fact]
    public async Task Disinfect_ExistingBox_SetsIsDisinfectedTrueInDatabase()
    {
        var list = await Client.GetFromJsonAsync<List<StorageBoxResponseDto>>("/api/StorageBox");
        var targetId = list![1].Id;

        var response = await Client.PostAsync($"/api/StorageBox/{targetId}/disinfect", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var db = Services.GetRequiredService<IMongoDatabase>();
        var updatedInDb = await db.GetCollection<StorageBoxDocument>(DatabaseSeeder.CollectionName)
                                  .Find(x => x.Id == targetId)
                                  .FirstOrDefaultAsync();
        Assert.NotNull(updatedInDb);
        Assert.True(updatedInDb.IsDisinfected);
    }

    [Fact]
    public async Task Delete_ExistingId_ReturnsNoContentAndRemovesFromDatabase()
    {
        var list = await Client.GetFromJsonAsync<List<StorageBoxResponseDto>>("/api/StorageBox");
        var targetId = list!.First().Id;

        var response = await Client.DeleteAsync($"/api/StorageBox/{targetId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var db = Services.GetRequiredService<IMongoDatabase>();
        var count = await db.GetCollection<StorageBoxDocument>(DatabaseSeeder.CollectionName)
                            .CountDocumentsAsync(FilterDefinition<StorageBoxDocument>.Empty);
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task Delete_NonExistingId_ReturnsNotFound()
    {
        var fakeId = ObjectId.GenerateNewId().ToString();

        var response = await Client.DeleteAsync($"/api/StorageBox/{fakeId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}