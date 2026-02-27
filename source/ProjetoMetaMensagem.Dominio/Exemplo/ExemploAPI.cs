using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Exemplo
{
    public class ExemploAPI
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string apiUrl = "SUA_URL_DA_ZAPI"; // Substitua pela URL da sua Z-API
        private static readonly string instanceId = "SEU_INSTANCE_ID"; // Substitua pelo seu Instance ID
        private static readonly string accessToken = "SEU_ACCESS_TOKEN"; // Substitua pelo seu Access Token

        public async Task<int> SendWhatsAppMessage(string phoneNumber, string message)
        {
            try
            {
                var requestData = new
                {
                    phone = phoneNumber,
                    message = message
                };

                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var response = await client.PostAsync($"{apiUrl}/send-text/{instanceId}", content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);

                return 1;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro na requisição: {ex.Message}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocorreu um erro: {ex.Message}");
                return 0;
            }
        }
    }
}
