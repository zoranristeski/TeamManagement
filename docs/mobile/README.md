# MAUI Mobile Development

The MAUI application uses MVVM, CommunityToolkit.Mvvm, dependency injection, SecureStorage, and typed HttpClient services.

## Target strategy

The existing MAUI project remains on .NET 9 to avoid an unsafe framework migration. The backend uses .NET 10. Android, iOS, Mac Catalyst, and Windows targets retain their existing target frameworks.

## Local configuration

Set the API base URL at runtime using the `ApiBaseUrl` preference. A development default is `https://localhost:7001`; Android emulator deployments must use a reachable host address such as `10.0.2.2` or a deployed HTTPS origin.

## Authentication

The access token is stored using MAUI SecureStorage. It is attached to API requests and removed on sign-out.

## Implemented flows

- Sign in with an issued access token
- Dashboard synchronization
- Secure sign-out
- Bulk attendance recording

## Android build

Install Android workloads, API level 35, and Java 17:

```powershell
dotnet workload restore src/SportsTeamManagement.Mobile/SportsTeamManagement.Mobile.csproj
dotnet build src/SportsTeamManagement.Mobile/SportsTeamManagement.Mobile.csproj -t:InstallAndroidDependencies -f net9.0-android -p:AcceptAndroidSDKLicenses=True
dotnet build src/SportsTeamManagement.Mobile/SportsTeamManagement.Mobile.csproj -f net9.0-android
```
