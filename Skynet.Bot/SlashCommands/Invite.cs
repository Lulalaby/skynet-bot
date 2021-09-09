using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.SlashCommands;

namespace Skynet.Bot.SlashCommands
{
    public class Invite : SlashCommandModule
    {
        [SlashCommand("invite", "Get the invite link to the server")]
        public async Task Command(InteractionContext context)
        {
            DiscordInteractionResponseBuilder builder = new();

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithTitle("Invitation")
                .WithAuthor("Skynet")
                .WithDescription("https://discord.gg/ud4rkGAe9z")
                .WithColor(DiscordColor.Cyan);

            builder.AddEmbed(embedBuilder.Build());

            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
        }
    }
}