namespace DominosLocationMap.Core.Utilities.Helpers.DataConvertHelper
{
    public interface IObjectDataConverter
    {
        T JsonToObject<T>(string jsonString) where T : class, new();

        string ObjectToJson<T>(T entityObject) where T : class, new();

        T ParseObjectDataArray<T>(byte[] rawBytes) where T : class, new();
    }
}