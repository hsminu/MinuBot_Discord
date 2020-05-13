using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MinuBot.Handlers;
using System;
using System.Threading.Tasks;
using Victoria;

namespace MinuBot.Services
{
    public class DiscordService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandHandler _commandHandler;
        private readonly ServiceProvider _services;
        private readonly LavaNode _lavaNode;
        private readonly LavaLinkAudio _audioService;
        private readonly GlobalData _globalData;



        public DiscordService()
        {
            _services = ConfigureServices();
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _commandHandler = _services.GetRequiredService<CommandHandler>();
            _lavaNode = _services.GetRequiredService<LavaNode>();
            _globalData = _services.GetRequiredService<GlobalData>();
            _audioService = _services.GetRequiredService<LavaLinkAudio>();

            SubscribeLavaLinkEvents();
            SubscribeDiscordEvents();
        }

        /* Initialize the Discord Client. */
        public async Task InitializeAsync()
        {
            await InitializeGlobalDataAsync();

            await _client.LoginAsync(TokenType.Bot, GlobalData.Config.DiscordToken);
            await _client.StartAsync();

            await _commandHandler.InitializeAsync();

            await Task.Delay(-1);
        }

        /* Hook Any Client Events Up Here. */
        private void SubscribeLavaLinkEvents()
        {
            _lavaNode.OnLog += LogAsync;
            _lavaNode.OnTrackEnded += _audioService.TrackEnded;
        }

        private void SubscribeDiscordEvents()
        {
            _client.Ready += ReadyAsync;
            _client.Log += LogAsync;
            _client.MessageReceived += MessageReceived;
            _client.UserJoined += UserJoined;
        }

        private async Task InitializeGlobalDataAsync()
        {
            await _globalData.InitializeAsync();
        }

        /* Used when the Client Fires the ReadyEvent. */
        private async Task ReadyAsync()
        {
            try
            {
                await _lavaNode.ConnectAsync();
                await _client.SetGameAsync(GlobalData.Config.GameStatus);

            }
            catch (Exception ex)
            {
                await LoggingService.LogInformationAsync(ex.Source, ex.Message);
            }

        }

        private async Task MessageReceived(SocketMessage message)
        {
            if (message.Author.Id == _client.CurrentUser.Id)
                return;

            return;
        }

        private async Task UserIsTyping(SocketUser user, ISocketMessageChannel socketMessage)
        {
            var message = await socketMessage.SendMessageAsync($"{user.Username}님이 입력하고 있습니다..", true);
        }

        public async Task UserJoined(SocketGuildUser user)
        {
            await user.SendMessageAsync($"안녕하세요 **{user.Guild.Name}**서버에 오신것을 환영하지않습니다.");
        }

        /*Used whenever we want to log something to the Console. 
            Todo: Hook in a Custom LoggingService. */
        private async Task LogAsync(LogMessage logMessage)
        {
            await LoggingService.LogAsync(logMessage.Source, logMessage.Severity, logMessage.Message);
        }

        /* Configure our Services for Dependency Injection. */
        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<LavaNode>()
                .AddSingleton(new LavaConfig())
                .AddSingleton<LavaLinkAudio>()
                .AddSingleton<UserCommandService>()
                .AddSingleton<BotService>()
                .AddSingleton<GlobalData>()
                .BuildServiceProvider();
        }
    }
}
