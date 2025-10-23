using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;

// Use the same namespace that's used by your controller/service DTOs
using PetCare.Api.Controllers;
using PetCare.Application.Inventory;

namespace PetCare.Api.Tests.Controllers
{
    public class InventoryControllerTests
    {
        private readonly Mock<IInventoryService> _serviceMock;
        private readonly InventoryController _controller;

        public InventoryControllerTests()
        {
            _serviceMock = new Mock<IInventoryService>(MockBehavior.Strict);
            _controller = new InventoryController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithItems()
        {
            var items = new List<InventoryItemDto>
            {
                new InventoryItemDto { Id = Guid.NewGuid(), Name = "Toy A", Category = "Toys", Supplier = "S1", Description = "desc", Quantity = 3 },
                new InventoryItemDto { Id = Guid.NewGuid(), Name = "Food B", Category = "Food", Supplier = "S2", Description = "desc2", Quantity = 10 }
            };
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(items);

            var result = await _controller.GetAll();

            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            ok.StatusCode.Should().Be(StatusCodes.Status200OK);
            ok.Value.Should().BeEquivalentTo(items);

            _serviceMock.Verify(s => s.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetById_WhenNotFound_ReturnsNotFound()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((InventoryItemDto?)null);

            var result = await _controller.GetById(id);

            result.Should().BeOfType<NotFoundResult>();
            _serviceMock.Verify(s => s.GetByIdAsync(id), Times.Once);
        }

        [Fact]
        public async Task GetById_WhenFound_ReturnsOk()
        {
            var id = Guid.NewGuid();
            var dto = new InventoryItemDto { Id = id, Name = "Found", Quantity = 1 };
            _serviceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(dto);

            var result = await _controller.GetById(id);

            var ok = result as OkObjectResult;
            ok.Should().NotBeNull();
            ok.Value.Should().BeEquivalentTo(dto);
            _serviceMock.Verify(s => s.GetByIdAsync(id), Times.Once);
        }

        [Fact]
        public async Task Search_WithEmptyQuery_ReturnsBadRequest()
        {
            var result = await _controller.Search("");

            var bad = result as BadRequestObjectResult;
            bad.Should().NotBeNull();
            bad.Value.Should().Be("Query is required.");
        }

        [Fact]
        public async Task Create_WhenDuplicate_ThrowsInvalidOperation_ReturnsBadRequestWithDuplicateFlag()
        {
            var createDto = new CreateInventoryDto { Name = "X", Quantity = 1 };
            _serviceMock.Setup(s => s.CreateAsync(createDto)).ThrowsAsync(new InvalidOperationException("duplicate"));

            var result = await _controller.Create(createDto);

            var bad = result as BadRequestObjectResult;
            bad.Should().NotBeNull();

            // Access anonymous object's properties via reflection
            var value = bad.Value!;
            var type = value.GetType();
            var dupProp = type.GetProperty("duplicate");
            dupProp.Should().NotBeNull("response should include a 'duplicate' property");
            var dupVal = dupProp.GetValue(value);
            Assert.True(dupVal is bool && (bool)dupVal);

            var msgProp = type.GetProperty("message");
            msgProp.Should().NotBeNull("response should include a 'message' property");
            var msgVal = msgProp.GetValue(value) as string;
            Assert.Equal("duplicate", msgVal);

            _serviceMock.Verify(s => s.CreateAsync(createDto), Times.Once);
        }

        [Fact]
        public async Task Update_WhenNotFound_ReturnsNotFound()
        {
            var id = Guid.NewGuid();
            var updateDto = new UpdateInventoryDto { Name = "Updated" };
            _serviceMock.Setup(s => s.UpdateAsync(id, updateDto)).ReturnsAsync(false);

            var result = await _controller.Update(id, updateDto);

            result.Should().BeOfType<NotFoundResult>();
            _serviceMock.Verify(s => s.UpdateAsync(id, updateDto), Times.Once);
        }

        [Fact]
        public async Task Update_WhenDuplicate_ThrowsInvalidOperation_ReturnsBadRequestWithDuplicateFlag()
        {
            var id = Guid.NewGuid();
            var updateDto = new UpdateInventoryDto { Name = "Updated" };
            _serviceMock.Setup(s => s.UpdateAsync(id, updateDto)).ThrowsAsync(new InvalidOperationException("duplicate update"));

            var result = await _controller.Update(id, updateDto);

            var bad = result as BadRequestObjectResult;
            bad.Should().NotBeNull();

            var value = bad.Value!;
            var type = value.GetType();
            var dupProp = type.GetProperty("duplicate");
            dupProp.Should().NotBeNull("response should include a 'duplicate' property");
            var dupVal = dupProp.GetValue(value);
            Assert.True(dupVal is bool && (bool)dupVal);

            var msgProp = type.GetProperty("message");
            msgProp.Should().NotBeNull("response should include a 'message' property");
            var msgVal = msgProp.GetValue(value) as string;
            Assert.Equal("duplicate update", msgVal);

            _serviceMock.Verify(s => s.UpdateAsync(id, updateDto), Times.Once);
        }

        [Fact]
        public async Task Delete_WhenNotFound_ReturnsNotFound()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(false);

            var result = await _controller.Delete(id);

            result.Should().BeOfType<NotFoundResult>();
            _serviceMock.Verify(s => s.DeleteAsync(id), Times.Once);
        }

        [Fact]
        public async Task Delete_WhenSuccess_ReturnsNoContent()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(true);

            var result = await _controller.Delete(id);

            result.Should().BeOfType<NoContentResult>();
            _serviceMock.Verify(s => s.DeleteAsync(id), Times.Once);
        }

        [Fact]
        public async Task UploadPhoto_WhenNoFile_ReturnsBadRequest()
        {
            var result = await _controller.UploadPhoto(null);

            var bad = result as BadRequestObjectResult;
            bad.Should().NotBeNull();
            bad.Value.Should().Be("No file uploaded");
        }

        [Fact]
        public async Task UploadPhoto_SavesFileAndReturnsUrl()
        {
            var originalCwd = Directory.GetCurrentDirectory();
            var tempDir = Path.Combine(Path.GetTempPath(), "petcare_tests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            try
            {
                Directory.SetCurrentDirectory(tempDir);

                var httpContext = new DefaultHttpContext();
                httpContext.Request.Scheme = "https";
                httpContext.Request.Host = new HostString("localhost:5001");
                _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

                var contentBytes = Encoding.UTF8.GetBytes("fake-image-content");
                var stream = new MemoryStream(contentBytes);
                IFormFile file = new FormFile(stream, 0, contentBytes.Length, "file", "photo.png")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/png"
                };

                var result = await _controller.UploadPhoto(file);

                var ok = result as OkObjectResult;
                ok.Should().NotBeNull();

                // Access url via reflection
                var value = ok.Value!;
                var type = value.GetType();
                var urlProp = type.GetProperty("url");
                urlProp.Should().NotBeNull("response should include a 'url' property");
                var urlVal = urlProp.GetValue(value) as string;
                urlVal.Should().NotBeNull();
                urlVal.Should().Contain("/images/inventory/");

                var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "inventory");
                Directory.Exists(imagesFolder).Should().BeTrue();

                var savedFiles = Directory.GetFiles(imagesFolder);
                savedFiles.Length.Should().BeGreaterThan(0);
                foreach (var f in savedFiles) File.Delete(f);
            }
            finally
            {
                Directory.SetCurrentDirectory(originalCwd);
                try { Directory.Delete(tempDir, true); } catch { /* best effort cleanup */ }
            }
        }
    }
}
