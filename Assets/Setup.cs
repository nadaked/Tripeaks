using UnityEngine;
using UnityEditor;
using static System.IO.Directory;
using static System.IO.Path;
using static UnityEditor.AssetDatabase;

public static class Setup
{
    [MenuItem("Tools/Setup/Create Project Folders")]
    public static void CreateProjectFolders()
    {
        // Root
        Folders.Create("_Project");

        // Main folders
        Folders.Create("_Project", "Scripts");
        Folders.Create("_Project", "Data");
        Folders.Create("_Project", "Resources");
        Folders.Create("_Project", "Materials");
        Folders.Create("_Project", "Models");
        Folders.Create("_Project", "Prefabs");
        Folders.Create("_Project", "Textures");
        Folders.Create("_Project", "ScriptableObjects");
        Folders.Create("_Project", "Animations");
        Folders.Create("_Project", "Scenes");

        // Scripts
        Folders.Create("_Project/Scripts", "Core");
        Folders.Create("_Project/Scripts", "Application");
        Folders.Create("_Project/Scripts", "Infrastructure");
        Folders.Create("_Project/Scripts", "Presentation");
        Folders.Create("_Project/Scripts", "Editor");

        // Core
        Folders.Create("_Project/Scripts/Core", "Game");
        Folders.Create("_Project/Scripts/Core", "Board");
        Folders.Create("_Project/Scripts/Core", "Cards");
        Folders.Create("_Project/Scripts/Core", "Deck");
        Folders.Create("_Project/Scripts/Core", "Actions");
        Folders.Create("_Project/Scripts/Core", "Triggers");
        Folders.Create("_Project/Scripts/Core", "Systems");
        Folders.Create("_Project/Scripts/Core", "Undo");

        // Core / Board
        Folders.Create("_Project/Scripts/Core/Board", "Slot");

        // Core / Actions
        Folders.Create("_Project/Scripts/Core/Actions", "Implementations");

        // Application
        Folders.Create("_Project/Scripts/Application", "Presenters");
        Folders.Create("_Project/Scripts/Application", "Services");
        Folders.Create("_Project/Scripts/Application", "Factories");

        // Presentation
        Folders.Create("_Project/Scripts/Presentation", "Views");
        Folders.Create("_Project/Scripts/Presentation", "Animations");
        Folders.Create("_Project/Scripts/Presentation", "FX");

        // Presentation / Views
        Folders.Create("_Project/Scripts/Presentation/Views", "Board");
        Folders.Create("_Project/Scripts/Presentation/Views", "Card");
        Folders.Create("_Project/Scripts/Presentation/Views", "Deck");

        // Editor / LevelTool
        Folders.Create("_Project/Scripts/Editor", "LevelTool");

        // Data
        Folders.Create("_Project/Data", "Level");
        Folders.Create("_Project/Data", "Cards");

        Refresh();
    }

    static class Folders
    {
        public static void Create(string root, params string[] folders)
        {
            var fullPath = Combine(Application.dataPath, root);

            if (!Exists(fullPath))
            {
                CreateDirectory(fullPath);
            }

            foreach (var folder in folders)
            {
                var path = Combine(fullPath, folder);

                if (!Exists(path))
                {
                    CreateDirectory(path);
                }
            }
        }
    }
}