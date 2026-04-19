using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace API.Data;

public class LikesRepository(AppDbContext context) : ILikesRepository
{
    public async Task<bool> AddLike(MemberLike like)
    {
        if (!await CheckIfMembersExist(like))
            return false;
        if (await CheckIfLikeExists(like))
            return false;

        // Add the like to the DB, and return state comparison.
        // State of newly added like must be equal to 'Added'.
        EntityEntry<MemberLike> result = await context.Likes.AddAsync(like);
        return result.State == EntityState.Added;
    }

    public async Task<bool> DeleteLike(MemberLike like)
    {
        if (!await CheckIfMembersExist(like))
            return false;
        if (!await CheckIfLikeExists(like))
            return false;

        EntityEntry<MemberLike> result = context.Likes.Remove(like);
        return result.State == EntityState.Deleted;
    }

    public async Task<IReadOnlyList<string>> GetCurrentMemberLikeIdsAsync(string memberId)
    {
        return await context.Likes
                .Where(like => like.SourceMemberId == memberId)
                .Select(like => like.TargetMemberId)
                .ToListAsync();
    }

    public async Task<MemberLike?> GetMemberLikeAsync(string sourceMemberId, string targetMemberId)
    {
        return await context.Likes
                .SingleOrDefaultAsync(
                    like => like.SourceMemberId == sourceMemberId
                         && like.TargetMemberId == targetMemberId
                );
    }

    public async Task<IReadOnlyList<Member>> GetMembersFromLikesAsync(string predicate, string memberId)
    {
        IQueryable<MemberLike> query = context.Likes.AsQueryable();

        switch (predicate)
        {
            case "liked": // Get all the members which 'memberId' liked.
                return await query
                    .Where(x => x.SourceMemberId == memberId)
                    .Select(x => x.TargetMember)
                    .ToListAsync();
            
            case "likedBy":
                return await query // Get all the members which 'memberId' is liked by.
                    .Where(x => x.TargetMemberId == memberId)
                    .Select(x => x.SourceMember)
                    .ToListAsync();
            
            default:
                // Mutual - Get all the members which satisfy both conditions:
                // 1) 'memberId' is liked by them.
                // 2) 'memberId' also liked them.
                IReadOnlyList<string> likedIdsByCurrentMember 
                    = await GetCurrentMemberLikeIdsAsync(memberId);
                
                return await query
                    .Where(x => x.TargetMemberId == memberId
                        && likedIdsByCurrentMember.Contains(x.SourceMemberId))
                    .Select(x => x.SourceMember)
                    .ToListAsync();
        }
    }

    public async Task<bool> SaveAllChanges()
    {
        return await context.SaveChangesAsync() > 0;
    }

    private async Task<bool> CheckIfMembersExist(MemberLike like)
    {
        bool sourceMemberExists = await context.Members.FindAsync(like.SourceMemberId) != null;
        bool targetMemberExists = await context.Members.FindAsync(like.TargetMemberId) != null;

        return sourceMemberExists && targetMemberExists;
    }

    private async Task<bool> CheckIfLikeExists(MemberLike like)
    {
        bool likeExists = await context.Likes.FirstOrDefaultAsync(
            item => item.SourceMemberId == like.SourceMemberId
            && item.TargetMemberId == like.TargetMemberId) != null;

        return likeExists;
    }
}
