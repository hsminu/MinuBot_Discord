using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MinuBot.Services;
using System.Threading.Tasks;
using System;

namespace MinuBot.Modules
{
    public class CommandModule : ModuleBase<SocketCommandContext>
    {
        public LavaLinkAudio AudioService { get; set; }
        public UserCommandService UserServices { get; set; }

        public static bool Shutup = false;
        public static string Name = "";
        public static string Dis = "";
        [Command("test")]
        public async Task test(SocketGuildUser user)
        {
            var message = await Context.Channel.GetMessageAsync(552567071490310165) as IUserMessage;
            var reactions = message.Reactions;
            foreach (var r in reactions)
            {
                await message.RemoveReactionAsync(r.Key, user as IUser);
            }
        }
        [Command("폭탄")]
        public async Task Bomb()
        {
            var message = await Context.Channel.SendMessageAsync("이 메세지는 5초뒤에 삭제됩니다.");
            var message2 = await Context.Channel.SendMessageAsync("5");
            await Task.Delay(1000);
            await message2.ModifyAsync(msg => msg.Content = "4");
            await Task.Delay(1000);
            await message2.ModifyAsync(msg => msg.Content = "3");
            await Task.Delay(1000);
            await message2.ModifyAsync(msg => msg.Content = "2");
            await Task.Delay(1000);
            await message2.ModifyAsync(msg => msg.Content = "1");
            await Task.Delay(1000);
            await message2.ModifyAsync(msg => msg.Content = "Bomb");
            await message.DeleteAsync();
        }

        [Command("닥쳐")]
        public async Task ShutUp([Remainder]string query)
        {
            Console.WriteLine("닥쳐");
            string[] split = query.Split('#');
            if (split.Length != 2)
            {
                await ReplyAsync("명령이 잘못되었습니다.");
                return;
            }
            Shutup = true;
            Name = split[0];
            Dis = split[1];
            return;
        }

        [Command("도움말")]
        public async Task Help()
        {
            await ReplyAsync("도움말입니다");
        }

        [Command("user")]
        public async Task userList()
        {
            await ReplyAsync(UserServices.UserList(Context.User as SocketGuildUser));
        }
    }
}
