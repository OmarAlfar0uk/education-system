using MediatR;

namespace EduocationSystem.Features.Questions.Queries
{
    public record GetQuestionTypesQuery(int id) :IRequest<string>;
    public class GetQuestionTypesHandler : IRequestHandler<GetQuestionTypesQuery, string>
    {
        public GetQuestionTypesHandler()
        {
        }
        public async Task<string> Handle(GetQuestionTypesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var types = new Dictionary<int, string>
                {
                    { 1, "Multiple Choice" },
                    { 2, "True/False" },
                    { 3, "Short Answer" },
                    { 4, "Essay" },
                    { 5, "Matching" },
                    { 6, "Fill in the Blanks" }
                };
                if (types.TryGetValue(request.id, out var typeName))
                {
                    return typeName;
                }
                else
                {
                    return "Unknown Type";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
