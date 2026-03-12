using UnityEngine;
using ShadowRhythm.Animation;
using ShadowRhythm.Command;
using ShadowRhythm.Fighter;

namespace ShadowRhythm.Debugging
{
    /// <summary>
    /// 动画测试运行器 - Sandbox_Animation 场景专用
    /// </summary>
    public class AnimationTestRunner : MonoBehaviour
    {
        [Header("依赖")]
        [SerializeField] private PuppetRigController rigController;
        [SerializeField] private CombatAnimationBridge animBridge;
        [SerializeField] private FighterRuntime fighterRuntime;

        [Header("测试设置")]
        [SerializeField] private bool useKeyboardShortcuts = true;

        private void Start()
        {
            // 自动查找组件
            if (rigController == null)
                rigController = FindObjectOfType<PuppetRigController>();
            if (animBridge == null)
                animBridge = FindObjectOfType<CombatAnimationBridge>();
            if (fighterRuntime == null)
                fighterRuntime = FindObjectOfType<FighterRuntime>();

            PrintInstructions();
        }

        private void Update()
        {
            if (!useKeyboardShortcuts) return;

            // 基础四键 - 使用方向键（与之前板块对齐）
            if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow) || UnityEngine.Input.GetKeyDown(KeyCode.W))
            {
                TestAnimation(CommandType.Lift, "提");
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow) ||  UnityEngine.Input.GetKeyDown(KeyCode.S))
            {
                TestAnimation(CommandType.Shake, "抖");
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow) || UnityEngine.Input.GetKeyDown(KeyCode.A))
            {
                TestAnimation(CommandType.Flick, "拨");
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow) || UnityEngine.Input.GetKeyDown(KeyCode.D))
            {
                TestAnimation(CommandType.Flash, "闪");
            }
            // 组合技测试
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
            {
                TestAnimation(CommandType.DashSlash, "突刺斩");
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
            {
                TestAnimation(CommandType.RisingStrike, "上挑斩");
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3))
            {
                TestAnimation(CommandType.ParryGuard, "弹反");
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha4))
            {
                TestAnimation(CommandType.ComboStab, "连刺");
            }
            // 特殊测试
            else if (UnityEngine.Input.GetKeyDown(KeyCode.H))
            {
                TestHitReaction();
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                TestReturnToIdle();
            }
            else if (UnityEngine. Input.GetKeyDown(KeyCode.R))
            {
                ResetAnimation();
            }
        }

        private void TestAnimation(CommandType commandType, string displayName)
        {
            Debug.Log($"[AnimationTest] 测试动画: {displayName} ({commandType})");

            if (animBridge != null)
            {
                animBridge.TriggerCommandAnimation(commandType);
            }
            else if (rigController != null)
            {
                rigController.PlayCommandAnimation(commandType);
            }
        }

        private void TestHitReaction()
        {
            Debug.Log("[AnimationTest] 测试受击动画");
            rigController?.PlayHitReaction();
        }

        private void TestReturnToIdle()
        {
            Debug.Log("[AnimationTest] 返回待机");
            rigController?.ReturnToIdle();
        }

        private void ResetAnimation()
        {
            Debug.Log("[AnimationTest] 重置动画状态");
            rigController?.ReturnToIdle();
        }

        private void PrintInstructions()
        {
            Debug.Log("═══════════════════════════════════════════════════");
            Debug.Log("       ANIMATION SANDBOX INITIALIZED       ");
            Debug.Log("═══════════════════════════════════════════════════");
            Debug.Log("基础四键（与之前板块对齐）:");
            Debug.Log("  ↑ / W = 提 (Lift)");
            Debug.Log("  ↓ / S = 抖 (Shake)");
            Debug.Log("  ← / A = 拨 (Flick)");
            Debug.Log("  → / D = 闪 (Flash)");
            Debug.Log("───────────────────────────────────────────────────");
            Debug.Log("组合技测试:");
            Debug.Log("  1 = 突刺斩 (DashSlash)   [拨+闪]");
            Debug.Log("  2 = 上挑斩 (RisingStrike) [拨+提]");
            Debug.Log("  3 = 弹反 (ParryGuard)    [拨+抖]");
            Debug.Log("  4 = 连刺 (ComboStab)     [拨→抖]");
            Debug.Log("───────────────────────────────────────────────────");
            Debug.Log("特殊操作:");
            Debug.Log("  H = 受击反馈 (HitReaction)");
            Debug.Log("  Space = 返回待机 (Idle)");
            Debug.Log("  R = 重置");
            Debug.Log("═══════════════════════════════════════════════════");
        }

        private void OnGUI()
        {
            // 左上角显示当前状态
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.BeginVertical("box");

            GUILayout.Label("═══ Animation Test ═══");

            if (rigController != null)
            {
                GUILayout.Label($"当前状态: {rigController.CurrentState}");
            }

            GUILayout.Space(10);
            GUILayout.Label("基础四键: ↑提 ↓抖 ←拨 →闪");
            GUILayout.Label("组合技: 1/2/3/4");
            GUILayout.Label("受击: H | 待机: Space");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}