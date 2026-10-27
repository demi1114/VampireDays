using UnityEditor;
using UnityEngine;

/// <summary>
/// SkillDataのカスタムInspector。
/// スキルタイプに応じて設定項目を切り替える。
/// </summary>
[CustomEditor(typeof(SkillData))]
public class SkillDataEditor : Editor
{
    private SerializedProperty id;
    private SerializedProperty skillName;
    private SerializedProperty skillType;
    private SerializedProperty rarity;
    private SerializedProperty normalData;
    private SerializedProperty enhancementList;

    private void OnEnable()
    {
        id = serializedObject.FindProperty("id");
        skillName = serializedObject.FindProperty("skillName");
        skillType = serializedObject.FindProperty("skillType");
        rarity = serializedObject.FindProperty("rarity");
        normalData = serializedObject.FindProperty("normalData");
        enhancementList =
            serializedObject.FindProperty("enhancementList");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField(
            "基本情報",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(id);
        EditorGUILayout.PropertyField(skillName);
        EditorGUILayout.PropertyField(skillType);
        EditorGUILayout.PropertyField(rarity);

        EditorGUILayout.Space(10);

        SkillType type =
            (SkillType)skillType.enumValueIndex;

        EditorGUILayout.LabelField(
            "通常版",
            EditorStyles.boldLabel
        );

        DrawVariant(normalData, type);

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField(
            "強化版",
            EditorStyles.boldLabel
        );

        for (int i = 0; i < enhancementList.arraySize; i++)
        {
            SerializedProperty element =
                enhancementList.GetArrayElementAtIndex(i);

            SerializedProperty enhancementType =
                element.FindPropertyRelative("enhancementType");

            string title = enhancementType.enumDisplayNames[
                enhancementType.enumValueIndex
            ];

            EditorGUILayout.BeginVertical(
                EditorStyles.helpBox
            );

            EditorGUILayout.BeginHorizontal();

            element.isExpanded = EditorGUILayout.Foldout(
                element.isExpanded,
                title,
                true
            );

            if (GUILayout.Button("削除", GUILayout.Width(50)))
            {
                enhancementList.DeleteArrayElementAtIndex(i);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }

            EditorGUILayout.EndHorizontal();

            if (element.isExpanded)
            {
                DrawVariant(element, type);
            }

            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("強化形態を追加"))
        {
            int index = enhancementList.arraySize;

            enhancementList.InsertArrayElementAtIndex(index);

            SerializedProperty element =
                enhancementList.GetArrayElementAtIndex(index);

            element.FindPropertyRelative("enhancementType")
                .enumValueIndex =
                (int)EnhancementType.Emerald;
        }

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// スキルタイプに応じてVariantの項目を表示する。
    /// </summary>
    private void DrawVariant(
        SerializedProperty variant,
        SkillType type)
    {
        if (variant == null)
            return;

        DrawField(variant, "enhancementType");
        DrawField(variant, "icon");
        DrawField(variant, "description");

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField(
            "発動設定",
            EditorStyles.boldLabel
        );

        DrawField(variant, "coolTime");
        DrawField(variant, "spawnCount");
        DrawField(variant, "resetChance");
        DrawField(variant, "spawnPosition");

        SpawnPositionType position =
            (SpawnPositionType)variant
                .FindPropertyRelative("spawnPosition")
                .enumValueIndex;

        if (position == SpawnPositionType.Forward)
        {
            DrawField(variant, "forwardDistance");
        }
        else if (position == SpawnPositionType.RandomAround)
        {
            DrawField(variant, "randomRadius");
        }

        EditorGUILayout.Space(5);

        switch (type)
        {
            case SkillType.Summon:
                DrawSummonFields(variant);
                break;

            case SkillType.Magic:
                DrawMagicFields(variant);
                break;

            case SkillType.Passive:
                DrawPassiveFields(variant);
                break;
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField(
            "演出・音声",
            EditorStyles.boldLabel
        );

        DrawField(variant, "castEffect");
        DrawField(variant, "castSE");
        DrawField(variant, "destroySE");
    }

    private void DrawSummonFields(
        SerializedProperty variant)
    {
        EditorGUILayout.LabelField(
            "召喚設定",
            EditorStyles.boldLabel
        );

        DrawField(variant, "summonPrefab");
        DrawField(variant, "summonLifetime");

        EditorGUILayout.HelpBox(
            "召喚物の寿命です。0以下の場合は、" +
            "時間による自動消滅を行いません。",
            MessageType.Info
        );
    }

    private void DrawMagicFields(
        SerializedProperty variant)
    {
        EditorGUILayout.LabelField(
            "魔法設定",
            EditorStyles.boldLabel
        );

        DrawField(variant, "magicEffectPrefab");

        EditorGUILayout.HelpBox(
            "魔法エフェクトの再生終了は、" +
            "エフェクト側の共通処理で管理します。" +
            "Particle Systemを使用する場合は、" +
            "Loopingをオフにしてください。",
            MessageType.Info
        );
    }

    private void DrawPassiveFields(
        SerializedProperty variant)
    {
        EditorGUILayout.LabelField(
            "パッシブ設定",
            EditorStyles.boldLabel
        );

        DrawField(variant, "passiveEffectPrefab");
        DrawField(variant, "dropMultiplier");
        DrawField(variant, "recoveryMultiplier");
        DrawField(variant, "drainTimeMultiplier");
        DrawField(variant, "moveSpeedMultiplier");
        DrawField(variant, "magicCoolTimeReduction");
        DrawField(variant, "summonCoolTimeReduction");
    }

    private void DrawField(
        SerializedProperty parent,
        string propertyName)
    {
        SerializedProperty property =
            parent.FindPropertyRelative(propertyName);

        if (property != null)
        {
            EditorGUILayout.PropertyField(
                property,
                true
            );
        }
    }
}