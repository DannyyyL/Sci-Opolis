//Author: Dan Lichtin
//File Name: WaveQueue.cs
//Project Name: PASS3
//Creation Date: December 27, 2022
//Modified Date: January 22, 2023
//Description: Carries stacks of enemeies that will be dequeued periodically
using Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
//PROOF OF CONCEPT:
//Queue - WaveQueue is a queue held by the driver class, and which carries an array of stacks (that carry enemeies)
//WaveQueue dequeues it's stacks and pops them simultaneously

namespace PASS3___Grade_12
{
    class WaveQueue
    {
        //Number that determins how many stacks will be in one wave(A.K.A how much enemy drops will there be in a wave)
        private const int STACK_NUM = 2;
        
        //Drop tracker and timer
        private Timer dropTimer = new Timer(3000, true);
        private int dropNum = 0;

        //Enemy stacks
        private EnemyStack[] enemyStacks = new EnemyStack[STACK_NUM];

        public WaveQueue(Texture2D[] enemyImgs, GraphicsDevice gd, Gun gun, int waveNum)
        {
            //Looping through the amount of stacks
            for (int i = 0; i < STACK_NUM; i++)
            {
                //Creating the enemy stacks
                enemyStacks[i] = new EnemyStack(enemyImgs, gd, gun, waveNum);
            }
        }

        //Pre: GameTime
        //Post: List of the enemy object
        //Desc: Returns a list of enemeies
        public List<Enemy> UpdateWaveQueue(GameTime gameTime)
        {
            //List of the enemy object
            List<Enemy> enemies = new List<Enemy>();

            //Access this statement if the drop timer is finished or inactive
            if (dropTimer.IsInactive() || dropTimer.IsFinished())
            {
                //Access this statement if the drop number is below the number of stacks
                if (dropNum < GetStackNum())
                {
                    //Adding enemies to the enemies list && +1 to the number of drops
                    enemies.AddRange(Dequeue());
                    dropNum++;
                }

                //Resetting the drop timer
                dropTimer.ResetTimer(true);
            }

            //Updating the drop timer
            dropTimer.Update(gameTime.ElapsedGameTime.Milliseconds);

            //Returning the enemies list
            return enemies;
        }

        //Pre: None
        //Post: List of the enemy object
        //Desc: Returns a list of enemeies
        public List<Enemy> Dequeue()
        {
            //List of the enemy object
            List<Enemy> enemies = new List<Enemy>();

            //Looping through an enemey stack, popping each enemy, and adding them to the list
            for (int i = 0; i < enemyStacks[dropNum].GetEnemyNum(); i++)
            {
                enemies.Add(enemyStacks[dropNum].Pop());
            }

            //Returning the enemies list
            return enemies;
        }

        //Pre: None
        //Post: An int
        //Desc: Returns the amount of stacks a wave queue holds
        public int GetStackNum()
        {
            return STACK_NUM;
        }

        //Pre: None
        //Post: A bool
        //Desc: Returns wether or not the wave is finished
        public bool IsWaveFinished()
        {
            //Access this statement if the amount of drops is equal/greater then the number of stacks
            if (dropNum >= STACK_NUM)
            {
                //Returning that the wave is finished
                return true;
            }

            //Returning that the wave isn't finished
            return false;
        }
    }
}
