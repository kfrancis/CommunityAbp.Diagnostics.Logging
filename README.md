# CommunityAbp.Diagnostics.Logging

[![NuGet](https://img.shields.io/nuget/v/CommunityAbp.Diagnostics.Logging.svg)](https://www.nuget.org/packages/CommunityAbp.Diagnostics.Logging/)
[![Downloads](https://img.shields.io/nuget/dt/CommunityAbp.Diagnostics.Logging.svg)](https://www.nuget.org/packages/CommunityAbp.Diagnostics.Logging/)

Diagnostic logging interceptor for ABP.io application services with selective attribute-based monitoring and retry pattern detection.

## Features

- ✅ **Selective Monitoring** - Use attributes to control which methods are logged
- ✅ **Correlation Tracking** - Automatic correlation ID propagation across service layers
- ✅ **Retry Detection** - Easily identify retry patterns and duplicate operations
- ✅ **Performance Monitoring** - Built-in timing and success tracking
- ✅ **Configurable Detail** - Adjust logging verbosity per method or environment
- ✅ **Zero Configuration** - Works out of the box with sensible defaults

## Installation

```bash
dotnet add package CommunityAbp.Diagnostics.Logging
```

## Quick Start

### 1. Add Module Dependency

```csharp
[DependsOn(typeof(CommunityAbpDiagnosticsLoggingModule))]
public class YourApiHostModule : AbpModule
{
    // Your existing configuration
}
```

### 2. Configure in appsettings.json

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

### 3. Add Attributes to Your Services

```csharp
public class ProductAppService : ApplicationService, IProductAppService
{
    [DiagnosticLogging(Description = "Product List")]
    public async Task<PagedResultDto<ProductDto>> GetListAsync(GetProductListDto input)
    {
        // Your implementation
    }
}
```

## Advanced Configuration

### Environment-Specific Setup

```csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    var hostEnvironment = context.Services.GetHostingEnvironment();
    
    if (hostEnvironment.IsDevelopment())
    {
        context.Services.AddCommunityAbpDiagnosticsLoggingForDevelopment();
    }
    else
    {
        context.Services.AddCommunityAbpDiagnosticsLoggingForProduction();
    }
}
```

### Custom Configuration

```csharp
context.Services.AddCommunityAbpDiagnosticsLogging(options =>
{
    options.EnableDiagnostics = true;
    options.RequireAttribute = true;
    options.LogStackTrace = false;
    options.LogLevel = LogLevel.Information;
});
```

## Usage Examples

### Interface-Level Monitoring
```csharp
[DiagnosticLogging(Description = "Order Management")]
public interface IOrderAppService : IApplicationService
{
    Task<OrderDto> CreateAsync(CreateOrderDto input);
    Task<PagedResultDto<OrderDto>> GetListAsync(GetOrderListDto input);
}
```

### Method-Level Selective Monitoring
```csharp
public class OrderAppService : ApplicationService, IOrderAppService
{
    [DiagnosticLogging(Description = "Order Creation", LogLevel = LogLevel.Information)]
    public async Task<OrderDto> CreateAsync(CreateOrderDto input) { }

    // No logging for simple operations
    public async Task<OrderDto> GetAsync(Guid id) { }
}
```

## Monitoring Retry Patterns

The package is particularly useful for detecting retry patterns (like those caused by resilience frameworks):

```
Normal Operation:
=== APP_SERVICE (Order Creation) START ===
RequestId: abc123, CorrelationId: def456, Duration: 1250ms
=== APP_SERVICE (Order Creation) END ===

Retry Pattern (Problematic):
=== APP_SERVICE (Order Creation) START ===
RequestId: abc123, CorrelationId: def456
=== APP_SERVICE (Order Creation) START ===  // Duplicate with same correlation!
RequestId: xyz789, CorrelationId: def456
```

## Documentation

For detailed documentation, examples, and advanced configuration options, see the [Usage Guide](docs/usage.md).

## License

This project is licensed under the MIT License.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
