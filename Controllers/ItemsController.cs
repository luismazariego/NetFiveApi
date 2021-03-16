using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IITemsRepository _repository;

        public ItemsController(IITemsRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IEnumerable<ItemDto> GetItems()
        {
            return _repository.GetItems().Select(Item => Item.AsDto());
        }

        [HttpGet("{id}")]
        public IActionResult GetItem(string id)
        {
            var item = _repository.GetItem(Guid.Parse(id));

            if (item is null)
            {
                return BadRequest("Item with that Id does not exist");
            }

            return Ok(item.AsDto());
        }

        [HttpPost]
        public IActionResult CreateItem(CreateItemDto itemDto)
        {
            Item item = new()
            {
                Id = Guid.NewGuid(),
                Name = itemDto.Name,
                Price = itemDto.Price,
                CreatedDate = DateTimeOffset.UtcNow
            };

            _repository.CreateItem(item);

            return CreatedAtAction(
                nameof(GetItem), 
                new { id = item.Id }, 
                item.AsDto()
            );
        }

        [HttpPut("{id}")]
        public IActionResult UpdatedItem(Guid id, UpdateItemDto itemDto)
        {
            var existingItem = _repository.GetItem(id);

            if (existingItem is null) return BadRequest("Item does not exists");

            Item updatedItem = existingItem with
            {
                Name = itemDto.Name,
                Price = itemDto.Price
            };

            _repository.UpdateItem(updatedItem);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteItem(Guid id)
        {
            var existingItem = _repository.GetItem(id);

            if (existingItem is null) return BadRequest("Item does not exists");

            _repository.DeleteItem(id);

            return NoContent();
        }
    }
}