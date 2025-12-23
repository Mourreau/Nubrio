FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.sln ./
COPY Nubrio.Presentation/Nubrio.Presentation.csproj Nubrio.Presentation/
COPY Nubrio.Application/Nubrio.Application.csproj Nubrio.Application/
COPY Nubrio.Infrastructure/Nubrio.Infrastructure.csproj Nubrio.Infrastructure/
COPY Nubrio.Domain/Nubrio.Domain.csproj Nubrio.Domain/
COPY Nubrio.Tests/Nubrio.Tests.csproj Nubrio.Tests/

RUN dotnet restore

COPY . .

RUN dotnet publish Nubrio.Presentation/Nubrio.Presentation.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "Nubrio.Presentation.dll"]