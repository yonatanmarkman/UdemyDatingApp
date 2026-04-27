using System;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface ILikesRepository
{
    Task<MemberLike?> GetMemberLikeAsync(string sourceMemberId, string targetMemberId);

    Task<PaginatedResult<Member>> GetMembersFromLikesAsync(LikesParams likesParams);

    Task<IReadOnlyList<string>> GetCurrentMemberLikeIdsAsync(string memberId);

    Task<bool> AddLike(MemberLike like);

    Task<bool> DeleteLike(MemberLike like);

    Task<bool> SaveAllChanges();
}
