using BASE.Service.Core.Model;
using BASE.Service.Core.Web;
using Microsoft.Extensions.Options;
var builder = WebApplication.CreateBuilder(args);

//Khởi tạo ConfigureServices chung
BaseStartupServices.ConfigureServices(builder, builder.Configuration, false);

//Add những service riêng
//builder.Services.AddApplicationService(builder.Configuration);

//Build app
var app = builder.Build();
BaseStartupServices.ConfigureApp(app);
app.Run();
