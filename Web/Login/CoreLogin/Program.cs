//using Microsoft.AspNetCore.HttpOverrides; 



using WebCore.Extension;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddMvc().AddNewtonsoftJson();
builder.Services.AddCors();
builder.Services.AddResponseCompression();

builder.WebHost.UseUrls("http://*:18020");




HttpContextHelper.Register(builder.Services);
var app = builder.Build();
var env = app.Environment;

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
HttpContextHelper.Initialize(app, env);
//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
//app.UseForwardedHeaders(new ForwardedHeadersOptions
//{
//    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
//});
app.UseAuthorization();
app.UseCors(configurePolicy => configurePolicy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
