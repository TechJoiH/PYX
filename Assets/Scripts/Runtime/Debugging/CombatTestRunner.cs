using System.Collections.Generic;
using UnityEngine;
using ShadowRhythm.Fighter;
using ShadowRhythm.Combat;
using ShadowRhythm.Rhythm;
using ShadowRhythm.Command;
using ShadowRhythm.Data.Models;
using ShadowRhythm.Core.Persistence;

namespace ShadowRhythm.Debugging
{
    /// <summary>
    /// 战斗测试运行器 - Sandbox_Combat 场景专用
    /// </summary>
    public class CombatTestRunner : MonoBehaviour
    {
        [Header("场景依赖")]
        [SerializeField] private BeatClockSystem beatClockSystem;
        [SerializeField] private CommandResolver commandResolver;
        [SerializeField] private CombatSystem combatSystem;

        [Header("战斗者 Prefab")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject enemyPrefab;

        [Header("生成位置")]
        [SerializeField] private Vector3 playerSpawnPos = new Vector3(-3, 0, 0);
        [SerializeField] private Vector3 enemySpawnPos = new Vector3(3, 0, 0);

        private FighterRuntime _player;
        private FighterRuntime _enemy;
        private JsonDataManager _jsonManager;

        private void Start()
        {
            Debug.Log("[CombatTestRunner] ===== Start() 被调用 =====");  // 添加这行
            
            _jsonManager = JsonDataManager.Instance;

            InitializeFighters();
            LoadMoveDefinitions();
            InitializeSystems();

            Debug.Log("═══════════════════════════════════════");
            Debug.Log("       COMBAT SANDBOX INITIALIZED       ");
            Debug.Log("═══════════════════════════════════════");
            Debug.Log("按键说明:");
            Debug.Log("  (↑) = Lift 抬起");
            Debug.Log("  (↓) = Shake 摇动");
            Debug.Log("  (←) = Flick 轻弹");
            Debug.Log("  (→) = Flash 闪避");
            Debug.Log("  ↑+← = 上劈");
            Debug.Log("  ↓+← = 弹反");
            Debug.Log("  →+← = 闪击");
            Debug.Log("  R = 重置战斗");
            Debug.Log("═══════════════════════════════════════");
        }

        private void Update()
        {
            // R 键重置
            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                ResetBattle();
            }
        }

        private void InitializeFighters()
        {
            Debug.Log($"[CombatTestRunner] InitializeFighters 开始, playerPrefab={playerPrefab}, enemyPrefab={enemyPrefab}");
            
            // 创建玩家
            if (playerPrefab != null)
            {
                Debug.Log("[CombatTestRunner] 使用 Prefab 创建玩家");
                var playerObj = Instantiate(playerPrefab, playerSpawnPos, Quaternion.identity);
                _player = playerObj.GetComponent<FighterRuntime>();
            }
            else
            {
                Debug.Log("[CombatTestRunner] 自动创建玩家");
                var playerObj = new GameObject("Player");
                playerObj.transform.position = playerSpawnPos;
                _player = playerObj.AddComponent<FighterRuntime>();
                SetupSimpleColliders(playerObj, "player");
            }

            // 创建敌人（站桩）
            if (enemyPrefab != null)
            {
                Debug.Log("[CombatTestRunner] 使用 Prefab 创建敌人");
                var enemyObj = Instantiate(enemyPrefab, enemySpawnPos, Quaternion.identity);
                _enemy = enemyObj.GetComponent<FighterRuntime>();
            }
            else
            {
                Debug.Log("[CombatTestRunner] 自动创建敌人");
                var enemyObj = new GameObject("Enemy_Dummy");
                enemyObj.transform.position = enemySpawnPos;
                _enemy = enemyObj.AddComponent<FighterRuntime>();
                SetupSimpleColliders(enemyObj, "enemy");
            }

            Debug.Log($"[CombatTestRunner] 战斗者初始化完成, _player={_player}, _enemy={_enemy}");
        }

        private void SetupSimpleColliders(GameObject obj, string ownerId)
        {
            var fighterRuntime = obj.GetComponent<FighterRuntime>();

            // Hurtbox（身体）
            var hurtboxObj = new GameObject("Hurtbox");
            hurtboxObj.transform.SetParent(obj.transform);
            hurtboxObj.transform.localPosition = Vector3.zero;

            var hurtboxCollider = hurtboxObj.AddComponent<BoxCollider2D>();
            hurtboxCollider.size = new Vector2(1f, 2f);
            hurtboxCollider.isTrigger = true;

            var hurtbox = hurtboxObj.AddComponent<HurtboxController>();
            hurtbox.SetOwner(ownerId);

            // 添加 Rigidbody2D（物理检测必需）
            var rb = hurtboxObj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            // Hitbox（攻击范围）
            var hitboxObj = new GameObject("Hitbox");
            hitboxObj.transform.SetParent(obj.transform);
            hitboxObj.transform.localPosition = new Vector3(1f, 0, 0);

            var hitboxCollider = hitboxObj.AddComponent<BoxCollider2D>();
            hitboxCollider.size = new Vector2(1.5f, 1f);
            hitboxCollider.isTrigger = true;

            var hitbox = hitboxObj.AddComponent<HitboxController>();
            hitbox.SetOwner(ownerId);

            // 添加控制器并手动设置引用
            if (ownerId == "player")
            {
                var controller = obj.AddComponent<PlayerCombatController>();
                SetControllerReferences(controller, fighterRuntime, hitbox, hurtbox);
            }
            else
            {
                var controller = obj.AddComponent<EnemyCombatController>();
                SetControllerReferences(controller, fighterRuntime, hitbox, hurtbox);
            }

            // 可视化（简单方块）
            var visualObj = new GameObject("Visual");
            visualObj.transform.SetParent(obj.transform);
            visualObj.transform.localPosition = Vector3.zero;

            var sprite = visualObj.AddComponent<SpriteRenderer>();
            sprite.sprite = CreateSquareSprite();
            sprite.color = ownerId == "player" ? new Color(0.3f, 0.5f, 1f) : new Color(1f, 0.3f, 0.3f);
        }

