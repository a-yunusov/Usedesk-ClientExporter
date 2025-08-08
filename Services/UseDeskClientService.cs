// Сервис для взаимодействия с UseDesk API
// Отвечает за получение списка клиентов, получение деталей клиента, повторные попытки при ошибках и проверку JSON-ответа

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using UsedeskClientExporter.Models;

namespace UsedeskClientExporter.Services
{
    public class UseDeskClientService
    {
        private readonly HttpClient _client;
        private const int MaxRetries = 5; // Максимальное количество попыток

        public UseDeskClientService()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://api.usedesk.ru/");
            _client.DefaultRequestHeaders.Add("User-Agent", "UsedeskClientExporter/1.0");
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        /// <summary>
        /// Получает список клиентов с пагинацией
        /// При ошибках делает паузу и повторяет
        /// </summary>
        /// <param name="token">Секретный ключ API</param>
        /// <param name="limit">Количество клиентов на странице</param>
        /// <param name="offset">Смещение (номер первой записи)</param>
        /// <returns>Список клиентов или null при неудаче</returns>
        public async Task<List<Models.Client>> GetClientsAsync(string token, int limit, int offset)
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    Console.WriteLine($"Запрос списка клиентов (offset={offset}, попытка {attempt}/{MaxRetries})");
                    var response = await _client.GetAsync($"clients?api_token={token}&limit={limit}&offset={offset}");

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Ошибка HTTP {response.StatusCode}: {response.ReasonPhrase}");

                        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            Console.WriteLine("Too Many Requests. Ждём 120 секунд");
                            await Task.Delay(120000);
                        }
                        else
                        {
                            Console.WriteLine("Ждём 120 секунд перед повторной попыткой");
                            await Task.Delay(120000);
                        }

                        continue;
                    }

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    if (!IsJson(jsonResponse))
                    {
                        Console.WriteLine("Ответ не является JSON");
                        Console.WriteLine($"Фрагмент: {jsonResponse.Substring(0, Math.Min(200, jsonResponse.Length))}...");
                        await Task.Delay(120000);
                        continue;
                    }

                    var clients = JsonSerializer.Deserialize<List<Models.Client>>(jsonResponse);
                    if (clients == null)
                    {
                        Console.WriteLine("Не удалось десериализовать список клиентов");
                        await Task.Delay(120000);
                        continue;
                    }

                    Console.WriteLine($"Получено {clients.Count} клиентов");
                    return clients;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка сети: {ex.Message}");
                    if (attempt < MaxRetries)
                    {
                        Console.WriteLine("Ждём 1 секунду");
                        await Task.Delay(1000);
                    }
                }
            }

            Console.WriteLine("Не удалось получить список клиентов после 5 попыток");
            return null;
        }

        /// <summary>
        /// Получает детальную информацию о клиенте по ID
        /// </summary>
        /// <param name="token">Секретный ключ API</param>
        /// <param name="clientId">ID клиента</param>
        /// <returns>Данные клиента или null</returns>
        public async Task<ClientData> GetClientDetailsAsync(string token, long clientId)
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    Console.WriteLine($"Запрос деталей клиента {clientId} (попытка {attempt}/{MaxRetries})");
                    var response = await _client.GetAsync($"client?api_token={token}&client_id={clientId}");

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Ошибка HTTP {response.StatusCode} для клиента {clientId}");

                        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            Console.WriteLine("Too Many Requests. Ждём 120 секунд");
                            await Task.Delay(120000);
                        }
                        else
                        {
                            Console.WriteLine("Ждём 60 секунд");
                            await Task.Delay(60000);
                        }

                        continue;
                    }

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    if (!IsJson(jsonResponse))
                    {
                        Console.WriteLine($"Ответ не JSON для клиента {clientId}");
                        await Task.Delay(120000);
                        continue;
                    }

                    var detailsList = JsonSerializer.Deserialize<List<ClientDetails>>(jsonResponse);
                    if (detailsList == null || detailsList.Count == 0)
                    {
                        Console.WriteLine($"Нет данных для клиента {clientId}");
                        return null;
                    }

                    return detailsList[0].Client;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обработке клиента {clientId}: {ex.Message}");
                    if (attempt < MaxRetries)
                    {
                        Console.WriteLine("Ждём 1 секунду");
                        await Task.Delay(1000);
                    }
                }
            }

            Console.WriteLine($"Не удалось обработать клиента {clientId}");
            return null;
        }

        /// <summary>
        /// Проверяет, является ли строка валидным JSON
        /// </summary>
        /// <param name="content">Строка для проверки</param>
        /// <returns>true, если это JSON</returns>
        private bool IsJson(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return false;
            try
            {
                using var doc = JsonDocument.Parse(content.Trim());
                return true;
            }
            catch { return false; }
        }
    }
}