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
    public class PeopleController : ControllerBase
    {
        private readonly NotifierContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PeopleController> _logger;

        public PeopleController(NotifierContext context, IMapper mapper, ILogger<PeopleController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all people with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PersonListDto>>> GetPeople(
            [FromQuery] string? search = null,
            [FromQuery] bool? birthdayToday = null,
            [FromQuery] bool? namedayToday = null,
            [FromQuery] bool? celebrationsThisWeek = null)
        {
            try
            {
                var query = _context.People.AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(p => 
                        p.FirstName.ToLower().Contains(search) ||
                        (p.LastName != null && p.LastName.ToLower().Contains(search)) ||
                        p.EGN.Contains(search) ||
                        (p.Email != null && p.Email.ToLower().Contains(search)) ||
                        (p.PhoneNumber != null && p.PhoneNumber.Contains(search)));
                }

                // Apply birthday today filter
                if (birthdayToday == true)
                {
                    var today = DateTime.Today;
                    query = query.Where(p => p.Birthday.Month == today.Month && p.Birthday.Day == today.Day);
                }

                // Apply nameday today filter
                if (namedayToday == true)
                {
                    var today = DateTime.Today;
                    query = query.Where(p => p.Nameday.HasValue && 
                        p.Nameday.Value.Month == today.Month && 
                        p.Nameday.Value.Day == today.Day);
                }

                var people = await query.OrderBy(p => p.FirstName).ThenBy(p => p.LastName).ToListAsync();
                var peopleDtos = _mapper.Map<List<PersonListDto>>(people);

                // Filter by celebrations this week if requested
                if (celebrationsThisWeek == true)
                {
                    peopleDtos = peopleDtos.Where(p => 
                        p.HasBirthdayThisWeek || p.HasNamedayThisWeek).ToList();
                }

                return Ok(peopleDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving people");
                return StatusCode(500, "An error occurred while retrieving people");
            }
        }

        /// <summary>
        /// Get a specific person by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PersonDto>> GetPerson(int id)
        {
            try
            {
                var person = await _context.People.FindAsync(id);

                if (person == null)
                {
                    return NotFound($"Person with ID {id} not found");
                }

                var personDto = _mapper.Map<PersonDto>(person);
                return Ok(personDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving person {PersonId}", id);
                return StatusCode(500, "An error occurred while retrieving the person");
            }
        }

        /// <summary>
        /// Create a new person
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PersonDto>> CreatePerson(CreatePersonDto createPersonDto)
        {
            try
            {
                // Validate EGN
                if (!EgnUtils.IsValidEgn(createPersonDto.EGN))
                {
                    return BadRequest("Invalid EGN format or checksum");
                }

                // Check if EGN already exists
                if (await _context.People.AnyAsync(p => p.EGN == createPersonDto.EGN))
                {
                    return Conflict($"A person with EGN {createPersonDto.EGN} already exists");
                }

                var person = _mapper.Map<People>(createPersonDto);
                
                // Extract birthday from EGN
                person.Birthday = EgnUtils.ExtractBirthday(createPersonDto.EGN);
                
                // Try to assign nameday
                var namedayMappings = await _context.NamedayMappings.ToListAsync();
                var namedayService = new NamedayService(namedayMappings);
                person.Nameday = namedayService.GetNamedayForPerson(person);

                _context.People.Add(person);
                await _context.SaveChangesAsync();

                var personDto = _mapper.Map<PersonDto>(person);
                return CreatedAtAction(nameof(GetPerson), new { id = person.Id }, personDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating person");
                return StatusCode(500, "An error occurred while creating the person");
            }
        }

        /// <summary>
        /// Update an existing person
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<PersonDto>> UpdatePerson(int id, UpdatePersonDto updatePersonDto)
        {
            if (id != updatePersonDto.Id)
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                var person = await _context.People.FindAsync(id);
                if (person == null)
                {
                    return NotFound($"Person with ID {id} not found");
                }

                // Validate EGN if it changed
                if (person.EGN != updatePersonDto.EGN)
                {
                    if (!EgnUtils.IsValidEgn(updatePersonDto.EGN))
                    {
                        return BadRequest("Invalid EGN format or checksum");
                    }

                    // Check if new EGN already exists
                    if (await _context.People.AnyAsync(p => p.EGN == updatePersonDto.EGN && p.Id != id))
                    {
                        return Conflict($"A person with EGN {updatePersonDto.EGN} already exists");
                    }
                }

                // Update fields
                person.FirstName = updatePersonDto.FirstName;
                person.LastName = updatePersonDto.LastName;
                person.Email = updatePersonDto.Email;
                person.PhoneNumber = updatePersonDto.PhoneNumber;
                person.Notes = updatePersonDto.Notes;
                person.NotificationsEnabled = updatePersonDto.NotificationsEnabled;

                // Update EGN and birthday if EGN changed
                if (person.EGN != updatePersonDto.EGN)
                {
                    person.EGN = updatePersonDto.EGN;
                    person.Birthday = EgnUtils.ExtractBirthday(updatePersonDto.EGN);
                }

                // Update nameday based on new first name
                var namedayMappings = await _context.NamedayMappings.ToListAsync();
                var namedayService = new NamedayService(namedayMappings);
                person.Nameday = namedayService.GetNamedayForPerson(person);

                await _context.SaveChangesAsync();

                var personDto = _mapper.Map<PersonDto>(person);
                return Ok(personDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating person {PersonId}", id);
                return StatusCode(500, "An error occurred while updating the person");
            }
        }

        /// <summary>
        /// Delete a person
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePerson(int id)
        {
            try
            {
                var person = await _context.People.FindAsync(id);
                if (person == null)
                {
                    return NotFound($"Person with ID {id} not found");
                }

                _context.People.Remove(person);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting person {PersonId}", id);
                return StatusCode(500, "An error occurred while deleting the person");
            }
        }

        /// <summary>
        /// Get people with celebrations (birthdays or namedays) in the next N days
        /// </summary>
        [HttpGet("upcoming-celebrations")]
        public async Task<ActionResult<IEnumerable<PersonListDto>>> GetUpcomingCelebrations([FromQuery] int days = 7)
        {
            try
            {
                if (days < 1 || days > 365)
                {
                    return BadRequest("Days must be between 1 and 365");
                }

                var people = await _context.People
                    .Where(p => p.NotificationsEnabled)
                    .ToListAsync();

                var peopleDtos = _mapper.Map<List<PersonListDto>>(people);
                
                // Filter to only those with celebrations in the specified period
                var today = DateTime.Today;
                var endDate = today.AddDays(days);

                var upcomingCelebrations = peopleDtos.Where(p =>
                {
                    // Check birthday
                    var birthdayThisYear = new DateTime(today.Year, p.Birthday.Month, p.Birthday.Day);
                    if (birthdayThisYear < today)
                        birthdayThisYear = birthdayThisYear.AddYears(1);
                    
                    var hasBirthdayInPeriod = birthdayThisYear >= today && birthdayThisYear <= endDate;

                    // Check nameday
                    var hasNamedayInPeriod = false;
                    if (p.Nameday.HasValue)
                    {
                        var namedayThisYear = new DateTime(today.Year, p.Nameday.Value.Month, p.Nameday.Value.Day);
                        if (namedayThisYear < today)
                            namedayThisYear = namedayThisYear.AddYears(1);
                        
                        hasNamedayInPeriod = namedayThisYear >= today && namedayThisYear <= endDate;
                    }

                    return hasBirthdayInPeriod || hasNamedayInPeriod;
                })
                .OrderBy(p => 
                {
                    // Order by next celebration date
                    var birthdayThisYear = new DateTime(today.Year, p.Birthday.Month, p.Birthday.Day);
                    if (birthdayThisYear < today)
                        birthdayThisYear = birthdayThisYear.AddYears(1);
                    
                    var nextCelebration = birthdayThisYear;
                    
                    if (p.Nameday.HasValue)
                    {
                        var namedayThisYear = new DateTime(today.Year, p.Nameday.Value.Month, p.Nameday.Value.Day);
                        if (namedayThisYear < today)
                            namedayThisYear = namedayThisYear.AddYears(1);
                        
                        if (namedayThisYear < nextCelebration)
                            nextCelebration = namedayThisYear;
                    }
                    
                    return nextCelebration;
                })
                .ToList();

                return Ok(upcomingCelebrations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming celebrations");
                return StatusCode(500, "An error occurred while retrieving upcoming celebrations");
            }
        }
    }
}
