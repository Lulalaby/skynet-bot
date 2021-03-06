using System.Linq;
using DisCatSharp.SlashCommands;

namespace Skynet.Bot.Extensions
{
    public static class SCommandsExtensions
    {
        /// <summary>
        /// Register ALL classes that implement <see cref="SlashCommandModule"/> from the assembly of
        /// <typeparamref name="TMarker"/>
        /// </summary>
        /// <param name="slash"></param>
        /// <typeparam name="TMarker"></typeparam>
        public static void RegisterCommandsFromAssembly<TMarker>(this SlashCommandsExtension slash)
        {
            var commands = typeof(TMarker).Assembly
                .ExportedTypes
                .Where(x => x.BaseType == typeof(SlashCommandModule))
                .ToList();

            foreach (var command in commands)
            {
                #if DEBUG
                slash.RegisterCommands(command, 885538658533376020);
                #elif RELEASE
                slash.RegisterCommands(command, 862027572455407616);
                #endif
            }
        }
    }
}