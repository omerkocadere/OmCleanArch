# RabbitMQ/MassTransit Configuration Documentation

## Overview
RabbitMQ and MassTransit services are now configurable and can be enabled/disabled through application settings. This allows for flexible deployment scenarios where messaging infrastructure may not be required.

## Configuration

### appsettings.json
```json
{
  "RabbitMQ": {
    "Enabled": true,
    "Host": "localhost",
    "VirtualHost": "/",
    "Username": "guest",
    "Password": "guest",
    "Port": 5672
  }
}
```

### Configuration Properties

- **Enabled**: `bool` (default: `true`) - Controls whether MassTransit services are registered
- **Host**: `string` (default: `"localhost"`) - RabbitMQ server hostname
- **VirtualHost**: `string` (default: `"/"`) - RabbitMQ virtual host
- **Username**: `string` (default: `"guest"`) - RabbitMQ username
- **Password**: `string` (default: `"guest"`) - RabbitMQ password
- **Port**: `int` (default: `5672`) - RabbitMQ port

## Implementation Details

### Service Registration
- `AddMassTransitServicesConditionally()` method checks the `RabbitMQ.Enabled` configuration
- If disabled, MassTransit services are not registered, avoiding startup errors when RabbitMQ is unavailable
- If enabled, full MassTransit configuration with Entity Framework outbox pattern is registered

### Database Provider Support
- Supports SqlServer, PostgreSQL, and SQLite outbox providers
- Database provider is automatically detected from `DatabaseSettings.Provider`

### Deployment Scenarios
1. **Full messaging enabled**: Set `RabbitMQ.Enabled: true` (default)
2. **Messaging disabled**: Set `RabbitMQ.Enabled: false` for environments without RabbitMQ

## Files Modified
- `src/Infrastructure/Options/RabbitMQOptions.cs` - Added `Enabled` property
- `src/Infrastructure/DependencyInjection.cs` - Added conditional service registration
- `src/Web.Api/appsettings.json` - Added `Enabled: true` 
- `src/Web.Api/appsettings.Development.json` - Added `Enabled: true`

## Benefits
- **Flexible Deployment**: Can deploy without RabbitMQ dependency
- **Environment-Specific**: Different environments can have different messaging configurations
- **Graceful Degradation**: Application starts successfully even when messaging is disabled
- **No Code Changes**: Pure configuration-driven feature toggle