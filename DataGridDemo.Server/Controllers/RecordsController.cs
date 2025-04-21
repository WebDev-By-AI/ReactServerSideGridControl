using DataGridDemo.Server.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq.Dynamic.Core;

namespace DataGridDemo.Server.Controllers
{
    // Your ASP.NET Core Controller
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class RecordsController : ControllerBase
    {

        public static List<User> Users
        {
            get
            {

                return new List<User>
                        {
                            new User { Id = 1, Name = "Khurram", Email = "khurram@gmail.com" },
                            new User { Id = 2, Name = "Ali", Email = "ali@example.com" },
                            new User { Id = 3, Name = "Sara", Email = "sara@example.com" },
                            new User { Id = 4, Name = "Ahmed", Email = "ahmed@example.com" },
                            new User { Id = 5, Name = "Zainab", Email = "zainab@example.com" },
                            new User { Id = 6, Name = "Hassan", Email = "hassan@example.com" },
                            new User { Id = 7, Name = "Fatima", Email = "fatima@example.com" },
                            new User { Id = 8, Name = "Bilal", Email = "bilal@example.com" },
                            new User { Id = 9, Name = "Ayesha", Email = "ayesha@example.com" },
                            new User { Id = 10, Name = "Omar", Email = "omar@example.com" },
                            new User { Id = 11, Name = "Yasmin", Email = "yasmin@example.com" },
                            new User { Id = 12, Name = "Samir", Email = "samir@example.com" },
                            new User { Id = 13, Name = "Lubna", Email = "lubna@example.com" },
                            new User { Id = 14, Name = "Ibrahim", Email = "ibrahim@example.com" },
                            new User { Id = 15, Name = "Mariam", Email = "mariam@example.com" },
                            new User { Id = 16, Name = "Usman", Email = "usman@example.com" },
                            new User { Id = 17, Name = "Rania", Email = "rania@example.com" },
                            new User { Id = 18, Name = "Fahad", Email = "fahad@example.com" },
                            new User { Id = 19, Name = "Nadia", Email = "nadia@example.com" },
                            new User { Id = 20, Name = "Talha", Email = "talha@example.com" },
                            new User { Id = 21, Name = "Zara", Email = "zara@example.com" },
                            new User { Id = 22, Name = "Kamal", Email = "kamal@example.com" },
                            new User { Id = 23, Name = "Aminah", Email = "aminah@example.com" },
                            new User { Id = 24, Name = "Saad", Email = "saad@example.com" },
                            new User { Id = 25, Name = "Hiba", Email = "hiba@example.com" },
                            new User { Id = 26, Name = "Tariq", Email = "tariq@example.com" },
                            new User { Id = 27, Name = "Shazia", Email = "shazia@example.com" },
                            new User { Id = 28, Name = "Adnan", Email = "adnan@example.com" },
                            new User { Id = 29, Name = "Laila", Email = "laila@example.com" },
                            new User { Id = 30, Name = "Farhan", Email = "farhan@example.com" }
                        };


            }
        }

        /// <summary>
        /// Gets paginated, filtered, and sorted user data.
        /// </summary>
        /// <param name="page">Optional - default value 1</param>
        /// <param name="pageSize">Optional - default value 10</param>
        /// <param name="filters">Optional - JSON string for filters</param>
        /// <param name="sortField">Optional - Field to sort by</param>
        /// <param name="sortDirection">Optional - Sort direction ('asc' or 'desc') - default 'asc'</param>
        /// <returns>Paginated, filtered, and sorted data</returns>
        [HttpGet("GetPaginatedData")]
        public async Task<ActionResult> GetPaginatedData(
            int page = 1,
            int pageSize = 10,
            string filters = null,
            string sortField = null, // <-- Add sort field parameter
            string sortDirection = "asc" // <-- Add sort direction parameter (default 'asc')
        )
        {
            var query = Users.AsQueryable();

            // Apply filters if they exist
            if (!string.IsNullOrEmpty(filters))
            {
                var filterItems = JsonSerializer.Deserialize<List<FilterItem>>(filters);
                foreach (var filter in filterItems)
                {
                    switch (filter.columnField)
                    {
                        case "id":
                            if (int.TryParse(filter.value, out int idValue))
                            {
                                if (filter.operatorValue == "equals")
                                    query = query.Where(u => u.Id == idValue);
                                else if (filter.operatorValue == "contains")
                                    query = query.Where(u => u.Id.ToString().Contains(idValue.ToString()));
                            }
                            break;
                        case "name":
                            if (filter.operatorValue == "equals")
                                query = query.Where(u => u.Name.Equals(filter.value, StringComparison.OrdinalIgnoreCase));
                            else if (filter.operatorValue == "contains")
                                query = query.Where(u => u.Name.Contains(filter.value, StringComparison.OrdinalIgnoreCase));
                            break;
                        case "email":
                            if (filter.operatorValue == "equals")
                                query = query.Where(u => u.Email.Equals(filter.value, StringComparison.OrdinalIgnoreCase));
                            else if (filter.operatorValue == "contains")
                                query = query.Where(u => u.Email.Contains(filter.value, StringComparison.OrdinalIgnoreCase));
                            break;
                    }
                }
            }


            // Apply Sorting - BEFORE Pagination
            if (!string.IsNullOrWhiteSpace(sortField))
            {
                // Basic validation for sort field name
                var validSortColumns = new[] { "id", "name", "email" }; // Match your User properties (case-insensitive is fine here)
                if (validSortColumns.Contains(sortField, StringComparer.OrdinalIgnoreCase))
                {
                    string sortOrder = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";
                    try
                    {
                       string serverSortField = GetServerSortField(sortField); // Helper function for safety/mapping
                       query = query.OrderBy($"{serverSortField} {sortOrder}");
                    }
                    catch (Exception ex)
                    {
                        // Log error if sorting fails (e.g., invalid field name after check)
                        Console.WriteLine($"Error applying sorting: {ex.Message}");
                        // Potentially ignore sorting or return an error
                    }
                }
                else
                {
                    Console.WriteLine($"Invalid sort field requested: {sortField}");
                    // Ignore invalid sort field
                }
            }
            else
            {
                // Default sort order if none is specified (optional, but good practice)
                query = query.OrderBy(u => u.Id); // Default sort by Id ascending
            }

            //Execute the buuild query 
            var totalCount = query.Count();
            var skip = (page - 1) * pageSize;
            var data = query.Skip(skip).Take(pageSize).ToList();

            return Ok(new
            {
                data,
                totalCount
            });
        }

          // Helper function to map client field names to server property names
    // And provide basic validation/sanitization
    private string GetServerSortField(string clientField)
    {
        // Use a dictionary for cleaner mapping and case-insensitivity
        var columnMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "id","Id" },
            { "name","Name" },
            { "email","Email" }
        };

        if (columnMapping.TryGetValue(clientField, out var serverField))
        {
            return serverField;
        }

        // Fallback or throw exception if mapping not found
        // For safety, fallback to a default or return null/empty to prevent OrderBy failure
        Console.WriteLine($"Warning: Unmapped sort field '{clientField}'. Defaulting to Id.");
        return "Id"; // Or return null and handle it in the calling code
    }
}
    


    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }


    // Add this class to handle filter deserialization
    public class FilterItem
    {
        [JsonPropertyName("field")]
        public string columnField { get; set; }
        [JsonPropertyName("operator")]
        public string operatorValue { get; set; }

        public string value { get; set; }
    }



}


