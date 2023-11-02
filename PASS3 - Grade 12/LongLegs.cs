//Author: Dan Lichtin
//File Name: LongLegs.cs
//Project Name: PASS3
//Creation Date: January 12, 2022
//Modified Date: January 22, 2023
//Description: A child class of the parent enemy class; handles the logic and data of the long legs enemy
using Animation2D;
using Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
//PROOF OF CONCEPT:
//OOP - Long legs is the chid class of the enemy object;
//It holds the functions and data of the enemy parent class but carries its own movement speed and animations
//The long legs enemy also carries another object, an ak47

namespace PASS3___Grade_12
{
    class LongLegs : Enemy
    {
        public LongLegs(Texture2D[] enemyImgs, GraphicsDevice gd, Gun gun) : base(enemyImgs, gd)
        {
            //Defining anims
            enemyAnims[LONG_LEGS + RUN] = new Animation(enemyImgs[LONG_LEGS + RUN], 6, 1, 6, 0, 0, 
                Animation.ANIMATE_FOREVER, 6, new Vector2(randomXVal, 0), 1.9f, true);
            enemyAnims[LONG_LEGS + DEATH] = new Animation(enemyImgs[LONG_LEGS + DEATH], 6, 1, 6, 0, 0, 
                Animation.ANIMATE_ONCE, 6, new Vector2(randomXVal, 0), 1.9f, true);

            //Creating a copy of a gun for this enemy
            this.enemyGun = gun.Clone();

            //Setting attributes
            rightSpeed = 1;
            leftSpeed = -1;
            enemyType = LONG_LEGS;
            enemyVisibleRec = new GameRectangle(gd, enemyAnims[enemyState].destRec);
        }

        //Pre: GameTime, 2D array of the tileset rectangles, List of the player object
        //Post: None
        //Desc: Updates and handles all enemy logic
        public override void EnemyLogic(GameTime gameTime, Rectangle[,] tileRecs, List<Player> players)
        {
            //Depending on the state of the enemy, change speed accordingly
            switch (enemyState)
            {
                case LONG_LEGS + DEATH:
                    enemyGun = null;
                    enemySpeed.X = 0;
                    break;
                default:
                    enemySpeed.X = currentSpeed;
                    break;
            }

            //Access this statement if long legs has a gun
            if (enemyGun != null)
            {
                //Updating the gun logic
                enemyGun.UpdateGun(enemyRec, gameTime, dir, SHOOTING);
            }

            //Updating the generic enemy data (recs, speed, etc.)
            UpdateEnemy(gameTime, dir, tileRecs);
        }
    }
}
