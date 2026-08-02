using System.Security.Claims;
using API.Api.Extensions;
using API.Application.DTOs;
using API.Application.Helpers;
using API.Application.Interfaces;
using API.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Api.Controllers
{
    [Authorize]
    public class MembersController(IMemberUnitOfWork uow,
        IPhotoService photoService,
        IValidator<MemberUpdateDto> memberUpdateValidator,
        IValidator<SavePromptAnswersDto> savePromptAnswersValidator) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Member>>> GetMembers(
                [FromQuery] MemberParams memberParams)
        {
            memberParams.CurrentMemberId = User.GetMemberId();

            return Ok(await uow.MemberRepository.GetMembersAsync(memberParams));
        }

        [HttpGet("{id}")] // locahost:5001/api/members/bob-id
        public async Task<ActionResult<Member>> GetMember(string id)
        {
            var member = await uow.MemberRepository.GetMemberByIdAsync(id);

            if (member == null) return NotFound();

            return member;
        }

        [HttpGet("{id}/photos")]
        public async Task<ActionResult<IReadOnlyList<Photo>>> GetMemberPhotos(string id)
        {
            var isCurrentUser = User.GetMemberId() == id;
            return Ok(await uow.MemberRepository.GetPhotosForMemberAsync(id, isCurrentUser));
        }

        [HttpGet("daily")]
        public async Task<ActionResult<IReadOnlyList<Member>>> GetDailySelection([FromQuery] int count = 6)
        {
            count = Math.Clamp(count, 1, 12);

            return Ok(await uow.MemberRepository.GetDailySelectionAsync(User.GetMemberId(), count));
        }

        [HttpGet("prompts/bank")]
        public async Task<ActionResult<IReadOnlyList<Prompt>>> GetPromptBank()
        {
            return Ok(await uow.MemberRepository.GetPromptBankAsync());
        }

        [HttpPut("prompts")]
        public async Task<ActionResult> SavePromptAnswers(SavePromptAnswersDto dto)
        {
            var validationResult = await savePromptAnswersValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                validationResult.AddToModelState(ModelState);
                return ValidationProblem();
            }

            if (dto.Answers.Select(x => x.PromptId).Distinct().Count() != dto.Answers.Count)
                return BadRequest("Chaque prompt ne peut être choisi qu'une fois");

            var member = await uow.MemberRepository.GetMemberForUpdate(User.GetMemberId());

            if (member == null) return BadRequest("Cannot get member from token");

            var bank = await uow.MemberRepository.GetPromptBankAsync();
            var validIds = bank.Select(x => x.Id).ToHashSet();

            if (dto.Answers.Any(a => !validIds.Contains(a.PromptId)))
                return BadRequest("Invalid prompt selected");

            member.PromptAnswers.Clear();
            for (var i = 0; i < dto.Answers.Count; i++)
            {
                member.PromptAnswers.Add(new PromptAnswer
                {
                    MemberId = member.Id,
                    PromptId = dto.Answers[i].PromptId,
                    Answer = dto.Answers[i].Answer,
                    DisplayOrder = i
                });
            }

            if (await uow.Complete()) return NoContent();

            return BadRequest("Problem saving prompt answers");
        }

        [HttpPut]
        public async Task<ActionResult> UpdateMember(MemberUpdateDto memberUpdateDto)
        {
            var validationResult = await memberUpdateValidator.ValidateAsync(memberUpdateDto);
            if (!validationResult.IsValid)
            {
                validationResult.AddToModelState(ModelState);
                return ValidationProblem();
            }

            var memberId = User.GetMemberId();

            var member = await uow.MemberRepository.GetMemberForUpdate(memberId);

            if (member == null) return BadRequest("Could not get member");

            member.DisplayName = memberUpdateDto.DisplayName ?? member.DisplayName;
            member.Description = memberUpdateDto.Description ?? member.Description;
            member.City = memberUpdateDto.City ?? member.City;
            member.Country = memberUpdateDto.Country ?? member.Country;

            member.User.DisplayName = memberUpdateDto.DisplayName ?? member.User.DisplayName;

            uow.MemberRepository.Update(member); // optional

            if (await uow.Complete()) return NoContent();

            return BadRequest("Failed to update member");
        }

        [HttpPost("add-photo")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Photo>> AddPhoto(IFormFile file)
        {
            var member = await uow.MemberRepository.GetMemberForUpdate(User.GetMemberId());

            if (member == null) return BadRequest("Cannot update member");

            var result = await photoService.UploadPhotoAsync(file);

            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
                MemberId = User.GetMemberId(),
                IsApproved = true
            };

            if (member.ImageUrl == null)
            {
                member.ImageUrl = photo.Url;
                member.User.ImageUrl = photo.Url;
            }

            member.Photos.Add(photo);

            if (await uow.Complete()) return photo;

            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var member = await uow.MemberRepository.GetMemberForUpdate(User.GetMemberId());

            if (member == null) return BadRequest("Cannot get member from token");

            var photo = member.Photos.SingleOrDefault(x => x.Id == photoId);

            if (member.ImageUrl == photo?.Url || photo == null)
            {
                return BadRequest("Cannot set this as main image");
            }

            member.ImageUrl = photo.Url;
            member.User.ImageUrl = photo.Url;

            if (await uow.Complete()) return NoContent();

            return BadRequest("Problem setting main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var member = await uow.MemberRepository.GetMemberForUpdate(User.GetMemberId());

            if (member == null) return BadRequest("Cannot get member from token");

            var photo = member.Photos.SingleOrDefault(x => x.Id == photoId);

            if (photo == null || photo.Url == member.ImageUrl)
            {
                return BadRequest("This photo cannot be deleted");
            }

            if (photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            member.Photos.Remove(photo);

            if (await uow.Complete()) return Ok();

            return BadRequest("Problem deleting the photo");
        }
    }
}
