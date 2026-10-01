using TicketBooking.Application;
using TicketBooking.Domain;

namespace TicketBooking.Infrastructure;

public class InMemoryBookingRepository : IBookingRepository
{
    private readonly Dictionary<Guid, Booking> _bookings = new();

    public Task<Booking?> GetByIdAsync(Guid id) => Task.FromResult(_bookings.TryGetValue(id, out var booking) ? booking : null);

    public async Task<Booking> AddAsync(Booking booking)
    {
        return _bookings[booking.Id] = booking;
    }
}