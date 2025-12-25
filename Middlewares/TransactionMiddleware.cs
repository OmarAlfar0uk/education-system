using EduocationSystem.Domain.Interfaces;

namespace EduocationSystem.Middlewares
{
    public class TransactionMiddleware : IMiddleware
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionMiddleware(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                await next(context);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}