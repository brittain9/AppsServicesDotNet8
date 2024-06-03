using Microsoft.Extensions.Hosting; // To use IHost, Host

using Microsoft.Extensions.DependencyInjection;
using WorkingWithCultures; // To use AddLocalization, AddTransiet<T>

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>{
        services.AddLocalization(options => {
            options.ResourcesPath = "Resources";
        });
        services.AddTransient<AlexResources>();
    })
    .Build();

OutputEncoding = System.Text.Encoding.UTF8;

OutputCultures("Current culture");

string[] cultureCodes = { "da-DK", "en-GB", "fa-IR", "fr-CA", "fr-FR", "he-IL", "pl-PL", "sl-SL", "el-GR"};

foreach (string code in cultureCodes){
    CultureInfo culture = CultureInfo.GetCultureInfo(code);
    WriteLine($"{culture.Name}: {culture.EnglishName} / {culture.NativeName}");
}

WriteLine();
Write("Enter an ISO culture code: ");
string? cultureCode = ReadLine();
if (string.IsNullOrWhiteSpace(cultureCode))
{
  cultureCode = "en-US";
}
CultureInfo ci;
try
{
  ci = CultureInfo.GetCultureInfo(cultureCode);
}
catch (CultureNotFoundException)
{
  WriteLine($"Culture code not found: {cultureCode}");
  WriteLine("Exiting the app.");
  return;
}
// change the current cultures on the thread
CultureInfo.CurrentCulture = ci;
CultureInfo.CurrentUICulture = ci;
OutputCultures("After changing the current culture");

AlexResources resources = host.Services.GetRequiredService<AlexResources>();

Write(resources.GetEnterYourNamePrompt());
string? name = ReadLine();
if(string.IsNullOrWhiteSpace(name)){
    name = "Bob";
}

Write(resources.GetEnterYourDOBPrompt());
string? dobText = ReadLine();

if(string.IsNullOrWhiteSpace(dobText)){
    dobText = ci.Name switch
    {
      "en-US" or "fr-CA" => "1/27/1990",
      "da-DK" or "fr-FR" or "pl-PL" => "27/1/1990",
      "fa-IR" => "1990/1/27",
      _ => "1/27/1990"
    };
}

Write(resources.GetEnterYourSalaryPrompt()); 
string? salaryText = ReadLine();
if (string.IsNullOrWhiteSpace(salaryText))
{
  salaryText = "34500";
}

DateTime dob = DateTime.Parse(dobText);
int minutes = (int)DateTime.Today.Subtract(dob).TotalMinutes;
decimal salary = decimal.Parse(salaryText);
WriteLine(resources.GetPersonDetails(name, dob, minutes, salary));