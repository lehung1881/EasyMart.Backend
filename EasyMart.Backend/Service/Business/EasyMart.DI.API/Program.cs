using BASE.Service.Core.Web;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

//Khởi tạo ConfigureServices chung
BaseStartupServices.ConfigureServices(builder, builder.Configuration);

//Khởi tạo cấu hình toàn chương trình
//var configGlobal = new GlobalConfig();
//new ConfigureFromConfigurationOptions<GlobalConfig>(builder.Configuration).Configure(configGlobal);
//ConfigUtils.InitGlobalConfig(configGlobal);

//Add những service riêng
//builder.Services.AddApplicationService(builder.Configuration);

//Build app
var app = builder.Build();
BaseStartupServices.ConfigureApp(app);
app.Run();
