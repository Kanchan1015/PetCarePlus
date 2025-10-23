using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FluentAssertions;

// Adjust namespaces to match your project layout
using PetCare.Application.Inventory;
using PetCare.Application.Common.Interfaces;
using PetCare.Domain.Inventory;

namespace PetCare.Application.Tests.Inventory
{
    public class InventoryServiceTests
    {
        private readonly Mock<IInventoryRepository> _repoMock;
        private readonly InventoryService _service;

        public InventoryServiceTests()
        {
            _repoMock = new Mock<IInventoryRepository>(MockBehavior.Strict);
            _service = new InventoryService(_repoMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_MapsDomainToDto()
        {
            // Arrange
            var domainItems = new List<InventoryItem>
            {
                new InventoryItem
                {
                    Id = Guid.NewGuid(),
                    Name = "Food A",
                    Quantity = 5,
                    Category = "Food",
                    Supplier = "S1",
                    Description = "desc",
                    PhotoUrl = "/images/1.png",
                    ExpiryDate = DateTime.UtcNow.AddMonths(1)
                }
            };

            _repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((IReadOnlyList<InventoryItem>)domainItems));

            // Act
            var outDtos = (await _service.GetAllAsync()).ToList();

            // Assert
            outDtos.Should().HaveCount(1);
            outDtos[0].Id.Should().Be(domainItems[0].Id);
            outDtos[0].Name.Should().Be("Food A");
            outDtos[0].PhotoUrl.Should().Be("/images/1.png");

            _repoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenFound_MapsAndReturns()
        {
            // Arrange
            var id = Guid.NewGuid();
            var domain = new InventoryItem
            {
                Id = id,
                Name = "Toy",
                Quantity = 2,
                Category = "Toys",
                Supplier = "S2",
                Description = "desc2"
            };

            _repoMock
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((InventoryItem?)domain));

            // Act
            var dto = await _service.GetByIdAsync(id);

            // Assert
            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.Name.Should().Be("Toy");

            _repoMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
        {
            var id = Guid.NewGuid();
            _repoMock
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((InventoryItem?)null));

            var dto = await _service.GetByIdAsync(id);

            dto.Should().BeNull();
            _repoMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenDuplicateName_ThrowsInvalidOperation()
        {
            // Arrange - repository already contains an item with same name (case-insensitive/trim)
            var existing = new List<InventoryItem>
            {
                new InventoryItem { Id = Guid.NewGuid(), Name = "My Item" }
            };
            _repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((IReadOnlyList<InventoryItem>)existing));

            var dto = new CreateInventoryDto { Name = " my item ", Quantity = 1, Category = "c", Supplier = "s" };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(dto));
            ex.Message.Should().Contain("already exists");

            _repoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_Valid_AddsAndReturnsDto()
        {
            // Arrange - empty repository
            _repoMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((IReadOnlyList<InventoryItem>)new List<InventoryItem>()));

            // Setup AddAsync to set Id and return the item. Use Returns with delegate matching signature.
            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<InventoryItem>(), It.IsAny<CancellationToken>()))
                .Returns((InventoryItem it, CancellationToken ct) =>
                {
                    it.Id = Guid.NewGuid();
                    return Task.FromResult(it);
                });

            var dto = new CreateInventoryDto
            {
                Name = "New Item",
                Quantity = 4,
                Category = "C",
                Supplier = "Sup",
                Description = "d",
                PhotoUrl = "/img/x.png",
                ExpiryDate = DateTime.UtcNow.AddDays(10)
            };

            // Act
            var created = await _service.CreateAsync(dto);

            // Assert
            created.Should().NotBeNull();
            created.Id.Should().NotBeEmpty();
            created.Name.Should().Be("New Item");
            created.PhotoUrl.Should().Be("/img/x.png");

            _repoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<InventoryItem>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenNotFound_ReturnsFalse()
        {
            var id = Guid.NewGuid();
            _repoMock
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((InventoryItem?)null));

            var dto = new UpdateInventoryDto { Name = "X", Quantity = 1, Category = "C", Supplier = "S" };

            var result = await _service.UpdateAsync(id, dto);

