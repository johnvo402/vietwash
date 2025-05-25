using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services.Identity;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Services;
using Domain.Aggregates.Services.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces.Services;

namespace Application.Feature.Services.Command.Update;

public class UpdateServiceHandler(
	IUnitOfWork unitOfWork,
	IMapper mapper,
	IMediaUpdateService<Service> mediaUpdateService,
	IServiceLaundryService serviceLaundryService,
	IActionAccessorService accessorService
) : IRequestHandler<UpdateServiceCommand, UpdateServiceResponse>
{
	public async ValueTask<UpdateServiceResponse> Handle(UpdateServiceCommand command, CancellationToken cancellationToken)
	{
		Service? existingService = await unitOfWork.Repository<Service>().FindByConditionAsync(new GetServiceWithIncludeByIdSpecification(command.ServiceId), cancellationToken)
			?? throw new NotFoundException(
	 [Messager.Create<Service>().Message(MessageType.Found).Negative().BuildMessage()]
 );

		string? oldServiceImage = existingService.Image;

		mapper.Map(command.Service, existingService);

		var currentUserId = accessorService.Id;
		existingService.UpdatedBy = currentUserId;
		existingService.UpdatedAt = DateTime.UtcNow;

		// Kiểm tra và tạo Slug nếu name thay đổi
		//if (existingService.Name != command.Service.Name)
		//{
		//	existingService.Slug = GenerateSlug(command.Service.Name);

		//	bool slugExists = await unitOfWork.Repository<Service>()
		//		.AnyAsync(s => s.Id != existingService.Id && s.Slug == existingService.Slug, cancellationToken);
		//	if (slugExists)
		//	{
		//		throw new InvalidOperationException($"Slug '{existingService.Slug}' already exists.");
		//	}
		//}

		// Lấy danh sách UnitRelation cũ
        var existingUnitRelations = existingService.UnitRelations.ToList();
        // Ánh xạ UnitRelations từ request
        var updatedUnitRelations = mapper.Map<List<UnitRelation>>(command.Service.UnitRelations);
        // Danh sách để lưu các UnitRelation sẽ được cập nhật hoặc thêm mới
        var unitRelationsToProcess = new List<UnitRelation>();

        foreach (var updatedUnitRelation in updatedUnitRelations)
        {
            // Đảm bảo ReferenceId đúng
            updatedUnitRelation.ReferenceId = existingService.Id;
            // Tìm UnitRelation cũ khớp với Id (nếu có)
            var existingUnitRelation = existingUnitRelations.FirstOrDefault(ur => ur.Id == updatedUnitRelation.Id);
            if (existingUnitRelation != null)
            {
                // Cập nhật UnitRelation cũ
                mapper.Map(updatedUnitRelation, existingUnitRelation);
                unitRelationsToProcess.Add(existingUnitRelation);
            }
            else
            {
                // Thêm mới UnitRelation
                updatedUnitRelation.CreatedBy = currentUserId;
                unitRelationsToProcess.Add(updatedUnitRelation);
            }
        }
		string? newServiceImage = existingService.Image;
		try
		{
			DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

			await serviceLaundryService.UpdateServiceAsync(existingService, unitRelationsToProcess, transaction);

			await unitOfWork.SaveAsync(cancellationToken);
			await unitOfWork.CommitAsync(cancellationToken);

			if (!string.IsNullOrEmpty(oldServiceImage))
			{
				await mediaUpdateService.DeleteAvatarAsync(oldServiceImage);
			}

			return mapper.Map<UpdateServiceResponse>(existingService);
		}

		catch
		{
			if (!string.IsNullOrEmpty(existingService.Image))
			{
				await mediaUpdateService.DeleteAvatarAsync(existingService.Image);
			}
			await unitOfWork.RollbackAsync(cancellationToken);
			throw;
		}
	}

	public string GenerateSlug(string input)
	{
		string slug = input.ToLowerInvariant()
			.Normalize(NormalizationForm.FormD);

		var sb = new StringBuilder();
		foreach (var c in slug)
			if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
				sb.Append(c);

		slug = Regex.Replace(sb.ToString(), @"[^a-z0-9\s-]", "");
		slug = Regex.Replace(slug, @"[\s-]+", "-").Trim('-');

		return slug;
	}
}


