using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using OData.Swagger.Services;
using Serilog;
using OData.Models;
using System.Linq;

namespace OData
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            SetUpApiLogging(Configuration);
        }

        private static void SetUpApiLogging(IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            services.AddDbContext<AdventureWorksLT2014Context>(item => item.UseSqlServer(Configuration.GetConnectionString("DatabaseConnection")));

            services.AddOData();

            services.AddSwaggerGen();

            services.AddOdataSwaggerSupport();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            logger.AddSerilog();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseSwagger();
            app.UseSwaggerUI((config) =>
            {
                config.SwaggerEndpoint("/swagger/v1/swagger.json", "OData Web API (AdventureWorks)");

            });

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.EnableDependencyInjection();
                endpoints.Select().Filter().OrderBy().Count().Expand().MaxTop(100);
                endpoints.MapODataRoute("api", "api", GetEdmModel());
            });
        }

        IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            builder.EntitySet<Address>("Addresses");
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<CustomerAddress>("CustomerAddresses")
                .EntityType
                .HasKey(table => new { table.CustomerId, table.AddressId });
            builder.EntitySet<Product>("Products");
            builder.EntitySet<ProductCategory>("ProductCategories");
            builder.EntitySet<ProductDescription>("ProductDescriptions");
            builder.EntitySet<ProductModel>("ProductModels");
            builder.EntitySet<ProductModelProductDescription>("ProductModelProductDescriptions")
                .EntityType
                .HasKey(table => new { table.ProductModelId, table.ProductDescriptionId });
            builder.EntitySet<SalesOrderDetail>("SalesOrderDetails");
            builder.EntitySet<SalesOrderHeader>("SalesOrderHeaders")
                .EntityType
                .HasKey(table => new { table.SalesOrderId });

            return builder.GetEdmModel();
        }
    }
}
