using Riok.Mapperly.Abstractions.ReferenceHandling;

namespace Riok.Mapperly.Abstractions.Tests.ReferenceHandling;

public class PreserveReferenceHandlerTest
{
    private readonly IReferenceHandler _handler = new PreserveReferenceHandler();

    [Fact]
    public void EmptyReferenceHandlerShouldReturnFalse()
    {
        _handler.TryGetReference(new MyObj(), out MyDto? _).ShouldBeFalse();
    }

    [Fact]
    public void SetReferenceShouldBeReturned()
    {
        var myObj = new MyObj { Value = 1 };
        var myDto = new MyDto { Value = 2 };
        _handler.SetReference(myObj, myDto);
        _handler.TryGetReference(myObj, out MyDto? mySecondDto).ShouldBeTrue();
        myDto.ShouldBe(mySecondDto);
    }

    class MyDto
    {
        public int Value { get; set; }
    }

    class MyObj
    {
        public int Value { get; set; }
    }
}
