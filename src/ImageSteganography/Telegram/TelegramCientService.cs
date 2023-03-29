using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using ImageSteganography.Services;
using ImageSteganography.Models;
using System.Threading;

namespace ImageSteganography.Telegram;

public class TelegramCientService
{
    private readonly List<EncodeImageRequest> requests = new List<EncodeImageRequest>();
    private readonly ImageSteganographyService _imageSteganographyService;
    private readonly string _token;
    public TelegramCientService(string token)
    {
        _imageSteganographyService = new ImageSteganographyService();
        _token = token;
    }

    public async Task ConfigureTelegramCient()
    {
        var botClient = new TelegramBotClient(_token);

        using CancellationTokenSource cts = new();

        // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };

        botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        var me = await botClient.GetMeAsync();

        Console.WriteLine($"Start listening for @{me.Username}");
        Console.ReadLine();

        // Send cancellation request to stop bot
        cts.Cancel();
    }

    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Only process Message updates: https://core.telegram.org/bots/api#message
        if (update.Message is not { } message)
            return;
        var chatId = message.Chat.Id;

        //if (message.Text is not { } messageText)
        //    return;
        //Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

        if(message.Type == MessageType.Photo) 
        {
        
        }

        if (message.Text is not { } messageText)
            return;
        Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

        switch (messageText.ToLower())
        {
            case "/start":
                await HandleStartCommand(botClient, update, cancellationToken);
                break;
            case "/help":
                await HandleHelpCommand(botClient, update, cancellationToken);
                break;
            case "/encode":
                await HandleEncodeCommand(botClient, update, cancellationToken);
                break;

            default:
                await botClient.SendTextMessageAsync(
                chatId: update.Message!.Chat.Id,
                text: "Invalid command! try /help",
                cancellationToken: cancellationToken);
                break;
        }


    }

    async Task HandleEncodeCommand(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) 
    {
       if(requests.)
    }

    async Task HandleStartCommand(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        string content = "Welcome to ImageSteganography bot, this bot hide your text in image using The Least Significant Bit Technique,\nvisit https://zbigatron.com/image-steganography-simple-examples/ for more info.\n\nThe application was created for a university project lesson,\nInstructor: Dr.Nona Helmi , Student: Shervin Ivari\n\n Type /help to see commands";
        Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: update.Message!.Chat.Id,
            text: content,
            cancellationToken: cancellationToken);
    }

    async Task HandleHelpCommand(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        Dictionary<string, string> commands = new Dictionary<string, string>()
        {
            {"/start","Displays bot information" },
            {"/help","Displays bot commands" },
            {"/encode","Hides content in the given photo." },
            {"/decode","extract content in the given photo." },
        };
        string content = $"Commands:\n{string.Join("\n", commands.Select(kv => $"{kv.Key} : {kv.Value}"))}";
        Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: update.Message!.Chat.Id,
            text: content,
            cancellationToken: cancellationToken);
    }

    Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }

}