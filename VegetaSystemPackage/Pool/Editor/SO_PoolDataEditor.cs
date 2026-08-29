using UnityEditor;
using UnityEngine;

namespace VegetaSystem.Editor
{
    [CustomEditor(typeof(SO_PoolData))]
    public class SO_PoolDataEditor : UnityEditor.Editor
    {
        private enum PoolMode
        {
            Single,
            Multiple
        }

        private SerializedProperty isMultipleProp;
        private SerializedProperty poolItemsProp;
        private SerializedProperty prefabProp;
        private SerializedProperty initAmountProp;

        private void OnEnable()
        {
            isMultipleProp = serializedObject.FindProperty("IsMutilple");
            poolItemsProp = serializedObject.FindProperty("PoolItems");
            prefabProp = serializedObject.FindProperty("Prefab");
            initAmountProp = serializedObject.FindProperty("InitAmount");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PoolMode mode = isMultipleProp.boolValue ? PoolMode.Multiple : PoolMode.Single;

            EditorGUI.BeginChangeCheck();
            mode = (PoolMode)EditorGUILayout.EnumPopup("Pool Mode", mode);
            if (EditorGUI.EndChangeCheck())
            {
                isMultipleProp.boolValue = mode == PoolMode.Multiple;
            }

            EditorGUILayout.Space(6);

            if (mode == PoolMode.Single)
            {
                EditorGUILayout.LabelField("Single Prefab", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(prefabProp);
                EditorGUILayout.PropertyField(initAmountProp);
            }
            else
            {
                EditorGUILayout.LabelField("Multiple Prefabs", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(poolItemsProp, true);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
