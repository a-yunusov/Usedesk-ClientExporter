// ������ ��� ���������� ������ API
// UseDesk ���������� ������ � �������: { "client": { ... } }
// ���� ����� ��������� ��������� ��������������� ����� �����
namespace UsedeskClientExporter.Models
{
    public class ClientDetails
    {
        [System.Text.Json.Serialization.JsonPropertyName("client")]
        public ClientData Client { get; set; }
    }
}