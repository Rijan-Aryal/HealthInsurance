using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerDto>> GetAllAsync()
    {
        return await _context.Customers
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                FullName = c.FullName,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                DateOfBirth = c.DateOfBirth,
                CitizenshipNumber = c.CitizenshipNumber,
                CreatedDate = c.CreatedDate
            })
            .ToListAsync();
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return null;

        return new CustomerDto
        {
            Id = customer.Id,
            FullName = customer.FullName,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            DateOfBirth = customer.DateOfBirth,
            CitizenshipNumber = customer.CitizenshipNumber,
            CreatedDate = customer.CreatedDate
        };
    }

    public async Task<Customer> AddAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer?> UpdateAsync(Guid id, Customer customer)
    {
        var existing = await _context.Customers.FindAsync(id);
        if (existing == null) return null;

        existing.FullName = customer.FullName;
        existing.Email = customer.Email;
        existing.Phone = customer.Phone;
        existing.Address = customer.Address;
        existing.DateOfBirth = customer.DateOfBirth;
        existing.CitizenshipNumber = customer.CitizenshipNumber;
        // UpdatedDate handled by BaseEntity if exists

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return false;

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        return true;
    }
}

