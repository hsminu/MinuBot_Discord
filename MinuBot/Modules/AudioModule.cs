using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MinuBot.Services;
using System.Threading.Tasks;

namespace MinuBot.Modules
{
    public class AudioModule : ModuleBase<SocketCommandContext>
    {
        /* Get our AudioService from DI */
        public LavaLinkAudio AudioService { get; set; }
        static bool playvalue = false;

        /* All the below commands are ran via Lambda Expressions to keep this file as neat and closed off as possible. 
              We pass the AudioService Task into the section that would normally require an Embed as that's what all the
              AudioService Tasks are returning. */

        [Command("Join")]
        public async Task JoinAndPlay()
            => await ReplyAsync(await AudioService.JoinAsync(Context.Guild, Context.User as IVoiceState, Context.Channel as ITextChannel));

        [Command("Leave")]
        public async Task Leave()
            => await ReplyAsync(await AudioService.LeaveAsync(Context.Guild, Context.User as IVoiceState));

        [Command("Play")]
        public async Task Play([Remainder]string query)
        {
            int select = int.MinValue;
            if (int.TryParse(query, out select) == false)
            {
                await AudioService.PlayAsync(Context.User as SocketGuildUser, Context.Guild, Context.User as IVoiceState, Context.Channel as ITextChannel, query);
                playvalue = true;
            }
            else if (select != int.MinValue)
            {
                await AudioService.SelectAsync(Context.Guild, Context.User.Username, select);
                playvalue = false;
            }

        }

        [Command("Stop")]
        public async Task Stop()
            => await ReplyAsync(embed: await AudioService.StopAsync(Context.Guild));

        [Command("List")]
        public async Task List()
            => await ReplyAsync(await AudioService.ListAsync(Context.Guild, int.MinValue));

        [Command("List")]
        public async Task List([Remainder]string query)
        {
            int select = int.MinValue;
            if (int.TryParse(query, out select) == false)
                await ReplyAsync("명령 구문이 올바르지 않습니다. **;list (page)**");
            else if (select < 1)
                await ReplyAsync("0이하의 숫자는 입력할 수 없습니다.");
            else
                await ReplyAsync(await AudioService.ListAsync(Context.Guild, select));
        }

        [Command("Skip")]
        public async Task Skip()
            => await ReplyAsync(await AudioService.SkipTrackAsync(Context.Guild));

        [Command("Volume")]
        public async Task Volume(int volume)
            => await ReplyAsync(await AudioService.SetVolumeAsync(Context.Guild, volume));

        [Command("Pause")]
        public async Task Pause()
            => await ReplyAsync(await AudioService.PauseAsync(Context.Guild));

        [Command("Resume")]
        public async Task Resume()
            => await ReplyAsync(await AudioService.ResumeAsync(Context.Guild));

        [Command("P")]
        public async Task P([Remainder]string query)
        {
            int select = int.MinValue;
            if (int.TryParse(query, out select) == false)
            {
                await AudioService.PlayAsync(Context.User as SocketGuildUser, Context.Guild, Context.User as IVoiceState, Context.Channel as ITextChannel, query);
                playvalue = true;
            }
            else if (select != int.MinValue)
            {
                await AudioService.SelectAsync(Context.Guild, Context.User.Username, select);
                playvalue = false;
            }
        }

        [Command("1")]
        public async Task Play1track()
        {
            if (playvalue == true)
            {
                await AudioService.SelectAsync(Context.Guild, Context.User.Username, 1);
                playvalue = false;
            }
        }

        [Command("2")]
        public async Task Play2track()
        {
            if (playvalue == true)
            {
                await AudioService.SelectAsync(Context.Guild, Context.User.Username, 2);
                playvalue = false;
            }
        }

        [Command("3")]
        public async Task Play3track()
        {
            if (playvalue == true)
            {
                await AudioService.SelectAsync(Context.Guild, Context.User.Username, 3);
                playvalue = false;
            }
        }

        [Command("4")]
        public async Task Play4track()
        {
            if (playvalue == true)
            {
                await AudioService.SelectAsync(Context.Guild, Context.User.Username, 4);
                playvalue = false;
            }
        }

        [Command("5")]
        public async Task Play5track()
        {
            if (playvalue == true)
            {
                await AudioService.SelectAsync(Context.Guild, Context.User.Username, 5);
                playvalue = false;
            }
        }
    }
}
