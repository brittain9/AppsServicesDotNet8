using Serilog; // Log, LoggerConfiguration, RollingInterval
using Serilog.Core; // To use logger
using Serilogging.Models; // To use ProductPageView

// Create a new logger that will write to the console and to a text file, one-file-per-day, named with the date
using Logger log = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Assign the new logger to the static entry point for the logging.
Log.Logger = log;
Log.Information("The global logger has been configured.");

// Log some example entries of differing severity.
Log.Warning("Danger, Serilog, Danger!");
Log.Error("This is an error!");
Log.Fatal("Fatal Problem!");

ProductPageView pageView = new ProductPageView(){
    PageTitle = "Chai",
    SiteSection = "Beverages",
    ProductId = 1,
};

Log.Information("{@PageView} occurred at {Viewed}", pageView, DateTimeOffset.UtcNow);

Log.CloseAndFlush();