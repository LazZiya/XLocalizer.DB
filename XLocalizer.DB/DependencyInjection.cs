using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using XLocalizer.DB.EF;
using XLocalizer.DB.Models;
using XLocalizer.DataAnnotations;
using XLocalizer.Common;
using XLocalizer.Identity;
using XLocalizer.Translate;
using XLocalizer.ModelBinding;

namespace XLocalizer.DB
{
    /// <summary>
    /// XLocalizer DB extensions
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Add XLocalizer with database support using the built-in entity models
        /// </summary>
        /// <typeparam name="TContext">Application db context</typeparam>
        /// <param name="builder">builder</param>
        /// <returns></returns>
        public static IMvcBuilder AddXDbLocalizerDb<TContext>(this IMvcBuilder builder)
            where TContext : DbContext
        {
            var ops = new XLocalizerOptions();

            return builder
                .AddXDbLocalizer<TContext, DummyTranslator, XDbResource>(o => o = ops);
        }

        /// <summary>
        /// Add XLocalizer support using the built-in entity models
        /// </summary>
        /// <typeparam name="TContext">Application db context</typeparam>
        /// <param name="builder"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IMvcBuilder AddXDbLocalizer<TContext>(this IMvcBuilder builder, Action<XLocalizerOptions> options)
            where TContext : DbContext
        {
            return builder
                .AddXDbLocalizer<TContext, DummyTranslator, XDbResource>(options);
        }

        /// <summary>
        /// Add XLocalizer with database support using the built-in entity models,
        /// and use defined translation service type
        /// </summary>
        /// <typeparam name="TContext">Application db context</typeparam>
        /// <typeparam name="TTranslator">Translation service</typeparam>
        /// <param name="builder">builder</param>
        /// <returns></returns>
        public static IMvcBuilder AddXDbLocalizer<TContext, TTranslator>(this IMvcBuilder builder)
            where TContext : DbContext
            where TTranslator : IStringTranslator
        {
            var ops = new XLocalizerOptions();

            return builder
                .AddXDbLocalizer<TContext, TTranslator, XDbResource>(o => o = ops);
        }

        /// <summary>
        /// Add XLocalizer with database support using the built-in entity models,
        /// and use defined translation service type
        /// </summary>
        /// <typeparam name="TContext">Application db context</typeparam>
        /// <typeparam name="TTranslator">Translation service</typeparam>
        /// <param name="builder">builder</param>
        /// <param name="options">builder</param>
        /// <returns></returns>
        public static IMvcBuilder AddXDbLocalizer<TContext, TTranslator>(this IMvcBuilder builder, Action<XLocalizerOptions> options)
            where TContext : DbContext
            where TTranslator : IStringTranslator
        {
            return builder
                .AddXDbLocalizer<TContext, TTranslator, XDbResource>(options);
        }

        /// <summary>
        /// Add XLocalizer with DB support using customized entity models,
        /// and use defined translation service type
        /// </summary>
        /// <typeparam name="TContext">DbContext</typeparam>
        /// <typeparam name="TTranslator">Translation service</typeparam>
        /// <typeparam name="TResource">Type of resource DbEntity</typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMvcBuilder AddXDbLocalizer<TContext, TTranslator, TResource>(this IMvcBuilder builder)
            where TContext : DbContext
            where TTranslator : IStringTranslator
            where TResource : class, IXDbResource, new()
        {
            var ops = new XLocalizerOptions();

            return builder
                .AddXDbLocalizer<TContext, TTranslator, TResource>(o => o = ops);
        }

        /// <summary>
        /// Add XLocalizer with DB support using customized entity models,
        /// and use defined translation service type
        /// </summary>
        /// <typeparam name="TContext">DbContext</typeparam>
        /// <typeparam name="TTranslator">Translation service</typeparam>
        /// <typeparam name="TResource">Type of localization DbEntity</typeparam>
        /// <param name="options">XLDbOptions</param>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMvcBuilder AddXDbLocalizer<TContext, TTranslator, TResource>(this IMvcBuilder builder, Action<XLocalizerOptions> options)
            where TContext : DbContext
            where TTranslator : IStringTranslator
            where TResource : class, IXDbResource, new()
        {
            builder.Services.Configure<XLocalizerOptions>(options);

            // ExpressMemoryCache for caching localized values
            builder.Services.AddSingleton<ExpressMemoryCache>();

            // Try add a default data exporter service, unless another service is defined in startup
            builder.Services.TryAddSingleton<IDbResourceExporter, EFDbResourceExporter<TContext>>();

            // Try add a default resx service.
            // if another service has been registered this will be bypassed, and the other service will be in use.
            builder.Services.TryAddSingleton<IDbResourceProvider, EFDbResourceProvider<TContext>>();

            // Register IStringLocalizer for the default shared resource and translation type
            // This is the default (shared) resource entity and translation
            builder.Services.AddSingleton<IStringLocalizer, DbStringLocalizer<TResource>>();
            builder.Services.AddSingleton<IStringLocalizerFactory, DbStringLocalizerFactory<TResource>>();

            // Register IHtmlLocalizer for the default shared resource and translation type
            // This is the default (shared) resource entity and translation
            builder.Services.AddSingleton<IHtmlLocalizer, DbHtmlLocalizer<TResource>>();
            builder.Services.AddSingleton<IHtmlLocalizerFactory, DbHtmlLocalizerFactory<TResource>>();
            
            // Register generic IDbStringLocalizer for user defined resource and translation entities
            // e.g. IDbStringLocalizer<ProductArea, ProductAreaTranslation>
            // e.g. IDbStringLocalizer<UserArea, UserAreaTranslation>
            builder.Services.AddTransient(typeof(IStringLocalizer<>), typeof(DbStringLocalizer<>));
            builder.Services.AddTransient(typeof(IHtmlLocalizer<>), typeof(DbHtmlLocalizer<>));

            // Express localizer factories for creating localizers with the default shared resource type
            // Use .Create() method for creating localizers.
            builder.Services.AddSingleton<IExpressStringLocalizerFactory, DbStringLocalizerFactory<TResource>>();
            builder.Services.AddSingleton<IExpressHtmlLocalizerFactory, DbHtmlLocalizerFactory<TResource>>();
            

            // Configure route culture provide
            return builder.AddDbDataManagers<TContext>()
                          .AddDbDataAnnotationsLocalization<TResource>()
                          .AddModelBindingLocalization()
                          .AddIdentityErrorsLocalization()
                          .WithTranslationService<TTranslator>();
        }

        /// <summary>
        /// Add DataAnnotations localization with specified resource type.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMvcBuilder AddDbDataAnnotationsLocalization<TResource>(this IMvcBuilder builder)
            where TResource : class, IXDbResource
        {
            // Add ExpressValdiationAttributes to provide error messages by default without using ErrorMessage="..."
            builder.Services.AddTransient<IValidationAttributeAdapterProvider, ExpressValidationAttributeAdapterProvider>();

            // Add data annotations locailzation
            builder.AddDataAnnotationsLocalization(ops =>
            {
                // This will look for localization resource of default type T (shared resource)
                ops.DataAnnotationLocalizerProvider = (type, factory) => factory.Create(typeof(TResource));
            });

            return builder;
        }
    }
}
