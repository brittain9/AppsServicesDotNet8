using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Northwind.Models;
// for appsettings
using static System.Environment; // CurrentDirectory 
using static System.IO.Path; // Combine
using Microsoft.Extensions.Configuration; // ConfigurationBuilder

DbContextOptionsBuilder<HierarchyDb> options = new();

// could put the stringbuilder code into a shared project or just put connectstring in json
SqlConnectionStringBuilder builder = new();

builder.DataSource = "tcp:127.0.0.1,1433";
builder.InitialCatalog = "Northwind";
builder.TrustServerCertificate = true;
builder.MultipleActiveResultSets = true;

builder.ConnectTimeout = 3; // fail faster. Default is 15

builder.IntegratedSecurity = false; // for windows integrated auth

// storing UserID and Password in appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile(Combine(CurrentDirectory, "appsettings.json"), optional: false, reloadOnChange: true)
    .Build();

builder.UserID = configuration.GetConnectionString("UserID");
builder.Password  = configuration.GetConnectionString("Password");

WriteLine(builder.ConnectionString); // Data Source=tcp:127.0.0.1,1433;Initial Catalog=Northwind;Integrated Security=False;User ID=sa;Password=s3cret-Ninja;Multiple Active Result Sets=True;Connect Timeout=3;Trust Server Certificate=True

options.UseSqlServer(builder.ConnectionString);

using (HierarchyDb db = new(options.Options)){
    bool deleted = await db.Database.EnsureDeletedAsync();
    WriteLine($"Database deleted: {deleted}");

    bool created = await db.Database.EnsureCreatedAsync();
    WriteLine($"Database created: {created}");

    WriteLine("SQL script used to create the database");
    WriteLine(db.Database.GenerateCreateScript());

    if ((db.Employees is not null) && (db.Students is not null)){
        db.Students.Add(new Student { Name = "Connor Roy", Subject = "Politics"});

        db.Employees.Add(new Employee { Name = "Kerry Castellabate", HireDate = DateTime.UtcNow });

        int result = db.SaveChanges();
        WriteLine($"{result} people added.");
    }

    if(db.Students is null || !db.Students.Any()){
        WriteLine("There are no students.");
    }
    else{
        foreach(Student student in db.Students){
            WriteLine("{0} studies {1}"
                , student.Name, student.Subject);
        }
    }

    if(db.Employees is null || !db.Employees.Any()){
        WriteLine("There are no employees");
    }
    else{
        foreach (Employee employee in db.Employees){
            WriteLine("{0} was hired on {1}", 
                employee.Name, employee.HireDate);
        }
    }

    if(db.People is null || !db.People.Any()){
        WriteLine("There are no people.");
    }
    else{
        foreach(Person person in db.People){
            WriteLine("{0} has an ID of {1}", 
                person.Name, person.Id);
        }
    }
}
