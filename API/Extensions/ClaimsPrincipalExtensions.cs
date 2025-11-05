using System;
using System.Security.Claims;

namespace API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetMemberId(this ClaimsPrincipal user)
    {
        // This exception cannot be technically reached in the current flow,
        // because the user cannot access the MembersController without a token,
        // and we don't issue tokens that do not contain an Id with the NameIdentifier.
        // So this 'if' statement is solely meant for our compiler.
        return user.FindFirstValue(ClaimTypes.NameIdentifier) 
            ?? throw new Exception("Cannot get memberId from token. ");
    }
}
