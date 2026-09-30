# Unity FPS Prototype

这是一个可直接在 Unity 6 中打开并运行的单机第一人称射击原型。项目使用原始几何体自动生成场景，不需要额外素材或手动拖拽配置。

## 运行

1. 使用 Unity Hub 打开本仓库根目录。
2. 打开 `Assets/Scenes/Main.unity`。
3. 点击 Play。
4. 操作：WASD 移动，鼠标视角，左键射击，R 换弹，Shift 冲刺，Esc 释放鼠标。

游戏会自动创建地面、掩体、玩家、武器、敌人和 HUD。消灭全部敌人即可获胜；玩家生命值归零则失败。按 Enter 可重新开始。

## 兼容性

目标版本：Unity 6。核心逻辑在单个 `Assets/Scripts/FpsGame.cs` 文件中，使用 Unity 内置组件和旧输入 API，若项目启用了 Input System Only，请在 Player Settings > Active Input Handling 中选择 Both。

本项目是原创教学原型，不包含《三角洲》中的角色、地图、商标或素材。
