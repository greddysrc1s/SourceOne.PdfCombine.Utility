using Microsoft.Data.SqlClient;
using Serilog;
using SourceOne.PdfCombine.Utility.Models;
using System.Data;

namespace SourceOne.PdfCombine.Utility.Services;

/// <summary>
/// Service for querying unallocated PDF records from the database
/// </summary>
public class PdfQueryService
{
    private readonly string _connectionString;

    public PdfQueryService(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        Log.Debug("PdfQueryService initialized");
    }

    /// <summary>
    /// Executes the urptJFKS_Unallocated_PDF_Query_S1S stored procedure
    /// </summary>
    /// <param name="company">Company ID</param>
    /// <param name="beginDate">Begin date for filtering</param>
    /// <param name="endDate">End date for filtering</param>
    /// <returns>List of unallocated PDF records</returns>
    public async Task<List<UnallocatedPdfRecord>> GetUnallocatedPdfRecordsAsync(int company, DateTime beginDate, DateTime endDate)
    {
        Log.Debug("Executing urptJFKS_Unallocated_PDF_Query_S1S with parameters: Company={Company}, BeginDate={BeginDate:yyyy-MM-dd}, EndDate={EndDate:yyyy-MM-dd}",
            company, beginDate, endDate);

        var records = new List<UnallocatedPdfRecord>();

        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("dbo.urptJFKS_Unallocated_PDF_Query_S1S", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 300 // 5 minutes timeout
            };

            // Add parameters
            command.Parameters.AddWithValue("@Co", company);
            command.Parameters.AddWithValue("@BeginDate", beginDate);
            command.Parameters.AddWithValue("@EndDate", endDate);

            await connection.OpenAsync();
            Log.Debug("Database connection opened successfully");

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var record = new UnallocatedPdfRecord
                {
                    Vendor = reader.GetInt32(reader.GetOrdinal("Vendor")),
                    Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? null : reader.GetString(reader.GetOrdinal("Name")),
                    Invoice = reader.IsDBNull(reader.GetOrdinal("Invoice")) ? null : reader.GetString(reader.GetOrdinal("Invoice")),
                    Job = reader.IsDBNull(reader.GetOrdinal("Job")) ? null : reader.GetString(reader.GetOrdinal("Job")),
                    Expense_Account = reader.IsDBNull(reader.GetOrdinal("Expense_Account")) ? null : reader.GetString(reader.GetOrdinal("Expense_Account")),
                    Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                    Invoice_Date = reader.IsDBNull(reader.GetOrdinal("Invoice_Date")) ? null : reader.GetDateTime(reader.GetOrdinal("Invoice_Date")),
                    Item = reader.IsDBNull(reader.GetOrdinal("item")) ? null : reader.GetString(reader.GetOrdinal("item")),
                    UniqueAttchID = reader.IsDBNull(reader.GetOrdinal("UniqueAttchID")) ? null : reader.GetGuid(reader.GetOrdinal("UniqueAttchID")),
                    AddDate = reader.IsDBNull(reader.GetOrdinal("AddDate")) ? null : reader.GetDateTime(reader.GetOrdinal("AddDate")),
                    OrigFileName = reader.IsDBNull(reader.GetOrdinal("OrigFileName")) ? null : reader.GetString(reader.GetOrdinal("OrigFileName")),

                    //UniqueAttchID_Line = reader.IsDBNull(11) ? Guid.Empty : reader.GetGuid(11), // Column index for second UniqueAttchID
                    //AddDate_Line = reader.IsDBNull(12) ? DateTime.MinValue : reader.GetDateTime(12), // Column index for second AddDate
                    //OrigFileName_Line = reader.IsDBNull(13) ? null : reader.GetString(13), // Column index for second OrigFileName
                    DateEntered = reader.IsDBNull(reader.GetOrdinal("dateEntered")) ? null : reader.GetDateTime(reader.GetOrdinal("dateEntered"))
                };

                records.Add(record);
            }

