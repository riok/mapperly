using System.Collections.Generic;

namespace Riok.Mapperly.IntegrationTests.Dto
{
    public class DIDto
    {
        public DINestedDto Nested { get; set; } = new();
        public List<DINestedDto> NestedList { get; set; } = [];
        public Dictionary<string, DINestedDto> NestedDictionary { get; set; } = new();
        public HashSet<DINestedDto> NestedSet { get; set; } = [];
        public SortedSet<DINestedDto> NestedSortedSet { get; set; } = [];
    }
}
