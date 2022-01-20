using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Easy.Utilities
{
    public class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        private static T _instance;
    
        public static T instance
        {
            get
            {
                if (_instance == null)
                {
                    string typeAsPath = typeof(T).ToString().Replace('.', '/');
                    _instance = Resources.Load<T>(typeAsPath);

                    if (_instance == null)
                    {
                        _instance = CreateInstance<T>();
#if UNITY_EDITOR
                        string path = Path.Combine(Application.dataPath, "Resources", typeAsPath + ".asset");
                        string directory = Path.GetDirectoryName(path);

                        if (!Directory.Exists(directory))
                            Directory.CreateDirectory(directory);

                        string relativePath = path.Substring(path.IndexOf(@"Assets\"));
                        AssetDatabase.CreateAsset(_instance, relativePath);
#endif
                    }
                }
                return _instance;
            }
        }
    }
}