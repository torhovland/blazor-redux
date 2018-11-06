using BlazorRedux;
using Microsoft.AspNetCore.Blazor.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Minimal
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddReduxStore<MyState, IAction>(new MyState(), Reducers.RootReducer);
        }

        public void Configure(IBlazorApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
