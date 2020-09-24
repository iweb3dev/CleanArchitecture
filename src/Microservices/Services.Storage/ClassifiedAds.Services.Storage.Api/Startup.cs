using AutoMapper;
using ClassifiedAds.Infrastructure.MessageBrokers;
using ClassifiedAds.Infrastructure.Web.Filters;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ClassifiedAds.Services.Storage
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(StorageModuleServiceCollectionExtensions));

            services.AddControllers(configure =>
            {
                configure.Filters.Add(typeof(GlobalExceptionFilter));
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAnyOrigin", builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });

            services.AddDateTimeProvider();
            services.AddApplicationServices();

            var messageBrokerOptions = new MessageBrokerOptions { Provider = "Fake" };
            var storageOptions = new StorageOptions { Provider = "Fake" };

            services.AddStorageModule(storageOptions, messageBrokerOptions, Configuration.GetConnectionString("ClassifiedAds"));

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                    .AddIdentityServerAuthentication(options =>
                    {
                        options.Authority = Configuration["IdentityServerAuthentication:Authority"];
                        options.ApiName = Configuration["IdentityServerAuthentication.ApiName"];
                        options.RequireHttpsMetadata = Configuration["IdentityServerAuthentication.RequireHttpsMetadata"] == "true";
                    });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors("AllowAnyOrigin");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
