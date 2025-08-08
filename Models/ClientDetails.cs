// Обёртка для детального ответа API
// UseDesk возвращает данные в формате: { "client": { ... } }
// Этот класс позволяет корректно десериализовать такой ответ
namespace UsedeskClientExporter.Models
{
    public class ClientDetails
    {
        [System.Text.Json.Serialization.JsonPropertyName("client")]
        public ClientData Client { get; set; }
    }
}