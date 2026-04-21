using AdditionalHelpers.Services;
using CommonObjects.DTO;
using CommonObjects.Requests.Messages;
using CommonObjects.Responses;
using CommonObjects.Results;
using SecurityLibrary;
using SecurityLibrary.Tokens;
using SpoofMessageService.Models;
using SpoofMessageService.Models.Enums;
using SpoofMessageService.Services;
using SpoofMessageService.Services.Events;
using SpoofMessageService.Services.Repositories;
using SpoofMessageService.Services.Setters;
using SpoofMessageService.Services.Validators;
using System.Collections.Concurrent;

namespace SpoofMessageService.ServiceRealizations;

public class MessageService(
        ILoggerService loggerService,
        IMessageRepository messageRepository,
        IMessageValidator messageValidator,
        IChatUserService chatUserService,
        IFileTokenService fileTokenService,
        IAttachmentService attachmentService,
        IFileMetadatumService fileMetadatumService,
        IMessageEventService messageEventService
    ) : IMessageService
{
    private readonly IAttachmentService _attachmentService = attachmentService;
    private readonly IFileMetadatumService _fileMetadatumService = fileMetadatumService;
    private readonly ILoggerService _loggerService = loggerService;
    private readonly IMessageEventService _messageEventService = messageEventService;
    private readonly IMessageRepository _messageRepository = messageRepository;
    private readonly IMessageValidator _messageValidator = messageValidator;
    private readonly IChatUserService _chatUserService = chatUserService;
    private readonly IFileTokenService _fileTokenService = fileTokenService;

    public async Task<Result> DeleteMessage(
            Guid messageId, 
            Guid chatId,
            Guid userId)
    {
        try
        {
            Task<Message?> messageTask = Task.Run(() => _messageRepository.GetByIdAsync(messageId));
            Task<Result<ChatUser>> resultTask = Task.Run(() => _chatUserService.GetAndCheckPermission(
                    chatId,
                    userId,
                    Rules.DeleteMessage
                ));
            await Task.WhenAll(messageTask, resultTask);

            if (!resultTask.Result.Success)
                return Result.From(resultTask.Result);

            Result result = _messageValidator.IsAvailableAndOwner(
                    messageTask.Result,
                    chatId
                );
            if (!result.Success)
                return result;

            await _messageRepository.DeleteAsync(messageTask.Result!);
            return Result.OkResult();
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result.ErrorResult("Internal server error");
        }
    }

    public async Task<Result<EditMessageResponse>> EditMessage(
            EditMessageRequest request,
            Guid userId)
    {
        try
        {
            Task<Message?> messageTask = Task.Run(() => _messageRepository.GetByIdAsync(request.Id));
            Task<Result<ChatUser>> resultTask = Task.Run(() => _chatUserService.GetAndCheckPermission(
                    request.ChatId,
                    userId,
                    Rules.EditMessage
                ));
            await Task.WhenAll(messageTask, resultTask);

            if (!resultTask.Result.Success)
                return Result<EditMessageResponse>.From(resultTask.Result);
            Result result = _messageValidator.IsAvailableAndOwner(
                    messageTask.Result,
                    userId
                );
            if (!result.Success)
                return Result<EditMessageResponse>.From(result);

            messageTask.Result!.Set(
                    request
                );
            CancellationTokenSource tokenSource = new();

            ConcurrentBag<Attachment> attachments = [];
            ConcurrentBag<Attachment> attachments2 = [];
            await Parallel.ForEachAsync(request.Attachments, async (attachmentDTO, cancellationToken) =>
            {
                if (!_fileTokenService.IsValid(attachmentDTO.Token, userId, out Guid fileId))
                {
                    tokenSource.Cancel();
                    return;
                }
                if (attachmentDTO.IsAdded)
                {
                    Result<FileMetadatum> result = await _fileMetadatumService.Get(fileId);
                    if (!result.Success)
                    {
                        tokenSource.Cancel();
                        return;
                    }
                    attachments.Add(attachmentDTO.Set(fileId));
                    attachments2.Add(attachmentDTO.Set(fileId, result.Body!));
                }
                else
                {
                    await _attachmentService.RemoveAttachment(fileId);
                }
            });

            foreach (var attachment in attachments)
                messageTask.Result!.Attachments.Add(attachment);
            messageTask.Result.LastModified = DateTime.UtcNow;
            await _messageRepository.UpdateAsync(messageTask.Result!);
            messageTask.Result!.Attachments.Clear();
            foreach (var attachment in attachments2)
                messageTask.Result!.Attachments.Add(attachment);
            EditMessageResponse response = new(
               messageTask.Result!.Id,
               messageTask.Result.ChatId,
               resultTask.Result.Body!.User.Login,
               resultTask.Result.Body.User.Name,
               null,
               resultTask.Result.Body.User.AvatarId?.ToByteArray(),
               resultTask.Result.Body.User.OriginalFileName!,
               string.IsNullOrEmpty(request.Text) ? null : messageTask.Result.Text,
               messageTask.Result.LastModified,
               []);
            Attachment attachment1;
            List<Attachment> attachments42 = [.. attachments2];
            for(int i = 0; i < attachments42.Count; i++)
            {
                attachment1 = attachments42[i];
                response.Attachments.Add(new(
                       request.Attachments[i].IsAdded,
                       attachment1.Id.ToByteArray(),
                       [],
                       attachment1.OriginalFileName,
                       attachment1.Category,
                       attachment1.AdditionalMetadata,
                       attachment1.Size
                    ));
            }
            return Result<EditMessageResponse>.OkResult(response);
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result<EditMessageResponse>.ErrorResult("Internal server error");
        }
    }

    public async Task<Result<MessageDTO>> SendMessage(
            CreateMessageRequest request,
            Guid userId)
    {
        if(string.IsNullOrWhiteSpace(request.Text) && (request.Attachments is null || request.Attachments.Count == 0))
            return Result<MessageDTO>.BadRequest("At least one field is required: Text or Attachments");

        try
        {
            Result<ChatUser> chatUserResult = await _chatUserService.GetAndCheckPermission(
                    request.ChatId,
                    userId,
                    Rules.SendTexts
                );
            if (!chatUserResult.Success)
                return Result<MessageDTO>.From(chatUserResult);

            Message message = request.Set(
                    userId
                );
            CancellationTokenSource tokenSource = new();
            ConcurrentBag<Attachment> attachments = [];
            ConcurrentBag<Attachment> attachments2 = [];
            await Parallel.ForEachAsync(request.Attachments, async (attachmentDTO, cancellationToken) =>
            {
                if (!_fileTokenService.IsValid(attachmentDTO.Token, userId, out Guid fileId))
                {
                    tokenSource.Cancel();
                    return;
                }
                Result<FileMetadatum> result = await _fileMetadatumService.Get(fileId);
                if (!result.Success)
                {
                    tokenSource.Cancel();
                    return;
                }
                attachments.Add(attachmentDTO.Set(fileId));
                attachments2.Add(attachmentDTO.Set(fileId, result.Body!));
            });
            foreach (var attachment in attachments)
                message.Attachments.Add(attachment);
            await _messageRepository.AddAsync(message);
            message.Attachments.Clear();
            foreach (var attachment in attachments2)
                message.Attachments.Add(attachment);
            
            message.User = chatUserResult.Body!.User;

            MessageDTO messageDTO = new(
                message!.Id,
                message.ChatId,
                chatUserResult.Body.User.Login,
                chatUserResult.Body.User.Name,
                null,
                chatUserResult.Body.User.AvatarId is null
                            ? null
                            : Hasher.GetKey(chatUserResult.Body.User.AvatarId.Value.ToByteArray()),
                chatUserResult.Body.User.OriginalFileName,
                message.Text,
                message.SentAt,
                message.Attachments.Count == 0
                ? null : [.. message.Attachments.Select(x =>
                                    new CommonObjects.Requests.Attachments.Attachment(
                                        Hasher.GetKey(x.Id.ToByteArray()),
                                        [],
                                        x.OriginalFileName,
                                        x.Category,
                                        x.AdditionalMetadata,
                                        x.Size))]);
            _messageEventService.ReciveMessage(messageDTO, chatUserResult.Body.Chat);
            return Result<MessageDTO>.OkResult(messageDTO);
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result<MessageDTO>.ErrorResult("Internal server error");
        }
    }

    public async Task<Result<List<MessageDTO>>> GetMessagesAfterDate(
            Guid chatId,
            Guid userId,
            DateTime date,
            int take = 50
        )
    {
        try
        {
            Result<ChatUser> resultChatUser = await _chatUserService.GetAndCheckPermission(
                    chatId,
                    userId,
                    Rules.DeleteMessage
                );
            if (!resultChatUser.Success)
                return Result<List<MessageDTO>>.From(resultChatUser);

            return Result<List<MessageDTO>>.OkResult(
                [.. (await _messageRepository.GetMessagesAfterDate(
                    chatId,
                    date,
                    take
                    )).Select(x => x.Set(
                        [..
                            x.Attachments.Select(x => new CommonObjects.Requests.Attachments.Attachment(
                            Hasher.GetKey(x.FileMetadataId.ToByteArray()),
                            _fileTokenService.CreateToken(
                                userId,
                                x.FileMetadata.Id),
                            x.OriginalFileName,
                            x.FileMetadata.Category,
                            x.FileMetadata.Metadata,
                            x.FileMetadata.Size))
                        ],
                        x.User.AvatarId is null
                            ? null
                            : _fileTokenService.CreateToken(userId, x.User.AvatarId.Value),
                        x.User.AvatarId is null
                            ? null
                            : Hasher.GetKey(x.User.AvatarId.Value.ToByteArray())))
                    ]
                );
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result<List<MessageDTO>>.ErrorResult("Internal server error");
        }
    }

    public async Task<Result<List<MessageDTO>>> GetMessagesBeforeDate(
            Guid chatId,
            Guid userId,
            DateTime date,
            int take = 50
        )
    {
        try
        {
            Result<ChatUser> resultCHatUser = await _chatUserService.GetAndCheckPermission(
                    chatId,
                    userId,
                    Rules.DeleteMessage
                );
            if (!resultCHatUser.Success)
                return Result<List<MessageDTO>>.From(resultCHatUser);

            return Result<List<MessageDTO>>.OkResult(
                [.. (await _messageRepository.GetMessagesBeforeDate(
                    chatId,
                    date,
                    take
                    )).Select(x => x.Set(
                        [..
                            x.Attachments.Select(x => new CommonObjects.Requests.Attachments.Attachment(
                            Hasher.GetKey(x.FileMetadataId.ToByteArray()),
                            _fileTokenService.CreateToken(
                                userId,
                                x.FileMetadata.Id),
                            x.OriginalFileName,
                            x.FileMetadata.Category,
                            x.FileMetadata.Metadata,
                            x.FileMetadata.Size))
                        ],
                        x.User.AvatarId is null
                            ? null
                            : _fileTokenService.CreateToken(userId, x.User.AvatarId.Value),
                        x.User.AvatarId is null
                            ? null
                            : Hasher.GetKey(x.User.AvatarId.Value.ToByteArray())))
                    ]
                );
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result<List<MessageDTO>>.ErrorResult("Internal server error");
        }
    }

    public async Task<Result<List<MessageDTO>>> GetSkippedMessages(
            Guid userId,
            DateTime after,
            int take = 50
        )
    {
        try
        {
            List<Message> messages = await _messageRepository.GetMessageSinceDate(userId, after, take);
            Result result = _messageValidator.IsAvailableCollection(messages);
            if (!result.Success)
                return Result<List<MessageDTO>>.From(result);

            return Result<List<MessageDTO>>.OkResult(
                [.. messages.Select(x => x.Set(
                    [..
                        x.Attachments.Select(x => new CommonObjects.Requests.Attachments.Attachment(
                        Hasher.GetKey(x.FileMetadataId.ToByteArray()),
                        _fileTokenService.CreateToken(
                            userId,
                            x.FileMetadata.Id),
                        x.OriginalFileName,
                        x.FileMetadata.Category,
                        x.FileMetadata.Metadata,
                        x.FileMetadata.Size))
                    ],
                    x.User.AvatarId is null
                        ? null
                        : _fileTokenService.CreateToken(userId, x.User.AvatarId.Value),
                    x.User.AvatarId is null
                        ? null
                        : Hasher.GetKey(x.User.AvatarId.Value.ToByteArray())))
                    ]
                );
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result<List<MessageDTO>>.ErrorResult("Internal server error");
        }
    }
}
