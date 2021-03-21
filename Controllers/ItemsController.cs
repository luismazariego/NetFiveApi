using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Dtos;
using Catalog.Entities;
using Catalog.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly IItemsRepository _repository;

        public ItemsController(IItemsRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IEnumerable<ItemDto>> GetItemsAsync()
        {
            return (await _repository.GetItemsAsync())
                .Select(Item => Item.AsDto());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetItemAsync(string id)
        {
            var item = await _repository.GetItemAsync(Guid.Parse(id));

            if (item is null)
            {
                return BadRequest("Item with that Id does not exist");
            }

            return Ok(item.AsDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateItemAsync(CreateItemDto itemDto)
        {
            Item item = new()
            {
                Id = Guid.NewGuid(),
                Name = itemDto.Name,
                Price = itemDto.Price,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await _repository.CreateItemAsync(item);

            return CreatedAtAction(
                nameof(GetItemAsync), 
                new { id = item.Id }, 
                item.AsDto()
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatedItemAsync(Guid id, 
            UpdateItemDto itemDto)
        {
            var existingItem = await _repository.GetItemAsync(id);

            if (existingItem is null) return BadRequest("Item does not exists");

            Item updatedItem = existingItem with
            {
                Name = itemDto.Name,
                Price = itemDto.Price
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