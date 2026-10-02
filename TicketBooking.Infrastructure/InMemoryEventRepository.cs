using TicketBooking.Application;
using TicketBooking.Domain;
using System.Collections.Concurrent;

namespace TicketBooking.Infrastructure;

public class InMemoryEventRepository : IEventRepository
{
    private readonly ConcurrentDictionary<Guid, Event> _events = new();
    public InMemoryEventRepository()
    {
        var seed = new Event("Concert-01", new[] { "A1", "A2", "A3", "A4", "A5" });
        _events[seed.Id] = seed;
    }
    
    public Task<IEnumerable<Event>> GetAllAsync() => Task.FromResult<IEnumerable<Event>>(_events.Values);

    public Task<Event?> GetByIdAsync(Guid id)
        => Task.FromResult(_events.TryGetValue(id, out var evt) ? evt : null);

    public Task<Event> AddAsync(Event evt)
    {
        if (!_events.TryAdd(evt.Id, evt))
            throw new InvalidOperationException($"Event {evt.Id} already exists");
        return Task.FromResult(evt);
    }

    public Task<Event> UpdateAsync(Event evt)
    {
        _events[evt.Id] = evt;
        return Task.FromResult(evt);
    }
}