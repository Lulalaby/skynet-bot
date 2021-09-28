using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DeepAI;
using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;

namespace Skynet.Bot.Analyzers
{
    public class FoodAnalyzer : IMessageAnalyzer
    {
        readonly Regex _action = new (@".*(([iI]\'m|im|[iI]\sam|go|going|am)\s.*(eat|hungry|starving))$", RegexOptions.IgnoreCase);
        private readonly Regex _active = new (@".*(([iI]\'m|im|[iI]\sam|am)\s.*(munching|chowing|chowing\sdown|scarfing\sdown|eating)).*", RegexOptions.IgnoreCase);
        
        private readonly Random _random = new();
        private readonly string[] reactions = 
        {
            ":canned_food:",
            ":pizza:",
            ":hamburger:",
            ":fork_and_knife:",
            ":pancakes:"
        };

        public async Task<bool> ProcessMessage(DiscordClient sender, MessageCreateEventArgs message)
        {
            if (message.Handled)
                return false;
            
            var result = _action.Match(message.Message.Content);

            if (result.Success)
            {
                var emoji = DiscordEmoji.FromName(sender, reactions[_random.Next(0, reactions.Length)]);
                await message.Message.CreateReactionAsync(emoji);

                message.Handled = true;
                return true;
            }
            return false;
        }
    }
}