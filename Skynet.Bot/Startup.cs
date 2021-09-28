using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Skynet.Bot.Interfaces;
using Skynet.Bot.Options;
using Skynet.Wiki;

namespace Skynet.Bot
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddWikiService();
            services.Configure<BotOptions>(Configuration.GetSection("Bot"));
            
            services.AddSingleton<IBot, Bot>();
            services.AddHostedService(serviceProvider => (Bot)serviceProvider.GetRequiredService<IBot>());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            
        }
    }
}