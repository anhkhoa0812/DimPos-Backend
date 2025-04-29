using DimPos.Catalog.Application.Features.ModifierGroups.Command.CreateModifierGroups;
using DimPos.Catalog.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace DimPos.Catalog.Application.Common.Mapper;
[Mapper]
public static partial class ModifierGroupsMapper
{
    public static partial ModifierGroups ToModifierGroups(CreateModifierGroupsCommand command);
}