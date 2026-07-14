using MyPanelExpress.Models;

namespace MyPanelExpress.Services.Interfaces;

public interface IContactService
{
    Task<Response<object>> SendMessageAsync(CreateContactMessage dto);
}
