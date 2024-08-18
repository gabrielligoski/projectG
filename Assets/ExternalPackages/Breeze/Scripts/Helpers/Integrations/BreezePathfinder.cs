#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Breeze.Core
{
    public class BreezePathfinder : MonoBehaviour
    {
#if !Breeze_AI_Pathfinder_Enabled
        [MenuItem("Breeze/Integrations/Enable A* Pathfinder Integration")]
        public static void enableIntegration()
        {
            AddRemoveDefine("Breeze_AI_Pathfinder_Enabled", true);
        }
#else
        [MenuItem("Breeze AI/Integrations/Disable A* Pathfinder Integration")]
        public static void enableIntegration()
        {
            AddRemoveDefine("Breeze_AI_Pathfinder_Enabled", false);
        }
#endif
        
        private static void AddRemoveDefine(string DefineName, bool add)
        {
            List<string> Symbols = new List<string> { DefineName };

            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> allDefines = definesString.Split(';').ToList();
            
            switch (add)
            {
                case true:
                    allDefines.AddRange(Symbols.Except(allDefines));
                    break;
                case false when allDefines.Contains(DefineName):
                    allDefines.Remove(DefineName);
                    break;
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                string.Join(";", allDefines.ToArray()));
        }
    }   
}
#endif