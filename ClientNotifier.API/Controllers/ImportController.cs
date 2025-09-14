using AutoMapper;
using ClientNotifier.Core.DTOs;
using ClientNotifier.Core.Models;
using ClientNotifier.Core.Services;
using ClientNotifier.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ClientNotifier.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly NotifierContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ImportController> _logger;
        private readonly ExcelImportService _importService;

        public ImportController(NotifierContext context, IMapper mapper, ILogger<ImportController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _importService = new ExcelImportService();
        }

        /// <summary>
        /// Import people from Excel file
        /// </summary>
        [HttpPost("excel")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ImportResultDto>> ImportFromExcel(
            IFormFile file,
            [FromForm] bool skipFirstRow = true,
            [FromForm] bool updateExisting = false)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            // Validate file extension
            var allowedExtensions = new[] { ".xlsx", ".xls" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest("Invalid file type. Only Excel files (.xlsx, .xls) are allowed");
            }

            // Validate file size (max 10MB)
            if (file.Length > 10 * 1024 * 1024)
            {
                return BadRequest("File size exceeds 10MB limit");
            }

            try
            {
                byte[] fileContent;
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    fileContent = stream.ToArray();
                }

                _logger.LogInformation($"Starting import of file: {file.FileName} ({file.Length} bytes)");

                // Process Excel file
                var importResult = await _importService.ImportFromExcelAsync(fileContent, skipFirstRow, updateExisting);

                // Save to database
                var savedResult = await SaveImportedDataAsync(fileContent, skipFirstRow, updateExisting);
                
                // Merge results
                importResult.SuccessfulImports = savedResult.SuccessfulImports;
                importResult.UpdatedRecords = savedResult.UpdatedRecords;
                importResult.SkippedDuplicates = savedResult.SkippedDuplicates;
                importResult.FailedImports = savedResult.FailedImports;
                importResult.Errors.AddRange(savedResult.Errors);

                _logger.LogInformation($"Import completed: {importResult.SuccessfulImports} success, {importResult.FailedImports} failed");

                return Ok(importResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Excel import");
                return StatusCode(500, new ImportResultDto
                {
                    Errors = new List<ImportErrorDto>
                    {
                        new ImportErrorDto
                        {
                            ErrorMessage = "An error occurred during import: " + ex.Message
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Download Excel import template
        /// </summary>
        [HttpGet("template")]
        public IActionResult DownloadTemplate()
        {
            try
            {
                var templateBytes = _importService.GenerateImportTemplate();
                return File(templateBytes, 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    "ClientImportTemplate.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating import template");
                return StatusCode(500, "Error generating template");
            }
        }

        /// <summary>
        /// Get import status and history
        /// </summary>
        [HttpGet("history")]
        public async Task<ActionResult> GetImportHistory()
        {
            // This is a placeholder for future implementation
            // You could store import history in a separate table
            return Ok(new
            {
                message = "Import history will be available in a future version",
                lastImport = await _context.People
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => p.CreatedAt)
                    .FirstOrDefaultAsync()
            });
        }

        private async Task<ImportResultDto> SaveImportedDataAsync(byte[] fileContent, bool skipFirstRow, bool updateExisting)
        {
            var result = new ImportResultDto();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var stream = new MemoryStream(fileContent);
                using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
                var worksheet = workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                    return result;

                var importService = new ExcelImportService();
                var tempResult = await importService.ImportFromExcelAsync(fileContent, skipFirstRow, updateExisting);
                
                // Get successfully validated rows
                var startRow = skipFirstRow ? 2 : 1;
                var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;

                for (int row = startRow; row <= lastRow; row++)
                {
                    try
                    {
                        var firstName = worksheet.Cell(row, 1).Value.ToString()?.Trim();
                        var lastName = worksheet.Cell(row, 2).Value.ToString()?.Trim();
                        var egn = worksheet.Cell(row, 3).Value.ToString()?.Trim()?.Replace(" ", "").Replace("-", "");
                        var email = worksheet.Cell(row, 4).Value.ToString()?.Trim();
                        var phone = worksheet.Cell(row, 5).Value.ToString()?.Trim();
                        var notes = worksheet.Cell(row, 6).Value.ToString()?.Trim();
                        var notifyText = worksheet.Cell(row, 7).Value.ToString()?.Trim()?.ToLower() ?? "";

                        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(egn))
                            continue;

                        if (!EgnUtils.IsValidEgn(egn))
                            continue;

                        // Check for existing person
                        var existingPerson = await _context.People.FirstOrDefaultAsync(p => p.EGN == egn);

                        if (existingPerson != null)
                        {
                            if (updateExisting)
                            {
                                // Update existing person
                                existingPerson.FirstName = firstName;
                                existingPerson.LastName = lastName;
                                existingPerson.Email = email;
                                existingPerson.PhoneNumber = phone;
                                existingPerson.Notes = notes;
                                existingPerson.NotificationsEnabled = notifyText != "не" && notifyText != "no" && notifyText != "false";
                                existingPerson.Birthday = EgnUtils.ExtractBirthday(egn);

                                // Update nameday
                                var namedayMappings = await _context.NamedayMappings.ToListAsync();
                                var namedayService = new NamedayService(namedayMappings);
                                existingPerson.Nameday = namedayService.GetNamedayForPerson(existingPerson);

                                result.UpdatedRecords++;
                            }
                            else
                            {
                                result.SkippedDuplicates++;
                            }
                        }
                        else
                        {
                            // Create new person
                            var person = new People
                            {
                                FirstName = firstName,
                                LastName = lastName,
                                EGN = egn,
                                Email = email,
                                PhoneNumber = phone,
                                Notes = notes,
                                NotificationsEnabled = notifyText != "не" && notifyText != "no" && notifyText != "false",
                                Birthday = EgnUtils.ExtractBirthday(egn)
                            };

                            // Assign nameday
                            var namedayMappings = await _context.NamedayMappings.ToListAsync();
                            var namedayService = new NamedayService(namedayMappings);
                            person.Nameday = namedayService.GetNamedayForPerson(person);

                            _context.People.Add(person);
                            result.SuccessfulImports++;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.FailedImports++;
                        result.Errors.Add(new ImportErrorDto
                        {
                            RowNumber = row,
                            ErrorMessage = ex.Message
                        });
                    }
                }

                if (result.SuccessfulImports > 0 || result.UpdatedRecords > 0)
                {
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving imported data");
                throw;
            }

            stopwatch.Stop();
            result.ProcessingTime = stopwatch.Elapsed;
            result.TotalRows = result.SuccessfulImports + result.FailedImports + result.UpdatedRecords + result.SkippedDuplicates;

            return result;
        }
    }
}
