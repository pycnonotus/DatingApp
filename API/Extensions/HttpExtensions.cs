using System.Text.Json;
using API.Helpers;
using Microsoft.AspNetCore.Http;

namespace API.Extensions
{
    public static class HttpExtensions
    {
        public static void AddPagingHeader(this HttpResponse response, int currentPage, int itemsPerPage, int totalItems, int totalPages)
        {
            var pagingHeader = new PaginationHeader(
                currentPage, itemsPerPage, totalItems, totalPages
            );
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            response.Headers.Add("Pagination", JsonSerializer.Serialize(pagingHeader, options));
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");


        }
    }
}
