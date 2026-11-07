using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SomaUI;

public static class ServiceCollectionExtensions {

    public static IServiceCollection AddUI(this IServiceCollection services) {
        services.TryAddScoped<IToastService, ToastService>();
        services.TryAddScoped<IPopupService, PopupService>();
        services.TryAddScoped<DialogService>();
        services.TryAddScoped<SheetService>();
        services.TryAddScoped<TooltipInteractionService>();
        services.TryAddScoped<FloatingContentService>();

        return services;
    }
}
