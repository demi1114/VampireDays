
using UnityEngine;
using UnityEditor;

/// <summary>
/// SkillData専用のカスタムInspector。
/// スキル種類に応じて表示する項目を切り替える。
/// </summary>
[CustomEditor(typeof(SkillData))]
public class SkillDataEditor : Editor
{
    private SerializedProperty idProp;
    private SerializedProperty skillNameProp;
    private SerializedProperty skillTypeProp;
    private SerializedProperty normalDataProp;
    private SerializedProperty enhancementListProp;
    private SerializedProperty rarityProp;

    private void OnEnable()
    {
        idProp = serializedObject.FindProperty("id");
        skillNameProp = serializedObject.FindProperty("skillName");
        skillTypeProp = serializedObject.FindProperty("skillType");
        normalDataProp = serializedObject.FindProperty("normalData");
        enhancementListProp =
            serializedObject.FindProperty("enhancementList");
        rarityProp = serializedObject.FindProperty("rarity");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 基本情報
        EditorGUILayout.LabelField(
            "基本情報",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(idProp, new GUIContent("ID"));
        EditorGUILayout.PropertyField(
            skillNameProp,
            new GUIContent("スキル名"));

        EditorGUILayout.PropertyField(
            skillTypeProp,
            new GUIContent("スキル種類"));

        EditorGUILayout.PropertyField(
            rarityProp,new GUIContent("レア度"));

        EditorGUILayout.Space(10);

        // 通常版
        EditorGUILayout.LabelField(
            "通常版",
            EditorStyles.boldLabel
        );

        DrawVariant(normalDataProp);

        EditorGUILayout.Space(10);

        // 強化版一覧
        EditorGUILayout.LabelField(
            "強化版一覧",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(
            enhancementListProp,
            new GUIContent("強化形態"),
            false
        );

        if (enhancementListProp.isExpanded)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(
                enhancementListProp.FindPropertyRelative("Array.size"),
                new GUIContent("要素数")
            );

            for (int i = 0;
                 i < enhancementListProp.arraySize;
                 i++)
            {
                SerializedProperty element =
                    enhancementListProp.GetArrayElementAtIndex(i);

                SerializedProperty typeProp =
                    element.FindPropertyRelative("enhancementType");

                string typeName = "強化版 " + (i + 1);

                if (typeProp != null)
                {
                    typeName =
                        typeProp.enumDisplayNames[
                            typeProp.enumValueIndex
                        ];
                }

                element.isExpanded = EditorGUILayout.Foldout(
                    element.isExpanded,
                    typeName,
                    true
                );

                if (element.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    DrawVariant(element);
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// スキル種類に応じてVariantの項目を表示する。
    /// </summary>
    private void DrawVariant(SerializedProperty variant)
    {
        if (variant == null)
            return;

        SkillData data = (SkillData)target;
        SkillType type = data.skillType;

        EditorGUI.indentLevel++;

        // 共通項目
        EditorGUILayout.LabelField(
            "表示設定",
            EditorStyles.boldLabel
        );

        DrawField(variant, "enhancementType", "強化形態");
        DrawField(variant, "icon", "アイコン");
        DrawField(variant, "description", "説明");

        EditorGUILayout.Space(5);

        if (type == SkillType.Passive)
        {
            //========================================
            // パッシブスキル
            //========================================

            EditorGUILayout.LabelField(
                "パッシブ効果",
                EditorStyles.boldLabel
            );

            DrawField(variant, "dropMultiplier", "血液ドロップ倍率");
            DrawField(variant, "recoveryMultiplier", "回復倍率");
            DrawField(variant, "drainTimeMultiplier", "吸血時間倍率");
            DrawField(variant, "moveSpeedMultiplier", "移動速度倍率");

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField(
                "CT短縮効果",
                EditorStyles.boldLabel
            );

            DrawField(
                variant,
                "magicCoolTimeReduction",
                "魔法CT短縮率"
            );

            DrawField(
                variant,
                "summonCoolTimeReduction",
                "召喚CT短縮率"
            );
        }
        else
        {
            //========================================
            // 召喚・魔法スキル
            //========================================

            EditorGUILayout.LabelField(
                "基本性能",
                EditorStyles.boldLabel
            );

            DrawField(variant, "coolTime", "CT");
            DrawField(variant, "duration", "持続時間");
            DrawField(variant, "spawnCount", "生成数");
            DrawField(variant, "resetChance", "CTリセット確率");

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField(
                "生成設定",
                EditorStyles.boldLabel
            );

            DrawField(variant, "spawnPosition", "生成位置");

            SerializedProperty spawnPositionProp =
                variant.FindPropertyRelative("spawnPosition");

            if (spawnPositionProp != null)
            {
                SpawnPositionType positionType =
                    (SpawnPositionType)spawnPositionProp.enumValueIndex;

                if (positionType == SpawnPositionType.Forward)
                {
                    DrawField(
                        variant,
                        "forwardDistance",
                        "前方距離"
                    );
                }
                else if (
                    positionType == SpawnPositionType.RandomAround)
                {
                    DrawField(
                        variant,
                        "randomRadius",
                        "ランダム生成半径"
                    );
                }
            }

            DrawField(variant, "prefab", "生成Prefab");
            DrawField(variant, "lifeTime", "生成物の寿命");

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField(
                "演出",
                EditorStyles.boldLabel
            );

            DrawField(variant, "castEffect", "発動エフェクト");
            DrawField(variant, "castSE", "発動SE");
            DrawField(variant, "destroySE", "消滅SE");
        }

        EditorGUI.indentLevel--;
    }

    /// <summary>
    /// 指定したSerializedPropertyを表示する。
    /// </summary>
    private void DrawField(
        SerializedProperty parent,
        string propertyName,
        string label)
    {
        SerializedProperty property =
            parent.FindPropertyRelative(propertyName);

        if (property != null)
        {
            EditorGUILayout.PropertyField(
                property,
                new GUIContent(label),
                true
            );
        }
    }
}
