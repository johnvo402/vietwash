using Application.Common.Interfaces;
using Domain.Aggregates.Services;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Ardalis.GuardClauses;
using Serilog;
using Application.Common.Interfaces.UnitOfWorks;

namespace Infrastructure.Services.Managements
{
    public class ServiceLaundryService : IServiceLaundryService
    {
        private readonly IDbContext _context;
        private readonly ILogger _logger;
        private readonly DbSet<Service> _serviceContext;
        private readonly DbSet<Group> _groupContext;
        private readonly DbSet<Unit> _unitContext;
        private readonly DbSet<Category> _categoryContext;
        private readonly DbSet<GroupService> _groupServiceContext;
        private readonly DbSet<UnitRelation> _unitRelationContext;

        public ServiceLaundryService(IDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
            _serviceContext = context.Set<Service>();
            _groupContext = context.Set<Group>();
            _unitContext = context.Set<Unit>();
            _categoryContext = context.Set<Category>();
            _groupServiceContext = context.Set<GroupService>();
            _unitRelationContext = context.Set<UnitRelation>();
        }

        public DbSet<Service> Services => _serviceContext;
        public DbSet<Group> Groups => _groupContext;
        public DbSet<Unit> Units => _unitContext;
        public DbSet<UnitRelation> UnitRelations => _unitRelationContext;

