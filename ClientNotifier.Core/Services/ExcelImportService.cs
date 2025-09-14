using ClosedXML.Excel;
using ClientNotifier.Core.DTOs;
using ClientNotifier.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClientNotifier.Core.Services
{
    public class ExcelImportService
    {
        private readonly Dictionary<string, string> _headerMappings = new()
        {
            // Bulgarian headers
            { "име", "firstname" },
            { "фамилия", "lastname" },
            { "презиме", "lastname" },
            { "егн", "egn" },
            { "имейл", "email" },
            { "е-мейл", "email" },
            { "e-mail", "email" },
            { "телефон", "phone" },
            { "телефонен номер", "phone" },
            { "мобилен", "phone" },
            { "бележки", "notes" },
            { "забележки", "notes" },
            { "коментар", "notes" },
            { "известия", "notifications" },
            { "нотификации", "notifications" },
            { "напомняния", "notifications" },
            
            // English headers
            { "first name", "firstname" },
            { "firstname", "firstname" },
            { "name", "firstname" },
            { "last name", "lastname" },
            { "lastname", "lastname" },
            { "surname", "lastname" },
            { "email", "email" },
            { "phone", "phone" },
            { "phone number", "phone" },
            { "mobile", "phone" },
            { "notes", "notes" },
            { "comments", "notes" },
            { "notifications", "notifications" },
            { "notify", "notifications" }
        };

        public async Task<ImportResultDto> ImportFromExcelAsync(byte[] fileContent, bool skipFirstRow = true, bool updateExisting = false)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new ImportResultDto();
            var rows = new List<ExcelRowDto>();

            try
            {
                using var stream = new MemoryStream(fileContent);
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        RowNumber = 0,
                        ErrorMessage = "Excel файлът не съдържа работни листове"
                    });
                    return result;
                }

                // Map headers
                var headerMap = MapHeaders(worksheet);
                if (!ValidateHeaders(headerMap, result))
                {
                    return result;
                }

                // Read data rows
                var startRow = skipFirstRow ? 2 : 1;
                var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
                result.TotalRows = Math.Max(0, lastRow - startRow + 1);

                for (int row = startRow; row <= lastRow; row++)
                {
                    var rowData = ReadRow(worksheet, row, headerMap);
                    if (rowData != null)
                    {
                        rows.Add(rowData);
                    }
                }

                // Process rows
                foreach (var row in rows)
                {
                    await ProcessRowAsync(row, updateExisting, result);
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ImportErrorDto
                {
                    RowNumber = 0,
                    ErrorMessage = $"Грешка при обработка на файла: {ex.Message}"
                });
            }

            stopwatch.Stop();
            result.ProcessingTime = stopwatch.Elapsed;
            return result;
        }

        public byte[] GenerateImportTemplate()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Клиенти");

            // Add headers
            var headers = new[] { "Име", "Фамилия", "ЕГН", "Имейл", "Телефон", "Бележки", "Известия" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // Add sample data
            worksheet.Cell(2, 1).Value = "Иван";
            worksheet.Cell(2, 2).Value = "Иванов";
            worksheet.Cell(2, 3).Value = "8001014509";
            worksheet.Cell(2, 4).Value = "ivan@example.com";
            worksheet.Cell(2, 5).Value = "+359888123456";
            worksheet.Cell(2, 6).Value = "VIP клиент";
            worksheet.Cell(2, 7).Value = "Да";

            worksheet.Cell(3, 1).Value = "Мария";
            worksheet.Cell(3, 2).Value = "Петрова";
            worksheet.Cell(3, 3).Value = "8508154502";
            worksheet.Cell(3, 4).Value = "maria@example.com";
            worksheet.Cell(3, 5).Value = "+359887654321";
            worksheet.Cell(3, 6).Value = "";
            worksheet.Cell(3, 7).Value = "Да";

            // Add instructions
            worksheet.Cell(5, 1).Value = "Инструкции:";
            worksheet.Cell(5, 1).Style.Font.Bold = true;
            worksheet.Cell(6, 1).Value = "1. ЕГН трябва да е точно 10 цифри";
            worksheet.Cell(7, 1).Value = "2. Имейл адресът трябва да е валиден";
            worksheet.Cell(8, 1).Value = "3. Полето 'Известия' приема 'Да' или 'Не'";
            worksheet.Cell(9, 1).Value = "4. Рожденият ден се изчислява автоматично от ЕГН";
            worksheet.Cell(10, 1).Value = "5. Именният ден се определя автоматично от името";

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private Dictionary<string, int> MapHeaders(IXLWorksheet worksheet)
        {
            var headerMap = new Dictionary<string, int>();
            var firstRow = worksheet.Row(1);
            var lastColumn = firstRow.LastCellUsed()?.Address.ColumnNumber ?? 0;

            for (int col = 1; col <= lastColumn; col++)
            {
                var headerValue = firstRow.Cell(col).Value.ToString()?.Trim().ToLower() ?? "";
                if (!string.IsNullOrEmpty(headerValue) && _headerMappings.ContainsKey(headerValue))
                {
                    var mappedName = _headerMappings[headerValue];
                    if (!headerMap.ContainsKey(mappedName))
                    {
                        headerMap[mappedName] = col;
                    }
                }
            }

            return headerMap;
        }

        private bool ValidateHeaders(Dictionary<string, int> headerMap, ImportResultDto result)
        {
            var requiredHeaders = new[] { "firstname", "egn" };
            var missingHeaders = requiredHeaders.Where(h => !headerMap.ContainsKey(h)).ToList();

            if (missingHeaders.Any())
            {
                result.Errors.Add(new ImportErrorDto
                {
                    RowNumber = 0,
                    ErrorMessage = $"Липсват задължителни колони: {string.Join(", ", missingHeaders.Select(h => h == "firstname" ? "Име" : "ЕГН"))}"
                });
                return false;
            }

            return true;
        }

        private ExcelRowDto? ReadRow(IXLWorksheet worksheet, int rowNumber, Dictionary<string, int> headerMap)
        {
            var row = worksheet.Row(rowNumber);
            
            // Check if row is empty
            var hasData = false;
            foreach (var col in headerMap.Values)
            {
                if (!string.IsNullOrWhiteSpace(row.Cell(col).Value.ToString()))
                {
                    hasData = true;
                    break;
                }
            }

            if (!hasData) return null;

            var rowDto = new ExcelRowDto
            {
                RowNumber = rowNumber,
                FirstName = GetCellValue(row, headerMap, "firstname"),
                LastName = GetCellValue(row, headerMap, "lastname"),
                EGN = GetCellValue(row, headerMap, "egn")?.Replace(" ", "").Replace("-", ""),
                Email = GetCellValue(row, headerMap, "email"),
                PhoneNumber = GetCellValue(row, headerMap, "phone"),
                Notes = GetCellValue(row, headerMap, "notes")
            };

            // Parse notifications
            var notificationsValue = GetCellValue(row, headerMap, "notifications")?.ToLower() ?? "";
            rowDto.NotificationsEnabled = notificationsValue != "не" && notificationsValue != "no" && notificationsValue != "false" && notificationsValue != "0";

            // Store raw data for error reporting
            foreach (var header in headerMap)
            {
                rowDto.RowData[header.Key] = row.Cell(header.Value).Value.ToString() ?? "";
            }

            return rowDto;
        }

        private string? GetCellValue(IXLRow row, Dictionary<string, int> headerMap, string field)
        {
            if (headerMap.TryGetValue(field, out var colIndex))
            {
                var value = row.Cell(colIndex).Value.ToString()?.Trim();
                return string.IsNullOrEmpty(value) ? null : value;
            }
            return null;
        }

        private async Task ProcessRowAsync(ExcelRowDto row, bool updateExisting, ImportResultDto result)
        {
            await Task.Run(() =>
            {
                try
                {
                    // Validate required fields
                    if (string.IsNullOrWhiteSpace(row.FirstName))
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            RowNumber = row.RowNumber,
                            FirstName = row.FirstName,
                            LastName = row.LastName,
                            EGN = row.EGN,
                            ErrorMessage = "Името е задължително",
                            RowData = row.RowData
                        });
                        result.FailedImports++;
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(row.EGN))
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            RowNumber = row.RowNumber,
                            FirstName = row.FirstName,
                            LastName = row.LastName,
                            EGN = row.EGN,
                            ErrorMessage = "ЕГН е задължително",
                            RowData = row.RowData
                        });
                        result.FailedImports++;
                        return;
                    }

                    // Validate EGN
                    if (!EgnUtils.IsValidEgn(row.EGN))
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            RowNumber = row.RowNumber,
                            FirstName = row.FirstName,
                            LastName = row.LastName,
                            EGN = row.EGN,
                            ErrorMessage = "Невалидно ЕГН",
                            RowData = row.RowData
                        });
                        result.FailedImports++;
                        return;
                    }

                    // Validate email if provided
                    if (!string.IsNullOrWhiteSpace(row.Email))
                    {
                        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                        if (!emailRegex.IsMatch(row.Email))
                        {
                            result.Errors.Add(new ImportErrorDto
                            {
                                RowNumber = row.RowNumber,
                                FirstName = row.FirstName,
                                LastName = row.LastName,
                                EGN = row.EGN,
                                ErrorMessage = $"Невалиден имейл адрес: {row.Email}",
                                RowData = row.RowData
                            });
                            result.FailedImports++;
                            return;
                        }
                    }

                    // NOTE: In a real implementation, you would check for duplicates
                    // and handle updates here. For now, we'll just count as successful.
                    result.SuccessfulImports++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new ImportErrorDto
                    {
                        RowNumber = row.RowNumber,
                        FirstName = row.FirstName,
                        LastName = row.LastName,
                        EGN = row.EGN,
                        ErrorMessage = $"Грешка: {ex.Message}",
                        RowData = row.RowData
                    });
                    result.FailedImports++;
                }
            });
        }
    }
}
