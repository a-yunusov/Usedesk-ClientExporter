// ������ ��� �������������� � UseDesk API
// �������� �� ��������� ������ ��������, ��������� ������� �������, ��������� ������� ��� ������� � �������� JSON-������

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
        private const int MaxRetries = 5; // ������������ ���������� �������

        public UseDeskClientService()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://api.usedesk.ru/");
            _client.DefaultRequestHeaders.Add("User-Agent", "UsedeskClientExporter/1.0");
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        /// <summary>
        /// �������� ������ �������� � ����������
        /// ��� ������� ������ ����� � ���������
        /// </summary>
        /// <param name="token">��������� ���� API</param>
        /// <param name="limit">���������� �������� �� ��������</param>
        /// <param name="offset">�������� (����� ������ ������)</param>
        /// <returns>������ �������� ��� null ��� �������</returns>
        public async Task<List<Models.Client>> GetClientsAsync(string token, int limit, int offset)
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    Console.WriteLine($"������ ������ �������� (offset={offset}, ������� {attempt}/{MaxRetries})");
                    var response = await _client.GetAsync($"clients?api_token={token}&limit={limit}&offset={offset}");

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"������ HTTP {response.StatusCode}: {response.ReasonPhrase}");

                        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            Console.WriteLine("Too Many Requests. ��� 120 ������");
                            await Task.Delay(120000);
                        }
                        else
                        {
                            Console.WriteLine("��� 120 ������ ����� ��������� ��������");
                            await Task.Delay(120000);
                        }

                        continue;
                    }

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    if (!IsJson(jsonResponse))
                    {
                        Console.WriteLine("����� �� �������� JSON");
                        Console.WriteLine($"��������: {jsonResponse.Substring(0, Math.Min(200, jsonResponse.Length))}...");
                        await Task.Delay(120000);
                        continue;
                    }

                    var clients = JsonSerializer.Deserialize<List<Models.Client>>(jsonResponse);
                    if (clients == null)
                    {
                        Console.WriteLine("�� ������� ��������������� ������ ��������");
                        await Task.Delay(120000);
                        continue;
                    }

                    Console.WriteLine($"�������� {clients.Count} ��������");
                    return clients;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"������ ����: {ex.Message}");
                    if (attempt < MaxRetries)
                    {
                        Console.WriteLine("��� 1 �������");
                        await Task.Delay(1000);
                    }
                }
            }

            Console.WriteLine("�� ������� �������� ������ �������� ����� 5 �������");
            return null;
        }

        /// <summary>
        /// �������� ��������� ���������� � ������� �� ID
        /// </summary>
        /// <param name="token">��������� ���� API</param>
        /// <param name="clientId">ID �������</param>
        /// <returns>������ ������� ��� null</returns>
        public async Task<ClientData> GetClientDetailsAsync(string token, long clientId)
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    Console.WriteLine($"������ ������� ������� {clientId} (������� {attempt}/{MaxRetries})");
                    var response = await _client.GetAsync($"client?api_token={token}&client_id={clientId}");

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"������ HTTP {response.StatusCode} ��� ������� {clientId}");

                        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            Console.WriteLine("Too Many Requests. ��� 120 ������");
                            await Task.Delay(120000);
                        }
                        else
                        {
                            Console.WriteLine("��� 60 ������");
                            await Task.Delay(60000);
                        }

                        continue;
                    }

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    if (!IsJson(jsonResponse))
                    {
                        Console.WriteLine($"����� �� JSON ��� ������� {clientId}");
                        await Task.Delay(120000);
                        continue;
                    }

                    var detailsList = JsonSerializer.Deserialize<List<ClientDetails>>(jsonResponse);
                    if (detailsList == null || detailsList.Count == 0)
                    {
                        Console.WriteLine($"��� ������ ��� ������� {clientId}");
                        return null;
                    }

                    return detailsList[0].Client;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"������ ��� ��������� ������� {clientId}: {ex.Message}");
                    if (attempt < MaxRetries)
                    {
                        Console.WriteLine("��� 1 �������");
                        await Task.Delay(1000);
                    }
                }
            }

            Console.WriteLine($"�� ������� ���������� ������� {clientId}");
            return null;
        }

        /// <summary>
        /// ���������, �������� �� ������ �������� JSON
        /// </summary>
        /// <param name="content">������ ��� ��������</param>
        /// <returns>true, ���� ��� JSON</returns>
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