# DEMO RING 项目代码系统学习指南

<a id="top"></a>

> 生成日期：2026-09-13  
> 项目类型：第三人称类魂系多人动作 RPG 原型  
> Unity 版本：2022.3.62f3  
> 渲染管线：URP 14.0.12  
> 网络框架：Unity Netcode for GameObjects 1.12.2  
> 输入系统：Unity Input System 1.14.2  
> 主要学习范围：`Assets/Scripcts`、`Assets/Data`、`Assets/PlayerControls.cs`

---

<a id="toc"></a>

## 目录

1. [第一章：项目全景与学习路线](#chapter-1)  
2. [第二章：总体架构：数据、组件与世界管理器](#chapter-2)  
3. [第三章：角色系统：继承与组件化](#chapter-3)  
4. [第四章：输入、移动、摄像机与动画](#chapter-4)  
5. [第五章：战斗系统：伤害、防御与处决](#chapter-5)  
6. [第六章：物品、武器、动作与装备](#chapter-6)  
7. [第七章：法术与远程武器](#chapter-7)  
8. [第八章：AI、状态机与 Boss](#chapter-8)  
9. [第九章：Netcode 多人同步](#chapter-9)  
10. [第十章：存档、世界管理器与交互物](#chapter-10)  
11. [第十一章：UI 系统](#chapter-11)  
12. [第十二章：设计模式、进阶主题与推荐阅读顺序](#chapter-12)  
13. [第十三章：代码精读与 Unity 实战教程](#chapter-13)  
14. [附录：核心文件速查表](#appendix)

---

<a id="chapter-1"></a>

## 第一章：项目全景与学习路线

### 1.1 这个项目是什么

DEMO RING 是一个类魂系动作 RPG 原型，已经覆盖了从主菜单、角色创建与存档，到第三人称移动、锁定敌人、近战连招、防御、弹反、法术、远程弓箭、AI 敌人和 Boss 战等一套完整玩法闭环。

它最适合学习的目标不是“某个单一系统”，而是：

- Unity 中大型角色系统的组织方式；
- ScriptableObject 数据驱动设计；
- Animator、Animation Event、Root Motion 与角色控制器如何配合；
- Unity Netcode 的 `NetworkVariable`、RPC、服务器权威和网络生成；
- AI 状态机、装备模型热切换和 UI 状态管理。



[↑ 回到目录](#toc)


### 1.2 哪些目录需要重点读

| 目录 | 是否重点 | 说明 |
| --- | --- | --- |
| `Assets/Scripcts` | 重点 | 项目核心游戏代码 |
| `Assets/Data` | 重点 | ScriptableObject 数据、Prefab、动画控制器 |
| `Assets/PlayerControls.cs` | 重点 | Input System 自动生成的强类型输入包装类 |
| `Assets/Scenes` | 了解 | 主菜单与主世界场景 |
| `Assets/Art` | 非重点 | 美术、音乐、动画素材 |
| `Assets/Imports` | 非重点 | 第三方商店资源 |
| `Assets/Plugins` | 了解 | 第三方插件 |
| `Library`、`Temp`、`obj`、`Test` | 不读 | Unity 生成物或测试副本 |



[↑ 回到目录](#toc)


### 1.3 推荐学习顺序

不要从 150 个脚本逐个看，建议按下面 6 个阶段推进：

1. **先读架构**：`CharacterManager`、`PlayerManager`、`AICharacterManager`，理解继承关系。  
2. **再跟一条主流程**：输入 -> 移动/旋转 -> 攻击动作 -> 武器碰撞体 -> 伤害 RPC -> 伤害效果 -> UI 血量。  
3. **读数据层**：`Item`、`WeaponItem`、`WeaponItemAction`、`EquipmentModel`，理解 ScriptableObject 如何被配置和实例化。  
4. **读 AI**：`AIState` 及四个状态，再到 Boss 的睡眠、唤醒、阶段切换和雾门。  
5. **读网络层**：`CharacterNetworkManager`、`PlayerNetworkManager` 和各类 `ServerRpc`、`ClientRpc`。  
6. **读外围系统**：存档、世界管理器、交互物、UI。

---


[↑ 回到目录](#toc)


<a id="chapter-2"></a>

## 第二章：总体架构：数据、组件与世界管理器

### 2.1 三层结构

项目可以抽象成三层：

```text
数据层
├── ScriptableObject 物品/武器/动作/状态/效果
└── Prefab、动画控制器、Unity 场景

角色层
├── CharacterManager（玩家与 AI 的公共基类）
├── 多个专职 Manager 组件
└── PlayerManager / AICharacterManager 子类

世界层
├── WorldSaveGameManager
├── WorldItemDatabase
├── WorldAIManager
├── WorldActionManager
├── WorldSoundFXManager
├── WorldUtilityManager
└── 其他全局单例
```



[↑ 回到目录](#toc)


### 2.2 核心继承关系

```text
UnityEngine.MonoBehaviour
├── CharacterLocomotionManager
│   ├── PlayerLocomotionManager
│   └── AICharacterLocomotionManager
├── CharacterAnimatorManager
│   └── PlayerAnimatorManager
├── CharacterStatsManager
│   └── PlayerStatsManager
├── CharacterEffectsManager
│   └── PlayerEffectsManager
├── CharacterSoundFXManager
│   ├── PlayerSoundFXManager
│   ├── AIBOSS01SoundFXManager
│   └── AIBOSS02SoundFXManager
├── CharacterEquipmentManager
│   └── PlayerEquipmentManager
├── CharacterInventoryManager
│   ├── PlayerInventoryManager
│   └── AICharacterInventoryManager
└── DamageCollider
    ├── MeleeWeaponDamageCollider
    ├── RangedProjectileDamageCollider
    ├── FireBallDamageCollider
    └── 各类敌人伤害碰撞体

Unity.Netcode.NetworkBehaviour
├── CharacterManager
│   ├── PlayerManager
│   └── AICharacterManager
│       └── AIBossCharacterManager
│           ├── AIBOSS01CharacterManager
│           └── AIBOSS02CharacterManager
├── CharacterNetworkManager
│   ├── PlayerNetworkManager
│   └── AICharacterNetworkManager
│       └── AIBossCharacterNetworkManager
├── CharacterCombatManager
│   ├── PlayerCombatManager
│   └── AICharacterCombatManager
│       ├── AIUndeadCombatManager
│       ├── AIBOSS01CharacterCombatManager
│       └── AIBOSS02CharacterCombatManager
└── Interactable
    ├── PickUpItemInteractable
    ├── SiteOfGraceInteractable
    └── FogWallInteractable
```



[↑ 回到目录](#toc)


### 2.3 ScriptableObject 数据体系

```text
ScriptableObject
├── Item
│   ├── EquipmentItem
│   │   ├── WeaponItem
│   │   │   ├── MeleeWeaponItem
│   │   │   ├── RangedWeaponItem
│   │   │   └── CasterWeaponItem
│   │   └── ArmorItem
│   │       ├── HeadEquipmentItem
│   │       ├── BodyEquipmentItem
│   │       ├── HandEquipmentItem
│   │       └── LegEquipmentItem
│   ├── SpellItem
│   │   ├── FireBall
│   │   └── TestSpell
│   ├── RangedProjectileItem
│   └── AshOfWar
│       └── ParryAshOfWar
├── WeaponItemAction
│   ├── LightAttackWeaponItemAction
│   ├── HeavyAttackWeaponItemAction
│   ├── OffHandMeleeAction
│   ├── AimAction
│   ├── FireProjectileAction
│   └── CastIncantationAction
├── AIState
│   ├── IdleState
│   ├── PursueTargetState
│   ├── CombatStanceState
│   ├── AttackState
│   └── BossSleepState
├── AICharacterAttackAction
├── InstantCharacterEffect
│   ├── TakeDamageEffect
│   ├── TakeCriticalDamageEffect
│   ├── TakeBlockedDamageEffect
│   └── TakeStaminaDamageEffect
├── StaticCharacterEffect
│   └── TwoHandingEffect
└── EquipmentModel
```



[↑ 回到目录](#toc)


### 2.4 理解这个架构的关键点

`MonoBehaviour` 和 `NetworkBehaviour` 负责运行时行为，`ScriptableObject` 负责可配置数据。项目中大量的 `CreateAssetMenu` 脚本不是挂在场景物体上的行为类，而是用来在 Project 窗口中创建 `.asset` 数据资产。

例如：

- `WeaponItemAction` 是动作策略，不直接执行输入；
- `AIState` 是状态数据，由 `AICharacterManager` 每帧调用 `Tick`；
- `Item` 和 `EquipmentModel` 是物品与模型的描述数据，由 Player 的 Manager 读取并应用。

---


[↑ 回到目录](#toc)


<a id="chapter-3"></a>

## 第三章：角色系统：继承与组件化

### 3.1 核心文件

| 文件 | 类 | 职责 |
| --- | --- | --- |
| `Assets/Scripcts/Character/CharacterManager.cs` | `CharacterManager : NetworkBehaviour` | 所有角色的网络生命周期、位置同步、死亡流程 |
| `Assets/Scripcts/Character/Player/PlayerManager.cs` | `PlayerManager : CharacterManager` | 玩家初始化、网络回调订阅、存档桥接 |
| `Assets/Scripcts/Character/AI Character/AICharacterManager.cs` | `AICharacterManager : CharacterManager` | AI 状态机驱动、NavMeshAgent 控制 |
| `Assets/Scripcts/Character/AI Character/Boss/AIBossCharacterManager.cs` | `AIBossCharacterManager : AICharacterManager` | Boss 状态、音乐、阶段切换、雾门 |
| `Assets/Scripcts/Character/AI Character/Boss/BOSS01/AIBOSS01CharacterManager.cs` | `AIBOSS01CharacterManager : AIBossCharacterManager` | BOSS01 具体角色 |
| `Assets/Scripcts/Character/AI Character/Boss/Knight of the Crucible/AIBOSS02CharacterManager.cs` | `AIBOSS02CharacterManager : AIBossCharacterManager` | BOSS02 具体角色 |



[↑ 回到目录](#toc)


### 3.2 CharacterManager 是理解整个项目的入口

`CharacterManager` 同时继承了 `NetworkBehaviour`，所以它是“本地角色”和“网络角色”的统一入口。

关键职责：

- 在 `Awake` 中缓存 `CharacterController`、`Animator`、`CharacterNetworkManager` 等组件；
- 在 `Update` 中处理网络位置同步；
- 通过 `ProcessDeathEvent` 统一死亡流程；
- 通过 `IgnoreMyOwnColliders` 避免角色自己的碰撞体互相伤害。

**推荐代码位置**：先读字段和 `Awake`，再读 `Update` 中的 `IsOwner` 分支。



[↑ 回到目录](#toc)


### 3.3 PlayerManager 的职责

`PlayerManager` 是本地玩家特有的逻辑入口，重点有三块：

1. **组件装配**：`Awake` 获取所有 Player 专属 Manager；  
2. **网络回调装配**：`OnNetworkSpawn` 中订阅大量 `NetworkVariable.OnValueChanged`；  
3. **存档装配**：把网络属性写入 `CharacterSaveData`，或从存档恢复网络属性。

这是项目中最典型的“初始化即注册回调”写法：

```csharp
playerNetworkManager.currentHealth.OnValueChanged +=
    PlayerUIManager.instance.playerUIHudManager.SetNewHealthValue;
```

对应的 `OnNetworkDespawn` 中会取消订阅，避免重复注册。



[↑ 回到目录](#toc)


### 3.4 组件化 Manager 清单

| 组件 | 基类 | 职责 |
| --- | --- | --- |
| `CharacterNetworkManager` | `NetworkBehaviour` | 定义所有需要同步的网络变量和 RPC |
| `CharacterLocomotionManager` | `MonoBehaviour` | 重力、地面检测、旋转 |
| `CharacterAnimatorManager` | `MonoBehaviour` | 动画参数、动作播放、受击动画 |
| `CharacterStatsManager` | `MonoBehaviour` | 体力恢复、属性公式 |
| `CharacterCombatManager` | `NetworkBehaviour` | 目标锁定、攻击类型、连招状态 |
| `CharacterEffectsManager` | `MonoBehaviour` | 即时效果处理、VFX 播放 |
| `CharacterEquipmentManager` | `MonoBehaviour` | 装备基类 |
| `CharacterInventoryManager` | `MonoBehaviour` | 背包基类 |
| `CharacterSoundFXManager` | `MonoBehaviour` | 受伤、攻击、死亡音效 |
| `CharacterUIManager` | `MonoBehaviour` | 角色头顶血条 |
| `PlayerBodyManager` | `MonoBehaviour` | 男/女身体、头部、头发、四肢显隐 |



[↑ 回到目录](#toc)


### 3.5 PlayerBodyManager 与换装的关系

`PlayerBodyManager` 不处理装备数据，只处理角色本体的显示/隐藏。  
例如装备全盔时，`PlayerEquipmentManager` 调用 `DisableHead` 和 `DisableHair`；脱掉装备后再恢复。

这一层将“数据决策”留在装备系统，将“身体部件开关”集中在身体管理器，职责非常清晰。

---


[↑ 回到目录](#toc)


<a id="chapter-4"></a>

## 第四章：输入、移动、摄像机与动画

### 4.1 输入系统

核心文件：

- `Assets/PlayerControls.inputactions`
- `Assets/PlayerControls.cs`
- `Assets/Scripcts/Character/Player/PlayerInputManager.cs`

`PlayerControls.cs` 是 Input System 根据 `.inputactions` 自动生成的 C# 类，提供强类型 Action 访问。

`PlayerInputManager` 是单例，负责：

- 监听移动、视角、翻滚、跳跃、冲刺、轻攻击、重攻击、锁定等输入；
- 根据当前场景决定启用或禁用输入；
- 把输入转发给 `PlayerLocomotionManager`、`PlayerCamera`、`PlayerCombatManager` 等系统。



[↑ 回到目录](#toc)


### 4.2 移动核心

核心文件：

- `Assets/Scripcts/Character/CharacterLocomotionManager.cs`
- `Assets/Scripcts/Character/Player/PlayerLocomotionManager.cs`

`CharacterLocomotionManager` 处理通用的重力、地面检测和 Y 速度：

```csharp
isGrounded = Physics.CheckSphere(
    transform.position,
    groundCheckSphereRadius,
    groundLayer);
```

`PlayerLocomotionManager` 处理玩家特有的：

- 行走、奔跑、冲刺速度分层；
- 锁定状态下的三种旋转方式；
- 跳跃和翻滚/后跳的体力消耗与动作锁检查。



[↑ 回到目录](#toc)


### 4.3 第三人称摄像机与锁定

核心文件：

- `Assets/Scripcts/Character/Player/PlayerCamera.cs`
- `Assets/Scripcts/Character/LockOnTransform.cs`

`PlayerCamera` 使用 `SmoothDamp` 跟随，使用 `SphereCast` 处理墙体遮挡。

锁定目标搜索大致流程：

1. `Physics.OverlapSphere` 搜索锁定半径内的对象；
2. 过滤死亡目标、自身、不在视角范围内的目标；
3. 使用 `Linecast` 排除遮挡目标；
4. 按角度和距离分成最近、左、右三个候选目标。



[↑ 回到目录](#toc)


### 4.4 动画系统

核心文件：

- `Assets/Scripcts/Character/CharacterAnimatorManager.cs`
- `Assets/Scripcts/Character/Player/PlayerAnimatorManager.cs`
- `Assets/Scripcts/Animator/ResetActionFlags.cs`
- `Assets/Scripcts/Animator/ResetIsJumping.cs`
- `Assets/Scripcts/Animator/ResetRiposteActionFlags.cs`
- `Assets/Scripcts/Animator/ToggleAttackType.cs`
- `Assets/Scripcts/Character/ToggleBlockingAnimation.cs`

`CharacterAnimatorManager` 把连续输入量化为离散 Blend Tree 参数，并负责通过 `CrossFade` 播放动作动画。

`PlayerAnimatorManager` 的 `OnAnimatorMove` 是 Root Motion 的关键：

```csharp
if (player.characterAnimatorManager.applyRootMotion)
{
    player.characterController.Move(player.animator.deltaPosition);
    player.transform.rotation *= player.animator.deltaRotation;
}
```

`Animator` 目录下的脚本是 `StateMachineBehaviour`，挂在动画状态上，用于在特定动画帧进入、更新或退出时重置 `isAttacking`、`isJumping`、弹反标记等网络状态。

---


[↑ 回到目录](#toc)


<a id="chapter-5"></a>

## 第五章：战斗系统：伤害、防御与处决

### 5.1 一条攻击的完整链路

```text
玩家按下攻击键
  -> PlayerInputManager 接收输入
  -> PlayerCombatManager 选择当前武器动作
  -> WeaponItemAction.AttemptToPerformAction
  -> PlayerAnimatorManager 播放攻击动画
  -> 动画事件开启武器伤害碰撞体
  -> DamageCollider.OnTriggerEnter 命中目标
  -> 生成 InstantCharacterEffect 并计算伤害
  -> 攻击方通过 ServerRpc 通知服务器
  -> 服务器通过 ClientRpc 通知所有客户端
  -> 受击方 ProcessInstantEffect
  -> TakeDamageEffect 扣除血量、播放受击动画/音效/VFX
```



[↑ 回到目录](#toc)


### 5.2 伤害碰撞体

核心文件：

- `Assets/Scripcts/Colliders/DamageCollider.cs`
- `Assets/Scripcts/Colliders/MeleeWeaponDamageCollider.cs`
- `Assets/Scripcts/Colliders/Enemy/UndeadDamageCollider.cs`
- `Assets/Scripcts/Colliders/Enemy/BOSS01DamagerCollider.cs`
- `Assets/Scripcts/Colliders/Enemy/BOSS02DamageCollider.cs`

`DamageCollider` 的关键设计：

- `characterDamaged` 列表防止一次攻击对同一目标多次结算；
- 碰撞体默认关闭，由动画事件在攻击帧开启和关闭；
- 在 `OnTriggerEnter` 中检查阵营、格挡方向和是否无敌。



[↑ 回到目录](#toc)


### 5.3 伤害效果

核心文件：

- `Assets/Scripcts/Effects/Instant Character Effect/InstantCharacterEffect.cs`
- `Assets/Scripcts/Effects/Instant Character Effect/TakeDamageEffect.cs`
- `Assets/Scripcts/Effects/Instant Character Effect/TakeBlockedDamageEffect.cs`
- `Assets/Scripcts/Effects/Instant Character Effect/TakeCriticalDamageEffect.cs`
- `Assets/Scripcts/Effects/Instant Character Effect/TakeStaminaDamageEffect.cs`

这些效果都是 `ScriptableObject`。运行时通常先 `Instantiate` 一份再处理，避免直接修改 `.asset` 数据。

`TakeDamageEffect` 的伤害计算会考虑：

- 攻击者的物理、魔法、火焰、雷电、神圣伤害；
- 受击者的对应抗性；
- 防御减伤；
- 当前攻击类型的倍率；
- 处决和弹反倍率。



[↑ 回到目录](#toc)


### 5.4 防御与弹反

防御判断在 `DamageCollider` 中，核心是根据攻击方向与受击者朝向的点积判断是否成功格挡。

弹反相关文件：

- `Assets/Scripcts/Item/Ashes Of War/ParryAshOfWar.cs`
- `Assets/Scripcts/Animator/ResetRiposteActionFlags.cs`

当 `MeleeWeaponDamageCollider` 检测到攻击者处于 `isParryable`，且目标正在 `isParrying`，会进入弹反/处决流程，通过 RPC 同步被弹反者的网络状态。



[↑ 回到目录](#toc)


### 5.5 连招窗口

连招不靠“冷却时间”驱动，而靠动画事件驱动：

```text
攻击动画播放到某一帧
  -> EnableDoCombo：允许玩家在窗口内继续按攻击键
  -> 攻击动画结束前
  -> DisableDoCombo：关闭连招窗口
```

核心代码在：

- `Assets/Scripcts/Character/Player/PlayerAnimatorManager.cs`
- `Assets/Scripcts/Weapon Action/LightAttackWeaponItemAction.cs`
- `Assets/Scripcts/Weapon Action/HeavyAttackWeaponItemAction.cs`

---


[↑ 回到目录](#toc)


<a id="chapter-6"></a>

## 第六章：物品、武器、动作与装备

### 6.1 物品继承链

核心文件：

- `Assets/Scripcts/Item/Item.cs`
- `Assets/Scripcts/Item/Equipment Item/EquipmentItem.cs`
- `Assets/Scripcts/Item/Weapon Item/WeaponItem.cs`
- `Assets/Scripcts/Item/Weapon Item/Melee Weapon/MeleeWeaponItem.cs`
- `Assets/Scripcts/Item/Weapon Item/Ranged Weapon/RangedWeaponItem.cs`
- `Assets/Scripcts/Item/Weapon Item/Caster Weapon/CasterWeaponItem.cs`
- `Assets/Scripcts/Item/Equipment Item/HeadEquipmentItem.cs`
- `Assets/Scripcts/Item/Equipment Item/BodyEquipmentItem.cs`
- `Assets/Scripcts/Item/Equipment Item/HandEquipmentItem.cs`
- `Assets/Scripcts/Item/Equipment Item/LegEquipmentItem.cs`

`Item` 是最基础的数据对象，包含：

- `itemName`
- `itemIcon`
- `itemID`
- `itemDescription`

`WeaponItem` 在 `EquipmentItem` 基础上增加伤害、防御、攻击修饰、体力消耗和绑定动作。



[↑ 回到目录](#toc)


### 6.2 武器动作是策略模式

核心文件：

- `Assets/Scripcts/Weapon Action/WeaponItemAction.cs`
- `Assets/Scripcts/Weapon Action/LightAttackWeaponItemAction.cs`
- `Assets/Scripcts/Weapon Action/HeavyAttackWeaponItemAction.cs`
- `Assets/Scripcts/Weapon Action/OffHandMeleeAction.cs`
- `Assets/Scripcts/Weapon Action/AimAction.cs`
- `Assets/Scripcts/Weapon Action/FireProjectileAction.cs`
- `Assets/Scripcts/Weapon Action/CastIncantationAction.cs`

`WeaponItemAction` 是动作基类，`WeaponItem` 中保存若干动作引用。不同武器只需要绑定不同动作资产，就能复用同一套玩家攻击逻辑。

例如：

- 近战武器绑定 `LightAttackWeaponItemAction` 和 `HeavyAttackWeaponItemAction`；
- 弓绑定 `AimAction` 和 `FireProjectileAction`；
- 施法触媒绑定 `CastIncantationAction`。



[↑ 回到目录](#toc)


### 6.3 武器加载与切换

核心文件：

- `Assets/Scripcts/Character/Player/PlayerEquipmentManager.cs`
- `Assets/Scripcts/Character/WeaponModelInstantiationSlot.cs`
- `Assets/Scripcts/Item/Weapon Item/WeaponManager.cs`

`WeaponModelInstantiationSlot` 是模型挂点，例如右手、左手、盾牌位、背部。

切换武器不是直接操作模型，而是修改网络变量：

```text
切换输入
  -> PlayerEquipmentManager 修改当前右手武器 ID
  -> NetworkVariable 的 OnValueChanged 触发
  -> 所有客户端加载对应武器模型
```



[↑ 回到目录](#toc)


### 6.4 装备模型与换装

核心文件：

- `Assets/Scripcts/Item/EquipmentModel/EquipmentModel.cs`
- `Assets/Scripcts/Character/Player/PlayerEquipmentManager.cs`
- `Assets/Scripcts/Character/Player/PlayerBodyManager.cs`

`EquipmentModel` 是一个 ScriptableObject，用 `EquipmentModelType` 描述这个模型属于哪个身体部位，例如：

- `FullHelmet`
- `Torso`
- `RightUpperArm`
- `RightLowerArm`
- `Hips`
- `RightLeg`
- `Back`

`EquipmentModel.LoadEquipmentModel` 根据玩家性别，在 `PlayerEquipmentManager` 预初始化的模型数组中找到同名模型并 `SetActive(true)`。

`PlayerEquipmentManager.Awake` 会把挂在父物体下的所有模型子物体缓存成数组，这样换装时只需要按名字查找并显示对应模型。



[↑ 回到目录](#toc)


### 6.5 装备 UI

核心文件：

- `Assets/Scripcts/Character/Player/PlayerUI/PlayerUIEquipmentManager.cs`
- `Assets/Scripcts/Character/Player/PlayerUI/PlayerUIEquipmentManagerInputManager.cs`
- `Assets/Scripcts/UI/UI_EquipmentInventorySlot.cs`

装备 UI 从 `PlayerInventoryManager` 读取右手武器、左手武器、头、身体、手、腿等装备，刷新槽位图标；点击具体槽位后显示对应类型的物品列表，选中后把装备 ID 写回玩家网络变量。

---


[↑ 回到目录](#toc)


<a id="chapter-7"></a>

## 第七章：法术与远程武器

### 7.1 法术系统

核心文件：

- `Assets/Scripcts/Item/Spells/SpellItem.cs`
- `Assets/Scripcts/Item/Spells/FireBall.cs`
- `Assets/Scripcts/Item/Spells/TestSpell.cs`
- `Assets/Scripcts/Item/Spells/SpellManager/SpellManager.cs`
- `Assets/Scripcts/Item/Spells/SpellManager/FireBallManager.cs`
- `Assets/Scripcts/Weapon Action/CastIncantationAction.cs`

`SpellItem` 描述法术消耗、蓄力倍率、动画、音效和 FX。  
`FireBall` 是具体法术，覆盖施法逻辑并创建火球。

施法流程：

```text
CastIncantationAction
  -> SpellItem.CanICastSpell
  -> 播放施法动画
  -> 创建 warm-up FX
  -> SuccessfullyCastSpell 或 Full Charge
  -> 扣除体力和专注值
  -> FireBallManager 创建火球
  -> FireBallDamageCollider 命中后结算伤害
```



[↑ 回到目录](#toc)


### 7.2 远程武器与弓箭

核心文件：

- `Assets/Scripcts/Item/Weapon Item/Ranged Weapon/RangedWeaponItem.cs`
- `Assets/Scripcts/Item/Equipment Item/RangedProjectileItem.cs`
- `Assets/Scripcts/Weapon Action/AimAction.cs`
- `Assets/Scripcts/Weapon Action/FireProjectileAction.cs`
- `Assets/Scripcts/Colliders/Projectiles/RangedProjectileDamageCollider.cs`
- `Assets/Scripcts/Utility/SpellInstantiationLocation.cs`

远程武器的输入被拆成两个动作：

1. `AimAction`：进入瞄准状态，必要时把武器改为双手持握；  
2. `FireProjectileAction`：选择主/副弹药槽，检查弹药，播放搭箭和拉弓动画，并通过 RPC 同步已经搭上的箭。

`RangedProjectileDamageCollider` 独立处理飞行弹道命中，包括格挡角度判断和伤害 RPC。

---


[↑ 回到目录](#toc)


<a id="chapter-8"></a>

## 第八章：AI、状态机与 Boss

### 8.1 AI 状态机

核心文件：

- `Assets/Scripcts/Character/AI Character/States/AIState.cs`
- `Assets/Scripcts/Character/AI Character/States/IdleState.cs`
- `Assets/Scripcts/Character/AI Character/States/PursueTargetState.cs`
- `Assets/Scripcts/Character/AI Character/States/CombatStanceState.cs`
- `Assets/Scripcts/Character/AI Character/States/AttackState.cs`
- `Assets/Scripcts/Character/AI Character/States/BossSleepState.cs`

这是本项目最值得学习的部分之一：AI 状态不是 `enum + switch`，而是 ScriptableObject。

`AIState.Tick` 接收 `AICharacterManager`，返回下一个 `AIState`。返回自身表示保持当前状态，返回其他状态表示切换。

典型状态转换：

```text
IdleState
  -> 找到目标 -> PursueTargetState
  -> 没找到目标 -> 保持 Idle

PursueTargetState
  -> 进入攻击距离 -> CombatStanceState
  -> 丢失目标 -> IdleState

CombatStanceState
  -> 通过加权随机选择攻击 -> AttackState

AttackState
  -> 攻击完成并恢复 -> CombatStanceState
```



[↑ 回到目录](#toc)


### 8.2 攻击动作数据

核心文件：

- `Assets/Scripcts/Character/AI Character/Actions/AICharacterAttackAction.cs`

每个 AI 攻击动作包含：

- 动画名称；
- `AttackType`；
- 权重 `attackWeigth`；
- 恢复时间；
- 最小/最大攻击角度；
- 最小/最大攻击距离。

`CombatStanceState` 先过滤满足角度和距离的攻击，再按权重随机选择。



[↑ 回到目录](#toc)


### 8.3 Boss 状态与阶段切换

核心文件：

- `Assets/Scripcts/Character/AI Character/Boss/AIBossCharacterManager.cs`
- `Assets/Scripcts/Character/AI Character/Boss/EventTriggerBossFight.cs`

Boss 额外管理三个网络状态：

- `bossFightIsActive`
- `hasBeenAwakened`
- `hasBeenDefeated`

Boss 流程：

```text
生成时检查存档
  -> 如果未击败，进入 SleepState
  -> 玩家进入 Boss 区域
  -> EventTriggerBossFight 唤醒 Boss
  -> 播放 BGM、显示 Boss 血条、激活雾门
  -> 血量低于阶段阈值时 PhaseChange
  -> 死亡时关闭雾门、保存击败状态、播放胜利弹窗
```



[↑ 回到目录](#toc)


### 8.4 敌人具体实现

核心文件：

- `Assets/Scripcts/Character/AI Character/Undead Character/AIUndeadCombatManager.cs`
- `Assets/Scripcts/Character/AI Character/Boss/BOSS01/AIBOSS01CharacterCombatManager.cs`
- `Assets/Scripcts/Character/AI Character/Boss/Knight of the Crucible/AIBOSS02CharacterCombatManager.cs`

这些类通常负责：

- 给动画事件提供方法；
- 开启/关闭对应伤害碰撞体；
- 设置本次攻击伤害倍率；
- 播放攻击音效。

---


[↑ 回到目录](#toc)


<a id="chapter-9"></a>

## 第九章：Netcode 多人同步

### 9.1 网络模型

项目使用 Unity Netcode for GameObjects，采用 Host 模式：

- Host 同时是服务器和客户端；
- 服务器负责生成 AI、处理伤害 RPC、切换场景；
- 客户端负责本地输入、本地表现，并通过网络变量和 RPC 与服务器同步。

核心文件：

- `Assets/Scripcts/Character/CharacterNetworkManager.cs`
- `Assets/Scripcts/Character/Player/PlayerNetworkManager.cs`
- `Assets/Scripcts/Character/AI Character/AICharacterNetworkManager.cs`
- `Assets/Scripcts/Character/AI Character/Boss/AIBossCharacterNetworkManager.cs`



[↑ 回到目录](#toc)


### 9.2 NetworkVariable

项目使用大量 `NetworkVariable<T>`，例如：

| 类型 | 示例 |
| --- | --- |
| `int` | 当前血量、体力、武器 ID |
| `float` | 移动参数 |
| `bool` | 攻击、跳跃、防御、死亡状态 |
| `Vector3` | 网络位置 |
| `Quaternion` | 网络旋转 |
| `ulong` | 锁定目标 NetworkObjectId |
| `FixedString64Bytes` | 角色名称 |

所有网络变量的值变化都会触发 `OnValueChanged`。`PlayerManager.OnNetworkSpawn` 中集中订阅回调，在 `OnNetworkDespawn` 中集中取消订阅。



[↑ 回到目录](#toc)


### 9.3 RPC 模式

常见的模式：

```text
本地玩家触发表现
  -> ServerRpc 上传到服务器
  -> 服务器验证
  -> ClientRpc 广播给其他客户端
```

伤害同步就是典型例子：

```text
攻击者命中
  -> NotifyTheServerOfCharacterDamageServerRpc
  -> 服务器广播
  -> 受击者本地 ProcessCharacterDamageFromServer
```



[↑ 回到目录](#toc)


### 9.4 网络生成

核心文件：

- `Assets/Scripcts/WorldManager/WorldAIManager.cs`
- `Assets/Scripcts/Utility/NetworkObjectSpawner.cs`
- `Assets/Scripcts/Utility/AICharacterSpawner.cs`

服务器使用 `NetworkObject.Spawn()` 生成 AI 和网络交互物。生成后 Netcode 会负责把对象同步到所有客户端。



[↑ 回到目录](#toc)


### 9.5 场景切换

核心文件：

- `Assets/Scripcts/WorldManager/WorldSaveGameManager.cs`

项目没有直接使用 `SceneManager.LoadSceneAsync` 做网络场景切换，而是使用：

```csharp
NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
```

这样可以确保场景加载与 Netcode 的客户端同步流程一致。

---


[↑ 回到目录](#toc)


<a id="chapter-10"></a>

## 第十章：存档、世界管理器与交互物

### 10.1 存档数据结构

核心文件：

- `Assets/Scripcts/Game Saving/CharacterSaveData.cs`
- `Assets/Scripcts/Game Saving/SaveFileDataWriter.cs`
- `Assets/Scripcts/Game Saving/SerializableDictionary.cs`

`CharacterSaveData` 是 `[Serializable]` 数据类，保存：

- 角色名、性别；
- 当前场景；
- 血量和体力；
- 属性等级；
- 当前装备 ID；
- 已激活赐福点和 Boss 状态。

`SaveFileDataWriter` 使用 `JsonUtility` 进行 JSON 序列化，并写入 `Application.persistentDataPath`。



[↑ 回到目录](#toc)


### 10.2 世界管理器

核心文件：

- `Assets/Scripcts/WorldManager/WorldSaveGameManager.cs`
- `Assets/Scripcts/WorldManager/WorldItemDatabase.cs`
- `Assets/Scripcts/WorldManager/WorldActionManager.cs`
- `Assets/Scripcts/WorldManager/WorldAIManager.cs`
- `Assets/Scripcts/WorldManager/WorldGameSessionManager.cs`
- `Assets/Scripcts/WorldManager/WorldObjectManager.cs`
- `Assets/Scripcts/WorldManager/WorldSoundFXManager.cs`
- `Assets/Scripcts/WorldManager/WorldUtilityManager.cs`
- `Assets/Scripcts/WorldManager/WorldCharacterEffectsManager.cs`

世界管理器几乎都使用 MonoBehaviour 单例模式：

```csharp
if (instance == null)
    instance = this;
else
    Destroy(gameObject);

DontDestroyOnLoad(gameObject);
```

它们的作用：

| 管理器 | 作用 |
| --- | --- |
| `WorldSaveGameManager` | 创建、读取、删除角色存档，加载世界场景 |
| `WorldItemDatabase` | 注册物品并分配 ID，按 ID 查询 |
| `WorldActionManager` | 注册和查询武器动作 |
| `WorldAIManager` | 服务器生成 AI |
| `WorldGameSessionManager` | 维护当前游戏会话中的玩家 |
| `WorldObjectManager` | 管理雾门等世界对象 |
| `WorldSoundFXManager` | 全局音效、Boss BGM |
| `WorldUtilityManager` | Layer、阵营、角度等工具 |
| `WorldCharacterEffectsManager` | 共享效果模板和 VFX |



[↑ 回到目录](#toc)


### 10.3 交互物系统

核心文件：

- `Assets/Scripcts/Object/Interactable.cs`
- `Assets/Scripcts/Character/Player/PlayerInteractionManager.cs`
- `Assets/Scripcts/Object/Pick Up Item/PickUpItemInteractable.cs`
- `Assets/Scripcts/Object/Site Of Graces/SiteOfGraceInteractable.cs`
- `Assets/Scripcts/Object/Fog Walls/FogWallInteractable.cs`

`Interactable` 是网络交互物基类，检测本地玩家进入/离开触发器，把交互物加入 `PlayerInteractionManager` 的列表。

`PlayerInteractionManager.Interact` 取列表第一个对象执行交互。  
不同子类覆盖 `Interact`：

- `PickUpItemInteractable` 拾取物品；
- `SiteOfGraceInteractable` 回满血量和体力、激活赐福点并保存；
- `FogWallInteractable` 控制 Boss 战雾门。

---


[↑ 回到目录](#toc)


<a id="chapter-11"></a>

## 第十一章：UI 系统

### 11.1 UI 管理器

核心文件：

- `Assets/Scripcts/Character/Player/PlayerUI/PlayerUIManager.cs`
- `Assets/Scripcts/Character/Player/PlayerUI/PlayerUIHudManager.cs`
- `Assets/Scripcts/Character/Player/PlayerUI/PlayerUIPopUpManager.cs`
- `Assets/Scripcts/Character/Player/PlayerUI/PlayerUICharacterMenuManager.cs`
- `Assets/Scripcts/Character/Player/PlayerUI/PlayerUIEquipmentManager.cs`
- `Assets/Scripcts/Character/Player/PlayerUI/PlayerUIEquipmentManagerInputManager.cs`
- `Assets/Scripcts/Character/Player/PlayerUI/PlayerUISelectedOnEnable.cs`
- `Assets/Scripcts/Character/Player/PlayerUI/PlayerUIToggleHUD.cs`

`PlayerUIManager` 聚合 HUD、弹窗、角色菜单和装备菜单，并维护：

- `menuWindowIsOpen`
- `popUpWindowIsOpen`

输入管理器通过这些标志决定游戏输入是否被菜单 UI 接管。



[↑ 回到目录](#toc)


### 11.2 HUD

`PlayerUIHudManager` 负责：

- 血条、体力条、专注条；
- 左右手武器快捷栏；
- Boss 血条的实例化和显示；
- 网络变量变化时刷新 UI。



[↑ 回到目录](#toc)


### 11.3 通用 UI 组件

核心文件：

- `Assets/Scripcts/UI/UI_StatBar.cs`
- `Assets/Scripcts/UI/UI_Character_HP_Bar.cs`
- `Assets/Scripcts/UI/UI_Boss_HP_Bar.cs`
- `Assets/Scripcts/UI/UI_Character_Save_Slot.cs`
- `Assets/Scripcts/UI/UI_EquipmentInventorySlot.cs`
- `Assets/Scripcts/UI/UI_Match_Scroll_Wheel_To_Selected_Button.cs`

`UI_StatBar` 是进度条基类，封装当前值和最大值的显示。  
角色血条和 Boss 血条分别继承它，并处理跟随目标或订阅网络血量变化。

---


[↑ 回到目录](#toc)


<a id="chapter-12"></a>

## 第十二章：设计模式、进阶主题与推荐阅读顺序

### 12.1 项目中体现的设计模式

| 模式 | 典型位置 | 学习价值 |
| --- | --- | --- |
| 组件模式 | `CharacterManager` + 各种 Manager | 避免一个角色类无限膨胀 |
| 继承 + 模板方法 | `CharacterManager`、`CharacterAnimatorManager`、`DamageCollider` | 公共流程放基类，差异放子类 |
| 单例模式 | 所有 `WorldManager`、`PlayerUIManager`、`PlayerInputManager` | 全局唯一访问点 |
| 状态模式 | `AIState` 体系 | ScriptableObject 状态机，可配置、可扩展 |
| 策略模式 | `WeaponItemAction` 体系 | 不同武器绑定不同动作 |
| 观察者模式 | `NetworkVariable.OnValueChanged` | 网络变量变化自动通知 UI 和逻辑 |
| 数据驱动 | `Item`、`WeaponItem`、`EquipmentModel`、`AIState` | 策划可在 Inspector 中配置 |
| 对象组合 | `PlayerEquipmentManager` + `PlayerBodyManager` | 换装逻辑和身体显隐分离 |



[↑ 回到目录](#toc)


### 12.2 建议深入阅读的代码路径

#### 路径 A：角色与移动

```text
CharacterManager
  -> PlayerManager
  -> PlayerInputManager
  -> PlayerLocomotionManager
  -> PlayerCamera
  -> PlayerAnimatorManager
```



[↑ 回到目录](#toc)


#### 路径 B：一次近战攻击

```text
PlayerInputManager
  -> PlayerCombatManager
  -> LightAttackWeaponItemAction
  -> PlayerAnimatorManager
  -> MeleeWeaponDamageCollider
  -> DamageCollider
  -> PlayerNetworkManager
  -> TakeDamageEffect
```



[↑ 回到目录](#toc)


#### 路径 C：AI 状态机

```text
AICharacterManager
  -> AICharacterCombatManager
  -> IdleState
  -> PursueTargetState
  -> CombatStanceState
  -> AttackState
  -> AICharacterAttackAction
```



[↑ 回到目录](#toc)


#### 路径 D：装备与换装

```text
Item
  -> EquipmentItem
  -> ArmorItem
  -> PlayerInventoryManager
  -> PlayerEquipmentManager
  -> EquipmentModel
  -> PlayerBodyManager
  -> PlayerUIEquipmentManager
```



[↑ 回到目录](#toc)


#### 路径 E：存档与场景

```text
TitleScreenManager
  -> WorldSaveGameManager
  -> CharacterSaveData
  -> SaveFileDataWriter
  -> PlayerManager
  -> WorldObjectManager
```



[↑ 回到目录](#toc)


### 12.3 可以重点研究的进阶主题

1. **动画事件驱动战斗**：为什么很多游戏把伤害判定放在动画事件中，而不是放在 `Update` 中。  
2. **Root Motion 与 CharacterController 的配合**：哪些动作应该由代码移动，哪些动作应该由动画移动。  
3. **网络变量与 RPC 的边界**：什么时候用 `NetworkVariable`，什么时候用 RPC。  
4. **ScriptableObject 状态机与普通枚举状态机的取舍**：AI 状态资产如何被创建、配置和实例化。  
5. **装备模型的按名称查找方案**：为什么用 `GameObject.name` 匹配模型，而不是直接存 GameObject 引用。  
6. **服务器权威与 Owner 写入权限**：伤害、死亡和 Boss 状态为什么需要服务器参与。

---


[↑ 回到目录](#toc)


<a id="chapter-13"></a>

## 第十三章：代码精读与 Unity 实战教程

这一章是“从读代码到会写代码”的桥梁。建议先阅读前十二章，建立整体认知，再回到这里做代码精读和 Unity 实操。



[↑ 回到目录](#toc)


### 13.1 精读一：CharacterManager 如何成为所有角色的骨架

文件：`Assets/Scripcts/Character/CharacterManager.cs`



[↑ 回到目录](#toc)


#### 13.1.1 关键字段解析

```csharp
public NetworkVariable<bool> isDead = new NetworkVariable<bool>(
    false,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Owner
);
```

逐行理解：

1. `NetworkVariable<bool>` 是 Netcode 的可同步变量，不是普通的 `bool`。  
2. 第一个参数 `false` 是初始值，表示角色刚生成时未死亡。  
3. `Everyone` 表示所有客户端都能读取这个值。  
4. `Owner` 表示只有这个 `NetworkObject` 的 Owner 能修改它。

在 Unity 中做多人游戏，不能把 `isDead` 写成普通 `bool`，因为普通字段不会自动同步到其他客户端。网络变量是“跨设备共享同一份游戏状态”的基础。



[↑ 回到目录](#toc)


#### 13.1.2 Awake 为什么要一次性缓存所有组件

```csharp
characterController = GetComponent<CharacterController>();
characterNetworkManager = GetComponent<CharacterNetworkManager>();
animator = GetComponent<Animator>();
characterEffectsManager = GetComponent<CharacterEffectsManager>();
// ...
```

这里体现的是 Unity 常见优化习惯：**避免在每帧里反复调用 `GetComponent`**。

`GetComponent` 需要查找组件，调用成本比直接访问字段高。角色会频繁访问移动、动画、战斗、网络等组件，所以 `Awake` 中一次性缓存，后续直接使用字段引用。



[↑ 回到目录](#toc)


#### 13.1.3 Owner 与非 Owner 的位置同步

```csharp
if (IsOwner)
{
    characterNetworkManager.networkPosition.Value = transform.position;
    characterNetworkManager.networkRotation.Value = transform.rotation;
}
else
{
    transform.position = Vector3.SmoothDamp(
        transform.position,
        characterNetworkManager.networkPosition.Value,
        ref characterNetworkManager.networkPositionVelocity,
        characterNetworkManager.networkPositionSmoothTime
    );

    transform.rotation = Quaternion.Slerp(
        transform.rotation,
        characterNetworkManager.networkRotation.Value,
        characterNetworkManager.networkRotationSmoothTime
    );
}
```

这段代码是第三人称多人游戏的标准位置同步模式：

- **Owner**：本地控制的角色以本地物理和动画结果为准，然后把结果写入网络变量，发给其他客户端。  
- **非 Owner**：不自己计算移动，而是读取 Owner 上传的位置，并使用 `SmoothDamp`/`Slerp` 插值，减少网络抖动。

Unity 教程角度：

1. `Vector3.SmoothDamp` 是一个带速度平滑的插值函数，比 `Lerp` 更适合处理网络位置，因为它在接近目标时会自动减速。  
2. `ref characterNetworkManager.networkPositionVelocity` 是速度缓存，函数内部会维护它，下一帧继续使用。  
3. `Quaternion.Slerp` 用来平滑旋转，避免角色瞬间转向。



[↑ 回到目录](#toc)


#### 13.1.4 忽略自身碰撞体

```csharp
foreach (var collider in ignoreColliders)
{
    foreach (var otherCollider in ignoreColliders)
    {
        Physics.IgnoreCollision(collider, otherCollider, true);
    }
}
```

角色的武器、盾牌、身体等可能挂着多个碰撞体。如果不忽略彼此碰撞，武器挥动时容易与自己的角色身体发生碰撞，导致抖动或错误伤害。

`Physics.IgnoreCollision(a, b, true)` 是 Unity 提供的 API，用来指定两个碰撞体互相不产生物理碰撞。



[↑ 回到目录](#toc)


### 13.2 精读二：输入系统如何把按键变成动作

文件：`Assets/Scripcts/Character/Player/PlayerInputManager.cs`



[↑ 回到目录](#toc)


#### 13.2.1 Input System 的事件订阅

```csharp
playerControls.PlayerMovement.Movement.performed += i =>
    movementInput = i.ReadValue<Vector2>();

playerControls.PlayerActions.Dodge.performed += i =>
    dodge_Input = true;

playerControls.PlayerActions.Sprint.performed += i =>
    sprint_Input = true;

playerControls.PlayerActions.Sprint.canceled += i =>
    sprint_Input = false;
```

`performed` 表示输入动作完成一次触发，适合跳跃、翻滚、锁定这种“按下一次执行一次”的操作。  
`canceled` 表示按键松开，适合冲刺这种“按住期间持续有效”的操作。

Unity 教程角度：

- Input System 的核心是 Action，不是具体按键。  
- `Movement` Action 会读取摇杆或 WASD，最终得到一个 `Vector2`。  
- `ReadValue<Vector2>()` 把输入转换成适合角色移动的二维向量。



[↑ 回到目录](#toc)


#### 13.2.2 为什么输入管理器要用单例并控制场景启用

`PlayerInputManager` 使用 `DontDestroyOnLoad`，但主菜单和游戏场景不应该同时接收玩家输入。

```csharp
if (newScene.buildIndex == WorldSaveGameManager.instance.GetWorldSceneIndex())
{
    instance.enabled = true;
    playerControls.Enable();
}
else
{
    instance.enabled = false;
}
```

Unity 场景切换时，`activeSceneChanged` 事件会触发，输入管理器根据目标场景决定是否开启输入。这比在多个场景里各放一个输入对象更安全，也避免重复单例。



[↑ 回到目录](#toc)


### 13.3 精读三：移动、旋转和跳跃

文件：`Assets/Scripcts/Character/Player/PlayerLocomotionManager.cs`



[↑ 回到目录](#toc)


#### 13.3.1 摄像机方向到世界方向

```csharp
moveDirection =
    PlayerCamera.instance.transform.forward * verticalMovement +
    PlayerCamera.instance.transform.right * horizontalMovement;
moveDirection.Normalize();
moveDirection.y = 0;
```

这是第三人称移动的经典计算：

1. `verticalMovement` 表示前后输入；  
2. `horizontalMovement` 表示左右输入；  
3. 摄像机前方乘以前后输入，摄像机右方乘以左右输入；  
4. 把两个向量相加，得到“相对摄像机方向”的移动方向；  
5. `y = 0` 是为了把方向压到水平面，避免角色朝上下移动。



[↑ 回到目录](#toc)


#### 13.3.2 三种速度状态

```csharp
if (isSprinting)
{
    Move(sprintingSpeed * Time.deltaTime);
}
else if (moveAmount > 0.5f)
{
    Move(runningSpeed * Time.deltaTime);
}
else
{
    Move(walkingSpeed * Time.deltaTime);
}
```

这里根据 `moveAmount` 和冲刺状态选择行走、奔跑或冲刺。所有位移都乘以 `Time.deltaTime`，保证不同帧率下移动速度一致。



[↑ 回到目录](#toc)


#### 13.3.3 跳跃的物理实现

跳跃并不是简单把角色向上移动，而是分成两步：

1. 玩家按下跳跃时，检查是否在地面、是否正在做动作、体力是否足够。  
2. 播放跳跃动画，设置 `isJumping`，扣除体力。  
3. 在动画的起跳帧调用 `ApplyJumpVelocity`：

```csharp
yVelocity.y = Mathf.Sqrt(jumpHeight * -2 * gravityForce);
```

这个公式来自物理：

```text
v = sqrt(2 * g * h)
```

其中 `gravityForce` 是负数，所以代码里写成 `jumpHeight * -2 * gravityForce`。理解这个公式后，修改 `jumpHeight` 就能得到更合理的手感。



[↑ 回到目录](#toc)


### 13.4 精读四：一次近战伤害如何产生

文件：

- `Assets/Scripcts/Colliders/DamageCollider.cs`
- `Assets/Scripcts/Effects/Instant Character Effect/TakeDamageEffect.cs`



[↑ 回到目录](#toc)


#### 13.4.1 为什么伤害判定放在 OnTriggerEnter

武器碰撞体默认关闭，攻击动画播放到伤害帧时才由动画事件开启：

```csharp
public virtual void EnableDamageCollider()
{
    characterDamaged.Clear();
    damageCollider.enabled = true;
}
```

Unity 教程角度：

- `OnTriggerEnter` 只会在碰撞体刚接触时触发一次，适合“这一刀命中谁”。  
- 普通 `Update` 每帧检测，容易出现一帧内重复伤害。  
- 项目用 `characterDamaged` 列表辅助去重，保证同一目标在一次攻击中只结算一次。



[↑ 回到目录](#toc)


#### 13.4.2 伤害效果是数据对象，不是硬编码

```csharp
TakeDamageEffect damageEffect =
    Instantiate(WorldCharacterEffectsManager.instance.takeDamageEffect);

damageEffect.physicalDamage = physicalDamage;
damageEffect.fireDamage = fireDamage;
damageEffect.poiseDamage = poiseDamage;
damageEffect.contactPoint = contactPoint;

damageTarget.characterEffectsManager.ProcessInstantEffect(damageEffect);
```

这里不是直接修改敌人血量，而是：

1. 从全局模板复制一份伤害效果；  
2. 填入本次攻击的具体伤害值；  
3. 交给目标角色的 `CharacterEffectsManager` 统一处理。

这种做法的好处是：伤害、流血特效、受击音效、韧性计算等逻辑都集中在 `TakeDamageEffect` 中，新增伤害类型时可以复用这条链路。



[↑ 回到目录](#toc)


#### 13.4.3 方向性受击动画

`TakeDamageEffect` 根据 `angleHitFrom` 判断攻击来自前方、后方、左侧还是右侧：

```text
-180 ~ -145 或 145 ~ 180  -> 前方
-45 ~ 45                    -> 后方
-144 ~ -45                  -> 左侧
45 ~ 144                    -> 右侧
```

Unity 教程角度：

角度来自攻击者位置与受击者前方方向的关系。把角度范围与动画名称对应，就能让角色“从哪个方向被打，就朝哪个方向播放受击反应”。



[↑ 回到目录](#toc)


### 13.5 精读五：AI 状态机为什么用 ScriptableObject

文件：

- `Assets/Scripcts/Character/AI Character/AICharacterManager.cs`
- `Assets/Scripcts/Character/AI Character/States/CombatStanceState.cs`



[↑ 回到目录](#toc)


#### 13.5.1 状态机每帧只做一件事

```csharp
AIState nextState = currentState?.Tick(this);

if (nextState != null)
{
    currentState = nextState;
}
```

`Tick` 返回：

- `this`：保持当前状态；  
- 另一个 `AIState`：切换状态；  
- `null`：不改变状态。

状态切换不需要修改角色类的 `switch`，新增一个 `.asset` 状态并拖到 AI 上即可。



[↑ 回到目录](#toc)


#### 13.5.2 加权随机攻击

```csharp
int totalWeight = 0;
foreach (var potentialAttack in potentialAttacks)
    totalWeight += potentialAttack.attackWeight;

int randomValue = Random.Range(1, totalWeight + 1);

int processWeight = 0;
foreach (var potentialAttack in potentialAttacks)
{
    processWeight += potentialAttack.attackWeight;

    if (processWeight >= randomValue)
    {
        chosenAttack = potentialAttack;
        hasAttacked = true;
        return;
    }
}
```

这是“加权随机”的标准写法。假设三个攻击权重分别是 `10`、`20`、`30`，总权重是 `60`，那么它们被选中的概率就是 `10/60`、`20/60`、`30/60`。

Unity 教程角度：

- 使用 `Random.Range(1, totalWeight + 1)` 是为了避免 0 导致某些边界问题。  
- 权重越大，越容易被选中，策划可以据此控制 AI 行为。



[↑ 回到目录](#toc)


### 13.6 Unity 实战教程：如何追踪一条完整攻击流程

推荐在 Unity Editor 中实际操作：

1. 打开 `Assets/Scenes/Scene_World_Scene_01.unity`。  
2. 在 Hierarchy 中选中 Player，查看 `PlayerManager`、`PlayerInputManager`、`PlayerLocomotionManager`、`PlayerCombatManager`、`PlayerEquipmentManager` 等组件。  
3. 运行游戏，按攻击键。  
4. 在 Inspector 中观察 `isPerformingAction`、`isAttacking`、`currentWeaponBeingUsed` 等字段变化。  
5. 打开 Animator 窗口，查看攻击动画状态和动画事件。  
6. 选中武器模型，查看 `MeleeWeaponDamageCollider`，观察 `damageCollider` 的启用时机。  
7. 在 `TakeDamageEffect` 中打断点，查看一次伤害的计算过程。

建议练习：

- 新建一个 `WeaponItemAction` 子类，例如 `JumpAttackAction`，绑定到某个武器资产上。  
- 修改 `PlayerLocomotionManager` 中的 `runningSpeed` 和 `jumpHeight`，观察手感变化。  
- 新建一个 `AIState`，例如 `RetreatState`，让 AI 血量低时后撤。  
- 在 `PlayerNetworkManager` 中新增一个 `NetworkVariable<bool>`，练习如何让新状态在多人客户端间同步。


[↑ 回到目录](#toc)


<a id="appendix"></a>

## 附录：核心文件速查表

### 角色与组件

| 文件 | 作用 |
| --- | --- |
| `Assets/Scripcts/Character/CharacterManager.cs` | 角色基类、网络生命周期 |
| `Assets/Scripcts/Character/Player/PlayerManager.cs` | 玩家装配、存档、回调订阅 |
| `Assets/Scripcts/Character/CharacterNetworkManager.cs` | 公共网络变量和 RPC |
| `Assets/Scripcts/Character/Player/PlayerNetworkManager.cs` | 玩家专属网络变量和回调 |
| `Assets/Scripcts/Character/CharacterLocomotionManager.cs` | 重力与地面检测 |
| `Assets/Scripcts/Character/Player/PlayerLocomotionManager.cs` | 玩家移动、跳跃、翻滚 |
| `Assets/Scripcts/Character/CharacterAnimatorManager.cs` | 动画参数和动作播放 |
| `Assets/Scripcts/Character/Player/PlayerAnimatorManager.cs` | Root Motion 与连招窗口 |
| `Assets/Scripcts/Character/CharacterStatsManager.cs` | 属性与体力恢复 |
| `Assets/Scripcts/Character/Player/PlayerStatsManager.cs` | 玩家属性计算 |
| `Assets/Scripcts/Character/CharacterCombatManager.cs` | 战斗公共状态 |
| `Assets/Scripcts/Character/Player/PlayerCombatManager.cs` | 玩家战斗逻辑 |
| `Assets/Scripcts/Character/CharacterEffectsManager.cs` | 效果处理公共逻辑 |
| `Assets/Scripcts/Character/Player/PlayerEffectsManager.cs` | 玩家效果处理 |
| `Assets/Scripcts/Character/Player/PlayerInputManager.cs` | 输入分发 |
| `Assets/Scripcts/Character/Player/PlayerCamera.cs` | 摄像机与锁定 |
| `Assets/Scripcts/Character/Player/PlayerInteractionManager.cs` | 交互物列表 |
| `Assets/Scripcts/Character/Player/PlayerBodyManager.cs` | 身体部件显隐 |



[↑ 回到目录](#toc)


### 物品、武器与装备

| 文件 | 作用 |
| --- | --- |
| `Assets/Scripcts/Item/Item.cs` | 物品基类 |
| `Assets/Scripcts/Item/Equipment Item/EquipmentItem.cs` | 装备基类 |
| `Assets/Scripcts/Item/Weapon Item/WeaponItem.cs` | 武器数据 |
| `Assets/Scripcts/Item/Weapon Item/Melee Weapon/MeleeWeaponItem.cs` | 近战武器 |
| `Assets/Scripcts/Item/Weapon Item/Ranged Weapon/RangedWeaponItem.cs` | 远程武器 |
| `Assets/Scripcts/Item/Weapon Item/Caster Weapon/CasterWeaponItem.cs` | 施法触媒 |
| `Assets/Scripcts/Item/Equipment Item/HeadEquipmentItem.cs` | 头部装备 |
| `Assets/Scripcts/Item/Equipment Item/BodyEquipmentItem.cs` | 身体装备 |
| `Assets/Scripcts/Item/Equipment Item/HandEquipmentItem.cs` | 手部装备 |
| `Assets/Scripcts/Item/Equipment Item/LegEquipmentItem.cs` | 腿部装备 |
| `Assets/Scripcts/Item/EquipmentModel/EquipmentModel.cs` | 装备模型匹配 |
| `Assets/Scripcts/Weapon Action/WeaponItemAction.cs` | 武器动作基类 |
| `Assets/Scripcts/Weapon Action/LightAttackWeaponItemAction.cs` | 轻攻击 |
| `Assets/Scripcts/Weapon Action/HeavyAttackWeaponItemAction.cs` | 重攻击 |
| `Assets/Scripcts/Character/Player/PlayerEquipmentManager.cs` | 武器和装备模型加载 |
| `Assets/Scripcts/Character/Player/PlayerInventoryManager.cs` | 玩家背包 |



[↑ 回到目录](#toc)


### 法术、远程与碰撞

| 文件 | 作用 |
| --- | --- |
| `Assets/Scripcts/Item/Spells/SpellItem.cs` | 法术基类 |
| `Assets/Scripcts/Item/Spells/FireBall.cs` | 火球术 |
| `Assets/Scripcts/Item/Spells/SpellManager/SpellManager.cs` | 法术管理器基类 |
| `Assets/Scripcts/Item/Spells/SpellManager/FireBallManager.cs` | 火球飞行逻辑 |
| `Assets/Scripcts/Item/Equipment Item/RangedProjectileItem.cs` | 远程弹药 |
| `Assets/Scripcts/Colliders/DamageCollider.cs` | 伤害碰撞体基类 |
| `Assets/Scripcts/Colliders/MeleeWeaponDamageCollider.cs` | 近战伤害碰撞体 |
| `Assets/Scripcts/Colliders/Projectiles/RangedProjectileDamageCollider.cs` | 远程弹药伤害 |
| `Assets/Scripcts/Colliders/Spells/SpellDamageCollider.cs` | 法术伤害基类 |
| `Assets/Scripcts/Colliders/Spells/FireBallDamageCollider.cs` | 火球伤害 |



[↑ 回到目录](#toc)


### AI 与 Boss

| 文件 | 作用 |
| --- | --- |
| `Assets/Scripcts/Character/AI Character/AICharacterManager.cs` | AI 角色基类 |
| `Assets/Scripcts/Character/AI Character/States/AIState.cs` | 状态基类 |
| `Assets/Scripcts/Character/AI Character/States/IdleState.cs` | 待机与索敌 |
| `Assets/Scripcts/Character/AI Character/States/PursueTargetState.cs` | 追击 |
| `Assets/Scripcts/Character/AI Character/States/CombatStanceState.cs` | 选择攻击 |
| `Assets/Scripcts/Character/AI Character/States/AttackState.cs` | 执行攻击 |
| `Assets/Scripcts/Character/AI Character/Actions/AICharacterAttackAction.cs` | AI 攻击数据 |
| `Assets/Scripcts/Character/AI Character/Boss/AIBossCharacterManager.cs` | Boss 状态与阶段 |
| `Assets/Scripcts/Character/AI Character/Boss/EventTriggerBossFight.cs` | Boss 战触发 |
| `Assets/Scripcts/Character/AI Character/Boss/BOSS01/AIBOSS01CharacterCombatManager.cs` | BOSS01 战斗实现 |
| `Assets/Scripcts/Character/AI Character/Boss/Knight of the Crucible/AIBOSS02CharacterCombatManager.cs` | BOSS02 战斗实现 |



[↑ 回到目录](#toc)


### 世界、存档、交互与 UI

| 文件 | 作用 |
| --- | --- |
| `Assets/Scripcts/WorldManager/WorldSaveGameManager.cs` | 存档与场景加载 |
| `Assets/Scripcts/Game Saving/CharacterSaveData.cs` | 存档数据结构 |
| `Assets/Scripcts/Game Saving/SaveFileDataWriter.cs` | JSON 文件读写 |
| `Assets/Scripcts/Game Saving/SerializableDictionary.cs` | 可序列化字典 |
| `Assets/Scripcts/WorldManager/WorldItemDatabase.cs` | 物品数据库 |
| `Assets/Scripcts/WorldManager/WorldAIManager.cs` | AI 网络生成 |
| `Assets/Scripcts/WorldManager/WorldObjectManager.cs` | 世界对象管理 |
| `Assets/Scripcts/WorldManager/WorldSoundFXManager.cs` | 全局音效与 BGM |
| `Assets/Scripcts/Object/Interactable.cs` | 交互物基类 |
| `Assets/Scripcts/Object/Pick Up Item/PickUpItemInteractable.cs` | 拾取物 |
| `Assets/Scripcts/Object/Site Of Graces/SiteOfGraceInteractable.cs` | 赐福点 |
| `Assets/Scripcts/Object/Fog Walls/FogWallInteractable.cs` | Boss 雾门 |
| `Assets/Scripcts/Character/Player/PlayerUI/PlayerUIManager.cs` | UI 管理器 |
| `Assets/Scripcts/Character/Player/PlayerUI/PlayerUIHudManager.cs` | HUD |
| `Assets/Scripcts/Character/Player/PlayerUI/PlayerUIEquipmentManager.cs` | 装备 UI |
| `Assets/Scripcts/ManuScene/TitleScreenManager.cs` | 主菜单 |
| `Assets/Scripcts/Enums.cs` | 全局枚举 |

---

> **学习提示**：这个项目适合按“继承关系 -> 单一系统 -> 完整流程 -> 网络扩展”的顺序读。第一遍不要试图理解所有细节，先能说出每套系统负责什么、数据从哪里来、状态如何变化，第二遍再逐行跟踪 RPC 和动画事件。



[↑ 回到目录](#toc)


