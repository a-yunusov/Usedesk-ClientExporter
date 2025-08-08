// Основная модель данных о клиенте
// Содержит все поля, возвращаемые UseDesk API
using System.Collections.Generic;

namespace UsedeskClientExporter.Models
{
    public class ClientData
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public long Id { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string Name { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("tickets")]
        public List<long> Tickets { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("emails")]
        public List<EmailData> EmailList { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("phones")]
        public List<PhoneData> PhoneList { get; set; }
    }

    // Модель для email-адреса
    public class EmailData
    {
        [System.Text.Json.Serialization.JsonPropertyName("email")]
        public string Email { get; set; }
    }

    // Модель для телефона
    public class PhoneData
    {
        [System.Text.Json.Serialization.JsonPropertyName("phone")]
        public string Phone { get; set; }
    }
}