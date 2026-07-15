# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copia os arquivos de projeto (.csproj)
# O Render usará o contexto '.' para encontrar a pasta 'source'
COPY ["source/ProjetoMetaMensagem.WebAPI/ProjetoMetaMensagem.WebAPI.csproj", "source/ProjetoMetaMensagem.WebAPI/"]
COPY ["source/ProjetoMetaMensagem.Dominio/ProjetoMetaMensagem.Dominio.csproj", "source/ProjetoMetaMensagem.Dominio/"]
COPY ["source/ProjetoMetaMensagem.Data/ProjetoMetaMensagem.Data.csproj", "source/ProjetoMetaMensagem.Data/"]
COPY ["source/ProjetoMetaMensagem.Servico/ProjetoMetaMensagem.Servico.csproj", "source/ProjetoMetaMensagem.Servico/"]

# 2. Restaura as dependências
RUN dotnet restore "source/ProjetoMetaMensagem.WebAPI/ProjetoMetaMensagem.WebAPI.csproj"

# 3. Copia todo o conteúdo da raiz (Contexto '.')
COPY . .

# 4. Define o diretório de trabalho para a publicação
WORKDIR "/src/source/ProjetoMetaMensagem.WebAPI"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Estágio Final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# No Render, é recomendável usar a porta 8080 ou a que eles fornecerem via variável PORT
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ProjetoMetaMensagem.WebAPI.dll"]