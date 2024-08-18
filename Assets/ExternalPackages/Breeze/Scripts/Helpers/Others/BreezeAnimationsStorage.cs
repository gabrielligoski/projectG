using System;
using System.Collections.Generic;
using UnityEngine;

namespace Breeze.Core
{
    [Serializable]
    public class AttackAnimBase
    {
        public AnimationClip AttackAnim;
        public int MinDamage = 15;
        public int MaxDamage = 30;
    }
    public class BreezeAnimationsStorage : ScriptableObject
    {
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
    }   
}
