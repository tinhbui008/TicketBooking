using TicketBooking.Domain;

namespace TicketBooking.Application;

public interface IEventRepository
{
    Task<IEnumerable<Event>> GetAllAsync();
    Task<Event?> GetByIdAsync(Guid id);
    Task<Event> AddAsync(Event evt);
    Task<Event> UpdateAsync(Event evt);
}