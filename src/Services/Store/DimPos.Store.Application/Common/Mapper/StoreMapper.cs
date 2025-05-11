using DimPos.Store.Application.Features.Stores.Command.CreateStore;
using Riok.Mapperly.Abstractions;

namespace DimPos.Store.Application.Common.Mapper;

[Mapper]
public static partial class StoreMapper
{
    public static partial Domain.Entities.Store ToStores(CreateStoreCommand createStoreCommand); 
}