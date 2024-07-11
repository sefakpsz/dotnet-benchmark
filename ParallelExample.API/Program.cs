using ParallelExample.API;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();



app.MapGet("/youtube", () =>
{
    var response = new method().run();

    return response;
});



app.Run();
