using GiveAID.Application.Features.Faqs.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Faqs.Commands.Update;

/// <summary>
/// Handler for UpdateFaqCommand.
/// </summary>
public class UpdateFaqCommandHandler : IRequestHandler<UpdateFaqCommand, FaqDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateFaqCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FaqDto> Handle(UpdateFaqCommand request, CancellationToken cancellationToken)
    {
        var faq = await _context.Faqs.FindAsync(new object[] { request.FaqId }, cancellationToken);

        if (faq == null)
        {
            throw new InvalidOperationException($"FAQ with ID {request.FaqId} not found.");
        }

        if (request.Question != null) faq.Question = request.Question;
        if (request.Answer != null) faq.Answer = request.Answer;
        if (request.Category != null) faq.Category = request.Category;
        if (request.DisplayOrder.HasValue) faq.DisplayOrder = request.DisplayOrder.Value;
        if (request.IsActive.HasValue) faq.IsActive = request.IsActive.Value;
        if (request.IsFeatured.HasValue) faq.IsFeatured = request.IsFeatured.Value;
        faq.UpdatedAt = DateTime.UtcNow;

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
