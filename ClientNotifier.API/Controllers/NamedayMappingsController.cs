using AutoMapper;
using ClientNotifier.Core.DTOs;
using ClientNotifier.Core.Models;
using ClientNotifier.Core.Services;
using ClientNotifier.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientNotifier.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NamedayMappingsController : ControllerBase
    {
        private readonly NotifierContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<NamedayMappingsController> _logger;

        public NamedayMappingsController(NotifierContext context, IMapper mapper, ILogger<NamedayMappingsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all nameday mappings with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NamedayMappingDto>>> GetNamedayMappings(
            [FromQuery] string? name = null,
            [FromQuery] int? month = null,
            [FromQuery] int? day = null)
        {
            try
            {
                var query = _context.NamedayMappings.AsQueryable();

                // Apply name filter
                if (!string.IsNullOrWhiteSpace(name))
                {
                    query = query.Where(n => n.Name.ToLower().Contains(name.ToLower()));
                }

                // Apply month filter
                if (month.HasValue)
                {
                    if (month.Value < 1 || month.Value > 12)
                    {
                        return BadRequest("Month must be between 1 and 12");
                    }
                    query = query.Where(n => n.Month == month.Value);
                }

                // Apply day filter
                if (day.HasValue)
                {
                    if (day.Value < 1 || day.Value > 31)
                    {
                        return BadRequest("Day must be between 1 and 31");
                    }
                    query = query.Where(n => n.Day == day.Value);
                }

                var mappings = await query
                    .OrderBy(n => n.Month)
                    .ThenBy(n => n.Day)
                    .ThenBy(n => n.Name)
                    .ToListAsync();

                var mappingDtos = _mapper.Map<List<NamedayMappingDto>>(mappings);
                return Ok(mappingDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving nameday mappings");
                return StatusCode(500, "An error occurred while retrieving nameday mappings");
            }
        }

        /// <summary>
        /// Get a specific nameday mapping by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<NamedayMappingDto>> GetNamedayMapping(int id)
        {
            try
            {
                var mapping = await _context.NamedayMappings.FindAsync(id);

                if (mapping == null)
                {
                    return NotFound($"Nameday mapping with ID {id} not found");
                }

                var mappingDto = _mapper.Map<NamedayMappingDto>(mapping);
                return Ok(mappingDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving nameday mapping {MappingId}", id);
                return StatusCode(500, "An error occurred while retrieving the nameday mapping");
            }
        }

        /// <summary>
        /// Create a new nameday mapping
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<NamedayMappingDto>> CreateNamedayMapping(CreateNamedayMappingDto createDto)
        {
            try
            {
                // Validate date
                try
                {
                    var testDate = new DateTime(2024, createDto.Month, createDto.Day);
                }
                catch
                {
                    return BadRequest($"Invalid date: {createDto.Day}/{createDto.Month}");
                }

                // Check if name already has a nameday on this date
                var exists = await _context.NamedayMappings.AnyAsync(n => 
                    n.Name.ToLower() == createDto.Name.ToLower() &&
                    n.Month == createDto.Month &&
                    n.Day == createDto.Day);

                if (exists)
                {
                    return Conflict($"Nameday for {createDto.Name} on {createDto.Day}/{createDto.Month} already exists");
                }

                var mapping = _mapper.Map<NamedayMapping>(createDto);
                _context.NamedayMappings.Add(mapping);
                await _context.SaveChangesAsync();

                // Update namedays for all people with this name
                await UpdatePeopleNamedays(createDto.Name);

                var mappingDto = _mapper.Map<NamedayMappingDto>(mapping);
                return CreatedAtAction(nameof(GetNamedayMapping), new { id = mapping.Id }, mappingDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating nameday mapping");
                return StatusCode(500, "An error occurred while creating the nameday mapping");
            }
        }

        /// <summary>
        /// Update an existing nameday mapping
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<NamedayMappingDto>> UpdateNamedayMapping(int id, UpdateNamedayMappingDto updateDto)
        {
            if (id != updateDto.Id)
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                var mapping = await _context.NamedayMappings.FindAsync(id);
                if (mapping == null)
                {
                    return NotFound($"Nameday mapping with ID {id} not found");
                }

                // Validate date
                try
                {
                    var testDate = new DateTime(2024, updateDto.Month, updateDto.Day);
                }
                catch
                {
                    return BadRequest($"Invalid date: {updateDto.Day}/{updateDto.Month}");
                }

                var oldName = mapping.Name;

                // Update mapping
                mapping.Name = updateDto.Name;
                mapping.Month = updateDto.Month;
                mapping.Day = updateDto.Day;

                await _context.SaveChangesAsync();

                // Update namedays for affected people
                if (oldName != updateDto.Name)
                {
                    await UpdatePeopleNamedays(oldName);
                }
                await UpdatePeopleNamedays(updateDto.Name);

                var mappingDto = _mapper.Map<NamedayMappingDto>(mapping);
                return Ok(mappingDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating nameday mapping {MappingId}", id);
                return StatusCode(500, "An error occurred while updating the nameday mapping");
            }
        }

        /// <summary>
        /// Delete a nameday mapping
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNamedayMapping(int id)
        {
            try
            {
                var mapping = await _context.NamedayMappings.FindAsync(id);
                if (mapping == null)
                {
                    return NotFound($"Nameday mapping with ID {id} not found");
                }

                var affectedName = mapping.Name;

                _context.NamedayMappings.Remove(mapping);
                await _context.SaveChangesAsync();

                // Update namedays for affected people
                await UpdatePeopleNamedays(affectedName);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting nameday mapping {MappingId}", id);
                return StatusCode(500, "An error occurred while deleting the nameday mapping");
            }
        }

        /// <summary>
        /// Get nameday mappings grouped by date
        /// </summary>
        [HttpGet("grouped-by-date")]
        public async Task<ActionResult<IEnumerable<NamedayGroupDto>>> GetNamedaysGroupedByDate([FromQuery] int? month = null)
        {
            try
            {
                var query = _context.NamedayMappings.AsQueryable();

                if (month.HasValue)
                {
                    if (month.Value < 1 || month.Value > 12)
                    {
                        return BadRequest("Month must be between 1 and 12");
                    }
                    query = query.Where(n => n.Month == month.Value);
                }

                var mappings = await query.ToListAsync();

                var grouped = mappings
                    .GroupBy(n => new { n.Month, n.Day })
                    .Select(g => new NamedayGroupDto
                    {
                        Month = g.Key.Month,
                        Day = g.Key.Day,
                        DateDisplay = $"{g.Key.Day:D2}/{g.Key.Month:D2}",
                        Names = g.Select(n => n.Name).OrderBy(n => n).ToList(),
                        PeopleCount = 0 // Will be updated below
                    })
                    .OrderBy(g => g.Month)
                    .ThenBy(g => g.Day)
                    .ToList();

                // Add people count for each date
                foreach (var group in grouped)
                {
                    group.PeopleCount = await _context.People
                        .CountAsync(p => p.Nameday.HasValue && 
                                        p.Nameday.Value.Month == group.Month && 
                                        p.Nameday.Value.Day == group.Day);
                }

                return Ok(grouped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving grouped namedays");
                return StatusCode(500, "An error occurred while retrieving grouped namedays");
            }
        }

        /// <summary>
        /// Get today's namedays
        /// </summary>
        [HttpGet("today")]
        public async Task<ActionResult<NamedayGroupDto>> GetTodaysNamedays()
        {
            try
            {
                var today = DateTime.Today;
                var todaysNamedays = await _context.NamedayMappings
                    .Where(n => n.Month == today.Month && n.Day == today.Day)
                    .OrderBy(n => n.Name)
                    .ToListAsync();

                var result = new NamedayGroupDto
                {
                    Month = today.Month,
                    Day = today.Day,
                    DateDisplay = today.ToString("dd/MM"),
                    Names = todaysNamedays.Select(n => n.Name).ToList(),
                    PeopleCount = await _context.People
                        .CountAsync(p => p.Nameday.HasValue && 
                                        p.Nameday.Value.Month == today.Month && 
                                        p.Nameday.Value.Day == today.Day)
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving today's namedays");
                return StatusCode(500, "An error occurred while retrieving today's namedays");
            }
        }

        /// <summary>
        /// Import namedays from JSON
        /// </summary>
        [HttpPost("import")]
        public async Task<ActionResult> ImportNamedays([FromBody] List<CreateNamedayMappingDto> namedays)
        {
            if (namedays == null || !namedays.Any())
            {
                return BadRequest("No namedays provided for import");
            }

            try
            {
                var importedCount = 0;
                var skippedCount = 0;
                var errors = new List<string>();

                foreach (var namedayDto in namedays)
                {
                    try
                    {
                        // Validate date
                        var testDate = new DateTime(2024, namedayDto.Month, namedayDto.Day);

                        // Check if already exists
                        var exists = await _context.NamedayMappings.AnyAsync(n =>
                            n.Name.ToLower() == namedayDto.Name.ToLower() &&
                            n.Month == namedayDto.Month &&
                            n.Day == namedayDto.Day);

                        if (exists)
                        {
                            skippedCount++;
                            continue;
                        }

                        var mapping = _mapper.Map<NamedayMapping>(namedayDto);
                        _context.NamedayMappings.Add(mapping);
                        importedCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error importing {namedayDto.Name} ({namedayDto.Day}/{namedayDto.Month}): {ex.Message}");
                    }
                }

                if (importedCount > 0)
                {
                    await _context.SaveChangesAsync();

                    // Update all people's namedays
                    await UpdateAllPeopleNamedays();
                }

                return Ok(new
                {
                    imported = importedCount,
                    skipped = skippedCount,
                    errors = errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing namedays");
                return StatusCode(500, "An error occurred while importing namedays");
            }
        }

        #region Private Methods

        private async Task UpdatePeopleNamedays(string name)
        {
            var people = await _context.People
                .Where(p => p.FirstName.ToLower() == name.ToLower())
                .ToListAsync();

            if (people.Any())
            {
                var namedayMappings = await _context.NamedayMappings.ToListAsync();
                var namedayService = new NamedayService(namedayMappings);

                foreach (var person in people)
                {
                    person.Nameday = namedayService.GetNamedayForPerson(person);
                }

                await _context.SaveChangesAsync();
            }
        }

        private async Task UpdateAllPeopleNamedays()
        {
            var people = await _context.People.ToListAsync();
            var namedayMappings = await _context.NamedayMappings.ToListAsync();
            var namedayService = new NamedayService(namedayMappings);

            foreach (var person in people)
            {
                person.Nameday = namedayService.GetNamedayForPerson(person);
            }

            await _context.SaveChangesAsync();
        }

        #endregion
    }
}
