using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flash2;
using UnityEngine;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Runtime;
using UnhollowerRuntimeLib;
using Flash2.Selector.MainGame;

namespace CustomCharacterLoader.CharacterManager
{
    public class CustomCharacterManager : MonoBehaviour
    {
        // Lists of character stuff
        public List<CustomCharacter> characters = new List<CustomCharacter>();

        
        public CustomCharacterManager() { }
        public CustomCharacterManager(IntPtr ptr) : base(ptr) { }
        public CustomCharacterManager(IntPtr ptr, string path) : base(ptr)
        {
            // Read the Characters folder
            foreach (string dir in Directory.GetDirectories(path))
            {
                Console.WriteLine(dir);
                // Reads all the Json files in folders
                foreach (string json in Directory.GetFiles(dir, "*.json"))
                {
                    StreamReader reader = new StreamReader(json);
                    string data = reader.ReadToEnd();
                    string name = dir.Substring(dir.LastIndexOf("\\") + 1);
                    CustomCharacter character = new CustomCharacter(name, data, dir);
                    string charaIdStr = "";
                    foreach (char c in name)
                    {
                        int index = (int)c % 32;
                        charaIdStr += index;
                    }
                    character.charaId = Convert.ToInt32(charaIdStr.Substring(0, 6));
                    reader.Close();

                    if (character.asset != null)
                    {
                        characters.Add(character);
                    }
                }
            }
        }

        // Create character select icons for all custom characters
        public bool importedCharacters = false;
        public void Load()
        {
            if (!this.importedCharacters)
            {
                SelMgCharaItemDataListObject[] gameCharacterList = Resources.FindObjectsOfTypeAll<SelMgCharaItemDataListObject>(); // character select page
                
                if (gameCharacterList != null && gameCharacterList.Length > 0)
                {
                    foreach (CustomCharacter chara in characters)
                    {
                        chara.CreateItemData(gameCharacterList[0]);
                    }
                    Main.Output("Created Custom Character Slots.");
                    this.importedCharacters = true;
                }
            }
            Update(); // Make sure pictures stay
        }

        // Update icons
        public void Update()
        {
            foreach (CustomCharacter chara in characters)
            {
                chara.UpdateCharacterSelect();
            }
        }
    }
}
