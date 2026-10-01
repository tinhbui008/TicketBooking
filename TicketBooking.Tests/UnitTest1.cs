using TicketBooking.Domain;

namespace TicketBooking.Tests;

public class UnitTest1
{
    [Fact]
    public void HoldSeat_AlreadyHeld_ShouldThrow()
    {
        var evt = new Event("Concert-01",  new []{ "A1"});
        evt.HoldSeat("A1");
        
        Assert.Throws<InvalidOperationException>(() => evt.HoldSeat("A1"));
    }
    
    [Fact]
    public void HoldSeat_AvailableSeat_ShouldBecomeHeld() 
    {
        var evt = new Event("Concert-01",  new []{ "A1"});
        evt.HoldSeat("A1");

        var seat = evt.Seats.FirstOrDefault(s => s.Name == "A1");
        
        Assert.Equal(ESeatStatus.Held, seat.Status);
    }

    [Fact]
    public void HoldSeat_SeatNotExist_ShouldThrow()
    {
        var evt = new Event("Concert-01",  new []{ "A1"});

        Assert.Throws<KeyNotFoundException>(() => evt.HoldSeat("Z9"));
    }

    [Fact]
    public void NewBooking_ShouldBe_SeatHeld()
    {
        var evt = new Booking(Guid.NewGuid(), "A1");
    }
}