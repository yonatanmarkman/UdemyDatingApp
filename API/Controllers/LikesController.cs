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
    public class LikesController(IUnitOfWork unitOfWork) : BaseApiController
    {
        [HttpPost("{targetMemberId}")]
        public async Task<ActionResult> ToggleLike(string targetMemberId)
        {
            string sourceMemberId = User.GetMemberId();

            if (sourceMemberId == targetMemberId)
                return BadRequest("You cannot like yourself. ");
            
            MemberLike? existingLike
                 = await unitOfWork.LikesRepository.GetMemberLikeAsync(sourceMemberId, targetMemberId);

            if (existingLike == null)
            {
                var like = new MemberLike
                {
                    SourceMemberId = sourceMemberId, 
                    TargetMemberId = targetMemberId
                };

                await unitOfWork.LikesRepository.AddLike(like);
            }
            else
            {
                await unitOfWork.LikesRepository.DeleteLike(existingLike);
            }

            if (await unitOfWork.Complete())
                return Ok();

            return BadRequest("Failed to update like. ");
        }

        [HttpGet("list")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetCurrentMemberLikeIds()
        {
            IReadOnlyList<string> memberLikeIds
                 = await unitOfWork.LikesRepository.GetCurrentMemberLikeIdsAsync(User.GetMemberId());
            
            return Ok(memberLikeIds);
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<Member>>> GetMembersFromLikes(
            [FromQuery] LikesParams likesParams)
        {
            likesParams.MemberId = User.GetMemberId();

            PaginatedResult<Member> members
                 = await unitOfWork.LikesRepository.GetMembersFromLikesAsync(likesParams);

            return Ok(members);
        }
    }
}
