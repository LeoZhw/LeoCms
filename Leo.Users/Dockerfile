FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["Leo.Users/Leo.Users.csproj", "Leo.Users/"]
RUN dotnet restore "Leo.Users/Leo.Users.csproj"
COPY . .
WORKDIR "/src/Leo.Users"
RUN dotnet build "Leo.Users.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Leo.Users.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Leo.Users.dll"]