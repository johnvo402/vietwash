using System.Data.Common;
using Application.Common.Interfaces;
using Application.Common.Interfaces.UnitOfWorks;
using Ardalis.GuardClauses;
using Domain.Aggregates.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Infrastructure.Services.Managements
{
    public class ServiceLaundryService : IServiceLaundryService
    {
        private readonly IDbContext _context;
        private readonly ILogger _logger;
        private readonly DbSet<Service> _serviceContext;
        private readonly DbSet<Unit> _unitContext;
        private readonly DbSet<Category> _categoryContext;
        private readonly DbSet<UnitRelation> _unitRelationContext;

        public ServiceLaundryService(IDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
            _serviceContext = context.Set<Service>();
            _unitContext = context.Set<Unit>();
            _categoryContext = context.Set<Category>();
            _unitRelationContext = context.Set<UnitRelation>();
        }

        public DbSet<Service> Services => _serviceContext;
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

        public async Task UpdateServiceAsync(
            Service service,
            IEnumerable<UnitRelation> unitRelations,
            DbTransaction? transaction = null
        )
        {
            bool hasExternalTransaction = transaction != null;

            try
            {
                if (!hasExternalTransaction)
                    await _context.DatabaseFacade.BeginTransactionAsync();

                // Lấy danh sách UnitRelation hiện tại
                var existingUnitRelations = service.UnitRelations.ToList();

                // Xác định UnitRelation cần xóa (không còn trong danh sách mới)
                var unitRelationIdsToKeep = unitRelations
                    .Select(ur => ur.Id)
                    .Where(id => id > 0)
                    .ToList();
                var unitRelationsToRemove = existingUnitRelations
                    .Where(ur => !unitRelationIdsToKeep.Contains(ur.Id))
                    .ToList();

                // Xóa UnitRelation không còn trong danh sách mới
                _unitRelationContext.RemoveRange(unitRelationsToRemove);

                // Cập nhật hoặc thêm mới UnitRelation
                foreach (var unitRelation in unitRelations)
                {
                    unitRelation.ServiceId = service.Id;
                    var existingUnitRelation = existingUnitRelations.FirstOrDefault(ur =>
                        ur.Id == unitRelation.Id
                    );
                    if (existingUnitRelation != null)
                    {
                        _unitRelationContext.Update(unitRelation); // Cập nhật
                    }
                    else if (unitRelation.Id == 0 || unitRelation.Id == null)
                    {
                        unitRelation.Id = 0; // Đảm bảo DB tự tăng
                        _unitRelationContext.Add(unitRelation); // Thêm mới
                    }
                }
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

        public async Task AddUnitToServiceAsync(
            IEnumerable<UnitRelation> unitRelations,
            DbTransaction? transaction = null
        )
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
                    //Guard.Against.Default(relation.UnitId, nameof(relation.UnitId));
                    Guard.Against.Default(relation.ServiceId, nameof(relation.ServiceId));

                    // Check if relation already exists
                    var existingRelation = await _unitRelationContext.FirstOrDefaultAsync(ur =>
                        //ur.UnitId == relation.UnitId &&
                        ur.ServiceId == relation.ServiceId
                    );

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
        //public async Task<List<Service>> GetServicesByGroupIdAsync(long groupId)
        //{
        //    return await _serviceContext
        //        .Where(s => s.GroupServices.Any(gs => gs.GroupId == groupId))
        //        .ToListAsync();
        //}
    }
}
