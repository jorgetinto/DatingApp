using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository _repository;
        private readonly IMapper _mapper;

        public MessagesController(IDatingRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
        }

        [HttpGet("{id}", Name="GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();      

            var messageFormRepo = await _repository.GetMessage(id);

            if (messageFormRepo == null)
                return NotFound();

            return Ok(messageFormRepo);  
        }

        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser(int userId, 
        [FromQuery] MessageParams messageParams)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            messageParams.UserId = userId;

            var messagesFromRepo = await _repository.GetMessagesForUser(messageParams);
            var messages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);
            
            Response.AddPagination(messagesFromRepo.CurrentPage,
                                    messagesFromRepo.PageSize,
                                    messagesFromRepo.TotalCount,
                                    messagesFromRepo.TotalPages);
            return Ok(messages);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto messageForCreationDto)
        {
            var sender = await _repository.GetUser(userId);

            if (sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            messageForCreationDto.SenderId = userId;

            var recipient = await _repository.GetUser(messageForCreationDto.RecipientID);

            if (recipient == null)
                return BadRequest("Could not find user");
            
            var message = _mapper.Map<Message>(messageForCreationDto);

            _repository.Add(message);

            if (await _repository.SaveAll())
            {
                var messageToReturn = _mapper.Map<MessageToReturnDto>(message);
                return CreatedAtRoute("GetMessage", new {userId, id = message.Id}, messageToReturn);
            }

            throw new Exception("Creating the message failed on save");
        }

        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetMessageThread(int userId, int recipientId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageFromRepo = await _repository.GetMessageThread(userId, recipientId);

            var messageThread = _mapper.Map<IEnumerable<MessageToReturnDto>>(messageFromRepo);

            return Ok(messageThread);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(int id, int userId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            
            var messageFormRepo = await _repository.GetMessage(id);

            if (messageFormRepo.SenderId == userId)
                messageFormRepo.SenderDeleted = true;
            
            if (messageFormRepo.RecipientId == userId)
                messageFormRepo.RecipientDeleted = true;
            
            if (messageFormRepo.SenderDeleted && messageFormRepo.RecipientDeleted)
                _repository.Delete(messageFormRepo);
            
            if (await _repository.SaveAll())
                return NoContent();
            
            throw new Exception("Error deleting the message");
        }

    }
}