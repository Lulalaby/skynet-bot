using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using Skynet.Wiki.API;
using Skynet.Wiki.API.Models;

namespace Skynet.Bot.Analyzers
{
    public class TagAnalyzer : IMessageAnalyzer
    {
        private readonly WikiApi _api;
        private Tag[] _tags;
        private Regex _tagRegex;
        
        public TagAnalyzer(WikiApi api)
        {
            _api = api;
            
            InitializeTags();
        }

        /// <summary>
        /// Initialize the tag regex pattern
        /// </summary>
        void InitializeTags()
        {
            // Retrieve the tags from Engineer's Notebook
            _tags = _api.GetTagsAsync().Result;
            
            // We utilize the PIPE to indicate various alternative words that can be selected
            string matchingClause = string.Join("|", _tags.Select(x => $"{x.Name}"));
            string pattern = $@"({matchingClause})";
            _tagRegex = new Regex(pattern, RegexOptions.IgnoreCase);
        }
        
        public async Task<bool> ProcessMessage(DiscordClient sender, MessageCreateEventArgs message)
        {
            // Get all the matches in the message
            var match = _tagRegex.Matches(message.Message.Content);

            if (match.Count <= 0)
                return false;
            
            // Grab all the tags that were discovered in the group
            var temp = match.Select(x => x.Value).ToArray();
            var response = await _api.FindDocs(temp);

            /*
                If nothing was found --> should return null
                That means we shall return null since we didn't "process" it. Nor will
                we output anything
            */
            
            if (response.bytes == null)
                return false;
            
            using MemoryStream stream = new MemoryStream(response.bytes);

            // Attach the file result 
            DiscordMessageBuilder builder = new DiscordMessageBuilder()
                .WithFile(response.filename, stream);
            
            await message.Message.RespondAsync(builder);

            message.Handled = true;
            return true;
        }
    }
}