using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using System.IO;
using System;
using UnhollowerRuntimeLib;
using System.Reflection;
using CustomCharacterLoader.CharacterManager;
using CustomCharacterLoader.Patches;
using CustomCharacterLoader.PlayerManager;
using CustomCharacterLoader.Sounds;
using Flash2.Selector.MainGame;
using Flash2;
using UnhollowerBaseLib.Runtime;
using Flash2.Selector;
using Flash2.Selector.TimeAttack;
using HarmonyLib;

namespace CustomCharacterLoader
{
    public static class Main
    {
        // File Paths
        public static string PATH = "";

        // Mod Objects
        public static CustomCharacterManager characterManager = null;
        public static PlayerLoader playerLoader = null;
        public static SoundImporter soundImporter = null;

		// Asset Bundle
		public static AssetBundle assetBundle;
		public static Shader CustomShader { get; private set; }
        
        // Console Text
        public static void Output(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("CustomCharacterLoader: " + text);
            Console.ForegroundColor = ConsoleColor.White;
        }
        private static void LoadAssetBundle()
		{
			if (assetBundle == null)
			{
				Output("Loading Shader AssetBundle...");
				string assetBundlePath = Path.Combine(PATH, "sbmshader");
				assetBundle = AssetBundle.LoadFromFile(assetBundlePath);

				if (assetBundle != null)
				{
					Output("Shader AssetBundle loaded successfully.");
				}
				else
				{
					Output("Failed to load Shader AssetBundle. Make sure \"" + assetBundlePath + "\" exists.");
				}
			}
			else
			{
			}
		}

		// Mod Start
		public static void OnModStart()
        {
			// Get Paths
			PATH = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Create il2cpp objects
            var obj = new GameObject { hideFlags = HideFlags.HideAndDontSave };
            Object.DontDestroyOnLoad(obj);

            ClassInjector.RegisterTypeInIl2Cpp(typeof(CustomCharacterManager));
            Main.characterManager = new CustomCharacterManager(obj.AddComponent(Il2CppType.Of<CustomCharacterManager>()).Pointer, Path.Combine(Main.PATH, "Characters\\"));

            ClassInjector.RegisterTypeInIl2Cpp(typeof(PlayerLoader));
            Main.playerLoader = new PlayerLoader(obj.AddComponent(Il2CppType.Of<PlayerLoader>()).Pointer);

            ClassInjector.RegisterTypeInIl2Cpp(typeof(SoundImporter));
            Main.soundImporter = new SoundImporter(obj.AddComponent(Il2CppType.Of<SoundImporter>()).Pointer);

            // Create detours
            CharaOnSubmitPatch.CreateMainGameDetour();
            CharaOnSubmitPatch.CreateTimeAttackDetour();
            CharaNamePatch.CreateMainGameDetour();
            CharaNamePatch.CreateTimeAttackDetour();
            GetCueSheetPatch.CreateGetCueSheetDetour();
        }

        // Mod Update (Split by scene names)
        public static string sceneName;
        public static bool charaSelectFix = false;
        public static bool loadCharacter = false;
        public static bool voicebanks = false;

        public static void OnModUpdate()
        {
            LoadAssetBundle();
            // If the main game is active, load the custom character.
            if (MainGame.Exists && loadCharacter)
            {
                playerLoader.LoadPlayer(assetBundle);
            }
            
            sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "MainMenu")
            {
                if (UnityEngine.Object.FindObjectOfType<SelMainMenuSequence>() == null) return;
                if (!charaSelectFix)
                {
                    // Just do this until we get an actual return, hide the errors from the logs.
                    try
                    {
                        SelMainMenuSequence mainMenu = UnityEngine.Object.FindObjectOfType<SelMainMenuSequence>();
                        SelMgCharaSelectWindow charaSelectWindow = mainMenu.GetMainMenuWindow(SelectorDef.MainMenuKind.MgCharaSelect).Cast<SelMgCharaSelectWindow>();
                        charaSelectWindow.m_ItemDataList.Capacity = 100;
                        SelTaCharaSelectWindow selTaCharaSelect = mainMenu.GetMainMenuWindow(SelectorDef.MainMenuKind.MgTimeAttackCharaSelect).Cast<SelTaCharaSelectWindow>();
                        selTaCharaSelect.m_ItemDataList.Capacity = 100;
                        charaSelectFix = true;
                    }
                    catch
                    {

                    }
                    
                }
                if (!charaSelectFix) return;
                try {soundImporter.Load();}
                catch (Exception ex) { Console.WriteLine(ex.Message); } // The mod likes to throw errors when this method loads too fast.
                if(soundImporter.importedSounds)
                {
                    try {characterManager.Load();}
                    catch (Exception ex) { Console.WriteLine(ex.Message); } // The mod likes to throw errors when this method loads too fast.
                }
            }
        }

    }
}

/* Might be useful for future things? Checks if other mods are active
Assembly assembly = Assembly.GetCallingAssembly();
Type loader = assembly.GetType("BananaModManager.Loader.IL2Cpp.Loader");
PropertyInfo infoList = loader.GetProperty("Mods");
List<BananaModManager.Shared.Mod> mods = (List<BananaModManager.Shared.Mod>)infoList.GetValue(loader);
foreach (var mod in mods)
{
    string modName = mod.GetAssembly().ToString().Split(',')[0];
    if (modName == "Dynamic Sounds (Main Cast)")
    {
        Main.DYNAMIC_SOUNDS_PATH = mod.Directory.ToString();
        Main.Output("Found Dynamic Sounds Path");
    }
    else if(modName == "GuestCharacters")
    {
        Main.GUEST_CHARACTER_PATH = mod.Directory.ToString();
        Main.Output("Found Guest Characters Path");
    }
}
*/