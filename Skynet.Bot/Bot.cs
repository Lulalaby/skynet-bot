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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Skynet.Bot.Analyzers;
using Skynet.Bot.Extensions;
using Skynet.Bot.Interfaces;
using Skynet.Bot.Options;

namespace Skynet.Bot
{
    public class Bot : BackgroundService, IBot
    {
        #region Discord Functionality

        public DiscordClient Client { get; }
        public InteractivityExtension Interactivity { get; }
        public SlashCommandsExtension SlashCommands { get; }
        public IServiceProvider ServiceProvider { get; }
        
        public DiscordGuild MainGuild { get; private set; }
        #endregion

        private readonly IConfiguration _configuration;
        private readonly ILogger<Bot> _logger;
        private IMessageAnalyzer[] _messageAnalyzers;
        
        public Bot(IConfiguration configuration, ILogger<Bot> logger, IServiceProvider serviceProvider,
            IOptions<BotOptions> options)
        {
            _configuration = configuration;
            _logger = logger;
            
            var discordConfig = new DiscordConfiguration
            {
                Token = options.Value.Token,
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
            
            // automatically register our slash commands
            slashCommandsExtension.RegisterCommandsFromAssembly<Program>();
            
            InitializeAnalyzers();
        }

        private void InitializeAnalyzers()
        {
            // Register 
            var analyzerTypes = GetType().Assembly
                .ExportedTypes
                .Where(x => x.IsAssignableTo(typeof(IMessageAnalyzer)) && !x.IsAbstract)
                .ToArray();

            _messageAnalyzers = new IMessageAnalyzer[analyzerTypes.Length];

            for (int i = 0; i < analyzerTypes.Length; i++)
                _messageAnalyzers[i] = (IMessageAnalyzer) ActivatorUtilities.CreateInstance(ServiceProvider, analyzerTypes[i]);
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
            // We don't care if you're a bot
            if (e.Author.IsBot)
                return;

            foreach (var analyzer in _messageAnalyzers)
                if (!e.Handled)
                {
                    bool result = await analyzer.ProcessMessage(sender, e);
                    
                    if (result)
                        return;
                }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Client.ConnectAsync();
            
            #if DEBUG
            MainGuild = await Client.GetGuildAsync(885538658533376020);
            #elif RELEASE
            MainGuild = await Client.GetGuildAsync(862027572455407616);
            #endif
            
            while (!stoppingToken.IsCancellationRequested)
                await Task.Delay(1000, stoppingToken);
        }
    }
}