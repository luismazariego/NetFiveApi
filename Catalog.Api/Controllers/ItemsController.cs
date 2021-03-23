using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Api.Entities;
using Catalog.Api.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static Catalog.Api.Dtos.Dtos;

namespace Catalog.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly IItemsRepository _repository;
        private readonly ILogger<ItemsController> _logger;

        public ItemsController(IItemsRepository repository,
            ILogger<ItemsController> logger)
        {
            _logger = logger;
            _repository = repository;
        }

        [HttpGet]
        public async Task<IEnumerable<ItemDto>> GetItemsAsync(
            string nameToMatch = null)
        {
            var items = (await _repository.GetItemsAsync())
                .Select(Item => Item.AsDto());

            if(!string.IsNullOrWhiteSpace(nameToMatch))
            {
                items = items.Where(
                    x => x.Name.Contains(
                        nameToMatch, 
                        StringComparison.OrdinalIgnoreCase)
                );
            }

            _logger.LogInformation(
                $"{DateTime.UtcNow.ToString("hh:mm:ss")}: "+
                $"Retrieved {items.Count()} elements");

            return items;
        }        

        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetItemAsync(string id)
        {
            var item = await _repository.GetItemAsync(Guid.Parse(id));

            if (item is null)
            {
                return BadRequest("Item with that Id does not exist");
            }

            return item.AsDto();
        }

        [HttpPost]
        public async Task<ActionResult<ItemDto>> CreateItemAsync(CreateItemDto itemDto)
        {
            Item item = new()
            {
                Id = Guid.NewGuid(),
                Name = itemDto.Name,
                Price = itemDto.Price,
                CreatedDate = DateTimeOffset.UtcNow,
                Description = itemDto.Description
            };

            await _repository.CreateItemAsync(item);

            return CreatedAtAction(
                nameof(GetItemAsync),
                new { id = item.Id },
                item.AsDto()
            );
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdatedItemAsync(Guid id,
            UpdateItemDto itemDto)
        {
            var existingItem = await _repository.GetItemAsync(id);

            if (existingItem is null) return BadRequest("Item does not exists");

            Item updatedItem = existingItem with
            {
                Name = itemDto.Name,
                Price = itemDto.Price,
                Description = itemDto.Description
            };

            await _repository.UpdateItemAsync(updatedItem);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(Guid id)
        {
            var existingItem = await _repository.GetItemAsync(id);

            if (existingItem is null) return BadRequest("Item does not exists");

            await _repository.DeleteItemAsync(id);

            return NoContent();
        }

        
    }
}