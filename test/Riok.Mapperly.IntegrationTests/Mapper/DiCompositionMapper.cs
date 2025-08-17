using System;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.IntegrationTests.Dto;

namespace Riok.Mapperly.IntegrationTests.Mapper
{
    [Mapper]
    public partial class DiCompositionMapper
    {
        [MapperServiceProvider]
        public IServiceProvider Services { get; }

        public DiCompositionMapper(IServiceProvider services) => Services = services;

        public partial DIDto RootMap(DITestModel src);

        public partial void RootMap(DITestModel src, DIDto dst);
    }
}
