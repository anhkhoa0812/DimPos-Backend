using DimPos.Orchestrator.SagaState.BrandMenuItems.UpdateBrandMenuItem.Activities;
using MassTransit;
using SharedProject.Events.UpdateBrandMenuItem;

namespace DimPos.Orchestrator.SagaState.BrandMenuItems.UpdateBrandMenuItem;

public class UpdateBrandMenuItemSageStateMachine : MassTransitStateMachine<UpdateBrandMenuItemSageState>
{
    public UpdateBrandMenuItemSageStateMachine()
    {
        InstanceState(x => x.CurrentState);
        Event(() => UpdateBrandMenuItemResponseEvent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
            x.SelectId(ctx => ctx.Message.CorrelationId);
            x.OnMissingInstance(m => m.Discard());
        });
        Initially(
            When(UpdateBrandMenuItemResponseEvent)
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Received UpdateBrandMenuItemResponse for {context.CorrelationId!.Value}");
                })
                .Activity(x => x.OfType<CreateStorePriceForBrandMenuItemActivity>())
                .Then(context =>
                {
                    Console.WriteLine($"[Saga] Published CreateStorePriceForBrandMenuItemRequest for {context.CorrelationId!.Value}");
                })
                .Finalize()
        );
        SetCompletedWhenFinalized();
    }
    public Event<UpdateBrandMenuItemResponseModel> UpdateBrandMenuItemResponseEvent { get; private set; } = null!;
}