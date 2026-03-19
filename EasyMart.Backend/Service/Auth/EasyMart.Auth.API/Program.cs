using BASE.Service.Core.Model;
using BASE.Service.Core.Web;
using Microsoft.Extensions.Options;
var builder = WebApplication.CreateBuilder(args);

Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

//Khởi tạo ConfigureServices chung
BaseStartupServices.ConfigureServices(builder, builder.Configuration);

//Add những service riêng
//builder.Services.AddApplicationService(builder.Configuration);

//Build app
var app = builder.Build();
BaseStartupServices.ConfigureApp(app);
app.Run();
