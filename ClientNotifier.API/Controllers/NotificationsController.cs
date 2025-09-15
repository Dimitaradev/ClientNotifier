using ClientNotifier.Core.Models;
using ClientNotifier.API.Services;
using ClientNotifier.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientNotifier.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly NotifierContext _context;
        private readonly EmailService _emailService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(NotifierContext context, EmailService emailService, IWebHostEnvironment env, ILogger<NotificationsController> logger)
        {
            _context = context;
            _emailService = emailService;
            _env = env;
            _logger = logger;
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetToday()
        {
            var today = DateTime.Today;

            var people = await _context.People.ToListAsync();

            var birthdays = people.Where(p => p.Birthday.Month == today.Month && p.Birthday.Day == today.Day).ToList();
            var namedays = people.Where(p => p.Nameday.HasValue && p.Nameday.Value.Month == today.Month && p.Nameday.Value.Day == today.Day).ToList();

            var sentEmailIds = await _context.NotificationLogs
                .Where(n => n.Channel == NotificationChannel.Email && n.SentAtUtc >= today && n.SentAtUtc < today.AddDays(1))
                .Select(n => new { n.PersonId, n.Type })
                .ToListAsync();

            return Ok(new
            {
                birthdays = birthdays.Select(p => new {
                    p.Id, p.FirstName, p.LastName, p.Email, p.FullName,
                    alreadySent = sentEmailIds.Any(s => s.PersonId == p.Id && s.Type == NotificationType.Birthday)
                }),
                namedays = namedays.Select(p => new {
                    p.Id, p.FirstName, p.LastName, p.Email, p.FullName,
                    alreadySent = sentEmailIds.Any(s => s.PersonId == p.Id && s.Type == NotificationType.Nameday)
                })
            });
        }

        public record PreviewRequest(int PersonId, string Type);

        [HttpPost("preview")]
        public async Task<IActionResult> Preview([FromBody] PreviewRequest req)
        {
            var person = await _context.People.FindAsync(req.PersonId);
            if (person == null) return NotFound("Person not found");

            var type = req.Type?.ToLower();
            if (type != "birthday" && type != "nameday") return BadRequest("Type must be 'birthday' or 'nameday'");

            var templatesDir = Path.Combine(_env.ContentRootPath, "Data", "templates");
            var templatePath = Path.Combine(templatesDir, type == "birthday" ? "birthday.html" : "nameday.html");
            var subject = type == "birthday" ? $"Честит рожден ден, {person.FirstName}!" : $"Честит имен ден, {person.FirstName}!";
            var body = _emailService.RenderTemplate(templatePath,
                ("FirstName", person.FirstName),
                ("FullName", person.FullName),
                ("Date", (type == "birthday" ? person.Birthday : person.Nameday)?.ToString("dd.MM" ) ?? ""));

            return Ok(new { subject, body });
        }

        [HttpPost("send/{personId}")]
        public async Task<IActionResult> Send(int personId, [FromQuery] string type, [FromQuery] bool confirm = false)
        {
            if (!confirm) return BadRequest("Confirm must be true to send");

            var person = await _context.People.FindAsync(personId);
            if (person == null) return NotFound("Person not found");
            if (string.IsNullOrWhiteSpace(person.Email)) return BadRequest("Person has no email");

            var isBirthday = string.Equals(type, "birthday", StringComparison.OrdinalIgnoreCase);
            var isNameday = string.Equals(type, "nameday", StringComparison.OrdinalIgnoreCase);
            if (!isBirthday && !isNameday) return BadRequest("Type must be 'birthday' or 'nameday'");

            // Prevent duplicate sends today
            var today = DateTime.Today;
            var alreadySent = await _context.NotificationLogs.AnyAsync(n =>
                n.PersonId == personId && n.Channel == NotificationChannel.Email &&
                n.Type == (isBirthday ? NotificationType.Birthday : NotificationType.Nameday) &&
                n.SentAtUtc >= today && n.SentAtUtc < today.AddDays(1));
            if (alreadySent) return Conflict("Already sent today");

            var templatesDir = Path.Combine(_env.ContentRootPath, "Data", "templates");
            var templatePath = Path.Combine(templatesDir, isBirthday ? "birthday.html" : "nameday.html");
            var subject = isBirthday ? $"Честит рожден ден, {person.FirstName}!" : $"Честит имен ден, {person.FirstName}!";
            var body = _emailService.RenderTemplate(templatePath,
                ("FirstName", person.FirstName),
                ("FullName", person.FullName),
                ("Date", (isBirthday ? person.Birthday : person.Nameday)?.ToString("dd.MM" ) ?? ""));

            try
            {
                await _emailService.SendEmailAsync(person.Email!, subject, body);
                _context.NotificationLogs.Add(new NotificationLog
                {
                    PersonId = person.Id,
                    Channel = NotificationChannel.Email,
                    Type = isBirthday ? NotificationType.Birthday : NotificationType.Nameday,
                    Subject = subject,
                    SentAtUtc = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                return Ok(new { sent = true });
            }
            catch (Exception ex)
            {
                _context.NotificationLogs.Add(new NotificationLog
                {
                    PersonId = person.Id,
                    Channel = NotificationChannel.Email,
                    Type = isBirthday ? NotificationType.Birthday : NotificationType.Nameday,
                    Subject = subject,
                    Error = ex.Message,
                    SentAtUtc = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                _logger.LogError(ex, "Error sending email");
                return StatusCode(500, "Failed to send email");
            }
        }
    }
}


