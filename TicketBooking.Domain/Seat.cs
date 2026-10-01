namespace TicketBooking.Domain;

public class Seat
{
    public Guid Id { get; }
    public string Name { get; private set; }
    public ESeatStatus Status { get; private set; }

    public Seat(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
        Status = ESeatStatus.Available;
    }

    internal void Hold()
    {
        Status = ESeatStatus.Held;
    }
    
    internal void Book()
    {
        Status  = ESeatStatus.Booked;
    }

    internal void Release()
    {
        Status  = ESeatStatus.Available;
    }
}