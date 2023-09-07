
using System;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using PROJcreate;

namespace PROJmain
{
    class main
    {
        //global variables:
        public static string serverName = System.Environment.MachineName;
        public static string connectionString = $"Server={serverName};Integrated Security=True;";

        //this is the user interface it will guide the user to all the commands to see all the functionallity of the system 
        static void Main(string[] args)
        {
            string DBNAME = "MYDB";
            //when setting up the user gives the DB name to allow multiple setups 

            Console.WriteLine("welcom ");
            while (true)
            {
                Console.WriteLine("Enter a commands: [start, load, stats, search, view, data mine,  exit ] ");
                string command = Console.ReadLine();
                if (!string.IsNullOrEmpty(command))
                {
                    switch (command)
                    {
                        //calls the setup function to create a new db with all tables in the name of the user's choosing 
                        case "start":
                            {
                                Console.WriteLine("enter database name:");
                                DBNAME = Console.ReadLine();
                                if (!string.IsNullOrEmpty(DBNAME))
                                {
                                    //will call the function only if the chosen name is approved 
                                    if (!DatabaseExists(DBNAME)) Setup(DBNAME);
                                    else Console.WriteLine("database exists, try again");
                                }
                                else Console.WriteLine("Invalid input. Please provide a non-empty database name.");

                                break;
                            }

                        //calls the load function to populate all the tables in the desired database from the given dorectory 
                        case "load":
                            {
                                Console.WriteLine("enter the database:");
                                string DBNAME = Console.ReadLine();
                                Console.WriteLine("enter directory path:");
                                string DirPath = Console.ReadLine();                               
                                if (!string.IsNullOrEmpty(DBNAME) && !string.IsNullOrEmpty(DirPath))
                                {
                                    //will call the function only if the chosen database exists to be used 
                                    if (DatabaseExists(DBNAME)) loadFiles(DBNAME, DirPath);
                                    else Console.WriteLine("database doesnt exists, please enter 'start'");
                                }
                                else Console.WriteLine("Invalid input. Please provide a non-empty directory or database name.");

                                break;
                            }

                        //calls the stats function, the user will be guided from the function 
                        case "stats":
                            {
                                stats();
                                break;
                            }

                        //this functionallity will give all option to serch items 
                        case "search":
                            {
                                Console.WriteLine("enter what: [file, word, group, expression ]");
                                string input = Console.ReadLine();
                                if (!string.IsNullOrEmpty(input))
                                {
                                    switch (input)
                                    {
                                        //first search for files , by the word value and by the metadata value
                                        case "file":
                                            {
                                                List<string> printlist = new List<string>();
                                                Console.WriteLine("Enter a methide: [word, metadata]");
                                                string minput = Console.ReadLine();
                                                if (!string.IsNullOrEmpty(minput))
                                                {
                                                    switch (minput)
                                                    {
                                                        case "word":
                                                            {
                                                                // Get user input 
                                                                Console.WriteLine("Enter a word:");
                                                                string word = Console.ReadLine();
                                                                if (!string.IsNullOrEmpty(word))
                                                                {
                                                                    //get the result list
                                                                    printlist = FindFileByWord(word);
                                                                    print(printlist);
                                                                }
                                                                else Console.WriteLine("Invalid input. Please provide a non-empty parameters ");
                                                                break;
                                                            }
                                                        case "metadata":
                                                            {
                                                                // Get user input 
                                                                Console.WriteLine("Enter a metadata type:");
                                                                string MTD = Console.ReadLine();
                                                                Console.WriteLine("Enter a word:");
                                                                string word = Console.ReadLine();
                                                                if (!string.IsNullOrEmpty(MTD) && !string.IsNullOrEmpty(word))
                                                                {
                                                                    //get the result list
                                                                    printlist = FindFileByMTD(MTD, word);
                                                                    print(printlist);
                                                                }
                                                                else Console.WriteLine("Invalid input. Please provide a non-empty parameters ");
                                                                break;
                                                            }
                                                        default:
                                                            {
                                                                // Code to execute when command doesn't match any case
                                                                Console.WriteLine("Command is not recognized.");
                                                                break;
                                                            }
                                                    }
                                                }
                                                else Console.WriteLine("Invalid input. Please provide a non-empty command ");
                                                break;
                                            }
                                        // second search for words by file, by two indexes
                                        case "word":
                                            {
                                                List<string> printlist = new List<string>();
                                                Console.WriteLine("Enter parameters: ");

                                                Console.WriteLine("Enter a file (or press Enter for null):");
                                                string inputString = Console.ReadLine();

                                                Console.WriteLine("Enter a paragraph number (or press Enter for -1):");
                                                int param1;
                                                if (!int.TryParse(Console.ReadLine(), out param1))
                                                {
                                                    param1 = -1;
                                                }

                                                Console.WriteLine("Enter a sentence number in paragraph (or press Enter for -1):");
                                                int param2;
                                                if (!int.TryParse(Console.ReadLine(), out param2))
                                                {
                                                    param2 = -1;
                                                }

                                                Console.WriteLine("Enter an sentence number in file (or press Enter for -1):");
                                                int param3;
                                                if (!int.TryParse(Console.ReadLine(), out param3))
                                                {
                                                    param3 = -1;
                                                }

                                                Console.WriteLine("Enter a character number in sentence (or press Enter for -1):");
                                                int param4;
                                                if (!int.TryParse(Console.ReadLine(), out param4))
                                                {
                                                    param4 = -1;
                                                }

                                                //the function will filter according to the parameters sent
                                                if (!string.IsNullOrEmpty(inputString))
                                                {
                                                    printlist = ListWords(inputString, param1, param2, param3, param4);
                                                    print(printlist);
                                                }
                                                else Console.WriteLine("Invalid input. Please provide a non-empty parameters ");

                                                break;
                                            }

                                        // third search by group , allows to create a new group or print an existing group's members
                                        case "group":
                                            {
                                                Console.WriteLine("enter command: [add, list ]");
                                                string ginput = Console.ReadLine();
                                                if (!string.IsNullOrEmpty(ginput))
                                                {
                                                    switch (ginput)
                                                    {
                                                        case "add":
                                                            {
                                                                // Get user input for a list of words as a single string
                                                                Console.WriteLine("Enter a list of words separated by spaces:");
                                                                string userInput = Console.ReadLine();

                                                                // Get user input for the group name
                                                                Console.WriteLine("Enter a group name:");
                                                                string groupName = Console.ReadLine();

                                                                if (!string.IsNullOrEmpty(userInput) && !string.IsNullOrEmpty(groupName))
                                                                {
                                                                    // Split the user input into individual words
                                                                    List<string> wordList = userInput.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                                                                    // Call the GroupCreate function with the word list and group name
                                                                    GroupCreate(wordList, groupName);
                                                                }
                                                                else Console.WriteLine("Invalid input. Please provide a non-empty parameters ");
                                                                break;
                                                            }
                                                        case "list":
                                                            {
                                                                List<string> printlist = new List<string>();
                                                                // Get user input for the group name
                                                                Console.WriteLine("Enter a group name:");
                                                                string groupName = Console.ReadLine();
                                                                if (!string.IsNullOrEmpty(groupName))
                                                                {
                                                                    //get the result list
                                                                    printlist = GroupSearch(groupName);
                                                                    print(printlist);
                                                                }
                                                                else Console.WriteLine("Invalid input. Please provide a non-empty parameters ");
                                                                break;
                                                            }
                                                        default:
                                                            {
                                                                // Code to execute when command doesn't match any case
                                                                Console.WriteLine("Command is not recognized.");
                                                                break;
                                                            }
                                                    }
                                                }
                                                else Console.WriteLine("Invalid input. Please provide a non-empty command ");

                                                break;
                                            }
                                        //fourth search by expression
                                        case "expression":
                                            {
                                                List<string> printlist = new List<string>();
                                                // Get user input for the group name
                                                Console.WriteLine("Enter an expression:");
                                                string exprs = Console.ReadLine();
                                                if (!string.IsNullOrEmpty(exprs))
                                                {
                                                    //get the result list
                                                    //the list here is the index list of the starting point of the expression
                                                    //this function will handle the creation of new expressions as well
                                                    printlist = SearchExpression(exprs);
                                                    print(printlist);
                                                }
                                                else Console.WriteLine("Invalid input. Please provide a non-empty parameters ");
                                                break;
                                            }
                                        default:
                                            {
                                                // Code to execute when command doesn't match any case
                                                Console.WriteLine("Command is not recognized.");
                                                break;
                                            }
                                    }
                                }
                                else Console.WriteLine("Invalid input. Please provide a non-empty command ");
                                break;
                            }

                        //this command will show a 3 sentence context of a chosen word 
                        case "view":
                            {
                                Console.WriteLine("Enter a word to view:");
                                string word = Console.ReadLine();
                                if (!string.IsNullOrEmpty(word))
                                {
                                    //the function will handle the specific location and printing
                                    view(word);
                                }
                                else Console.WriteLine("Invalid input. Please provide a non-empty parameters ");
                                break;
                            }
                        //apriory data mine on the tables , will calculate and print the results 
                        case "data mine":
                            {
                                ExecuteAprioriOnMetaData();
                                break;
                            }

                        default:
                            {
                                // Code to execute when command doesn't match any case
                                Console.WriteLine("Command is not recognized.");
                                break;
                            }

                        case "exit":
                            {
                                Console.WriteLine("Exiting the program.");
                                return; // Exit the program
                            }

                    }
                }
                else Console.WriteLine("Invalid input. Please provide a non-empty command ");
            }
        }

        //function to print result lists 
        static void print(List<string> printlist)
        {
            // Check if the returned list is not null and not empty
            if (printlist != null && printlist.Count > 0)
            {
                // print the populated printlist list
                foreach (string item in printlist)
                {
                    Console.WriteLine(item);
                }
            }
            else
            {
                Console.WriteLine("No matching data found.");
            }
        }


        //----------------------------------------------------------------------------------------------------/
    }
}