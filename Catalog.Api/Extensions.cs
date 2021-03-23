
using Catalog.Api.Entities;
using static Catalog.Api.Dtos.Dtos;

namespace Catalog.Api
{
    public static class Extensions
    {
        public static ItemDto AsDto(this Item item)
        {
            return new ItemDto
            (
                Id: item.Id,
                CreatedDate: item.CreatedDate,
                Name: item.Name,
                Price: item.Price,
                Description: item.Description
            );
        }
    }
}