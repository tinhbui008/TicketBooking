using TicketBooking.Domain;
using TicketBooking.Domain.Exceptions;

namespace TicketBooking.Application;

public class BookingService(IEventRepository eventRepository, IBookingRepository bookingRepository)
{
    public async Task<Booking> CreateAsync(Guid eventId, string seatName)
    {
        var evt = await eventRepository.GetByIdAsync(eventId);
        if (evt == null)
            throw new EventNotFoundException(eventId);
        
        evt.HoldSeat(seatName);
        await eventRepository.UpdateAsync(evt);
        var booking = new Booking(eventId, seatName);
        return await bookingRepository.AddAsync(booking);
    }
}