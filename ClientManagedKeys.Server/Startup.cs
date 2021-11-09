using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using ClientManagedKeys.Server.Services;
using ClientManagedKeys.Server.Swagger;
using Dalion.HttpMessageSigning;
using Dalion.HttpMessageSigning.Verification;
using Dalion.HttpMessageSigning.Verification.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace ClientManagedKeys.Server
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
            services.AddSingleton<IKeyService, KeyService>();
            services.AddSingleton<IKeyProvider, DemoKeyProvider>();
            
            services
                .AddControllers()
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverterWithAttributeSupport()));
            
            var authCert = new X509Certificate2(typeof(Startup).Assembly.GetResourceAsBytes("DemoAuthCert.der"));

            services
                .AddHttpMessageSignatureVerification(
                    Client.Create(
                        "f011e0fca818d436a75abf878a31accf1e7d80d4",
                        "Aiia 'Client Managed Keys' Test Client",
                        SignatureAlgorithm.CreateForVerification(authCert, HashAlgorithmName.SHA256)
                    )
                )
                .AddAuthentication(SignedHttpRequestDefaults.AuthenticationScheme)
                .AddSignedRequests();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Client Managed Keys",
                    Version = "v1",
                    Description = ReadDocumentationFile("README.md")
                });

                c.TagActionsBy(description => new List<string>()
                {
                    "Key operations"
                });

                c.SchemaFilter<DescribeEnumMemberValues>();
                
                c.AddSecurityDefinition("HTTP Signatures", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Description = ReadDocumentationFile("authentication.md")
                    
                });

                var filePathServer = Path.Combine(AppContext.BaseDirectory, "ClientManagedKeys.Server.xml");
                var filePathModels = Path.Combine(AppContext.BaseDirectory, "ClientManagedKeys.Models.xml");
                
                c.IncludeXmlComments(filePathServer);
                c.IncludeXmlComments(filePathModels);

            });
        }

        private static string ReadDocumentationFile(string resourceName)
        {
            return Encoding.UTF8.GetString(typeof(Startup).Assembly.GetResourceAsBytes(resourceName));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            
            app.UseSwagger();
            app.UseReDoc(c =>
            {
                c.SpecUrl = "/swagger/v1/swagger.json";
                c.RoutePrefix = "docs";
            });
        }
    }
}