            Log.Information("Successfully retrieved {RecordCount} records from database", records.Count);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing GetUnallocatedPdfRecordsAsync");
            throw;
        }

        return records;
    }

    /// <summary>
    /// Executes the brptGetAttachmentData_S1S stored procedure to retrieve attachment data
    /// </summary>
    /// <param name="uniqueAttchID">The unique attachment ID (GUID format)</param>
    /// <returns>Attachment data including the file bytes and metadata</returns>
    public async Task<AttachmentData?> GetAttachmentDataAsync(Guid uniqueAttchID)
    {
        Log.Debug("Executing brptGetAttachmentData_S1S for UniqueAttchID={UniqueAttchID}", uniqueAttchID);

        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("dbo.brptGetAttachmentData_S1S", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 300 // 5 minutes timeout
            };

            // Add parameter
            command.Parameters.AddWithValue("@UniqueAttchID", uniqueAttchID);

            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var attachmentData = new AttachmentData
                {
                    HQCo = reader.IsDBNull(reader.GetOrdinal("HQCo")) ? 0 :  reader.GetByte(reader.GetOrdinal("HQCo")),
                    FormName = reader.IsDBNull(reader.GetOrdinal("FormName")) ? null : reader.GetString(reader.GetOrdinal("FormName")),
                    KeyField = reader.IsDBNull(reader.GetOrdinal("KeyField")) ? null : reader.GetString(reader.GetOrdinal("KeyField")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                    AddedBy = reader.IsDBNull(reader.GetOrdinal("AddedBy")) ? null : reader.GetString(reader.GetOrdinal("AddedBy")),
                    AddDate = reader.IsDBNull(reader.GetOrdinal("AddDate")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("AddDate")),
                    DocName = reader.IsDBNull(reader.GetOrdinal("DocName")) ? null : reader.GetString(reader.GetOrdinal("DocName")),
                    AttachmentID = reader.GetInt32(reader.GetOrdinal("AttachmentID")),
                    TableName = reader.IsDBNull(reader.GetOrdinal("TableName")) ? null : reader.GetString(reader.GetOrdinal("TableName")),
                    UniqueAttchID = reader.GetGuid(reader.GetOrdinal("UniqueAttchID")),
                    OrigFileName = reader.IsDBNull(reader.GetOrdinal("OrigFileName")) ? null : reader.GetString(reader.GetOrdinal("OrigFileName")),
                    FileBytes = reader.IsDBNull(reader.GetOrdinal("AttachmentData")) ? null : (byte[])reader["AttachmentData"],
                    AttachmentFileType = reader.IsDBNull(reader.GetOrdinal("AttachmentFileType")) ? null : reader.GetString(reader.GetOrdinal("AttachmentFileType"))
                };

                Log.Debug("Successfully retrieved attachment data for {FileName} ({FileSize})",
                    attachmentData.OrigFileName, attachmentData.GetFileSizeString());

                return attachmentData;
            }

            Log.Warning("No attachment data found for UniqueAttchID={UniqueAttchID}", uniqueAttchID);
            return null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error executing GetAttachmentDataAsync for UniqueAttchID={UniqueAttchID}", uniqueAttchID);
            throw;
        }
    }

    /// <summary>
    /// Gets the count of unallocated PDF records
    /// </summary>
    public async Task<int> GetRecordCountAsync(int company, DateTime beginDate, DateTime endDate)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("urptJFKS_Unallocated_PDF_Query_S1S", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@Company", company);
        command.Parameters.AddWithValue("@BeginDate", beginDate);
        command.Parameters.AddWithValue("@EndDate", endDate);

        var records = new List<UnallocatedPdfRecord>();
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            records.Add(new UnallocatedPdfRecord());
        }

        return records.Count;
    }

    /// <summary>
    /// Gets the list of companies from JCCO table
    /// </summary>
    /// <returns>List of companies with JCCo, Name, and Label</returns>
    public async Task<List<Company>> GetCompaniesAsync()
    {
        var companies = new List<Company>();

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT JCCo, [Name], CAST(JCCo AS VARCHAR(3)) + ' - ' + [Name] AS Label 
                FROM JCCO 
                INNER JOIN HQCO ON JCCo = HQCo 
                ORDER BY JCCo";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                // Handle both byte and int types for JCCo
                int jcCoValue;
                var jcCoOrdinal = reader.GetOrdinal("JCCo");
                
                if (reader.GetFieldType(jcCoOrdinal) == typeof(byte))
                {
                    jcCoValue = reader.GetByte(jcCoOrdinal);
                }
                else
                {
                    jcCoValue = reader.GetInt32(jcCoOrdinal);
                }

                companies.Add(new Company
                {
                    JCCo = jcCoValue,
                    Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? null : reader.GetString(reader.GetOrdinal("Name")),
                    Label = reader.IsDBNull(reader.GetOrdinal("Label")) ? null : reader.GetString(reader.GetOrdinal("Label"))
                });
            }

            Log.Information($"Retrieved {companies.Count} companies from database");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving companies from database");
            throw;
        }

        return companies;
    }
}
