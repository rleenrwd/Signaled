
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Signaled.Application.Events;

namespace Signaled.Infrastructure.Persistence;

public class EventNumberGenerator : IEventNumberGenerator
{ 
    private readonly SignaledDbContext _context;

    public EventNumberGenerator(SignaledDbContext context)
    {
        _context = context;
    }


    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {   
        // Stores ONE "long" value in a list
        var value = await _context.Database.SqlQueryRaw<long>(
            "SELECT NEXT VALUE FOR [dbo].[EventNumberSequence] AS [Value]") 
            .ToListAsync(cancellationToken); 

            long number = value.Single(); // Stores the only value in the value list

            return "EVT-" + number.ToString("D4", CultureInfo.InvariantCulture); // D4 formats the number with four digits(using leading 0s when needed). CultureInfo.InvariantCulture keeps formatting consistent
    }
}