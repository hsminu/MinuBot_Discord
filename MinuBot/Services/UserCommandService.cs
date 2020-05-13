using Discord;
using Discord.WebSocket;
using MinuBot.Handlers;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;
using Victoria.Enums;

namespace MinuBot.Services
{
    public sealed class UserCommandService
    {
        public string UserList(SocketGuildUser user)
        {
            var guild = user.Guild;
            var users = guild.Users;
            string[] Username = users.Select(x => x.Username).ToArray();

            string usernamelist = "";
            for (int i = 0; i < Username.Length; i++)
                usernamelist += $"{i + 1}. {Username[i]}\n";
            usernamelist += $"\n총 {Username.Length}명";

            return usernamelist;
        }
    }
}
