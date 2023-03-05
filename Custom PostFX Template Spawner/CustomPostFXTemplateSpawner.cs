using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class CustomPostFXTemplateSpawner : EditorWindow
{
    private string effectsFolder = "";
    private string effectName = "New Effect";

    [MenuItem("Tools/PostFX Template Spawner/Create New Effect")]
    static void Init()
    {
        CustomPostFXTemplateSpawner window = (CustomPostFXTemplateSpawner)GetWindow(typeof(CustomPostFXTemplateSpawner), true, "New Custom Post Processing Effect");
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Custom Effects Folder", GUILayout.Width(EditorGUIUtility.labelWidth - 4));
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField(effectsFolder);
        EditorGUI.EndDisabledGroup();
        if (GUILayout.Button("...", GUILayout.Width(22)))
        {
            effectsFolder = EditorUtility.OpenFolderPanel("Select Custom Effects Folder", "Assets", "");
            if (!string.IsNullOrEmpty(effectsFolder))
            {
                if (effectsFolder.StartsWith(Application.dataPath))
                {
                    effectsFolder = "Assets" + effectsFolder.Substring(Application.dataPath.Length);
                }
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Effect name", GUILayout.Width(EditorGUIUtility.labelWidth));
        effectName = EditorGUILayout.TextField(effectName);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Create", GUILayout.Width(80)))
        {
            CreateEffect();
            Close();
        }
        GUILayout.EndHorizontal();
    }

    async void CreateEffect()
    {

        string settingsTemplatePath = "Packages/com.parkinglotgames.custom-postfx-template-spawner/Custom PostFX Template Spawner/Templates/DefaultPostProcessingSettings.txt";
        string rendererTemplatePath = "Packages/com.parkinglotgames.custom-postfx-template-spawner/Custom PostFX Template Spawner/Templates/DefaultPostProcessingRenderer.txt";
        string shaderTemplatePath = "Packages/com.parkinglotgames.custom-postfx-template-spawner/Custom PostFX Template Spawner/Templates/DefaultShader.txt";
        string hlslTemplatePath = "Packages/com.parkinglotgames.custom-postfx-template-spawner/Custom PostFX Template Spawner/Templates/DefaultHLSLInclude.txt";

        // Create the folders if they don't exist
        string settingsFolder = $"{effectsFolder}/Settings";
        string renderersFolder = $"{effectsFolder}/Renderers";
        string shadersFolder = $"{effectsFolder}/Shaders";
        if (!Directory.Exists(settingsFolder))
            Directory.CreateDirectory(settingsFolder);
        if (!Directory.Exists(renderersFolder))
            Directory.CreateDirectory(renderersFolder);
        if (!Directory.Exists(shadersFolder))
            Directory.CreateDirectory(shadersFolder);

        // Get name without spaces
        string effectNameWithoutSpaces = effectName.Replace(" ", "");

        // Generate unique names for the effect files
        string settingsFileName = GenerateUniqueFileName(settingsFolder, $"{effectNameWithoutSpaces}.cs");
        string rendererFileName = GenerateUniqueFileName(renderersFolder, $"{effectNameWithoutSpaces}Renderer.cs");
        string shaderFileName = GenerateUniqueFileName(shadersFolder, $"{effectNameWithoutSpaces}.shader");
        string hlslFileName = GenerateUniqueFileName(shadersFolder, $"{effectNameWithoutSpaces}.hlsl");

        // Read the template files
        string settingsTemplateText = File.ReadAllText(settingsTemplatePath);
        string rendererTemplateText = File.ReadAllText(rendererTemplatePath);
        string shaderTemplateText = File.ReadAllText(shaderTemplatePath);
        string hlslTemplateText = File.ReadAllText(hlslTemplatePath);

        // Replace the placeholders in the template texts with the effect name
        settingsTemplateText = settingsTemplateText.Replace("#SCRIPTNAME#", effectNameWithoutSpaces);
        settingsTemplateText = settingsTemplateText.Replace("#SPACESCRIPTNAME#", effectName);
        rendererTemplateText = rendererTemplateText.Replace("#SCRIPTNAME#", effectNameWithoutSpaces);
        rendererTemplateText = rendererTemplateText.Replace("#SPACESCRIPTNAME#", effectName);
        shaderTemplateText = shaderTemplateText.Replace("#NAME#", effectNameWithoutSpaces   );
        shaderTemplateText = shaderTemplateText.Replace("#SPACENAME#", effectName);
        hlslTemplateText = hlslTemplateText.Replace("#NAME#", effectNameWithoutSpaces);
        hlslTemplateText = hlslTemplateText.Replace("#SPACENAME#", effectName);

        // Write the files
        await Task.WhenAll(
        WriteFileAsync(settingsFolder, settingsFileName, settingsTemplateText),
        WriteFileAsync(renderersFolder, rendererFileName, rendererTemplateText),
        WriteFileAsync(shadersFolder, shaderFileName, shaderTemplateText),
        WriteFileAsync(shadersFolder, hlslFileName, hlslTemplateText)
        );

        // Refresh the asset database
        AssetDatabase.Refresh();
    }

    string GenerateUniqueFileName(string folder, string fileName)
    {
        string path = $"{folder}/{fileName}";
        if (File.Exists(path))
        {
            int index = 1;
            string baseName = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);
            while (File.Exists($"{folder}/{baseName}{index}{extension}"))
            {
                index++;
            }
            return $"{baseName}{index}{extension}";
        }
        else
        {
            return fileName;
        }
    }

    async Task WriteFileAsync(string folder, string fileName, string content)
    {
        string path = $"{folder}/{fileName}";
        using (StreamWriter writer = new StreamWriter(path))
        {
            await writer.WriteAsync(content);
        }
    }
}