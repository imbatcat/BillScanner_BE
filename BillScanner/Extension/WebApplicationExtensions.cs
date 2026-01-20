namespace BillScanner.Extension
{
    /// <summary>
    /// Extension methods for configuring the WebApplication
    /// </summary>
    public static class WebApplicationExtensions
    {
        /// <summary>
        /// Maps OpenAPI document to Swagger UI
        /// </summary>
        public static void UseOpenApiWithSwagger(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                // Map the OpenAPI endpoint
                app.MapOpenApi();

                // Use Swagger UI to visualize the OpenAPI document
                app.UseSwagger(options =>
                {
                    options.RouteTemplate = "swagger/{documentName}/swagger.json";
                });

                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "BillScanner API v1");
                    options.RoutePrefix = "swagger";
                    options.DocumentTitle = "BillScanner API Documentation";
                    options.EnableDeepLinking();
                    options.DisplayRequestDuration();
                });


            }
        }
    }
}
