using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerRepository _repository;

    public CustomerController(ICustomerRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomerDto>>> GetAll()
    {
        var customers = await _repository.GetAllAsync();
        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetById(Guid id)
    {
        var customer = await _repository.GetByIdAsync(id);
        if (customer == null) return NotFound();
        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(CreateCustomerDto dto)
    {
        var customer = new Customer
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            DateOfBirth = dto.DateOfBirth,
            CitizenshipNumber = dto.CitizenshipNumber
        };

        var created = await _repository.AddAsync(customer);
        var createdDto = await _repository.GetByIdAsync(created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, createdDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerDto>> Update(Guid id, UpdateCustomerDto dto)
    {
        var customer = new Customer
        {
            Id = id,
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            DateOfBirth = dto.DateOfBirth,
            CitizenshipNumber = dto.CitizenshipNumber
        };

        var updated = await _repository.UpdateAsync(id, customer);
        if (updated == null) return NotFound();
        var updatedDto = await _repository.GetByIdAsync(id);
        return Ok(updatedDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _repository.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}

