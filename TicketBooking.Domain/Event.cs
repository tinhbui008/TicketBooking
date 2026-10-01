namespace TicketBooking.Domain;

public class Event
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public IReadOnlyCollection<Seat> Seats => _seats;
    private readonly List<Seat> _seats;

    public Event(string name, IEnumerable<string> seatNames)
    {
        Id = Guid.NewGuid();
        Name = name;
        _seats = seatNames.Select(name => new Seat(name)).ToList();
    }

    public void HoldSeat(string seatName)
    {
        if (string.IsNullOrEmpty(seatName))
        {
            throw new ArgumentNullException(nameof(seatName));
        }
       var seat = _seats.FirstOrDefault(s => s.Name == seatName);
       if (seat == null)
       {
           throw new KeyNotFoundException($"Seat with name {seatName} does not exist");
       }
       
       if (seat.Status != ESeatStatus.Available)
       {
           throw new InvalidOperationException($"Seat  with name {seatName} is holding");
       }
       
       seat.Hold();
    }
}