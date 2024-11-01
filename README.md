# Amazon Simple Email Service Bounce/Complaint Sample

Generate certificate and configure local machine:

PC:

```PowerShell
dotnet dev-certs https --clean
dotnet dev-certs https -ep "$env:USERPROFILE\.aspnet\https\aspnetapp.pfx" -p Passw0rd
dotnet dev-certs https --trust
```

MAC:

```
dotnet dev-certs https --clean
dotnet dev-certs https -ep ~/.aspnet/https/aspnetapp.pfx -p Passw0rd
dotnet dev-certs https --trust
```

Read more here:
https://learn.microsoft.com/en-us/aspnet/core/security/docker-compose-https?view=aspnetcore-6.0#starting-a-container-with-https-support-using-docker-compose

Use the `docker-compose.yml` file to startup the service and Postgres. Or run `docker compose up --build`.    
Navigate to `https://localhost:5001/swagger` to start the application.

