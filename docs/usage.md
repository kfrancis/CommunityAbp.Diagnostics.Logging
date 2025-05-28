# CommunityAbp.Diagnostics.Logging - Usage Guide

## Table of Contents

- [Installation and Setup](#installation-and-setup)
- [Basic Usage](#basic-usage)
- [Configuration Options](#configuration-options)
- [Attribute Reference](#attribute-reference)
- [Usage Patterns](#usage-patterns)
- [Monitoring and Debugging](#monitoring-and-debugging)
- [Performance Considerations](#performance-considerations)
- [Troubleshooting](#troubleshooting)
- [Best Practices](#best-practices)
- [Advanced Scenarios](#advanced-scenarios)

---

## Installation and Setup

### 1. Install the Package

```bash
dotnet add package CommunityAbp.Diagnostics.Logging
```

### 2. Add Module Dependency

Add the module dependency to your API Host or Web project:

```csharp
[DependsOn(
    typeof(CommunityAbpDiagnosticsLoggingModule),
    typeof(YourApplicationModule),
    // ... other dependencies
)]
public class YourApiHostModule : AbpModule
{
    // Your existing configuration
}
```

### 3. Basic Configuration

Add configuration to your `appsettings.json`:

```json
{
  "CommunityAbp": {
    "Diagnostics": {
      "Logging": {
        "EnableDiagnostics": true,
        "RequireAttribute": true,
        "LogLevel": "Warning"
      }
    }
  }
}
```

### 4. Apply Attributes

Start monitoring your application services:

```csharp
public class ProductAppService : ApplicationService, IProductAppService
{
    [DiagnosticLogging(Description = "Product Operations")]
    public async Task<PagedResultDto<ProductDto>> GetListAsync(GetProductListDto input)
    {
        // Your implementation
        return new PagedResultDto<ProductDto>();
    }
}
```

---

## Basic Usage

### Zero Configuration Approach

The simplest way to get started:

```csharp
// 1. Add module dependency (as shown above)

// 2. Enable in appsettings.json
{
  "CommunityAbp": {
    "Diagnostics": {
      "Logging": {
        "EnableDiagnostics": true
      }
    }
  }
}

// 3. Add attributes to methods you want to monitor
[DiagnosticLogging]
public async Task<OrderDto> CreateOrderAsync(CreateOrderDto input)
{
    // Your implementation
}
```

### Understanding the Log Output

When you call a monitored method, you'll see output like this:

```
=== APP_SERVICE (Product Operations) START ===
RequestId: a1b2c3d4, CorrelationId: e5f6g7h8, Time: 14:30:15.123
Method: ProductAppService.GetListAsync, Hash: 123456789
Is Proxy: True
UOW ID: uow_abc123, UOW IsActive: True
Arguments: input=Skip=0, Max=20, Sort=Name
=== APP_SERVICE (Product Operations) END ===
RequestId: a1b2c3d4, Duration: 1250ms, Success: True
```

**Key Information:**
- **RequestId**: Unique identifier for this specific method call
- **CorrelationId**: Tracks related operations across service boundaries
- **Duration**: How long the method took to execute
- **Arguments**: Method parameters (safely formatted)
- **UOW Info**: Unit of Work details for database operations

---

## Configuration Options

### Configuration Methods

#### 1. appsettings.json Configuration (Recommended)

```json
{
  "CommunityAbp": {
    "Diagnostics": {
      "Logging": {
        "EnableDiagnostics": true,
        "RequireAttribute": true,
        "LogStackTrace": false,
        "LogUnitOfWork": true,
        "LogArguments": true,
        "StackTraceDepth": 10,
        "LogLevel": "Warning"
      }
    }
  }
}
```

#### 2. Programmatic Configuration

```csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    context.Services.AddCommunityAbpDiagnosticsLogging(options =>
    {
        options.EnableDiagnostics = true;
        options.RequireAttribute = true;
        options.LogLevel = LogLevel.Information;
    });
}
```

#### 3. Environment-Specific Configuration

```csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    var hostEnvironment = context.Services.GetHostingEnvironment();
    
    if (hostEnvironment.IsDevelopment())
    {
        // Enhanced logging for development
        context.Services.AddCommunityAbpDiagnosticsLoggingForDevelopment();
    }
    else if (hostEnvironment.IsProduction())
    {
        // Minimal overhead for production
        context.Services.AddCommunityAbpDiagnosticsLoggingForProduction();
    }
    else
    {
        // Custom configuration for staging/testing
        context.Services.AddCommunityAbpDiagnosticsLogging(options =>
        {
            options.EnableDiagnostics = true;
            options.RequireAttribute = true;
            options.LogStackTrace = false;
            options.LogLevel = LogLevel.Information;
        });
    }
}
```

### Configuration Options Reference

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `EnableDiagnostics` | `bool` | `true` | Master switch to enable/disable all diagnostic logging |
| `RequireAttribute` | `bool` | `false` | When `true`, only methods with `[DiagnosticLogging]` are logged |
| `LogStackTrace` | `bool` | `false` | Include call stack information in logs |
| `LogUnitOfWork` | `bool` | `true` | Include Unit of Work information |
| `LogArguments` | `bool` | `true` | Include method arguments in logs |
| `StackTraceDepth` | `int` | `10` | Number of stack frames to include |
| `LogLevel` | `LogLevel` | `Warning` | Default log level for diagnostic messages |

---

## Attribute Reference

### `[DiagnosticLogging]` Attribute

The `DiagnosticLoggingAttribute` can be applied to:
- **Methods** - Monitor specific operations
- **Classes** - Monitor all methods in the class
- **Interfaces** - Monitor all implementing methods

#### Attribute Properties

```csharp
[DiagnosticLogging(
    Enabled = true,                    // Enable/disable for this method
    Description = "Custom description", // Custom label in logs
    LogStackTrace = false,             // Include stack trace
    LogUnitOfWork = true,              // Include UoW information
    LogArguments = true,               // Include method arguments
    LogLevel = LogLevel.Information,   // Custom log level
    StackTraceDepth = 10              // Stack trace depth
)]
```

#### Basic Examples

```csharp
// Simple monitoring
[DiagnosticLogging]
public async Task<OrderDto> GetOrderAsync(Guid id) { }

// With description
[DiagnosticLogging(Description = "Order Creation")]
public async Task<OrderDto> CreateOrderAsync(CreateOrderDto input) { }

// Enhanced debugging
[DiagnosticLogging(
    Description = "Critical Payment Processing",
    LogStackTrace = true,
    LogLevel = LogLevel.Error
)]
public async Task ProcessPaymentAsync(PaymentDto payment) { }

// Disable for specific method
[DiagnosticLogging(Enabled = false)]
public async Task GetCachedDataAsync() { }
```

---

## Usage Patterns

### Pattern 1: Interface-Level Monitoring

Apply to the entire service interface:

```csharp
[DiagnosticLogging(Description = "Order Management Service")]
public interface IOrderAppService : IApplicationService
{
    Task<OrderDto> CreateAsync(CreateOrderDto input);
    Task<PagedResultDto<OrderDto>> GetListAsync(GetOrderListDto input);
    Task UpdateAsync(Guid id, UpdateOrderDto input);
    Task DeleteAsync(Guid id);
}

// All methods in the implementing class will be monitored
public class OrderAppService : ApplicationService, IOrderAppService
{
    // All methods automatically get diagnostic logging
    public async Task<OrderDto> CreateAsync(CreateOrderDto input) { }
    public async Task<PagedResultDto<OrderDto>> GetListAsync(GetOrderListDto input) { }
    public async Task UpdateAsync(Guid id, UpdateOrderDto input) { }
    public async Task DeleteAsync(Guid id) { }
}
```

### Pattern 2: Class-Level with Method Overrides

Apply base settings to the class, override for specific methods:

```csharp
[DiagnosticLogging(
    Description = "Product Service",
    LogLevel = LogLevel.Information
)]
public class ProductAppService : ApplicationService, IProductAppService
{
    // Inherits class-level settings
    public async Task<ProductDto> GetAsync(Guid id) { }

    // Override with enhanced logging for critical operations
    [DiagnosticLogging(
        Description = "Product Creation",
        LogStackTrace = true,
        LogLevel = LogLevel.Warning
    )]
    public async Task<ProductDto> CreateAsync(CreateProductDto input) { }

    // Disable logging for high-frequency operations
    [DiagnosticLogging(Enabled = false)]
    public async Task<bool> IsSkuAvailableAsync(string sku) { }
}
```

### Pattern 3: Selective Method Monitoring

Monitor only specific critical operations:

```csharp
public class PaymentAppService : ApplicationService, IPaymentAppService
{
    // Monitor payment processing (critical operation)
    [DiagnosticLogging(
        Description = "Payment Processing",
        LogStackTrace = true,
        LogLevel = LogLevel.Error
    )]
    public async Task<PaymentResultDto> ProcessPaymentAsync(ProcessPaymentDto input) { }

    // Monitor refunds (critical operation)
    [DiagnosticLogging(Description = "Payment Refund")]
    public async Task<RefundResultDto> RefundPaymentAsync(RefundPaymentDto input) { }

    // No monitoring for simple lookups (no attribute)
    public async Task<PaymentDto> GetPaymentAsync(Guid id) { }
    public async Task<bool> IsPaymentValidAsync(string token) { }
}
```

### Pattern 4: Environment-Specific Monitoring

Different monitoring strategies per environment:

```csharp
public class UserAppService : ApplicationService, IUserAppService
{
#if DEBUG
    [DiagnosticLogging(
        Description = "User Operations (Debug)",
        LogStackTrace = true,
        LogArguments = true,
        LogLevel = LogLevel.Debug
    )]
#else
    [DiagnosticLogging(
        Description = "User Operations",
        LogArguments = false, // Don't log sensitive user data in production
        LogLevel = LogLevel.Warning
    )]
#endif
    public async Task<UserDto> CreateUserAsync(CreateUserDto input) { }
}
```

---

## Monitoring and Debugging

### Detecting Retry Patterns

The diagnostic logging is particularly useful for detecting retry patterns caused by resilience frameworks like Aspire:

#### Normal Operation Pattern
```
14:30:15.123 - CorrelationId: abc123, RequestId: req001 - START
14:30:16.373 - CorrelationId: abc123, RequestId: req001 - END (Duration: 1250ms)
```

#### Retry Pattern (Problematic)
```
14:30:15.123 - CorrelationId: abc123, RequestId: req001 - START
14:30:17.123 - CorrelationId: abc123, RequestId: req002 - START  // Same correlation, different request!
14:30:16.373 - CorrelationId: abc123, RequestId: req001 - END (Duration: 1250ms)
14:30:18.373 - CorrelationId: abc123, RequestId: req002 - END (Duration: 1250ms)
```

### Log Analysis Queries

#### Using grep/awk (Linux/Mac)
```bash
# Find operations with multiple starts (indicates retries)
grep "APP_SERVICE.*START" app.log | \
cut -d',' -f2 | \
sort | uniq -c | \
awk '$1 > 1 {print "Retry detected for CorrelationId: " $2}'

# Find slow operations (>30 seconds)
grep "Duration:" app.log | \
awk -F'Duration: |ms' '$2 > 30000 {print $0}'

# Count operations by service
grep "APP_SERVICE.*START" app.log | \
grep -o "Method: [^,]*" | \
sort | uniq -c | sort -nr
```

#### Using PowerShell (Windows)
```powershell
# Find retry patterns
Get-Content app.log | 
Where-Object { $_ -match "APP_SERVICE.*START" } | 
ForEach-Object { ($_ -split ',')[1] } | 
Group-Object | 
Where-Object { $_.Count -gt 1 } | 
ForEach-Object { Write-Host "Retry detected for $($_.Name)" }

# Find slow operations
Get-Content app.log | 
Where-Object { $_ -match "Duration: (\d+)ms" } | 
Where-Object { [int]($Matches[1]) -gt 30000 }
```

### Correlation Tracking

The diagnostic logging automatically propagates correlation IDs across service boundaries:

```csharp
// Page Handler sets correlation ID
public async Task<JsonResult> OnPostLoadDataAsync()
{
    // Correlation ID is automatically set in HttpContext
    var result = await _productAppService.GetListAsync(input);
    return new JsonResult(result);
}

// Application Service receives and logs the same correlation ID
[DiagnosticLogging(Description = "Product List")]
public async Task<PagedResultDto<ProductDto>> GetListAsync(GetProductListDto input)
{
    // The same correlation ID from the page handler is logged here
    return await GetProductsFromRepository(input);
}
```

---

## Performance Considerations

### Minimal Performance Impact

The diagnostic logging is designed to have minimal performance impact:

| Scenario | Overhead |
|----------|----------|
| Method with `[DiagnosticLogging]` | ~0.1-0.5ms per call |
| Method without attribute (RequireAttribute=true) | ~0.01ms per call |
| Disabled globally | ~0.001ms per call |

### Optimization Strategies

#### 1. Use RequireAttribute in Production
```json
{
  "CommunityAbp": {
    "Diagnostics": {
      "Logging": {
        "RequireAttribute": true  // Only log attributed methods
      }
    }
  }
}
```

#### 2. Disable Expensive Features in Production
```json
{
  "CommunityAbp": {
    "Diagnostics": {
      "Logging": {
        "LogStackTrace": false,   // Expensive to generate
        "LogArguments": false,    // Can contain sensitive data
        "StackTraceDepth": 5      // Reduce if stack trace is enabled
      }
    }
  }
}
```

#### 3. Use Appropriate Log Levels
```csharp
// High-frequency operations
[DiagnosticLogging(LogLevel = LogLevel.Debug)]  // Filtered out in production

// Critical operations  
[DiagnosticLogging(LogLevel = LogLevel.Warning)] // Always logged

// Error scenarios
[DiagnosticLogging(LogLevel = LogLevel.Error)]   // High priority logging
```

#### 4. Selective Monitoring for High-Volume Services
```csharp
public class HighVolumeAppService : ApplicationService
{
    // Monitor only critical operations
    [DiagnosticLogging(Description = "Critical Operation")]
    public async Task CriticalOperationAsync() { }

    // No monitoring for high-frequency simple operations
    public async Task GetCachedDataAsync() { }
    public async Task ValidateInputAsync() { }
}
```

---

## Troubleshooting

### Common Issues

#### Issue: No Diagnostic Logs Appearing

**Possible Causes:**
1. `EnableDiagnostics` is set to `false`
2. `RequireAttribute` is `true` but no methods have `[DiagnosticLogging]`
3. Log level filtering is excluding diagnostic messages
4. Module not properly registered

**Solutions:**
```csharp
// 1. Verify module registration
[DependsOn(typeof(CommunityAbpDiagnosticsLoggingModule))]

// 2. Check configuration
{
  "CommunityAbp": {
    "Diagnostics": {
      "Logging": {
        "EnableDiagnostics": true,
        "RequireAttribute": false  // Temporarily set to false for testing
      }
    }
  }
}

// 3. Verify log level configuration
{
  "Logging": {
    "LogLevel": {
      "CommunityAbp.Diagnostics.Logging": "Debug"  // Enable all diagnostic logs
    }
  }
}

// 4. Add test method
[DiagnosticLogging(Description = "Test Method")]
public async Task TestDiagnosticLoggingAsync()
{
    await Task.Delay(100);
    Logger.LogWarning("Test method executed");
}
```

#### Issue: Too Many Logs in Production

**Solutions:**
```json
{
  "CommunityAbp": {
    "Diagnostics": {
      "Logging": {
        "RequireAttribute": true,    // Only attributed methods
        "LogArguments": false,       // Reduce log size
        "LogStackTrace": false,      // Reduce log size
        "LogLevel": "Warning"        // Higher threshold
      }
    }
  }
}
```

#### Issue: Sensitive Data in Logs

**Solutions:**
```csharp
// Disable argument logging for sensitive operations
[DiagnosticLogging(
    Description = "User Authentication",
    LogArguments = false  // Don't log passwords, tokens, etc.
)]
public async Task AuthenticateUserAsync(AuthenticationDto input) { }

// Or disable globally
{
  "CommunityAbp": {
    "Diagnostics": {
      "Logging": {
        "LogArguments": false
      }
    }
  }
}
```

#### Issue: Performance Impact in High-Volume Scenarios

**Solutions:**
```csharp
// Use sampling for high-volume operations
private static int _callCounter = 0;

[DiagnosticLogging(Description = "High Volume Operation")]
public async Task HighVolumeOperationAsync()
{
    // Custom sampling logic
    if (Interlocked.Increment(ref _callCounter) % 100 == 0)
    {
        Logger.LogInformation("Processed {CallCount} operations", _callCounter);
    }
    
    // Your implementation
}

// Or use conditional attributes
#if DEBUG
[DiagnosticLogging(Description = "Debug Only")]
#endif
public async Task DebugOnlyMonitoringAsync() { }
```

### Debugging Steps

1. **Verify Basic Setup**
   ```csharp
   // Add simple test method
   [DiagnosticLogging(Description = "Setup Test")]
   public async Task TestSetupAsync()
   {
       Logger.LogWarning("Diagnostic test executed");
       await Task.Delay(1000);
   }
   ```

2. **Check Configuration Binding**
   ```csharp
   public override void ConfigureServices(ServiceConfigurationContext context)
   {
       // Log configuration to verify binding
       var config = context.Services.GetConfiguration();
       var section = config.GetSection("CommunityAbp:Diagnostics:Logging");
       Console.WriteLine($"EnableDiagnostics: {section["EnableDiagnostics"]}");
   }
   ```

3. **Verify Module Loading**
   ```csharp
   public override void OnApplicationInitialization(ApplicationInitializationContext context)
   {
       var logger = context.ServiceProvider.GetRequiredService<ILogger<YourModule>>();
       logger.LogInformation("CommunityAbp.Diagnostics.Logging module loaded");
   }
   ```

---

## Best Practices

### Development Environment

```csharp
// Enhanced logging for development
context.Services.AddCommunityAbpDiagnosticsLoggingForDevelopment();

// Or custom development configuration
context.Services.AddCommunityAbpDiagnosticsLogging(options =>
{
    options.EnableDiagnostics = true;
    options.RequireAttribute = false;  // Log all methods
    options.LogStackTrace = true;      // Detailed debugging
    options.LogArguments = true;       // See all parameters
    options.LogLevel = LogLevel.Debug; // Verbose logging
});
```

### Production Environment

```csharp
// Minimal overhead for production
context.Services.AddCommunityAbpDiagnosticsLoggingForProduction();

// Or custom production configuration
context.Services.AddCommunityAbpDiagnosticsLogging(options =>
{
    options.EnableDiagnostics = true;
    options.RequireAttribute = true;   // Only critical methods
    options.LogStackTrace = false;     // Reduce overhead
    options.LogArguments = false;      // Avoid sensitive data
    options.LogLevel = LogLevel.Warning; // Important events only
});
```

### Monitoring Strategy

1. **Critical Operations**: Always monitor operations that handle payments, user data, or business-critical workflows
2. **Performance Bottlenecks**: Monitor operations identified as slow or problematic
3. **Integration Points**: Monitor methods that interact with external services
4. **Retry-Prone Operations**: Monitor operations that might be affected by resilience patterns

```csharp
public class RecommendedMonitoringAppService : ApplicationService
{
    // Critical business operation
    [DiagnosticLogging(Description = "Order Processing", LogLevel = LogLevel.Warning)]
    public async Task ProcessOrderAsync(ProcessOrderDto input) { }

    // External service integration
    [DiagnosticLogging(Description = "Payment Gateway", LogLevel = LogLevel.Information)]
    public async Task ChargePaymentAsync(PaymentDto payment) { }

    // Performance-sensitive operation
    [DiagnosticLogging(Description = "Report Generation")]
    public async Task GenerateReportAsync(ReportRequestDto request) { }

    // Simple CRUD - no monitoring needed
    public async Task<ProductDto> GetProductAsync(Guid id) { }
}
```

### Log Management

1. **Use Structured Logging**: The diagnostic logs are structured and can be easily parsed
2. **Correlation Tracking**: Use correlation IDs to trace operations across services
3. **Alerting**: Set up alerts for retry patterns or performance degradation
4. **Retention**: Configure appropriate log retention based on volume and requirements

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "CommunityAbp.Diagnostics.Logging": "Warning"
    }
  },
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/diagnostic-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

---

## Advanced Scenarios

### Custom Argument Formatting

For complex objects, you might want custom argument formatting:

```csharp
public class CustomDto
{
    public string SensitiveData { get; set; }
    public string PublicData { get; set; }
    
    public override string ToString()
    {
        // Custom formatting that excludes sensitive data
        return $"PublicData={PublicData}, SensitiveData=[REDACTED]";
    }
}

[DiagnosticLogging(Description = "Custom Formatting")]
public async Task ProcessCustomDataAsync(CustomDto input)
{
    // The ToString() method will be used for logging
}
```

### Conditional Monitoring

```csharp
public class ConditionalMonitoringAppService : ApplicationService
{
    private readonly IConfiguration _configuration;
    
    public ConditionalMonitoringAppService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task ConditionalOperationAsync()
    {
        var enableDetailedLogging = _configuration.GetValue<bool>("EnableDetailedLogging");
        
        if (enableDetailedLogging)
        {
            Logger.LogInformation("Detailed operation starting...");
        }
        
        // Your implementation
    }
}
```

### Integration with Application Performance Monitoring

```csharp
public class APMIntegratedAppService : ApplicationService
{
    [DiagnosticLogging(Description = "APM Monitored Operation")]
    public async Task MonitoredOperationAsync()
    {
        using var activity = DiagnosticSource.StartActivity("CustomOperation", null);
        
        try
        {
            // Your implementation
            await PerformBusinessLogicAsync();
            
            activity?.AddTag("operation.success", "true");
        }
        catch (Exception ex)
        {
            activity?.AddTag("operation.success", "false");
            activity?.AddTag("error.message", ex.Message);
            throw;
        }
    }
}
```

### Batch Operation Monitoring

```csharp
public class BatchOperationAppService : ApplicationService
{
    [DiagnosticLogging(Description = "Batch Processing")]
    public async Task ProcessBatchAsync(List<BatchItemDto> items)
    {
        Logger.LogInformation("Processing batch of {ItemCount} items", items.Count);
        
        var successCount = 0;
        var errorCount = 0;
        
        foreach (var item in items)
        {
            try
            {
                await ProcessSingleItemAsync(item);
                successCount++;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to process item {ItemId}", item.Id);
                errorCount++;
            }
        }
        
        Logger.LogInformation(
            "Batch processing completed. Success: {SuccessCount}, Errors: {ErrorCount}",
            successCount, errorCount);
    }
}
```

---

This comprehensive usage guide should help users of the CommunityAbp.Diagnostics.Logging package understand how to effectively implement and configure diagnostic logging in their ABP.io applications. The guide covers everything from basic setup to advanced scenarios and troubleshooting, making it a valuable resource for both new users and those implementing complex monitoring requirements.
