using TicketBooking.Application;
using TicketBooking.Domain;

namespace TicketBooking.Infrastructure;

public class InMemoryBookingRepository : IBookingRepository
{
    private readonly Dictionary<Guid, Booking> _bookings = new();

    public Task<Booking?> GetByIdAsync(Guid id) => Task.FromResult(_bookings.TryGetValue(id, out var booking) ? booking : null);

    public Task<Booking> AddAsync(Booking booking) => Task.FromResult(_bookings[booking.Id] = booking);
}