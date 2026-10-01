using TicketBooking.Domain;

namespace TicketBooking.Application;

public class BookingService(IEventRepository eventRepository, IBookingRepository bookingRepository)
{
    public async Task<Booking> Create(Guid eventId, string seatName)
    {
        var evt = await eventRepository.GetByIdAsync(eventId);
        if (evt == null)
            throw new KeyNotFoundException($"Event with name {seatName} does not exist");
        
        evt.HoldSeat(seatName);
        await eventRepository.AddAsync(evt);
        var booking = new Booking(eventId, seatName);
        return await bookingRepository.AddAsync(booking);
    }
}