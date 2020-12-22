using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SimpleTextAdventure
{
    static class Program
    {
        public static string gameName = "SimpleTextAdventure";
        public static string gameAuthor = "Nonparoxysmic";
        public static string gameVersion = "Alpha 0.1_31";

        static readonly string wrappingIndent = "  ";
        static readonly bool indentFirstLine = false;

        static void Main()
        {
            Console.Title = gameName + " by " + gameAuthor;

            XElement worldData = XElement.Load("World.xml");

            List<XElement> zonesData = worldData.Elements().FirstOrDefault(x => x.Name == "Zones").Elements().ToList();
            List<Zone> zoneList = new List<Zone>();
            foreach (XElement zone in zonesData)
            {
                string nameData = zone.Attribute("Name").Value;
                if (nameData.IndexOf('#') < 0) Program.PrintErrorAndExit("XML: Error in Zone Name Data");
                string name = nameData.Substring(0, nameData.IndexOf('#')) + nameData.Substring(nameData.IndexOf('#') + 1);
                string codeName = "";
                for (int i = nameData.IndexOf('#') + 1; i < nameData.Length; i++)
                {
                    if (char.IsWhiteSpace(nameData[i])) break;
                    codeName += nameData[i];
                }
                if (!Boolean.TryParse(zone.Attribute("IsDark").Value, out bool isDark))
                {
                    PrintErrorAndExit("XML: Error in Zone Darkness Data");
                }
                string examineText = zone.Value;
                zoneList.Add(new Zone(codeName, name, isDark, examineText));
            }

            string startingZoneName = worldData.Element("StartingZone").Value;
            Zone startingZone = zoneList.FirstOrDefault(x => x.codeName.ToLower() == startingZoneName.ToLower());
            if (startingZone == null) Program.PrintErrorAndExit("XML: Error in Starting Zone Data");

            string[] introText = worldData.Element("Introduction").Value.Split('|');

            Player player = new Player("Tabula Rasa", startingZone);
            GameLoop gameLoop = new GameLoop(player, introText);

            List<XElement> itemData = worldData.Elements().FirstOrDefault(x => x.Name == "Items").Elements().ToList();
            List<Item> itemsForItemReferences = new List<Item>();
            foreach (XElement item in itemData)
            {
                string nameData = item.Attribute("Name").Value;
                if (nameData.IndexOf('#') < 0) Program.PrintErrorAndExit("XML: Error in Item Name Data");
                string name;
                if (nameData.IndexOf('|') < 0)
                {
                    name = nameData.Substring(0, nameData.IndexOf('#')) + nameData.Substring(nameData.IndexOf('#') + 1);
                }
                else
                {
                    name = nameData.Substring(0, nameData.IndexOf('#')) + nameData.Substring(nameData.IndexOf('#') + 1, nameData.IndexOf('|') - nameData.IndexOf('#') - 1);
                }
                
                string codeName = "";
                for (int i = nameData.IndexOf('#') + 1; i < nameData.Length; i++)
                {
                    if (char.IsWhiteSpace(nameData[i]) || nameData[i] == '|') break;
                    codeName += nameData[i];
                }
                string examineText = item.Value;
                string itemType = item.Attribute("Type").Value;

                Item newItem;
                if (itemType == "XtoY")
                {
                    newItem = new XtoY(codeName, name, examineText, gameLoop, item.Attribute("Y").Value, item.Attribute("UseMessage").Value);
                    itemsForItemReferences.Add(newItem);
                }
                else if (itemType == "XtoYZ")
                {
                    newItem = new XtoYZ(codeName, name, examineText, gameLoop, item.Attribute("Y").Value, item.Attribute("Z").Value, item.Attribute("UseMessage").Value);
                    itemsForItemReferences.Add(newItem);
                }
                else if (itemType == "XYtoZ")
                {
                    newItem = new XYtoZ(codeName, name, examineText, gameLoop, item.Attribute("Y").Value, item.Attribute("Z").Value, item.Attribute("CombineMessage").Value);
                    itemsForItemReferences.Add(newItem);
                }
                else if (itemType == "toX")
                {
                    newItem = new Item("toX", codeName, name, examineText);
                    itemsForItemReferences.Add(newItem);
                }
                else if (itemType == "Light")
                {
                    string activeName = nameData.Substring(nameData.IndexOf('|') + 1);
                    newItem = new Light(codeName, name, activeName, examineText, item.Attribute("ActivateMessage").Value, item.Attribute("DeactivateMessage").Value);
                }
                else if (itemType == "Key")
                {
                    newItem = new Item("Key", codeName, name, examineText);
                    itemsForItemReferences.Add(newItem);
                }
                else
                {
                    newItem = new Item("X", codeName, name, examineText);
                }
                
                if (item.Attribute("StartLocation").Value == "PLAYER")
                {
                    player.inventory.Add(newItem);
                }
                else if (item.Attribute("StartLocation").Value == "NONE")
                {
                    gameLoop.inactiveItems.Add(newItem);
                }
                else
                {
                    string zoneCodeName = item.Attribute("StartLocation").Value;
                    Zone itemZone = zoneList.FirstOrDefault(x => x.codeName.ToLower() == zoneCodeName.ToLower());
                    if (itemZone == null) Program.PrintErrorAndExit("XML: Error in Item Zone Data");
                    itemZone.items.Add(newItem);
                }
            }
            foreach (Item itemX in itemsForItemReferences)
            {
                if (itemX.type == "XtoY")
                {
                    Item itemY = itemsForItemReferences.FirstOrDefault(x => x.codeName == (itemX as XtoY).Y);
                    if (itemY == null) Program.PrintErrorAndExit("XML: Error in Item Connections");
                    (itemX as XtoY).itemY = itemY;
                }
                else if (itemX.type == "XtoYZ")
                {
                    Item itemY = itemsForItemReferences.FirstOrDefault(x => x.codeName == (itemX as XtoYZ).Y);
                    if (itemY == null) Program.PrintErrorAndExit("XML: Error in Item Connections");
                    (itemX as XtoYZ).itemY = itemY;
                    Item itemZ = itemsForItemReferences.FirstOrDefault(x => x.codeName == (itemX as XtoYZ).Z);
                    if (itemZ == null) Program.PrintErrorAndExit("XML: Error in Item Connections");
                    (itemX as XtoYZ).itemZ = itemZ;
                }
                else if (itemX.type == "XYtoZ")
                {
                    Item itemY = itemsForItemReferences.FirstOrDefault(x => x.codeName == (itemX as XYtoZ).Y);
                    if (itemY == null) Program.PrintErrorAndExit("XML: Error in Item Connections");
                    (itemX as XYtoZ).itemY = itemY;
                    Item itemZ = itemsForItemReferences.FirstOrDefault(x => x.codeName == (itemX as XYtoZ).Z);
                    if (itemZ == null) Program.PrintErrorAndExit("XML: Error in Item Connections");
                    (itemX as XYtoZ).itemZ = itemZ;
                }
            }

            List<XElement> connectionsData = worldData.Elements().FirstOrDefault(x => x.Name == "ZoneConnections").Elements().ToList();
            foreach (XElement connection in connectionsData)
            {
                string start = connection.Attribute("Start").Value;
                string directionName = connection.Attribute("Direction").Value;
                string end = connection.Attribute("End").Value;
                string keyItemName = "";
                Item key = new Item();
                if (connection.Attribute("Key") != null)
                {
                    keyItemName = connection.Attribute("Key").Value;
                    key = itemsForItemReferences.FirstOrDefault(x => x.codeName.ToLower() == keyItemName.ToLower());
                }
                bool isOneWay = connection.Attribute("OneWay") != null;

                Zone startZone = zoneList.FirstOrDefault(x => x.codeName.ToLower() == start.ToLower());
                Zone endZone = zoneList.FirstOrDefault(x => x.codeName.ToLower() == end.ToLower());
                Direction moveDirection = 0;
                if (startZone == null || endZone == null || !Parser.TryParseDirection(directionName, out moveDirection)) Program.PrintErrorAndExit("XML: Error in Zone Connection Data");

                if (keyItemName == "")
                {
                    if (isOneWay) startZone.AddExit(moveDirection, endZone);
                    else Zone.ConnectZones(startZone, moveDirection, endZone);
                }
                else
                {
                    if (isOneWay) startZone.AddExit(moveDirection, endZone, key);
                    else Zone.ConnectZones(startZone, moveDirection, endZone, key);
                }
            }

            gameLoop.PlayGame();
        }

        public static void PrintWrappedText(string text)
        {
            PrintWrappedText(text, Console.WindowWidth - 1, wrappingIndent, indentFirstLine);
        }

        public static void PrintWrappedText(string text, int width, string indent = "", bool doIndent = false)
        {
            string trimmedText = text.Trim();

            int indentLength = 0;
            if (doIndent)
            {
                trimmedText = indent + trimmedText;
                indentLength = indent.Length;
            }

            if (trimmedText.Length <= width)
            {
                Console.WriteLine(trimmedText);
                return;
            }

            int lineBreakWidth = width;
            for (int pos = width; pos > indentLength; pos--)
            {
                if (Char.IsWhiteSpace(trimmedText[pos]))
                {
                    lineBreakWidth = pos;
                    break;
                }
            }
            string firstLine = trimmedText.Substring(0, lineBreakWidth).TrimEnd();
            string remainder = trimmedText.Substring(lineBreakWidth);

            Console.WriteLine(firstLine);
            if (indent != "") doIndent = true;
            PrintWrappedText(remainder, width, indent, doIndent);
        }

        public static void PrintErrorAndExit(string message = "Unknown Error")
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Program.PrintWrappedText("ERROR: " + message);
            Program.PrintWrappedText("Press any key to exit.");
            Console.ReadKey(true);
            Environment.Exit(0);
        }
    }
}
