using AutoMapper;
using MappingObjects.Mappers;
using Northwind.EntityModels;
using Northwind.ViewModels;
using System.Text;

// Set the text out encoduing to UTF-8 to support unicode chars
OutputEncoding = Encoding.UTF8;

Cart cart = new(
    customer: new(
        FirstName: "John",
        LastName: "Smith"
    ),
    Items: new(){
        new(ProductName: "Apples", UnitPrice: 0.49M, Quantity: 10),
        new(ProductName: "Bananas", UnitPrice: 0.99M, Quantity: 4),
    }
);

WriteLine("*** Orignal data before mapping");

WriteLine($"{cart.customer}");
foreach (var item in cart.Items){
    WriteLine($"{item}");
}

// Get the mapper config for converting a cart to a summary.
MapperConfiguration config = CartToSummaryMapper.GetMapperConfiguration();

IMapper mapper = config.CreateMapper();

// Perform the mapping
Summary summary = mapper.Map<Cart, Summary>(cart);

// Output the results
WriteLine("*** After mapping.");
WriteLine($"Summary: {summary.FullName} spent {summary.Total:C}");
