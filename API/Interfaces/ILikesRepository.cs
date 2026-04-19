using System;
using API.Entities;

namespace API.Interfaces;

public interface ILikesRepository
{
    Task<MemberLike?> GetMemberLikeAsync(string sourceMemberId, string targetMemberId);

    Task<IReadOnlyList<Member>> GetMembersFromLikesAsync(string predicate, string memberId);

    Task<IReadOnlyList<string>> GetCurrentMemberLikeIdsAsync(string memberId);

    Task<bool> AddLike(MemberLike like);

    Task<bool> DeleteLike(MemberLike like);

    Task<bool> SaveAllChanges();
}
