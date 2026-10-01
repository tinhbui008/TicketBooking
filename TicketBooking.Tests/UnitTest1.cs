using TicketBooking.Domain;

namespace TicketBooking.Tests;

public class UnitTest1
{
    [Fact]
    public void HoldSeat_AlreadyHeld_ShouldThrow()
    {
        var evt = new Event("Concert-01",  new []{ "A1"});
        evt.HoldSeat("A1");
        
        Assert.Throws<InvalidOperationException>(() => evt.HoldSeat("A"));
    }
}