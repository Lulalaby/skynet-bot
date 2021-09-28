using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.EventArgs;

namespace Skynet.Bot.Analyzers
{
    /// <summary>
    /// Consume a discord message and do something based on it
    /// </summary>
    public interface IMessageAnalyzer
    {
        /// <summary>
        /// Consume message event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        /// <returns>True -> Prevent other analyzers from processing the same message. False -> nothing was done</returns>
        Task<bool> ProcessMessage(DiscordClient sender, MessageCreateEventArgs message);
    }
}