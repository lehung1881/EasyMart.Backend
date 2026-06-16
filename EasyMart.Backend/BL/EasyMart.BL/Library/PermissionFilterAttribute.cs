using BASE.Service.Core.Services;
using BASE.Service.Core.Utils;
using EasyMart.BL.System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

namespace EasyMart.BL.Library
{
    /// <summary>
    /// Filter kiểm tra quyền truy cập API.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class PermissionFilterAttribute : ActionFilterAttribute
    {
        protected IServiceProvider _serviceProvider;
        /// <summary>
        /// Mã quyền cần kiểm tra.
        /// </summary>
        private readonly string _permissionCode;

        /// <summary>
        /// Khởi tạo filter kiểm tra quyền.
        /// </summary>
        /// <param name="permissionCode">Mã quyền yêu cầu.</param>
        public PermissionFilterAttribute(string permissionCode)
        {
            _permissionCode = permissionCode;
        }

        /// <summary>
        /// Kiểm tra quyền trước khi thực thi Action.
        /// Trả về lỗi nếu người dùng không có quyền truy cập.
        /// </summary>
        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            //if (IsAllowAnonymous(context))
            //{
            //    await next();
            //    return;
            //}

            bool check = await CheckPermissionAction(context);

            if (!check)
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status200OK;
                context.HttpContext.Response.ContentType = "application/json; charset=UTF-8";

                var errorResponse = new
                {
                    Success = false,
                    UserMessage = "Không có quyền thực hiện chức năng",
                    SubCode = 403
                };

                string jsonResponse = ConvertUtil.SerializeObject(errorResponse);

                await context.HttpContext.Response.WriteAsync(
                    jsonResponse,
                    Encoding.UTF8);

                return;
            }

            await next();
        }

        /// <summary>
        /// Kiểm tra quyền thực hiện Action hiện tại.
        /// Trả về true nếu được phép truy cập.
        /// </summary>
        private async Task<bool> CheckPermissionAction(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;

            // Lấy UserId từ JWT
            string? userId = httpContext.User?
                .FindFirst("UserId")?
                .Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }

            var bl = new BLUser(new BASE.Service.Core.BL.CoreWebServiceCollection(_serviceProvider));
            await bl.GetUsercache();

            // TODO:
            // Resolve PermissionService từ DI

            // var permissionService = httpContext.RequestServices
            //     .GetRequiredService<IPermissionService>();

            // return await permissionService
            //     .HasPermissionAsync(userId, _permissionCode);

            return true;
        }

        /// <summary>
        /// Kiểm tra Action hiện tại có cho phép truy cập nặc danh hay không.
        /// Trả về true nếu có gắn AllowAnonymous.
        /// </summary>
        //private static bool IsAllowAnonymous(ActionExecutingContext context)
        //{
        //    return context.ActionDescriptor.EndpointMetadata
        //        .Any(x => x is AllowAnonymousAttribute);
        //}
    }
}