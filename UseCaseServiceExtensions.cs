using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CheckYourEligibility.Shared.Extensions
{
    public static class UseCaseServiceCollectionExtensions
    {
        public static IServiceCollection AddAllUseCases(this IServiceCollection services, Assembly assembly)
        {
            var useCases = assembly.GetTypes()
                .Where(type => type.Name.EndsWith("UseCase")
                    && !type.IsInterface
                    && !type.IsAbstract);

            foreach (var useCase in useCases)
            {
                var useCaseInterface = useCase.GetInterfaces()
                    .FirstOrDefault(i => i.Name.EndsWith("UseCase"));

                if (useCaseInterface != null)
                {
                    services.AddScoped(useCaseInterface, useCase);
                }
            }

            return services;
        }

        // Specific method for Parent app usecases
        public static IServiceCollection AddParentUseCases(this IServiceCollection services, Assembly assembly)
        {
            return services.AddUseCasesWithPrefix(assembly, "Parent");
        }

        // Specific method for Admin app usecases
        public static IServiceCollection AddAdminUseCases(this IServiceCollection services, Assembly assembly)
        {
            return services.AddUseCasesWithPrefix(assembly, "Admin");
        }

        private static IServiceCollection AddUseCasesWithPrefix(this IServiceCollection services, Assembly assembly, string prefix)
        {
            var useCases = assembly.GetTypes()
                .Where(type => type.Name.EndsWith("UseCase")
                    && type.Name.StartsWith(prefix)
                    && !type.IsInterface
                    && !type.IsAbstract);

            foreach (var useCase in useCases)
            {
                var useCaseInterface = useCase.GetInterfaces()
                    .FirstOrDefault(i => i.Name.EndsWith("UseCase"));

                if (useCaseInterface != null)
                {
                    services.AddScoped(useCaseInterface, useCase);
                }
            }

            return services;
        }
    }
}