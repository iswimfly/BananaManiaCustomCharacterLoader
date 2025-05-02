using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Reflection;
using Flash2;
using UnityEngine;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using Object = UnityEngine.Object;
using System.Linq;

namespace CustomCharacterLoader.CharacterManager
{
    public class CustomCharacter : UnityEngine.Object
    {
        public int charaId;

        // Sound Stuff
        public string monkey_acb = "";
        public string banana_acb = "";
        public string announcer_acb = "";
        public int aCue = 0;
        public int monkey_voice_id = 0;

        // Chara Select Stuff
        public SelMgCharaItemData itemData;
        public string base_monkey = "jet";
        public string charaName = "";

        // Unity Asset Bundle Stuff
        public string assetName = "";
        public List<AssetBundle> asset = new List<AssetBundle>();
        public AssetBundle banana_bundle;
        public bool[] bananaTypes = {false, false};
        public Sprite icon;
        public Sprite banner;
        public Sprite pause;
        public List<int> iconInstances = new List<int>();
        public List<string> costumeNames = new List<string>();

        private class CharacterJsonTemplate
        {
            public string asset_bundle { get; set; }
            public string banana_bundle { get; set; } = "";
            public string base_monkey { get; set; } = "jet";
            public string monkey_acb { get; set; } = "";
            public string banana_acb { get; set; } = "";
            public string announcer_acb { get; set; } = "";
            public List<string> costume_names {  get; set; } = new List<string>();
        }

        // Reads a json file then get the asset bundle and base_character
        public CustomCharacter(string characterName, string json, string dir)
        {
            CharacterJsonTemplate template = JsonSerializer.Deserialize<CharacterJsonTemplate>(json);

            template.base_monkey = template.base_monkey.ToLower();
            String[] charakind = Array.ConvertAll(Enum.GetNames(typeof(Chara.eKind)), chara => chara.ToString().ToLower());
            if (Array.Exists(charakind, chara => chara == template.base_monkey))
            {
                this.base_monkey = template.base_monkey;
            }

            this.charaName = characterName;
            this.costumeNames = template.costume_names;
            if (this.costumeNames.Count == 0)
            {
                this.costumeNames.Add(this.charaName);
            }
            this.assetName = template.asset_bundle;

            // Open asset bundle
            try
            {
                Main.Output("Loading Character: " + this.charaName);
                AssetBundle checkForNull = AssetBundle.LoadFromFile(Path.Combine(dir, this.assetName));
                if (checkForNull == null)
                {
                    Main.Output("Cant find Asset bundle:" + dir + this.assetName);
                }
                else
                {
                    int i = 0;
                    while (checkForNull != null)
                    {
                        this.asset.Add(checkForNull);
                        i++;
                        checkForNull = AssetBundle.LoadFromFile(Path.Combine(dir, this.assetName + $"_{i}"));
                    }
                    Main.Output("Loaded Asset Bundle: " + this.assetName);
                }
                
            }
            catch (Exception ex)
            {
                Main.Output("Unable to load file: " + Path.Combine(dir, this.assetName));
            }

            // Bestow Bananas
            if (template.banana_bundle != "")
            {
                Main.Output("Loading " + this.charaName + " Custom bananas: " + this.banana_bundle);
                Console.WriteLine(Path.Combine(dir, template.banana_bundle));
                this.banana_bundle = AssetBundle.LoadFromFile(Path.Combine(dir, template.banana_bundle));
                if (this.banana_bundle == null)
                {
                    Main.Output("Cant find Banana bundle:" + dir + this.assetName);
                }

            }

            // Open sound files
            CriAtomExAcb sounds = CriAtomExAcb.LoadAcbFile(null, Path.Combine(dir, template.monkey_acb), null);
            if(sounds != null)
            {
                this.monkey_acb =  Path.Combine(dir, template.monkey_acb);
                sounds.Dispose();
            }
            sounds = CriAtomExAcb.LoadAcbFile(null, Path.Combine(dir, template.announcer_acb), null);
            if (sounds != null)
            {
                this.announcer_acb = Path.Combine(dir, template.announcer_acb);
                sounds.Dispose();
            }
        }

