using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CZFramework.CZNode.Editor
{
    public class NodePreference
    {
        private static string lastKey = "CZNode.Settings";

        private static Settings settings;

        public static Settings Setting
        {
            get
            {
                if (settings == null)
                {
                    if (EditorPrefs.HasKey(lastKey))
                        settings = JsonUtility.FromJson<Settings>(EditorPrefs.GetString(lastKey));
                    else
                        settings = new Settings();
                }
                return settings;
            }
            private set { settings = value; }
        }

#if UNITY_2019_1_OR_NEWER
        [SettingsProvider]
        public static SettingsProvider CreateXNodeSettingsProvider()
        {
            SettingsProvider provider = new SettingsProvider("Preferences/CZNode", SettingsScope.User)
            {
                guiHandler = (searchContext) => { PreferencesGUI(); },
                keywords = new HashSet<string>(new[] {"CZNode", "node", "editor", "graph", "connections", "ports"})
            };
            return provider;
        }
#endif

#if !UNITY_2019_1_OR_NEWER
        [PreferenceItem("Node Editor")]
#endif
        private static void PreferencesGUI()
        {
            EditorGUI.BeginChangeCheck();
            Setting.min = EditorGUILayout.FloatField("Min", Setting.min);
            Setting.max = EditorGUILayout.FloatField("Max", Setting.max);

            if (EditorGUI.EndChangeCheck())
                SavePrefs(lastKey, Setting);
        }

        private static void SavePrefs(string key, Settings settings)
        {
            EditorPrefs.SetString(key, JsonUtility.ToJson(settings));
        }

        public static void OpenPreferences()
        {
            try
            {
#if UNITY_2018_3_OR_NEWER
                SettingsService.OpenUserPreferences("Preferences/CZNode");
#else
                //Open preferences window
                Assembly assembly = Assembly.GetAssembly(typeof(UnityEditor.EditorWindow));
                Type type = assembly.GetType("UnityEditor.PreferencesWindow");
                type.GetMethod("ShowPreferencesWindow", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);

                //Get the window
                EditorWindow window = EditorWindow.GetWindow(type);

                //Make sure custom sections are added (because waiting for it to happen automatically is too slow)
                FieldInfo refreshField = type.GetField("m_RefreshCustomPreferences", BindingFlags.NonPublic | BindingFlags.Instance);
                if ((bool) refreshField.GetValue(window))
                {
                    type.GetMethod("AddCustomSections", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(window, null);
                    refreshField.SetValue(window, false);
                }

                //Get sections
                FieldInfo sectionsField = type.GetField("m_Sections", BindingFlags.Instance | BindingFlags.NonPublic);
                IList sections = sectionsField.GetValue(window) as IList;

                //Iterate through sections and check contents
                Type sectionType = sectionsField.FieldType.GetGenericArguments()[0];
                FieldInfo sectionContentField = sectionType.GetField("content", BindingFlags.Instance | BindingFlags.Public);
                for (int i = 0; i < sections.Count; i++)
                {
                    GUIContent sectionContent = sectionContentField.GetValue(sections[i]) as GUIContent;
                    if (sectionContent.text == "Node Editor")
                    {
                        //Found contents - Set index
                        FieldInfo sectionIndexField = type.GetField("m_SelectedSectionIndex", BindingFlags.Instance | BindingFlags.NonPublic);
                        sectionIndexField.SetValue(window, i);
                        return;
                    }
                }

#endif
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.LogWarning("Unity has changed around internally. Can't open properties through reflection. Please contact xNode developer and supply unity version number.");
            }
        }

        public class Settings
        {
            public float min = 0.1f, max = 2;
        }
    }
}