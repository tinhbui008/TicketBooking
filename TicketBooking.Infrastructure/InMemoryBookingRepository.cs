using TicketBooking.Application;
using TicketBooking.Domain;
using System.Collections.Concurrent;

namespace TicketBooking.Infrastructure;
public class InMemoryBookingRepository : IBookingRepository
{
    private readonly ConcurrentDictionary<Guid, Booking> _bookings = new();

    public Task<Booking?> GetByIdAsync(Guid id) => Task.FromResult(_bookings.TryGetValue(id, out var booking) ? booking : null);

    public Task<Booking> AddAsync(Booking booking)
    {
        if (!_bookings.TryAdd(booking.Id, booking))
            throw new InvalidOperationException($"Booking {booking.Id} already exists");
        return Task.FromResult(booking);
    }
}