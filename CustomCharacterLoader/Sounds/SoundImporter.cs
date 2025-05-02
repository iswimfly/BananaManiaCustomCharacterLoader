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

namespace CustomCharacterLoader.Sounds
{
    public class SoundImporter : MonoBehaviour
    {
        private int sound_id = 6000;
        private int cue_id = 10001;
        private static int default_cue_id = 10000;
        public Sound sound = null;
        public bool importedSounds = false;
        public SoundImporter() { }
        public SoundImporter(IntPtr ptr) : base(ptr) { }
        public List<sound_id.cuesheet> voicebanks = new List<sound_id.cuesheet>();

        public void Load()
        {
            if(!this.importedSounds)
            {
                if (this.sound == null)
                {
                    Main.Output("Finding sound instance");
                    this.sound = Sound.Instance;
                }
                else
                {
                    // Default announcer
                    Main.Output("Loading a_custom_default acb: " + Path.Combine(Main.PATH, "a_custom_default"));
                    
                    Sound.Instance.m_cueSheetParamDict[(sound_id.cuesheet)sound_id] = new Sound.cuesheet_param_t(sound_id.ToString(), Path.Combine(Main.PATH, "a_custom_default.acb"), null); // default announcer acb
                    
                    Sound.Instance.m_cueParamDict[(sound_id.cue)default_cue_id] = new Sound.cue_param_t((sound_id.cuesheet)sound_id, "Cue_02");
                    
                    Sound.Instance.LoadCueSheetASync((sound_id.cuesheet)sound_id);
                    
                    sound_id++;

                    IntPtr cueIdPtr = IL2CPP.GetIl2CppNestedType(IL2CPP.GetIl2CppClass("Assembly-CSharp.dll", "Flash2", "sound_id"), "cue");
                    Il2CppSystem.Type cueIdType = UnhollowerRuntimeLib.Il2CppType.TypeFromPointer(cueIdPtr);

                    Il2CppSystem.Reflection.Assembly assembly = Il2CppSystem.AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "Assembly-CSharp");
                    Il2CppSystem.Type enumRuntimeHelper = assembly.GetType("Framework.EnumRuntimeHelper`1");
                    Il2CppSystem.Type erhCue = enumRuntimeHelper.MakeGenericType(new Il2CppReferenceArray<Il2CppSystem.Type>(new Il2CppSystem.Type[] { cueIdType }));

                    Il2CppSystem.Reflection.MethodInfo cueValToNameGetter = erhCue.GetProperty("valueToNameCollection").GetGetMethod();
                    Il2CppSystem.Collections.Generic.Dictionary<sound_id.cue, string> cueValToName = cueValToNameGetter.Invoke(null, new Il2CppReferenceArray<Il2CppSystem.Object>(0)).Cast<Il2CppSystem.Collections.Generic.Dictionary<sound_id.cue, string>>();
                    cueValToName.Add((sound_id.cue)default_cue_id, default_cue_id.ToString());
                    
                    Il2CppSystem.Reflection.MethodInfo cueNameToValGetter = erhCue.GetProperty("nameToValueCollection").GetGetMethod();
                    Il2CppSystem.Collections.Generic.Dictionary<string, sound_id.cue> cueNameToVal = cueNameToValGetter.Invoke(null, new Il2CppReferenceArray<Il2CppSystem.Object>(0)).Cast<Il2CppSystem.Collections.Generic.Dictionary<string, sound_id.cue>>();
                    cueNameToVal.Add(default_cue_id.ToString(), (sound_id.cue)default_cue_id);
                    
                    Main.Output("Loading sounds");
                    foreach (CustomCharacter character in Main.characterManager.characters)
                    {
                        try
                        {
                            if (!String.IsNullOrEmpty(character.monkey_acb))
                            {
                                Sound.Instance.m_cueSheetParamDict[(sound_id.cuesheet)sound_id] = new Sound.cuesheet_param_t(sound_id.ToString(), character.monkey_acb, null); //monkey_acb = full file path
                                // Load the character's voicebank. Move this elsewhere to avoid overload and crashing with too many characters.
                                // Sound.Instance.LoadCueSheetASync((sound_id.cuesheet)sound_id);
                                voicebanks.Add((sound_id.cuesheet)sound_id);
                                character.monkey_voice_id = sound_id;
                                sound_id++;
                            }
                            if (!String.IsNullOrEmpty(character.announcer_acb))
                            {
                                Sound.Instance.m_cueSheetParamDict[(sound_id.cuesheet)sound_id] = new Sound.cuesheet_param_t(sound_id.ToString(), character.announcer_acb, null); //monkey_acb = full file path
                                Sound.Instance.m_cueParamDict[(sound_id.cue)cue_id] = new Sound.cue_param_t((sound_id.cuesheet)sound_id, "Cue_02");
                                Sound.Instance.LoadCueSheetASync((sound_id.cuesheet)sound_id);
                                character.aCue = cue_id;
                                sound_id++;
                                cue_id++;
                            }
                            else
                            {
                                character.aCue = default_cue_id;
                            }
                        }
                        catch (Exception e) {Console.WriteLine(e.Message); }
                        
                    }
                    this.importedSounds = true;
                    Console.WriteLine("Done");
                }
            }
        }
    }
}
