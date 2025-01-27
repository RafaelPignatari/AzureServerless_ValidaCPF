using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureServerless_ValidaCPF
{
    public static class Function1
    {
        [FunctionName("ValidaCPF")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                string cpf = data?.cpf;

                if (IsValidCpf(cpf))
                    return new OkObjectResult($"O CPF {cpf} é válido.");
                else
                    return new BadRequestObjectResult($"CPF inválido");
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.Message);
            }

            return new BadRequestObjectResult("CPF inválido");
        }

        public static bool IsValidCpf(string cpf)
        {
            // Remove caracteres não númericos.
            char[] digits = new char[11];
            int digitIndex = 0;
            for (int i = 0; i < cpf.Length; i++)
            {
                if (char.IsDigit(cpf[i]))
                {
                    if (digitIndex >= 11)
                        return false;
                    digits[digitIndex++] = cpf[i];
                }
            }

            if (digitIndex != 11)
                return false;

            // Checa números repetidos (111.111.111-11, etc)
            bool allSame = true;
            for (int i = 1; i < digits.Length; i++)
            {
                if (digits[i] != digits[0])
                {
                    allSame = false;
                    break;
                }
            }
            if (allSame)
                return false;

            int[] multiplier1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int sum = 0;
            for (int i = 0; i < 9; i++)
                sum += (digits[i] - '0') * multiplier1[i];

            int remainder = sum % 11;
            int firstDigit = remainder < 2 ? 0 : 11 - remainder;

            int[] multiplier2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            sum = 0;
            for (int i = 0; i < 10; i++)
                sum += (digits[i] - '0') * multiplier2[i];

            remainder = sum % 11;
            int secondDigit = remainder < 2 ? 0 : 11 - remainder;

            return digits[9] - '0' == firstDigit && digits[10] - '0' == secondDigit;
        }
    }
}
