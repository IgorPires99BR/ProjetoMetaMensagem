# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copia os arquivos de projeto (.csproj) respeitando a estrutura de pastas
# Isso permite o cache de camadas do Docker para um restore mais rápido
COPY ["source/ProjetoMetaMensagem.WebAPI/ProjetoMetaMensagem.WebAPI.csproj", "source/ProjetoMetaMensagem.WebAPI/"]
COPY ["source/ProjetoMetaMensagem.Dominio/ProjetoMetaMensagem.Dominio.csproj", "source/ProjetoMetaMensagem.Dominio/"]
COPY ["source/ProjetoMetaMensagem.Servico/ProjetoMetaMensagem.Servico.csproj", "source/ProjetoMetaMensagem.Servico/"]
COPY ["source/ProjetoMetaMensagem.Data/ProjetoMetaMensagem.Data.csproj", "source/ProjetoMetaMensagem.Data/"]

# 2. Restaura as dependências do projeto principal
RUN dotnet restore "source/ProjetoMetaMensagem.WebAPI/ProjetoMetaMensagem.WebAPI.csproj"

# 3. Copia todo o conteúdo da solução para dentro do container
COPY . .

# 4. Muda o diretório de trabalho para onde está o projeto WebAPI
WORKDIR "/src/source/ProjetoMetaMensagem.WebAPI"

# 5. Compila e publica os arquivos em modo Release
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Estágio Final
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Define o fuso horário (opcional, mas útil para logs no Brasil)
ENV TZ=America/Sao_Paulo

ENTRYPOINT ["dotnet", "ProjetoMetaMensagem.WebAPI.dll"]
