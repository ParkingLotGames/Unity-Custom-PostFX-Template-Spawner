using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class CustomPostFXTemplateSpawner : EditorWindow
{
    enum EffectTypes
    {
        Default = 0,
        Cg = 1,
        Multipass = 2,
    }
    string effectsFolder = "Assets/Postprocessing";
    string effectName = "New PPV3 Effect";
    EffectTypes effectType = new EffectTypes();

    readonly string settingsTemplatePath = "Packages/com.parkinglotgames.custom-postfx-template-spawner/Custom PostFX Template Spawner/Templates/DefaultPostProcessingSettings.txt";
    readonly string cgSettingsTemplatePath = "Packages/com.parkinglotgames.custom-postfx-template-spawner/Custom PostFX Template Spawner/Templates/DefaultCGPostProcessingSettings.txt";
    readonly string multiPassSettingsTemplatePath = "Packages/com.parkinglotgames.custom-postfx-template-spawner/Custom PostFX Template Spawner/Templates/DefaultMultipassPostProcessingSettings.txt";
    readonly string rendererTemplatePath = "Packages/com.parkinglotgames.custom-postfx-template-spawner/Custom PostFX Template Spawner/Templates/DefaultPostProcessingRenderer.txt";
    readonly string cgRendererTemplatePath = "Packages/com.parkinglotgames.custom-postfx-template-spawner/Custom PostFX Template Spawner/Templates/DefaultCGPostProcessingRenderer.txt";
    readonly string multiPassRendererTemplatePath = "Packages/com.parkinglotgames.custom-postfx-template-spawner/Custom PostFX Template Spawner/Templates/DefaultMultipassPostProcessingRenderer.txt";
    readonly string shaderTemplatePath = "Packages/com.parkinglotgames.custom-postfx-template-spawner/Custom PostFX Template Spawner/Templates/DefaultShader.txt";
    readonly string cgShaderTemplatePath = "Packages/com.parkinglotgames.custom-postfx-template-spawner/Custom PostFX Template Spawner/Templates/DefaultCGShader.txt";
    readonly string multiPassShaderTemplatePath = "Packages/com.parkinglotgames.custom-postfx-template-spawner/Custom PostFX Template Spawner/Templates/DefaultMultipassShader.txt";
    readonly string hlslIncludeTemplatePath = "Packages/com.parkinglotgames.custom-postfx-template-spawner/Custom PostFX Template Spawner/Templates/DefaultHLSLInclude.txt";
    readonly string cgIncludeTemplatePath = "Packages/com.parkinglotgames.custom-postfx-template-spawner/Custom PostFX Template Spawner/Templates/DefaultCGInclude.txt";
    readonly string multiPassIncludeTemplatePath = "Packages/com.parkinglotgames.custom-postfx-template-spawner/Custom PostFX Template Spawner/Templates/DefaultMultipassInclude.txt";
    readonly string cgCommonTemplatePath = "Packages/com.parkinglotgames.custom-postfx-template-spawner/Custom PostFX Template Spawner/Templates/DefaultCGCommon.txt";
    readonly string hlslCommonTemplatePath = "Packages/com.parkinglotgames.custom-postfx-template-spawner/Custom PostFX Template Spawner/Templates/DefaultHLSLCommon.txt";

    [MenuItem("Assets/Create/Post Processing/Custom Effect", priority = 60)]
    [MenuItem("Tools/Post Processing/Create New Custom Effect")]
    static void Init()
    {
        CustomPostFXTemplateSpawner window = (CustomPostFXTemplateSpawner)GetWindow(typeof(CustomPostFXTemplateSpawner), true, "Create New Custom Post Processing Stack V3 Effect");
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
        GUILayout.Label("Effect type", GUILayout.Width(EditorGUIUtility.labelWidth));
        effectType = (EffectTypes)EditorGUILayout.EnumPopup(effectType);
        GUILayout.EndHorizontal();

        switch (effectType)
        {
            case EffectTypes.Default:        
                effectName = "New Effect";
                break;
            case EffectTypes.Cg:
                effectName = "New Cg Effect";
                break;
            case EffectTypes.Multipass:
                effectName = "New Multipass Effect";
                break;
        }
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
        }
        GUILayout.EndHorizontal();
    }

    async void CreateEffect()
    {
        string settingsFileName = "";
        string rendererFileName = "";
        string shaderFileName = "";
        string includeFileName = "";
        string commonFileName = "";

        string settingsTemplateText = "";
        string rendererTemplateText = "";
        string shaderTemplateText = "";
        string includeTemplateText = "";
        string commonTemplateText = "";
        // Create the folders if they don't exist
        string settingsFolder = $"{effectsFolder}/Settings";
        string renderersFolder = $"{effectsFolder}/Renderers";
        string shadersFolder = $"{effectsFolder}/Shaders";
        string includeFolder = $"{effectsFolder}/Shaders/include";
        if (!Directory.Exists(settingsFolder))
            Directory.CreateDirectory(settingsFolder);
        if (!Directory.Exists(renderersFolder))
            Directory.CreateDirectory(renderersFolder);
        if (!Directory.Exists(shadersFolder))
            Directory.CreateDirectory(shadersFolder);
        if (!Directory.Exists(includeFolder))
            Directory.CreateDirectory(includeFolder);

        // Get name without spaces
        string effectNameWithoutSpaces = effectName.Replace(" ", "");
        string effectNameCamelCase = ToCamelCase(effectNameWithoutSpaces);

        // Generate unique names for the effect files
        settingsFileName = GenerateUniqueFileName(settingsFolder, $"{effectNameWithoutSpaces}.cs");
        rendererFileName = GenerateUniqueFileName(renderersFolder, $"{(effectType == EffectTypes.Cg ? "Cg" : "") + effectNameWithoutSpaces}Renderer.cs");
        shaderFileName = GenerateUniqueFileName(shadersFolder, $"{(effectType == EffectTypes.Cg ? "Cg" : "") + effectNameWithoutSpaces}.shader");
        includeFileName = GenerateUniqueFileName(includeFolder, $"{effectNameWithoutSpaces}.{(effectType == EffectTypes.Default ? "hlsl" : "cginc")}");
        commonFileName = $"Common.{(effectType == EffectTypes.Default ? "hlsl" : "cginc")}";

        if (!File.Exists($"{includeFolder}/{commonFileName}"))
        {
            commonTemplateText = File.ReadAllText(effectType == EffectTypes.Default ? hlslCommonTemplatePath : cgCommonTemplatePath);
            await Task.WhenAll(WriteFileAsync(includeFolder, commonFileName, commonTemplateText));
        }
        // Read the template files
        switch (effectType)
        {
            case EffectTypes.Default:
                rendererTemplateText = File.ReadAllText(rendererTemplatePath);
                settingsTemplateText = File.ReadAllText(settingsTemplatePath);
                shaderTemplateText = File.ReadAllText(shaderTemplatePath);
                includeTemplateText = File.ReadAllText(hlslIncludeTemplatePath);
                break;
            case EffectTypes.Cg:
                rendererTemplateText = File.ReadAllText(cgRendererTemplatePath);
                settingsTemplateText = File.ReadAllText(cgSettingsTemplatePath);
                shaderTemplateText = File.ReadAllText(cgShaderTemplatePath);
                includeTemplateText = File.ReadAllText(cgIncludeTemplatePath);
                break;
            case EffectTypes.Multipass:
                rendererTemplateText = File.ReadAllText(multiPassRendererTemplatePath);
                settingsTemplateText = File.ReadAllText(multiPassSettingsTemplatePath);
                shaderTemplateText = File.ReadAllText(multiPassShaderTemplatePath);
                includeTemplateText = File.ReadAllText(multiPassIncludeTemplatePath);
                break;
        }

        // Replace the placeholders in the template texts with the effect name
        settingsTemplateText = settingsTemplateText.Replace("#SCRIPTNAME#", effectNameWithoutSpaces);
        settingsTemplateText = settingsTemplateText.Replace("#SPACESCRIPTNAME#", effectName);
        rendererTemplateText = rendererTemplateText.Replace("#SCRIPTNAME#", effectNameWithoutSpaces);
        rendererTemplateText = rendererTemplateText.Replace("#CAMELSCRIPTNAME#", effectNameCamelCase);
        rendererTemplateText = rendererTemplateText.Replace("#SPACESCRIPTNAME#", effectName);
        shaderTemplateText = shaderTemplateText.Replace("#NAME#", effectNameWithoutSpaces);
        shaderTemplateText = shaderTemplateText.Replace("#SPACENAME#", effectName);
        includeTemplateText = includeTemplateText.Replace("#NAME#", effectNameWithoutSpaces);
        includeTemplateText = includeTemplateText.Replace("#SPACENAME#", effectName);

        // Write the files
        await Task.WhenAll(
        WriteFileAsync(settingsFolder, settingsFileName, settingsTemplateText),
        WriteFileAsync(renderersFolder, rendererFileName, rendererTemplateText),
        WriteFileAsync(shadersFolder, shaderFileName, shaderTemplateText),
        WriteFileAsync(includeFolder, includeFileName, includeTemplateText)
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

    string ToCamelCase(string input)
    {
        if (input.Length > 0)
        {
            input = char.ToLower(input[0]) + input.Substring(1);
        }

        return input;
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