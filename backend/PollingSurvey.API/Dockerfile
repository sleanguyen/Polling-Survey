# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY PollingSurvey.API/*.csproj PollingSurvey.API/
COPY PollingSurvey.Application/*.csproj PollingSurvey.Application/
COPY PollingSurvey.Domain/*.csproj PollingSurvey.Domain/
COPY PollingSurvey.Infrastructure/*.csproj PollingSurvey.Infrastructure/

RUN dotnet restore PollingSurvey.API/PollingSurvey.API.csproj

COPY PollingSurvey.API/ PollingSurvey.API/
COPY PollingSurvey.Application/ PollingSurvey.Application/
COPY PollingSurvey.Domain/ PollingSurvey.Domain/
COPY PollingSurvey.Infrastructure/ PollingSurvey.Infrastructure/

RUN dotnet publish PollingSurvey.API/PollingSurvey.API.csproj -c Release -o /app/publish

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet","PollingSurvey.API.dll"]