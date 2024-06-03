using System.Security.Cryptography;
using Microsoft.Win32.SafeHandles;

namespace Northwind.Services;

public class DiscountService
{
    private TimeProvider _timeProvider;

    public DiscountService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }
    public decimal GetDiscount(){
        // var now = DateTime.UtcNow;

        var now = _timeProvider.GetUtcNow();

        return now.DayOfWeek switch{
            DayOfWeek.Saturday or DayOfWeek.Sunday => 0.2M,
            _ => 0m
        };
    }
}
