#nullable enable
namespace Riok.Mapperly.IntegrationTests.Mapper
{
    public static partial class CircularReferenceMapper
    {
        public static partial Riok.Mapperly.IntegrationTests.Dto.CircularReferenceDto ToDto(Riok.Mapperly.IntegrationTests.Models.CircularReferenceObject obj)
        {
            return MapToCircularReferenceDto(obj, new Riok.Mapperly.Abstractions.ReferenceHandling.Internal.PreserveReferenceHandler());
        }

        private static Riok.Mapperly.IntegrationTests.Dto.CircularReferenceDto MapToCircularReferenceDto(Riok.Mapperly.IntegrationTests.Models.CircularReferenceObject source, Riok.Mapperly.Abstractions.ReferenceHandling.IReferenceHandler refHandler)
        {
            if (refHandler.TryGetReference<Riok.Mapperly.IntegrationTests.Models.CircularReferenceObject, Riok.Mapperly.IntegrationTests.Dto.CircularReferenceDto>(source, out var existingTargetReference))
                return existingTargetReference;
            var target = new Riok.Mapperly.IntegrationTests.Dto.CircularReferenceDto();
            refHandler.SetReference<Riok.Mapperly.IntegrationTests.Models.CircularReferenceObject, Riok.Mapperly.IntegrationTests.Dto.CircularReferenceDto>(source, target);
            target.Value = source.Value;
            if (source.Parent != null)
            {
                target.Parent = MapToCircularReferenceDto(source.Parent, refHandler);
            }

            return target;
        }
    }
}