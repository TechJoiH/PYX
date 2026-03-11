using UnityEngine;
using ShadowRhythm.Fighter;
using ShadowRhythm.Combat;
using ShadowRhythm.Rhythm;

namespace ShadowRhythm.Debugging
{
    /// <summary>
    /// 战斗调试面板 - 显示战斗状态和结果
    /// </summary>
    public class CombatDebugPanel : MonoBehaviour
    {
        [Header("依赖")]
        [SerializeField] private FighterRuntime playerFighter;
        [SerializeField] private FighterRuntime enemyFighter;
        [SerializeField] private CombatSystem combatSystem;
        [SerializeField] private BeatClockSystem beatClockSystem;

        [Header("颜色")]
        [SerializeField] private Color hitColor = Color.red;
        [SerializeField] private Color parryColor = Color.cyan;
        [SerializeField] private Color dodgeColor = Color.yellow;
        [SerializeField] private Color blockColor = Color.gray;

        private CombatResult? _lastResult;
        private float _resultDisplayTimer;

        private void Start()
        {
            // 延迟查找，等待 CombatTestRunner 创建角色
            Invoke(nameof(FindReferences), 0.1f);
        }

        private void FindReferences()
        {
            // 自动查找
            if (playerFighter == null)
                playerFighter = GameObject.Find("Player")?.GetComponent<FighterRuntime>();
            if (enemyFighter == null)
                enemyFighter = GameObject.Find("Enemy_Dummy")?.GetComponent<FighterRuntime>();
            if (combatSystem == null)
                combatSystem = FindObjectOfType<CombatSystem>();
            if (beatClockSystem == null)
                beatClockSystem = FindObjectOfType<BeatClockSystem>();

            if (combatSystem != null)
            {
                combatSystem.OnCombatResult += HandleCombatResult;
            }

            Debug.Log($"[CombatDebugPanel] 已关联 Player={playerFighter != null}, Enemy={enemyFighter != null}");
        }

        private void OnDestroy()
        {
            if (combatSystem != null)
            {
                combatSystem.OnCombatResult -= HandleCombatResult;
            }
        }

        private void Update()
        {
            if (_resultDisplayTimer > 0)
                _resultDisplayTimer -= Time.deltaTime;
        }

        private void HandleCombatResult(CombatResult result)
        {
            _lastResult = result;
            _resultDisplayTimer = 1.5f;
        }

        private void OnGUI()
        {
            // 左侧 - 玩家状态
            DrawFighterStatus(playerFighter, new Rect(10, 10, 250, 200), "PLAYER");

            // 右侧 - 敌人状态
            DrawFighterStatus(enemyFighter, new Rect(Screen.width - 260, 10, 250, 200), "ENEMY");

            // 中央 - 战斗结果
            DrawCombatResult();

            // 底部 - 当前拍点
            DrawBeatInfo();

            // 操作提示
            DrawControls();
        }

        private void DrawFighterStatus(FighterRuntime fighter, Rect area, string label)
        {
            if (fighter == null)
            {
                GUILayout.BeginArea(area);
                GUILayout.BeginVertical("box");
                GUILayout.Label($"═══ {label} ═══");
                GUILayout.Label("未找到角色");
                GUILayout.EndVertical();
                GUILayout.EndArea();
                return;
            }

            GUILayout.BeginArea(area);
            GUILayout.BeginVertical("box");

            GUILayout.Label($"═══ {label} ═══");

            // 生命值
            float hpPercent = fighter.Stats.HealthPercent;
            GUI.color = Color.Lerp(Color.red, Color.green, hpPercent);
            GUILayout.Label($"HP: {fighter.Stats.currentHealth}/{fighter.Stats.maxHealth}");

            // 血条
            var hpBarRect = GUILayoutUtility.GetRect(200, 20);
            GUI.color = Color.gray;
            GUI.Box(hpBarRect, "");
            GUI.color = Color.Lerp(Color.red, Color.green, hpPercent);
            GUI.Box(new Rect(hpBarRect.x, hpBarRect.y, hpBarRect.width * hpPercent, hpBarRect.height), "");

            GUI.color = Color.white;

            // 状态
            var state = fighter.StateMachine.CurrentState;
            GUI.color = GetStateColor(state);
            GUILayout.Label($"State: {state}");
            GUI.color = Color.white;

            // 当前招式
            if (fighter.CurrentMove != null)
            {
                GUILayout.Label($"Move: {fighter.CurrentMove.displayName}");
            }
            else
            {
                GUILayout.Label("Move: -");
            }

            // 属性
            GUILayout.Label($"ATK: {fighter.Stats.baseAttack} | DEF: {fighter.Stats.baseDefense}");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawCombatResult()
        {
            if (!_lastResult.HasValue || _resultDisplayTimer <= 0) return;

            var result = _lastResult.Value;
            string text = result.resultType switch
            {
                CombatResultType.Hit => $"HIT! -{result.damage}",
                CombatResultType.Parried => "PARRY!",
                CombatResultType.Dodged => "DODGE!",
                CombatResultType.Blocked => "BLOCK!",
                _ => ""
            };

            if (string.IsNullOrEmpty(text)) return;

            GUI.color = GetResultColor(result.resultType);

            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 48,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            GUI.Label(new Rect(0, Screen.height / 2 - 50, Screen.width, 100), text, style);
            GUI.color = Color.white;
        }

        private void DrawBeatInfo()
        {
            if (beatClockSystem == null || !beatClockSystem.IsRunning) return;

            GUILayout.BeginArea(new Rect(Screen.width / 2 - 150, Screen.height - 80, 300, 70));
            GUILayout.BeginVertical("box");

            GUILayout.Label($"Beat: {beatClockSystem.CurrentBeatIndex} | Bar: {beatClockSystem.CurrentBarIndex} | {beatClockSystem.CurrentBeatInBar + 1}/4");
            
            // 节拍进度条
            var progressRect = GUILayoutUtility.GetRect(280, 15);
            GUI.color = Color.gray;
            GUI.Box(progressRect, "");
            GUI.color = Color.cyan;
            GUI.Box(new Rect(progressRect.x, progressRect.y, progressRect.width * beatClockSystem.CurrentBeatProgress, progressRect.height), "");
            GUI.color = Color.white;

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawControls()
        {
            GUILayout.BeginArea(new Rect(10, Screen.height - 120, 300, 110));
            GUILayout.BeginVertical("box");

            GUILayout.Label("═══ 操作说明 ═══");
            GUILayout.Label("← = 轻斩 | ↓ = 短刺 | → = 闪避");
            GUILayout.Label("←+→ = 冲刺斩 | ↑+← = 上挑");
            GUILayout.Label("↑+→ = 弹反");
            GUILayout.Label("R = 重置战斗");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private Color GetStateColor(FighterState state)
        {
            return state switch
            {
                FighterState.Idle => Color.white,
                FighterState.Startup => Color.yellow,
                FighterState.Active => Color.red,
                FighterState.Recovery => Color.gray,
                FighterState.Hitstun => new Color(1f, 0.5f, 0f),
                FighterState.Guard => Color.blue,
                FighterState.Parry => Color.cyan,
                FighterState.Dash => Color.green,
                FighterState.Dead => Color.black,
                _ => Color.white
            };
        }

        private Color GetResultColor(CombatResultType type)
        {
            return type switch
            {
                CombatResultType.Hit => hitColor,
                CombatResultType.Parried => parryColor,
                CombatResultType.Dodged => dodgeColor,
                CombatResultType.Blocked => blockColor,
                _ => Color.white
            };
        }
    }
}