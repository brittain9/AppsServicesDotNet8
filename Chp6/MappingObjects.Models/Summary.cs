namespace Northwind.ViewModels;

public record class Summary
{
    // Thesee properties can be initalized once but never changed.
    public string? FullName { get; init; }
    public decimal Total { get; init; }

    // This record will have a default parameterless constructor
    // This is automatically generated: public Summary()
}
