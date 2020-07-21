using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace XLocalizer.DB.EF
{
    /// <summary>
    /// DI works...
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Register DbDataManagers
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMvcBuilder AddDbDataManagers<TContext>(this IMvcBuilder builder)
            where TContext : DbContext
        {
            builder.Services.AddSingleton<IDbResourceManager, EFResourceManager<TContext>>();
            builder.Services.AddSingleton<IDbCultureManager, EFCultureManager<TContext>>();

            return builder;
        }
    }
}
