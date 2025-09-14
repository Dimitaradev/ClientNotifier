using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientNotifier.Core.DTOs
{
    public class ExcelImportDto
    {
        public string FileName { get; set; } = string.Empty;
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
        public bool SkipFirstRow { get; set; } = true;
        public bool UpdateExisting { get; set; } = false;
    }

    public class ImportResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessfulImports { get; set; }
        public int FailedImports { get; set; }
        public int UpdatedRecords { get; set; }
        public int SkippedDuplicates { get; set; }
        public List<ImportErrorDto> Errors { get; set; } = new();
        public TimeSpan ProcessingTime { get; set; }
    }

    public class ImportErrorDto
    {
        public int RowNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? EGN { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public Dictionary<string, string> RowData { get; set; } = new();
    }

    public class ExcelRowDto
    {
        public int RowNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? EGN { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Notes { get; set; }
        public bool NotificationsEnabled { get; set; } = true;
        public Dictionary<string, string> RowData { get; set; } = new();
    }

    public class ImportTemplateDto
    {
        public string FileName { get; set; } = "ClientImportTemplate.xlsx";
        public List<string> Headers { get; set; } = new()
        {
            "Име",
            "Фамилия",
            "ЕГН",
            "Имейл",
            "Телефон",
            "Бележки",
            "Известия (Да/Не)"
        };
        public List<ExcelRowDto> SampleData { get; set; } = new()
        {
            new ExcelRowDto
            {
                FirstName = "Иван",
                LastName = "Иванов",
                EGN = "8001014509",
                Email = "ivan@example.com",
                PhoneNumber = "+359888123456",
                Notes = "VIP клиент",
                NotificationsEnabled = true
            },
            new ExcelRowDto
            {
                FirstName = "Мария",
                LastName = "Петрова",
                EGN = "8508154502",
                Email = "maria@example.com",
                PhoneNumber = "+359887654321",
                Notes = "",
                NotificationsEnabled = true
            }
        };
    }
}
