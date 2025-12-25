using MediatR;
using EduocationSystem.Features.Accounts.Commands;
using EduocationSystem.Features.Accounts.Dtos;
using EduocationSystem.Shared.Responses;

namespace EduocationSystem.Features.Accounts.Orchestrators
{
    public record OrchestrateForgetPasswordCommand(ForgotPasswordDto Dto) : IRequest<ServiceResponse<bool>>;

    public class OrchestrateForgetPasswordCommandHandler : IRequestHandler<OrchestrateForgetPasswordCommand, ServiceResponse<bool>>
    {
        private readonly IMediator _mediator;

        public OrchestrateForgetPasswordCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ServiceResponse<bool>> Handle(OrchestrateForgetPasswordCommand request, CancellationToken cancellationToken)
        {
            // Step 1: Dispatch ForgotPasswordCommand (generates and saves code)
            var forgotResult = await _mediator.Send(new ForgotPasswordCommand(request.Dto), cancellationToken);
            if (!forgotResult.IsSuccess)
                return forgotResult;

            // ForgotPasswordCommand already handles sending, so no need for additional step

            return ServiceResponse<bool>.SuccessResponse(true, "Reset code sent if email exists", "تم إرسال كود إعادة التعيين إذا كان البريد موجوداً");
        }
    }
}