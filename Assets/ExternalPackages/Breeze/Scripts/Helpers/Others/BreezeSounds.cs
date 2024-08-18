using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Breeze.Core
{
    [Serializable]
    public class soundCouple
    {
        public AudioClip clip;
        [Range(0.1f, 1.0f)] 
        public float volume = 1.0f;
    }

    public enum SoundType
    {
        Idle,
        AttackMissed,
        AttackSuccessful,
        TookDamage,
        Death,
        Alerted,
        LostTarget
    }
    public class BreezeSounds : MonoBehaviour
    {
        [Range(0.1f, 1.0f)] 
        public float GeneralVolumeMultiplier = 1.0f;
        
       //Stationary Sounds
       public List<soundCouple> IdleSounds = new List<soundCouple>();
       
       //Movement Sounds
       public List<soundCouple> WalkFootsteps = new List<soundCouple>();
       public List<soundCouple> RunFootsteps = new List<soundCouple>();
       
       //Combat Sounds
       public AudioClip OnAttackMissed;
       [Range(0.1f, 1.0f)] 
       public float OnAttackMissedVolume = 1.0f; 
       
       public AudioClip OnAttackSuccessful;
       [Range(0.1f, 1.0f)] 
       public float OnAttackSuccessfulVolume = 1.0f; 
       
       public AudioClip OnTookDamage;
       [Range(0.1f, 1.0f)] 
       public float OnTookDamageVolume = 1.0f; 
       
       public AudioClip OnDeath;
       [Range(0.1f, 1.0f)] 
       public float OnDeathVolume = 1.0f; 
       
       //Other Sounds
       public AudioClip OnAlerted;
       [Range(0.1f, 1.0f)] 
       public float OnAlertedVolume = 1.0f; 
       
       public AudioClip OnLostTarget;
       [Range(0.1f, 1.0f)] 
       public float OnLostTargetVolume = 1.0f;

       private AudioSource _source;

       private bool footstepPlaying = true;
       
       private void OnValidate()
       {
           if(Application.isPlaying)
               return;

           if (_source == null)
           {
               _source = GetComponent<AudioSource>() == null
                   ? gameObject.AddComponent<AudioSource>()
                   : GetComponent<AudioSource>();
               _source.playOnAwake = false;
           }
       }

       private void Awake()
       {
           if (_source == null)
           {
               _source = GetComponent<AudioSource>() == null
                   ? gameObject.AddComponent<AudioSource>()
                   : GetComponent<AudioSource>();
               _source.playOnAwake = false;
           }
           
           if (GetComponent<BreezeSystem>() != null)
           {
               GetComponent<BreezeSystem>().BreezeSounds = this;
           }
       }

       private void LateUpdate()
       {
           if (!footstepPlaying && !_source.isPlaying)
               footstepPlaying = true;
       }

       public void PlaySound(SoundType soundType)
       {
           if(_source.isPlaying)
               return;
           
           AudioClip clipToPlay = null;
           float volumeToAdjust = 0.0f;

           switch (soundType)
           {
               case SoundType.Idle:
                   if(IdleSounds.Count <= 0)
                       return;
                   soundCouple couple = IdleSounds[Random.Range(0, IdleSounds.Count)];
                   clipToPlay = couple.clip;
                   volumeToAdjust = couple.volume;
                   break;
               case SoundType.AttackMissed:
                   clipToPlay = OnAttackMissed;
                   volumeToAdjust = OnAttackMissedVolume;
                   break;
               case SoundType.AttackSuccessful:
                   clipToPlay = OnAttackSuccessful;
                   volumeToAdjust = OnAttackSuccessfulVolume;
                   break;
               case SoundType.TookDamage:
                   clipToPlay = OnTookDamage;
                   volumeToAdjust = OnTookDamageVolume;
                   break;
               case SoundType.Death:
                   clipToPlay = OnDeath;
                   volumeToAdjust = OnDeathVolume;
                   break;
               case SoundType.Alerted:
                   clipToPlay = OnAlerted;
                   volumeToAdjust = OnAlertedVolume;
                   break;
               case SoundType.LostTarget:
                   clipToPlay = OnLostTarget;
                   volumeToAdjust = OnLostTargetVolume;
                   break;
           }
           
           if(clipToPlay == null)
               return;
           
           footstepPlaying = false;
           
           volumeToAdjust *= GeneralVolumeMultiplier;
           
           _source.Stop();
           _source.loop = soundType == SoundType.Idle;
           _source.PlayOneShot(clipToPlay, volumeToAdjust);
       }

       public void Walk()
       {
           if(!footstepPlaying || WalkFootsteps.Count <= 0)
               return;

           soundCouple couple = WalkFootsteps[Random.Range(0, WalkFootsteps.Count)];
           var clipToPlay = couple.clip;
           var volumeToAdjust = couple.volume;
           
           _source.Stop();
           _source.loop = false;
           _source.PlayOneShot(clipToPlay, volumeToAdjust);
       }
       
       public void Run()
       {
           if(!footstepPlaying || RunFootsteps.Count <= 0)
               return;

           soundCouple couple = RunFootsteps[Random.Range(0, RunFootsteps.Count)];
           var clipToPlay = couple.clip;
           var volumeToAdjust = couple.volume;
           
           _source.Stop();
           _source.loop = false;
           _source.PlayOneShot(clipToPlay, volumeToAdjust);
       }
    }
}