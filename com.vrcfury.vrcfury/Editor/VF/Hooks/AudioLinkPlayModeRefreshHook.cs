﻿using System.Linq;
using UnityEditor;
using UnityEngine;
using VF.Menu;
using VRC.SDKBase.Editor.BuildPipeline;
using Object = UnityEngine.Object;

namespace VF.Hooks {
    /**
     * If a renderer comes into existence after audiolink has loaded, it will never attach to the new renderer. We fix
     * this by forcing a reload after any avatars are built.
     */
    internal class AudioLinkPlayModeRefreshHook : IVRCSDKPreprocessAvatarCallback {
        public int callbackOrder => int.MaxValue;
        private static bool triggerReload = false;

        public bool OnPreprocessAvatar(GameObject _vrcCloneObject) {
            triggerReload = true;
            EditorApplication.delayCall += DelayCall;
            return true;
        }

        private static void DelayCall() {
            if (!triggerReload) return;
            triggerReload = false;
            if (!Application.isPlaying) return;
            RestartAudiolink();
        }
        
        private static void RestartAudiolink() {
            if (!PlayModeMenuItem.Get()) return;

            var alComponentType = ReflectionUtils.GetTypeFromAnyAssembly("VRCAudioLink.AudioLink");
            if (alComponentType == null) alComponentType = ReflectionUtils.GetTypeFromAnyAssembly("AudioLink.AudioLink");
            if (alComponentType == null) return;
            foreach (var gm in Object.FindObjectsOfType(alComponentType).OfType<UnityEngine.Component>()) {
                if (gm.gameObject.activeSelf) {
                    Debug.Log($"VRCFury is restarting AudioLink object {gm.gameObject.name} ...");
                    gm.gameObject.SetActive(false);
                    gm.gameObject.SetActive(true);
                }
            }
        }
    }
}
