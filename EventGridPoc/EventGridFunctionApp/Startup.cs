using ListenEventGrid.Services.InfoService;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: FunctionsStartup(typeof(ListenEventGrid.Startup))]

namespace ListenEventGrid
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            string sqlConnectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
            builder.Services.AddSingleton<IInfoService>(sp =>
            {
                return new InfoService(sqlConnectionString);
            });
        }
    }
}