using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

public enum SoundEffectType {
    WIN, BUTTON_CLICK, MONSTER_HURT, MONSTER_DEFEATED, LUNG_DAMAGE, VAPE_BROKEN, CORRECT, CHAIN_START, CHAIN_EXTEND, CHAIN_END, FLIP_CARD, INCORRECT_MATCH
}

public class SoundManager : Singleton<SoundManager> {
    [SerializeField] private List<AudioClip> win;
    [SerializeField] private List<AudioClip> buttonClick;
    [SerializeField] private List<AudioClip> monsterHurt;
    [SerializeField] private List<AudioClip> monsterDefeated;
    [SerializeField] private List<AudioClip> lungDamage;
    [SerializeField] private List<AudioClip> vapeBroken;
    [SerializeField] private List<AudioClip> correct;
    [SerializeField] private List<AudioClip> chainStart;
    [SerializeField] private List<AudioClip> chainExtend;
    [SerializeField] private List<AudioClip> chainEnd;
    [SerializeField] private List<AudioClip> flipCard;
    [SerializeField] private List<AudioClip> incorrectMatch;
    [SerializeField] private AudioClip backgroundMusic;
    [Space]
    [SerializeField, Range(0f, 2f)] private float backgroundMusicVolume = 1;
    [SerializeField, Range(0f, 2f)] private float soundEffectVolume = 1;

    private AudioSource backgroundMusicSource;
    private List<AudioSource> soundEffectSources;
    private List<List<AudioClip>> soundEffects;

    protected override void Awake( ) {
        base.Awake( );

        backgroundMusicSource = gameObject.AddComponent<AudioSource>( );
        backgroundMusicSource.clip = backgroundMusic;
        backgroundMusicSource.playOnAwake = true;
        backgroundMusicSource.loop = true;
        backgroundMusicSource.volume = backgroundMusicVolume;
        backgroundMusicSource.Play( );

        // Uses the same order as the SoundEffectType enum
        soundEffects = new List<List<AudioClip>>( ) {
            win, buttonClick, monsterHurt, monsterDefeated, lungDamage, vapeBroken, correct, chainStart, chainExtend, chainEnd, flipCard, incorrectMatch
        };

        soundEffectSources = new List<AudioSource>( );
        for (int i = 0; i < soundEffects.Count; i++) {
            AudioSource source = gameObject.AddComponent<AudioSource>( );
            source.volume = soundEffectVolume;
            source.playOnAwake = false;
            soundEffectSources.Add(source);
        }
    }

    /// <summary>
    /// Play a specific sound effect
    /// </summary>
    /// <param name="soundEffectType">The type of sound effect to play</param>
    /// <param name="index">The specific index of the sound effect to play within its audio clip list. If set to -1, a random audio clip will be played.</param>
    /// <param name="pitch">The pitch to play the sound effect at. A value of 1 is the default pitch of the sound effect</param>
    public void PlaySoundEffect(SoundEffectType soundEffectType, int index = -1, float pitch = 1) {
        int soundEffectIndex = (int) soundEffectType;
        List<AudioClip> clips = soundEffects[soundEffectIndex];

        if (clips.Count == 0) {
            return;
        }

        soundEffectSources[soundEffectIndex].pitch = pitch;

        if (index < 0 || index >= clips.Count) {
            soundEffectSources[soundEffectIndex].PlayOneShot(clips[Random.Range(0, clips.Count)]);
        } else {
            soundEffectSources[soundEffectIndex].PlayOneShot(clips[index]);
        }
    }
}
