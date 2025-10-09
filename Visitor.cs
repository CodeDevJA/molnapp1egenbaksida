using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using Npgsql;

namespace Visitors;

// ===== VISITOR DATA MODEL =====
// This class holds the structure of the visitor data we receive from the frontend
public class VisitorData
{
    [JsonPropertyName("firstname")]
    public string Firstname { get; set; } = string.Empty;

    [JsonPropertyName("surname")]
    public string Surname { get; set; } = string.Empty;

    [JsonPropertyName("company")]
    public string Company { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;
}

// ===== VISITOR FUNCTION CLASS =====
// This is the main Azure Function that handles visitor registration
public class VisitorFunction
{
    private readonly ILogger<VisitorFunction> _logger;

    // Constructor: Azure Functions automatically provides the logger
    public VisitorFunction(ILogger<VisitorFunction> logger)
    {
        _logger = logger;
    }

    // ===== MAIN FUNCTION: HANDLES BOTH GET AND POST REQUESTS =====
    [Function("Visitor")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        // ===== CORS HEADERS =====
        // Allow requests from any origin (frontend on GitHub Pages)
        var response = new OkObjectResult("Welcome to Azure Functions!");
        response.Headers = new HeaderDictionary();
        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

        // ===== HANDLE PREFLIGHT REQUESTS (CORS) =====
        // Browser sends OPTIONS request first to check if allowed
        if (req.Method == "OPTIONS")
        {
            _logger.LogInformation("Handling CORS preflight request");
            return new OkResult();
        }

        // ===== HANDLE GET REQUESTS =====
        // Useful for testing if the function is working
        if (req.Method == "GET")
        {
            _logger.LogInformation("GET request received - function is working");
            return new OkObjectResult(new
            {
                message = "Visitor Registration API is running!",
                status = "Use POST to register a visitor",
                timestamp = DateTime.UtcNow
            });
        }

        // ===== HANDLE POST REQUESTS =====
        // This is where we process visitor registrations from the frontend
        if (req.Method == "POST")
        {
            try
            {
                _logger.LogInformation("POST request received - processing visitor registration");

                // ===== READ AND PARSE JSON DATA FROM FRONTEND =====
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                _logger.LogInformation("Received data: {RequestBody}", requestBody);

                // Parse JSON into VisitorData object
                var visitorData = JsonSerializer.Deserialize<VisitorData>(requestBody);

                if (visitorData == null)
                {
                    _logger.LogWarning("Invalid JSON data received");
                    return new BadRequestObjectResult(new { error = "Invalid JSON data" });
                }

                // ===== VALIDATE REQUIRED FIELDS =====
                if (string.IsNullOrWhiteSpace(visitorData.Firstname) ||
                    string.IsNullOrWhiteSpace(visitorData.Surname) ||
                    string.IsNullOrWhiteSpace(visitorData.Company) ||
                    string.IsNullOrWhiteSpace(visitorData.Email))
                {
                    _logger.LogWarning("Missing required fields in visitor data");
                    return new BadRequestObjectResult(new { error = "All fields are required" });
                }

                // ===== SAVE TO POSTGRESQL DATABASE =====
                bool saveSuccess = await SaveVisitorToDatabase(visitorData);

                if (!saveSuccess)
                {
                    _logger.LogError("Failed to save visitor to database");
                    return new StatusCodeResult(500);
                }

                // ===== LOG TO APPLICATION INSIGHTS =====
                // This happens automatically through the ILogger
                _logger.LogInformation("Visitor registered successfully: {Firstname} {Surname} from {Company}",
                    visitorData.Firstname, visitorData.Surname, visitorData.Company);

                // ===== SUCCESS RESPONSE =====
                var successResponse = new
                {
                    success = true,
                    message = "Visitor registered successfully",
                    visitor = new
                    {
                        visitorData.Firstname,
                        visitorData.Surname,
                        visitorData.Company,
                        visitorData.Email,
                        timestamp = DateTime.UtcNow
                    }
                };

                _logger.LogInformation("Registration completed successfully for {Email}", visitorData.Email);
                return new OkObjectResult(successResponse);
            }
            catch (Exception ex)
            {
                // ===== ERROR HANDLING =====
                _logger.LogError(ex, "Error processing visitor registration");
                return new StatusCodeResult(500);
            }
        }

        // If we get here, it's a method we don't support
        return new BadRequestObjectResult(new { error = "Method not supported" });
    }

    // ===== DATABASE SAVE METHOD =====
    // This method handles saving visitor data to PostgreSQL
    private async Task<bool> SaveVisitorToDatabase(VisitorData visitorData)
    {
        // Get connection string from environment variables
        string connectionString = Environment.GetEnvironmentVariable("POSTGRESQL_CONNECTION_STRING")
                                ?? throw new InvalidOperationException("PostgreSQL connection string not found");

        // SQL command to insert visitor data
        // Using parameters to prevent SQL injection attacks
        string sql = @"
            INSERT INTO visitors (firstname, surname, company, email, timestamp) 
            VALUES (@firstname, @surname, @company, @email, @timestamp)";

        try
        {
            // Create and open database connection
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            // Create command with parameters
            using var command = new NpgsqlCommand(sql, connection);

            // Add parameters - this is important for security!
            command.Parameters.AddWithValue("@firstname", visitorData.Firstname);
            command.Parameters.AddWithValue("@surname", visitorData.Surname);
            command.Parameters.AddWithValue("@company", visitorData.Company);
            command.Parameters.AddWithValue("@email", visitorData.Email);

            // Use provided timestamp or current time
            var timestamp = string.IsNullOrEmpty(visitorData.Timestamp)
                ? DateTime.UtcNow
                : DateTime.Parse(visitorData.Timestamp);
            command.Parameters.AddWithValue("@timestamp", timestamp);

            // Execute the command
            int rowsAffected = await command.ExecuteNonQueryAsync();

            _logger.LogInformation("Database save: {RowsAffected} rows affected", rowsAffected);

            // Return true if we successfully inserted one row
            return rowsAffected == 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error while saving visitor");
            return false;
        }
    }
}
