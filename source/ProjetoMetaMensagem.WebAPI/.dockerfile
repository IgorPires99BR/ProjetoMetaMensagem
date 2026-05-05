# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# 1. Copia os projetos respeitando a sua estrutura de pastas
COPY ["source/ProjetoMetaMensagem.WebAPI/ProjetoMetaMensagem.WebAPI.csproj", "source/ProjetoMetaMensagem.WebAPI/"]
COPY ["source/ProjetoMetaMensagem.Dominio/ProjetoMetaMensagem.Dominio.csproj", "source/ProjetoMetaMensagem.Dominio/"]
COPY ["source/ProjetoMetaMensagem.Data/ProjetoMetaMensagem.Data.csproj", "source/ProjetoMetaMensagem.Data/"]
COPY ["source/ProjetoMetaMensagem.Servico/ProjetoMetaMensagem.Servico.csproj", "source/ProjetoMetaMensagem.Servico/"]

# 2. Restaura as dependências
RUN dotnet restore "source/ProjetoMetaMensagem.WebAPI/ProjetoMetaMensagem.WebAPI.csproj"

# 3. Copia todo o conteúdo e compila
COPY . .
WORKDIR "/src/source/ProjetoMetaMensagem.WebAPI"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Estágio Final - Rodando em .NET 6.0
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# No .NET 6, a porta padrão é a 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ProjetoMetaMensagem.WebAPI.dll"]
