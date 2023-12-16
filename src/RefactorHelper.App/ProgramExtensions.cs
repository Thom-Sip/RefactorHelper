﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RefactorHelper.Models.Config;
using System.Diagnostics;

namespace RefactorHelper.App
{
    public static class ProgramExtensions
    {
        public static IServiceCollection AddRefactorHelper(this IServiceCollection services, RefactorHelperSettings settings)
        {
            services.AddSingleton<RefactorHelperApp>(new RefactorHelperApp(settings));

            return services;
        }

        public static void AddRefactorHelperEndpoint(this WebApplication app)
        {
            app.MapGet("/run-refactor-helper", async context => {
                context.Response.Headers["content-type"] = "text/html";
                var service = app.Services.GetRequiredService<RefactorHelperApp>();
                var result = await service.Run();

                if (result.Any())
                {
                    var p = new Process
                    {
                        StartInfo = new ProcessStartInfo(result.First())
                        {
                            UseShellExecute = true
                        }
                    };
                    p.Start();
                }

                await context.Response.WriteAsync("Thank you for using RefactorHelper");
            });
        }
    }
}
