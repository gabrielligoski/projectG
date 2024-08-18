#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Breeze.Core
{
    public class BreezeAnimations : MonoBehaviour
    {
        public BreezeSystem System;

        // Idle Tab
        public List<AnimationClip> BaseIdleAnims = new List<AnimationClip>();
        public List<AnimationClip> CombatIdleAnims = new List<AnimationClip>();

        // Movement Tab
        public AnimationClip WalkFwdAnim;
        public bool MirrorWalkFwdAnim;
        public AnimationClip WalkRAnim;
        public bool MirrorWalkRAnim;
        public AnimationClip WalkLAnim;
        public bool MirrorWalkLAnim;
        public AnimationClip WalkBwdAnim;
        public bool MirrorWalkBwdAnim;

        public AnimationClip RunFwdAnim;
        public bool MirrorRunFwdAnim;
        public AnimationClip RunRAnim;
        public bool MirrorRunRAnim;
        public AnimationClip RunLAnim;
        public bool MirrorRunLAnim;

        public AnimationClip CWalkFwdAnim;
        public bool CMirrorWalkFwdAnim;
        public AnimationClip CWalkRAnim;
        public bool CMirrorWalkRAnim;
        public AnimationClip CWalkLAnim;
        public bool CMirrorWalkLAnim;
        public AnimationClip CWalkBwdAnim;
        public bool CMirrorWalkBwdAnim;

        public AnimationClip CRunFwdAnim;
        public bool CMirrorRunFwdAnim;
        public AnimationClip CRunRAnim;
        public bool CMirrorRunRAnim;
        public AnimationClip CRunLAnim;
        public bool CMirrorRunLAnim;

        // Combat Tab
        public List<AttackAnimBase> AttackAnims = new List<AttackAnimBase>();
        public List<AnimationClip> HitAnims = new List<AnimationClip>();

        public AnimationClip EquipAnim;
        public AnimationClip UnEquipAnim;

        public List<AnimationClip> BlockAnims = new List<AnimationClip>();

        public AnimationClip ReloadAnim;

        public List<AnimationClip> DeathAnims = new List<AnimationClip>();

        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            System = GetComponent<BreezeSystem>();
        }

        public void Copy()
        {
            CWalkFwdAnim = WalkFwdAnim;
            CWalkBwdAnim = WalkBwdAnim;
            CMirrorWalkBwdAnim = MirrorWalkBwdAnim;
            CWalkRAnim = WalkRAnim;
            CWalkLAnim = WalkLAnim;
            CRunFwdAnim = RunFwdAnim;
            CRunLAnim = RunLAnim;
            CRunRAnim = RunRAnim;
        }

        public void Done(RuntimeAnimatorController m_RuntimeAnimatorController)
        {
            GetComponent<Animator>().runtimeAnimatorController = m_RuntimeAnimatorController;
            AnimatorController m_AnimatorController = m_RuntimeAnimatorController as AnimatorController;

            if (m_AnimatorController == null)
                return;

            System.IdleAnimCount = BaseIdleAnims.Count;
            System.CIdleAnimCount = CombatIdleAnims.Count;
            System.HitAnimCount = HitAnims.Count;
            System.BlockAnimCount = BlockAnims.Count;
            System.AttackAnimCount = AttackAnims.Count;
            System.DeathAnimCount = DeathAnims.Count;

            System.AttackDamagesList.Clear();
            foreach (var t in AttackAnims)
            {
                System.AttackDamagesList.Add(new attackDamages
                {
                    minDamage = t.MinDamage,
                    MaxDamage = t.MaxDamage
                });
            }

            if (System.WeaponType != BreezeEnums.WeaponType.Unarmed)
            {
                if (System.WeaponType == BreezeEnums.WeaponType.Shooter)
                {
                    AssignState("Reload", m_AnimatorController);
                }

                if (System.UseEquipSystem)
                {
                    AssignState("Draw", m_AnimatorController);
                    AssignState("Holster", m_AnimatorController);
                }
            }

            if (System.HitReactionPossibility > 0)
            {
                AssignStates("Hit States", m_AnimatorController);
                AssignStates("Combat Hit States", m_AnimatorController);
            }

            if (System.UseBlockingSystem)
            {
                AssignStates("Block States", m_AnimatorController);
            }

            if (AttackAnims.Count > 0)
            {
                AssignStates("Attack States", m_AnimatorController);
            }

            if (DeathAnims.Count > 0)
            {
                AssignStates("Death States", m_AnimatorController);
            }

            // Base Movement Blend Tree
            AnimatorState mStateMachine =
                m_AnimatorController.layers[0].stateMachine.states[0].state;
            BlendTree movementBlendTree =
                mStateMachine.motion as BlendTree;

            var serializedIdleBlendTreeRef = new SerializedObject(movementBlendTree);
            var MovementBlendTreeChildren = serializedIdleBlendTreeRef.FindProperty("m_Childs");

            // back
            var MovementMotionSlot = MovementBlendTreeChildren.GetArrayElementAtIndex(0);
            var MovementMotion = MovementMotionSlot.FindPropertyRelative("m_Motion");
            BlendTree BackBlendTree =
                MovementMotion.objectReferenceValue as BlendTree;
            var SerializedBackBlendTree = new SerializedObject(BackBlendTree);
            var BackBlendTreeChildren = SerializedBackBlendTree.FindProperty("m_Childs");
            var BackBaseanim = BackBlendTreeChildren.GetArrayElementAtIndex(0).FindPropertyRelative("m_Motion");
            BackBaseanim.objectReferenceValue = WalkBwdAnim;
            BackBaseanim = BackBlendTreeChildren.GetArrayElementAtIndex(0).FindPropertyRelative("m_TimeScale");
            BackBaseanim.floatValue = MirrorWalkBwdAnim ? -1 : 1;

            // Idle
            var MovementMotionSlot0 = MovementBlendTreeChildren.GetArrayElementAtIndex(1);
            var MovementMotion0 = MovementMotionSlot0.FindPropertyRelative("m_Motion");
            BlendTree IdleBlendTree =
                MovementMotion0.objectReferenceValue as BlendTree;
            var SerializedIdleBlendTree = new SerializedObject(IdleBlendTree);
            var IdleBlendTreeChildren = SerializedIdleBlendTree.FindProperty("m_Childs");

            for (int i = 0; i < BaseIdleAnims.Count; i++)
            {
                var anim = IdleBlendTreeChildren.GetArrayElementAtIndex(i).FindPropertyRelative("m_Motion");
                anim.objectReferenceValue = BaseIdleAnims[i];
            }

            // Walking
            var MovementMotionSlot1 = MovementBlendTreeChildren.GetArrayElementAtIndex(2);
            var MovementMotion1 = MovementMotionSlot1.FindPropertyRelative("m_Motion");
            BlendTree WalkBlendTree =
                MovementMotion1.objectReferenceValue as BlendTree;
            var SerializedWalkBlendTree = new SerializedObject(WalkBlendTree);
            var WalkBlendTreeChildren = SerializedWalkBlendTree.FindProperty("m_Childs");
            var R_Anim = WalkBlendTreeChildren.GetArrayElementAtIndex(0).FindPropertyRelative("m_Motion");
            R_Anim.objectReferenceValue = WalkRAnim;
            R_Anim = WalkBlendTreeChildren.GetArrayElementAtIndex(0).FindPropertyRelative("m_Mirror");
            R_Anim.boolValue = MirrorWalkRAnim;
            var F_Anim = WalkBlendTreeChildren.GetArrayElementAtIndex(1).FindPropertyRelative("m_Motion");
            F_Anim.objectReferenceValue = WalkFwdAnim;
            F_Anim = WalkBlendTreeChildren.GetArrayElementAtIndex(1).FindPropertyRelative("m_Mirror");
            F_Anim.boolValue = MirrorWalkFwdAnim;
            var L_Anim = WalkBlendTreeChildren.GetArrayElementAtIndex(2).FindPropertyRelative("m_Motion");
            L_Anim.objectReferenceValue = WalkLAnim;
            L_Anim = WalkBlendTreeChildren.GetArrayElementAtIndex(2).FindPropertyRelative("m_Mirror");
            L_Anim.boolValue = MirrorWalkLAnim;

            // Running
            var MovementMotionSlot2 = MovementBlendTreeChildren.GetArrayElementAtIndex(3);
            var MovementMotion2 = MovementMotionSlot2.FindPropertyRelative("m_Motion");
            BlendTree RunBlendTree =
                MovementMotion2.objectReferenceValue as BlendTree;
            var SerializedRunBlendTree = new SerializedObject(RunBlendTree);
            var RunBlendTreeChildren = SerializedRunBlendTree.FindProperty("m_Childs");
            var Run_R_Anim = RunBlendTreeChildren.GetArrayElementAtIndex(0).FindPropertyRelative("m_Motion");
            Run_R_Anim.objectReferenceValue = RunRAnim;
            Run_R_Anim = RunBlendTreeChildren.GetArrayElementAtIndex(0).FindPropertyRelative("m_Mirror");
            Run_R_Anim.boolValue = MirrorRunRAnim;
            var Run_F_Anim = RunBlendTreeChildren.GetArrayElementAtIndex(1).FindPropertyRelative("m_Motion");
            Run_F_Anim.objectReferenceValue = RunFwdAnim;
            Run_F_Anim = RunBlendTreeChildren.GetArrayElementAtIndex(1).FindPropertyRelative("m_Mirror");
            Run_F_Anim.boolValue = MirrorRunFwdAnim;
            var Run_L_Anim = RunBlendTreeChildren.GetArrayElementAtIndex(2).FindPropertyRelative("m_Motion");
            Run_L_Anim.objectReferenceValue = RunLAnim;
            Run_L_Anim = RunBlendTreeChildren.GetArrayElementAtIndex(2).FindPropertyRelative("m_Mirror");
            Run_L_Anim.boolValue = MirrorRunLAnim;


            for (int a = 0; a < 2; a++)
            {
                if (a > 0 && System.WeaponType == BreezeEnums.WeaponType.Unarmed)
                    break;

                // Combat Movement Blend Tree
                AnimatorState CStateMachine =
                    m_AnimatorController.layers[a].stateMachine.states[a > 0 ? 0 : 1].state;
                BlendTree CmovementBlendTree =
                    CStateMachine.motion as BlendTree;

                var CserializedIdleBlendTreeRef = new SerializedObject(CmovementBlendTree);
                var CMovementBlendTreeChildren = CserializedIdleBlendTreeRef.FindProperty("m_Childs");

                // Back
                var CMovementMotionSlot = CMovementBlendTreeChildren.GetArrayElementAtIndex(0);
                var CMovementMotion = CMovementMotionSlot.FindPropertyRelative("m_Motion");
                BlendTree CBackBlendTree =
                    CMovementMotion.objectReferenceValue as BlendTree;
                var CSerializedBackBlendTree = new SerializedObject(CBackBlendTree);
                var CBackBlendTreeChildren = CSerializedBackBlendTree.FindProperty("m_Childs");
                var Backanim = CBackBlendTreeChildren.GetArrayElementAtIndex(0).FindPropertyRelative("m_Motion");
                Backanim.objectReferenceValue = CWalkBwdAnim;
                Backanim = CBackBlendTreeChildren.GetArrayElementAtIndex(0).FindPropertyRelative("m_TimeScale");
                Backanim.floatValue = CMirrorWalkBwdAnim ? -1 : 1;

                // Idle
                var CMovementMotionSlot0 = CMovementBlendTreeChildren.GetArrayElementAtIndex(1);
                var CMovementMotion0 = CMovementMotionSlot0.FindPropertyRelative("m_Motion");
                BlendTree CIdleBlendTree =
                    CMovementMotion0.objectReferenceValue as BlendTree;
                var CSerializedIdleBlendTree = new SerializedObject(CIdleBlendTree);
                var CIdleBlendTreeChildren = CSerializedIdleBlendTree.FindProperty("m_Childs");

                for (int i = 0; i < CombatIdleAnims.Count; i++)
                {
                    var anim = CIdleBlendTreeChildren.GetArrayElementAtIndex(i).FindPropertyRelative("m_Motion");
                    anim.objectReferenceValue = CombatIdleAnims[i];
                }

                // Walking
                var CMovementMotionSlot1 = CMovementBlendTreeChildren.GetArrayElementAtIndex(2);
                var CMovementMotion1 = CMovementMotionSlot1.FindPropertyRelative("m_Motion");
                BlendTree CWalkBlendTree =
                    CMovementMotion1.objectReferenceValue as BlendTree;
                var CSerializedWalkBlendTree = new SerializedObject(CWalkBlendTree);
                var CWalkBlendTreeChildren = CSerializedWalkBlendTree.FindProperty("m_Childs");
                var CR_Anim = CWalkBlendTreeChildren.GetArrayElementAtIndex(0).FindPropertyRelative("m_Motion");
                CR_Anim.objectReferenceValue = CWalkRAnim;
                CR_Anim = CWalkBlendTreeChildren.GetArrayElementAtIndex(0).FindPropertyRelative("m_Mirror");
                CR_Anim.boolValue = CMirrorWalkRAnim;
                var CF_Anim = CWalkBlendTreeChildren.GetArrayElementAtIndex(1).FindPropertyRelative("m_Motion");
                CF_Anim.objectReferenceValue = CWalkFwdAnim;
                CF_Anim = CWalkBlendTreeChildren.GetArrayElementAtIndex(1).FindPropertyRelative("m_Mirror");
                CF_Anim.boolValue = CMirrorWalkFwdAnim;
                var CL_Anim = CWalkBlendTreeChildren.GetArrayElementAtIndex(2).FindPropertyRelative("m_Motion");
                CL_Anim.objectReferenceValue = CWalkLAnim;
                CL_Anim = CWalkBlendTreeChildren.GetArrayElementAtIndex(2).FindPropertyRelative("m_Mirror");
                CL_Anim.boolValue = CMirrorWalkLAnim;

                // Running
                var CMovementMotionSlot2 = CMovementBlendTreeChildren.GetArrayElementAtIndex(3);
                var CMovementMotion2 = CMovementMotionSlot2.FindPropertyRelative("m_Motion");
                BlendTree CRunBlendTree =
                    CMovementMotion2.objectReferenceValue as BlendTree;
                var CSerializedRunBlendTree = new SerializedObject(CRunBlendTree);
                var CRunBlendTreeChildren = CSerializedRunBlendTree.FindProperty("m_Childs");
                var CRun_R_Anim = CRunBlendTreeChildren.GetArrayElementAtIndex(0).FindPropertyRelative("m_Motion");
                CRun_R_Anim.objectReferenceValue = CRunRAnim;
                CRun_R_Anim = CRunBlendTreeChildren.GetArrayElementAtIndex(0).FindPropertyRelative("m_Mirror");
                CRun_R_Anim.boolValue = CMirrorRunRAnim;
                var CRun_F_Anim = CRunBlendTreeChildren.GetArrayElementAtIndex(1).FindPropertyRelative("m_Motion");
                CRun_F_Anim.objectReferenceValue = CRunFwdAnim;
                CRun_F_Anim = CRunBlendTreeChildren.GetArrayElementAtIndex(1).FindPropertyRelative("m_Mirror");
                CRun_F_Anim.boolValue = CMirrorRunFwdAnim;
                var CRun_L_Anim = CRunBlendTreeChildren.GetArrayElementAtIndex(2).FindPropertyRelative("m_Motion");
                CRun_L_Anim.objectReferenceValue = CRunLAnim;
                CRun_L_Anim = CRunBlendTreeChildren.GetArrayElementAtIndex(2).FindPropertyRelative("m_Mirror");
                CRun_L_Anim.boolValue = CMirrorRunLAnim;

                CSerializedIdleBlendTree.ApplyModifiedProperties();
                CSerializedBackBlendTree.ApplyModifiedProperties();
                CSerializedRunBlendTree.ApplyModifiedProperties();
                CSerializedWalkBlendTree.ApplyModifiedProperties();
            }

            SerializedBackBlendTree.ApplyModifiedProperties();
            SerializedIdleBlendTree.ApplyModifiedProperties();
            SerializedRunBlendTree.ApplyModifiedProperties();
            SerializedWalkBlendTree.ApplyModifiedProperties();
        }

        private void AssignStates(string stateName, AnimatorController m_AnimatorController)
        {
            foreach (var t in m_AnimatorController.layers[0].stateMachine.stateMachines)
            {
                if (t.stateMachine.name == stateName)
                {
                    for (int j = 0; j < GetsListCount(stateName); j++)
                    {
                        t.stateMachine.states[j].state
                            .motion = GetsClip(stateName, j);
                    }
                }
            }
        }

        public void ExportAnimations()
        {
            string FilePath = EditorUtility.SaveFilePanelInProject("Save as Animations Storage",
                System.name + " Animations",
                "asset", "Please enter a file name to save the file to");

            if (FilePath != string.Empty)
            {
                BreezeAnimationsStorage asset = ScriptableObject.CreateInstance<BreezeAnimationsStorage>();

                string UserFilePath = FilePath;
                AssetDatabase.CreateAsset(asset, UserFilePath);
                BreezeAnimationsStorage storage =
                    AssetDatabase.LoadAssetAtPath(UserFilePath, typeof(ScriptableObject)) as BreezeAnimationsStorage;
                EditorUtility.SetDirty(this);

                if (storage == null)
                    return;

                storage.BaseIdleAnims = BaseIdleAnims;
                storage.CombatIdleAnims = CombatIdleAnims;
                storage.AttackAnims = AttackAnims;
                storage.BlockAnims = BlockAnims;
                storage.EquipAnim = EquipAnim;
                storage.DeathAnims = DeathAnims;
                storage.HitAnims = HitAnims;
                storage.ReloadAnim = ReloadAnim;
                storage.RunFwdAnim = RunFwdAnim;
                storage.RunLAnim = RunLAnim;
                storage.RunRAnim = RunRAnim;
                storage.UnEquipAnim = UnEquipAnim;
                storage.WalkBwdAnim = WalkBwdAnim;
                storage.WalkFwdAnim = WalkFwdAnim;
                storage.WalkLAnim = WalkLAnim;
                storage.WalkRAnim = WalkRAnim;
                storage.CRunFwdAnim = CRunFwdAnim;
                storage.CRunLAnim = CRunLAnim;
                storage.CRunRAnim = CRunRAnim;
                storage.CWalkBwdAnim = CWalkBwdAnim;
                storage.CWalkFwdAnim = CWalkFwdAnim;
                storage.CWalkLAnim = CWalkLAnim;
                storage.CWalkRAnim = CWalkRAnim;
                storage.MirrorRunFwdAnim = MirrorRunFwdAnim;
                storage.MirrorRunLAnim = MirrorRunLAnim;
                storage.MirrorRunRAnim = MirrorRunRAnim;
                storage.MirrorWalkBwdAnim = MirrorWalkBwdAnim;
                storage.MirrorWalkFwdAnim = MirrorWalkFwdAnim;
                storage.MirrorWalkLAnim = MirrorWalkLAnim;
                storage.MirrorWalkRAnim = MirrorWalkRAnim;
                storage.CMirrorRunFwdAnim = CMirrorRunFwdAnim;
                storage.CMirrorRunLAnim = CMirrorRunLAnim;
                storage.CMirrorRunRAnim = CMirrorRunRAnim;
                storage.CMirrorWalkBwdAnim = CMirrorWalkBwdAnim;
                storage.CMirrorWalkFwdAnim = CMirrorWalkFwdAnim;
                storage.CMirrorWalkLAnim = CMirrorWalkLAnim;
                storage.CMirrorWalkRAnim = CMirrorWalkRAnim;
            }
        }

        public void ImportAnimations()
        {
            string FilePath = EditorUtility.OpenFilePanel("Select An Animations Storage", "", "asset");

            if (FilePath != string.Empty)
            {
                FilePath = "Assets" + FilePath.Split(new string[] { "Assets" }, StringSplitOptions.None)[1];
                BreezeAnimationsStorage storage =
                    AssetDatabase.LoadAssetAtPath(FilePath, typeof(ScriptableObject)) as BreezeAnimationsStorage;
                EditorUtility.SetDirty(this);

                if (storage == null)
                    return;

                BaseIdleAnims = storage.BaseIdleAnims;
                CombatIdleAnims = storage.CombatIdleAnims;
                AttackAnims = storage.AttackAnims;
                BlockAnims = storage.BlockAnims;
                EquipAnim = storage.EquipAnim;
                DeathAnims = storage.DeathAnims;
                HitAnims = storage.HitAnims;
                ReloadAnim = storage.ReloadAnim;
                RunFwdAnim = storage.RunFwdAnim;
                RunLAnim = storage.RunLAnim;
                RunRAnim = storage.RunRAnim;
                UnEquipAnim = storage.UnEquipAnim;
                WalkBwdAnim = storage.WalkBwdAnim;
                WalkFwdAnim = storage.WalkFwdAnim;
                WalkLAnim = storage.WalkLAnim;
                WalkRAnim = storage.WalkRAnim;
                CRunFwdAnim = storage.CRunFwdAnim;
                CRunLAnim = storage.CRunLAnim;
                CRunRAnim = storage.CRunRAnim;
                CWalkBwdAnim = storage.CWalkBwdAnim;
                CWalkFwdAnim = storage.CWalkFwdAnim;
                CWalkLAnim = storage.CWalkLAnim;
                CWalkRAnim = storage.CWalkRAnim;
                MirrorRunFwdAnim = storage.MirrorRunFwdAnim;
                MirrorRunLAnim = storage.MirrorRunLAnim;
                MirrorRunRAnim = storage.MirrorRunRAnim;
                MirrorWalkBwdAnim = storage.MirrorWalkBwdAnim;
                MirrorWalkFwdAnim = storage.MirrorWalkFwdAnim;
                MirrorWalkLAnim = storage.MirrorWalkLAnim;
                MirrorWalkRAnim = storage.MirrorWalkRAnim;
                CMirrorRunFwdAnim = storage.CMirrorRunFwdAnim;
                CMirrorRunLAnim = storage.CMirrorRunLAnim;
                CMirrorRunRAnim = storage.CMirrorRunRAnim;
                CMirrorWalkBwdAnim = storage.CMirrorWalkBwdAnim;
                CMirrorWalkFwdAnim = storage.CMirrorWalkFwdAnim;
                CMirrorWalkLAnim = storage.CMirrorWalkLAnim;
                CMirrorWalkRAnim = storage.CMirrorWalkRAnim;
            }
        }

        private void AssignState(string stateName, AnimatorController m_AnimatorController)
        {
            foreach (var t in m_AnimatorController.layers[0].stateMachine.states)
            {
                if (t.state.name == stateName)
                {
                    t.state.motion = GetClip(stateName);
                }
            }
        }

        private AnimationClip GetClip(string _name)
        {
            return _name switch
            {
                "Reload" => ReloadAnim,
                "Draw" => EquipAnim,
                "Holster" => UnEquipAnim,
                _ => null
            };
        }

        private AnimationClip GetsClip(string _name, int index)
        {
            return _name switch
            {
                "Hit States" => HitAnims[index],
                "Combat Hit States" => HitAnims[index],
                "Block States" => BlockAnims[index],
                "Attack States" => AttackAnims[index].AttackAnim,
                "Death States" => DeathAnims[index],
                _ => null
            };
        }

        private int GetsListCount(string _name)
        {
            return _name switch
            {
                "Hit States" => HitAnims.Count,
                "Combat Hit States" => HitAnims.Count,
                "Block States" => BlockAnims.Count,
                "Attack States" => AttackAnims.Count,
                "Death States" => DeathAnims.Count,
                _ => 0
            };
        }
    }
}
#endif