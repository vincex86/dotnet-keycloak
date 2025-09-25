# Installation

```c#
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

# Configuration

The configuration is available in the appsettings.json file

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "KeycloakIssuer": "https://192.168.1.183/auth/realms/widetech",
  "ClientID": "wide"
}

```