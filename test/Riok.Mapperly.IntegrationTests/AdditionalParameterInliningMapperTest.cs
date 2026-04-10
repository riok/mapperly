using System.Collections.Generic;
using System.Linq;
using Riok.Mapperly.IntegrationTests.Mapper;
using Riok.Mapperly.IntegrationTests.Models;
using Shouldly;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    public class AdditionalParameterInliningMapperTest
    {
        [Fact]
        public void ProjectWithAdditionalParameterShouldInline()
        {
            var objects = new List<IdObject> { new() { IdValue = 42 } }.AsQueryable();
            var result = objects.ProjectWithAdditionalParameter(100).ToList();
            result.ShouldHaveSingleItem();
            result[0].IdValue.ShouldBe(42);
            result[0].ValueFromParameter.ShouldBe(100);
        }
    }
}
