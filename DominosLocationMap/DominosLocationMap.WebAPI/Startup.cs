using AutoMapper;
using DominosLocationMap.Business.Abstract;
using DominosLocationMap.Business.Managers;
using DominosLocationMap.Business.Queues.RabbitMq;
using DominosLocationMap.Business.Queues.Redis;
using DominosLocationMap.Core.CrossCutting.Caching;
using DominosLocationMap.Core.CrossCutting.Caching.Redis;
using DominosLocationMap.Core.CrossCutting.Logging;
using DominosLocationMap.Core.CrossCutting.Logging.NLog;
using DominosLocationMap.Core.Entities.Options;
using DominosLocationMap.Core.RabbitMQ;
using DominosLocationMap.Core.Utilities.Helpers.DataConvertHelper;
using DominosLocationMap.DataAccess.Abstract;
using DominosLocationMap.DataAccess.Concrete.EntityFramework;
using DominosLocationMap.DataAccess.Concrete.EntityFramework.DatbaseContext;
using DominosLocationMap.WebAPI.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;

namespace DominosLocationMap.WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddScoped<ILocationInfoService, LocationInfoManager>();
            services.AddScoped<ILocationInfoDal, EfLocationInfoDal>();
            services.AddDbContext<DominosLocationMapDbContext>(options => options.UseSqlServer(Configuration["ConnectionStrings:DominosDb"]));

            services.Configure<LocationOptions>(Configuration.GetSection("LocationOptions"));
            services.Configure<RedisOptions>(Configuration.GetSection("RedisOptions"));

            var redisOptions = Configuration.GetSection("RedisOptions").Get<RedisOptions>();
            services.AddMemoryCache();
            services.AddDistributedRedisCache(options =>
            {
                options.InstanceName = redisOptions.InstanceName;
                options.Configuration = redisOptions.Configuration;
            });

            //Log Configurations
            services.AddSingleton<ILogManager, NLogManager>();
            services.AddNLogConfig("/nlog.config");

            // caching
            services.AddSingleton<ICacheManager, RedisCacheManager>();

            // dto to entity model mapping
            services.AddAutoMapper(typeof(Startup));

            // AddDependencyResolvers islemi db operation objects

            services.AddScoped<IRabbitMqConfiguration, RabbitMqConfiguration>();
            services.AddScoped<IRabbitMqService, RabbitMqService>();
            services.AddScoped<IPublisherService, PublisherManager>();
            services.AddScoped<IObjectDataConverter, ObjectDataConverterManager>();

            services.AddScoped<IConsumerService, DataBaseConsumerManager>();
            services.AddScoped<IConsumerService, FileOperationConsumerManager>();

            #region Swagger options

            services.AddSwaggerGen((options) =>
            {
                options.SwaggerGeneratorOptions.IgnoreObsoleteActions = true;
                options.SwaggerDoc(Configuration.GetSection("Swagger:SwaggerName").Value, new OpenApiInfo()
                {
                    Version = Configuration.GetSection("Swagger:SwaggerDoc:Version").Value,
                    Title = Configuration.GetSection("Swagger:SwaggerDoc:Title").Value,
                    Description = Configuration.GetSection("Swagger:SwaggerDoc:Description").Value,
                    TermsOfService = new Uri(Configuration.GetSection("Swagger:SwaggerDoc:TermsOfService").Value),
                    Contact = new OpenApiContact
                    {
                        Name = Configuration.GetSection("Swagger:SwaggerDoc:Contact:Name").Value,
                        Email = string.Empty,
                        Url = new Uri(Configuration.GetSection("Swagger:SwaggerDoc:Contact:Url").Value),
                    },
                    License = new OpenApiLicense
                    {
                        Name = Configuration.GetSection("Swagger:SwaggerDoc:License:Name").Value,
                        Url = new Uri(Configuration.GetSection("Swagger:SwaggerDoc:License:Url").Value),
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            #endregion Swagger options
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(url: String.Format(Configuration.GetSection("Swagger:UseSwaggerUI:SwaggerEndpoint").Value, Configuration.GetSection("Swagger:SwaggerName").Value),
                    name: "Version CoreSwaggerWebAPI-1");
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}