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
    /// Executes the urptMycoPDFSearchAPEx_S1S stored procedure.
    ///
    /// SP contract:
    ///   @VendorGroup  tinyint  - required, no NULL path in WHERE clause
    ///   @Vendor       varchar  - comma-separated vendor numbers, or '-1' for all
    ///   @Job          varchar  - comma-separated job numbers, or '-1' for all
    ///   @DateType     varchar  - 'Financial Period' | 'Invoice Date' | 'Attach Date'
    ///   @GLAcctMV     varchar  - GL Account filter; NULL means all accounts
    /// </summary>
    public async Task<List<UnallocatedPdfRecord>> GetUnallocatedPdfRecordsAsync(
        int company,
        DateTime beginDate,
        DateTime endDate,
        string? dateType = null,
        string? vendorGroup = null,
        string? vendor = null,
        string? job = null,
        string? glAccount = null)
    {
        Log.Debug(
            "Executing urptMycoPDFSearchAPEx_S1S: Company={Company}, Begin={BeginDate:yyyy-MM-dd}, End={EndDate:yyyy-MM-dd}, DateType={DateType}, VendorGroup={VendorGroup}, Vendor={Vendor}, Job={Job}, GLAcctMV={GLAcctMV}",
            company, beginDate, endDate, dateType, vendorGroup,
            string.IsNullOrWhiteSpace(vendor) ? "(all)" : vendor,
            string.IsNullOrWhiteSpace(job) ? "(all)" : job, glAccount);

        var records = new List<UnallocatedPdfRecord>();

        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("dbo.urptMycoPDFSearchAPEx_S1S", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 300
            };

            // @DateType — must match SP literals exactly (spaces included)
            command.Parameters.AddWithValue("@APCo", company);
            command.Parameters.AddWithValue("@BeginDate", beginDate);
            command.Parameters.AddWithValue("@EndDate", endDate);
            command.Parameters.AddWithValue("@DateType",
                string.IsNullOrWhiteSpace(dateType) ? "Financial Period" : dateType);

            // @VendorGroup — SP WHERE clause has no null check; always pass a real byte value
            command.Parameters.AddWithValue("@VendorGroup",
                !string.IsNullOrWhiteSpace(vendorGroup) ? byte.Parse(vendorGroup) : (byte)1);

            // @Vendor — comma-separated vendor numbers, or '-1' for all vendors
            command.Parameters.AddWithValue("@VendorMV",
                string.IsNullOrWhiteSpace(vendor) ? "-1" : vendor);

            // @JCCo — same as APCo
            command.Parameters.AddWithValue("@JCCo", company);

            // @Job — SP uses sentinel '-1' to mean "all jobs"
            command.Parameters.AddWithValue("@JobMV",
                string.IsNullOrWhiteSpace(job) ? "-1" : job);

            // @GLAcctMV — GL Account filter; NULL means all accounts
            command.Parameters.AddWithValue("@GLAcctMV",
                string.IsNullOrWhiteSpace(glAccount) ? DBNull.Value : (object)glAccount);

            await connection.OpenAsync();
            Log.Debug("Database connection opened successfully");

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var record = new UnallocatedPdfRecord
                {
                    Vendor          = reader.GetInt32(reader.GetOrdinal("Vendor")),
                    Name            = reader.IsDBNull(reader.GetOrdinal("Name"))              ? null : reader.GetString(reader.GetOrdinal("Name")),
                    Invoice         = reader.IsDBNull(reader.GetOrdinal("Invoice"))           ? null : reader.GetString(reader.GetOrdinal("Invoice")),
                    Job             = reader.IsDBNull(reader.GetOrdinal("Job"))               ? null : reader.GetString(reader.GetOrdinal("Job")),
                    JobName         = reader.IsDBNull(reader.GetOrdinal("JobName"))           ? null : reader.GetString(reader.GetOrdinal("JobName")),
                    Expense_Account = reader.IsDBNull(reader.GetOrdinal("Expense_Account"))   ? null : reader.GetString(reader.GetOrdinal("Expense_Account")),
                    Amount          = reader.GetDecimal(reader.GetOrdinal("Amount")),
                    Invoice_Date    = reader.IsDBNull(reader.GetOrdinal("Invoice_Date"))      ? null : reader.GetDateTime(reader.GetOrdinal("Invoice_Date")),
                    Item            = reader.IsDBNull(reader.GetOrdinal("item"))              ? null : reader.GetString(reader.GetOrdinal("item")),
                    UniqueAttchID   = reader.IsDBNull(reader.GetOrdinal("APTHUniqueAttchID")) ? null : reader.GetGuid(reader.GetOrdinal("APTHUniqueAttchID")),
                    AddDate         = reader.IsDBNull(reader.GetOrdinal("APTLAddDate"))       ? null : reader.GetDateTime(reader.GetOrdinal("APTLAddDate")),
                    OrigFileName    = reader.IsDBNull(reader.GetOrdinal("APTHOrigFileName"))  ? null : reader.GetString(reader.GetOrdinal("APTHOrigFileName")),
                    DateEntered     = reader.IsDBNull(reader.GetOrdinal("dateEntered"))       ? null : reader.GetDateTime(reader.GetOrdinal("dateEntered"))
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
    public async Task<AttachmentData?> GetAttachmentDataAsync(Guid uniqueAttchID)
    {
        Log.Debug("Executing brptGetAttachmentData_S1S for UniqueAttchID={UniqueAttchID}", uniqueAttchID);

        try
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("dbo.brptGetAttachmentData_S1S", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 300
            };

            command.Parameters.AddWithValue("@UniqueAttchID", uniqueAttchID);

            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var attachmentData = new AttachmentData
                {
                    HQCo               = reader.IsDBNull(reader.GetOrdinal("HQCo"))               ? 0                  : reader.GetByte(reader.GetOrdinal("HQCo")),
                    FormName           = reader.IsDBNull(reader.GetOrdinal("FormName"))           ? null               : reader.GetString(reader.GetOrdinal("FormName")),
                    KeyField           = reader.IsDBNull(reader.GetOrdinal("KeyField"))           ? null               : reader.GetString(reader.GetOrdinal("KeyField")),
                    Description        = reader.IsDBNull(reader.GetOrdinal("Description"))        ? null               : reader.GetString(reader.GetOrdinal("Description")),
                    AddedBy            = reader.IsDBNull(reader.GetOrdinal("AddedBy"))            ? null               : reader.GetString(reader.GetOrdinal("AddedBy")),
                    AddDate            = reader.IsDBNull(reader.GetOrdinal("AddDate"))            ? DateTime.MinValue  : reader.GetDateTime(reader.GetOrdinal("AddDate")),
                    DocName            = reader.IsDBNull(reader.GetOrdinal("DocName"))            ? null               : reader.GetString(reader.GetOrdinal("DocName")),
                    AttachmentID       = reader.GetInt32(reader.GetOrdinal("AttachmentID")),
                    TableName          = reader.IsDBNull(reader.GetOrdinal("TableName"))          ? null               : reader.GetString(reader.GetOrdinal("TableName")),
                    UniqueAttchID      = reader.GetGuid(reader.GetOrdinal("UniqueAttchID")),
                    OrigFileName       = reader.IsDBNull(reader.GetOrdinal("OrigFileName"))       ? null               : reader.GetString(reader.GetOrdinal("OrigFileName")),
                    FileBytes          = reader.IsDBNull(reader.GetOrdinal("AttachmentData"))     ? null               : (byte[])reader["AttachmentData"],
                    AttachmentFileType = reader.IsDBNull(reader.GetOrdinal("AttachmentFileType")) ? null               : reader.GetString(reader.GetOrdinal("AttachmentFileType"))
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
    /// Gets the list of companies from JCCO table
    /// </summary>
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
                var jcCoOrdinal = reader.GetOrdinal("JCCo");
                int jcCoValue = reader.GetFieldType(jcCoOrdinal) == typeof(byte)
                    ? reader.GetByte(jcCoOrdinal)
                    : reader.GetInt32(jcCoOrdinal);

                companies.Add(new Company
                {
                    JCCo  = jcCoValue,
                    Name  = reader.IsDBNull(reader.GetOrdinal("Name"))  ? null : reader.GetString(reader.GetOrdinal("Name")),
                    Label = reader.IsDBNull(reader.GetOrdinal("Label")) ? null : reader.GetString(reader.GetOrdinal("Label"))
                });
            }

            Log.Information("Retrieved {Count} companies from database", companies.Count);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving companies from database");
            throw;
        }

        return companies;
    }

    /// <summary>
    /// Gets vendor groups from HQCO table based on company
    /// </summary>
    public async Task<List<VendorGroup>> GetVendorGroupsAsync(int company)
    {
        var vendorGroups = new List<VendorGroup>();

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("SELECT VendorGroup FROM HQCO WHERE HQCo = @Company", connection);
            command.Parameters.AddWithValue("@Company", company);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var ordinal = reader.GetOrdinal("VendorGroup");
                if (reader.IsDBNull(ordinal))
                    continue;

                var fieldType = reader.GetFieldType(ordinal);
                string? vendorGroupValue = fieldType == typeof(byte)   ? reader.GetByte(ordinal).ToString()
                                         : fieldType == typeof(string) ? reader.GetString(ordinal)
                                                                       : reader.GetValue(ordinal)?.ToString();

                if (!string.IsNullOrWhiteSpace(vendorGroupValue))
                    vendorGroups.Add(new VendorGroup { VendorGroupCode = vendorGroupValue });
            }

            Log.Information("Retrieved {Count} vendor groups for company {Company}", vendorGroups.Count, company);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving vendor groups for company {Company}", company);
            throw;
        }

        return vendorGroups;
    }

    /// <summary>
    /// Gets vendors from APVM table based on vendor group
    /// </summary>
    public async Task<List<Vendor>> GetVendorsAsync(string vendorGroup)
    {
        var vendors = new List<Vendor>();

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(
                "SELECT Vendor, Name FROM APVM WHERE VendorGroup = @VendorGroup ORDER BY Name", connection);
            command.Parameters.AddWithValue("@VendorGroup", vendorGroup);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var vendorOrdinal = reader.GetOrdinal("Vendor");
                var fieldType = reader.GetFieldType(vendorOrdinal);
                int vendorNumber = fieldType == typeof(byte)  ? reader.GetByte(vendorOrdinal)
                                 : fieldType == typeof(short) ? reader.GetInt16(vendorOrdinal)
                                                              : reader.GetInt32(vendorOrdinal);

                vendors.Add(new Vendor
                {
                    VendorNumber = vendorNumber,
                    Name         = reader.IsDBNull(reader.GetOrdinal("Name")) ? null : reader.GetString(reader.GetOrdinal("Name"))
                });
            }

            Log.Information("Retrieved {Count} vendors for vendor group {VendorGroup}", vendors.Count, vendorGroup);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving vendors for vendor group {VendorGroup}", vendorGroup);
            throw;
        }

        return vendors;
    }

    /// <summary>
    /// Gets jobs filtered by company, and optionally by vendor or vendor group.
    /// </summary>
    public async Task<List<Job>> GetJobsAsync(int company, string? vendorGroup = null, string? vendor = null)
    {
        var jobs = new List<Job>();

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            string query;
            SqlCommand command;

            if (!string.IsNullOrWhiteSpace(vendor))
            {
                // Filter by vendor list (comma-separated) and optionally vendor group
                var vendorNumbers = vendor.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                // SQL Server has a limit on parameters (2100). If too many vendors, fall back to vendor group filter.
                // Practical limit: use 500 to be safe
                if (vendorNumbers.Length > 500)
                {
                    Log.Warning("Too many vendors selected ({Count}), falling back to vendor group filter", vendorNumbers.Length);

                    if (!string.IsNullOrWhiteSpace(vendorGroup))
                    {
                        // Use vendor group filter instead
                        query = @"
                            SELECT DISTINCT 
                                APTL.JCCo, APTL.Job, JCJM.Description,
                                APTL.Job + ' - ' + JCJM.Description AS JobName
                            FROM dbo.APTL
                                INNER JOIN dbo.APTH
                                    ON APTL.APCo = APTH.APCo
                                    AND APTL.Mth = APTH.Mth 
                                    AND APTL.APTrans = APTH.APTrans
                                INNER JOIN JCJM 
                                    ON APTL.JCCo = JCJM.JCCo
                                    AND APTL.Job = JCJM.Job
                            WHERE APTH.APCo = @Company
                                AND APTH.VendorGroup = @VendorGroup
                            ORDER BY APTL.Job";

                        command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Company", company);
                        command.Parameters.AddWithValue("@VendorGroup", byte.Parse(vendorGroup));

                        Log.Information("Retrieving jobs for company {Company}, vendor group {VendorGroup} (all vendors)", company, vendorGroup);
                    }
                    else
                    {
                        // No vendor group, get all jobs for company
                        query = "SELECT Job, Description, Job + ' - ' + Description AS JobName FROM JCJM WHERE JCCo = @Company ORDER BY Job";
                        command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Company", company);

                        Log.Information("Retrieving all jobs for company {Company} (too many vendors selected)", company);
                    }
                }
                else
                {
                    // Build IN clause dynamically for reasonable number of vendors
                    // First, parse and validate all vendor numbers
                    var validVendorNumbers = new List<int>();
                    foreach (var vendorStr in vendorNumbers)
                    {
                        if (int.TryParse(vendorStr, out int vendorNum))
                        {
                            validVendorNumbers.Add(vendorNum);
                        }
                        else
                        {
                            Log.Warning("Invalid vendor number: {VendorNumber}, skipping", vendorStr);
                        }
                    }

                    if (validVendorNumbers.Count == 0)
                    {
                        // No valid vendors, fall back to all jobs
                        query = "SELECT Job, Description, Job + ' - ' + Description AS JobName FROM JCJM WHERE JCCo = @Company ORDER BY Job";
                        command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Company", company);

                        Log.Information("No valid vendors provided, retrieving all jobs for company {Company}", company);
                    }
                    else
                    {
                        var vendorParams = new List<string>();
                        for (int i = 0; i < validVendorNumbers.Count; i++)
                        {
                            vendorParams.Add($"@Vendor{i}");
                        }

                        query = $@"
                            SELECT DISTINCT 
                                APTL.JCCo, APTL.Job, JCJM.Description,
                                APTL.Job + ' - ' + JCJM.Description AS JobName
                            FROM dbo.APTL
                                INNER JOIN dbo.APTH
                                    ON APTL.APCo = APTH.APCo
                                    AND APTL.Mth = APTH.Mth 
                                    AND APTL.APTrans = APTH.APTrans
                                INNER JOIN JCJM 
                                    ON APTL.JCCo = JCJM.JCCo
                                    AND APTL.Job = JCJM.Job
                            WHERE APTH.APCo = @Company
                                AND APTH.Vendor IN ({string.Join(",", vendorParams)})
                                AND (@VendorGroup IS NULL OR APTH.VendorGroup = @VendorGroup)
                            ORDER BY APTL.Job";

                        command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Company", company);

                        for (int i = 0; i < validVendorNumbers.Count; i++)
                        {
                            command.Parameters.AddWithValue($"@Vendor{i}", validVendorNumbers[i]);
                        }

                        command.Parameters.AddWithValue("@VendorGroup",
                            !string.IsNullOrWhiteSpace(vendorGroup) ? (object)byte.Parse(vendorGroup) : DBNull.Value);

                        Log.Information("Retrieving jobs for company {Company}, {VendorCount} vendors{VG}",
                            company, validVendorNumbers.Count, vendorGroup != null ? $", vendor group {vendorGroup}" : "");
                    }
                }
            }
            else if (!string.IsNullOrWhiteSpace(vendorGroup))
            {
                // Filter by vendor group only
                query = @"
                    SELECT DISTINCT 
                        APTL.JCCo, APTL.Job, JCJM.Description,
                        APTL.Job + ' - ' + JCJM.Description AS JobName
                    FROM dbo.APTL
                        INNER JOIN dbo.APTH
                            ON APTL.APCo = APTH.APCo
                            AND APTL.Mth = APTH.Mth 
                            AND APTL.APTrans = APTH.APTrans
                        INNER JOIN JCJM 
                            ON APTL.JCCo = JCJM.JCCo
                            AND APTL.Job = JCJM.Job
                    WHERE APTH.APCo = @Company
                        AND APTH.VendorGroup = @VendorGroup
                    ORDER BY APTL.Job";

                command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Company", company);
                command.Parameters.AddWithValue("@VendorGroup", byte.Parse(vendorGroup));

                Log.Information("Retrieving jobs for company {Company}, vendor group {VendorGroup}", company, vendorGroup);
            }
            else
            {
                // No filters — all jobs for company
                query = "SELECT Job, Description, Job + ' - ' + Description AS JobName FROM JCJM WHERE JCCo = @Company ORDER BY Job";
                command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Company", company);

                Log.Information("Retrieving all jobs for company {Company}", company);
            }

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                jobs.Add(new Job
                {
                    JobNumber   = reader.IsDBNull(reader.GetOrdinal("Job"))         ? null : reader.GetString(reader.GetOrdinal("Job")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                    JobName     = reader.IsDBNull(reader.GetOrdinal("JobName"))     ? null : reader.GetString(reader.GetOrdinal("JobName"))
                });
            }

            Log.Information("Retrieved {Count} jobs for company {Company}", jobs.Count, company);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving jobs for company {Company}, vendorGroup {VendorGroup}, vendor {Vendor}",
                company, vendorGroup, vendor);
            throw;
        }

        return jobs;
    }
}
