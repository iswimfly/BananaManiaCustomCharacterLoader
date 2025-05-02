using System;
using Flash2;
using Flash2.Selector.MainGame;
using Flash2.Selector.TimeAttack;
using HarmonyLib;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Runtime;
using UnhollowerRuntimeLib;

namespace CustomCharacterLoader.Patches
{
    // Delegates for OnSelect() methods
    public static class CharaNamePatch
    {
        // Delegate to rename character banner
        private delegate void OnSelectDelegate(IntPtr _thisPtr, IntPtr playerIndex, IntPtr inputLayer, IntPtr itemData);
        static void OnSelect(IntPtr _thisPtr, IntPtr playerIndex, IntPtr inputLayer, IntPtr itemData)
        {
            SelMgCharaItemData selectedChara = new(itemData);
            SelMgCharaSelectWindow _instance = new(_thisPtr);
            int charaIconId = selectedChara.costumeList[selectedChara.costumeIndex].spriteIcon.GetInstanceID();
            foreach(CharacterManager.CustomCharacter character in Main.characterManager.characters)
            {
                foreach (int instanceId in character.iconInstances)
                {
                    if (instanceId == charaIconId)
                    {
                        _instance.themeBeltView.SetCharacterName(character.costumeNames[selectedChara.costumeIndex]);
                        break;
                    }
                }
            }
        }


        // Another Delegate to rename character banner
        private delegate void OnButtonDownDelegate(IntPtr _thisPtr, IntPtr playerIndex, IntPtr inputLayer, AppInput.eAction actionLayer, IntPtr itemData);
        static void OnButtonDown(IntPtr _thisPtr, IntPtr playerIndex, IntPtr inputLayer, AppInput.eAction actionLayer, IntPtr itemData)
        {
            if (actionLayer == AppInput.eAction.Sel_ChangeCostume)
            {
                SelMgCharaItemData selectedChara = new(itemData);
                SelMgCharaSelectWindow _instance = new(_thisPtr);
                int charaIconId = selectedChara.costumeList[selectedChara.costumeIndex].spriteIcon.GetInstanceID();
                foreach (CharacterManager.CustomCharacter character in Main.characterManager.characters)
                {
                    foreach (int instanceId in character.iconInstances)
                    {
                        if (instanceId == charaIconId)
                        {
                            _instance.themeBeltView.SetCharacterName(character.costumeNames[selectedChara.costumeIndex]);
                            break;
                        }
                    }
                }
            }
        }

        // Main Game
        private static OnSelectDelegate mg_CustomSelect;
        private static OnSelectDelegate mg_OriginalSelect;
        private static OnButtonDownDelegate mg_CustomButtonDown;
        private static OnButtonDownDelegate mg_OriginalButtonDown;
        public static unsafe void CreateMainGameDetour()
        {
            mg_CustomSelect = MgOnSelect;
            var original = typeof(SelMgCharaSelectWindow).GetMethod(nameof(SelMgCharaSelectWindow.onSelect), AccessTools.all);
            var methodInfo = UnityVersionHandler.Wrap((Il2CppMethodInfo*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(original).GetValue(null));
            mg_OriginalSelect = ClassInjector.Detour.Detour(methodInfo.MethodPointer, mg_CustomSelect);

            mg_CustomButtonDown = MgOnButtonDown;
            original = typeof(SelMgCharaSelectWindow).GetMethod(nameof(SelMgCharaSelectWindow.onButtonDown), AccessTools.all);
            methodInfo = UnityVersionHandler.Wrap((Il2CppMethodInfo*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(original).GetValue(null));
            mg_OriginalButtonDown = ClassInjector.Detour.Detour(methodInfo.MethodPointer, mg_CustomButtonDown);

        }
        static void MgOnSelect(IntPtr _thisPtr, IntPtr playerIndex, IntPtr inputLayer, IntPtr itemData)
        {
            mg_OriginalSelect(_thisPtr, playerIndex, inputLayer, itemData);
            OnSelect(_thisPtr, playerIndex, inputLayer, itemData);
        }

        static void MgOnButtonDown(IntPtr _thisPtr, IntPtr playerIndex, IntPtr inputLayer, AppInput.eAction actionLayer, IntPtr itemData)
        {
            mg_OriginalButtonDown(_thisPtr, playerIndex, inputLayer, actionLayer, itemData);
            OnButtonDown(_thisPtr, playerIndex, inputLayer, actionLayer, itemData);
        }

        // Time Attack
        private static OnSelectDelegate ta_CustomSelect;
        private static OnSelectDelegate ta_OriginalSelect;
        private static OnButtonDownDelegate ta_CustomButtonDown;
        private static OnButtonDownDelegate ta_OriginalButtonDown;
        public static unsafe void CreateTimeAttackDetour()
        {
            ta_CustomSelect = TaOnSelect;
            var original = typeof(SelTaCharaSelectWindow).GetMethod(nameof(SelTaCharaSelectWindow.onSelect), AccessTools.all);
            var methodInfo = UnityVersionHandler.Wrap((Il2CppMethodInfo*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(original).GetValue(null));
            ta_OriginalSelect = ClassInjector.Detour.Detour(methodInfo.MethodPointer, ta_CustomSelect);

            ta_CustomButtonDown = TaOnButtonDown;
            original = typeof(SelMgCharaSelectWindow).GetMethod(nameof(SelTaCharaSelectWindow.onButtonDown), AccessTools.all);
            methodInfo = UnityVersionHandler.Wrap((Il2CppMethodInfo*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(original).GetValue(null));
            ta_OriginalButtonDown = ClassInjector.Detour.Detour(methodInfo.MethodPointer, ta_CustomButtonDown);
        }
        static void TaOnSelect(IntPtr _thisPtr, IntPtr playerIndex, IntPtr inputLayer, IntPtr itemData)
        {
            ta_OriginalSelect(_thisPtr, playerIndex, inputLayer, itemData);
            OnSelect(_thisPtr, playerIndex, inputLayer, itemData);
        }
        static void TaOnButtonDown(IntPtr _thisPtr, IntPtr playerIndex, IntPtr inputLayer, AppInput.eAction actionLayer, IntPtr itemData)
        {
            ta_OriginalButtonDown(_thisPtr, playerIndex, inputLayer, actionLayer, itemData);
            OnButtonDown(_thisPtr, playerIndex, inputLayer, actionLayer, itemData);
        }
    }
}
