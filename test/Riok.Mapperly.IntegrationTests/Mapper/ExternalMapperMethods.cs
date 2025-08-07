using Riok.Mapperly.IntegrationTests.Models;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    public class ExternalMapperMethods
    {
        public static int MapStatic(int source) => source + 1;

        public static int ComputeSumStatic(IdObject testObject) => testObject.IdValue + 2;

        public static int IntValueStatic() => 13;

        public int MapInstance(int source) => source + 1;

        public int ComputeSumInstance(IdObject testObject) => testObject.IdValue + 2;

        public int IntValueInstance() => 13;
    }
}
