// Скрипт для краткого списка клиентов (только ID)
// Используется при получении списка клиентов с пагинацией
namespace UsedeskClientExporter.Models
{
    public class Client
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public long Id { get; set; }
    }
}