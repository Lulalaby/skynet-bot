using System;
using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.Interactivity;
using DisCatSharp.SlashCommands;

namespace Skynet.Bot.Interfaces
{
    public interface IBot
    {
        DiscordClient Client { get; }
        InteractivityExtension Interactivity { get; }
        SlashCommandsExtension SlashCommands { get; }
        IServiceProvider ServiceProvider { get; }
        DiscordGuild MainGuild { get; }
    }
}