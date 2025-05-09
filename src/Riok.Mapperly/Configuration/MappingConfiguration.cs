using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Descriptors;

namespace Riok.Mapperly.Configuration;

public record MappingConfiguration(
    MapperAttribute Mapper,
    EnumMappingConfiguration Enum,
    MembersMappingConfiguration Members,
    IReadOnlyCollection<DerivedTypeMappingConfiguration> DerivedTypes,
    bool UseDeepCloning,
    SupportedFeatures SupportedFeatures
);
