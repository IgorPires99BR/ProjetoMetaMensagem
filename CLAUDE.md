# ProjetoMetaMensagem (Contact Solution) — guia para agentes

Backend .NET (ASP.NET Core, Dapper, SQL Server) de uma plataforma multi-tenant de
automação de WhatsApp Business (campanhas, chatbot de flows, chat ao vivo, CRM
pipeline, templates Meta). Frontend em `../AngularContact` (mesma pasta pai
`ContactSolution/`, referenciado pelo `.sln` via caminho relativo `../AngularContact`).

## Ambiente local

- .NET SDK 8 + runtime 6 via `~/.dotnet` (PATH já configurado no `.zprofile`/`.zshrc`).
- Banco: **não use a imagem `mcr.microsoft.com/mssql/server` em Apple Silicon** — trava
  sob emulação QEMU (erro de mapeamento de memória). Use `mcr.microsoft.com/azure-sql-edge`
  (nativa ARM64), rodando via Colima/Docker:
  ```
  docker run -d --name sql_server --platform linux/arm64 -p 1433:1433 \
    -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=SuaSenhaForte123!" \
    mcr.microsoft.com/azure-sql-edge:latest
  ```
- `sqlcmd` (via `brew install sqlcmd`) tem um bug de TLS com o certificado autoassinado
  do container (`x509: negative serial number`) — não funciona para conectar localmente.
  Use `Microsoft.Data.SqlClient` (via um console app `dotnet run` descartável, ou a própria
  API) para rodar SQL contra o banco local.
- `appsettings.Development.json` é gitignored e não existe no repo — precisa ser criado
  localmente com `ConnectionStrings:ContactSolutionDB`, `JwtSettings:SecretKey` (qualquer
  string aleatória forte), e as demais sections (`ApiWhatsappConnectionConfiguration`,
  `GmailConfig`, `GeminiConfiguration`) com placeholders óbvios se não tiver as credenciais
  reais — a API sobe e funciona sem elas, só as integrações reais (Meta/Gmail/Gemini) não.
- Scripts SQL em `BD/` são numerados e devem rodar em ordem (00 → 22, depois `Y01`/`Y02`).
  Pule `006_Create_MensagemRecebida.sql` — é uma versão antiga/duplicada, superada pelo
  `14 - CREATE_TABLE_MensagemRecebida.sql` (rodar os dois quebra, o 14 não tem `IF NOT EXISTS`).

## Padrões de código (siga ao adicionar/alterar algo)

- **Mediator manual, não MediatR**: todo caso de uso é `Command` + `Handler` +
  `Result` + `Validator` em `Dominio/UseCases/<Área>/<Ação>/`, registrado manualmente em
  `Program.cs` como `AddScoped<IRequestHandler<TCommand, Response<TResult>>, THandler>()`.
  **Se você criar um handler novo e esquecer de registrar em `Program.cs`, ele compila
  normalmente e só falha em runtime com 500 ("No service for type...")** — não há rede de
  segurança do compilador para isso. Sempre confirme registrando E testando o endpoint.
- **Serviços com HttpClient tipado**: `AddHttpClient<IInterface, Implementacao>()` — só se
  a implementação recebe `HttpClient` no construtor. Se ela usa `IHttpClientFactory`
  diretamente (como `WebhookDispatcherService`), use `AddScoped` normal.
- Comentários em português, curtos, só explicando o *porquê* não óbvio (bug histórico,
  decisão de design, contrato externo) — nunca o *o quê*.
- Nomes de tabela em português (`Fluxo`, `FluxoEtapa`, `EstadoConversa`,
  `ConfiguracaoWebhook`), mas classes/campos C# continuam em inglês (`Flow`, `FlowEtapa`,
  `ConversationState`, `WebhookConfig`) — migração de nomenclatura pendente, documentada
  nos comentários da migration 21. Repositórios fazem a ponte via `nameof()`/constantes.

## Checklist de validação antes de commitar mudanças de lógica de negócio

Não confie só em "buildou" — vários bugs neste projeto (serviços não registrados no DI,
etapa de Flow que só logava warning) só aparecem em runtime. Siga este roteiro:

1. **Build limpo**: `dotnet build source/ProjetoMetaMensagem.WebAPI/ProjetoMetaMensagem.WebAPI.csproj`
   — 0 erros (avisos de compatibilidade net6.0 dos pacotes são pré-existentes, ignore).
2. **Boot real da API** contra o banco local (`dotnet run` na pasta WebAPI) — confira o
   log de inicialização por exceções (ex: `CampanhaWorker iniciado` deve aparecer sem
   `fail:` logo em seguida).
3. **Smoke test de auth**: `POST /api/auth/login` com um usuário seed — confirma JWT,
   BCrypt e a cadeia toda de DI/DB.
4. **Teste funcional do caminho que você mudou**, não só o endpoint isolado:
   - Webhook da Meta: precisa de assinatura HMAC-SHA256 válida (`X-Hub-Signature-256:
     sha256=<hmac>`) calculada com o `ApiWhatsappConnectionConfiguration:AppSecret` do seu
     `appsettings.Development.json`, senão o middleware descarta a requisição.
   - Fluxos/campanhas que chamam a Meta de verdade: sem credenciais reais, espere um erro
     401/403 da Meta — o importante é confirmar que a lógica *chegou* até a chamada HTTP
     certa (URL, payload) e que a falha é tratada sem derrubar o request (log de warning,
     não exceção não capturada).
   - Insira dados de teste direto no banco (via script C# descartável com
     `Microsoft.Data.SqlClient`) quando precisar de um cenário específico (Empresa +
     Contato + Flow + FlowEtapa + Template, por exemplo) — é mais rápido e confiável que
     passar pela API para popular estado de teste.
5. **Confira o efeito colateral no banco**, não só o HTTP 200: um `HistoricoDisparo` só
   deve ser gravado em caso de sucesso real, um `EstadoConversa` deve avançar de etapa
   corretamente, etc. Um endpoint pode responder 200 e não ter feito nada.
6. Sempre revise o `git diff` file por file por consistência de padrão (comentários,
   nomes, tratamento de erro) antes do commit — não só rode os testes.
