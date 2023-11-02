//Author: Dan Lichtin
//File Name: Gun.cs
//Project Name: PASS3
//Creation Date: December 16, 2022
//Modified Date: January 22, 2023
//Description: The gun parent class for all guns in the game
//Holds the gun type, dmg, range, and mag size of a gun
using Animation2D;
using Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
//PROOF OF CONCEPT:
//OOP - Gun is the parent class of all guns; it holds functions that the child guns can call, and the general gun data that the gun carries
//Lists - Gun carries a list of bullets

namespace PASS3___Grade_12
{
    class Gun
    {
        //Gun data
        protected string gunType;
        protected int selectedGun;
        protected Animation[] gunAnims = new Animation[AK47 + SHOOTING + SAWED_OFF + SHOOTING];
        protected Texture2D[] gunImgs;
        protected Vector2 gunLoc;
        protected Texture2D reloadIcon;
        protected Rectangle reloadIconRec;
        protected int gunHolder;

        //Bullet data
        protected List<Bullet> bullets = new List<Bullet>();
        protected Texture2D bulletImg;
        protected bool bulletCollided = false;

        //Timers
        protected Timer reloadTimer;
        protected Timer shootingTimer;
        protected int shootingTime = 450;
        protected int reloadTime = 2500;
        protected int upgradedShootingTime = 275;
        protected int upgradedReloadTime = 1500;
        protected int enemyShootingTime = 1300;
        protected int enemyReloadTime = 2800;

        //Gun stats
        protected int magSize;
        protected int mag = 0;
        protected bool upgraded;

        //Gun & Dir states
        protected const int IDLE = 0;
        protected const int SHOOTING = 1;
        protected const int AK47 = 0;
        protected const int SAWED_OFF = 2;
        protected const int LEFT = 0;
        protected const int RIGHT = 1;
        protected int PLAYER = 0;
        protected int ENEMY = 1;

        //Graphics device
        protected GraphicsDevice gd;
        
        //Enemy data
        protected List<Enemy> enemies;
        protected List<Player> players;

        public Gun(GraphicsDevice gd, Texture2D[] gunImgs, Texture2D bulletImg,
            Texture2D reloadIcon, Rectangle reloadIconRec, List<Enemy> enemies, List<Player> players, int gunHolder, bool upgraded)
        {
            this.gd = gd; 
            this.bulletImg = bulletImg;
            this.gunImgs = gunImgs;
            this.reloadIcon = reloadIcon;
            this.reloadIconRec = reloadIconRec;
            this.enemies = enemies;
            this.players = players;
            this.gunHolder = gunHolder;
            this.upgraded = upgraded;

            //Depending on wether the gun is upgraded or wether it is being held by an enemey; change the gun's reload & shooting speed
            if (gunHolder == ENEMY)
            {
                shootingTimer = new Timer(enemyShootingTime, false);
                reloadTimer = new Timer(enemyReloadTime, false);
            }
            else if (upgraded)
            {
                shootingTimer = new Timer(upgradedShootingTime, false);
                reloadTimer = new Timer(upgradedReloadTime, false);
            }
            else
            {
                shootingTimer = new Timer(shootingTime, false);
                reloadTimer = new Timer(reloadTime, false);
            }
        }

        //Pre: None
        //Post: String
        //Desc: Returns a string of the gun type (ex. "Assault Rifle")
        public virtual string GetGunType()
        {
            return gunType;
        }

        //Pre: The rectangle of the player, gameTime, the direction of the gun, and a gunstate (int)
        //Post: None
        //Desc: Updates gun position, anim, bullet & shooting logic, etc.
        public virtual void UpdateGun(Rectangle playerRec, GameTime gameTime, int dir, int gunState)
        {
        }

