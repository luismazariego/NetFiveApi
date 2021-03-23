using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Catalog.Api.Controllers;
using Catalog.Api.Entities;
using Catalog.Api.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static Catalog.Api.Dtos.Dtos;

namespace Catalog.UnitTests
{
    public class ItemsControllerTest
    {       
        private readonly Mock<IItemsRepository> _repoStub = new();
        private readonly Mock<ILogger<ItemsController>> _loggerStub = new();

        //standar name convention: UnitOfWork_StateUnderTest_ExpectedBehavior
        [Fact]
        public async Task GetItemAsync_WithUnexistingItem_ReturnNotFound()
        {
            //Arrange: Set up everything (input, mocks, var etc)
            _repoStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Item)null);

            ItemsController controller = 
                new (_repoStub.Object,_loggerStub.Object);

            //Act: Execute test
            var result = await controller.GetItemAsync(Guid.NewGuid().ToString());

            //Assert: Verify results
            //Assert.IsType<BadRequestObjectResult>(result.Result);
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetItemAsync_WithExistingItem_ReturnsExpectedItem()
        {
            //Arrange
            var expectedItem = CreateRandomItem();

            _repoStub.Setup(r => r.GetItemAsync(It.IsAny<Guid>()))
                .ReturnsAsync(expectedItem);
            
            ItemsController controller =
                new(_repoStub.Object, _loggerStub.Object);

            //act
            var result = await controller.GetItemAsync(Guid.NewGuid().ToString());

            //Assert
            result.Value.Should().BeEquivalentTo(
                expectedItem,
                options=>options.ComparingByMembers<Item>());

            
            // Assert.IsType<ItemDto>(result.Value);
            // var dto = (result as ActionResult<ItemDto>).Value;
            // Assert.Equal(expectedItem.Id, dto.Id);
            // Assert.Equal(expectedItem.Name, dto.Name);
        }

        [Fact]
        public async Task GetItemsAsync_WithExistingItems_ReturnsAllItems()
        {
            //Arrange
            var expectedItems = new[] { 
                CreateRandomItem(), 
                CreateRandomItem(), 
                CreateRandomItem() };

            _repoStub.Setup(r => r.GetItemsAsync()).ReturnsAsync(expectedItems);

            ItemsController controller = new(_repoStub.Object, _loggerStub.Object);

            //Act
            var actualItems = await controller.GetItemsAsync();

            //Assert
            actualItems.Should().BeEquivalentTo(
                expectedItems,
                opt=>opt.ComparingByMembers<Item>());
        }

        [Fact]
        public async Task GetItemsAsync_WithMatchingItems_ReturnsMatchingItems()
        {
            //Arrange
            var allItems = new[] 
            {
                new Item(){Name="Potion"},
                new Item(){Name="Antidote"},
                new Item(){Name="Hi-Potion"}                
            };

            string nameToMatch = "Potion";

            _repoStub.Setup(r => r.GetItemsAsync()).ReturnsAsync(allItems);

            ItemsController controller = new(_repoStub.Object, _loggerStub.Object);

            //Act
            IEnumerable<ItemDto> foundItems = await controller
                .GetItemsAsync(nameToMatch);

            //Assert
            foundItems
                .Should()
                .OnlyContain(item => 
                    item.Name == allItems[0].Name ||
                    item.Name == allItems[2].Name);
        }

        [Fact]
        public async Task CreateItemAsync_WithItemToCreate_ReturnsCreatedItem()
        {
            Random rand = new();

            //Arrange
            var itemToCreate = new CreateItemDto
            (
                Name: Guid.NewGuid().ToString(),
                Price: rand.Next(1000),
                Description: Guid.NewGuid().ToString()
            );

            ItemsController controller = new(_repoStub.Object, _loggerStub.Object);

            //Act
            var result = await controller.CreateItemAsync(itemToCreate);
        
            //Assert
            var createdItem = (result.Result as CreatedAtActionResult)
                .Value as ItemDto;

            itemToCreate.Should().BeEquivalentTo(
                createdItem,
                options => options.ComparingByMembers<ItemDto>()
                    .ExcludingMissingMembers()
            );

            createdItem.Id
                .Should()
                .NotBeEmpty();

            createdItem.CreatedDate
                .Should()
                .BeCloseTo(DateTimeOffset.UtcNow, 1000);
        }

        [Fact]
        public async Task UpdateItemAsync_WithExistingItem_ReturnsNoContent()
        {
            //Arrange
            Item existingItem = CreateRandomItem();

            _repoStub.Setup(r => r.GetItemAsync(It.IsAny<Guid>()))
                .ReturnsAsync(existingItem);

            UpdateItemDto itemToUpdate = new (
                Name: Guid.NewGuid().ToString(),
                Price: existingItem.Price + 3,
                Description: Guid.NewGuid().ToString()
            );

            ItemsController controller = new(_repoStub.Object, _loggerStub.Object);

            //Act
            var result = await controller.UpdatedItemAsync(
                existingItem.Id, itemToUpdate);

            //Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteItemAsync_WithExistingItem_ReturnsNoContent()
        {
            //Arrange
            Item existingItem = CreateRandomItem();

            _repoStub.Setup(r => r.GetItemAsync(It.IsAny<Guid>()))
                .ReturnsAsync(existingItem);

            ItemsController controller = new(_repoStub.Object, _loggerStub.Object);

            //Act
            var result = await controller.DeleteItem(existingItem.Id);

            //Assert
            result.Should().BeOfType<NoContentResult>();
        }
        
        private Item CreateRandomItem()
        {
            Random rand = new();

            return new Item
            {
                Id = Guid.NewGuid(),
                Name = Guid.NewGuid().ToString(),
                Price = rand.Next(1000),
                CreatedDate = DateTimeOffset.UtcNow,
                Description = Guid.NewGuid().ToString()
            };
        }
    }
}
