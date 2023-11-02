//Author: Dan Lichtin
//File Name: SawedOff.cs
//Project Name: PASS3
//Creation Date: December 16, 2022
//Modified Date: January 22, 2023
//Description: The child class of the gun parent class;
//Sawed Off Shotgun; Double bullets, locked weapon (has to be unlocked)
using Animation2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
//PROOF OF CONCEPT:
//OOP - Gun is the parent class and the sawed off is the child; sawed off carries it's own magsize and shooting style
//Lists - Gun carries a list of bullets

namespace PASS3___Grade_12
{
    class SawedOff : Gun
    {
        public SawedOff(GraphicsDevice gd, Texture2D[] gunImgs, Texture2D bulletImg,
            Texture2D reloadIcon, Rectangle reloadIconRec, List<Enemy> enemies, List<Player> players, int gunHolder, bool upgrade) 
            : base(gd, gunImgs, bulletImg, reloadIcon, reloadIconRec, enemies, players, gunHolder, upgrade)
        {
            //Defining anims
            gunAnims[SAWED_OFF + IDLE] = new Animation(gunImgs[SAWED_OFF + IDLE], 1, 1, 1, 0, 0, Animation.ANIMATE_FOREVER, 1, gunLoc, 0.8f, true);
            gunAnims[SAWED_OFF + SHOOTING] = new Animation(gunImgs[SAWED_OFF + SHOOTING], 14, 1, 14, 0, 0,
                Animation.ANIMATE_FOREVER, 2, gunLoc, 0.8f, true);

            //Setting attributes
            selectedGun = SAWED_OFF;
            gunType = "Shotgun";
            magSize = 8;
        }

        //Pre: The rectangle of the player, gameTime, the direction of the gun, and a gunstate (int)
        //Post: None
        //Desc: Updates gun position, anim, bullet & shooting logic, etc.
        public override void UpdateGun(Rectangle playerRec, GameTime gameTime, int dir, int gunState)
        {
            //Updating the gun coordinates according to the player direction (parameter)
            if (dir == RIGHT)
            {
                //Updating gun recs with info from player rec
                gunAnims[SAWED_OFF + SHOOTING].destRec.X = (int)(playerRec.X + gunImgs[selectedGun].Width / 3);
                gunAnims[SAWED_OFF + IDLE].destRec.X = gunAnims[SAWED_OFF + SHOOTING].destRec.X;
            }
            else
            {
                //Updating gun recs with info from player rec
                gunAnims[SAWED_OFF + SHOOTING].destRec.X = (int)(playerRec.X - gunImgs[selectedGun].Width * 1.125);
                gunAnims[SAWED_OFF + IDLE].destRec.X = gunAnims[SAWED_OFF + SHOOTING].destRec.X + (int)(gunImgs[selectedGun].Width / 2);
            }

            //Updating gun recs and gun locs with info from player rec
            gunAnims[SAWED_OFF + SHOOTING].destRec.Y = (int)playerRec.Y + (int)(gunImgs[IDLE].Height / 1.25);
            gunAnims[SAWED_OFF + IDLE].destRec.Y = gunAnims[SAWED_OFF + SHOOTING].destRec.Y + (int)(gunImgs[IDLE].Height / 4);
            gunLoc.X = gunAnims[SAWED_OFF + SHOOTING].destRec.X;
            gunLoc.Y = gunAnims[SAWED_OFF + SHOOTING].destRec.Y;

            //Handling the gun logic based on the gun state, current mag, and the reload timer
            if (gunState == SHOOTING && magSize > mag && (shootingTimer.IsFinished() || shootingTimer.IsInactive()))
            {
                //Depending on what direction the player is facing, add two bullet and add their origin direction accordingly
                if (dir == RIGHT)
                {
                    bullets.Add(new Bullet(bulletImg, new Vector2(gunLoc.X + (int)(gunImgs[IDLE].Width / 1.5), (int)(gunLoc.Y / 1.025)), RIGHT, gd));
                    bullets.Add(new Bullet(bulletImg, new Vector2(gunLoc.X + (int)(gunImgs[IDLE].Width / 1.5), (int)(gunLoc.Y * 1.025)), RIGHT, gd));
                }
                else
                {
                    bullets.Add(new Bullet(bulletImg, new Vector2(gunLoc.X, (int)(gunLoc.Y / 1.025)), LEFT, gd));
                    bullets.Add(new Bullet(bulletImg, new Vector2(gunLoc.X, (int)(gunLoc.Y * 1.025)), LEFT, gd));
                }

                //Adding +1 to the current mag
                mag++;

                //Resetting the reload timer if the current mag is over the mag size
                if (mag >= magSize)
                {
                    reloadTimer.ResetTimer(true);
                }

                //Access this statement if the gunholder is a player
                if (gunHolder == PLAYER)
                {
                    //Playing shooting sfx
                    Game1.sfxManager.PlayShooting();
                }

                //Resetting the shooting timer
                shootingTimer.ResetTimer(true);
            }
            else if (mag >= magSize && (reloadTimer.IsInactive() || reloadTimer.IsFinished()))
            {
                //Emptying mag
                mag = 0;
            }

            //Updating the bullets 
            UpdateBullets(players);

            //Updating the timers
            gunAnims[SAWED_OFF + SHOOTING].Update(gameTime);
            reloadTimer.Update(gameTime.ElapsedGameTime.Milliseconds);
            shootingTimer.Update(gameTime.ElapsedGameTime.Milliseconds);
        }
    }
}
