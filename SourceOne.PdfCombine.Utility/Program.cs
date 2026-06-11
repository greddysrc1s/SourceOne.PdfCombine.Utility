using Microsoft.Extensions.Configuration;
using Serilog;
using SourceOne.PdfCombine.Utility.Forms;

// For .NET 9 top-level statements, we need to use a proper Main method with STAThread attribute
internal class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        // Build configuration to read appsettings.json from application directory
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Initialize Serilog from configuration
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        try
        {
            Log.Information("PDF Combine Utility - Windows Application Started");
            Log.Information($"Application Base Directory: {AppContext.BaseDirectory}");
            Log.Information($"Thread Apartment State: {Thread.CurrentThread.GetApartmentState()}");

            // Ensure STA apartment state for proper dialog handling
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Run the main form
            Application.Run(new MainForm());

            Log.Information("Application closed successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            MessageBox.Show($"A fatal error occurred: {ex.Message}", "Fatal Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
