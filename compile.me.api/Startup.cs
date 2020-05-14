using compile.me.api.Services;
using compile.me.api.Services.Compiler;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace compile.me.api
{
    public class Startup
    {
        /// <summary>
        /// The env
        /// </summary>
        private readonly IHostEnvironment _environment;

        /// <summary>
        /// The configuration
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup" /> class.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <param name="configuration">The configuration.</param>
        public Startup(IHostEnvironment environment, IConfiguration configuration)
        {
            this._environment = environment;
            this._configuration = configuration;
        }

        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="services">The services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            var root = this._configuration.GetSection("configuration");

            services.AddCors(options =>
            {
                options.AddPolicy("allowOrigins",
                    builder =>
                    {
                        builder.AllowAnyOrigin().AllowAnyHeader().AllowCredentials();
                        builder.WithOrigins("http://localhost:3000", "http://web:3000", "http://twitch:8081");
                    });
            });

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.None;
                options.UseCamelCasing(true);
            });


            var publisher = root.GetSection("compiler").GetValue<string>("publisher");
           
            services.AddSingleton(new CompilerPublisher(publisher));
            services.AddHostedService<WorkerManagementService>();
            services.AddHostedService<CompilerService>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure
        /// the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseCors("allowOrigins");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}