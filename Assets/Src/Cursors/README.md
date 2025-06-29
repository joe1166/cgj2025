# 鼠标样式设置

这个文件夹用来存放游戏中使用的鼠标样式图片。

## 使用说明

1. **准备鼠标图片**：
   - 将你的两张鼠标图片（默认状态和按下状态）放入此文件夹
   - 推荐使用PNG格式，支持透明背景
   - 建议尺寸：32x32像素或64x64像素

2. **设置图片导入**：
   - 选中鼠标图片文件
   - 在Inspector中将Texture Type设置为"Cursor"
   - 点击Apply应用设置

3. **配置CursorManager**：
   - 在Unity场景中找到CursorManager GameObject
   - 将默认状态鼠标图片拖拽到"Default Cursor Texture"字段
   - 将按下状态鼠标图片拖拽到"Pressed Cursor Texture"字段
   - 调整Hotspot位置（热点，即鼠标点击的实际位置）

4. **热点设置**：
   - 默认热点位置是图片中心点
   - 如果你的鼠标图片有特殊的点击位置（比如箭头的尖端），需要手动调整hotspot值
   - hotspot使用像素坐标，(0,0)是图片左上角

## 文件命名建议

- `cursor_default.png` - 默认状态鼠标
- `cursor_pressed.png` - 按下状态鼠标

## 调试

- 在CursorManager组件中勾选"Show Debug Info"可以在控制台看到鼠标状态切换的日志信息 