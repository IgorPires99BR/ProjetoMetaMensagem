# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# 1. Copia os projetos respeitando a sua estrutura de pastas
COPY ["Apresentacao/ProjetoMetaMensagem.WebAPI/ProjetoMetaMensagem.WebAPI.csproj", "Apresentacao/ProjetoMetaMensagem.WebAPI/"]
COPY ["Dominio/ProjetoMetaMensagem.Dominio.csproj", "Dominio/"]
COPY ["Infraestrutura/ProjetoMetaMensagem.Infraestrutura.csproj", "Infraestrutura/"]

# 2. Restaura as dependências
RUN dotnet restore "Apresentacao/ProjetoMetaMensagem.WebAPI/ProjetoMetaMensagem.WebAPI.csproj"

# 3. Copia todo o conteúdo e compila
COPY . .
WORKDIR "/src/Apresentacao/ProjetoMetaMensagem.WebAPI"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Estágio Final - Rodando em .NET 6.0
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# No .NET 6, a porta padrão é a 80
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "ProjetoMetaMensagem.WebAPI.dll"]