        private void SetControllerReferences(PlayerCombatController controller, FighterRuntime runtime, HitboxController hitbox, HurtboxController hurtbox)
        {
            // 使用反射设置私有字段
            var type = typeof(PlayerCombatController);
            type.GetField("fighterRuntime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(controller, runtime);
            type.GetField("hitbox", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(controller, hitbox);
            type.GetField("hurtbox", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(controller, hurtbox);
        }

        private void SetControllerReferences(EnemyCombatController controller, FighterRuntime runtime, HitboxController hitbox, HurtboxController hurtbox)
        {
            var type = typeof(EnemyCombatController);
            type.GetField("fighterRuntime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(controller, runtime);
            type.GetField("hitbox", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(controller, hitbox);
            type.GetField("hurtbox", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(controller, hurtbox);
        }

        private Sprite CreateSquareSprite()
        {
            var tex = new Texture2D(32, 32);
            var colors = new Color[32 * 32];
            for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
            tex.SetPixels(colors);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        }

        private void LoadMoveDefinitions()
        {
            // 尝试从 JSON 加载
            var moves = _jsonManager.LoadData<MoveDefinitionContainer>("Balance/move_definitions");

            if (moves != null && moves.moves != null)
            {
                combatSystem?.RegisterMoveDefinitions(moves.moves);
                Debug.Log($"[CombatTestRunner] 从 JSON 加载 {moves.moves.Count} 个招式定义");
            }
            else
            {
                // 使用默认招式
                var defaultMoves = CreateDefaultMoves();
                combatSystem?.RegisterMoveDefinitions(defaultMoves);
                Debug.Log("[CombatTestRunner] 使用默认招式定义");
            }
        }

        private List<MoveDefinitionModel> CreateDefaultMoves()
        {
            return new List<MoveDefinitionModel>
            {
                new MoveDefinitionModel
                {
                    moveId = "flick_light",
                    displayName = "轻斩",
                    commandType = "Flick",
                    startupBeats = 0,
                    activeBeats = 1,
                    recoveryBeats = 1,
                    damage = 10
                },
                new MoveDefinitionModel
                {
                    moveId = "shake_stab",
                    displayName = "短刺",
                    commandType = "Shake",
                    startupBeats = 0,
                    activeBeats = 1,
                    recoveryBeats = 1,
                    damage = 8
                },
                new MoveDefinitionModel
                {
                    moveId = "dash_slash",
                    displayName = "冲刺斩",
                    commandType = "DashSlash",
                    startupBeats = 0,
                    activeBeats = 1,
                    recoveryBeats = 1,
                    damage = 20
                },
                new MoveDefinitionModel
                {
                    moveId = "rising_strike",
                    displayName = "上挑斩",
                    commandType = "RisingStrike",
                    startupBeats = 0,
                    activeBeats = 1,
                    recoveryBeats = 2,
                    damage = 15
                }
            };
        }

        private void InitializeSystems()
        {
            // 初始化战斗系统
            if (combatSystem == null)
            {
                combatSystem = FindObjectOfType<CombatSystem>();
                if (combatSystem == null)
                {
                    combatSystem = gameObject.AddComponent<CombatSystem>();
                }
            }

            combatSystem.Initialize(_player, _enemy);

            // 订阅事件
            if (_player != null)
            {
                _player.OnDamaged += (dmg, hp) => Debug.Log($"[Player] 受伤 -{dmg}, 剩余 {hp} HP");
                _player.OnDeath += () => Debug.Log("[Player] 死亡！");
            }

            if (_enemy != null)
            {
                _enemy.OnDamaged += (dmg, hp) => Debug.Log($"[Enemy] 受伤 -{dmg}, 剩余 {hp} HP");
                _enemy.OnDeath += () => Debug.Log("[Enemy] 死亡！游戏胜利！");
            }
        }

        private void ResetBattle()
        {
            _player?.Reset();
            _enemy?.Reset();
            Debug.Log("[CombatTestRunner] 战斗已重置");
        }
    }
}