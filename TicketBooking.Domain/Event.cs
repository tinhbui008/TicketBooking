using TicketBooking.Domain.Exceptions;

namespace TicketBooking.Domain;

public class Event
{
    public Guid Id { get; }
    public string Name { get; }
    public IReadOnlyCollection<Seat> Seats => _seats;
    private readonly List<Seat> _seats;

    public Event(string name, IEnumerable<string> seatNames)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(seatNames);

        var names = seatNames.ToList();
        if (names.Count == 0)
            throw new ArgumentException("Event must have at least one seat", nameof(seatNames));
        if (names.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Seat name must not be empty", nameof(seatNames));
        if (names.Distinct().Count() != names.Count)
            throw new ArgumentException("Seat names must be unique", nameof(seatNames));

        Id = Guid.NewGuid();
        Name = name;
        _seats = names.Select(seatName => new Seat(seatName)).ToList();
    }

    public void EnsureSeatCanBeHeld(string seatName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seatName);

        var seat = _seats.FirstOrDefault(s => s.Name == seatName);
        if (seat == null)
        {
            throw new SeatNotFoundException(seatName);
        }

        if (seat.Status != ESeatStatus.Available)
        {
            throw new SeatNotAvailableException(seatName, seat.Status);
        }

        // seat.Hold();
    }
}