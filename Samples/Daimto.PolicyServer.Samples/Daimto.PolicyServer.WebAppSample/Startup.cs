using Daimto.PolicyServer.WebAppSample.SchoolPolicy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Daimto.PolicyServer.WebAppSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            //Adds MVC services to the specified IServiceCollection
            services.AddMvc(options =>
            {
                // Creates a new instance of AuthorizationPolicyBuilder
                // It sets up a default authorization policy for the application
                // in this case, authenticated users are required (besides controllers/actions that have [AllowAnonymous]
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()  // Adds DenyAnonymousAuthorizationRequirement to the current instance.
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });

            // this sets up authentication - for this demo we simply use a local cookie
            // typically authentication would be done using an external provider
            services.AddAuthentication("Cookies")
                .AddCookie("Cookies");

            // this sets up the PolicyServer client library and policy provider - configuration is loaded from appsettings.json
            services.AddPolicyServerClient(Configuration.GetSection("Policy"))
                .AddAuthorizationPermissionPolicies();

            // Add handler for our custom requirements
            services.AddTransient<IAuthorizationHandler, PrepareOrderRequirmentHandler>();
            services.AddTransient<IAuthorizationHandler, OrderMealRequirmentHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            //Adds the AuthenticationMiddleware to the specified IApplicationBuilder, which enables authentication capabilities.
            app.UseAuthentication();

            // Add the policy server claims transformation middleware to the pipeline.
            // This middleware will turn application roles and permissions into claims and add them to the current user
            // Usage: [Authorize(Roles="foo")] and IsInRole functionality
            app.UsePolicyServerClaimsTransformation();

            // Enables static file serving for the current request path
            app.UseStaticFiles();

            // Adds MVC to the IApplicationBuilder request execution pipeline
            // with a default route named 'default' and the following template:
            // '{controller=Home}/{action=Index}/{id?}'.
            app.UseMvcWithDefaultRoute();
        }
    }
}
