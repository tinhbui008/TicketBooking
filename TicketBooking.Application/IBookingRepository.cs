using TicketBooking.Domain;

namespace TicketBooking.Application;

public interface IBookingRepository
{
    Task<Booking> GetByIdAsync(Guid id);
    Task<Booking> AddAsync(Booking  booking);
}