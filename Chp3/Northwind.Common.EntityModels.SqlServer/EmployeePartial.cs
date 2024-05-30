using System.ComponentModel.DataAnnotations.Schema; // [NotMapped]

namespace Northwind.EntityModels;

// this code is in a seperate partial class so that you can regenerate your code using dotnet ef and not overwrite this
public partial class Employee : IHasLastRefreshed
{
    [NotMapped]
    public DateTimeOffset LastRefreshed{get; set;}
}
