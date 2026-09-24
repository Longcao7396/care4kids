using GiveAID.Application.Features.Faqs.DTOs;
using GiveAID.Domain.Entities;
using MediatR;

namespace GiveAID.Application.Features.Faqs.Commands.Create;

/// <summary>
/// Handler for CreateFaqCommand.
/// </summary>
public class CreateFaqCommandHandler : IRequestHandler<CreateFaqCommand, FaqDto>
{
    private readonly IApplicationDbContext _context;

    public CreateFaqCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FaqDto> Handle(CreateFaqCommand request, CancellationToken cancellationToken)
    {
        var faq = new Faq
        {
            Question = request.Question,
            Answer = request.Answer,
            Category = request.Category,
            DisplayOrder = request.DisplayOrder,
            IsFeatured = request.IsFeatured,
            IsActive = true,
            ViewCount = 0,
            CreatedBy = request.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Faqs.Add(faq);
        await _context.SaveChangesAsync(cancellationToken);

        return new FaqDto
        {
            FaqId = faq.FaqId,
            Question = faq.Question,
            Answer = faq.Answer,
            Category = faq.Category,
            DisplayOrder = faq.DisplayOrder,
            IsActive = faq.IsActive,
            IsFeatured = faq.IsFeatured,
            ViewCount = faq.ViewCount
        };
    }
}
