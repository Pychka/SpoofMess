using DataSaveHelpers.ServiceRealizations;
using SpoofMessageService.Models;
using SpoofMessageService.Services.Validators;

namespace SpoofMessageService.ServiceRealizations.Validators;

public class ChatValidator : SoftDeletableValidator<Chat>, IChatValidator
{

}