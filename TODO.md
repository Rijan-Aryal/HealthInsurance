# Customer Feature Implementation TODO

## Remaining Steps:
- [x] 1. Update Domain/Entities/Policy.cs (add Customer relation)
- [x] 2. Create Application/DTOs/CustomerDto.cs
- [x] 3. Update Application/DTOs/PolicyDto.cs (add CustomerId/Customer)
- [x] 4. Create Application/Interfaces/ICustomerRepository.cs
- [x] 5. Update Infrastructure/Data/AppDbContext.cs (DbSet + relations)
- [x] 6. Create Infrastructure/Repositories/CustomerRepository.cs
- [x] 7. Update Infrastructure/DependencyInjection.cs (register repo)
- [x] 8. Create Presentation/Controllers/CustomerController.cs
- [ ] 9. Generate & run EF migration
- [ ] 10. Test CRUD endpoints

**Fixed PolicyController, IPolicyRepository, PolicyRepository to align with Customer changes (DTO projection, new fields). Build errors resolved.**

Progress will be updated after each step.

