using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces;

public interface ICustomerRepository
{
    Task<List<CustomerDto>> GetAllAsync();
    Task<CustomerDto?> GetByIdAsync(Guid id);
    Task<Customer> AddAsync(Customer customer);
    Task<Customer?> UpdateAsync(Guid id, Customer customer);
    Task<bool> DeleteAsync(Guid id);
}

