#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Breeze.Core
{

    [InitializeOnLoad]
    public class DefineLoad : MonoBehaviour
    {

        private static bool added = false;

        static DefineLoad()
        {
            if(!added)
                AddRemoveDefine("Breeze_Behaviour", true);
        }

        private static void AddRemoveDefine(string DefineName, bool add)
        {
            
            List<string> Symbols = new List<string>();
            Symbols.Add(DefineName);
          
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
            added = true;
        }
    }
    
}

#endif