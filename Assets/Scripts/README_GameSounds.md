# 游戏音效系统配置说明

## 概述
这个系统为游戏中的各种交互添加了音效反馈，提升游戏体验。

## 音效列表

### 按钮音效
- `ButtonHover`: 按钮悬停音效
- `ButtonClick`: 按钮点击音效

### 物品交互音效
- `ItemDragStart`: 物品拖拽开始音效
- `ItemPlaceSuccess`: 物品放置成功音效
- `ItemPlaceFail`: 物品放置失败音效
- `ItemRun`: 物品跑动音效
- `SettleEnd`: settle时间结束音效

## 音效配置要求

### AudioManager配置
确保在 `AudioManager` 的 `sfxClips` 数组中添加以下音效文件：

```csharp
// 按钮音效
- ButtonHover.audioClip
- ButtonClick.audioClip

// 物品交互音效
- ItemDragStart.audioClip
- ItemPlaceSuccess.audioClip
- ItemPlaceFail.audioClip
- ItemRun.audioClip
- SettleEnd.audioClip
```

## 音效触发时机

### 按钮音效
- **悬停音效**: 鼠标悬停在按钮上时播放
- **点击音效**: 鼠标点击按钮时播放

### 物品交互音效
- **拖拽开始**: 开始拖拽物品时播放
- **放置成功**: 物品放置到正确位置时播放
- **放置失败**: 物品放置到错误位置时播放
- **物品跑动**: 物品移动时定期播放（有间隔控制）
- **Settle结束**: 物品settle时间结束时播放

## 音效参数配置

### DraggableItem音效参数
```csharp
[Header("音效设置")]
public string dragStartSound = "ItemDragStart";
public string placeSuccessSound = "ItemPlaceSuccess";
public string placeFailSound = "ItemPlaceFail";
public float dragStartVolume = 0.8f;
public float placeSuccessVolume = 1.0f;
public float placeFailVolume = 0.7f;
```

### MovableItem音效参数
```csharp
[Header("音效设置")]
public string itemRunSound = "ItemRun";
public string settleEndSound = "SettleEnd";
public float itemRunVolume = 0.6f;
public float settleEndVolume = 0.8f;
public float runSoundInterval = 0.5f; // 跑动音效播放间隔
```

### Level0Item音效参数
```csharp
[Header("音效设置")]
public string dragStartSound = "ItemDragStart";
public string placeSuccessSound = "ItemPlaceSuccess";
public string placeFailSound = "ItemPlaceFail";
public string itemRunSound = "ItemRun";
public string settleEndSound = "SettleEnd";
public float dragStartVolume = 0.8f;
public float placeSuccessVolume = 1.0f;
public float placeFailVolume = 0.7f;
public float itemRunVolume = 0.6f;
public float settleEndVolume = 0.8f;
public float runSoundInterval = 0.5f;
```

## 音效文件建议

### 音效类型建议
- **ButtonHover**: 轻柔的"滴"声或"嗡"声
- **ButtonClick**: 清脆的点击声
- **ItemDragStart**: 物品被拿起的声音
- **ItemPlaceSuccess**: 物品放置成功的确认音
- **ItemPlaceFail**: 物品放置失败的提示音
- **ItemRun**: 物品移动的脚步声或移动音效
- **SettleEnd**: 物品重新激活的音效

### 音效时长建议
- 按钮音效: 0.1-0.3秒
- 物品交互音效: 0.2-0.8秒
- 跑动音效: 0.3-0.6秒

### 音量建议
- 按钮音效: 0.7-0.9
- 拖拽音效: 0.6-0.8
- 放置音效: 0.8-1.0
- 跑动音效: 0.4-0.6

## 注意事项
1. 确保所有音效文件都已添加到AudioManager中
2. 音效文件名称要与代码中的配置一致
3. 跑动音效有间隔控制，避免过于频繁播放
4. 可以根据需要调整音效的音量和播放间隔
5. 音效文件格式建议使用WAV或MP3格式

## 扩展功能
- 可以为不同类型的物品设置不同的音效
- 可以根据游戏状态调整音效音量
- 支持音效的淡入淡出效果
- 可以添加3D音效支持 