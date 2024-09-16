using OracleCMS.CarStocks.Web.Areas.Admin.Queries.Roles;
using OracleCMS.CarStocks.Web.Areas.Admin.Queries.Users;
using MediatR;

namespace OracleCMS.CarStocks.Web.Helper
{
    public class UserHelper(IMediator mediator)
    {
        public async Task<string> GetUserName(string userId)
        {
            string userName = "[User Not Found]";
            _ = (await (mediator.Send(new GetUserByIdQuery(userId)))).Select(l => userName = l.Name!);            
            return userName;
        }
        public async Task<string?> GetUserRole(string? userId)
        {             
            return await mediator.Send(new GetRoleByUserIdQuery(userId));
        }
    }
}
