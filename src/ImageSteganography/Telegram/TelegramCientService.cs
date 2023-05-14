using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using ImageSteganography.Services;
using ImageSteganography.Models;
using Telegram.Bot.Types.InputFiles;

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
        if (update.Message!.Type == MessageType.Document)
        {
            await HandleDecodeCommand(botClient, update, cancellationToken);
            return;
        }

        if (update.Message!.Type == MessageType.Photo)
        {
            await HandleEncodeCommand(botClient, update, cancellationToken);
            return;
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
            case "/cancel":
            case "/done":
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
        var activeRequest = requests.FirstOrDefault(c => c.ChatId == update.Message!.Chat.Id);
        string content = string.Empty;

        if (activeRequest == null && update.Message?.Text?.ToLower() != "/encode")
        {
            content = "Enter /encode to Start.";
            await botClient.SendTextMessageAsync(
            chatId: update.Message!.Chat.Id,
            text: content,
            cancellationToken: cancellationToken);
            return;
        }

        if (update.Message?.Text?.ToLower() == "/encode")
        {
            requests.Add(new EncodeImageRequest()
            {
                ChatId = update.Message!.Chat.Id,
            });
            content = "Encoding started, Please upload your image with caption.\nEnter /cancel for canceling the operation.";
            await botClient.SendTextMessageAsync(
                  chatId: update.Message!.Chat.Id,
                  text: content,
                  cancellationToken: cancellationToken);
            return;
        }

        if (update.Message?.Text?.ToLower() == "/cancel")
        {
            requests.Remove(activeRequest!);
            content = "Canceled.";
            await botClient.SendTextMessageAsync(
                  chatId: update.Message!.Chat.Id,
                  text: content,
                  cancellationToken: cancellationToken);
            return;
        }

        if (update.Message!.Type == MessageType.Photo)
        {
            if (string.IsNullOrEmpty(update.Message.Caption))
            {
                content = "Please upload your image with caption.";
                Message sentMessage = await botClient.SendTextMessageAsync(
                                            chatId: update.Message!.Chat.Id,
                                            text: content,
                                            cancellationToken: cancellationToken);
                return;
            }
            else
            {
                activeRequest.Image = new System.IO.MemoryStream();
                var photo = update!.Message!.Photo[^1]!;
                var file = await botClient.GetFileAsync(photo.FileId);
                await botClient.DownloadFileAsync(file!.FilePath!, activeRequest!.Image, cancellationToken);
                activeRequest!.Content = update.Message.Caption;
                activeRequest!.Unicode = true;
                _ = botClient.SendTextMessageAsync(
                                            chatId: update.Message!.Chat.Id,
                                            text: "Start encoding ...",
                                            cancellationToken: cancellationToken);
                var result = await _imageSteganographyService.EncodeImageAsync(activeRequest);
                InputOnlineFile encodedPicutre = new(result.EncodedImage, DateTime.Now.ToString() + ".png");
                Message sentMessage = await botClient.SendDocumentAsync(
                                            chatId: update.Message!.Chat.Id,
                                            encodedPicutre,
                                            cancellationToken: cancellationToken);
                requests.Remove(activeRequest!);
                return;
            }
        }
    }

    async Task HandleDecodeCommand(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        DecodeImageRequest request = new() { Unicode = true };
        request.Image = new System.IO.MemoryStream();
        var photo = update!.Message!.Document!;
        var file = await botClient.GetFileAsync(photo.FileId);
        _ = botClient.SendTextMessageAsync(
                     chatId: update.Message!.Chat.Id,
                     text: "Start decoding ...",
                     cancellationToken: cancellationToken);
        await botClient.DownloadFileAsync(file!.FilePath!, request!.Image, cancellationToken);
        var result = await _imageSteganographyService.DecodeImage(request);
        Message sentMessage = await botClient.SendTextMessageAsync(
        chatId: update.Message!.Chat.Id,
        text: result,
        cancellationToken: cancellationToken);
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
            {"Upload document","extract content in the given photo." },
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