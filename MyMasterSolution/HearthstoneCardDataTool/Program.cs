using System;
using System.Collections.Generic;
using MainConsole.Classes;
using System.Linq;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace MainConsole
{
    class Program
    {
        private const string QUOTE = "\"";
        private const string ESCAPED_QUOTE = "\"\"";
        private static char[] CHARACTERS_THAT_MUST_BE_QUOTED = { ',', '"', '\n' };
        

        static void Main(string[] args)
        {
            DisplayConsoleUI();
        }

        private static void DisplayConsoleUI()
        {
            while (true)
            {
                Console.Clear();

                Console.WriteLine("Hearthstone Card Data Tool");
                Console.WriteLine("Main Menu");
                Console.WriteLine("");
                Console.Write("Download"); Console.CursorLeft = 13; Console.Write("[D]"); Console.CursorLeft = 20; Console.WriteLine("|    Download JSON card data from web api");
                Console.Write("Store"); Console.CursorLeft = 13; Console.Write("[S]"); Console.CursorLeft = 20; Console.WriteLine("|    Store the JSON data in memory");
                Console.WriteLine("");
                Console.Write("Quit"); Console.CursorLeft = 13; Console.Write("[Q]"); Console.CursorLeft = 20; Console.WriteLine("|    Quit the application");

                //Console.WriteLine("2 - Press 2 to download gold card images");
                //Console.WriteLine("3 - Press 3 to load json card data to database");
                //Console.WriteLine("4 - Press 4 to create csv file from json web api");
                //Console.WriteLine("Escape - Press escape to quit");
                //Console.WriteLine("");
                //Console.WriteLine("Enter a command");

                switch (Console.ReadLine()[0].ToString().ToUpper())
                {
                    case "D":
                        if(DialogBox("Download data?"))
                        {
                            DisplayDownloadUI();
                        }
                        break;
                    case "S":
                        if(WebServiceManager.JSON_Content == null)
                        {
                            Console.WriteLine("There is no JSON data preasent.");
                            if (DialogBox("would you like to download the JSON data?"))
                                DisplayDownloadUI();
                            else
                            {
                                Console.WriteLine("Cannot store null data");
                                Console.WriteLine("Press any Key to continue");
                                Console.ReadKey();
                            }
                        }
                        else
                        {
                            CreateCSVFileFromJsonWebApi();
                        }
                        break;
                    case "Q":
                        if(DialogBox("Are you sure you want to quit?"))
                        {
                            Environment.Exit(0);
                        }
                        break;
                    default:
                        Console.WriteLine("Unrecognised command please try again");
                        Console.ReadKey();
                        break;
                }

            }

            //    case '2':
            //        DownloadAllGoldCardImages();
            //        break;
            //    case '3':
            //        LoadCardDataToDatabase();
            //        break;
            //    case '4':
            //        CreateCSVFileFromJsonWebApi();
            //        break;
            //}
        }

        private static void DisplayDownloadUI()
        {

            while (true)
            {
                Console.Clear();

                Console.WriteLine("Hearthstone Card Data Tool");
                Console.WriteLine("Download Menu");
                Console.WriteLine("");
                Console.Write("All"); Console.CursorLeft = 13; Console.Write("[A]"); Console.CursorLeft = 20; Console.WriteLine("|    Download json card data for ALL cards from web api");
                Console.Write("Collectable"); Console.CursorLeft = 13; Console.Write("[C]"); Console.CursorLeft = 20; Console.WriteLine("|    Download json card data for COLLECTABLE cards from web api");
                Console.WriteLine("");
                Console.Write("Main Menu"); Console.CursorLeft = 13; Console.Write("[M]"); Console.CursorLeft = 20; Console.WriteLine("|    Return to the Main Menu");


                Thread thread = new Thread(GetCards);
                switch (Console.ReadLine()[0].ToString().ToUpper())
                {

                    case "A":
                        Console.WriteLine("Beginning download of ALL cards");
                        thread.Start(0);

                        while (thread.ThreadState == System.Threading.ThreadState.Running)
                        {
                            Console.Write(".");
                            Thread.Sleep(500);
                            Console.Write(".");
                            Thread.Sleep(500);
                            Console.Write(".");
                            Thread.Sleep(500);
                            Console.CursorLeft = 0;
                            Console.Write("   ");
                            Console.CursorLeft = 0;
                            Thread.Sleep(500);
                        }
                        thread.Join();
                        thread = null;
                        if (WebServiceManager.JSON_Content != null)
                        {
                            Console.WriteLine("");
                            Console.WriteLine("Successfully downloaded ALL cards!");
                            if (DialogBox("Return to Main Menu?"))
                                return;
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Something went wrong please try again.");
                            Console.WriteLine("Press Any key to continue");
                            Console.ReadKey();
                            break;
                        }

                    case "C":
                        Console.WriteLine("Beginning download of COLLECTABLE cards");
                        thread.Start(1);

                        while (thread.ThreadState == System.Threading.ThreadState.Running)
                        {
                            Console.Write(".");
                            Thread.Sleep(500);
                            Console.Write(".");
                            Thread.Sleep(500);
                            Console.Write(".");
                            Thread.Sleep(500);
                            Console.CursorLeft = 0;
                            Console.Write("   ");
                            Console.CursorLeft = 0;
                            Thread.Sleep(500);
                        }
                        thread.Join();
                        thread = null;
                        if (WebServiceManager.JSON_Content != null)
                        {
                            Console.WriteLine("");
                            Console.WriteLine("Successfully downloaded COLLECTABLE cards!");
                            if (DialogBox("Return to Main Menu?"))
                                return;
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Something went wrong please try again.");
                            Console.WriteLine("Press Any key to continue");
                            Console.ReadKey();
                            break;
                        }
                    case "S":
                        CreateCSVFileFromJsonWebApi();
                        break;
                    case "M":
                        return;

                    default:
                        Console.WriteLine("Unrecognised command please try again");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private static bool DialogBox(string question)
        {
            Console.WriteLine(question);
            Console.WriteLine("[Y/N]");
            Console.WriteLine("");
            while(true)
            {
                string input = Console.ReadLine().ToString().ToUpper();

                if (input == "Y")
                {
                    return true;
                }
                else if (input == "N")
                {
                    return false;
                }
                else
                {
                    Console.CursorTop --;
                    Console.WriteLine("          ");
                    Console.CursorTop-=2;
                    Console.WriteLine("There was an error please try again");
                }
            }
        }

       
        public static void GetCards(object par)
        {
            WebServiceManager.GetAllCards((int)par);
        }

        private static void DownloadAllRegularCardImages()
        {
            Console.Clear();
            Console.WriteLine("\r\n");
            if (WebServiceManager.JSON_Content == null)
            {
                Console.WriteLine("The json card data has not been downloaded, you must download the data first.");
                DisplayDownloadUI();
            }

            int count = 0;

            Uri regularImagesPath = new Uri(WebServiceManager.RegularImagesFilePath);
            DirectoryInfo di = Directory.CreateDirectory(regularImagesPath.LocalPath);
            Console.WriteLine("\r\n");
            Console.WriteLine("Regular Card Images will be downloaded to: " + regularImagesPath.LocalPath);
            Console.WriteLine("\r\n");
            Console.WriteLine("Do you want to change the path for downloaded Regular Card Images?");
            Console.WriteLine("Enter Y for Yes or N for No");

            switch (Console.ReadKey().KeyChar.ToString().ToUpper())
            {
                case "Y":
                    Console.WriteLine("\r\n");
                    Console.WriteLine("Enter new path for downloaded Regular Card Images:");
                    WebServiceManager.RegularImagesFilePath = @"file:///" + Console.ReadLine();
                    Console.WriteLine("\r\n");
                    Console.WriteLine("Regular Card Images will be downloaded to: " + WebServiceManager.RegularImagesFilePath);
                    break;
                case "N":
                    Console.WriteLine("\r\n");
                    Console.WriteLine("Regular Card Images will be downloaded to: " + WebServiceManager.RegularImagesFilePath);
                    break;
                default:
                    break;
            }

            Console.WriteLine("\r\n");
            Console.WriteLine("DOWNLOADING regular image files...");

            count = WebServiceManager.DownloadRegularImageFiles();

            Console.WriteLine("\r\n");
            Console.WriteLine("DOWNLOADED" + count + " regular image files.");

            Console.WriteLine("\r\n");
            Console.WriteLine("DONE downloading regular image files.");

            Console.WriteLine("\r\n");
            Console.WriteLine("----------------------");

            DisplayConsoleUI();
        }

        private static void DownloadAllGoldCardImages()
        {
            Console.Clear();
            Console.WriteLine("\r\n");
            if (WebServiceManager.JSON_Content == null)
            {
                Console.WriteLine("The json card data has not been downloaded, you must download the data first.");
                DisplayDownloadUI();
            }

            int count = 0;

            Uri goldImagesPath = new Uri(WebServiceManager.GoldImagesFilePath);
            DirectoryInfo di = Directory.CreateDirectory(goldImagesPath.LocalPath);

            Console.WriteLine("\r\n");
            Console.WriteLine("Gold Card Images will be downloaded to: " + goldImagesPath.LocalPath);

            Console.WriteLine("\r\n");
            Console.WriteLine("Do you want to change the path for downloaded Gold Card Images?");
            Console.WriteLine("Enter Y for Yes or N for No");

            switch (Console.ReadKey().KeyChar.ToString().ToUpper())
            {
                case "Y":
                    Console.WriteLine("\r\n");
                    Console.WriteLine("Enter new path for downloaded Gold Card Images:");
                    WebServiceManager.GoldImagesFilePath = @"file:///" + Console.ReadLine();
                    Console.WriteLine("\r\n");
                    Console.WriteLine("Gold Card Images will be downloaded to: " + WebServiceManager.GoldImagesFilePath);
                    break;
                case "N":
                    Console.WriteLine("\r\n");
                    Console.WriteLine("Gold Card Images will be downloaded to: " + WebServiceManager.GoldImagesFilePath);
                    break;
                default:
                    break;
            }

            Console.WriteLine("\r\n");
            Console.WriteLine("DOWNLOADING gold image files...");

            count = WebServiceManager.DownloadGoldImageFiles();

            Console.WriteLine("\r\n");
            Console.WriteLine("DOWNLOADED" + count + " gold image files...");
            Console.WriteLine("\r\n");
            Console.WriteLine("DONE downloading gold image files.");

            Console.WriteLine("\r\n");
            Console.WriteLine("----------------------");

            DisplayConsoleUI();
        }


        private static void LoadCardDataToDatabase()
        {
            Console.Clear();
            Console.WriteLine("\r\n");
            if (WebServiceManager.JSON_Content == null)
            {
                Console.WriteLine("The json card data has not been downloaded, you must download the data first.");
                DisplayDownloadUI();
            }

            int count = 0;

            // entity framework will create the database if it doesn't exist
            Console.WriteLine("\r\n");

            if (!WebServiceManager.CheckIfDatabaseExists())
            {
                Console.WriteLine("Database does not exist.");
                Console.WriteLine("\r\n");
                Console.WriteLine("CREATING database...");
                Console.WriteLine("\r\n");
                Console.WriteLine("ADDING card data to database...");
            }
            else
            {
                Console.WriteLine("Database already exists.");
                Console.WriteLine("\r\n");
                Console.WriteLine("ADDING or MODIFYING card data in database...");
            }
            
            

            count = WebServiceManager.CreateLoadDatabase();

            Console.WriteLine("\r\n");
            Console.WriteLine("LOADED " + count + " card records to the database.");

            Console.WriteLine("\r\n");
            Console.WriteLine("----------------------");

            DisplayConsoleUI();
        }

        public static string Escape(string s)
        {
            if (!String.IsNullOrEmpty(s))
            {
                if (s.Contains(QUOTE))
                    s = s.Replace(QUOTE, ESCAPED_QUOTE);

                if (s.IndexOfAny(CHARACTERS_THAT_MUST_BE_QUOTED) > -1)
                    s = QUOTE + s + QUOTE;
            }
 
            return s;
        }



        private static void CreateCSVFileFromJsonWebApi()
        {
            Console.WriteLine("\r\n");
            if (WebServiceManager.JSON_Content == null)
            {
                Console.WriteLine("The json card data has not been downloaded, you must download the data first.");
                DisplayDownloadUI();
            }

            Uri uriCsvFilePath = new Uri(WebServiceManager.CSVFilePath);

            List<Card> listCards = WebServiceManager.ProvideCardList();

            StringBuilder builder = new StringBuilder();

            Console.WriteLine("\r\n");
            Console.WriteLine("The CSV will be created in: " + uriCsvFilePath.LocalPath);
            Console.WriteLine("\r\n");

            if(DialogBox("Do you want to change the path for the CSV file?"))
            {
                Console.WriteLine("\r\n");
                Console.WriteLine("Enter new path for the CSV file:");

                uriCsvFilePath = new Uri(@"file:///" + Console.ReadLine());
            }
            Directory.CreateDirectory(Path.GetDirectoryName(uriCsvFilePath.LocalPath));


            string filename = "";

            if (WebServiceManager.AllCards)
            {
                filename = "HS_Cards_All.csv"; 
            }
            else
            {
                filename = "HS_Cards.csv";
            }
            Console.WriteLine("CSV will be created as: " + uriCsvFilePath.LocalPath + filename);

            if (DialogBox("Continue?"))
            {
                using (FileStream fs = new FileStream(uriCsvFilePath.LocalPath + filename, FileMode.Create))
                using (TextWriter writer = new StreamWriter(fs))
                {
                    builder.Append("cardId,name,cardSet,type,rarity,text,playerClass,locale,mechanics,faction,health,collectible,img,imgSource,imgIcon,imgGold,attack,race,cost,flavor,artist,howToGet,howToGetGold,durability,elite");

                    Console.WriteLine("\r\n");
                    Console.WriteLine("WRITING header columns to CSV file...");
                    writer.WriteLine(builder);

                    Console.WriteLine("\r\n");
                    Console.WriteLine("WRITING rows to CSV file...");

                    foreach (var item in listCards)
                    {
                        builder.Clear();

                        builder.Append(Escape(item.cardId) + "," + Escape(item.name) + "," + Escape(item.cardSet) + "," + Escape(item.type) + "," + Escape(item.rarity) + "," + Escape(item.text) + "," + Escape(item.playerClass) + "," + Escape(item.locale) + ",");

                        if (item.mechanics != null)
                        {
                            for (int i = 0; i < item.mechanics.Count; i++)
                            {
                                builder.Append(Escape(item.mechanics[i].name));

                                if (i != item.mechanics.Count - 1)
                                {
                                    builder.Append("    ");
                                }
                            }
                        }
                        else
                        {
                            builder.Append(" ");
                        }

                        builder.Append("," + Escape(item.faction) + "," + Escape(item.health.ToString()) + "," + Escape(item.collectible.ToString()) + "," + Escape(item.img) + "," + Escape(item.imgSource) + "," + Escape(item.imgIcon) + "," + Escape(item.imgGold) + "," + Escape(item.attack.ToString()) + "," + Escape(item.race) + "," + Escape(item.cost.ToString()) + "," + Escape(item.flavor) + "," + Escape(item.artist) + "," + Escape(item.howToGet) + "," + Escape(item.howToGetGold) + "," + Escape(item.durability.ToString()) + "," + Escape(item.elite.ToString()));


                        writer.WriteLine(builder);
                    }

                    Console.WriteLine("\r\n");
                    Console.WriteLine("DONE writing CSV file.");
                }

                using (FileStream fs = new FileStream(uriCsvFilePath.LocalPath + "HS_Card_Mechanics.csv", FileMode.Create))
                using (TextWriter writer = new StreamWriter(fs))
                {
                    builder.Clear();
                    builder.Append("Mechanic_name,Card_cardId,cardMechanicId");

                    Console.WriteLine("\r\n");
                    Console.WriteLine("WRITING header columns to Mechanic CSV file...");
                    writer.WriteLine(builder);

                    Console.WriteLine("\r\n");
                    Console.WriteLine("WRITING rows to Mechanic CSV file...");

                    foreach (var item in listCards)
                    {
                        if (item.mechanics != null)
                        {
                            for (int i = 0; i < item.mechanics.Count; i++)
                            {

                                builder.Clear();
                                builder.Append(Escape(item.mechanics[i].name) + "," + Escape(item.cardId) + "," + Escape(item.mechanics[i].cardMechanicId.ToString()));

                                writer.WriteLine(builder);
                            }
                        }
                    }

                    Console.WriteLine("\r\n");
                    Console.WriteLine("DONE writing mechanic CSV file.");
                }

                Console.WriteLine("\r\n");
                Console.WriteLine("----------------------");
                Console.ReadKey();
            }
            DisplayConsoleUI();
        }
    }
}