            result.Should().BeFalse();
            _repoMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenDuplicateName_ThrowsInvalidOperation()
        {
            // Arrange: repository has two items; one with same normalized name and different id
            var idToUpdate = Guid.NewGuid();
            var existingItem = new InventoryItem { Id = Guid.NewGuid(), Name = "Other Item" };
            var targetItem = new InventoryItem { Id = idToUpdate, Name = "Target" };

            _repoMock.Setup(r => r.GetByIdAsync(idToUpdate, It.IsAny<CancellationToken>())).Returns(Task.FromResult((InventoryItem?)targetItem));
            _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult((IReadOnlyList<InventoryItem>)new List<InventoryItem> { existingItem, targetItem }));

            var dto = new UpdateInventoryDto { Name = " other item ", Quantity = 1, Category = "C", Supplier = "S" };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(idToUpdate, dto));
            ex.Message.Should().Contain("already exists");

            _repoMock.Verify(r => r.GetByIdAsync(idToUpdate, It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenValid_UpdatesAndReturnsTrue()
        {
            var id = Guid.NewGuid();
            var item = new InventoryItem
            {
                Id = id,
                Name = "Old",
                Quantity = 1,
                Category = "OldCat",
                Supplier = "OldSup"
            };

            _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).Returns(Task.FromResult((InventoryItem?)item));
            _repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult((IReadOnlyList<InventoryItem>)new List<InventoryItem> { item }));
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<InventoryItem>(), It.IsAny<CancellationToken>())).Returns((InventoryItem it, CancellationToken ct) => Task.FromResult(it));

            var dto = new UpdateInventoryDto
            {
                Name = "New Name",
                Quantity = 10,
                Category = "NewCat",
                Supplier = "NewSup",
                Description = "desc",
                PhotoUrl = "/img/new.png",
                ExpiryDate = DateTime.UtcNow.AddDays(5)
            };

            var result = await _service.UpdateAsync(id, dto);

            result.Should().BeTrue();
            item.Name.Should().Be("New Name");
            item.Quantity.Should().Be(10);
            item.PhotoUrl.Should().Be("/img/new.png");

            _repoMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(It.Is<InventoryItem>(i => i.Id == id && i.Name == "New Name"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenNotExists_ReturnsFalse()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.ExistsAsync(id, It.IsAny<CancellationToken>())).Returns(Task.FromResult(false));

            var res = await _service.DeleteAsync(id);

            res.Should().BeFalse();
            _repoMock.Verify(r => r.ExistsAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenExists_DeletesAndReturnsTrue()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.ExistsAsync(id, It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));
            _repoMock.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var res = await _service.DeleteAsync(id);

            res.Should().BeTrue();
            _repoMock.Verify(r => r.ExistsAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            _repoMock.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    public class InventoryDtoValidationTests
    {
        private static IList<ValidationResult> Validate(object model)
        {
            var ctx = new ValidationContext(model);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, ctx, results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void CreateInventoryDto_Valid_PassesValidation()
        {
            var dto = new CreateInventoryDto
            {
                Name = "Valid",
                Quantity = 0,
                Category = "C",
                Supplier = "S",
                Description = "d"
            };

            var results = Validate(dto);
            results.Should().BeEmpty();
        }

        [Fact]
        public void CreateInventoryDto_MissingRequired_FailsValidation()
        {
            var dto = new CreateInventoryDto
            {
                // Name missing
                Quantity = -1, // invalid per Range
                Category = "",
                Supplier = ""
            };

            var results = Validate(dto);
            results.Should().NotBeEmpty();
            var memberNames = results.SelectMany(r => r.MemberNames).ToList();
            memberNames.Should().Contain("Name");
            memberNames.Should().Contain("Quantity");
            memberNames.Should().Contain("Category");
            memberNames.Should().Contain("Supplier");
        }

        [Fact]
        public void UpdateInventoryDto_MissingRequired_FailsValidation()
        {
            var dto = new UpdateInventoryDto
            {
                Name = "",
                Quantity = -5,
                Category = "",
                Supplier = ""
            };

            var results = Validate(dto);
            results.Should().NotBeEmpty();
            var memberNames = results.SelectMany(r => r.MemberNames).ToList();
            memberNames.Should().Contain("Name");
            memberNames.Should().Contain("Quantity");
            memberNames.Should().Contain("Category");
            memberNames.Should().Contain("Supplier");
        }
    }
}
