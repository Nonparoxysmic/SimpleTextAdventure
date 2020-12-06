using System;
using System.Collections.Generic;

namespace SimpleTextAdventure
{
    class GameLoop
    {
        readonly Player player;
        int commandNumber;
        public List<Item> inactiveItems = new List<Item>();
        readonly string[] introText;

        public GameLoop(Player player, string[] introText)
        {
            this.player = player;
            this.introText = introText;
        }

        public void PlayGame()
        {
            foreach (string paragraph in introText)
            {
                Program.PrintWrappedText(paragraph);
            }
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
                        PrintGameHelp(parameters);
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
                        if (player.currentZone.codeName == "underwater")
                        {
                            Program.PrintWrappedText("You drown.");
                            gameOver = true;
                        }
                        else
                        {
                            Program.PrintWrappedText("You wait. Nothing interesting happens.");
                        }
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

                // Check for game end conditions here
                
            }

            Console.Write(Environment.NewLine + "GAME OVER" + Environment.NewLine + "Press any key to exit");
            Console.ReadKey(true);
        }
        
        void PrintGameHelp(Parameter[] parameters)
        {
            if (parameters.Length == 1 && parameters[0].type == ParameterType.String && parameters[0].stringParameter == "BLANK")
            {
                Program.PrintWrappedText("Type \"help\" for a list of commands.");
                return;
            }
            Program.PrintWrappedText("List of Commands:");
            Program.PrintWrappedText("- Menu Commands: quit, help, version");
            Program.PrintWrappedText("- Basic Commands: look, move, examine, wait");
            Program.PrintWrappedText("- Item Commands: inventory, take, drop, use, combine");
            Program.PrintWrappedText("Variations:");
            Program.PrintWrappedText("- look, look <direction>, l <direction>");
            Program.PrintWrappedText("- move <direction>, go <direction>, <direction>");
            Program.PrintWrappedText("- examine, x, examine <target>, x <target>");
            Program.PrintWrappedText("- inventory, i");
            Program.PrintWrappedText("- take <item>, take all");
            Program.PrintWrappedText("- drop <item>, drop all");
            Program.PrintWrappedText("- use <item>");
            Program.PrintWrappedText("- combine <item> <item>");
            Program.PrintWrappedText("Directions:");
            Program.PrintWrappedText("- north, n, east, e, south, s, west, w, up, u, down, d, in, out");
        }

        void PrintGameVersion()
        {
            Program.PrintWrappedText(Program.gameName + " by " + Program.gameAuthor);
            Program.PrintWrappedText("Version: " + Program.gameVersion);
        }
    }
}
