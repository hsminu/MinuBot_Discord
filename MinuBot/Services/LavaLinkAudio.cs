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
    public sealed class LavaLinkAudio
    {
        private readonly LavaNode _lavaNode;

        public LavaLinkAudio(LavaNode lavaNode)
            => _lavaNode = lavaNode;

        public async Task<string> JoinAsync(IGuild guild, IVoiceState voiceState, ITextChannel textChannel)
        {
            if (_lavaNode.HasPlayer(guild))
            {
                return $"이미 {voiceState.VoiceChannel.Name}채널에 접속중입니다.";
            }

            if (voiceState.VoiceChannel is null)
            {
                return $"{guild.Name}: 당신은 우선 음성 채널에 들어와야 합니다.";
            }

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, textChannel);
                return $"{voiceState.VoiceChannel.Name}채널에 접속합니다.";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        static Victoria.Responses.Rest.SearchResponse search;
        static IUserMessage message;
        public async Task PlayAsync(SocketGuildUser user, IGuild guild, IVoiceState voiceState, ITextChannel textChannel, string query)
        {
            if (user.VoiceChannel == null)
                await textChannel.SendMessageAsync(await JoinAsync(guild, voiceState, textChannel));

            if (!_lavaNode.HasPlayer(guild))
                await textChannel.SendMessageAsync(await JoinAsync(guild, voiceState, textChannel));
            var a = UserList(user);

            try
            {
                var player = _lavaNode.GetPlayer(guild);
                message = await textChannel.SendMessageAsync($"유튜브에서 {query}를 검색하고 있습니다...");
                search = await _lavaNode.SearchYouTubeAsync(query);

                if (search.LoadStatus == LoadStatus.NoMatches)
                {
                    await message.DeleteAsync();
                    await textChannel.SendMessageAsync("검색 결과가 없습니다.");
                    return;
                }

                string[] TrackLength = new string[5];

                for (int i = 0; i < 5; i++)
                {
                    TrackLength[i] = search.Tracks.ElementAt(i).Duration.ToString();
                    if (TrackLength[i][0] == '0' && TrackLength[i][1] == '0')
                    {
                        TrackLength[i] = TrackLength[i].Remove(0, 3);
                    }
                }

                string trackmessage =
                    $"**1:** {search.Tracks.ElementAt(0).Title} ({TrackLength[0]})\n" +
                    $"**2:** {search.Tracks.ElementAt(1).Title} ({TrackLength[1]})\n" +
                    $"**3:** {search.Tracks.ElementAt(2).Title} ({TrackLength[2]})\n" +
                    $"**4:** {search.Tracks.ElementAt(3).Title} ({TrackLength[3]})\n" +
                    $"**5:** {search.Tracks.ElementAt(4).Title} ({TrackLength[4]})";

                await message.ModifyAsync(msg => msg.Content = $"**`!play n`** **명령어를 사용하여 트랙을 선택해 주시길 바랍니다**\n{trackmessage}");
                return;
            }

            catch (Exception ex)
            {
                await textChannel.SendMessageAsync(ex.Message);
                return;
            }

        }
        public struct Trackvalue
        {
            public ulong guildId;
            public string title;
            public string name;
            public string playtime;
            public int second;
        }
        static int guildcount = 0;
        static int[] trackcount = new int[10];
        static int[] NowTrack = new int[10];
        static Trackvalue[,] trackvalue = new Trackvalue[20, 500];
        public async Task SelectAsync(IGuild guild, string username, int select)
        {
            var player = _lavaNode.GetPlayer(guild);
            var track = search.Tracks.ElementAt(select - 1);

            string TrackLength = TrackTime(search.Tracks.ElementAt(select - 1));

            if (CheckhasGuildId(guild.Id) == false)
            {
                guildcount++;
                trackvalue[guildcount, 0].guildId = guild.Id;
            }

            int a = GuildCount(guild.Id);

            Console.WriteLine($"a: {a}");
            trackvalue[a, trackcount[a]].title = track.Title;
            trackvalue[a, trackcount[a]].name = username;
            trackvalue[a, trackcount[a]].playtime = TrackLength;
            trackvalue[a, trackcount[a]].second = (int)track.Duration.TotalSeconds;
            trackcount[a]++;
            Console.WriteLine($"tracktime: {track.Duration.TotalSeconds}");

            if (player.Track != null && player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
            {
                player.Queue.Enqueue(track);
                await message.ModifyAsync(msg => msg.Content = $"곡 #{select}번이 선택되었습니다. {track.Title} ({TrackLength})");
                return;
            }

            await player.PlayAsync(track);
            await message.ModifyAsync(msg => msg.Content = $"곡 **#{select}**번이 선택되었습니다. {track.Title} ({TrackLength})");
            return;
        }

        /*This is ran when a user uses the command Leave.
            Task Returns an Embed which is used in the command call. */
        public async Task<string> LeaveAsync(IGuild guild, IVoiceState voiceState)
        {
            try
            {
                //Get The Player Via GuildID.
                var player = _lavaNode.GetPlayer(guild);

                //if The Player is playing, Stop it.
                if (player.PlayerState is PlayerState.Playing)
                {
                    await player.StopAsync();
                }

                //Leave the voice channel.
                await _lavaNode.LeaveAsync(player.VoiceChannel);

                await player.DisposeAsync();

                return $"{voiceState.VoiceChannel.Name} 채널에서 나갔습니다.";
            }
            //Tell the user about the error so they can report it back to us.
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }

        /*This is ran when a user uses the command List 
            Task Returns an Embed which is used in the command call. */
        public async Task<string> ListAsync(IGuild guild, int page)
        {
            try
            {
                /* Create a string builder we can use to format how we want our list to be displayed. */
                var descriptionBuilder = new StringBuilder();

                /* Get The Player and make sure it isn't null. */
                var player = _lavaNode.GetPlayer(guild);
                if (player == null)
                    return "사용자를 찾을 수 없습니다.";

                if (player.PlayerState is PlayerState.Playing)
                {
                    if (player.Track == null)
                    {
                        return "현재 재생중인 음악이 없습니다.";
                    }
                    else
                    {
                        int TotalSecond = 0;
                        int a = GuildCount(guild.Id);
                        string[] Queue = new string[player.Queue.Count + 1];
                        Queue[0] = $" **{trackvalue[a, NowTrack[a]].title}, {trackvalue[a, NowTrack[a]].name}**에 의해 추가됨. `[{trackvalue[a, NowTrack[a]].playtime}]`";
                        int i = 1;
                        foreach (LavaTrack track in player.Queue.Items)
                        {
                            Queue[i] = $" **{trackvalue[a, NowTrack[a] + i].title}, {trackvalue[a, NowTrack[a] + i].name}**에 의해 추가됨. `[{trackvalue[a, NowTrack[a] + i].playtime}]`";
                            i++;
                        }

                        for (int x = NowTrack[i]; x < trackcount[a]; x++)
                            TotalSecond += trackvalue[a, x].second;
                        int hour = 0, min = 0, sec = 0;
                        sec = TotalSecond % 60;
                        TotalSecond /= 60;
                        min = TotalSecond % 60;
                        TotalSecond /= 60;
                        hour = TotalSecond;
                        string Totaltime = "";
                        Console.WriteLine($"{hour} {min} {sec}");
                        if (hour == 0)
                        {
                            if (min < 10)
                                Totaltime += "0" + min + ":";
                            else
                                Totaltime += min + ":";
                            if (sec < 10)
                                Totaltime += "0" + sec;
                            else
                                Totaltime += sec;
                        }
                        else
                        {
                            if (hour < 10)
                                Totaltime += "0" + hour + ":";
                            else
                                Totaltime += hour + ":";
                            if (min < 10)
                                Totaltime += "0" + min + ":";
                            else
                                Totaltime += min + ":";
                            if (sec < 10)
                                Totaltime += "0" + sec;
                            else
                                Totaltime += sec;
                        }

                        string List = "";
                        string pagecount = "";

                        if (Queue.Length < 10)
                        {
                            pagecount = "페이지 **1**의 **1**\n\n";
                            List += $"`[1]` **▷**";
                            List += Queue[0] + "\n";
                            for (int j = 1; j < i; j++)
                            {
                                List += $"[{j + 1}]";
                                List += Queue[j] + "\n";
                            }
                            List += $"\n총 **{i}**개 트랙, 총 시간 **[{Totaltime}]**의 트랙이 재생 큐에 있습니다.";
                        }
                        else
                        {
                            if (Queue.Length / 10 < page)
                                return "**page**의 수가 올바르지 않습니다.";
                            pagecount = $"페이지 **{Queue.Length / 10}**의 **{page}**\n\n";
                            if (Queue.Length / 10 == page)
                            {
                                for (int j = page * 10; j < i; j++)
                                {
                                    List += $"[{j + 1}]";
                                    List += Queue[j] + "\n";
                                }
                            }
                            else
                            {
                                for (int j = page * 10; j < (page * 10) + 10; j++)
                                {
                                    List += $"[{j + 1}]";
                                    List += Queue[j] + "\n";
                                }
                            }
                            List += $"\n총 **{i}**개 트랙, 총 시간 **[{Totaltime}]**의 트랙이 재생 큐에 있습니다.";
                        }

                        return pagecount + List;
                    }
                }
                else
                {
                    return "현재 재생중인 음악이 없습니다.";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        /*This is ran when a user uses the command Skip 
            Task Returns an Embed which is used in the command call. */
        public async Task<string> SkipTrackAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);
                /* Check if the player exists */
                if (player == null)
                    return "사용자를 찾을 수 없습니다.";
                /* Check The queue, if it is less than one (meaning we only have the current song available to skip) it wont allow the user to skip.
                     User is expected to use the Stop command if they're only wanting to skip the current song. */
                if (player.Queue.Count < 1)
                {
                    return "현재 플레이어가 재생할수 있는 곡이 없습니다.";
                }
                else
                {
                    try
                    {
                        /* Save the current song for use after we skip it. */
                        var currentTrack = player.Track;
                        /* Skip the current song. */
                        await player.SkipAsync();
                        return $"트랙 번호 #1가 스킵되었습니다.\n {currentTrack.Title}";
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }

                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /*This is ran when a user uses the command Stop 
            Task Returns an Embed which is used in the command call. */
        public async Task<Embed> StopAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);

                if (player == null)
                    return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check{GlobalData.Config.DefaultPrefix}Help for info on how to use the bot.");

                /* Check if the player exists, if it does, check if it is playing.
                     If it is playing, we can stop.*/
                if (player.PlayerState is PlayerState.Playing)
                {
                    await player.StopAsync();
                }

                await LoggingService.LogInformationAsync("Music", $"Bot has stopped playback.");
                return await EmbedHandler.CreateBasicEmbed("Music Stop", "I Have stopped playback & the playlist has been cleared.", Color.Blue);
            }
            catch (Exception ex)
            {
                return await EmbedHandler.CreateErrorEmbed("Music, Stop", ex.Message);
            }
        }

        /*This is ran when a user uses the command Volume 
            Task Returns a String which is used in the command call. */
        public async Task<string> SetVolumeAsync(IGuild guild, int volume)
        {
            if (volume > 150 || volume <= 0)
            {
                return $"Volume must be between 1 and 150.";
            }
            try
            {
                var player = _lavaNode.GetPlayer(guild);
                await player.UpdateVolumeAsync((ushort)volume);
                await LoggingService.LogInformationAsync("Music", $"Bot Volume set to: {volume}");
                return $"Volume has been set to {volume}.";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> PauseAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);
                if (!(player.PlayerState is PlayerState.Playing))
                {
                    await player.PauseAsync();
                    return $"There is nothing to pause.";
                }

                await player.PauseAsync();
                return $"**Paused:** {player.Track.Title}, what a bamboozle.";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> ResumeAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);

                if (player.PlayerState is PlayerState.Paused)
                {
                    await player.ResumeAsync();
                }

                return $"**Resumed:** {player.Track.Title}";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
            }
        }

        public async Task TrackEnded(TrackEndedEventArgs args)
        {
            Console.Write("TrackEnded: ");
            Console.WriteLine(args.Player.TextChannel.GuildId);
            NowTrack[GuildCount(args.Player.TextChannel.GuildId)]++;
            Console.WriteLine($"nowTrack: {NowTrack[GuildCount(args.Player.TextChannel.GuildId)]}");
            if (!args.Reason.ShouldPlayNext())
            {
                return;
            }

            if (!args.Player.Queue.TryDequeue(out var queueable))
            {
                return;
            }

            if (!(queueable is LavaTrack track))
            {
                return;
            }
            await args.Player.PlayAsync(track);
        }
        public string TrackTime(LavaTrack track)
        {
            if (track == null)
                return null;

            string TrackLength = track.Duration.ToString();
            if (TrackLength[0] == '0' && TrackLength[1] == '0')
                TrackLength = TrackLength.Remove(0, 3);

            return TrackLength;
        }

        public SocketGuildUser[] UserList(SocketGuildUser user)
        {
            var guild = user.Guild;
            var users = guild.Users;
            SocketGuildUser[] guildUsers = users.ToArray();
            Console.WriteLine(guildUsers[0].Username);

            return guildUsers;
        }

        public int GuildCount(ulong GuildId)
        {
            int i;
            for (i = 0; i < guildcount + 1; i++)
            {
                if (trackvalue[i, 0].guildId == GuildId)
                    break;
            }

            return i;
        }

        public bool CheckhasGuildId(ulong GuildId)
        {
            for (int i = 0; i < guildcount + 1; i++)
            {
                if (trackvalue[i, 0].guildId == GuildId)
                    return true;
            }

            return false;
        }
    }
}