        // Create item data for character select screen
        public void CreateItemData(SelMgCharaItemDataListObject itemDataList)
        {
            // Get CharaKind
            SelMgCharaItemData clone = itemDataList.m_ItemDataList[0];
            foreach (SelMgCharaItemData character in itemDataList.m_ItemDataList.list)
            {
                if (character.characterKind.ToString().ToLower() == this.base_monkey)
                {
                    clone = character;
                    break;
                }
            }

            // Item Data class
            this.itemData = new SelMgCharaItemData();
            this.itemData.costumeIndex = 0;
            this.itemData.m_CharacterKind = clone.m_CharacterKind;
            this.itemData.m_CostumeList = new SelMgCharaItemData.CostumeList();
            
            // Character Select Sprites
            foreach(AssetBundle ab in this.asset)
            {
                this.icon = ab.LoadAsset<Sprite>("icon");
                this.banner =ab.LoadAsset<Sprite>("banner");
                this.pause = ab.LoadAsset<Sprite>("pause");

                // Costume
                SelMgCharaItemData.CostumeData costume = new SelMgCharaItemData.CostumeData();
                costume.m_DisplayItemName = clone.costumeList[0].m_DisplayItemName;
                costume.m_PartsSetIndex = clone.costumeList[0].m_PartsSetIndex;
                costume.m_PointShopID = "Invalid";
                costume.spriteIcon = this.icon;
                costume._spriteIcon_k__BackingField = this.icon;
                costume.spritePicture = this.pause;
                costume._spritePicture_k__BackingField = this.pause;

                iconInstances.Add(costume.spriteIcon.GetInstanceID());

                this.itemData.m_CostumeList.Add(costume);

            }

            // Item Data Cont..
            this.itemData.m_DescriptionText = clone.m_DescriptionText;
            this.itemData.m_IsHideText = clone.m_IsHideText;

            if(this.announcer_acb != "")
            {
                IntPtr cueIdPtr = IL2CPP.GetIl2CppNestedType(IL2CPP.GetIl2CppClass("Assembly-CSharp.dll", "Flash2", "sound_id"), "cue");
                Il2CppSystem.Type cueIdType = UnhollowerRuntimeLib.Il2CppType.TypeFromPointer(cueIdPtr);

                Il2CppSystem.Reflection.Assembly assembly = Il2CppSystem.AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "Assembly-CSharp");
                Il2CppSystem.Type enumRuntimeHelper = assembly.GetType("Framework.EnumRuntimeHelper`1");
                Il2CppSystem.Type erhCue = enumRuntimeHelper.MakeGenericType(new Il2CppReferenceArray<Il2CppSystem.Type>(new Il2CppSystem.Type[] { cueIdType }));

                Il2CppSystem.Reflection.MethodInfo cueValToNameGetter = erhCue.GetProperty("valueToNameCollection").GetGetMethod();
                Il2CppSystem.Collections.Generic.Dictionary<sound_id.cue, string> cueValToName = cueValToNameGetter.Invoke(null, new Il2CppReferenceArray<Il2CppSystem.Object>(0)).Cast<Il2CppSystem.Collections.Generic.Dictionary<sound_id.cue, string>>();
                cueValToName.Add((sound_id.cue)this.aCue, this.aCue.ToString());

                Il2CppSystem.Reflection.MethodInfo cueNameToValGetter = erhCue.GetProperty("nameToValueCollection").GetGetMethod();
                Il2CppSystem.Collections.Generic.Dictionary<string, sound_id.cue> cueNameToVal = cueNameToValGetter.Invoke(null, new Il2CppReferenceArray<Il2CppSystem.Object>(0)).Cast<Il2CppSystem.Collections.Generic.Dictionary<string, sound_id.cue>>();
                cueNameToVal.Add(this.aCue.ToString(), (sound_id.cue)this.aCue);    
            }

            this.itemData.m_NarrationCueID = this.aCue.ToString();
            this.itemData.m_PointShopID = "Invalid";
            this.itemData.m_SupplementaryText = clone.m_SupplementaryText;
            this.itemData.m_Text = clone.m_Text;

            itemDataList.m_ItemDataList.Add(this.itemData);
        }

        public void UpdateCharacterSelect()
        {
            int i = 0;
            foreach (AssetBundle ab in this.asset)
            {
                this.icon = ab.LoadAsset<Sprite>("icon");
                this.banner = ab.LoadAsset<Sprite>("banner");
                this.itemData.costumeList[i].spritePicture = this.banner;
                this.itemData.costumeList[i].spriteIcon = this.icon;
                i++;
            }
        }

        public GameObject InitializeCharacter(Shader shader, Shader eyeShader, int CostumeIndex)
        {
            GameObject modModel = Object.Instantiate(this.asset[CostumeIndex].LoadAsset<GameObject>("character"));

            // set shaders
            Il2CppArrayBase<SkinnedMeshRenderer> MeshRenderers = modModel.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer meshRenderer in MeshRenderers)
            {
                foreach (Material material in meshRenderer.materials)
                {
                    if (!material.name.Contains("balls") && !material.name.Contains("Eye") && !material.name.Contains("Custom") && !material.name.Contains("Alpha")) // if someone asks for yet another exception throw them out a window
                    {
                        material.shader = shader;
                    }
                    else if (material.name.Contains("Eye") && material.name.Contains("Alpha"))
                    {
                        material.shader = eyeShader;
                    }
                }
            }
            
            modModel.SetActive(true);
            return modModel;
        }
        
        public GameObject InitializeBanana(Banana.eKind nanner, MeshRenderer renderer, Shader shader)
        {
            GameObject modModel = null;
            
            if (nanner == Banana.eKind.Fusa)
            {
                var ripeBanana = this.banana_bundle.LoadAsset<GameObject>("bunch");
                if (ripeBanana != null)
                {
                    modModel = Object.Instantiate(ripeBanana);
                }
            }
            else
            {
                var ripeBanana = this.banana_bundle.LoadAsset<GameObject>("banana");
                if (ripeBanana != null)
                {
                    modModel = Object.Instantiate(ripeBanana);
                }
            }

            MeshRenderer meshRenderer = null;
            try
            {
                meshRenderer = modModel.GetComponent<MeshRenderer>();
            }
            catch (Exception ex)
            { 
                Console.WriteLine(ex.Message);
            }
            if (meshRenderer != null)
            {
                if (!meshRenderer.material.name.Contains("balls") && !meshRenderer.material.name.Contains("Custom") && !meshRenderer.material.name.Contains("Alpha")) // if someone asks for yet another exception throw them out a window
                {
                    meshRenderer.material.shader = shader;
                    meshRenderer.lightmapIndex = 0;
                }
                meshRenderer.enabled = false;

            }
            else
            {
                
            }

            modModel.SetActive(true);
            return modModel;
        }
    }
}