        //Pre: Sprite Batch required to draw, the flip of the gun, and it's gunstate
        //Post: None
        //Desc: Draws the gun, it's collision rectanlges (if needed), and it's UI (reload icon)
        public virtual void DrawGun(SpriteBatch spriteBatch, SpriteEffects flipType, int gunState)
        {
            //Drawing the gun
            gunAnims[gunState + selectedGun].Draw(spriteBatch, Color.White, flipType);

            //Loops through the bullet list
            for (int i = 0; i < bullets.Count; i++)
            {
                //Drawing the bullet
                bullets[i].DrawBullet(spriteBatch);
            }

            //Access this statement if the player is holding the gun, and if the mag is over the magsize
            if (gunHolder == PLAYER && mag >= magSize)
            {
                //Drawing the reload icon and playing the reload sfx
                spriteBatch.Draw(reloadIcon, reloadIconRec, Color.White);
                Game1.sfxManager.PlayReload();
            }
        }

        //Pre: List of enemies
        //Post: None
        //Desc: Setting the enemies to the gun
        public virtual void SetEnemies(List<Enemy> enemies)
        {
            this.enemies = enemies;
        }

        //Pre: List of players
        //Post: None
        //Desc: Handles the movement/physics of all bullets, their collision logic, etc.
        protected void UpdateBullets(List<Player> players)
        {
            //Looping through the bullets list
            for (int i = 0; i < bullets.Count; i++)
            {
                //Updating the bullet
                bullets[i].UpdateBullet();

                //Access this statement if the enemies list isnt null and the gun holder is a player
                if (enemies != null && gunHolder == PLAYER)
                {
                    //Looping through the enemies list
                    for (int j = 0; j < enemies.Count; j++)
                    {
                        //If the bullet collides with the enemy, trigger the death of the enemy and set bullet collision to true
                        if (enemies[j].GetCollisionRec().Intersects(bullets[i].GetBulletRec()))
                        {
                            enemies[j].TriggerDeath(true);
                            bulletCollided = true;
                            break;
                        }
                    }
                }

                //Access this statement if the gun holder is a enemey
                if (gunHolder == ENEMY)
                {
                    //Looping through the player list
                    for (int j = 0; j < players.Count; j++)
                    {
                        //If the bullet collides with a player, deduct health from the player and set bullet collision to true
                        if (bullets[i].GetBulletRec().Intersects(players[j].GetCollisionRec()))
                        {
                            players[j].HealthDeduction();
                            bulletCollided = true;
                            break;
                        }
                    }
                }

                //Deactivating the bullet depending on if it's out of bounds or if it's animation is done
                if ((bullets[i].GetBulletX() > Game1.screenWidth) || (bullets[i].GetBulletY() > Game1.screenHeight)
                    || (bullets[i].GetBulletX() < 0) || (bullets[i].GetBulletY() < 0))
                {
                    //Removing the bullet
                    bullets.RemoveAt(i);
                    i--;
                }
                else if (bullets[i].IsBulletAnimated() == false)
                {
                    //Removing the bullet
                    bullets.RemoveAt(i);
                    i--;
                }
                else if (bulletCollided)
                {
                    //Removing the bullet
                    bullets.RemoveAt(i);
                    i--;
                    
                    //Resetting bullet collision bool
                    bulletCollided = false;
                }
            }
        }

        //Pre: None
        //Post: Gun object
        //Desc: Creates a new gun based off the current gun info and clones it
        public virtual Gun Clone()
        {
            return null;
        }

        //Pre: None
        //Post: None
        //Desc: Reset's a gun as if it's brand new; emptying the mag and resetting all timers
        public void ResetGun()
        {
            //Emptying mag
            mag = 0;

            //Depending on upgrade status; reset the gun to it's proper timers
            if (upgraded)
            {
                shootingTimer = new Timer(upgradedShootingTime, false);
                reloadTimer = new Timer(upgradedReloadTime, false);
            }
            else
            {
                shootingTimer = new Timer(shootingTime, false);
                reloadTimer = new Timer(reloadTime, false);       
            }
        }
    }
}
