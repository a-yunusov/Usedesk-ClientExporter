// ������ ��� �������� ������ �������� (������ ID)
// ������������ ��� ��������� ������ �������� � ����������
namespace UsedeskClientExporter.Models
{
    public class Client
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public long Id { get; set; }
    }
}