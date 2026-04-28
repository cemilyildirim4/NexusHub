using Microsoft.AspNetCore.Diagnostics;
using Nexus.Core.Models;
using System.Net;
namespace Nexus.Api.Handlers
{

    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext , Exception exception , CancellationToken cancellationToken ) 
        {
            // hatayı logluyoruz (konsolda veya dosyada görebilmek için)
            logger.LogError(exception , "There is an error that not excepted {Message}" , exception.Message);

            //Kullanıcıya döneceiğimiz standart cevap
            var errorResponse = new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "An unexpected error occurred. Please try again later."
            };
            httpContext.Response.StatusCode = errorResponse.StatusCode;

            //cevabı json formatında döndürüyoruz
            await httpContext.Response.WriteAsJsonAsync(errorResponse , cancellationToken);

            return true; // hatayı işlediğimizi belirtiyoruz
        }
    }
}
