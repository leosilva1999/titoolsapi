using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TiTools_backend.Context;
using TiTools_backend.Controllers;
using TiTools_backend.DTOs;
using TiTools_backend.Models;
using TiTools_backend.Repositories;
using TiTools_backend.Services;

namespace TiTools_backend.Tests.Controllers
{
    public class EquipmentControllerTests
    {
        private readonly EquipmentController _equipmentController;
        private readonly IEquipmentRepository _mockEquipmentRepository;
        private readonly IEquipmentService _mockEquipmentService;
        private readonly Faker _faker = new("pt_BR");
        private readonly AppDbContext _appDb;

        public EquipmentControllerTests()
        {
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>();
            dbContextOptions.UseInMemoryDatabase("titoolsdbtest");

            _appDb = new AppDbContext(dbContextOptions.Options);

            _mockEquipmentRepository = Substitute.For<IEquipmentRepository>();
            _mockEquipmentService = Substitute.For<IEquipmentService>();

            _equipmentController = new EquipmentController(_mockEquipmentService);

        }

        [Fact]
        public async Task GetEquipmentsAsync_GivenAllParameters_ThenShoudBeReturnAEmptyList()
        {
            //Arrange
            _mockEquipmentService
                .GetEquipmentsAsync(10, 0, Arg.Any<EquipmentFilterDTO>())
                .Returns((new List<Equipment>(), 0));

            //Act
            var result = await _equipmentController.GetEquipmentsAsync(10, 0, new EquipmentFilterDTO());

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
        }
        
        [Fact]
        public async Task GetEquipmentsAsync_WhenThereAreItems_ThenShoudBeReturnOkWithElements()
        {
            //Arrange
            var fakeList = new List<Equipment>
            {
                new Equipment { EquipmentId = 1, EquipmentName = "Notebook" }
            };
            _mockEquipmentService
                .GetEquipmentsAsync(10, 0, Arg.Any<EquipmentFilterDTO>())
                .Returns((fakeList, 1));

            //Act
            var result = await _equipmentController.GetEquipmentsAsync(10, 0, new EquipmentFilterDTO());

            //Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;

            okResult.Value.Should().BeEquivalentTo(new
            {
                EquipmentList = fakeList,
                EquipmentCount = 1,
                Errors = false
            });
        }

        [Fact]
        public async Task GetEquipmentsAsync_WhenServiceReturnsNull_ThenShoudBeReturnBadRequest()
        {
            //Arrange
            _mockEquipmentService
                .GetEquipmentsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<EquipmentFilterDTO>())
                .Returns(((List<Equipment>)null, 0));

            //Act
            var result = await _equipmentController.GetEquipmentsAsync(10, 0, new EquipmentFilterDTO());

            //Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().NotBeNull();            
        }

        [Fact]
        public async Task GetEquipmentWithLoansAsync_GivenValidEquipmentId_ThenShoudBeReturnOkResultStatus()
        {
            //Arrange
            var fakeList = new List<object>
            {
                new { EquipmentId = 1, EquipmentName = "Notebook", loans = new { } }
            };

            _mockEquipmentService
                .GetEquipmentWithLoansAsync(1)
                .Returns(fakeList);

            //Act
            var result = await _equipmentController.GetEquipmentWithLoansAsync(1);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
        }
        
        [Fact]
        public async Task GetEquipmentWithLoansAsync_GivenValidEquipmentId_ThenShoudBeReturnAnObjectWithLoans()
        {
            //Arrange
            var fakeList = new List<object>
            {
                new { EquipmentId = 1, EquipmentName = "Notebook", loans = new { } }
            };

            _mockEquipmentService
                .GetEquipmentWithLoansAsync(1)
                .Returns(fakeList);

            //Act
            var result = await _equipmentController.GetEquipmentWithLoansAsync(1);

            //Assert
            result.Should().NotBeNull();

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;

            okResult.Value.Should().BeEquivalentTo(fakeList);
        }

