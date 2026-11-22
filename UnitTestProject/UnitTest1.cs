using AutoMapper;
using Moq;
using MyWebAPI.Dtos;
using MyWebAPI.Models;
using MyWebAPI.Repositories;
using MyWebAPI.Services;
using Xunit;

namespace UnitTestProject
{
    public class StorageBoxServiceTests
    {
        private readonly Mock<IStorageBoxRepository> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly StorageBoxService _service;

        public StorageBoxServiceTests()
        {
            _mockRepo = new Mock<IStorageBoxRepository>();
            _mockMapper = new Mock<IMapper>();
            _service = new StorageBoxService(_mockRepo.Object, _mockMapper.Object);
        }

        [Theory]
        [InlineData(1500, 2500, 4000)]
        [InlineData(500.5, 1499.5, 2000)]
        [InlineData(0, 0, 0)]
        public async Task CalculateOccupiedMonthlyRevenueAsync_WithOccupiedBoxes_ReturnsExpectedSum(
            double firstPrice, double secondPrice, double expectedRevenue)
        {
            var boxes = new List<StorageBox>
            {
                new StorageBox { Id = "1", Status = StorageBoxStatus.Occupied, MonthlyPrice = (decimal)firstPrice },
                new StorageBox { Id = "2", Status = StorageBoxStatus.Available, MonthlyPrice = 2000m },
                new StorageBox { Id = "3", Status = StorageBoxStatus.Occupied, MonthlyPrice = (decimal)secondPrice },
                new StorageBox { Id = "4", Status = StorageBoxStatus.Reserved, MonthlyPrice = 1800m }
            };
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(boxes);

            decimal revenue = await _service.CalculateOccupiedMonthlyRevenueAsync();

            Assert.Equal((decimal)expectedRevenue, revenue);
        }

        [Fact]
        public async Task CalculateOccupiedMonthlyRevenueAsync_EmptyCollection_ReturnsZero()
        {
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<StorageBox>());

            decimal revenue = await _service.CalculateOccupiedMonthlyRevenueAsync();

            Assert.Equal(0m, revenue);
        }

        [Fact]
        public async Task CalculateOccupiedMonthlyRevenueAsync_CallsRepositoryGetAllOnce()
        {
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<StorageBox>());

            await _service.CalculateOccupiedMonthlyRevenueAsync();

            _mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task CountReadyForRentBoxesAsync_WithMixedBoxes_ReturnsOnlyAvailableAndDisinfectedCount()
        {
            var boxes = new List<StorageBox>
            {
                new StorageBox { Id = "1", Status = StorageBoxStatus.Available, IsDisinfected = true },
                new StorageBox { Id = "2", Status = StorageBoxStatus.Available, IsDisinfected = false },
                new StorageBox { Id = "3", Status = StorageBoxStatus.Occupied, IsDisinfected = true },
                new StorageBox { Id = "4", Status = StorageBoxStatus.Available, IsDisinfected = true }
            };
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(boxes);

            int count = await _service.CountReadyForRentBoxesAsync();

            Assert.Equal(2, count);
        }

        [Fact]
        public async Task CountReadyForRentBoxesAsync_EmptyCollection_ReturnsZero()
        {
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<StorageBox>());

            int count = await _service.CountReadyForRentBoxesAsync();

