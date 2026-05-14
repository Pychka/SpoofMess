using DataSaveHelpers.Services;
using SpoofMessageService.Models;

namespace SpoofMessageService.Services.Validators;

public interface IUserValidator : ISoftDeletableValidator<User>
{
}