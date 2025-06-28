using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class ItemDataCreator : EditorWindow
{
    [MenuItem("Tools/Create Item Data")]
    static void Init()
    {
        var window = GetWindow<ItemDataCreator>();
        window.Show();
    }

    void OnGUI()
    {
        if (GUILayout.Button("Generate From Selected Object"))
        {
            if (Selection.activeGameObject == null)
            {
                Debug.LogError("No object selected");
                return;
            }

            var transform = Selection.activeGameObject.transform;
            var data = ScriptableObject.CreateInstance<ItemData>();

            // 初始化correctPositions列表并添加当前位置
            data.correctPositions = new List<Vector2>();
            data.correctPositions.Add(transform.position);

            // 直接保存原始类型
            data.rotation = transform.rotation;
            data.scale = transform.localScale;

            // 获取并保存Sprite
            var spriteRenderer = Selection.activeGameObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                data.itemSprite = spriteRenderer.sprite;
            }
            else
            {
                Debug.LogWarning("Selected object doesn't have a SpriteRenderer with a sprite!");
            }

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Item Data",
                "NewItemData",
                "asset",
                "Select save location");

            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(data, path);
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = data;
            }
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Add Position to Existing ItemData"))
        {
            if (Selection.activeGameObject == null)
            {
                Debug.LogError("No object selected");
                return;
            }

            // 选择现有的ItemData文件
            string itemDataPath = EditorUtility.OpenFilePanel(
                "Select ItemData Asset",
                "Assets",
                "asset");

            if (!string.IsNullOrEmpty(itemDataPath))
            {
                // 将绝对路径转换为项目相对路径
                if (itemDataPath.StartsWith(Application.dataPath))
                {
                    itemDataPath = "Assets" + itemDataPath.Substring(Application.dataPath.Length);
                }

                // 加载ItemData
                ItemData itemData = AssetDatabase.LoadAssetAtPath<ItemData>(itemDataPath);
                if (itemData != null)
                {
                    // 确保correctPositions列表已初始化
                    if (itemData.correctPositions == null)
                    {
                        itemData.correctPositions = new List<Vector2>();
                    }

                    // 添加当前位置到正确位置列表
                    Vector2 currentPosition = Selection.activeGameObject.transform.position;
                    itemData.correctPositions.Add(currentPosition);

                    // 标记为已修改并保存
                    EditorUtility.SetDirty(itemData);
                    AssetDatabase.SaveAssets();

                    Debug.Log($"已将位置 {currentPosition} 添加到 {itemData.name} 的正确位置列表中");
                    EditorUtility.FocusProjectWindow();
                    Selection.activeObject = itemData;
                }
                else
                {
                    Debug.LogError("选择的文件不是有效的ItemData资源！");
                }
            }
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Merge ItemData Positions to PositionManager"))
        {
            // 使用ObjectSelector来选择多个ItemData文件
            // 由于Unity的限制，我们需要逐个选择文件
            List<ItemData> selectedItemData = new List<ItemData>();

            // 显示提示信息
            EditorUtility.DisplayDialog(
                "选择ItemData文件",
                "请在弹出的Project窗口中按住Ctrl键选择多个ItemData文件，然后点击此按钮继续。\n\n或者你可以逐个选择文件。",
                "确定");

            // 获取当前选中的对象
            Object[] selectedObjects = Selection.objects;
            foreach (Object obj in selectedObjects)
            {
                if (obj is ItemData)
                {
                    selectedItemData.Add(obj as ItemData);
                }
            }

            if (selectedItemData.Count == 0)
            {
                Debug.LogError("请先选择ItemData文件！");
                return;
            }

            List<Vector2> allPositions = new List<Vector2>();
            List<string> loadedItemNames = new List<string>();

            foreach (ItemData itemData in selectedItemData)
            {
                if (itemData != null && itemData.correctPositions != null)
                {
                    // 添加所有位置到合并列表
                    allPositions.AddRange(itemData.correctPositions);
                    loadedItemNames.Add(itemData.name);

                    Debug.Log($"已加载 {itemData.name} 的 {itemData.correctPositions.Count} 个位置");
                }
                else
                {
                    Debug.LogWarning($"无法加载ItemData: {itemData.name}");
                }
            }

            if (allPositions.Count > 0)
            {
                // 查找场景中的PositionManager
                PositionManager positionManager = FindObjectOfType<PositionManager>();
                if (positionManager != null)
                {
                    // 设置合并后的位置列表
                    positionManager.correctPositions = allPositions;

                    // 标记为已修改
                    EditorUtility.SetDirty(positionManager);

                    Debug.Log($"成功合并 {loadedItemNames.Count} 个ItemData的位置，共 {allPositions.Count} 个位置");
                    Debug.Log($"已设置到PositionManager: {positionManager.name}");
                    Debug.Log($"加载的ItemData: {string.Join(", ", loadedItemNames)}");

                    // 选中PositionManager
                    Selection.activeGameObject = positionManager.gameObject;
                }
                else
                {
                    Debug.LogError("场景中没有找到PositionManager！请确保场景中有PositionManager组件。");
                }
            }
            else
            {
                Debug.LogWarning("没有找到任何有效的位置数据！");
            }
        }
    }
}
