# Continuous Integration

The GitHub Actions workflow validates the backend, web portal, and MAUI Android target.

MAUI CI requires Java 17 and Android API 35. It must not publish or deploy artifacts without approval.

Azure deployment remains manual. No Azure resources or secrets should be created or committed by CI.