            Assert.Equal(0, count);
        }

        [Theory]
        [InlineData(null, "2")]
        [InlineData(15.0, "2")]
        [InlineData(6.0, "1")]
        public async Task GetMostExpensiveAvailableBoxAsync_WithVariousVolumeFilters_ReturnsExpectedBoxId(
            double? maxVolume, string expectedId)
        {
            var smallBox = new StorageBox { Id = "1", Status = StorageBoxStatus.Available, Volume = 4, MonthlyPrice = 1200m };
            var largeBox = new StorageBox { Id = "2", Status = StorageBoxStatus.Available, Volume = 12, MonthlyPrice = 3500m };
            var occupiedBox = new StorageBox { Id = "3", Status = StorageBoxStatus.Occupied, Volume = 5, MonthlyPrice = 5000m };

            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<StorageBox> { smallBox, largeBox, occupiedBox });
                _mockMapper.Setup(m => m.Map<StorageBoxResponseDto>(It.IsAny<StorageBox>()))
                           .Returns((StorageBox b) => new StorageBoxResponseDto { Id = b.Id!, MonthlyPrice = b.MonthlyPrice });

            var result = await _service.GetMostExpensiveAvailableBoxAsync(maxVolume);

            Assert.Equal(expectedId, result.Id);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task GetMostExpensiveAvailableBoxAsync_InvalidMaxVolume_ThrowsArgumentException(double maxVolume)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetMostExpensiveAvailableBoxAsync(maxVolume));
        }

        [Fact]
        public async Task GetMostExpensiveAvailableBoxAsync_NoAvailableBoxes_ThrowsInvalidOperationException()
        {
            var boxes = new List<StorageBox>
            {
                new StorageBox { Id = "1", Status = StorageBoxStatus.Occupied, MonthlyPrice = 2000m }
            };
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(boxes);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetMostExpensiveAvailableBoxAsync());
        }

        [Fact]
        public async Task GetByIdAsync_ExistingBox_ReturnsMappedDto()
        {
            var box = new StorageBox { Id = "box-1", MonthlyPrice = 1500m };
            _mockRepo.Setup(r => r.GetByIdAsync("box-1")).ReturnsAsync(box);
            _mockMapper.Setup(m => m.Map<StorageBoxResponseDto>(box))
                       .Returns(new StorageBoxResponseDto { Id = "box-1", MonthlyPrice = 1500m });

            var result = await _service.GetByIdAsync("box-1");

            Assert.Equal("box-1", result.Id);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingBox_ThrowsKeyNotFoundException()
        {
            _mockRepo.Setup(r => r.GetByIdAsync("missing")).ReturnsAsync((StorageBox?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetByIdAsync("missing"));
        }

        [Fact]
        public async Task DisinfectAsync_AvailableNotDisinfectedBox_CallsReplaceAsyncOnce()
        {
            var box = new StorageBox { Id = "box-1", Status = StorageBoxStatus.Available, IsDisinfected = false };
            _mockRepo.Setup(r => r.GetByIdAsync("box-1")).ReturnsAsync(box);
            _mockMapper.Setup(m => m.Map<StorageBoxResponseDto>(box))
                       .Returns(new StorageBoxResponseDto { Id = "box-1", IsDisinfected = true });

            await _service.DisinfectAsync("box-1");

            _mockRepo.Verify(r => r.ReplaceAsync("box-1", It.Is<StorageBox>(b => b.IsDisinfected)), Times.Once);
        }

        [Theory]
        [InlineData(StorageBoxStatus.Occupied, false)]
        [InlineData(StorageBoxStatus.Available, true)]
        public async Task DisinfectAsync_InvalidBoxState_ThrowsInvalidOperationException(
            StorageBoxStatus status, bool isDisinfected)
        {
            var box = new StorageBox { Id = "box-1", Status = status, IsDisinfected = isDisinfected };
            _mockRepo.Setup(r => r.GetByIdAsync("box-1")).ReturnsAsync(box);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DisinfectAsync("box-1"));
        }

        [Fact]
        public async Task DeleteAsync_AvailableBoxWithoutContract_CallsDeleteAsyncOnce()
        {
            var box = new StorageBox { Id = "box-1", Status = StorageBoxStatus.Available, CurrentContractId = null };
            _mockRepo.Setup(r => r.GetByIdAsync("box-1")).ReturnsAsync(box);

            await _service.DeleteAsync("box-1");

            _mockRepo.Verify(r => r.DeleteAsync("box-1"), Times.Once);
        }

        [Theory]
        [InlineData(StorageBoxStatus.Occupied, "contract-1")]
        [InlineData(StorageBoxStatus.Reserved, null)]
        [InlineData(StorageBoxStatus.Available, "contract-1")]
        public async Task DeleteAsync_UnavailableOrWithContract_ThrowsInvalidOperationException(
            StorageBoxStatus status, string? contractId)
        {
            var box = new StorageBox { Id = "box-1", Status = status, CurrentContractId = contractId };
            _mockRepo.Setup(r => r.GetByIdAsync("box-1")).ReturnsAsync(box);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync("box-1"));
        }
    }
}