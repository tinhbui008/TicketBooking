using TicketBooking.Application;
using TicketBooking.Domain;

namespace TicketBooking.Infrastructure;

public class InMemoryEventRepository : IEventRepository
{
    private readonly  Dictionary<Guid, Event> _events = new Dictionary<Guid, Event>();
    
    public async Task<Event> GetByIdAsync(Guid id)
    {
        return _events.ContainsKey(id) ? _events[id] : null;
    }

    public async Task<Event> AddAsync(Event evt)
    {
        _events.Add(evt.Id, evt);
        return evt;
    }

    public async Task<Event> UpdateAsync(Event evt)
    {
        _events[evt.Id] = evt;
        return evt;
    }
}