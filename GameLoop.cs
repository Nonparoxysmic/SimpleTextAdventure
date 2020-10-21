﻿using System;
using System.Collections.Generic;

namespace SimpleTextAdventure
{
    class GameLoop
    {
        readonly Player player;
        int commandNumber;
        public List<Item> inactiveItems = new List<Item>();

        public GameLoop(Player player)
        {
            this.player = player;
        }

        public void PlayGame()
        {
            Program.PrintWrappedText("TESTING MODE // Goal: take 9 items to the study.");
            Console.WriteLine();
            Program.PrintWrappedText("You are in " + player.currentZone.name + ".");
            player.currentZone.PrintExamineText(player.hasLightSource);
            
            bool gameOver = false;
            while (!gameOver)
            {
                commandNumber++;
                Console.Write(Environment.NewLine + "[" + commandNumber + "] > ");
                Parser.ParseUserInput(Console.ReadLine(), out Command command, out Parameter[] parameters);

                switch (command)
                {
                    case Command.GameQuit:
                        Environment.Exit(0);
                        break;
                    case Command.GameHelp:
                        PrintGameHelp();
                        break;
                    case Command.GameVersion:
                        PrintGameVersion();
                        break;
                    case Command.Look:
                        player.LookAction(Parser.ParseDirectionParameter(parameters));
                        break;
                    case Command.Move:
                        player.MoveAction(Parser.ParseDirectionParameter(parameters));
                        break;
                    case Command.Examine:
                        player.ExamineAction(parameters);
                        break;
                    case Command.Wait:
                        Program.PrintWrappedText("You wait. Nothing interesting happens.");
                        break;
                    case Command.Inventory:
                        player.PrintInventory();
                        break;
                    case Command.Take:
                        player.TakeAction(parameters);
                        break;
                    case Command.Drop:
                        player.DropAction(parameters);
                        break;
                    case Command.Use:
                        player.UseAction(parameters);
                        break;
                    case Command.Combine:
                        player.CombineAction(parameters);
                        break;
                    default:
                        Program.PrintWrappedText("Unrecognized command. Type \"help\" for a list of commands.");
                        break;
                }

                // Example game over condition:
                if (player.currentZone.codeName == "study" && (player.inventory.Count + player.currentZone.items.Count) >= 9)
                {
                    gameOver = true;
                    Console.WriteLine();
                    if (player.inventory.Count + player.currentZone.items.Count == 9)
                    {
                        Program.PrintWrappedText("TESTING MODE // You win! You have brought 9 items to the study.");
                    }
                    else
                    {
                        Program.PrintWrappedText("TESTING MODE // You win! You have brought more than 9 items to the study.");
                    }
                }
            }

            Console.WriteLine();
            Program.PrintWrappedText("GAME OVER");
            Console.Write("Press any key to exit");
            Console.ReadKey(true);
        }
        
        void PrintGameHelp()
        {
            Program.PrintWrappedText("List of Commands: quit, help, version, look, move / go, examine, wait, inventory, take, drop, use, combine.");
            Program.PrintWrappedText("Commands with Parameters: look <direction>, move <direction>, examine <target>, take <item>, drop <item>, use <item>, combine <item> <item>.");
            Program.PrintWrappedText("Quick Commands: l (look), x (examine), i (inventory), n (north / move north), etc.");
        }

        void PrintGameVersion()
        {
            Program.PrintWrappedText(Program.gameName + " by " + Program.gameAuthor);
            Program.PrintWrappedText("Version: " + Program.gameVersion);
        }
    }
}
