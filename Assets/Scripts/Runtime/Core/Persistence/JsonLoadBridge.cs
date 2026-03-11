using ShadowRhythm.Data.Models;
using System.Collections.Generic;
namespace ShadowRhythm.Core.Persistence
{
    /// <summary>
    /// JSON 数据加载桥接器，提供类型化的数据加载方法
    /// </summary>
    public sealed class JsonLoadBridge
    {
        private readonly JsonDataManager _jsonDataManager;

        public JsonLoadBridge(JsonDataManager jsonDataManager)
        {
            _jsonDataManager = jsonDataManager;
        }

        public SongMetaModel LoadSongMeta(string songId)
        {
            return _jsonDataManager.LoadDataFromSubfolder<SongMetaModel>("Songs", $"song_{songId}_meta");
        }

        public EnemyPatternModel LoadEnemyPattern(string patternId)
        {
            return _jsonDataManager.LoadDataFromSubfolder<EnemyPatternModel>("Patterns", patternId);
        }

        public List<MoveDefinitionModel> LoadMoveDefinitions()
        {
            var container = _jsonDataManager.LoadDataFromSubfolder<MoveDefinitionContainer>("Moves", "move_definitions");
            return container?.moves ?? new List<MoveDefinitionModel>();
        }

        public JudgeWindowConfigModel LoadJudgeWindowConfig()
        {
            return _jsonDataManager.LoadDataFromSubfolder<JudgeWindowConfigModel>("Balance", "judge_windows");
        }
    }
}