﻿using RefactorHelper.Comparer;
using RefactorHelper.RequestHandler;
using RefactorHelper.UIGenerator;
using RefactorHelper.SwaggerProcessor;
using RefactorHelper.Models.Config;
using RefactorHelper.Models;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace RefactorHelper.App
{
    public class RefactorHelperApp
    {
        private RefactorHelperSettings Settings { get; set; }
        private RefactorHelperState State { get; set; } = new();

        private SwaggerProcessorService SwaggerProcessorService { get; set; }
        private RequestHandlerService RequestHandlerService { get; set; }
        private CompareService CompareService { get; set; }
        private UIGeneratorService UIGeneratorService { get; set; }

        public RefactorHelperApp(RefactorHelperSettings settings)
        {
            Settings = SetDefaults(settings);

            // Setup Swagger Processor
            SwaggerProcessorService = new SwaggerProcessorService(Settings);

            // Setup Request Handler
            RequestHandlerService = new RequestHandlerService(
                new HttpClient
                {
                    BaseAddress = new Uri(Settings.BaseUrl1)
                },
                new HttpClient
                {
                    BaseAddress = new Uri(Settings.BaseUrl2)
                }, 
                Settings
            );

            // Compare Service
            CompareService = new CompareService();

            // UI Generator
            UIGeneratorService = new UIGeneratorService(Settings.ContentFolder, Settings.OutputFolder);
        }

        public async Task<string> OpenUI(HttpContext httpContext)
        {
            if(string.IsNullOrWhiteSpace(State.SwaggerJson))
            {
                var client = new HttpClient();
                var result = await client.GetAsync(Settings.SwaggerUrl);
                State.SwaggerJson = await result.Content.ReadAsStringAsync();
            }

            // Get requests from swagger
            State.Data = SwaggerProcessorService.ProcessSwagger(State.SwaggerJson);

            if (Settings.RunOnStart)
                await Run(httpContext);

            // Generate output
            UIGeneratorService.GenerateUI(State, httpContext);

            return State.GetCurrentRequest()?.ResultHtml ?? "";
        }

        public async Task Run(HttpContext httpContext)
        {
            // Perform api Requests
            await RequestHandlerService.QueryApis(State);

            // Get diffs on responses
            CompareService.CompareResponses(State);
        }

        public string GetResultPage(HttpContext context, int requestId)
        {
            State.CurrentRequest = requestId;
            return UIGeneratorService.GetSinglePageContent(State.GetCurrentRequest(), State, context);
        }

        public async Task<string> RetryCurrentRequest(HttpContext context)
        {
            // Perform single api request and update result
            await RequestHandlerService.QueryEndpoint(State.GetCurrentRequest());

            // Update Compare Result
            CompareService.CompareResponse(State.GetCurrentRequest());

            // Get Content Block to display in page
            UIGeneratorService.GetSinglePageContent(State.GetCurrentRequest(), State, context);

            return UIGeneratorService.GetSinglePageContent(State.GetCurrentRequest(), State, context);
        }

        public string GetRequestListHtml() => UIGeneratorService.GetRequestListHtml();

        public string GetContentFile(string filename) => File.ReadAllText(Path.Combine(Settings.ContentFolder, filename));

        private static RefactorHelperSettings SetDefaults(RefactorHelperSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.OutputFolder))
                settings.OutputFolder = Path.Combine(GetBinPath(), "Output");

            if (string.IsNullOrWhiteSpace(settings.ContentFolder))
                settings.ContentFolder = Path.Combine(GetBinPath(), "Content");

            return settings;
        }

        private static string GetBinPath() =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath ?? "");
    }
}
