using System;
using System.IO;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data;
using SchoolManagement.Models;
using ClosedXML.Excel;
using CsvHelper;
using System.Collections.Generic;

namespace SchoolManagement.Services
{
    public interface IAuditService
    {
        Task LogAsync(string action, string entity, string? entityId = null, string? details = null, int? schoolId = null);
    }

    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string action, string entity, string? entityId = null, string? details = null, int? schoolId = null)
        {
            try
            {
                var httpUser = _httpContextAccessor.HttpContext?.User;
                var userName = httpUser?.Identity?.IsAuthenticated == true ? httpUser.Identity.Name ?? "User" : "System";
                var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

                var audit = new AuditLog
                {
                    SchoolId = schoolId,
                    UserName = userName,
                    Action = action,
                    Entity = entity,
                    EntityId = entityId,
                    Details = details,
                    IpAddress = ip,
                    DateTime = DateTime.UtcNow
                };

                _context.AuditLogs.Add(audit);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Never let audit log failure break the primary application flow
            }
        }
    }

    public interface IExportService
    {
        byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName);
        byte[] ExportToCsv<T>(IEnumerable<T> data);
    }

    public class ExportService : IExportService
    {
        public byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);
            worksheet.Cell(1, 1).InsertTable(data);
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] ExportToCsv<T>(IEnumerable<T> data)
        {
            using var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(data);
            }
            return memoryStream.ToArray();
        }
    }
}
