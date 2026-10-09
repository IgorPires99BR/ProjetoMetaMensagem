using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Servicos;
using QRCoder;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Cobranca
{
    // Pagina publica do Pix de uma cobranca: destino do botao "Pagar com Pix" do template e
    // origem da imagem do QR no cabecalho. Quem abre e o cliente final, sem login.
    //
    // O txid (Id da cobranca, 122 bits aleatorios) e a unica chave -- nao da pra adivinhar o de
    // outra pessoa. A pagina so mostra o que ja foi enviado ao proprio cliente no WhatsApp.
    [AllowAnonymous]
    public class PagamentoPixController : ControllerBase
    {
        private static readonly Regex FormatoTxid = new("^[a-f0-9]{32}$", RegexOptions.Compiled);
        private static readonly CultureInfo Brasil = new("pt-BR");

        private readonly IUnitOfWork _unitOfWork;

        public PagamentoPixController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("pix/{txid}/qrcode.png")]
        public async Task<IActionResult> QrCode([FromRoute] string txid)
        {
            var cobranca = await ObterCobranca(txid);
            if (cobranca?.PixCopiaECola == null) return NotFound();

            using var gerador = new QRCodeGenerator();
            using var dados = gerador.CreateQrCode(cobranca.PixCopiaECola, QRCodeGenerator.ECCLevel.M);
            var png = new PngByteQRCode(dados).GetGraphic(12);

            // A Meta baixa a imagem uma vez por mensagem; cache curto evita reprocessar o QR a
            // cada abertura da pagina sem prender um QR antigo se a cobranca mudar.
            Response.Headers.CacheControl = "public, max-age=600";
            return File(png, "image/png");
        }

        [HttpGet("pix/{txid}")]
        public async Task<IActionResult> Pagina([FromRoute] string txid)
        {
            var cobranca = await ObterCobranca(txid);
            if (cobranca?.PixCopiaECola == null)
                return Content(Montar("Cobrança não encontrada", "<p class=\"aviso\">Este link de pagamento não existe ou foi removido.</p>"), "text/html; charset=utf-8");

            var empresa = await _unitOfWork.Empresa.ObterPorId(cobranca.EmpresaId);
            var nomeEmpresa = WebUtility.HtmlEncode(empresa?.Nome ?? "");
            var valor = (cobranca.ValorCobrado ?? cobranca.Valor).ToString("N2", Brasil);
            var vencimento = cobranca.DataVencimento.ToString("dd/MM/yyyy", Brasil);

            string corpo;
            if (cobranca.Status == StatusCobrancaCliente.Paga)
            {
                var pagoEm = cobranca.DataPagamento?.ToString("dd/MM/yyyy 'às' HH:mm", Brasil) ?? "";
                corpo = $@"<p class=""ok"">Pagamento confirmado{(pagoEm == "" ? "" : " em " + pagoEm)}.</p>
<p class=""linha""><span>Valor</span><strong>R$ {valor}</strong></p>";
            }
            else if (cobranca.Status != StatusCobrancaCliente.Pendente || (cobranca.PixExpiraEm.HasValue && cobranca.PixExpiraEm.Value < HorarioBrasilia.Agora()))
            {
                corpo = $@"<p class=""aviso"">Este Pix não está mais disponível para pagamento. Fale com {(nomeEmpresa == "" ? "quem enviou a cobrança" : nomeEmpresa)} para receber um novo.</p>";
            }
            else
            {
                var encargos = (cobranca.Multa ?? 0m) + (cobranca.Juros ?? 0m);
                var copiaECola = WebUtility.HtmlEncode(cobranca.PixCopiaECola);
                var validoAte = cobranca.PixExpiraEm?.ToString("dd/MM/yyyy 'às' HH:mm", Brasil);
                corpo = $@"<p class=""linha""><span>Valor</span><strong>R$ {valor}</strong></p>
<p class=""linha""><span>Vencimento</span><strong>{vencimento}</strong></p>
{(encargos > 0 ? $@"<p class=""nota"">Inclui R$ {encargos.ToString("N2", Brasil)} de multa e juros por atraso.</p>" : "")}
<img class=""qr"" src=""/pix/{cobranca.Txid}/qrcode.png"" alt=""QR Code do Pix"" width=""240"" height=""240"">
<p class=""nota"">Abra o app do seu banco, escolha Pix &gt; Pix Copia e Cola e cole o código abaixo.</p>
<textarea id=""codigo"" readonly rows=""4"">{copiaECola}</textarea>
<button id=""copiar"" type=""button"">Copiar código Pix</button>
{(validoAte == null ? "" : $@"<p class=""nota"">Válido até {validoAte}.</p>")}
<script>
document.getElementById('copiar').addEventListener('click', function () {{
  var campo = document.getElementById('codigo'), botao = this;
  function feito() {{ botao.textContent = 'Código copiado'; setTimeout(function () {{ botao.textContent = 'Copiar código Pix'; }}, 2500); }}
  if (navigator.clipboard) {{ navigator.clipboard.writeText(campo.value).then(feito); }}
  else {{ campo.select(); document.execCommand('copy'); feito(); }}
}});
</script>";
            }

            var titulo = nomeEmpresa == "" ? "Pagamento via Pix" : $"Pagamento via Pix · {nomeEmpresa}";
            Response.Headers.CacheControl = "no-store";
            return Content(Montar(titulo, corpo), "text/html; charset=utf-8");
        }

        private async Task<Dominio.Entidades.CobrancaCliente?> ObterCobranca(string txid)
        {
            txid = (txid ?? "").ToLowerInvariant();
            if (!FormatoTxid.IsMatch(txid)) return null;
            return await _unitOfWork.CobrancaCliente.ObterPorTxid(txid);
        }

        private static string Montar(string titulo, string corpo) => $@"<!doctype html>
<html lang=""pt-BR"">
<head>
<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
<meta name=""robots"" content=""noindex"">
<title>{titulo}</title>
<style>
  :root {{ --bg:#f4f6fb; --card:#fff; --texto:#1c2333; --suave:#5d6577; --borda:#e1e5ee; --azul:#2f5fe0; --verde:#1e9e57; --alerta:#b4541a; }}
  @media (prefers-color-scheme: dark) {{ :root {{ --bg:#11151d; --card:#1a202b; --texto:#e8ebf2; --suave:#a3aabb; --borda:#2c3442; --azul:#6f93ff; --verde:#46c983; --alerta:#f0a060; }} }}
  * {{ box-sizing:border-box; }}
  body {{ margin:0; background:var(--bg); color:var(--texto); font:16px/1.5 system-ui,-apple-system,'Segoe UI',Roboto,sans-serif; }}
  main {{ max-width:420px; margin:0 auto; padding:24px 16px 40px; }}
  .card {{ background:var(--card); border:1px solid var(--borda); border-radius:14px; padding:22px 18px; }}
  h1 {{ font-size:1.15rem; margin:0 0 16px; }}
  .linha {{ display:flex; justify-content:space-between; margin:6px 0; }}
  .linha span {{ color:var(--suave); }}
  .nota {{ color:var(--suave); font-size:.86rem; margin:10px 0; }}
  .qr {{ display:block; width:240px; max-width:100%; height:auto; margin:16px auto; background:#fff; padding:8px; border-radius:8px; }}
  textarea {{ width:100%; font:13px/1.4 ui-monospace,Menlo,Consolas,monospace; padding:10px; border:1px solid var(--borda); border-radius:8px; background:var(--bg); color:var(--texto); resize:none; word-break:break-all; }}
  button {{ width:100%; margin-top:10px; padding:13px; border:0; border-radius:10px; background:var(--azul); color:#fff; font-size:1rem; font-weight:600; cursor:pointer; }}
  .ok {{ color:var(--verde); font-weight:600; }}
  .aviso {{ color:var(--alerta); }}
</style>
</head>
<body><main><div class=""card""><h1>{titulo}</h1>{corpo}</div></main></body>
</html>";
    }
}
