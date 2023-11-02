//Author: Dan Lichtin
//File Name: SFXManager.cs
//Project Name: PASS3
//Creation Date: December 29, 2022
//Modified Date: January 22, 2023
//Description: Manages all the sound effects in the game,
//created for easy access to generate a sound whenever needed 
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
//PROOF OF CONCEPT:
//Lists - The SFX Manager carries a list of sound effects & sound effect instances

namespace PASS3___Grade_12
{
    public class SFXManager
    {
        //Lists of sound effects and sound effect instances
        private List<SoundEffect> soundSFX;
        private List<SoundEffectInstance> soundEffects = new List<SoundEffectInstance>();

        //Sound identifiers
        private const int CLICK = 0;
        private const int ERROR = 1;
        private const int PURCHASE = 2;
        private const int WALK = 3;
        private const int SHOOTING = 4;
        private const int RELOAD = 5;
        private const int DEATH = 6;
        private const int WAVE = 7;
        private const int HIT = 8;
        private const int JUMP = 9;

        //Buffer
        private int walkBuffer = 0;

        public SFXManager(List<SoundEffect> soundSFX)
        {
            this.soundSFX = soundSFX;

            //Looping thorugh all the sfx and adding them as instances
            for (int i = 0; i < soundSFX.Count; i++)
            {
                soundEffects.Add(soundSFX[i].CreateInstance());
            }
        }

        public void PlayClick()
        {
            PlaySound(CLICK);
        }

        public void PlayError()
        {
            PlaySound(ERROR);
        }

        public void PlayPurchase()
        {
            PlaySound(PURCHASE);
        }

        public void PlayWalk()
        {
            //Access this statement if the walk buffer is 0
            if (walkBuffer == 0)
            {
                //Play the walk sfx
                PlaySound(WALK);
            }
            
            //Depending on the value of walk buffer; add or reset it's value
            if (walkBuffer >= 20)
            {
                walkBuffer = 0;
            }
            else
            {
                walkBuffer++;
            }
        }

        public void PlayShooting()
        {
            soundSFX[SHOOTING].CreateInstance().Play();
        }

        public void PlayReload()
        {
            PlaySound(RELOAD);
        }

        public void PlayDeath()
        {
            soundSFX[DEATH].CreateInstance().Play();
        }

        public void PlayWave()
        {
            soundSFX[WAVE].CreateInstance().Play();
        }

        public void PlayHit()
        {
            soundSFX[HIT].CreateInstance().Play();
        }

        public void PlayJump()
        {
            soundSFX[JUMP].CreateInstance().Play();
        }

        //Pre: An identifier (int) as to which sound to play
        //Post: None
        //Desc: Plays a sound as long as it is not currently playing
        private void PlaySound(int identifier)
        {
            //Access this statement if the sound isn't playing
            if (soundEffects[identifier].State == SoundState.Stopped)
            {
                //Play the sound
                soundEffects[identifier].Play();
            }
        }
    }
}
