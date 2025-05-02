using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flash2;
using UnityEngine;
using UnhollowerRuntimeLib;
using Object = UnityEngine.Object;
using CustomCharacterLoader.CharacterManager;
using UnhollowerBaseLib;
using Il2CppSystem.Reflection;
using BananaModManager.Shared;
using System.Net.NetworkInformation;
using UnityEngine.UI;
using Framework.UI;
using CustomCharacterLoader.Patches;

namespace CustomCharacterLoader.PlayerManager
{
    public class PlayerLoader : MonoBehaviour
    {
        public enum CharacterType
        {
            None = 0,
            Character = 1,
            Costume = 2,
        }

        public CharacterType playerType = CharacterType.None;
        public int playerIndex;

        // Loading character stuff
        public CustomCharacter selectedCharacter;
        public static bool loadBanana = false;
        private GameObject customBananaObject = null;
        private GameObject customPlayerObject;
        private static GameObject pauseChara = null;
        private static Image imageComponent = null;
        public bool isCostume = false;

        public PlayerLoader() { }
        public PlayerLoader(IntPtr ptr) : base(ptr) { }

        // Load character according to player type
        public void CheckPlayerType()
        {
            if (playerType == CharacterType.Character)
            {
                selectedCharacter = Main.characterManager.characters[playerIndex];
                Sound.Instance.LoadCueSheetASync((sound_id.cuesheet)selectedCharacter.monkey_voice_id);
                Main.Output("Selected a custom character!");
                Main.loadCharacter = true;
            }
            else
            {
                Main.loadCharacter = false;
            }
        }
        
        // Insert character into the game
        public void LoadPlayer(AssetBundle AssetBundle)
        {
            if (playerType == CharacterType.Character)
            {
                // Look for player object
                if (customPlayerObject == null)
                {
                    customPlayerObject = ReplaceModel(selectedCharacter, AssetBundle);
                }

                if (selectedCharacter.banana_bundle != null & customBananaObject == null)
                {
                    customBananaObject = ReplaceBananas(selectedCharacter, AssetBundle);
                }

                // Change Pause Icon
                if (selectedCharacter.pause == null)
                {
                    selectedCharacter.pause = selectedCharacter.asset[CharaOnSubmitPatch.costumeIndex].LoadAsset<Sprite>("pause");
                }
                if (pauseChara != null)
                {
                    if (imageComponent != null)
                    {
                        imageComponent.sprite = selectedCharacter.pause;
                    }
                    else
                    {
                        imageComponent = pauseChara.transform.GetChild(0).GetComponent<Image>();
                    }
                }
                else
                {
                    pauseChara = GameObject.Find("pos_pause_chara");
                }
            }
        }

        public static GameObject ReplaceBananas(CustomCharacter selectedCharacter, AssetBundle assetbundle)
        {
            GameObject aBanana = null;
            Il2CppArrayBase<Banana> bananas = Object.FindObjectsOfType<Banana>();
            if (bananas.Length > 1)
            {
                
                Shader shader = assetbundle.LoadAsset<Shader>("assets/eatass2.shader");
                
                foreach (var banana in bananas)
                {
                    aBanana = banana.gameObject;
                    GameObject bananaObject = null;
                    var rendererList = banana.GetComponentsInChildren<MeshRenderer>();
                    
                    foreach (MeshRenderer renderer in rendererList)
                    {
                        bananaObject = selectedCharacter.InitializeBanana(banana.kind, renderer, shader);
                        renderer.gameObject.transform.localScale = bananaObject.transform.localScale;
                        if (banana.kind == Banana.eKind.Fusa || banana.kind == Banana.eKind.Normal)
                        {
                            renderer.material = bananaObject.GetComponent<MeshRenderer>().material;
                        }
                    }
                    var filterList = banana.GetComponentsInChildren<MeshFilter>();
                    foreach (MeshFilter filter in filterList)
                    {
                        filter.mesh = bananaObject.GetComponent<MeshFilter>().mesh;
                    }
                }
            }
            return aBanana;
        }

        // Replace ingame model
        public static GameObject ReplaceModel(CustomCharacter selectedCharacter, AssetBundle assetbundle)
        {
            GameObject customPlayerObject = null;
            Il2CppArrayBase<Player> players = Resources.FindObjectsOfTypeAll<Player>(); // FUTURE, THIS DONT WORK WITH MULTIPLE PLAYERS
            if (players.Length > 1)
            {
                GameObject playerContainer = players[0].m_Monkey.gameObject;
                if (playerContainer.GetComponent<MonkeyEye3ds>() != null)
                {
                    Object.Destroy(playerContainer.GetComponent<MonkeyEye3ds>());
                }
                GameObject target_model = playerContainer.GetComponentInChildren<Animator>().gameObject;
                Shader shader = assetbundle.LoadAsset<Shader>("assets/eatass2.shader");
				Shader eyeShader = assetbundle.LoadAsset<Shader>("assets/eyeass.shader");

				// make base character model invisible
				foreach (var component in playerContainer.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    component.enabled = false;
                }
                foreach (var component in playerContainer.GetComponentsInChildren<MeshRenderer>())
                {
                    component.enabled = false;
                }

                // load custom character
                customPlayerObject = selectedCharacter.InitializeCharacter(shader, eyeShader, CharaOnSubmitPatch.costumeIndex);
                Quaternion rotation = playerContainer.transform.rotation;
                playerContainer.transform.rotation = Quaternion.Euler(0, 0, 0);
                customPlayerObject.transform.parent = target_model.transform;
                Animator customAnimator = customPlayerObject.GetComponentInChildren<Animator>();
                playerContainer.GetComponent<MonkeyRef>().m_Animator = customAnimator;
                playerContainer.GetComponent<Monkey>().m_Animator = customAnimator;
                PlayerMotion playerMotion = customPlayerObject.GetComponent<PlayerMotion>();
                if (playerMotion != null)
                {
                    players[0].m_PlayerMotion = playerMotion;
                }
                playerContainer.transform.rotation = rotation;
			}
            return customPlayerObject;
        }
	}
}