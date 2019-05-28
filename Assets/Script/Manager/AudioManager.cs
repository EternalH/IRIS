using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// IRIS·Manager
/// </summary>
namespace IRIS.Manager
{
    /// <summary>
    /// 游戏音频管理
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public AudioManager Instance;

        public Slider mVSlider;
        public Slider sESlider;
        public AudioSource mainVolume;
        public List<AudioSource> soundEffects = new List<AudioSource>();

        public void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            mainVolume = GameObject.Find("Directional Light").GetComponent<AudioSource>();

            GameObject scene = GameObject.FindGameObjectWithTag("Scene");
            foreach (Transform child in scene.transform)
            {
                foreach (Transform gChild in child.transform)
                {
                    if (gChild.GetComponentInChildren<AudioSource>() != null)
                    {
                        AudioSource audio = gChild.GetComponentInChildren<AudioSource>();
                        soundEffects.Add(audio);
                    }
                }
            }

            InitVolume();
        }

        /// <summary>
        /// 设置主音量
        /// </summary>
        public void SetMainVolume()
        {
            if (mainVolume != null)
                mainVolume.volume = mVSlider.value;
            PlayerPrefs.SetFloat("MainVolume", mVSlider.value);
        }

        /// <summary>
        /// 设置音效
        /// </summary>
        public void SetSoundEffect()
        {
            if (soundEffects != null)
            {
                for (int i = 0; i < soundEffects.Count(); i++)
                {
                    soundEffects[i].volume = sESlider.value;
                }
            }
            PlayerPrefs.SetFloat("SoundEffects", sESlider.value);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 初始化音量
        /// </summary>
        public void InitVolume()
        {
            mVSlider.value = PlayerPrefs.GetFloat("MainVolume", 1.0f);
            sESlider.value = PlayerPrefs.GetFloat("SoundEffects", 1.0f);

            //主音量
            if (mainVolume != null)
                mainVolume.volume = mVSlider.value;
            //音效
            if (soundEffects != null)
            {
                for (int i = 0; i < soundEffects.Count(); i++)
                {
                    soundEffects[i].volume = sESlider.value;
                }

            }
        }
    }
}

