namespace Vanilla
{
    using UnityEngine;
	#if  UNITY_EDITOR
    using UnityEditor;

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
        {
            bool wasEnabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUI.PropertyField(rect, prop, true);
            GUI.enabled = wasEnabled;
        }
    }
	#endif
    
    public class ReadOnlyAttribute : PropertyAttribute {}
}