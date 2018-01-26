using AutoMapper;
using CoralTime.BL.Interfaces;
using CoralTime.DAL.Models;
using CoralTime.Services;
using CoralTime.ViewModels.Errors;
using CoralTime.ViewModels.Member;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoralTime.Api.v1.Odata.Members
{
    [Authorize]
    [Route("api/v1/odata/[controller]")]
    public class MembersController : BaseODataController<MembersController, IMemberService>
    {
        public MembersController(IMemberService service, ILogger<MembersController> logger, IMapper mapper)
            : base(logger, mapper, service) { }

        // GET: api/v1/odata/Members
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                return Ok(_service.GetAllMembers(this.GetUserNameWithImpersonation()));
            }
            catch (Exception e)
            {
                return SendErrorResponse(e);
            }
        }

        // GET api/v1/odata/Members(5)
        [ODataRoute("Members({id})")]
        [HttpGet("{id}")]
        public IActionResult GetById([FromODataUri]int id)
        {
            try
            {
                var value = _service.GetById(id);
                return Ok(_mapper.Map<Member, MemberView>(value));
            }
            catch (Exception e)
            {
                return SendErrorResponse(e);
            }
        }

        // GET api/v1/odata/Members(2)/projects
        [ODataRoute("Members({id})/projects")]
        [HttpGet("{id}/projects")]
        public IActionResult GetProjects([FromODataUri]int id)
        {
            try
            {
                var result = _service.GetTimeTrackerAllProjects(id);
                return Ok(result);
            }
            catch (Exception e)
            {
                return SendErrorResponse(e);
            }
        }

        // POST: api/v1/odata/Members
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([FromBody]MemberView memberData)
        {
            if (!ModelState.IsValid)
            {
                return SendInvalidModelResponse();
            }

            try
            {
                var createNewUserResult = await _service.CreateNewUser(memberData);

                if (memberData.SendInvitationEmail)
                {
                    var baseUrl = $"{Request.Scheme}://{Request.Host.Host}:{Request.Host.Port}";
                    await _service.SentInvitationEmailAsync(memberData, baseUrl);
                }

                var locationUri = $"{Request.Host}/api/v1/odata/Members/{createNewUserResult.Id}";

                return Created(locationUri, _mapper.Map<Member, MemberView>(createNewUserResult));
            }
            catch (Exception e)
            {
                return SendErrorResponse(e);
            }
        }

        // PUT: api/v1/odata/Members(1)
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody]MemberView memberView)
        {
            if (!ModelState.IsValid)
            {
                return SendInvalidModelResponse();
            }

            memberView.Id = id;

            try
            {
                var updatedMember = await _service.Update(this.GetUserName(), memberView);

                if (memberView.SendInvitationEmail)
                {
                    var baseUrl = $"{Request.Scheme}://{Request.Host.Host}:{Request.Host.Port}";
                    await _service.SentUpdateAccountEmailAsync(updatedMember, baseUrl);
                }

                return Ok(updatedMember);
            }
            catch (Exception e)
            {
                return SendErrorResponse(e);
            }
        }

        //DELETE :api/v1/odata/Members(1)
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Delete(int id)
        {
            return BadRequest($"Can't delete the member with Id - {id}");
        }
    }
}