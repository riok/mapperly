using System.Collections.Generic;

namespace Riok.Mapperly.IntegrationTests.Dto
{
    public class DITestModel
    {
        public DINestedModel Nested { get; set; } = new();

        public List<DINestedModel> NestedList { get; set; } = [];
        public Dictionary<string, DINestedModel> NestedDictionary { get; set; } = new();
        public HashSet<DINestedModel> NestedSet { get; set; } = [];
        public SortedSet<DINestedModel> NestedSortedSet { get; set; } = [];
    }
}
