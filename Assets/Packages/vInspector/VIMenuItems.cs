#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using static VInspector.Libs.VUtils;


namespace VInspector
{
	internal class VIMenuItems
    {
        public static bool scriptInspectorEnabled { get => !ScriptHasDefineDisabled(typeof(VIScriptComponentEditor)); set => SetDefineDisabledInScript(typeof(VIScriptComponentEditor), !value); }
        public static bool soInspectorEnabled { get => !ScriptHasDefineDisabled(typeof(VIScriptableObjectEditor)); set => SetDefineDisabledInScript(typeof(VIScriptableObjectEditor), !value); }
        public static bool staticInspectorEnabled { get => !ScriptHasDefineDisabled(typeof(VIScriptAssetEditor)); set => SetDefineDisabledInScript(typeof(VIScriptAssetEditor), !value); }
        public static bool resettableVariablesEnabled { get => !ScriptHasDefineDisabled(typeof(VIResettablePropDrawer)); set => SetDefineDisabledInScript(typeof(VIResettablePropDrawer), !value); }
        public static bool cleanerHeaderEnabled { get => EditorPrefs.GetBool("vInspector-hideScriptField", true); set { EditorPrefs.SetBool("vInspector-hideScriptField", value); } }

        public static bool pluginDisabled { get => !scriptInspectorEnabled && !soInspectorEnabled && !staticInspectorEnabled && !resettableVariablesEnabled && !cleanerHeaderEnabled; set => scriptInspectorEnabled = soInspectorEnabled = staticInspectorEnabled = resettableVariablesEnabled = cleanerHeaderEnabled = !value; }


        private const string menuDir = "Tools/vInspector/";

        private const string cleanerHeader = menuDir + "Cleaner header";
        private const string resettableVariables = menuDir + "Resettable variables";
        private const string staticInspector = menuDir + "Static inspector";

        private const string disable = menuDir + "Disable vInspector";



        [MenuItem(resettableVariables, false, 1)]
        private static void dadsaadsdadsas() => resettableVariablesEnabled = !resettableVariablesEnabled;
        [MenuItem(resettableVariables, true, 1)]
        private static bool dadsadadsdasadsas() { Menu.SetChecked(resettableVariables, resettableVariablesEnabled); return !pluginDisabled; }

        [MenuItem(cleanerHeader, false, 2)]
        private static void dadsadadsas() { cleanerHeaderEnabled = !cleanerHeaderEnabled; UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation(); }
        [MenuItem(cleanerHeader, true, 2)]
        private static bool dadsaddasadsas() { Menu.SetChecked(cleanerHeader, cleanerHeaderEnabled); return !pluginDisabled; }

        [MenuItem(staticInspector, false, 3)]
        private static void dadsaadsdadsdasas() => staticInspectorEnabled = !staticInspectorEnabled;
        [MenuItem(staticInspector, true, 3)]
        private static bool dadsadadsddsaasadsas() { Menu.SetChecked(staticInspector, staticInspectorEnabled); return !pluginDisabled; }


        [MenuItem(menuDir + "Join our Discord", false, 101)]
        private static void dadsas() => Application.OpenURL("https://discord.gg/4dG9KsbspG");

        [MenuItem(menuDir + "Get the rest of our Editor Ehnancers with a discount", false, 102)]
        private static void dadsadsas() => Application.OpenURL("https://assetstore.unity.com/packages/tools/utilities/editor-enhancers-bundle-251318?aid=1100lGLBn&pubref=menu");


        [MenuItem(disable, false, 1001)]
        private static void dadsaaadsdsdadsdasas() => pluginDisabled = !pluginDisabled;
        [MenuItem(disable, true, 1001)]
        private static bool dadsadaadsdsddsaasadsas() { Menu.SetChecked(disable, pluginDisabled); return true; }



    }
}
#endif