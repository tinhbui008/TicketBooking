using TicketBooking.Application;
using TicketBooking.Domain;

namespace TicketBooking.Infrastructure;

public class InMemoryBookingRepository : IBookingRepository
{
    private readonly Dictionary<Guid, Booking> _events = new();
    
    public async Task<Booking> GetByIdAsync(Guid id)
    {
        return _events.ContainsKey(id) ? _events[id] : null;
    }

    public async Task<Booking> AddAsync(Booking booking)
    {
        return _events[booking.Id] = booking;
    }
}