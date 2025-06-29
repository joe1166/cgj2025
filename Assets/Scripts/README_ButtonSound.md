# 按钮音效系统使用说明

## 概述
这个系统为Unity项目中的按钮添加悬停和点击音效功能。

## 组件说明

### 1. ButtonSoundManager
- **功能**: 为单个按钮添加音效功能
- **接口**: `IPointerEnterHandler`, `IPointerClickHandler`
- **自动功能**: 
  - 鼠标悬停时播放悬停音效
  - 鼠标点击时播放点击音效
  - 防重复播放机制

### 2. ButtonSoundAutoSetup
- **功能**: 自动为场景中的所有按钮添加音效管理器
- **自动功能**: 
  - 在Start时自动设置所有按钮
  - 可配置默认音效设置
  - 避免重复添加组件

## 使用方法

### 方法一：自动设置（推荐）
1. 在场景中创建一个空的GameObject
2. 添加 `ButtonSoundAutoSetup` 组件
3. 配置默认音效设置：
   - `defaultHoverSound`: 默认悬停音效名称
   - `defaultClickSound`: 默认点击音效名称
   - `defaultHoverVolume`: 默认悬停音量
   - `defaultClickVolume`: 默认点击音量

### 方法二：手动设置
1. 选择要添加音效的按钮
2. 添加 `ButtonSoundManager` 组件
3. 配置音效设置：
   - `hoverSoundName`: 悬停音效名称
   - `clickSoundName`: 点击音效名称
   - `hoverVolume`: 悬停音量
   - `clickVolume`: 点击音量
   - `hoverCooldown`: 悬停音效冷却时间

## 音效文件要求
确保在 `AudioManager` 中已经配置了以下音效：
- `ButtonHover`: 按钮悬停音效
- `ButtonClick`: 按钮点击音效

## 配置示例

### AudioManager配置
```csharp
// 在AudioManager的sfxClips数组中添加：
- ButtonHover.audioClip
- ButtonClick.audioClip
```

### 自动设置配置
```csharp
ButtonSoundAutoSetup autoSetup = gameObject.AddComponent<ButtonSoundAutoSetup>();
autoSetup.defaultHoverSound = "ButtonHover";
autoSetup.defaultClickSound = "ButtonClick";
autoSetup.defaultHoverVolume = 0.8f;
autoSetup.defaultClickVolume = 1.0f;
```

### 手动设置配置
```csharp
ButtonSoundManager soundManager = button.gameObject.AddComponent<ButtonSoundManager>();
soundManager.hoverSoundName = "ButtonHover";
soundManager.clickSoundName = "ButtonClick";
soundManager.hoverVolume = 0.8f;
soundManager.clickVolume = 1.0f;
soundManager.hoverCooldown = 0.1f;
```

## 注意事项
1. 确保 `AudioManager` 已经正确配置并运行
2. 音效文件名称要与 `AudioManager` 中的配置一致
3. 悬停音效有冷却时间，避免重复播放
4. 只有在按钮可交互时才会播放音效
5. 可以手动调用 `PlayHoverSoundManually()` 和 `PlayClickSoundManually()` 方法

## 扩展功能
- 可以为不同类型的按钮设置不同的音效
- 可以调整音效的音量和冷却时间
- 支持动态添加和移除音效管理器 