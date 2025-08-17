using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;
using Riok.Mapperly.IntegrationTests.Mapper;
using Shouldly;
using Xunit;

namespace Riok.Mapperly.IntegrationTests
{
    public class DiCompositionMapperTest
    {
        [Fact]
        public void UsesDiMapperWhenAvailable()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IMapper<DINestedModel, DINestedDto>>(new NestedMapper());
            var sp = services.BuildServiceProvider();
            var mapper = new DiCompositionMapper(sp);
            var src = new DITestModel { Nested = new DINestedModel { Value = 10 } };
            var res = mapper.RootMap(src);
            res.Nested.Value.ShouldBe(15);
        }

        [Fact]
        public void FallsBackWhenServiceMissing()
        {
            var services = new ServiceCollection().BuildServiceProvider();
            var mapper = new DiCompositionMapper(services);
            var src = new DITestModel { Nested = new DINestedModel { Value = 10 } };
            var res = mapper.RootMap(src);
            // default generated mapping copies value as-is
            res.Nested.Value.ShouldBe(10);
        }

        [Fact]
        public void UsesDiMapperWhenAvailableForExistingMapping()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IExistingMapper<DINestedModel, DINestedDto>>(new NestedMapper());
            var sp = services.BuildServiceProvider();
            var mapper = new DiCompositionMapper(sp);
            var src = new DITestModel { Nested = new DINestedModel { Value = 10 } };
            var res = new DIDto();
            mapper.RootMap(src, res);
            res.Nested.Value.ShouldBe(20);
        }

        [Fact]
        public void UsesDiMapperWithinCollectionsWhenAvailable()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IMapper<DINestedModel, DINestedDto>>(new NestedMapper());
            var sp = services.BuildServiceProvider();
            var mapper = new DiCompositionMapper(sp);

            var src = new DITestModel
            {
                NestedList = [new DINestedModel { Value = 1 }, new DINestedModel { Value = 2 }],
                NestedDictionary = new Dictionary<string, DINestedModel>
                {
                    ["a"] = new() { Value = 3 },
                    ["b"] = new() { Value = 4 },
                },
                NestedSet = [new DINestedModel { Value = 5 }, new DINestedModel { Value = 6 }],
                // keep SortedSet empty to avoid comparer requirements on destination type
                NestedSortedSet = [],
            };

            var res = mapper.RootMap(src);

            res.NestedList.Select(x => x.Value).ShouldBe([6, 7], ignoreOrder: false);
            res.NestedDictionary["a"].Value.ShouldBe(8);
            res.NestedDictionary["b"].Value.ShouldBe(9);
            res.NestedSet.Select(x => x.Value).Order().ShouldBe([10, 11]);
            res.NestedSortedSet.Count.ShouldBe(0);
        }

        [Fact]
        public void FallsBackWithinCollectionsWhenServiceMissing()
        {
            var services = new ServiceCollection();
            var sp = services.BuildServiceProvider();
            var mapper = new DiCompositionMapper(sp);

            var src = new DITestModel
            {
                NestedList = [new DINestedModel { Value = 1 }, new DINestedModel { Value = 2 }],
                NestedDictionary = new Dictionary<string, DINestedModel>
                {
                    ["a"] = new() { Value = 3 },
                    ["b"] = new() { Value = 4 },
                },
                NestedSet = [new DINestedModel { Value = 5 }, new DINestedModel { Value = 6 }],
                // keep SortedSet empty to avoid comparer requirements on destination type
                NestedSortedSet = [],
            };

            var res = mapper.RootMap(src);

            res.NestedList.Select(x => x.Value).ShouldBe(new[] { 1, 2 }, ignoreOrder: false);
            res.NestedDictionary["a"].Value.ShouldBe(3);
            res.NestedDictionary["b"].Value.ShouldBe(4);
            res.NestedSet.Select(x => x.Value).Order().ShouldBe(new[] { 5, 6 });
            res.NestedSortedSet.Count.ShouldBe(0);
        }
    }

    // helper nested mapper used by tests
    sealed file class NestedMapper : IMapper<DINestedModel, DINestedDto>, IExistingMapper<DINestedModel, DINestedDto>
    {
        public DINestedDto Map(DINestedModel source) => new() { Value = source.Value + 5 };

        public void Map(DINestedModel source, DINestedDto destination) => destination.Value = source.Value + 10;
    }
}
