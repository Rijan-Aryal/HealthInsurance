using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PolicyController : ControllerBase
{
    private readonly IPolicyRepository _policyRepository;

    public PolicyController(IPolicyRepository policyRepository)
    {
        _policyRepository = policyRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<PolicyDto>>> GetAll()
    {
        var policies = await _policyRepository.GetAllAsync();
        return Ok(policies);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PolicyDto?>> Get(Guid id)
    {
        var policy = await _policyRepository.GetByIdAsync(id);
        if (policy == null) return NotFound();
        return Ok(policy);
    }

    [HttpPost]
    public async Task<ActionResult<PolicyDto>> Create(CreatePolicyDto dto)
    {
        var policy = new Policy
        {
            PolicyNumber = dto.PolicyNumber,
            CustomerId = dto.CustomerId,
            PolicyType = dto.PolicyType,
            Status = PolicyStatus.Draft,
            CoverageAmount = dto.CoverageAmount,
            Premium = dto.Premium,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsAutoRenewal = dto.IsAutoRenewal
        };
        var created = await _policyRepository.AddAsync(policy);
        var createdDto = await _policyRepository.GetByIdAsync(created.Id);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, createdDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, UpdatePolicyDto dto)
    {
        var policy = new Policy
        {
            Id = id,
            PolicyNumber = dto.PolicyNumber,
            PolicyType = dto.PolicyType,
            CoverageAmount = dto.CoverageAmount,
            Premium = dto.Premium
        };
        var updated = await _policyRepository.UpdateAsync(id, policy);
        if (updated == null) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var success = await _policyRepository.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }

    // ===== Lifecycle Endpoints =====

    [HttpPost("{id}/activate")]
    public async Task<ActionResult<PolicyDto>> Activate(Guid id)
    {
        try
        {
            var policy = await _policyRepository.ActivateAsync(id);
            if (policy == null) return NotFound();
            var dto = await _policyRepository.GetByIdAsync(policy.Id);
            return Ok(dto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<PolicyDto>> Cancel(Guid id, [FromBody] PolicyStatusUpdateDto? dto)
    {
        try
        {
            var policy = await _policyRepository.CancelAsync(id);
            if (policy == null) return NotFound();
            var result = await _policyRepository.GetByIdAsync(policy.Id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/expire")]
    public async Task<ActionResult<PolicyDto>> Expire(Guid id)
    {
        try
        {
            var policy = await _policyRepository.ExpireAsync(id);
            if (policy == null) return NotFound();
            var dto = await _policyRepository.GetByIdAsync(policy.Id);
            return Ok(dto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/renew")]
    public async Task<ActionResult<PolicyDto>> Renew(Guid id, [FromBody] RenewPolicyDto dto)
    {
        try
        {
            var policy = await _policyRepository.RenewAsync(id, dto.NewEndDate);
            if (policy == null) return NotFound();
            var result = await _policyRepository.GetByIdAsync(policy.Id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ===== Endorsement Endpoints =====

    [HttpPost("{id}/endorse")]
    public async Task<ActionResult<EndorsementDto>> AddEndorsement(Guid id, [FromBody] CreateEndorsementDto dto)
    {
        try
        {
            var endorsement = new PolicyEndorsement
            {
                Type = dto.Type,
                Description = dto.Description,
                OldValue = dto.OldValue,
                NewValue = dto.NewValue,
                EffectiveDate = dto.EffectiveDate
            };
            var created = await _policyRepository.AddEndorsementAsync(id, endorsement);
            return Ok(new EndorsementDto
            {
                Id = created.Id,
                PolicyId = created.PolicyId,
                Type = created.Type,
                Description = created.Description,
                OldValue = created.OldValue,
                NewValue = created.NewValue,
                EffectiveDate = created.EffectiveDate,
                CreatedDate = created.CreatedDate
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/endorsements")]
    public async Task<ActionResult<List<EndorsementDto>>> GetEndorsements(Guid id)
    {
        var endorsements = await _policyRepository.GetEndorsementsAsync(id);
        return Ok(endorsements);
    }

    // ===== Query Endpoints =====

    [HttpGet("by-status/{status}")]
    public async Task<ActionResult<List<PolicyDto>>> GetByStatus(PolicyStatus status)
    {
        var policies = await _policyRepository.GetByStatusAsync(status);
        return Ok(policies);
    }

    [HttpGet("expired")]
    public async Task<ActionResult<List<PolicyDto>>> GetExpired()
    {
        var policies = await _policyRepository.GetExpiredAsync();
        return Ok(policies);
    }

    [HttpGet("up-for-renewal")]
    public async Task<ActionResult<List<PolicyDto>>> GetUpForRenewal([FromQuery] int daysBeforeExpiry = 7)
    {
        var policies = await _policyRepository.GetUpForRenewalAsync(daysBeforeExpiry);
        return Ok(policies);
    }
}

