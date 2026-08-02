using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using BehindTheScenesFootball.Managers;
using BehindTheScenesFootball.UI;

namespace BehindTheScenesFootball.Editor
{
    public class EditorSceneSetup : EditorWindow
    {
        [MenuItem("Perde Arkası Futbol/Setup Game Scene")]
        public static void SetupScene()
        {
            // 1. Create or Find GameManager GameObject
            GameObject gameManagerObj = GameObject.Find("_GameManagers");
            if (gameManagerObj == null)
            {
                gameManagerObj = new GameObject("_GameManagers");
                Undo.RegisterCreatedObjectUndo(gameManagerObj, "Create Game Managers");
                Debug.Log("Created '_GameManagers' GameObject.");
            }

            // 2. Attach required manager components
            AddManagerComponent<DatabaseManager>(gameManagerObj);
            AddManagerComponent<AgencyManager>(gameManagerObj);
            AddManagerComponent<SimulationEngine>(gameManagerObj);
            AddManagerComponent<UIManager>(gameManagerObj);

            // 3. Setup Main Camera background
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.clearFlags = CameraClearFlags.SolidColor;
                mainCam.backgroundColor = new Color(11f / 255f, 12f / 255f, 16f / 255f, 1f); // Dark background
                Undo.RecordObject(mainCam, "Setup Main Camera Color");
                Debug.Log("Main Camera background color configured.");
            }

            // 4. Mark scene as dirty so the changes are saved
            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }

            EditorUtility.DisplayDialog(
                "Kurulum Başarılı",
                "Oyun motoru yöneticileri ve arayüz yöneticisi başarıyla sahneye eklendi. Şimdi Unity'de 'Play' tuşuna basarak simülasyonu test edebilirsiniz!",
                "Tamam"
            );
        }

        private static void AddManagerComponent<T>(GameObject target) where T : MonoBehaviour
        {
            T comp = target.GetComponent<T>();
            if (comp == null)
            {
                comp = target.AddComponent<T>();
                Undo.RegisterCreatedObjectUndo(comp, $"Add {typeof(T).Name}");
                Debug.Log($"Added component: {typeof(T).Name}");
            }
        }
    }
}
