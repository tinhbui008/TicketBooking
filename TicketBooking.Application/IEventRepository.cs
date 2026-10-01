using TicketBooking.Domain;

namespace TicketBooking.Application;

public interface IEventRepository
{
    Task<Event> GetByIdAsync(Guid id);
    Task<Event> AddAsync(Event evt);
}