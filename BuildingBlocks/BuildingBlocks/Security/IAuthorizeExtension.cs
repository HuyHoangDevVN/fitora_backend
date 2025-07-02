using BuildingBlocks.Responses;

namespace BuildingBlocks.Security;

public interface IAuthorizeExtension
{
    UserLoginResponseBase GetUserFromClaimToken();
    bool ValidateToken();
    UserLoginResponseBase DecodeToken();
    UserLoginResponseBase DecodeExpiredToken();
      
    string GetToken();
}