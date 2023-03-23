//HintName: Mapper.g.cs
#nullable enable
public partial class Mapper
{
    private partial System.Linq.IQueryable<B> Map(System.Linq.IQueryable<A> source)
    {
#nullable disable
        return System.Linq.Queryable.Select(source, x => new B() { StringValue2 = x.StringValue, NestedValue = new D() { IntValue = (int)x.NestedValue.LongValue, NestedValue = new F() { ShortValue = x.NestedValue.NestedValue.ShortValue } } });
#nullable enable
    }

    private partial B MapToB(A source)
    {
        var target = new B();
        target.StringValue2 = source.StringValue;
        target.NestedValue = MapToD(source.NestedValue);
        return target;
    }

    private partial D MapToD(C source)
    {
        var target = new D();
        target.IntValue = (int)source.LongValue;
        target.NestedValue = MapToF(source.NestedValue);
        return target;
    }

    private F MapToF(E source)
    {
        var target = new F();
        target.ShortValue = source.ShortValue;
        return target;
    }
}
