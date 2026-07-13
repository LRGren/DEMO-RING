# DEMO RING — Unity 多人动作 RPG 代码学习指南

> **项目类型**: 类魂系（Souls-like）第三人称动作 RPG  
> **核心技术**: Unity 6 LTS + URP + Unity Netcode (Netcode for GameObjects)  
> **代码文件数**: ~80 个 C# 脚本  
> **教程来源**: 参考 YouTube 系列教程构建，适合 Unity 中高级学习者

---

## 目录

- [第一章：项目整体架构概览](#第一章项目整体架构概览)
- [第二章：角色系统 — 继承与组件化设计](#第二章角色系统--继承与组件化设计)
- [第三章：移动系统 — 物理、动画与输入](#第三章移动系统--物理动画与输入)
- [第四章：战斗系统 — 伤害、碰撞与连招](#第四章战斗系统--伤害碰撞与连招)
- [第五章：AI 系统 — ScriptableObject 状态机](#第五章AI-系统--ScriptableObject-状态机)
- [第六章：物品与武器系统](#第六章物品与武器系统)
- [第七章：多人联网架构](#第七章多人联网架构)
- [第八章：世界管理器与单例模式](#第八章世界管理器与单例模式)
- [第九章：UI、存档与菜单系统](#第九章UI存档与菜单系统)
- [第十章：初学者进阶 — 特殊代码库与设计模式分析](#第十章初学者进阶--特殊代码库与设计模式分析)

---

## 第一章：项目整体架构概览

### 1.1 目录结构总览

```
Assets/
├── Scripcts/                    ← ★ 核心游戏代码（学习重点）
│   ├── Animator/                # 动画事件辅助脚本
│   ├── Character/               # 角色系统
│   │   ├── Player/              # 玩家特有逻辑
│   │   │   └── PlayerUI/        # 玩家 HUD UI
│   │   └── AI Character/        # AI 角色
│   │       ├── Actions/         # AI 攻击动作定义
│   │       ├── States/          # AI 状态机
│   │       └── Undead Character/
│   ├── Colliders/               # 伤害碰撞体
│   ├── Effects/                 # 即时效果（伤害、体力伤害）
│   ├── Game Saving/             # 存档系统
│   ├── Item/                    # 物品/武器数据定义
│   ├── ManuScene/               # 主菜单场景
│   ├── UI/                      # 通用 UI 工具
│   ├── Utility/                 # 通用工具
│   ├── Weapon Action/           # 武器动作（轻/重攻击）
│   ├── WorldManager/            # 世界级管理器（单例）
│   ├── Enums.cs                 # 全局枚举定义
│   └── ...其他
├── Imports/                     # 第三方资源
├── Scenes/                      # 场景文件
├── Data/                        # ScriptableObject 数据资源
├── Art/                         # 美术资源
├── Settings/                    # URP 渲染设置
└── PlayerControls.cs            # Unity Input System 生成的输入映射类
```

### 1.2 系统间依赖关系图

```
                         ┌──────────────────────┐
                         │   World Managers      │
                         │  (Singleton 全局管理)  │
                         │  - SaveGame           │
                         │  - ItemDatabase       │
                         │  - SoundFX            │
                         │  - AIManager          │
                         │  - EffectsManager     │
                         │  - ActionManager      │
                         │  - UtilityManager     │
                         └──────┬───────────────┘
                                │ 依赖/调用
          ┌─────────────────────┼─────────────────────┐
          │                     │                     │
  ┌───────▼───────┐    ┌───────▼───────┐    ┌───────▼───────┐
  │ PlayerManager │    │AICharacterMgr │    │  UI System     │
  │   (继承自      │    │  (继承自       │    │  PlayerUIMgr   │
  │ CharacterMgr) │    │ CharacterMgr) │    │  TitleScreen   │
  └───────┬───────┘    └───────┬───────┘    └───────────────┘
          │                     │
  ┌───────▼─────────────────────▼───────┐
  │        CharacterManager             │
  │        (NetworkBehaviour)           │
  │  组合了以下组件:                      │
  │  - LocomotionManager (移动)          │
  │  - AnimatorManager (动画)            │
  │  - CombatManager (战斗)              │
  │  - NetworkManager (网络同步)          │
  │  - StatsManager (属性)               │
  │  - EffectsManager (效果)             │
  │  - SoundFXManager (音效)             │
  │  - EquipmentManager (装备)           │
  │  - InventoryManager (背包)           │
  └─────────────────────────────────────┘
```

---

## 第二章：角色系统 — 继承与组件化设计

### 2.1 核心继承链

这是项目最核心的继承体系，理解它等于理解了 50% 的架构：

```
NetworkBehaviour (Unity Netcode)
  └── CharacterManager          ← 所有角色的基类
        ├── PlayerManager       ← 玩家特有逻辑
        └── AICharacterManager  ← AI 特有逻辑 + 状态机
```

### 2.2 CharacterManager — 角色基类

**文件**: [CharacterManager.cs](Assets/Scripcts/Character/CharacterManager.cs)

```csharp
public class CharacterManager : NetworkBehaviour
```

**关键字段**:
| 字段 | 类型 | 说明 |
|------|------|------|
| `isDead` | `NetworkVariable<bool>` | 死亡状态（网络同步） |
| `characterController` | `CharacterController` | Unity 角色控制器 |
| `animator` | `Animator` | 动画控制器 |
| `isPerformingAction` | `bool` | 动作锁（施放动作时阻止其他输入） |
| `characterGroup` | `CharacterGroup` | 阵营（Team_01 / Team_02） |

**核心方法**:
- `Awake()` — 初始化所有组件引用 + `DontDestroyOnLoad`（角色跨场景保持）
- `Update()` — **网络位置同步**：Owner 上传位置，非 Owner 平滑插值
- `ProcessDeathEvent()` — 协程处理死亡流程（重置属性 → 播死亡动画 → 等待 → 虚化）
- `IgnoreMyOwnColliders()` — 自身碰撞体互忽略，防止武器碰到自己

> **⭐ 学习要点**: `NetworkVariable` 是 Unity Netcode 的核心概念，任何需要网络同步的变量都必须用它包装。它在值变化时自动同步到所有客户端。

### 2.3 PlayerManager — 玩家

**文件**: [PlayerManager.cs](Assets/Scripcts/Character/Player/PlayerManager.cs)

```csharp
public class PlayerManager : CharacterManager
```

**核心职责**:
1. **组件初始化** (`Awake`) — 获取所有 Player 特有的组件
2. **输入驱动** (`Update`) — `playerLocomotionManager.HandleAllMovement()` + 体力恢复
3. **网络回调绑定** (`OnNetworkSpawn`) — 大量 `OnValueChanged` 事件的订阅/取消
4. **存档桥接** — `SaveGameToCurrentCharacterData()` / `LoadGameFromCurrentCharacterData()`
5. **客户端加入处理** (`OnClientConnecterCallback`) — 当新客户端连接时加载其他玩家数据

**关键代码 — 网络回调绑定模式**:
```csharp
public override void OnNetworkSpawn()
{
    // 属性最大值变化 → 更新 HUD 最大值
    playerNetworkManager.vitality.OnValueChanged += playerNetworkManager.SetNewMaxHealthValue;
    playerNetworkManager.endurance.OnValueChanged += playerNetworkManager.SetNewMaxStaminaValue;
    
    // 当前值变化 → 更新 HUD 显示
    playerNetworkManager.currentHealth.OnValueChanged += PlayerUIManager.instance.playerUIHudManager.SetNewHealthValue;
    playerNetworkManager.currentStamina.OnValueChanged += PlayerUIManager.instance.playerUIHudManager.SetNewStaminaValue;
    playerNetworkManager.currentStamina.OnValueChanged += playerStatsManager.ResetStaminaTimer;
}
```

> **⭐ 学习要点**: `OnValueChanged` 是观察者模式的网络版实现，当网络变量改变时自动触发所有订阅的回调。必须在 `OnNetworkDespawn` 中取消订阅，防止内存泄漏。

### 2.4 组件化 Manager 体系

每个角色由多个专职 Manager 组件组成。以下表格展示所有组件及其职责：

| 组件 | 基类 | 职责 |
|------|------|------|
| `CharacterNetworkManager` | `NetworkBehaviour` | 网络同步：位置、动画参数、属性值、标记 |
| `CharacterLocomotionManager` | `MonoBehaviour` | 移动：重力、地面检测、旋转控制 |
| `CharacterAnimatorManager` | `MonoBehaviour` | 动画：参数更新、Root Motion、受击动画 |
| `CharacterStatsManager` | `MonoBehaviour` | 属性计算：体力/血量公式、体力恢复 |
| `CharacterCombatManager` | `NetworkBehaviour` | 战斗：锁定目标、攻击类型记录 |
| `CharacterEffectsManager` | `MonoBehaviour` | 效果处理：即时效果分发、血迹 VFX |
| `CharacterEquipmentManager` | `MonoBehaviour` | 装备基类（当前为空壳） |
| `CharacterInventoryManager` | `MonoBehaviour` | 背包基类（当前为空壳） |
| `CharacterSoundFXManager` | `MonoBehaviour` | 音效：受伤/攻击喊叫、随机音调 |

> **⭐ 学习要点**: 这是典型的 **组件模式（Component Pattern）**。每个 Manager 只负责一个领域，通过 `GetComponent<T>()` 互相引用，避免了庞大的单一类。

---

## 第三章：移动系统 — 物理、动画与输入

### 3.1 输入系统

**文件**: [PlayerInputManager.cs](Assets/Scripcts/Character/Player/PlayerInputManager.cs)

项目使用 **Unity Input System**（新版输入系统），通过 `PlayerControls.cs`（自动生成的 C# 包装类）处理所有输入。

**输入架构**:
```
硬件输入 → PlayerControls (C# 类) → PlayerInputManager (处理) → 各系统执行
```

**核心输入映射**:
| 输入 | Input Action | 目标方法 |
|------|-------------|----------|
| 移动摇杆 | `PlayerMovement.Movement` | `HandlePlayerMovementInput()` |
| 视角旋转 | `PlayerCamera.Movement` | `HandleCameraMovementInput()` |
| 翻滚/后跳 | `PlayerActions.Dodge` | `HandleDodgeInput()` |
| 跳跃 | `PlayerActions.Jump` | `HandleJumpInput()` |
| 疾跑 | `PlayerActions.Sprint` | `HandleSprintInput()` |
| RB（轻攻击） | `PlayerActions.RB` | `HandleRBInput()` |
| RT（重攻击） | `PlayerActions.RT` | `HandleRTInput()` |
| 锁定 | `PlayerActions.LockOn` | `HandleLockOnInput()` |

**场景感知的输入启用**:
```csharp
private void OnSceneChange(Scene oldScene, Scene newScene)
{
    if (newScene.buildIndex == WorldSaveGameManager.instance.GetWorldSceneIndex())
    {
        instance.enabled = true;   // 游戏场景 → 启用输入
    }
    else
    {
        instance.enabled = false;  // 菜单场景 → 禁用输入
    }
}
```

> **⭐ 学习要点**: Input System 使用 **事件驱动** 而非轮询（`performed` / `canceled` 事件），性能更好。`DontDestroyOnLoad` 确保 InputManager 跨场景存活，但通过 `OnSceneChange` 控制启用/禁用。

### 3.2 PlayerLocomotionManager — 移动核心

**文件**: [PlayerLocomotionManager.cs](Assets/Scripcts/Character/Player/PlayerLocomotionManager.cs)

```csharp
public class PlayerLocomotionManager : CharacterLocomotionManager
```

**移动速度分层**:
```csharp
walkingSpeed = 1.5f;     // 行走（输入量 ≤ 0.5）
runningSpeed = 4.5f;     // 奔跑（输入量 > 0.5）
sprintingSpeed = 7f;     // 疾跑（按住冲刺键）
freeFallSpeed = 2f;      // 空中移动
```

**旋转逻辑** (`HandleRotation()`) 分三种情况:
1. **锁定 + 疾跑/翻滚**: 朝向移动方向
2. **锁定 + 非疾跑**: 始终面向锁定目标
3. **非锁定**: 朝向摄像机前方向量计算的方向

**跳跃系统** (`AttemptToPerformJump()`):
- 检查动作锁、体力、是否已在跳跃、是否着地
- 根据移动状态（疾跑/奔跑/行走）缩放跳跃距离
- 消耗体力 (`jumpStaminaCost = 10`)

**翻滚/后跳** (`AttemptToPerformDodge()`):
- 有移动输入 → 向移动方向翻滚（`rollDirection`）
- 无移动输入 → 后跳（`Back_Step_01`）
- 消耗体力 (`dodgeStaminaCost = 25`)

### 3.3 基类 CharacterLocomotionManager — 重力与地面

**文件**: [CharacterLocomotionManager.cs](Assets/Scripcts/Character/CharacterLocomotionManager.cs)

**重力模拟**:
```csharp
gravityForce = -40f;
groundedYVelocity = -20;   // 着地时的 Y 速度（确保贴地）
fallStartYVelocity = -5;   // 开始下落时的初速度
```

**地面检测**: 使用 `Physics.CheckSphere` 球体检测
```csharp
isGrounded = Physics.CheckSphere(character.transform.position, groundCheckSphereRadius, groundLayer);
```

**下落检测逻辑**:
1. 每次 Update 检测是否着地
2. 若离地且不在跳跃中 → 设置下落初速度
3. 累积 `inAirTimer` → 传给 Animator（用于空中动画混合）
4. 每帧应用重力加速度

### 3.4 PlayerCamera — 第三人称摄像机

**文件**: [PlayerCamera.cs](Assets/Scripcts/Character/Player/PlayerCamera.cs)

**核心功能**:
- **跟随**: `Vector3.SmoothDamp` 平滑跟随玩家
- **旋转**: 水平/垂直双轴旋转（`leftAndRightAngle` / `upAndDownAngle`）
- **碰撞检测**: `Physics.SphereCast` 检测摄像机与障碍物
- **锁定系统**: 智能搜索最近/左/右目标

**锁定目标搜索** (`HandleLocatingLockOnTargets()`):
```csharp
// 1. Physics.OverlapSphere 在锁定半径内搜索所有碰撞体
// 2. 过滤条件:
//    - 目标未死亡
//    - 目标不是自己
//    - 目标在视野角度内 (minimumLockOnAngle ~ maximumLockOnAngle)
//    - 无遮挡 (Physics.Linecast 检测)
// 3. 按距离排序，找出 nearest/left/right 三个目标
```

### 3.5 动画系统

**文件**: [CharacterAnimatorManager.cs](Assets/Scripcts/Character/CharacterAnimatorManager.cs)

**运动参数快照** (`UpdateAnimatorMovementParameters`):
```csharp
// 将连续输入值量化为离散值 (-1, -0.5, 0, 0.5, 1)，适合 Blend Tree
if (horizontalValue > 0 && horizontalValue < 0.55f) snappedHorizontal = 0.5f;
else if (horizontalValue > 0.55f) snappedHorizontal = 1f;
// ...
if (isSprinting) snappedVertical = 2f;  // 冲刺覆盖为 2
```

**动作动画播放** (`PlayerTargetActionAnimation`):
```csharp
// 1. 设置 applyRootMotion
// 2. CrossFade 到目标动画 (0.2s 过渡)
// 3. 设置动作锁 (isPerformingAction)
// 4. 冻结旋转/移动 (canRotate/canMove)
// 5. 通过网络 RPC 同步动画到所有客户端
```

**文件**: [PlayerAnimatorManager.cs](Assets/Scripcts/Character/Player/PlayerAnimatorManager.cs)

**Root Motion 处理** (`OnAnimatorMove`):
```csharp
private void OnAnimatorMove()
{
    if (player.characterAnimatorManager.applyRootMotion)
    {
        Vector3 velocity = player.animator.deltaPosition;
        player.characterController.Move(velocity);
        player.transform.rotation *= player.animator.deltaRotation;
    }
}
```

> **⭐ 学习要点**: `OnAnimatorMove` 在 Animator 更新后、物理更新前调用。Root Motion 让动画驱动角色移动（而非代码），适合攻击动作等需要精确位置控制的场景。

---

## 第四章：战斗系统 — 伤害、碰撞与连招

### 4.1 伤害流程总览

```
武器碰撞体命中
  → DamageCollider.OnTriggerEnter()
    → 检查是否已伤害过该目标
    → 创建 TakeDamageEffect 实例
    → 根据攻击类型应用伤害修饰符
    → (攻击者 IsOwner) 通过 RPC 发送到服务器
      → CharacterNetworkManager.NotifyTheServerOfCharacterDamageServerRpc()
        → (服务器) 广播到所有客户端
          → 受击者本地 ProcessCharacterDamageFromServer()
            → characterEffectsManager.ProcessInstantEffect(damageEffect)
              → TakeDamageEffect.ProcessEffect()
                → CalculteDamage() → 扣除血量
                → PlayDirectionalBasedDamageAnimation() → 播放受击动画
                → PlayDamageSFX() / PlayDamageVFX() → 音效特效
```

### 4.2 DamageCollider — 伤害碰撞体基类

**文件**: [DamageCollider.cs](Assets/Scripcts/Colliders/DamageCollider.cs)

**关键设计**:
- **伤害列表去重**: `List<CharacterManager> characterDamaged` 防止同一次攻击对同一目标多次伤害
- **碰撞体生命周期**: 默认关闭，由动画事件开启/关闭
- **五种伤害类型**: 物理、魔法、火焰、雷电、神圣

### 4.3 MeleeWeaponDamageCollider — 玩家武器伤害

**文件**: [MeleeWeaponDamageCollider.cs](Assets/Scripcts/Colliders/MeleeWeaponDamageCollider.cs)

**攻击修饰符系统**:
```csharp
switch (characterCasuingDamage.characterCombatManager.currentAttackType)
{
    case AttackType.LightAttack01: ApplyAttackModifier(light_Attack_01_Modifier, damageEffect); break;
    case AttackType.HeavyAttack01: ApplyAttackModifier(heavy_Attack_01_Modifier, damageEffect); break;
    case AttackType.ChargedAttack01: ApplyAttackModifier(charged_Attack_01_Modifier, damageEffect); break;
    // ...
}
```

### 4.4 TakeDamageEffect — 伤害计算核心

**文件**: [TakeDamageEffect.cs](Assets/Scripcts/Effects/TakeDamageEffect.cs)

**方向性受击动画** (基于 `angleHitFrom`):
```csharp
// -180 ~ -145 或 145 ~ 180  → 正面受击
// -45 ~ 45                    → 背面受击
// -144 ~ -45                  → 左侧受击
// 45 ~ 144                    → 右侧受击
```

### 4.5 连招系统

**文件**: [PlayerAnimatorManager.cs](Assets/Scripcts/Character/Player/PlayerAnimatorManager.cs)

```csharp
// 动画事件 EnableDoCombo() → 允许连招
public override void EnableDoCombo()
{
    player.playerCombatManager.canComboWithMainHandWeapon = true;
}
// 动画事件 DisableDoCombo() → 禁止连招
public override void DisableDoCombo()
{
    player.playerCombatManager.canComboWithMainHandWeapon = false;
}
```

**文件**: [LightAttackWeaponItemAction.cs](Assets/Scripcts/Weapon Action/LightAttackWeaponItemAction.cs)

```csharp
// 连招逻辑: 在允许连招窗口内再次按攻击键 → 交替播放 LightAttack01/02
if (canComboWithMainHandWeapon && isPerformingAction)
{
    if (lastAttackAnimation == light_Attack_01)
        PlayerTargetAttackActionAnimation(LightAttack02, light_Attack_02, true);
    else if (lastAttackAnimation == light_Attack_02)
        PlayerTargetAttackActionAnimation(LightAttack01, light_Attack_01, true);
}
```

> **⭐ 学习要点**: 连招窗口通过 **动画事件（Animation Event）** 控制。动画播放到特定帧时调用 `EnableDoCombo()`，在另一个帧调用 `DisableDoCombo()`。这确保了玩家只能在正确的帧间隔内触发连招。

---

## 第五章：AI 系统 — ScriptableObject 状态机

### 5.1 AI 架构概览

这是项目**最具学习价值的子系统**之一，展示了如何使用 ScriptableObject 实现灵活的状态机。

```
AICharacterManager
  ├── NavMeshAgent (Unity 导航)
  ├── AICharacterCombatManager (AI 战斗逻辑)
  ├── AICharacterLocomotionManager (AI 移动)
  └── 状态机 (通过 ScriptableObject 定义)
        ├── IdleState       → 待机/搜索目标
        ├── PursueTargetState → 追击
        ├── CombatStanceState → 战斗姿态（选择攻击）
        └── AttackState     → 执行攻击
```

### 5.2 AIState — 状态基类

**文件**: [AIState.cs](Assets/Scripcts/Character/AI Character/States/AIState.cs)

```csharp
public class AIState : ScriptableObject
{
    public virtual AIState Tick(AICharacterManager aiCharacterManager)
    {
        return this;  // 默认返回自身（保持当前状态）
    }

    protected virtual AIState SwitchState(AICharacterManager aiCharacterManager, AIState nextState)
    {
        ResetStateFlags(aiCharacterManager);  // 清理状态标记
        return nextState;
    }
}
```

> **⭐ 学习要点**: 使用 **ScriptableObject** 而不是 `enum` + `switch` 实现状态机的好处：
> 1. 每个状态是独立文件，代码清晰
> 2. 可在 Inspector 中配置状态参数
> 3. 方便创建新状态而不修改现有代码（开闭原则）
> 4. 状态可复用

### 5.3 状态机运行流程

**文件**: [AICharacterManager.cs](Assets/Scripcts/Character/AI Character/AICharacterManager.cs)

```csharp
private void ProcessStateMachine()
{
    AIState nextState = currentState?.Tick(this);
    if (nextState != null)
    {
        currentState = nextState;
    }
    // 更新目标方向、角度、距离信息
}
```

### 5.4 各状态详解

#### IdleState（待机）

**文件**: [IdleState.cs](Assets/Scripcts/Character/AI Character/States/IdleState.cs)

```csharp
public override AIState Tick(AICharacterManager aiCharacterManager)
{
    if (aiCharacterManager.characterCombatManager.currentTarget != null)
        return SwitchState(aiCharacterManager, aiCharacterManager.pursueTarget);
    else
    {
        aiCharacterManager.aiCharacterCombatManager.FindATargetViaLineOfSight(aiCharacterManager);
        return this;  // 保持待机，继续搜索
    }
}
```

#### PursueTargetState（追击）

**文件**: [PursueTargetState.cs](Assets/Scripcts/Character/AI Character/States/PursueTargetState.cs)

```csharp
// 1. 检查是否在播放动作（是 → 等待）
// 2. 检查是否有目标（否 → 回到 Idle）
// 3. 启用 NavMeshAgent
// 4. 如果目标不在视野内 → 转向目标
// 5. 如果在攻击距离内 → 切换到 CombatStance
// 6. 否则 → 设置 NavMesh 路径继续追击
```

#### CombatStanceState（战斗姿态）— 核心 AI 逻辑

**文件**: [CombatStanceState.cs](Assets/Scripcts/Character/AI Character/States/CombatStanceState.cs)

**加权随机攻击选择**:
```csharp
// 1. 遍历所有攻击动作，过滤出满足角度和距离条件的
// 2. 使用 attackWeigth 作为权重进行加权随机
int totalWeigth = 0;
foreach (var attack in potentialAttacks) totalWeigth += attack.attackWeigth;

int randomValue = Random.Range(1, totalWeigth + 1);
int processWeight = 0;
foreach (var attack in potentialAttacks)
{
    processWeight += attack.attackWeigth;
    if (processWeight >= randomValue)
    {
        chosenAttack = attack;
        hasAttacked = true;
        return;
    }
}
```

> **⭐ 学习要点**: 加权随机比纯随机更可控 — 策划可以调整每个攻击的 `attackWeigth` 来控制 AI 的行为分布。

#### AttackState（攻击）

**文件**: [AttackState.cs](Assets/Scripcts/Character/AI Character/States/AttackState.cs)

```csharp
// 1. 检查是否有目标（否 → 回到 Idle）
// 2. 攻击时持续面向目标 (RotateTowardsTargetWhilstAttacking)
// 3. 等待动作恢复计时器
// 4. 执行攻击 → 设置 recovery timer
// 5. 攻击完成后 → 回到 CombatStance
```

### 5.5 AICharacterAttackAction — ScriptableObject 攻击数据

**文件**: [AICharacterAttackAction.cs](Assets/Scripcts/Character/AI Character/Actions/AICharacterAttackAction.cs)

```csharp
[CreateAssetMenu(menuName = "A.I/Actions/Attack")]
public class AICharacterAttackAction : ScriptableObject
{
    public string actionAnimation;           // 动画名称
    public AttackType attackType;            // 攻击类型
    public int attackWeigth = 50;            // 权重
    public float attackRecoveryTime = 1.5f;  // 恢复时间
    public float maximumAttackAngle = 35f;   // 最大攻击角度
    public float minimumAttackAngle = -35f;  // 最小攻击角度
    public float maximumAttackDistance = 3f; // 最大攻击距离
    public float minimumAttackDistance = 0f; // 最小攻击距离
}
```

> **⭐ 学习要点**: 攻击动作也是 ScriptableObject！策划可以在 Editor 中通过 Create Asset Menu 创建新攻击，配置参数后拖拽到 AI 的 `aiCharacterAttacks` 列表中即可生效。

### 5.6 AIUndeadCombatManager — 具体 AI 实现

**文件**: [AIUndeadCombatManager.cs](Assets/Scripcts/Character/AI Character/Undead Character/AIUndeadCombatManager.cs)

```csharp
public class AIUndeadCombatManager : AICharacterCombatManager
{
    // 攻击伤害 = 基础伤害 × 攻击修饰符
    public void SetAttack01Damage() { damage = baseDamage * attack01Modifier; }  // ×1.0
    public void SetAttack02Damage() { damage = baseDamage * attack02Modifier; }  // ×1.4
    
    // 配合动画事件：在攻击帧开启碰撞体 + 播放音效
    public void EnableRightHandDamageCollider() { ... PlayAttackGruntSFX(); ... }
    public void DisableRightHandDamageCollider() { ... }
}
```

---

## 第六章：物品与武器系统

### 6.1 ScriptableObject 继承链

```
ScriptableObject
  └── Item                        ← 基础物品数据
        └── WeaponItem            ← 武器数据 + 动作/伤害/耐力修饰
              └── MeleeWeaponItem ← 近战武器（弹刀、附魔预留）
```

### 6.2 Item.cs — 物品基类

**文件**: [Item.cs](Assets/Scripcts/Item/Item.cs)

```csharp
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public string itemDescription;  // [TextArea] 多行文本
    public int itemID;
}
```

### 6.3 WeaponItem.cs — 武器数据

**文件**: [WeaponItem.cs](Assets/Scripcts/Item/WeaponItem.cs)

```csharp
// 伤害属性（5 种类型）
public int physicalDamage, magicalDamage, fireDamage, holyDamage, lightningDamage;

// 攻击修饰符（6 种攻击类型 × 各自的倍率）
public float light_Attack_01_Modifier = 0.9f;
public float charged_Attack_02_Modifier = 2.5f;

// 耐力消耗
public int basicStaminaCost;
public float lightAttackStaminaModifier, heavyAttackStaminaModifier, chargedAttackStaminaModifier;

// 绑定的动作（ScriptableObject 引用）
public WeaponItemAction oh_RB_Action;  // 单手 RB
public WeaponItemAction oh_RT_Action;  // 单手 RT
```

### 6.4 WeaponItemAction — 武器动作多态

**文件**: [WeaponItemAction.cs](Assets/Scripcts/Weapon Action/WeaponItemAction.cs)

```csharp
public class WeaponItemAction : ScriptableObject
{
    public int actionID;
    
    public virtual void AttemptToPerformAction(PlayerManager player, WeaponItem weapon)
    {
        // 设置当前使用的武器 ID → 触发网络同步
        player.playerNetworkManager.currentWeaponBeingUsed.Value = weapon.itemID;
    }
}
```

**轻攻击** [LightAttackWeaponItemAction.cs](Assets/Scripcts/Weapon Action/LightAttackWeaponItemAction.cs):
```csharp
// 检查条件后执行连招切换
if (canComboWithMainHandWeapon && isPerformingAction)
    // 交替播放 LightAttack01 / LightAttack02
else if (!isPerformingAction)
    // 播放 LightAttack01
```

**重攻击** [HeavyAttackWeaponItemAction.cs](Assets/Scripcts/Weapon Action/HeavyAttackWeaponItemAction.cs):
```csharp
// 类似轻攻击结构，使用 HeavyAttack01 / HeavyAttack02
```

> **⭐ 学习要点**: 这是 **策略模式（Strategy Pattern）** 的应用。每种武器动作是一个 ScriptableObject 策略，武器通过引用 `oh_RB_Action` / `oh_RT_Action` 绑定不同的动作。不同武器可以绑定不同的攻击动作 ScriptableObject。

### 6.5 武器切换系统

**文件**: [PlayerEquipmentManager.cs](Assets/Scripcts/Character/Player/PlayerEquipmentManager.cs)

```csharp
// 右手武器切换逻辑:
// 1. 播切换动画
// 2. rightWeaponIndex++（循环 0→1→2→0）
// 3. 如果槽位是空手 → 跳过或装备 unarmedWeapon
// 4. 如果只有一把武器 → 切换到空手
// 5. 设置 currentRightHandWeaponID → 触发网络同步
```

> **⭐ 学习要点**: 武器切换通过 `NetworkVariable<int>` 的 `OnValueChanged` 触发，而非直接操作模型。当 `currentRightHandWeaponID` 改变时，所有客户端自动执行 `OnCurrentRightHandWeaponIDChanged` → 加载对应武器模型。

---

## 第七章：多人联网架构

### 7.1 网络架构模型

该项目使用 **Unity Netcode for GameObjects (NGO)** 的 **Server-Host 模式**：

```
        ┌──────────────────────────┐
        │     Host (Server+Client)  │
        │  - 运行完整游戏逻辑        │
        │  - 权威 AI 生成           │
        │  - 处理伤害 RPC           │
        └─────────┬────────────────┘
                  │
        ┌─────────┴────────────────┐
        │                          │
  ┌─────▼─────┐            ┌──────▼─────┐
  │ Client 1  │            │ Client 2   │
  │ - 本地渲染 │            │ - 本地渲染  │
  │ - 输入上传 │            │ - 输入上传  │
  └───────────┘            └────────────┘
```

### 7.2 网络变量 (NetworkVariable)

**文件**: [CharacterNetworkManager.cs](Assets/Scripcts/Character/CharacterNetworkManager.cs)

```csharp
// 声明: 默认值 + 读写权限
public NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>(
    Vector3.zero,
    NetworkVariableReadPermission.Everyone,   // 所有人可读
    NetworkVariableWritePermission.Owner       // 仅 Owner 可写
);
```

**项目中使用的 NetworkVariable 类型**:
- `Vector3` — 位置
- `Quaternion` — 旋转
- `float` — 移动参数、体力
- `int` — 血量、属性、武器 ID
- `bool` — isMoving, isJumping, isSprinting, isLockOn 等
- `ulong` — 锁定目标网络 ID
- `FixedString64Bytes` — 角色名称

### 7.3 RPC 远程过程调用

**ServerRpc** (客户端 → 服务器):
```csharp
[ServerRpc]
public void NotifyTheServerOfActionAnimationServerRpc(ulong clientId, string animationName, bool applyRootMotion)
{
    if (IsServer)
    {
        PlayActionAnimationForAllClientsClientRpc(clientId, animationName, applyRootMotion);
    }
}
```

**ClientRpc** (服务器 → 所有客户端):
```csharp
[ClientRpc]
public void PlayActionAnimationForAllClientsClientRpc(ulong clientId, string animationName, bool applyRootMotion)
{
    // 排除发送者：只有非本地客户端才执行
    if (clientId != NetworkManager.Singleton.LocalClientId)
    {
        PerformActionAnimationFromServer(animationName, applyRootMotion);
    }
}
```

**伤害同步的完整 RPC 链**:
```
客户端A攻击命中 → ServerRpc(伤害参数) → 服务器验证 → ClientRpc → 所有客户端处理伤害
```

### 7.4 网络位置同步

**Owner 上传位置** (CharacterManager.Update):
```csharp
if (IsOwner)
{
    characterNetworkManager.networkPosition.Value = transform.position;
    characterNetworkManager.networkRotation.Value = transform.rotation;
}
```

**非 Owner 插值** (CharacterManager.Update):
```csharp
else
{
    transform.position = Vector3.SmoothDamp(
        transform.position,
        characterNetworkManager.networkPosition.Value,
        ref characterNetworkManager.networkPositionVelocity,
        characterNetworkManager.networkPositionSmoothTime);  // 0.1s 平滑
    
    transform.rotation = Quaternion.Slerp(
        transform.rotation,
        characterNetworkManager.networkRotation.Value,
        characterNetworkManager.networkRotationSmoothTime);
}
```

### 7.5 AI 网络生成

**文件**: [WorldAIManager.cs](Assets/Scripcts/WorldManager/WorldAIManager.cs)

```csharp
// 仅在服务器端生成 AI
if (NetworkManager.Singleton.IsServer)
{
    foreach (var character in aiCharacters)
    {
        GameObject instance = Instantiate(character);
        instance.GetComponent<NetworkObject>().Spawn();  // 网络生成
        spawnedInCharacters.Add(instance);
    }
}
```

> **⭐ 学习要点**: `NetworkObject.Spawn()` 是服务器生成网络对象的入口。生成后 Netcode 自动处理所有客户端的同步。

---

## 第八章：世界管理器与单例模式

### 8.1 单例管理器一览

项目中几乎所有世界级管理器都使用 **Monobehaviour 单例模式**：

| 管理器 | 功能 |
|--------|------|
| `WorldGameSessionManager` | 活跃玩家列表管理 |
| `WorldSaveGameManager` | 存档/读档/删档 |
| `WorldActionManager` | 武器动作注册与查询 |
| `WorldItemDatabase` | 物品/武器数据注册与查询 |
| `WorldAIManager` | AI 角色生成/销毁 |
| `WorldSoundFXManager` | 共享音效资源 + 随机选择 |
| `WorldUtilityManager` | Layer 管理 + 阵营判定 + 角度计算 |
| `WorldCharacterEffectsManager` | 效果模板 + VFX 预设 |

### 8.2 单例实现模式

```csharp
public class WorldSoundFXManager : MonoBehaviour
{
    public static WorldSoundFXManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);  // 防止重复实例
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);  // 跨场景保持
    }
}
```

### 8.3 WorldSaveGameManager — 存档系统

**文件**: [WorldSaveGameManager.cs](Assets/Scripcts/WorldManager/WorldSaveGameManager.cs)

**存档流程**:
```
SaveGame()
  → player.SaveGameToCurrentCharacterData(ref currentCharacterData)
    → 写入场景索引、名字、位置、血量、体力、属性
  → SaveFileDataWriter.CreateNewCharacterSaveFile(data)
    → JsonUtility.ToJson(data) → FileStream 写入磁盘
```

**10 个角色槽位**: `CharacterSlot_01` ~ `CharacterSlot_10`

**新游戏流程** (`AttemptToCreateNewGame`):
```
遍历 10 个槽位 → 找第一个空槽 → 设置初始属性 → SaveGame → LoadWorldScene
```

### 8.4 SaveFileDataWriter — JSON 文件读写

**文件**: [SaveFileDataWriter.cs](Assets/Scripcts/Game Saving/SaveFileDataWriter.cs)

```csharp
// 写入: JsonUtility.ToJson(characterData) → FileStream.Write
// 读取: FileStream.Read → JsonUtility.FromJson<CharacterSaveData>(data)
// 路径: Application.persistentDataPath + 文件名
```

### 8.5 WorldItemDatabase — 数据库注册

**文件**: [WorldItemDatabase.cs](Assets/Scripcts/WorldManager/WorldItemDatabase.cs)

```csharp
private void Awake()
{
    // 自动分配 ID
    for (int i = 0; i < items.Count; i++)
        items[i].itemID = i;
}

public WeaponItem GetWeaponByID(int id)
{
    return weapons.FirstOrDefault(w => w.itemID == id);
}
```

### 8.6 WorldUtilityManager — 工具方法

**文件**: [WorldUtilityManager.cs](Assets/Scripcts/WorldManager/WorldUtilityManager.cs)

**阵营判定** (`CanIDamageThisTarget`):
```csharp
// Team_01 不能伤害 Team_01，可以伤害 Team_02
// Team_02 不能伤害 Team_02，可以伤害 Team_01
```

**带符号的角度计算** (`GetAngleOfTarget`):
```csharp
// 使用 Vector3.Angle + Vector3.Cross 判断方向
// cross.y < 0 → 目标在左侧（负角度）
// cross.y > 0 → 目标在右侧（正角度）
```

---

## 第九章：UI、存档与菜单系统

### 9.1 UI 系统

**文件**: [PlayerUIManager.cs](Assets/Scripcts/Character/Player/PlayerUI/PlayerUIManager.cs)

```csharp
public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager instance;
    public PlayerUIHudManager playerUIHudManager;    // 血条/体力条/快捷栏
    public PlayerUIPopUpManager playerUIPopUpManager; // 弹窗 ("YOU DIED")
}
```

### 9.2 PlayerUIHudManager — HUD

**文件**: [PlayerUIHudManager.cs](Assets/Scripcts/Character/Player/PlayerUI/PlayerUIHudManager.cs)

```csharp
// 血条更新
SetNewHealthValue(int oldValue, int newValue) → healthBar.SetStat(newValue)
SetMaxHealthValue(int maxHealth) → healthBar.SetMaxStat(maxHealth)

// 武器快捷栏
SetRightWeaponQuickSlot(int weaponID)
  → WorldItemDatabase.Instance.GetWeaponByID(weaponID)
    → 显示武器图标
```

### 9.3 PlayerUIPopUpManager — 弹窗系统

**文件**: [PlayerUIPopUpManager.cs](Assets/Scripcts/Character/Player/PlayerUI/PlayerUIPopUpManager.cs)

**"YOU DIED" 效果**（协程组合）:
```csharp
// 三个协程并行:
// 1. StretchPopUpTextOverTime  → 文字拉伸效果 (0 → 8.32 spacing, 8秒)
// 2. FadeInPopUpOverTime       → 渐入 (5秒)
// 3. WaitThenFadeOutPopUpOverTime → 等待 2秒 → 渐出 (5秒)
```

> **⭐ 学习要点**: `StartCoroutine` 的三个协程同时启动，互不阻塞。CanvasGroup 比直接修改 Image.color 更适合控制整个 UI 组的透明度。

### 9.4 TitleScreenManager — 主菜单

**文件**: [TitleScreenManager.cs](Assets/Scripcts/ManuScene/TitleScreenManager.cs)
**文件**: [TitleScreenLoadMenuInputManager.cs](Assets/Scripcts/ManuScene/TitleScreenLoadMenuInputManager.cs)

主菜单处理新游戏、加载游戏、删除存档等交互。

---

## 第十章：初学者进阶 — 特殊代码库与设计模式分析

### 10.1 设计模式总结

| 模式 | 应用位置 | 说明 |
|------|---------|------|
| **单例 (Singleton)** | 所有 WorldManager, PlayerUIManager, PlayerInputManager | 全局唯一访问点 |
| **组件模式** | CharacterManager + 各子 Manager | 职责分离，通过 GetComponent 组合 |
| **状态模式** | AI 状态机 (Idle/Pursue/CombatStance/Attack) | ScriptableObject 实现 |
| **模板方法模式** | CharacterManager.Update/FixedUpdate (virtual) | 基类定义骨架，子类重写 |
| **策略模式** | WeaponItemAction 体系 | 不同武器绑定不同动作策略 |
| **观察者模式** | NetworkVariable.OnValueChanged | 网络变量变化的自动通知 |
| **数据驱动** | Item/WeaponItem/WeaponItemAction/AIState 等全部为 ScriptableObject | 策划可通过 Inspector 配置游戏数据 |

### 10.2 Unity Netcode 关键知识点

**1. NetworkBehaviour 生命周期**:
```
Awake() → OnNetworkSpawn() → Start() → Update() → ... → OnNetworkDespawn()
```

**2. IsOwner 检查** — 项目中几乎所有客户端逻辑都包裹在 `if (IsOwner)` 中：
```csharp
if (!IsOwner) return;  // 非本地玩家不处理输入、UI 更新等
```

**3. 网络变量的权限控制**:
- `NetworkVariableReadPermission.Everyone` — 所有客户端可读
- `NetworkVariableWritePermission.Owner` — 仅 Owner 可写
- `[ServerRpc(RequireOwnership = false)]` — 非 Owner 也能调用（伤害 RPC）

### 10.3 体力系统

**文件**: [CharacterStatsManager.cs](Assets/Scripcts/Character/CharacterStatsManager.cs)

**体力公式**: `耐力等级 × 15`

**体力恢复机制**:
```csharp
// 延迟 5 秒后开始恢复
// 每 0.1 秒恢复 5 点
// 条件: 非冲刺、非执行动作
staminaRegenerationTimer += Time.deltaTime;
if (staminaRegenerationTimer >= staminaRegenerationDelay)  // 5s
    staminaRegenerationTicker += Time.deltaTime;
    if (staminaRegenerationTicker > 0.1f)
        currentStamina += staminaRegenerationAmount;  // +5
```

**体力惩罚**:
- 冲刺: 每秒消耗 2 点
- 翻滚: 一次 25 点
- 跳跃: 一次 10 点
- 攻击: 基础消耗 × 攻击类型修饰符

### 10.4 动画事件驱动的战斗系统

动画事件是战斗系统的核心触发机制：

```
攻击动画播放到特定帧:
  → 动画事件: EnableDoCombo()         → 开启连招窗口
  → 动画事件: OpenDamageCollider()     → 开启武器碰撞体
  → 动画事件: DisableDoCombo()         → 关闭连招窗口
  → 动画事件: CloseDamageCollider()    → 关闭武器碰撞体
```

**文件**: [ResetActionFlags.cs](Assets/Scripcts/Animator/ResetActionFlags.cs) / [ResetIsJumping.cs](Assets/Scripcts/Animator/ResetIsJumping.cs) / [ToggleAttackType.cs](Assets/Scripcts/Animator/ToggleAttackType.cs)

这些脚本挂在动画事件上，在动画结束时重置角色状态标记。

### 10.5 Enums.cs — 全局枚举

**文件**: [Enums.cs](Assets/Scripcts/Enums.cs)

```csharp
public enum CharacterSlot { CharacterSlot_01, ..., CharacterSlot_10, NO_SLOT }
public enum CharacterGroup { Team_01, Team_02 }
public enum WeaponModelSlot { RightHand, LeftHand }
public enum AttackType { LightAttack01, LightAttack02, HeavyAttack01, HeavyAttack02, ChargedAttack01, ChargedAttack02 }
```

### 10.6 PlayerControls.cs — Input System 自动生成

**文件**: [PlayerControls.cs](Assets/PlayerControls.cs)

这是 Unity Input System 自动生成的 C# 类。通过 `.inputactions` 资源编译生成，提供强类型的输入访问：

```csharp
playerControls.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();
playerControls.PlayerActions.Dodge.performed += i => dodge_Input = true;
playerControls.PlayerCamera.Movement.performed += i => cameraInput = i.ReadValue<Vector2>();
```

### 10.7 进阶优化建议

1. **对象池**: 当前的 Effect（TakeDamageEffect）每次都 `Instantiate`，在高频战斗场景下会产生 GC 压力
2. **动画哈希**: 项目已使用 `Animator.StringToHash`，这是最佳实践
3. **输入缓冲**: 当前输入是布尔标记 + 轮询，可考虑添加输入缓冲系统提升手感
4. **AI 索敌优化**: `FindATargetViaLineOfSight` 目前通过碰撞体检测，可添加 LOD 分级来减少远距离 AI 的检测频率

---

## 附录：核心代码文件速查表

### 角色系统
| 文件 | 基类 | 职责 |
|------|------|------|
| `CharacterManager.cs` | `NetworkBehaviour` | 角色基类，网络生命周期 |
| `PlayerManager.cs` | `CharacterManager` | 玩家初始化/存档/网络回调 |
| `AICharacterManager.cs` | `CharacterManager` | AI 状态机驱动 |
| `CharacterNetworkManager.cs` | `NetworkBehaviour` | 网络变量定义 + RPC |
| `CharacterLocomotionManager.cs` | `MonoBehaviour` | 重力/地面检测 |
| `CharacterAnimatorManager.cs` | `MonoBehaviour` | 动画参数/受击动画 |
| `CharacterStatsManager.cs` | `MonoBehaviour` | 体力恢复/属性公式 |
| `CharacterCombatManager.cs` | `NetworkBehaviour` | 锁定目标 |
| `CharacterEffectsManager.cs` | `MonoBehaviour` | 即时效果处理 |
| `CharacterSoundFXManager.cs` | `MonoBehaviour` | 音效播放 |
| `PlayerLocomotionManager.cs` | `CharacterLocomotionManager` | 移动/跳跃/翻滚/旋转 |
| `PlayerAnimatorManager.cs` | `CharacterAnimatorManager` | Root Motion / 连招标记 |
| `PlayerInputManager.cs` | `MonoBehaviour` | 输入处理（单例） |
| `PlayerCamera.cs` | `MonoBehaviour` | 第三视角 + 锁定系统 |
| `PlayerCombatManager.cs` | `CharacterCombatManager` | 武器动作执行/体力消耗 |
| `PlayerEquipmentManager.cs` | `CharacterEquipmentManager` | 武器加载/切换 |
| `PlayerInventoryManager.cs` | `CharacterInventoryManager` | 武器背包数组 |
| `PlayerEffectsManager.cs` | `CharacterEffectsManager` | 效果测试 |
| `PlayerStatsManager.cs` | `CharacterStatsManager` | 初始属性计算 |
| `PlayerSoundFXManager.cs` | `CharacterSoundFXManager` | (空) |
| `PlayerNetworkManager.cs` | `CharacterNetworkManager` | 武器/装备网络变量 |

### AI 系统
| 文件 | 基类 | 职责 |
|------|------|------|
| `AIState.cs` | `ScriptableObject` | 状态基类 |
| `IdleState.cs` | `AIState` | 搜索目标 |
| `PursueTargetState.cs` | `AIState` | NavMesh 追击 |
| `CombatStanceState.cs` | `AIState` | 加权随机选攻击 |
| `AttackState.cs` | `AIState` | 执行攻击 + 恢复 |
| `AICharacterAttackAction.cs` | `ScriptableObject` | 攻击数据定义 |
| `AICharacterCombatManager.cs` | (未读取) | AI 战斗行为 |
| `AIUndeadCombatManager.cs` | `AICharacterCombatManager` | 亡灵伤害/碰撞体 |
| `AICharacterLocomotionManager.cs` | `CharacterLocomotionManager` | 面向 NavMesh |

### 物品与武器
| 文件 | 基类 | 职责 |
|------|------|------|
| `Item.cs` | `ScriptableObject` | 物品基础数据 |
| `WeaponItem.cs` | `Item` | 武器完整数据 |
| `MeleeWeaponItem.cs` | `WeaponItem` | 近战武器标记 |
| `WeaponItemAction.cs` | `ScriptableObject` | 武器动作基类 |
| `LightAttackWeaponItemAction.cs` | `WeaponItemAction` | 轻攻击连招 |
| `HeavyAttackWeaponItemAction.cs` | `WeaponItemAction` | 重攻击连招 |
| `WeaponManager.cs` | `MonoBehaviour` | 武器伤害配置 |

### 伤害系统
| 文件 | 基类 | 职责 |
|------|------|------|
| `DamageCollider.cs` | `MonoBehaviour` | 伤害碰撞体基类 |
| `MeleeWeaponDamageCollider.cs` | `DamageCollider` | 玩家武器伤害 |
| `UndeadDamageCollider.cs` | `DamageCollider` | 亡灵武器伤害 |
| `InstantCharacterEffect.cs` | `ScriptableObject` | 效果基类 |
| `TakeDamageEffect.cs` | `InstantCharacterEffect` | 伤害计算/受击动画 |
| `TakeStaminaDamageEffect.cs` | `InstantCharacterEffect` | 体力伤害 |

### 世界管理器（单例）
| 文件 | 职责 |
|------|------|
| `WorldGameSessionManager.cs` | 活跃玩家列表 |
| `WorldSaveGameManager.cs` | 存档管理 |
| `WorldActionManager.cs` | 武器动作注册 |
| `WorldItemDatabase.cs` | 物品数据库 |
| `WorldAIManager.cs` | AI 生成管理 |
| `WorldSoundFXManager.cs` | 共享音效 |
| `WorldUtilityManager.cs` | Layer/阵营/角度工具 |
| `WorldCharacterEffectsManager.cs` | 效果模板 |

### 其他
| 文件 | 职责 |
|------|------|
| `Enums.cs` | 全局枚举定义 |
| `CharacterSaveData.cs` | 存档数据结构 `[System.Serializable]` |
| `SaveFileDataWriter.cs` | JSON 文件读写 |
| `PlayerControls.cs` | Input System 生成 |
| `TitleScreenManager.cs` | 主菜单 |
| `PlayerUIManager.cs` | UI 管理器 |
| `PlayerUIHudManager.cs` | HUD 血条/物品栏 |
| `PlayerUIPopUpManager.cs` | 弹窗动画 |
| `WeaponModelInstantiationSlot.cs` | 武器挂点 |
| `LockOnTransform.cs` | 锁定目标点 |

---

> **学习建议**:  
> 1. 先理解 `CharacterManager` → `PlayerManager` 的继承关系  
> 2. 再跟踪一次完整的攻击流程（输入 → 动作 → 碰撞 → 伤害 RPC → 效果处理）  
> 3. 然后研究 AI 状态机的 ScriptableObject 实现  
> 4. 最后深入 Netcode 的网络同步机制

---

*文档生成时间: 2026/06/08 — 覆盖 DEMO RING 项目 Assets/Scripcts 全量代码*
