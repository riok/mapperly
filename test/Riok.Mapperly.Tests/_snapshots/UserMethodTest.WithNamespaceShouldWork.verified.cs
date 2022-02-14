//HintName: Mapper.g.cs
#nullable enable
namespace MyCompany.MyMapper
{
    public sealed class Mapper : IMapper
    {
        public static readonly IMapper Instance = new Mapper();
        public string Map(int source)
        {
            return source.ToString();
        }
    }
}