#if UNITY_ANDROID
using System.IO;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

namespace Frontier.Editor
{
    public sealed class QuestGradleAssets : IPostGenerateGradleAndroidProject
    {
        // Run after Meta's Gradle generation callback (99999).
        public int callbackOrder=>100000;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            if(PlayerSettings.Android.targetArchitectures!=AndroidArchitecture.ARM64)
                throw new BuildFailedException("Quest builds must target ARM64 only.");
            // OVRPlugin.aar ships ARMv7 libraries under assets/lib, where ndk
            // abiFilters does not apply. Filter them during Gradle asset merging
            // without changing the installed package or its ARM64 libraries.
            const string marker="// Little Legion: exclude ARMv7 SDK assets";
            foreach(string module in new[]{path,Path.Combine(Path.GetDirectoryName(path),"launcher")})
            {
                string gradle=Path.Combine(module,"build.gradle");
                if(!File.Exists(gradle))throw new BuildFailedException("Gradle module not found: "+module);
                string source=File.ReadAllText(gradle);
                if(!source.Contains(marker))
                    File.AppendAllText(gradle,"\n"+marker+"\nandroid.androidResources.ignoreAssetsPatterns.add('armeabi-v7a')\n");
            }
            Debug.Log("LITTLE_LEGION_ARM64_ASSETS_CONFIGURED");
        }
    }
}
#endif
