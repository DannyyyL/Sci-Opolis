//Author: Dan Lichtin
//File Name: AK47.cs
//Project Name: PASS3
//Creation Date: December 16, 2022
//Modified Date: January 22, 2023
//Description: The child class of the gun parent class;
//AK47; Single bullet, starter weapon
using Animation2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
//PROOF OF CONCEPT:
//OOP - Gun is the parent class and the ak is the child; ak47 carries it's own magsize and shooting style
//Lists - Gun carries a list of bullets

namespace PASS3___Grade_12
{
    class AK47 : Gun
    {        
        public AK47(GraphicsDevice gd, Texture2D[] gunImgs, Texture2D bulletImg,
            Texture2D reloadIcon, Rectangle reloadIconRec, List<Enemy> enemies, List<Player> players, int gunHolder, bool upgrade)
            : base(gd, gunImgs, bulletImg, reloadIcon, reloadIconRec, enemies, players, gunHolder, upgrade)
        {
            //Defining anims
            gunAnims[AK47 + IDLE] = new Animation(gunImgs[AK47 + IDLE], 1, 1, 1, 0, 0, Animation.ANIMATE_FOREVER, 1, gunLoc, 0.8f, true);
            gunAnims[AK47 + SHOOTING] = new Animation(gunImgs[AK47 + SHOOTING], 24, 1, 24, 0, 0, Animation.ANIMATE_FOREVER, 1, gunLoc, 0.8f, true);

            //Setting attributes
            gunType = "Assault Rifle";
            magSize = 10;
            selectedGun = AK47;
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
                gunAnims[AK47 + SHOOTING].destRec.X = (int)playerRec.X + gunImgs[IDLE].Width / 4;
                gunAnims[AK47 + IDLE].destRec.X = gunAnims[SHOOTING].destRec.X;
            }
            else
            {
                //Updating gun recs with info from player rec
                gunAnims[AK47 + SHOOTING].destRec.X = (int)playerRec.X - (int)(gunImgs[IDLE].Width/1.175);
                gunAnims[AK47 + IDLE].destRec.X = gunAnims[SHOOTING].destRec.X + (int)(gunImgs[IDLE].Width / 3.5);
            }

            //Updating gun recs and gun locs with info from player rec
            gunAnims[AK47 + SHOOTING].destRec.Y = (int)playerRec.Y + (int)(gunImgs[IDLE].Height/1.25);
            gunAnims[AK47 + IDLE].destRec.Y = gunAnims[SHOOTING].destRec.Y + (int)(gunImgs[IDLE].Height / 4);
            gunLoc.X = gunAnims[AK47 + SHOOTING].destRec.X;
            gunLoc.Y = gunAnims[AK47 + SHOOTING].destRec.Y;

            //Handling the gun logic based on the gun state, current mag, and the reload timer
            if (gunState == SHOOTING && magSize > mag && (shootingTimer.IsFinished() || shootingTimer.IsInactive()))
            {
                //Depending on what direction the player is facing, add a bullet and add it's origin direction accordingly
                if (dir == RIGHT)
                {
                    bullets.Add(new Bullet(bulletImg, new Vector2(gunLoc.X + (int)(gunImgs[IDLE].Width/1.5), gunLoc.Y), RIGHT, gd));
                }
                else 
                {
                    bullets.Add(new Bullet(bulletImg, gunLoc, LEFT, gd));
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
            gunAnims[AK47 + SHOOTING].Update(gameTime);
            reloadTimer.Update(gameTime.ElapsedGameTime.Milliseconds);
            shootingTimer.Update(gameTime.ElapsedGameTime.Milliseconds);
        }

        //Pre: None
        //Post: Gun object
        //Desc: Creates a new gun based off the current gun info and clones it
        public override Gun Clone()
        {
            AK47 clonedAk = new AK47(gd, gunImgs, bulletImg, reloadIcon, reloadIconRec, enemies, players, gunHolder, false);

            //Returning the cloned gun
            return clonedAk;
        }
    }
}
