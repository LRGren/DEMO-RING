# DEMO RING 新武器接入流程

新增武器或补动作时，按以下顺序完成。优先用现有武器 Action 和 Humanoid 状态。武器默认动作合适时直接继承，同时保留独立 Override。

## 1. 列动作清单

- 明确武器支持的持法。Main 是主手单持，Off 是单手副手，TH 是双手共持一把，DW 是左右手各持一把。拳套这类由一件装备生成另一只模型的武器使用成对装备。
- 按项目能触发的状态列待机、轻击连段、重击蓄力与释放、跑攻、翻滚攻、后撤攻和跳攻。有需要时加处决、背刺或盾反。
- 列出状态名、源 FBX 或 DSAS ID 和对应的英文文件名。缺片先列出来，再选接得上的现有动作作为替代。已有可用 FBX 时，连同同名 `.meta` 一起处理。

## 2. 建武器资产和 Override

1. 在 `Assets/Data/Items/Weapons` 对应目录建武器资产，分配唯一 `itemID`，填模型、图标、数值和 Action，加入 `World Item Database.prefab`。可按测试需要给起始职业加装备入口。临时模型、图标或数值记在交付说明里。
2. 有独立分类时才新增 `WeaponClass`。需要同组配对时设置 `DualWieldGroup`。一件装备生成两件模型时按玩法设置 `isPairedWeapon` 和左右挂点。追加枚举值时保留已有序号。
3. 在 `Assets/Data/Animator Controller/_Override Animator` 建独立 Override，Base 指向 `Humanoid.controller`。只绑定需要替换的动作。现有状态缺入口时，在对应的 Sub-State Machine 增加状态。
4. 从装备和输入的正常入口检查 Action 可达。模型占位期间也要核对挂点和碰撞体。

## 3. 选片、导入和裁切

从游戏源动画重新制作 FBX 时，按 [c0000 动画批量转换交接](<D:/Gamings/eldenring/unpack/animation/player/EldenRingAnimationTransfer/README.md>) 的「0. 从 DSAS 开始首次导出」操作。工具的批量素材目录是 `D:\Gamings\eldenring\unpack\newAnim`。已有 FBX 的导入步骤在同一文档第 4 节。

1. 对新源做重定向前，核对 `Player.prefab` 使用的 Avatar。当前项目的 DSAS 配置是 `Profiles/DEMO-RING-Player.json`，使用 `ModularCharactersAvatar`，手指默认参考单持待机 `core_main_idle_01` 第 40 帧。转换前预览源 ID 和英文文件名。
2. Unity 中逐帧看起手、主挥击、命中、收势，以及接回待机的姿势。核对源帧率后记录裁切范围。剪去已经静止的尾帧，给过渡留少量余量。滞空优先用姿势匹配的 Hold/Idle。单帧左右的短片段播一次后保持，避免短循环抖动。
3. 使用正确的 Humanoid Avatar。导入默认根旋转和 Y 烘焙进姿势，基于 Original，偏移为 0。XZ 保留根位移，基于 Center of Mass。默认关闭 Loop Time、Mirror 和 Additive Reference Pose。实际需要循环的待机或 Hold 按片段配置，并确认 Unity Animation 页可编辑这些选项。
4. 按 `<weapon>_<grip>_<action>_<nn>` 命名，分段可加 `_start`、`_hold`、`_land` 或 `_release`。在工具目录的 `Profiles/UnityVariants` 为每种武器建独立配置，记录源帧、目标状态和事件。先预览，再运行 `--apply` 和 `--check`。替换已有引用时核对 GUID 和 Clip local file ID。

## 4. 接入跳攻

| 输入时机 | 动画顺序 | 判定归属 |
|---|---|---|
| 预计距触地超过 **0.55 秒** | 播空中 Start，有剩余滞空时间则接 Hold/Idle。无论播到哪里，触地以 **0.08 秒**混合到 Land。 | 空中挥击开判定，Land 收势和关判定。 |
| 预计 **0.55 秒内**触地 | 缓冲输入。确认触地后直接从 Ground Attack 开始，以 **0.10 秒**混合接入。 | Ground Attack 包含这次完整挥击和事件。 |

Start 和 Land 默认 **1×**。分流依据预计触地时间。普通跳轻优先继承 Base。直剑是现有参考，空中动作与 Land 继承 Humanoid 默认，晚按重击和双持跳攻通过 Override 借用斧头的 Ground Attack。

## 5. 配置动画事件

- 按实际挥击帧设置命中窗口。双持分别核对 `OpenLeftDamageCollider` 和 `OpenRightDamageCollider`。攻击窗口结束和触地收势时核对 `CloseDamageCollider`。
- 一次攻击只扣一次耐力。空中挥击和晚按 Ground Attack 分别有自己的事件。Hold 与 Land 负责保持和收势。
- 在能续招的帧打开连击窗口，核对下一段开始或动作重置时会关闭。每次裁切后重新核对事件的源帧和播放时机。

## 6. 验收和交付

- **资产验证**　核对装备、挂点、碰撞体、实际导入的 Humanoid Clip、Override、根位移和事件。C# 有改动时编译。
- **Play Mode**　试各持法能触发的轻击连段、蓄力释放、跑攻、翻滚攻和后撤攻。检查命中、耐力和收招后恢复移动。跳攻分别试早按、晚按、提前触地及 Start 正在接 Hold 时触地，补测斜坡或台阶、换武器和受击打断。双持分别确认两手都能打中。
- **联机**　改了 RPC、持法或模型同步时，用主机和客户端核对对方看到的动作和 Override。
- **交付说明**　列出已接动作、缺片、替代项和改动过的代码与资产，分别说明导入验证、Unity 批处理和 Play Mode 实测的覆盖范围。