        public async Task CreateServiceAsync(Service service, DbTransaction? transaction = null)
        {
            var isOwnerTransaction = transaction != null;
            try
            {
                if (!isOwnerTransaction && _context.DatabaseFacade.CurrentTransaction == null)
                    await _context.DatabaseFacade.BeginTransactionAsync();

                await _serviceContext.AddAsync(service);
                await _context.SaveChangesAsync();

                if (!isOwnerTransaction)
                    await _context.DatabaseFacade.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "Error in method {MethodName}. Exception Type: {ExceptionType}. Error Message: {ErrorMessage}. StackTrace: {StackTrace}",
                    nameof(CreateServiceAsync),
                    ex.GetType().Name,
                    ex.Message,
                    ex.StackTrace
                );

                if (!isOwnerTransaction)
                    await _context.DatabaseFacade.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task UpdateServiceAsync(Service service, IEnumerable<UnitRelation> unitRelations, DbTransaction? transaction = null)
        {
            bool hasExternalTransaction = transaction != null;

            try
            {
                if (!hasExternalTransaction)
                    await _context.DatabaseFacade.BeginTransactionAsync();

                // Xóa toàn bộ UnitRelation cũ
                _unitRelationContext.RemoveRange(service.UnitRelations);
                service.UnitRelations.Clear(); // Xóa trong bộ nhớ

                // Thêm tất cả UnitRelation mới
                var newUnitRelations = unitRelations.ToList();
                foreach (var unitRelation in newUnitRelations)
                {
                    unitRelation.ServiceId = service.Id; // Liên kết với Service
                    _unitRelationContext.Add(unitRelation); // Thêm mới
                }
                service.UnitRelations = newUnitRelations; // Gán danh sách mới

                // Cập nhật Service
                _serviceContext.Update(service);

                await _context.SaveChangesAsync();

                if (!hasExternalTransaction)
                    await _context.DatabaseFacade.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating service {ServiceId}", service.Id);
                if (!hasExternalTransaction)
                    await _context.DatabaseFacade.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task CreateGroupAsync(Group group, DbTransaction? transaction = null)
        {
            var isOwnerTransaction = transaction != null;
            try
            {
                if (!isOwnerTransaction && _context.DatabaseFacade.CurrentTransaction == null)
                    await _context.DatabaseFacade.BeginTransactionAsync();

                await _groupContext.AddAsync(group);
                await _context.SaveChangesAsync();

                if (!isOwnerTransaction)
                    await _context.DatabaseFacade.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "Error in method {MethodName}. Exception Type: {ExceptionType}. Error Message: {ErrorMessage}. StackTrace: {StackTrace}",
                    nameof(CreateGroupAsync),
                    ex.GetType().Name,
                    ex.Message,
                    ex.StackTrace
                );

                if (!isOwnerTransaction)
                    await _context.DatabaseFacade.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task UpdateGroupAsync(Group group, IEnumerable<Service> services, DbTransaction? transaction = null)
        {
            var isOwnerTransaction = transaction != null;
            try
            {
                if (!isOwnerTransaction && _context.DatabaseFacade.CurrentTransaction == null)
                    await _context.DatabaseFacade.BeginTransactionAsync();

                // Update the group
                _groupContext.Update(group);
                await _context.SaveChangesAsync();

                // Update services if provided
                if (services?.Any() == true)
                {
                    // Get current services for this group using the many-to-many relationship
                    var currentGroupServices = await _groupServiceContext
                        .Where(gs => gs.GroupId == group.Id)
                        .ToListAsync();

                    var currentServiceIds = currentGroupServices.Select(gs => gs.ServiceId).ToList();
                    var newServiceIds = services.Select(s => s.Id).ToList();

                    // Find services to add to the group
                    var servicesToAdd = newServiceIds.Except(currentServiceIds).ToList();

                    // Find services to remove from the group
                    var servicesToRemove = currentServiceIds.Except(newServiceIds).ToList();

                    // Add new group-service relationships
                    foreach (var serviceId in servicesToAdd)
                    {
                        await _groupServiceContext.AddAsync(new GroupService
                        {
                            GroupId = group.Id,
                            ServiceId = serviceId
                        });
                    }

                    // Remove old group-service relationships
                    var groupServicesToRemove = currentGroupServices
                        .Where(gs => servicesToRemove.Contains(gs.ServiceId))
                        .ToList();

                    _groupServiceContext.RemoveRange(groupServicesToRemove);

                    await _context.SaveChangesAsync();
                }

                if (!isOwnerTransaction)
                    await _context.DatabaseFacade.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "Error in method {MethodName}. Exception Type: {ExceptionType}. Error Message: {ErrorMessage}. StackTrace: {StackTrace}",
                    nameof(UpdateGroupAsync),
                    ex.GetType().Name,
                    ex.Message,
                    ex.StackTrace
                );

                if (!isOwnerTransaction)
                    await _context.DatabaseFacade.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task CreateUnitAsync(Unit unit, DbTransaction? transaction = null)
        {
            var isOwnerTransaction = transaction != null;
            try
            {
                if (!isOwnerTransaction && _context.DatabaseFacade.CurrentTransaction == null)
                    await _context.DatabaseFacade.BeginTransactionAsync();

                await _unitContext.AddAsync(unit);
                await _context.SaveChangesAsync();

                if (!isOwnerTransaction)
                    await _context.DatabaseFacade.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "Error in method {MethodName}. Exception Type: {ExceptionType}. Error Message: {ErrorMessage}. StackTrace: {StackTrace}",
                    nameof(CreateUnitAsync),
                    ex.GetType().Name,
                    ex.Message,
                    ex.StackTrace
                );

                if (!isOwnerTransaction)
                    await _context.DatabaseFacade.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task UpdateUnitAsync(Unit unit, DbTransaction? transaction = null)
        {
            var isOwnerTransaction = transaction != null;
            try
            {
                if (!isOwnerTransaction && _context.DatabaseFacade.CurrentTransaction == null)
                    await _context.DatabaseFacade.BeginTransactionAsync();

                _unitContext.Update(unit);
                await _context.SaveChangesAsync();

                if (!isOwnerTransaction)
                    await _context.DatabaseFacade.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "Error in method {MethodName}. Exception Type: {ExceptionType}. Error Message: {ErrorMessage}. StackTrace: {StackTrace}",
                    nameof(UpdateUnitAsync),
                    ex.GetType().Name,
                    ex.Message,
                    ex.StackTrace
                );

                if (!isOwnerTransaction)
                    await _context.DatabaseFacade.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task AddUnitToServiceAsync(IEnumerable<UnitRelation> unitRelations, DbTransaction? transaction = null)
        {
            Guard.Against.Null(unitRelations, nameof(unitRelations));

            var isOwnerTransaction = transaction != null;
            try
            {
                if (!isOwnerTransaction && _context.DatabaseFacade.CurrentTransaction == null)
                    await _context.DatabaseFacade.BeginTransactionAsync();

                foreach (var relation in unitRelations)
                {
                    // Make sure relation has both IDs
                    Guard.Against.Default(relation.UnitId, nameof(relation.UnitId));
                    Guard.Against.Default(relation.ServiceId, nameof(relation.ServiceId));

                    // Check if relation already exists
                    var existingRelation = await _unitRelationContext
                        .FirstOrDefaultAsync(ur =>
                            ur.UnitId == relation.UnitId &&
                            ur.ServiceId == relation.ServiceId);

                    if (existingRelation == null)
                    {

                        await _unitRelationContext.AddAsync(relation);
                    }
                }

                await _context.SaveChangesAsync();

                if (!isOwnerTransaction)
                    await _context.DatabaseFacade.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "Error in method {MethodName}. Exception Type: {ExceptionType}. Error Message: {ErrorMessage}. StackTrace: {StackTrace}",
                    nameof(AddUnitToServiceAsync),
                    ex.GetType().Name,
                    ex.Message,
                    ex.StackTrace
                );

                if (!isOwnerTransaction)
                    await _context.DatabaseFacade.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task CreateCategory(Category category, DbTransaction? transaction = null)
        {
            var isOwnerTransaction = transaction != null;
            try
            {
                if (!isOwnerTransaction && _context.DatabaseFacade.CurrentTransaction == null)
                    await _context.DatabaseFacade.BeginTransactionAsync();

                await _categoryContext.AddAsync(category);
                await _context.SaveChangesAsync();

                if (!isOwnerTransaction)
                    await _context.DatabaseFacade.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "Error in method {MethodName}. Exception Type: {ExceptionType}. Error Message: {ErrorMessage}. StackTrace: {StackTrace}",
                    nameof(CreateCategory),
                    ex.GetType().Name,
                    ex.Message,
                    ex.StackTrace
                );

                if (!isOwnerTransaction)
                    await _context.DatabaseFacade.RollbackTransactionAsync();
                throw;
            }
        }

        // Helper methods
        public async Task<List<Service>> GetServicesByGroupIdAsync(Ulid groupId)
        {
            return await _serviceContext
                .Where(s => s.GroupServices.Any(gs => gs.GroupId == groupId))
                .ToListAsync();
        }

        public async Task<List<Group>> GetGroupsByServiceIdAsync(Ulid serviceId)
        {
            return await _groupContext
                .Where(g => g.GroupServices.Any(gs => gs.ServiceId == serviceId))
                .ToListAsync();
        }


    }
}