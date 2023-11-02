//Author: Dan Lichtin
//File Name: Goop.cs
//Project Name: PASS3
//Creation Date: December 2, 2022
//Modified Date: January 22, 2023
//Description: A child class of the parent enemy class; handles the logic and data of the Goop enemy
using Animation2D;
using Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
//PROOF OF CONCEPT:
//OOP - Goop is the chid class of the enemy object;
//It holds the functions and data of the enemy parent class but carries its own movement speed, animations, and timers
//The goop enemy has a unique attack ability in so that it moves 3 times faster when it's attacking

namespace PASS3___Grade_12
{
    class Goop : Enemy
    {
        //Timers to track the cooldown of goop's attack
        private Timer attackCooldown = new Timer(4000, true);

        public Goop(Texture2D[] enemyImgs, GraphicsDevice gd) : base(enemyImgs, gd)
        {
            //Defining anims
            enemyAnims[GOOP + RUN] = new Animation(enemyImgs[GOOP + RUN], 6, 1, 6, 0, 0,
                Animation.ANIMATE_FOREVER, 5, new Vector2(randomXVal, 0), 2f, true);
            enemyAnims[GOOP + DEATH] = new Animation(enemyImgs[GOOP + DEATH], 11, 1, 11, 0, 0,
                Animation.ANIMATE_ONCE, 5, new Vector2(randomXVal, 0), 2f, true);
            enemyAnims[GOOP + ATTACK] = new Animation(enemyImgs[GOOP + ATTACK], 6, 1, 6, 0, 0,
                Animation.ANIMATE_FOREVER, 7, new Vector2(randomXVal, 0), 2f, true);

            //Setting attributes
            rightSpeed = 2;
            leftSpeed = -2;
            enemyType = GOOP;
            enemyState = GOOP + RUN;
            enemyVisibleRec = new GameRectangle(gd, enemyAnims[enemyState].destRec);
        }

        //Pre: GameTime, 2D array of the tileset rectangles, List of the player object
        //Post: None
        //Desc: Updates and handles all enemy logic
        public override void EnemyLogic(GameTime gameTime, Rectangle[,] tileRecs, List<Player> players)
        {
            //Depending on the state of the enemy, change speed accordingly, and handle attack logic
            switch (enemyState)
            {
                case GOOP + RUN:
                    enemySpeed.X = currentSpeed;

                    //Access this statement if goop's attack cooldown is done
                    if (attackCooldown.IsFinished())
                    {
                        //Changing enemy state to attack and resetting attack cooldown
                        enemyState = GOOP + ATTACK;
                        attackCooldown.ResetTimer(true);
                    }
                    break;
                case GOOP + ATTACK:
                    enemySpeed.X = currentSpeed * 3;

                    //Access this statement if goop's attack cooldown is done
                    if (attackCooldown.IsFinished())
                    {
                        //Changing enemy state to runninng and resetting attack cooldown
                        enemyState = GOOP + RUN;
                        attackCooldown.ResetTimer(true);
                    }
                    break;
                case GOOP + DEATH:
                    enemySpeed.X = 0;
                    break;
            }
            
            //Updating timer
            attackCooldown.Update(gameTime.ElapsedGameTime.Milliseconds);

            //Updating the generic enemy data (recs, speed, etc.)
            UpdateEnemy(gameTime, dir, tileRecs);
        }
    }
}
