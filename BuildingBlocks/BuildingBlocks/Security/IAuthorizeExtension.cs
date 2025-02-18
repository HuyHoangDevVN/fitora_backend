using BuildingBlocks.Responses;

namespace BuildingBlocks.Security;

public interface IAuthorizeExtension
{
    UserLoginResponseBase GetUserFromClaimToken();
    bool ValidateToken();
    UserLoginResponseBase DecodeToken();
      
    string GetToken();
}