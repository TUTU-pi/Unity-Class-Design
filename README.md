># Unity 多合一游戏合集

> **南昌大学虚拟现实课程设计** | 基于 Unity 2022.3.62f3c1 | [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

---

## 项目简介

本项目是南昌大学虚拟现实课程设计作品，使用 **Unity 2022.3 LTS** 引擎打造，以第一人称射击（FPS）为核心玩法，同时附带一款基于强化学习的策略卡牌对战游戏。项目将 3D 沉浸式交互体验与 2D 策略博弈有机融合，涵盖了从登录系统到神经网络 AI 的完整技术栈。

### 核心特色

- **第一人称射击战场 (ShootGame)**：完整的 FPS 战斗循环 —— 敌人生成、AI 追击、枪械射击、击杀计数、传送门机关。消灭全部敌人后传送门激活，提供紧张刺激的枪战体验
- **3D 探索世界 (3DGame)**：自由探索的开放场景，集成物资收集、弹药补给、门禁机关、火焰陷阱、传送标签等丰富交互元素
- **策略卡牌对战 (GameScene)**：石头剪刀布主题的回合制卡牌博弈，包含 6 种技能牌（石之力量/布之守护/剪之锋芒/临阵换策/两极反转），搭配 **DQN 深度强化学习神经网络 AI** 作为对手
- **登录与存档系统**：完整的账号注册/登录流程，JSON 本地持久化存储

视频展示：[https://www.bilibili.com/video/BV1awNE6pEnN](https://www.bilibili.com/video/BV1awNE6pEnN)
游玩方式：解压Proj.zip文件夹并运行其中的ClassDesign.exe
---

## 目录

- [项目简介](#项目简介)
- [快速开始](#快速开始)
- [场景与功能](#场景与功能)
  - [1. 登录系统 (LoginScene)](#1-登录系统-loginscene)
  - [2. 模式选择 (ModeSelect)](#2-模式选择-modeselect)
  - [3. 3D 探索世界 (3DGame)](#3-3d-探索世界-3dgame)
  - [4. 射击战场 (ShootGame)](#4-射击战场-shootgame)
  - [5. 卡牌对战 (GameScene)](#5-卡牌对战-gamescene)
- [神经网络 AI 系统](#神经网络-ai-系统)
- [脚本目录结构](#脚本目录结构)
- [技术要点](#技术要点)

---

## 快速开始

```bash
# 克隆仓库
git clone https://github.com/TUTU-pi/Unity-Class-Design.git

# 使用 Unity Hub 打开项目
# 1. 安装 Unity 2022.3.62f3c1（含 WebGL Build Support）
# 2. 在 Unity Hub 中添加项目目录
# 3. 打开后双击 Assets/Scenes/LoginScene.unity 即可运行
```

**运行要求**:

- Unity 2022.3 LTS 系列（开发版本 2022.3.62f3c1）
- Windows / macOS 均可

**场景入口**:

| 场景       | 路径                             | 说明                           |
| ---------- | -------------------------------- | ------------------------------ |
| LoginScene | `Assets/Scenes/LoginScene.unity` | 程序入口，登录/注册            |
| ModeSelect | `Assets/Scenes/ModeSelect.unity` | 卡牌对战入口（开始/退出/规则） |
| 3DGame     | `Assets/Scenes/3DGame.unity`     | 3D 探索主场景                  |
| ShootGame  | `Assets/Scenes/ShootGame.unity`  | 第一人称射击战场               |
| GameScene  | `Assets/Scenes/GameScene.unity`  | 卡牌对战                       |



---

## 场景与功能

### 1. 登录系统 (LoginScene)

**场景路径**: `Assets/Scenes/LoginScene.unity`

**核心脚本**: `Assets/LoginScripts/LoginController.cs`、`AccountManager.cs`

功能描述:

- 用户注册与登录界面
- 账号数据以 JSON 格式持久化存储到 `Assets/LoginScripts/accounts.json`
- 注册成功后自动跳转回登录面板
- 登录成功后跳转到 3DGame 主场景
- 支持退出游戏按钮

<img width="1234" height="694" alt="40f8ea13-0e98-40c4-99af-24a4e252a8df" src="https://github.com/user-attachments/assets/d17a0946-6d00-47f2-9d47-35460b458051" />


**关键代码逻辑**:

| 文件                 | 类                | 说明                                         |
| -------------------- | ----------------- | -------------------------------------------- |
| `LoginController.cs` | `LoginController` | UI 交互：登录/注册/面板切换/场景跳转         |
| `AccountManager.cs`  | `AccountManager`  | 单例模式，管理账号列表的增删查改与 JSON 读写 |

---

### 2. 模式选择 (ModeSelect) —— 卡牌对战入口

**场景路径**: `Assets/Scenes/ModeSelect.unity`

**核心脚本**: `Assets/2DScripts/SelectUI.cs`

功能描述:

- 卡牌对战游戏的入口页面，也是登录后跳转到 3DGame 场景后可进入的页面
- 提供"开始游戏"按钮进入卡牌对战（GameScene）
- 提供"退出游戏"按钮返回 3DGame 主场景
- 内置规则说明面板（RulesUI），点击规则按钮可展开/收起查看游戏规则



<img width="1231" height="692" alt="cc04fbde-f2d0-4e68-809d-723e6873254a" src="https://github.com/user-attachments/assets/33373239-737c-4b52-b529-d54cd1fe7db7" />



---

### 3. 3D 探索世界 (3DGame)

**场景路径**: `Assets/Scenes/3DGame.unity`

这是项目的主要 3D 场景，集成了玩家控制、物资收集、环境交互和射击系统。

#### 3.1 玩家控制系统

**核心脚本**: `Assets/Script/pla.cs`

| 操作         | 按键            |
| ------------ | --------------- |
| 移动         | W/A/S/D         |
| 冲刺         | 左 Ctrl + 移动  |
| 跳跃         | Space（仅地面） |
| 视角旋转     | 鼠标移动        |
| 切换 F3 相机 | C               |
| 切换枪械     | J（掏出/收起）  |
| 打开技能面板 | Tab             |

- 第一人称视角，鼠标控制 Yaw/Pitch 旋转
- 地面检测使用 Raycast + LayerMask
- 支持第三人称自由相机模式（通过 `changeCamerabutton` 切换）
- 收起武器时禁用移动，仅可旋转视角

#### 3.2 枪械系统

**核心脚本**: `Assets/Script/Gun.cs`、`Bullet.cs`

- **射击**：鼠标左键发射子弹，支持射速限制（`fireRate`）
- **弹夹系统**：弹夹容量 `magbulletcount`（默认 30），总弹药 `total_bullets`（默认 200）
- **换弹**：按 R 键或弹夹打空自动换弹，播放换弹动画（1 秒）
- **子弹**：直线飞行，一定时间后自动销毁，固定飞行方向不受重力影响
- **弹药补给**：通过收集物资补充弹药

#### 3.3 物资收集系统

**核心脚本**: `Assets/Script/supply.cs`、`SupplyCreater.cs`

- 物资在地图范围内定时自动生成（默认每 20 秒）
- 生成范围可配置（X/Z/Y 范围）
- 最大同时存在数量限制（`maxSupplyCnt`，默认 5）
- 玩家接触到物资时：
  - 自动补充弹药至满（调用 `Gun.GetSupply()`）
  - 增加物资计数（`supplyCnt++`），在 UI 上实时显示
  - 物资 GameObject 销毁
- 物资持续自转，方便玩家辨识


<img width="1230" height="694" alt="59256031-c1d4-4517-b031-b32e869a2b3e" src="https://github.com/user-attachments/assets/2f344cbd-7c9e-452a-80eb-2daed9678653" />




#### 3.4 环境交互

**门系统 (`Door.cs`)**:

- 两种打开方式：碰撞进入 + 鼠标悬浮按 E
- 打开后播放开门动画 + 音效
- 需要玩家收集至少 4 个物资才能触发开门
- 开门后倒计时自动关闭
<img width="1234" height="693" alt="587b1b5f-cf05-4fbc-a8cf-24bd29ef68c1" src="https://github.com/user-attachments/assets/214fccc1-507d-4d67-ac55-ef9fce3136cd" />
<img width="1233" height="695" alt="ab21854d-0678-44e8-905f-a52dae952d69" src="https://github.com/user-attachments/assets/dce8fe97-78d5-4f2f-95dc-7b908f4634f3" />



**火焰机关 (`FireTrigger.cs`)**:

- 鼠标悬浮按 E 或碰撞触发开关
- 开启/关闭火焰效果，伴随循环音效
<img width="1235" height="695" alt="2b015569-249c-4d94-97c7-a2d4dbe226b7" src="https://github.com/user-attachments/assets/c6127bbb-973d-4de2-93e1-18d2213cb383" />
<img width="1223" height="694" alt="29c6f97f-cf1b-41e2-8b8d-e6876a3916d2" src="https://github.com/user-attachments/assets/963b18f1-a75e-438b-876a-793f59076812" />


**传送标签 (`Tag.cs`)**:

- 始终面向目标位置旋转
- 鼠标悬停 + 左键点击将玩家传送至指定位置
<img width="1232" height="695" alt="e6e82cda-0e8d-49ac-9459-4566ca3a62f7" src="https://github.com/user-attachments/assets/3b30325e-f4a1-4fb9-a2db-fe0126d3df7c" />
<img width="1234" height="696" alt="8d8ff8df-f027-455d-917c-cbc8dab5a4f1" src="https://github.com/user-attachments/assets/f35f4948-aa5e-4243-9c83-e6905141f91d" />



**自由相机 (`camera.cs`)**:

- WASD 移动 + 鼠标拖拽平移
- Q/E 围绕目标点旋转
- 滚轮缩放
- Ctrl 加速
<img width="1234" height="696" alt="2ea10788-13d1-482a-871e-5eed0cdd0b28" src="https://github.com/user-attachments/assets/22fb6032-b9bd-4fa6-8179-aedce5fd9ed3" />




**场景切换 (`GameController.cs`)**:

- 通用按钮点击加载指定场景
<img width="1230" height="692" alt="632eae7b-d43a-4445-af14-67078c61f678" src="https://github.com/user-attachments/assets/2b9b97d2-89de-48bb-ad07-d1bdcf3fedeb" />



**进入射击游戏入口 (`GetinShootGame.cs`)**:

- 玩家碰撞到入口时显示提示面板
- 离开时隐藏提示
<img width="1229" height="694" alt="f11b4d56-d4fe-407a-8e2c-d29bee06fa33" src="https://github.com/user-attachments/assets/0c3f2dca-e2e4-48e8-a5e9-22c0bc97c9ad" />
<img width="1233" height="691" alt="2ea52f24-16c3-4b8d-8351-485d9647a12c" src="https://github.com/user-attachments/assets/b2bdebb9-dac9-4486-b5f8-2389e0cac774" />


---

### 4. 射击战场 (ShootGame)
**场景路径**: `Assets/Scenes/ShootGame.unity`

独立的战斗竞技场场景，玩家需要消灭所有敌人来激活传送门。
<img width="1230" height="686" alt="84b920b5-fdb7-4c49-a98a-4c823bb64cbd" src="https://github.com/user-attachments/assets/ee904cc6-5102-4094-b741-40bac3327185" />


#### 4.1 敌人生成系统

**核心脚本**: `Assets/Script/EnemySpawner.cs`

| 参数            | 默认值 | 说明                   |
| --------------- | ------ | ---------------------- |
| `enemyPrefab`   | -      | 敌人预制体             |
| `spawnPoints`   | -      | 生成位置点数组         |
| `spawnRange`    | 3      | 在生成点周围的随机范围 |
| `spawnInterval` | 0.8s   | 每个敌人的生成间隔     |
| `totalEnemies`  | 10     | 需要消灭的总敌人数     |

- 在指定位置的随机范围内逐批生成敌人
- 追踪所有存活敌人
- 全部消灭后自动激活传送门

#### 4.2 敌人 AI

**核心脚本**: `Assets/Script/Enemy.cs`

- **追踪行为**：持续计算与玩家的方向，以 `moveSpeed`（默认 2.5）速度追击
- **停止距离**：距离玩家 `stopDistance`（默认 1）以内停止移动
- **受击判定**：被 tag 为 "bullet" 的物体碰撞后通知 Spawner 并销毁自身
- **玩家伤害**：碰到 tag 为 "Player" 的物体时触发玩家死亡

#### 4.3 击杀计数器

**核心脚本**: `Assets/Script/EnemySpawner.cs`（UpdateUI 方法）

- 实时显示格式：`消灭: X / Y`
- X = 已生成且已死亡的敌人数
- Y = 总需消灭的敌人数
- 需要场景中创建 UI Text 并拖入 `enemyCountText` 槽位





#### 4.4 传送门系统

**核心脚本**: `Assets/Script/Portal.cs`

- **初始状态**：隐藏（Renderer 和 Collider 均禁用）
- **激活条件**：所有敌人被消灭
- **激活效果**：
  - 显示 Renderer
  - 发射材质激活（`_EMISSION` keyword）
  - 颜色在红/绿/蓝/青/品红/黄之间循环渐变
  - 碰撞器变为可触发状态
- **交互效果**：玩家触碰激活的传送门 → 重新加载当前场景（完整重置）

<img width="1230" height="700" alt="67e56e0a-0e74-474f-99a6-352832af4d09" src="https://github.com/user-attachments/assets/8e71af2a-9a37-4789-8888-2314e029376a" />




#### 4.5 出口门

**核心脚本**: `Assets/Script/ExitDoor.cs`

- 鼠标悬停时显示提示 UI
- 按下 E 键加载 `3DGame` 场景

<img width="1228" height="696" alt="e7c9ffd1-c6d2-4bd2-9872-67bdb30e516b" src="https://github.com/user-attachments/assets/a027dfca-b7e8-4507-a5ee-a86c565462b2" />


#### 4.6 玩家死亡处理

**核心脚本**: `Assets/Script/PlayerDeathHandler.cs`

- 被敌人碰到时触发死亡
- 显示死亡面板 UI
- 禁用玩家移动脚本（`pla.enabled = false`）
- 将 Rigidbody 速度归零
- 等待 3 秒后自动加载 `3DGame` 场景

<img width="1231" height="696" alt="5889298f-2338-4fdc-82c4-bd49aed233de" src="https://github.com/user-attachments/assets/077e6995-0190-4b4e-92d1-15eed1ec9b9d" />



---

### 5. 卡牌对战 (GameScene)

**场景路径**: `Assets/Scenes/GameScene.unity`

2D 卡牌对战游戏，核心玩法为"石头剪刀布"的卡牌演绎版本，带技能系统和神经网络 AI 对手。

#### 5.1 基础规则

- 双方各持有 3 张手牌（石头/剪刀/布各 1 张）
- 每回合从手牌中选择 1 张出牌
- 石头 > 剪刀 > 布 > 石头，胜者得 1 分
- 先获得 6 分者赢得整局游戏
- 牌堆有 30 张牌（三种各 10 张），手牌不足时从公共牌池补充

#### 5.2 回合流程

```
阶段 A（出牌前技能）→ 选择出牌 → 阶段 B（出牌后技能）→ 揭示结果 → 补充手牌
```

1. **阶段 A**：可选择使用出牌前技能（石之力量/布之守护/剪之锋芒）
2. **出牌阶段**：从手牌中选择一张出牌
3. **阶段 B**：可选择使用出牌后技能（临阵换策/两极反转）
4. **结算阶段**：揭示双方选择和技能，判定胜负
5. **补牌阶段**：从公共牌池选择新牌补充手牌

#### 5.3 技能系统

| 技能     | 阶段 | 效果                     |
| -------- | ---- | ------------------------ |
| 石之力量 | A    | 选择出石头时获胜得 2 分  |
| 布之守护 | A    | 选择出布时获胜得 2 分    |
| 剪之锋芒 | A    | 选择出剪刀时获胜得 2 分  |
| 临阵换策 | B    | 收回已出牌，重新选择出牌 |
| 两极反转 | B    | 互换双方本轮得分         |

- 初始每人抽取 1 张技能牌
- 每回合结束后双方各获得 1 张新技能牌（最多持有 3 张）
- 使用后从列表中移除
- AI 也有相同的技能系统并且会自主决策使用

#### 5.4 UI 系统

**核心脚本**: `Assets/2DScripts/UIManager.cs`、`HandDisplay.cs`、`OpponentHandDisplay.cs`、`SkillHandDisplay.cs`、`PlayerSkillBar.cs`

- 顶部分数显示：`你 X : Y AI`
- 回合数显示
- 公共牌池（3 张可选牌）带高亮交互
- 手牌展示区
- 对手手牌展示（可见，无隐藏信息）
- 技能面板：阶段提示文字 + 可选技能卡牌 + 跳过按钮
- 出牌展示区（双方已出牌）
- 牌堆剩余数量
- 结果面板（胜负 + 最终比分 + 重开按钮）
- 暂停面板（Tab 键，可继续或退出）

<img width="1234" height="696" alt="e51aad25-a989-4ca9-82c8-8848a0aad2f6" src="https://github.com/user-attachments/assets/92693f4d-310a-48de-b8dc-3a56b6c7ae93" />
<img width="1235" height="695" alt="705d5f0a-b589-4e0d-b205-c9d32eb8b2df" src="https://github.com/user-attachments/assets/52a789ba-79f9-4065-b91c-275c6b0eeef3" />
<img width="1236" height="693" alt="ebe3c9e4-0e8a-42f6-82b5-d6e111750f78" src="https://github.com/user-attachments/assets/ba89414e-aa2d-4064-bb59-77e38ad10942" />
<img width="1233" height="697" alt="3fa3ef5b-492f-4747-8290-8aee5ca5570e" src="https://github.com/user-attachments/assets/2d42f671-7e1c-4694-8423-6f6665079f47" />



---

## 神经网络 AI 系统

### 模型架构

**核心脚本**: `Assets/Model/NeuralNetwork.cs`、`NeuralAIDriver.cs`

- 基于 DQN（Deep Q-Network）训练的石头剪刀布 AI

- 4 个独立的 Phase Network，分别对应 4 个决策阶段

- 每个 Phase Network 结构：

  ```
  Linear(inDim → 128) → BatchNorm1d → ReLU
  → Linear(128 → 128) → BatchNorm1d → ReLU
  → Linear(128 → outDim) → Q-values
  ```

- Dropout 层仅在训练时使用，推理时跳过

### 决策流程

| 阶段    | 输入                       | 输出                        | 说明                   |
| ------- | -------------------------- | --------------------------- | ---------------------- |
| Phase 1 | 手牌、比分、连败、技能     | Q(skip/Stone/Cloth/Scissor) | 决定是否使用出牌前技能 |
| Phase 2 | 手牌、比分、连败、技能状态 | Q(Rock/Scissor/Paper)       | 决定出哪张牌           |
| Phase 3 | 手牌、比分、已出牌、技能   | Q(skip/Change/Reverse)      | 决定是否使用出牌后技能 |
| Phase 4 | 手牌、公共池、技能         | Q(pos0/pos1/pos2)           | 决定选哪张公共牌       |

### 状态编码

- 手牌使用三进制编码（按 R→S→P 固定顺序展开）
- 比分限制在 0-5 范围（Clamp）
- 连败次数最大 2
- 技能槽位编码为技能类型 ID
- 与 Python 训练代码的编码方式完全一致

### 权重加载

- 权重以 JSON 格式存储为 TextAsset
- `NeuralNetwork.LoadWeights()` 从 JSON 解析 4 个 Phase Network 的权重矩阵
- 纯 C# 实现前向传播，无需外部依赖
- 对 JSON 中的矩阵格式（嵌套数组）有完整的解析支持

### 决策策略

- 从可用动作的 Q 值中选择最大值动作（greedy）
- 不可用动作会被过滤（如没有对应技能时不可选择该技能动作）
- 支持 `debugLog` 开关输出详细 Q 值，便于调试

---

## 脚本目录结构

```
Assets/
├── Script/                      # 3D 游戏核心脚本
│   ├── pla.cs                   # 玩家移动与控制
│   ├── Gun.cs                   # 枪械射击系统
│   ├── Bullet.cs                # 子弹行为
│   ├── Enemy.cs                 # 敌人 AI（射击游戏）
│   ├── EnemySpawner.cs          # 敌人生成器 + 击杀计数
│   ├── Portal.cs                # 传送门（变色 + 场景重置）
│   ├── ExitDoor.cs              # 出口门（悬停 + E 键）
│   ├── PlayerDeathHandler.cs    # 玩家死亡处理
│   ├── supply.cs                # 物资收集
│   ├── SupplyCreater.cs         # 物资生成器
│   ├── Door.cs                  # 门（碰撞 + 悬停交互）
│   ├── FireTrigger.cs           # 火焰机关
│   ├── Tag.cs                   # 传送标签
│   ├── camera.cs                # 自由观察相机
│   ├── MouseOver.cs             # 鼠标悬停变色
│   ├── GetinShootGame.cs        # 射击游戏入口
│   ├── GameController.cs        # 通用场景跳转
│   ├── Target.cs                # 靶子（子弹命中动画）
│   └── fire.cs                  # 火焰标记
│
├── 2DScripts/                   # 卡牌对战游戏脚本
│   ├── GameManager.cs           # 核心游戏状态机（单例，~930 行）
│   ├── UIManager.cs             # 全部 UI 更新
│   ├── Card.cs                  # 卡牌组件（点击交互）
│   ├── HandDisplay.cs           # 玩家手牌展示
│   ├── OpponentHandDisplay.cs   # 对手手牌展示
│   ├── SkillCardDisplay.cs      # 技能卡牌组件
│   ├── SkillHandDisplay.cs      # 技能面板展示
│   ├── PlayerSkillBar.cs        # 玩家技能栏
│   ├── SelectUI.cs              # 模式选择
│   └── Panel.cs                 # 面板动画控制
│
├── Model/                       # AI 模型
│   ├── NeuralNetwork.cs         # DQN 前向传播引擎（纯 C#）
│   └── NeuralAIDriver.cs        # AI 决策接口 + 状态编码
│
├── LoginScripts/                # 登录系统
│   ├── LoginController.cs       # 登录/注册 UI 交互
│   └── AccountManager.cs        # 账号数据管理
│
├── Editor/                      # 编辑器工具
│   └── SkillUISetup.cs          # 一键创建技能 UI
│
└── Scenes/                      # 场景文件
    ├── LoginScene.unity         # 登录场景
    ├── ModeSelect.unity         # 模式选择场景
    ├── 3DGame.unity             # 3D 探索主场景
    ├── ShootGame.unity          # 射击战斗场景
    └── GameScene.unity          # 卡牌对战场景
```

---

## 技术要点

### 设计模式

- **单例模式**：`GameManager`（卡牌游戏状态机）、`AccountManager`（账号管理）、`NeuralAIDriver`（AI 引擎）
- **观察者模式**：`Card.OnClicked`、`SkillCardDisplay.OnClicked` 使用 `System.Action` 委托回调
- **协同程序**：大量使用 `IEnumerator` + `yield return` 处理异步流程（敌人分波生成、技能阶段等待、死亡后延迟跳转、动画等待等）
- **状态机**：`GameManager` 用 `GameState` 枚举管理 6 种游戏状态

### 关键 Unity 特性

- `SceneManager.LoadScene` 场景切换与重置
- `OnTriggerEnter` / `OnCollisionEnter` 碰撞检测
- `OnMouseOver` / `OnMouseEnter` / `OnMouseExit` 鼠标交互
- `FindObjectOfType<T>` / `FindGameObjectWithTag` 运行时查找
- `[SerializeField]` 序列化私有字段，在 Inspector 中配置
- `[Header]` 属性分组整理 Inspector
- `Animator.SetTrigger` 控制动画状态
- `Material.EnableKeyword("_EMISSION")` 材质发光效果
- `Mathf.PingPong` + `Color.Lerp` 实现颜色循环渐变
- `Rigidbody.velocity` 控制物理移动
- `Raycast` 地面检测
- `Cursor.lockState` 锁定/释放鼠标

### 安全与健壮性

- 所有外部输入（用户名、密码）有空字符串检查
- 脚本对可能为 null 的引用（Inspector 赋值项、`FindObjectOfType` 返回值、`Find` 子物体结果）均进行了空值保护
- 单例模式使用 `Destroy(gameObject)` 防止重复实例
- `GetComponent` 结果使用前均有 null 判断
- 敌人被销毁前通过 `CleanupNullRefs` 清理列表中的空引用

---

## 许可证

本项目基于 [MIT License](LICENSE) 开源。

## 致谢

- **南昌大学** — 虚拟现实课程教学与指导
- **DQN 训练模型** — 基于 Python PyTorch 训练的深度强化学习权重，通过纯 C# 推理引擎集成到 Unity 中

---
## 版权与使用声明
1. 本项目为**南昌大学课程设计作业**，仅用于学习与课程考核，**非商业用途**。
2. 项目中部分模型、贴图、资源素材来源于课程授课老师提供的教学资源，**未经原授权允许，不保证通用开源商用资格**。
3. 本项目开放的代码逻辑、脚本、自研架构遵循 **MIT License** 开源；
   但**外部教学资源、素材、模型不参与开源商用**。
4. 任何人使用本仓库内容时，**禁止将项目素材、资源用于商业盈利、二次分发与公开商用项目**，仅可用于个人学习参考。
5. 若涉及资源版权问题，可联系本人进行下架处理。
---

