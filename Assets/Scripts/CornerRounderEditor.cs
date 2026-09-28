#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace RoundedCorners
{
    [CustomEditor(typeof(CornerRounder))]
    public class CornerRounderEditor : Editor
    {
        private CornerRounder cornerRounder;

        private void OnEnable()
        {
            cornerRounder = (CornerRounder)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            bool independent = EditorGUILayout.Toggle(
                new GUIContent(
                    "Independent Corners",
                    "Allows each corner to have a different radius."
                ),
                cornerRounder.independent
            );

            EditorGUILayout.Space(8);

            if (independent)
            {
                EditorGUILayout.LabelField(
                    "Corner Radii",
                    EditorStyles.boldLabel
                );

                float topLeft = DrawRadiusField(
                    "Top Left",
                    cornerRounder.radiiSerialized.x
                );

                float topRight = DrawRadiusField(
                    "Top Right",
                    cornerRounder.radiiSerialized.y
                );

                float bottomRight = DrawRadiusField(
                    "Bottom Right",
                    cornerRounder.radiiSerialized.z
                );

                float bottomLeft = DrawRadiusField(
                    "Bottom Left",
                    cornerRounder.radiiSerialized.w
                );

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(
                        cornerRounder,
                        "Change Individual Corner Radii"
                    );

                    cornerRounder.independent = true;
                    cornerRounder.radiiSerialized = new Vector4(
                        topLeft,
                        topRight,
                        bottomRight,
                        bottomLeft
                    );

                    cornerRounder.Refresh();
                    EditorUtility.SetDirty(cornerRounder);
                }
            }
            else
            {
                float radius = DrawRadiusField(
                    "All Corners",
                    cornerRounder.radiiSerialized.x
                );

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(
                        cornerRounder,
                        "Change Corner Radius"
                    );

                    cornerRounder.independent = false;
                    cornerRounder.radiiSerialized = new Vector4(
                        radius,
                        radius,
                        radius,
                        radius
                    );

                    cornerRounder.Refresh();
                    EditorUtility.SetDirty(cornerRounder);
                }
            }

            EditorGUILayout.Space(8);

            if (GUILayout.Button("Refresh"))
            {
                Undo.RecordObject(
                    cornerRounder,
                    "Refresh Rounded Corners"
                );

                cornerRounder.Refresh();
                EditorUtility.SetDirty(cornerRounder);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private float DrawRadiusField(string label, float value)
        {
            value = EditorGUILayout.FloatField(label, value);
            return Mathf.Max(0f, value);
        }
    }
}

#endif