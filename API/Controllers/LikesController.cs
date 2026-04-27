using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LikesController(ILikesRepository likesRepository) : BaseApiController
    {
        [HttpPost("{targetMemberId}")]
        public async Task<ActionResult> ToggleLike(string targetMemberId)
        {
            string sourceMemberId = User.GetMemberId();

            if (sourceMemberId == targetMemberId)
                return BadRequest("You cannot like yourself. ");
            
            MemberLike? existingLike
                 = await likesRepository.GetMemberLikeAsync(sourceMemberId, targetMemberId);

            if (existingLike == null)
            {
                var like = new MemberLike
                {
                    SourceMemberId = sourceMemberId, 
                    TargetMemberId = targetMemberId
                };

                await likesRepository.AddLike(like);
            }
            else
            {
                await likesRepository.DeleteLike(existingLike);
            }

            if (await likesRepository.SaveAllChanges())
                return Ok();

            return BadRequest("Failed to update like. ");
        }

        [HttpGet("list")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetCurrentMemberLikeIds()
        {
            IReadOnlyList<string> memberLikeIds
                 = await likesRepository.GetCurrentMemberLikeIdsAsync(User.GetMemberId());
            
            return Ok(memberLikeIds);
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<Member>>> GetMembersFromLikes(
            [FromQuery] LikesParams likesParams)
        {
            likesParams.MemberId = User.GetMemberId();

            PaginatedResult<Member> members
                 = await likesRepository.GetMembersFromLikesAsync(likesParams);

            return Ok(members);
        }
    }
}
