using ClientNotifier.Core.Models;
using ClientNotifier.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ClientNotifier.Data
{
    public class DbInitializer
    {
        private readonly NotifierContext _context;
        private readonly ILogger<DbInitializer> _logger;
        private readonly IConfiguration _configuration;

        public DbInitializer(NotifierContext context, ILogger<DbInitializer> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InitializeAsync()
        {
            try
            {
                // Ensure database is created and all migrations are applied
                await _context.Database.MigrateAsync();
                _logger.LogInformation("Database migrations applied successfully");

                // Seed nameday data if not already present
                if (!await _context.NamedayMappings.AnyAsync())
                {
                    _logger.LogInformation("Seeding nameday data from JSON file...");
                    var namedays = await LoadNamedaysFromJsonAsync();
                    
                    if (namedays.Any())
                    {
                        await _context.NamedayMappings.AddRangeAsync(namedays);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Added {namedays.Count} nameday mappings");
                    }
                    else
                    {
                        _logger.LogWarning("No nameday data found in JSON file");
                    }
                }

                // Update namedays for existing people who don't have them
                var peopleWithoutNamedays = await _context.People
                    .Where(p => !p.Nameday.HasValue)
                    .ToListAsync();

                if (peopleWithoutNamedays.Any())
                {
                    _logger.LogInformation($"Updating namedays for {peopleWithoutNamedays.Count} people...");
                    var namedayMappings = await _context.NamedayMappings.ToListAsync();
                    var namedayService = new NamedayService(namedayMappings);

                    foreach (var person in peopleWithoutNamedays)
                    {
                        var nameday = namedayService.GetNamedayForPerson(person);
                        if (nameday.HasValue)
                        {
                            person.Nameday = nameday.Value;
                            _logger.LogInformation($"Set nameday for {person.FullName} to {nameday.Value:dd/MM}");
                        }
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the database");
                throw;
            }
        }

        private async Task<List<NamedayMapping>> LoadNamedaysFromJsonAsync()
        {
            var jsonFilePath = _configuration["DatabaseSettings:NamedayJsonPath"] ?? "Data/bulgarian-namedays.json";
            
            // Convert to absolute path if relative
            if (!Path.IsPathRooted(jsonFilePath))
            {
                jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, jsonFilePath);
            }
            
            if (!File.Exists(jsonFilePath))
            {
                _logger.LogError($"Nameday JSON file not found at: {jsonFilePath}");
                return new List<NamedayMapping>();
            }

            try
            {
                var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var namedayData = JsonSerializer.Deserialize<NamedayJsonRoot>(jsonContent, options);
                
                if (namedayData?.Namedays == null)
                {
                    _logger.LogError("Failed to deserialize nameday data");
                    return new List<NamedayMapping>();
                }

                return namedayData.Namedays.Select(n => new NamedayMapping
                {
                    Name = n.Name,
                    Month = n.Month,
                    Day = n.Day
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading nameday data from JSON");
                return new List<NamedayMapping>();
            }
        }

        public async Task SeedTestDataAsync()
        {
            // Only seed test data in development environment
            if (await _context.People.AnyAsync())
                return;

            var testPeople = new List<People>
            {
                new People
                {
                    FirstName = "Иван",
                    LastName = "Иванов",
                    EGN = "8001014509", // Born 01/01/1980, male
                    Birthday = new DateTime(1980, 1, 1),
                    Email = "ivan.ivanov@example.com",
                    PhoneNumber = "+359888123456",
                    Notes = "Test person 1"
                },
                new People
                {
                    FirstName = "Мария",
                    LastName = "Петрова",
                    EGN = "8508154502", // Born 15/08/1985, female
                    Birthday = new DateTime(1985, 8, 15),
                    Email = "maria.petrova@example.com",
                    PhoneNumber = "+359887654321",
                    Notes = "Test person 2"
                },
                new People
                {
                    FirstName = "Георги",
                    LastName = "Георгиев",
                    EGN = "9004234508", // Born 23/04/1990, male
                    Birthday = new DateTime(1990, 4, 23),
                    Email = "georgi@example.com",
                    Notes = "Has nameday on 23/04 (St. George's Day)"
                }
            };

            // Update namedays for test people
            var namedayMappings = await _context.NamedayMappings.ToListAsync();
            var namedayService = new NamedayService(namedayMappings);

            foreach (var person in testPeople)
            {
                var nameday = namedayService.GetNamedayForPerson(person);
                if (nameday.HasValue)
                {
                    person.Nameday = nameday.Value;
                }
            }

            await _context.People.AddRangeAsync(testPeople);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Added {testPeople.Count} test people");
        }

        private class NamedayJsonRoot
        {
            [JsonPropertyName("namedays")]
            public List<NamedayJsonItem> Namedays { get; set; } = new();
        }

        private class NamedayJsonItem
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;
            
            [JsonPropertyName("month")]
            public int Month { get; set; }
            
            [JsonPropertyName("day")]
            public int Day { get; set; }
        }
    }
}
