using AdditionalHelpers.Services;
using CommonObjects.DTO;
using CommonObjects.Requests.Messages;
using CommonObjects.Responses;
using CommonObjects.Results;
using RuleRoleHelper;
using RuleRoleHelper.ServiceRealizations;
using RuleRoleHelper.Services;
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
        IAttachmentAccessTokenService attachmentTokenService,
        IAttachmentService attachmentService,
        IFileMetadatumService fileMetadatumService,
        IMessageEventService messageEventService,
        IRuleService ruleService
    ) : IMessageService
{
    private readonly IAttachmentService _attachmentService = attachmentService;
    private readonly IFileMetadatumService _fileMetadatumService = fileMetadatumService;
    private readonly ILoggerService _loggerService = loggerService;
    private readonly IMessageEventService _messageEventService = messageEventService;
    private readonly IRuleService _ruleService = ruleService;
    private readonly IMessageRepository _messageRepository = messageRepository;
    private readonly IMessageValidator _messageValidator = messageValidator;
    private readonly IChatUserService _chatUserService = chatUserService;
    private readonly IFileTokenService _fileTokenService = fileTokenService;
    private readonly IAttachmentAccessTokenService _attachmentTokenService = attachmentTokenService;

    public async Task<Result<int>> Stat()
    {
        try
        {
            int count = await _messageRepository.GetCount();
            return Result<int>.OkResult(count);
        }
        catch (Exception ex)
        {
            _loggerService.Error("DataBase error", ex);
            return Result<int>.ErrorResult("Internal server error");
        }
    }


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
                    userId
                );
            if (!result.Success)
                return result;

            await _messageRepository.DeleteAsync(messageTask.Result!);
            _messageEventService.NotifyDeleteMessage(messageId, resultTask.Result.Body!.Chat);
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
            if (!string.IsNullOrEmpty(request.Text))
            {
                HasPermission hasPermission = _ruleService.HasPermission(resultTask.Result.Body!.Rules, (long)Rules.SendTexts);
                if (hasPermission != HasPermission.Allow)
                    return Result<EditMessageResponse>.Forbidden(hasPermission is HasPermission.NotSet ? "Not permission setted" : "Is dennied");
            }
            messageTask.Result!.Set(
                    request
                );
            CancellationTokenSource tokenSource = new();

            ConcurrentBag<Attachment> attachmentsToSend = [];
            ConcurrentBag<EditAttachment> attachments2 = [];
            if(request.Attachments?.Count > 0)
            {
                HasPermission hasPermission = _ruleService.HasPermission(resultTask.Result.Body!.Rules, (long)Rules.SendFiles);
                if (hasPermission != HasPermission.Allow)
                    return Result<EditMessageResponse>.Forbidden(hasPermission is HasPermission.NotSet ? "Not permission setted" : "Is dennied");
            }
            await Parallel.ForEachAsync(request.Attachments ?? [], async (attachmentDTO, cancellationToken) =>
            {
                if (attachmentDTO.IsAdded)
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
                        _loggerService.Warning("Can't handle new attachment");
                        return;
                    }
                    attachmentsToSend.Add(attachmentDTO.Set(fileId, result.Body!));
                }
                else
                {
                    if (!_attachmentTokenService.IsValid(
                        attachmentDTO.Token,
                        out Guid attachmentId))
                    {
                        tokenSource.Cancel();
                        _loggerService.Warning("Can't handle deleted attachment");
                        return;
                    }
                    await _attachmentService.RemoveAttachment(attachmentId);
                    attachments2.Add(attachmentDTO);
                }
            });

            foreach (var attachment in attachmentsToSend)
                messageTask.Result!.Attachments.Add(attachment);

            messageTask.Result!.LastModified = DateTime.UtcNow;
            await _messageRepository.Update(messageTask.Result!);

            EditMessageResponse message = new(
                messageTask.Result!.Id,
                messageTask.Result.ChatId,
                resultTask.Result.Body!.User.Login,
                resultTask.Result.Body.User.Name,
                string.IsNullOrEmpty(request.Text) ? null : messageTask.Result.Text,
                messageTask.Result.LastModified,
                [.. attachmentsToSend.Select(x =>
                    new EditAttachment(
                        true,
                        Hasher.GetKey(x.Id.ToByteArray()),
                        _attachmentTokenService.CreateToken(x.Id),
                        x.OriginalFileName,
                        x.Category,
                        x.AdditionalMetadata,
                        x.Size)).Concat(attachments2)]
            );

            _messageEventService.NotifyEditMessage(message, resultTask.Result.Body.Chat);

            return Result<EditMessageResponse>.OkResult(message);
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
                attachments.Add(attachmentDTO.Set(fileId, result.Body!));
            });
            foreach (var attachment in attachments)
                message.Attachments.Add(attachment);
            await _messageRepository.Save(message);
            
            message.User = chatUserResult.Body!.User;

            MessageDTO messageDTO = new(
                message!.Id,
                message.ChatId,
                chatUserResult.Body.User.Login,
                chatUserResult.Body.User.Name,
                message.Text,
                message.SentAt,
                message.Attachments.Count == 0
                ? null : [.. message.Attachments.Select(x =>
                                    new CommonObjects.Requests.Attachments.Attachment(
                                        Hasher.GetKey(x.Id.ToByteArray()),
                                        _attachmentTokenService.CreateToken(x.Id),
                                        x.OriginalFileName,
                                        x.Category,
                                        x.AdditionalMetadata,
                                        x.Size))]);
            _messageEventService.NotifyReciveMessage(messageDTO, chatUserResult.Body.Chat);
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
                        ]))
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
                        ]))
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
                        Hasher.GetKey(x.Id.ToByteArray()),
                        _attachmentTokenService.CreateToken(x.Id),
                        x.OriginalFileName,
                        x.FileMetadata.Category,
                        x.FileMetadata.Metadata,
                        x.FileMetadata.Size))
                    ]))
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
