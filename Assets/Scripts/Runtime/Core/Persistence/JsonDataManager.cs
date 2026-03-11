using System.IO;
using UnityEngine;

namespace ShadowRhythm.Core.Persistence
{
    public enum JsonType
    {
        JsonUtility,
        LitJson,
    }

    public sealed class JsonDataManager
    {
        private static JsonDataManager _instance;
        public static JsonDataManager Instance => _instance ??= new JsonDataManager();

        private JsonDataManager() { }

        /// <summary>
        /// 保存数据到 persistentDataPath
        /// </summary>
        public void SaveData(string fileName, object data, JsonType type = JsonType.LitJson)
        {
            string path = GetPersistentPath(fileName);
            string jsonStr = SerializeToJson(data, type);
            File.WriteAllText(path, jsonStr);
        }

        /// <summary>
        /// 加载数据，优先从 StreamingAssets 读取，其次从 persistentDataPath
        /// </summary>
        public T LoadData<T>(string fileName, JsonType type = JsonType.LitJson) where T : new()
        {
            string path = ResolveReadPath(fileName);
            if (string.IsNullOrEmpty(path))
                return new T();

            string jsonStr = File.ReadAllText(path);
            return DeserializeFromJson<T>(jsonStr, type);
        }

        /// <summary>
        /// 从指定子目录加载数据（用于分类 JSON）
        /// </summary>
        public T LoadDataFromSubfolder<T>(string subfolder, string fileName, JsonType type = JsonType.LitJson) where T : new()
        {
            string relativePath = Path.Combine(subfolder, fileName);
            return LoadData<T>(relativePath, type);
        }

        private string GetStreamingPath(string fileName)
        {
            return Path.Combine(Application.streamingAssetsPath, "Json", fileName + ".json");
        }

        private string GetPersistentPath(string fileName)
        {
            return Path.Combine(Application.persistentDataPath, fileName + ".json");
        }

        private string ResolveReadPath(string fileName)
        {
            string streamingPath = GetStreamingPath(fileName);
            if (File.Exists(streamingPath))
                return streamingPath;

            string persistentPath = GetPersistentPath(fileName);
            if (File.Exists(persistentPath))
                return persistentPath;

            Debug.LogWarning($"[JsonDataManager] 找不到文件: {fileName}");
            return null;
        }

        private string SerializeToJson(object data, JsonType type)
        {
            return type switch
            {
                JsonType.JsonUtility => JsonUtility.ToJson(data),
                JsonType.LitJson => LitJson.JsonMapper.ToJson(data),
                _ => string.Empty
            };
        }

        private T DeserializeFromJson<T>(string jsonStr, JsonType type) where T : new()
        {
            return type switch
            {
                JsonType.JsonUtility => JsonUtility.FromJson<T>(jsonStr),
                JsonType.LitJson => LitJson.JsonMapper.ToObject<T>(jsonStr),
                _ => new T()
            };
        }
    }
}