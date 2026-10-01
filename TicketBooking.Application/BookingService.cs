using TicketBooking.Domain;

namespace TicketBooking.Application;

public class BookingService(IBookingRepository  repository)
{
    public async Task<Booking> GetByIdAsync(Guid id) => await repository.GetByIdAsync(id);

    public async Task<Booking> AddAsync(Booking booking)
    {
        throw new NotImplementedException();
    }
}