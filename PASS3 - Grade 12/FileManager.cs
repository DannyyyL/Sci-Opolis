//Author: Dan Lichtin
//File Name: FileManager.cs
//Project Name: PASS3
//Creation Date: December 5, 2022
//Modified Date: January 22, 2023
//Description: Handles reading and saving multiple components of the game
using System;
using System.IO;
//PROOF OF CONCEPT:
//File I/O && 2D array - Reads and Saves files; reads the stage layout (a 2d array), reads and saves statistics,
//and reads and saves player upgrades
namespace PASS3___Grade_12
{
    class FileManager
    {
        //Variables used for reading & writing into the file
        static StreamReader inFile;
        static StreamWriter outFile;

        //Pre: String of the filepath, an int of the maxlevelwidth, and an int of the maxlevelheight
        //Post: Returns a 2D array of the level's tileset
        //Desc: Reads in a file that layout's the tiles in the stage, stores the tiles in a 2D array,
        //and then returns the 2D array
        public int[,] ReadStageLayout(string filePath, int maxLevelWidth, int maxLevelHeight)
        {
            try
            {
                //Open the file
                inFile = File.OpenText(filePath);

                //Creating the 2D array & normal array given the row and col size's (parameters)
                int[,] tileSet = new int[maxLevelHeight, maxLevelWidth];
                string[] data = new string[maxLevelWidth];

                //Empty string
                string dataTrash;

                //Ticker that will be used to track the rows of the 2D array
                int row = 0;

                //Ignores first three lines of the note (used as a legend)
                dataTrash = inFile.ReadLine();
                dataTrash = inFile.ReadLine();
                dataTrash = inFile.ReadLine();

                //Loop through the entire file
                while (!inFile.EndOfStream)
                {
                    //Splitting the line 
                    data = inFile.ReadLine().Split(',');

                    //Loop through how many columns there at in the tileset
                    for (int col = 0; col < tileSet.GetLength(1); col++)
                    {
                        //Adding the tile value to the current row and column; converting the value from a string to an int
                        tileSet[row, col] = Convert.ToInt32(data[col]);
                    }

                    //+1 to the row ticker
                    row++;
                }

                //Returning the tileset
                return tileSet;
            }
            catch (Exception e)
            {
                //Error msg
                Console.WriteLine(e.Message);
            }
            finally
            {
                //Access this statement if the file isn't null; then close it
                if (inFile != null)
                {
                    inFile.Close();
                }
            }

            return null; 
        }

        //Pre: String of the filepath and an int of the number of data componenets there are 
        //Post: Returns a 2D array of the data set
        //Desc: Reads in a file that tracks the statistics and currency of the player
        public string[,] ReadStatsAndInventory(string filePath, int numOfData)
        {
            try
            {
                //Open the file
                inFile = File.OpenText(filePath);

                //Creating a 2D array & normal array given the row size (parameter)
                string[] data;
                string[,] userData = new string[numOfData, 2];

                //Ticker that will be used to track the rows of the 2D array
                int row = 0;

                //Loop through the entire file
                while (!inFile.EndOfStream)
                {
                    //Reading and splitting the data line
                    data = inFile.ReadLine().Split(',');

                    //First element of the line will describe the stat
                    //Second element will be the stat itself (a number)
                    userData[row,0] = data[0] + ": ";
                    userData[row,1] = data[1];

                    //+1 to the row ticker
                    row++;
                }

                //Returning the set of statistics
                return userData;
            }
            catch (Exception e)
            {
                //Error message
                Console.WriteLine(e.Message);
            }
            finally
            {
                //Access this statement if the file isn't null; then close it
                if (inFile != null)
                {
                    inFile.Close();
                }
            }

            return null;
        }

        //Pre: String of the filepath and the 2D array of the player's statistics
        //Post: None
        //Desc: Saves and format's the player's statistics to a file
        public void SaveStatsAndInventory(string filePath, string[,] playerData)
        {
            try
            {
                //Creating the file
                outFile = File.CreateText(filePath);

                //Loop through how many statistics the 2d array has
                for(int i = 0; i < playerData.GetLength(0); i++)
                {
                    //Removing the last 2 characters of the stat description 
                    //Last 2 characters are useless and added for display purposes (: )
                    //Writing out to the file the stat description and them the stat number itself
                    outFile.WriteLine(playerData[i, 0].Remove(playerData[i,0].Length-2) + "," + playerData[i, 1]);
                }
            }
            catch (Exception e)
            {
                //Error msg
                Console.WriteLine("ERROR: " + e.Message);
            }
            finally
            {
                //Access this statement if the file isn't null; then close it
                if (outFile != null)
                {
                    outFile.Close();
                }
            }
        }

        //Pre: String of the filepath and an int of the number of upgrades to be read
        //Post: Returns an array of bool's
        //Desc: Reads in a file that tracks the lock status of the user's upgrades
        public bool[] ReadUpgrades(string filePath, int numOfUpgrades)
        {
            try
            {
                //Open the file
                inFile = File.OpenText(filePath);

                //Will store the line info
                string[] data;

                //Creating an array given the amount of upgrades the user has (parameter)
                bool[] unlocked = new bool[numOfUpgrades];

                //Empty string
                string dataTrash;

                //Ignores first two lines of the note (which are used for readability purposes)
                dataTrash = inFile.ReadLine();
                dataTrash = inFile.ReadLine();

                //Reading and splitting the line 
                data = inFile.ReadLine().Split(',');

                //Looping through how many elements data has
                for (int i = 0; i < data.Length; i++)
                {
                    //Depending on the lock status (1 = locked, 0 = unlocked), lock/unlock the necessary upgrades
                    if (data[i] == "1")
                    {
                        unlocked[i] = false;
                    }
                    else if (data[i] == "0")
                    {
                        unlocked[i] = true;
                    }
                }

                //Returning the array of upgrade status's
                return unlocked;
            }
            catch (Exception e)
            {
                //Error msg
                Console.WriteLine(e.Message);
            }
            finally
            {
                //Access this statement if the file isn't null; then close it
                if (inFile != null)
                {
                    inFile.Close();
                }
            }

            return null;
        }

        //Pre: String of the filepath and an array of bools tracking the user's upgrades
        //Post: None
        //Desc: Saves and format's the user's upgrades unlock status's to a file
        public void SaveUpgrades(string filePath, bool[] unlocked)
        {
            try
            {
                //Creating the file
                outFile = File.CreateText(filePath);

                //Writing the legend of the upgrades
                outFile.WriteLine("0 = Unlocked, 1 = locked;");
                outFile.WriteLine("");
                
                //Loop through how many upgrade status's there are
                for (int i = 0; i < unlocked.Length; i++)
                {
                    //Depending on the current element, write the according number to it's lock status (1 = locked, 0 = unlocked)
                    if (!unlocked[i])
                    {
                        outFile.Write("1");
                    }
                    else
                    {
                        outFile.Write("0");
                    }

                    //Add the commas unless it's the last element
                    if (i != unlocked.Length-1)
                    {
                        outFile.Write(",");
                    }
                }
            }
            catch (Exception e)
            {
                //Error msg
                Console.WriteLine("ERROR: " + e.Message);
            }           
            finally
            {
                //Access this statement if the file isn't null; then close it
                if (outFile != null)
                {
                    outFile.Close();
                }
            }
        }
    }
}
