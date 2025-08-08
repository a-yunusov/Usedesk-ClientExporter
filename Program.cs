// Точка входа приложения
// Скрипт выполняет ввод секретного ключа, запуск цикла выгрузки клиентов, запись данных в .csv
using System;
using System.Collections.Generic;
using System.IO;
using UsedeskClientExporter.Models;
using UsedeskClientExporter.Services;

namespace UsedeskClientExporter
{
    class Program
    {
        static async Task Main()
        {
            try
            {
                // Ввод секретного ключа
                Console.WriteLine("Скрипт отправляет API запросы для экспорта информации о клиентах (имя, телефон, email и номера тикетов) из Usedesk и сохраняет их в файл clients.csv");
                Console.WriteLine("Практическим методом было выяснено, что оптимальная задержка между запросами при получении ответа Too Many Request составляет 120 секунд, это значение и будет использоваться в скрипте");
                Console.WriteLine();

                Console.Write("Введите секретный ключ: ");
                string secretKey = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(secretKey))
                {
                    Console.WriteLine("Ошибка: секретный ключ не может быть пустым");
                    return;
                }

                // Инициализация сервиса и .csv файла
                var clientService = new UseDeskClientService();

                using var writer = new StreamWriter("clients.csv", false, System.Text.Encoding.UTF8);
                await writer.WriteLineAsync("ID;Name;Emails;Phone;TicketIDs");

                // Пагинация
                int offset = 0;
                const int limit = 100;

                Console.WriteLine("Начинаем выгрузку");

                // Основной цикл
                while (true)
                {
                    var clients = await clientService.GetClientsAsync(secretKey, limit, offset);
                    if (clients == null || clients.Count == 0)
                    {
                        Console.WriteLine("Выгрузка завершена.");
                        break;
                    }

                    foreach (var clientItem in clients)
                    {
                        var details = await clientService.GetClientDetailsAsync(secretKey, clientItem.Id);
                        if (details == null)
                        {
                            Console.WriteLine($"Не удалось получить данные клиента {clientItem.Id}.");
                            continue;
                        }

                        // Фильтр: только с email
                        if (details.EmailList == null || details.EmailList.Count == 0)
                        {
                            Console.WriteLine($"Клиент {clientItem.Id} пропущен (нет email)");
                            continue;
                        }

                        // Форматирование данных
                        var emails = string.Join(", ", details.EmailList.ConvertAll(e => EscapeCsv(e.Email)));
                        var phones = string.Join(", ", details.PhoneList?.ConvertAll(p => EscapeCsv(p.Phone)) ?? new List<string>());
                        var tickets = string.Join(",", details.Tickets ?? new List<long>());

                        // Запись в .csv
                        await writer.WriteLineAsync(
                            $"{details.Id};" +
                            $"\"{EscapeCsv(details.Name)}\";" +
                            $"\"{emails}\";" +
                            $"\"{phones}\";" +
                            $"{tickets}"
                        );

                        Console.WriteLine($"Клиент {details.Id} добавлен в .csv");
                    }

                    // Проверка последней страницы
                    if (clients.Count < limit)
                    {
                        Console.WriteLine("Все клиенты обработаны");
                        break;
                    }

                    // Увеличение offset
                    offset += limit;
                }

                Console.WriteLine("Экспорт завершён успешно! Результат: clients.csv");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Экранирует кавычки в строке для .csv
        /// </summary>
        private static string EscapeCsv(string value)
        {
            return value?.Replace("\"", "\"\"") ?? string.Empty;
        }
    }
}