        [Fact]
        public async Task GetEquipmentWithLoansAsync_WhenServiceReturnsInvalidOperationException_ThenShoudBeReturnBadRequest()
        {
            //Arrange
            _mockEquipmentService
                .GetEquipmentWithLoansAsync(Arg.Any<int>())
                .Throws(new InvalidOperationException());

            //Act
            var result = await _equipmentController.GetEquipmentWithLoansAsync(1);

            //Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task PostEquipmentAsync_WhenServiceReturnsInvalidOperationException_ThenShoudBeReturnBadRequest()
        {
            //Arrange
            _mockEquipmentService
                .PostEquipmentAsync(Arg.Any<Equipment>())
                .Throws(new InvalidOperationException());

            Equipment equipment = new Equipment
            {

            };

            //Act
            var result = await _equipmentController.PostEquipmentAsync(equipment);

            //Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task PostEquipmentAsync_GivenValidEquipment_ThenShoudBeReturnAnCreatedAtActionResult()
        {
            //Arrange
            Equipment equipment = new()
            {
                EquipmentName = _faker.Lorem.Word(),
                IpAddress = _faker.Internet.Ip(),
                MacAddress = _faker.Internet.Mac(),
                QrCode = _faker.Lorem.Word(),
                EquipmentLoanStatus = _faker.Random.Bool(),
            };

            _mockEquipmentService
                .PostEquipmentAsync (equipment)
                .Returns(equipment);

            //Act
            var result = await _equipmentController.PostEquipmentAsync(equipment);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<CreatedAtActionResult>();
        }
        
        [Fact]
        public async Task PostEquipmentAsync_GivenValidEquipment_ThenShoudBeReturnAnEquipment()
        {
            //Arrange
            Equipment equipment = new()
            {
                EquipmentName = _faker.Lorem.Word(),
                IpAddress = _faker.Internet.Ip(),
                MacAddress = _faker.Internet.Mac(),
                QrCode = _faker.Lorem.Word(),
                EquipmentLoanStatus = _faker.Random.Bool(),
            };

            _mockEquipmentService
                .PostEquipmentAsync (equipment)
                .Returns(equipment);

            //Act
            var result = await _equipmentController.PostEquipmentAsync(equipment);

            //Assert
            result.Should().NotBeNull();

            var createdAtActionResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var response = createdAtActionResult.Value.Should().BeOfType<Response>().Subject;

            response.Return.Should().BeEquivalentTo(equipment);
        }

        [Fact]
        public async Task PutEquipmentAsync_WhenServiceReturnsInvalidOperationException_ThenShoudBeReturnBadRequest()
        {
            //Arrange
            _mockEquipmentService
                .PutEquipmentAsync(Arg.Any<int>(), Arg.Any<EquipmentUpdateDTO>())
                .Throws(new InvalidOperationException());

            EquipmentUpdateDTO update = new EquipmentUpdateDTO
            {

            };

            //Act
            var result = await _equipmentController.PutEquipmentAsync(_faker.Random.Int(), update);

            //Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task PutEquipmentAsync_WhenServiceReturnsKeyNotFoundException_ThenShoudBeReturnBadRequest()
        {
            //Arrange
            _mockEquipmentService
                .PutEquipmentAsync(Arg.Any<int>(), Arg.Any<EquipmentUpdateDTO>())
                .Throws(new KeyNotFoundException());

            EquipmentUpdateDTO update = new EquipmentUpdateDTO
            {

            };

            //Act
            var result = await _equipmentController.PutEquipmentAsync(_faker.Random.Int(), update);

            //Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task PutEquipmentAsync_GivenValidParameters_ThenShoudBeReturnAnNoContent()
        {
            //Arrange
            EquipmentUpdateDTO update = new()
            {
                EquipmentName = _faker.Lorem.Word(),
                IpAddress = _faker.Internet.Ip(),
                MacAddress = _faker.Internet.Mac(),
                EquipmentLoanStatus = _faker.Random.Bool(),
            };

            _mockEquipmentService
                .PutEquipmentAsync(Arg.Any<int>(), Arg.Any<EquipmentUpdateDTO>())
                .Returns(update);

            //Act
            var result = await _equipmentController.PutEquipmentAsync(_faker.Random.Int(), update);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task UpdateStatusEquipmentAsync_WhenServiceReturnsInvalidOperationException_ThenShoudBeReturnBadRequest()
        {
            //Arrange
            _mockEquipmentService
                .UpdateStatusEquipmentAsync(Arg.Any<List<int>>(), Arg.Any<bool>())
                .Throws(new InvalidOperationException());

            List<int> ids = [];

            //Act
            var result = await _equipmentController.UpdateStatusEquipmentAsync(ids, _faker.Random.Bool());

            //Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateStatusEquipmentAsync_GivenValidParameters_ThenShoudBeReturnAnNoContent()
        {
            //Arrange
            var fakeList = new List<Equipment>
            {
                new Equipment { EquipmentId = 1, EquipmentName = "Notebook" }
            };

            List<int> ids = Enumerable.Range(0, 10)
                .Select(_ => _faker.Random.Int(1, 100))
                .ToList();

            _mockEquipmentService
                .UpdateStatusEquipmentAsync(ids, _faker.Random.Bool())
                .Returns(fakeList);



            //Act
            var result = await _equipmentController.UpdateStatusEquipmentAsync(ids, _faker.Random.Bool());

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteEquipmentAsync_GivenValidId_ThenShoudBeReturnAnOkResult()
        {
            //Arrange
            Equipment equipment = new()
            {
                EquipmentName = _faker.Lorem.Word(),
                IpAddress = _faker.Internet.Ip(),
                MacAddress = _faker.Internet.Mac(),
                QrCode = _faker.Lorem.Word(),
                EquipmentLoanStatus = _faker.Random.Bool(),
            };

            _mockEquipmentService
                .DeleteEquipmentAsync(Arg.Any<int>())
                .Returns(equipment);

            //Act
            var result = await _equipmentController.DeleteEquipmentAsync(_faker.Random.Int());

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteEquipmentAsync_GivenValidId_ThenShoudBeReturnAnEquipment()
        {
            //Arrange
            Equipment equipment = new()
            {
                EquipmentName = _faker.Lorem.Word(),
                IpAddress = _faker.Internet.Ip(),
                MacAddress = _faker.Internet.Mac(),
                QrCode = _faker.Lorem.Word(),
                EquipmentLoanStatus = _faker.Random.Bool(),
            };

            _mockEquipmentService
                .DeleteEquipmentAsync(Arg.Any<int>())
                .Returns(equipment);

            //Act
            var result = await _equipmentController.DeleteEquipmentAsync(_faker.Random.Int());

            //Assert
            result.Should().NotBeNull();

            var okObjectResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okObjectResult.Value.Should().BeOfType<Response>().Subject;

            response.Return.Should().BeEquivalentTo(equipment);
        }

        [Fact]
        public async Task DeleteEquipmentAsync_WhenServiceReturnsInvalidOperationException_ThenShoudBeReturnBadRequest()
        {
            //Arrange
            _mockEquipmentService
                .DeleteEquipmentAsync(Arg.Any<int>())
                .Throws(new InvalidOperationException());           

            //Act
            var result = await _equipmentController.DeleteEquipmentAsync(_faker.Random.Int());

            //Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().NotBeNull();
        }
    }
}
