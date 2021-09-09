using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Interactivity.Extensions;
using DisCatSharp.SlashCommands;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Skynet.Bot.Extensions;
using Skynet.Bot.Interfaces;
using Skynet.Bot.SlashCommands;

namespace Skynet.Bot
{
    public class Bot : BackgroundService, IBot
    {
        #region Discord Functionality

        public DiscordClient Client { get; }
        public InteractivityExtension Interactivity { get; }
        public SlashCommandsExtension SlashCommands { get; }
        public IServiceProvider ServiceProvider { get; }
        
        private DiscordGuild mainGuild;
        public DiscordGuild MainGuild
        {
            get
            {
                if(mainGuild == null)
                    if (Client.Guilds.Any())
                        #if DEBUG
                        mainGuild = Client.Guilds[885538658533376020];
                        #elif RELEASE
                        mainGuild = Client.Guilds[862027572455407616];
                        #endif

                return mainGuild;
            }
        }
        #endregion

        private readonly IConfiguration _configuration;
        private readonly ILogger<Bot> _logger;

        public Bot(IConfiguration configuration, ILogger<Bot> logger, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _logger = logger;
            
            var discordConfig = new DiscordConfiguration
            {
                Token = _configuration["Bot:Token"],
                TokenType = TokenType.Bot,
                MinimumLogLevel = LogLevel.Information,
                AutoReconnect = true,
                Intents = DiscordIntents.GuildEmojisAndStickers |
                          DiscordIntents.GuildMembers |
                          DiscordIntents.GuildInvites |
                          DiscordIntents.GuildMessageReactions |
                          DiscordIntents.GuildMessages |
                          DiscordIntents.GuildMessageTyping |
                          DiscordIntents.Guilds
            };

            Client = new DiscordClient(discordConfig);
            Client.MessageCreated += OnMessageCreated;
            Client.GuildMemberAdded += OnGuildMemberAdded;
            Client.ComponentInteractionCreated += OnComponentInteractionCreated;

            Interactivity = Client.UseInteractivity(new InteractivityConfiguration
            {
                PaginationBehaviour = PaginationBehaviour.WrapAround,
                PaginationDeletion = PaginationDeletion.DeleteMessage,
                PollBehaviour = PollBehaviour.DeleteEmojis,
                Timeout = TimeSpan.FromMinutes(5),
                PaginationEmojis = new PaginationEmojis
                {
                    Left = DiscordEmoji.FromName(Client, ":arrow_backward:", false),
                    Right = DiscordEmoji.FromName(Client, ":arrow_forward:", false),
                    SkipLeft = DiscordEmoji.FromName(Client, ":arrow_left:", false),
                    SkipRight = DiscordEmoji.FromName(Client, ":arrow_right:", false),
                    Stop = DiscordEmoji.FromName(Client, ":stop_button:", false)
                }
            });
            
            // Register all slash commands
            SlashCommandsExtension slashCommandsExtension = Client.UseSlashCommands(new SlashCommandsConfiguration()
            {
                Services = serviceProvider
            });

            slashCommandsExtension.RegisterCommandsFromAssembly<Program>();
        }

        private Task OnComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            throw new NotImplementedException();
        }

        private Task OnGuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Client.ConnectAsync();

            while (!stoppingToken.IsCancellationRequested)
                await Task.Delay(1000, stoppingToken);
        }
    }
}