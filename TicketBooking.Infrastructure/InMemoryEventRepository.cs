using TicketBooking.Application;
using TicketBooking.Domain;

namespace TicketBooking.Infrastructure;

public class InMemoryEventRepository : IEventRepository
{
    private readonly  Dictionary<Guid, Event> _events = new Dictionary<Guid, Event>();
    public InMemoryEventRepository()
    {
        var seed = new Event("Concert-01", new[] { "A1", "A2", "A3", "A4", "A5" });
        _events[seed.Id] = seed;
    }
    
    public async Task<IEnumerable<Event>> GetAllAsync()  => Task.FromResult(_events);

    public Task<Event?> GetByIdAsync(Guid id)
        => Task.FromResult(_events.TryGetValue(id, out var evt) ? evt : null);

    public Task<Event> AddAsync(Event evt)
    {
        _events.Add(evt.Id, evt);
        return Task.FromResult(evt);
    }

    public Task<Event> UpdateAsync(Event evt) => Task.FromResult(_events[evt.Id]);
}