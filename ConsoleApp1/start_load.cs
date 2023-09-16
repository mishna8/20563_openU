using System;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Xml.Linq;
using System.Collections.Generic;
using PROJmain;
using System.Linq;

namespace PROJcreate
{
    class startLoad
    {

        //function that returnes true or false if database already exists in the sql instance
        static bool DatabaseExists(string dbName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if entry exists in the master table
                    // BY USING A COUNT QUERY AND SEE IF THE NUMBER IS 0 OR 1 
                    string CheckQuery = $"SELECT COUNT(*) FROM {"sys.databases"} WHERE {"name"} = @name";
                    SqlCommand CheckCommand = new SqlCommand(CheckQuery, connection);
                    CheckCommand.Parameters.AddWithValue("@name", dbName);
                    int Count = (int)CheckCommand.ExecuteScalar();
                    return Count != 0;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking database existence: " + ex.Message);
                return false;
            }
        }

        ///creates the databse for the first time with all tables
        static void Setup(string databaseName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    //the database itself creation 
                    string createDatabaseQuery = $"CREATE DATABASE {databaseName}";
                    SqlCommand createDatabaseCommand = new SqlCommand(createDatabaseQuery, connection);
                    createDatabaseCommand.ExecuteNonQuery();

                    Console.WriteLine("Database created successfully!");

                    connection.ChangeDatabase(databaseName);

                    ///1.1
                    //create the FILES table
                    //THE COLUMS:  FileName, FilePath, WordCount, LineCount, ParagCount
                    string createFilesTableQuery = $"CREATE TABLE {"Files"} ({"FileName"} NVARCHAR(MAX), {"FilePath"} NVARCHAR(MAX), {"WordCount"} INT, {"LineCount"} INT, {"ParagCount"} INT)";
                    SqlCommand createFilesTableCommand = new SqlCommand(createFilesTableQuery, connection);
                    createFilesTableCommand.ExecuteNonQuery();

                    Console.WriteLine("Files table created successfully!");

                    ///1.2
                    //create the METADATA id by file table
                    //THE COLUMS:  FileName, 
                    //Patient, Doctor, Diag, Treat, Summary as id numbers of the metadata found
                    string createMTDTableQuery = $"CREATE TABLE {"MetaData"} ({"FileName"} NVARCHAR(MAX), {"Patient"} NVARCHAR(MAX),{"Doctor"} NVARCHAR(MAX),{"Diag"} NVARCHAR(MAX),{"Treat"} NVARCHAR(MAX))";
                    SqlCommand createMTDTableCommand = new SqlCommand(createMTDTableQuery, connection);
                    createMTDTableCommand.ExecuteNonQuery();

                    Console.WriteLine("MetaData table created successfully!");

                    ///1.2.0
                    //create the matadata values table 
                    //columns: ID , value , type (Patient=1, Doctor=2, Diag=3, Treat=4)
                    string createMTDregTableQuery = $"CREATE TABLE {"MTDREG"} ({"ID"} INT, {"value"} NVARCHAR(MAX), {"type"} INT)";
                    SqlCommand createMTDregTableCommand = new SqlCommand(createMTDregTableQuery, connection);
                    createMTDregTableCommand.ExecuteNonQuery();

                    Console.WriteLine("MetaData table created successfully!");

                    ///1.3
                    //create the Content table
                    //THE COLUMS:  WordId,WordValue, File, charCount, paragNum , lineInParagNum, lineNum, charInLineNum, wordInLineNum, Exprs
                    string creatWORDTableQuery = $"CREATE TABLE {"Content"} ({"WordId"} INT, {"WordValue"} NVARCHAR(MAX), {"File"} NVARCHAR(MAX),{"charCount"} INT, {"paragNum"} INT, {"lineInParagNum"} INT,{"lineNum"} INT, {"charInLineNum"} INT, {"wordInLineNum"} INT,{"Exprs"} INT)";
                    SqlCommand createWORDTableCommand = new SqlCommand(creatWORDTableQuery, connection);
                    createWORDTableCommand.ExecuteNonQuery();

                    Console.WriteLine("Content table created successfully!");

                    ///2.1
                    //create the Tags table
                    //THE COLUMS:  Group, Word
                    string creatTAGTableQuery = $"CREATE TABLE {"Tags"} ({"Group"} NVARCHAR(MAX), {"Word"} NVARCHAR(MAX))";
                    SqlCommand createTAGTableCommand = new SqlCommand(creatTAGTableQuery, connection);
                    createTAGTableCommand.ExecuteNonQuery();

                    Console.WriteLine("Tags table created successfully!");

                    ///2.2
                    //create the Expression table
                    //THE COLUMS:  Sentence, ID
                    string creatExprsTableQuery = $"CREATE TABLE {""} ({"Sentence"} NVARCHAR(MAX), {"ID"} INT)";
                    SqlCommand createExprsTableCommand = new SqlCommand(creatExprsTableQuery, connection);
                    createExprsTableCommand.ExecuteNonQuery();

                    Console.WriteLine("Expression table created successfully!");

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");

                }
            }
        }

        

        ///_______________________________________________________________________________________/

        ///this function will receive the desired database and a files directory to load files from to the table 
        static void loadFiles(string databaseName, string directoryPath)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    connection.ChangeDatabase(databaseName);

                    //get a list of all txt files in the given directory
                    string[] fileEntries = Directory.GetFiles(directoryPath, "*.txt");

                    foreach (string filePath in fileEntries)
                    {
                        // Check if file entry exists in the files table
                        // BY USING A COUNT QUERY AND SEE IF THE NUMBER OF FILE ARE 0 OR 1 
                        string fileCheckQuery = $"SELECT COUNT(*) FROM {"Files"} WHERE {"FilePath"} = @Path";
                        SqlCommand fileCheckCommand = new SqlCommand(fileCheckQuery, connection);
                        fileCheckCommand.Parameters.AddWithValue("@Path", filePath);
                        int fileCount = (int)fileCheckCommand.ExecuteScalar();

                        /*if the file doesnt exists we must 
                        1. create a file properties entry in file table 
                        2. load all words in the content table
                        3. load all new metadata in the metadata registery table 
                        4. create metadata mapping entry for the file in the metadata table 
                         */
                        if (fileCount == 0)
                        {
                            // handle files properties instert
                            // start getting the properties 
                            //name
                            string fileName = Path.GetFileName(filePath);
                            // get file stats  
                            int paragraphCount, lineCount, wordCount;
                            CountFileStats(filePath, out paragraphCount, out lineCount, out wordCount);
                            // the query 
                            string insertFileQuery = $"INSERT INTO Files (FileName, FilePath, WordNum, LineNume, ParagNum )" +
                                                      $"VALUES (@fileName, @filePath, @paragraphCount, @lineCount, @wordCount)";
                            using (SqlCommand insertFileCommand = new SqlCommand(insertFileQuery, connection))
                            {
                                insertFileCommand.Parameters.AddWithValue("@Path", filePath);
                                insertFileCommand.Parameters.AddWithValue("@fileName", fileName);
                                insertFileCommand.Parameters.AddWithValue("@paragraphCount", paragraphCount);
                                insertFileCommand.Parameters.AddWithValue("@lineCount", lineCount);
                                insertFileCommand.Parameters.AddWithValue("@wordCount", wordCount);
                                insertFileCommand.ExecuteNonQuery();
                            }

                            //handle metadata mapping entry for the file 
                            string insertMTDQuery = $"INSERT INTO Files (FileName, Patient, Doctor ,Diag ,Treat )" +
                                                      $"VALUES (@fileName, 0,0,0,0)";
                            using (SqlCommand insertMTDCommand = new SqlCommand(insertMTDQuery, connection))
                            {
                                insertMTDCommand.Parameters.AddWithValue("@fileName", filePath);
                                insertMTDCommand.ExecuteNonQuery();
                            }
                            //handle the word and metadata inserts & mapping
                            wordInsert(filePath);                           
                        }
                    }

                    Console.WriteLine("Entries added successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
        }

        //reads the file to extract overall stats for the files table
        static void CountFileStats(string filePath, out int paragraphCount, out int lineCount, out int wordCount)
        {
            paragraphCount = lineCount = wordCount = 0;

            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    bool inParagraph = false;
                    bool inWord = false;

                    while (!reader.EndOfStream)
                    {
                        char c = (char)reader.Read();
                        //if we have a new line break
                        if (c == '\n' || c == '\r')
                        {
                            inWord = false;
                            inParagraph = true;
                            lineCount++; //count the new line 
                        }
                        else if (char.IsWhiteSpace(c))
                        {
                            inWord = false;
                        }
                        else
                        {
                            //we are between spaces
                            if (!inWord)
                            {
                                wordCount++;//count the new word
                                inWord = true;
                            }
                            //we are not a space but also not a charecter, meanung between line break
                            if (inParagraph)
                            {
                                paragraphCount++;//count the new paragraph 
                                inParagraph = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading the file: " + ex.Message);
            }
        }

        //the function is called to read the words of the file and populate the other tables with its content 
        public static void wordInsert(string filePath)
        {
            // Initialize counters
            int paragNum = 0;
            int lineNum = 0;
            int lineInParagNum = 0;
            int charInLineNum = 0;
            int wordInLineNum = 0;
            int charInWordNum = 0;

            // Regular expression pattern for word splitting
            Regex wordRegex = new Regex(@"\b\w+\b");

            // Open the file for reading
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Increment counters
                    //when we get to each new line in total file the line counter increase
                    lineNum++;
                    // when we get to each new line in the current paragraph the counter increase
                    lineInParagNum++;
                    //the word count in the new line will reset to start counting
                    wordInLineNum = 0;
                    //the char count in the new line will reset to start counting
                    charInLineNum = 0;

                    // Check for paragraph break (empty line)
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        //when we get to new paragraph in total file the paragraph counter increase
                        paragNum++;
                        //and the line count in the new paragraphe will reset to start counting
                        lineInParagNum = 0;                         
                        continue;
                    }

                    // Split the line into words
                    string[] words = wordRegex.Matches(line.ToLower());
                    foreach (string word in words)
                    {
                        //get the word's charecter number (length)
                        charInWordNum = word.Length;
                        //when we get to new word in the current line the counter increase
                        wordInLineNum++;

                        // Insert word data into the "content" table
                        InsertWordData(word, filePath, charInWordNum, paragNum, lineInParagNum, lineNum, charInLineNum, wordInLineNum);
                        
                        // Check for special words and call the corresponding function
                        switch (word)
                        {
                            case "patient":
                                MTDHandler(word, 1, filePath);
                                break;
                            case "doctor":
                                MTDHandler(word, 2, filePath);
                                break;
                            case "diagnosis":
                                MTDHandler(word, 3, filePath);
                                break;
                            case "treatment":
                                MTDHandler(word, 4, filePath);
                                break;
                        }

                        //after the new word the charecter in line count will prepare to the next word
                        //counter wil move up by the number of charecters in the word we past plus one for the space between them;
                        charInLineNum += charInWordNum + 1;
                    }
                }
            }
        }

        private static void InsertWordData(string word, string filePath, int charInWordNum, int paragNum, int lineInParagNum, int lineNum, int charInLineNum, int wordInLineNum)
        {
            // Create a SqlConnection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Open the connection
                connection.Open();

                // Define your SQL query to insert data into the "content" table
                string insertQuery = "INSERT INTO content (WordValue, File, charCount, paragNum, lineInParagNum, lineNum, charInLineNum, wordInLineNum, Exprs) " +
                    "VALUES (@WordValue, @File, @charCount, @paragNum, @lineInParagNum, @lineNum, @charInLineNum, @wordInLineNum, 0)";

                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    // Set parameter values
                    command.Parameters.AddWithValue("@WordValue", word);
                    command.Parameters.AddWithValue("@File", filePath);
                    command.Parameters.AddWithValue("@charCount", charInWordNum);
                    command.Parameters.AddWithValue("@paragNum", paragNum);
                    command.Parameters.AddWithValue("@lineInParagNum", lineInParagNum);
                    command.Parameters.AddWithValue("@lineNum", lineNum);
                    command.Parameters.AddWithValue("@charInLineNum", charInLineNum);
                    command.Parameters.AddWithValue("@wordInLineNum", wordInLineNum);

                    // Execute the insert query
                    command.ExecuteNonQuery();
                }

                // Close the connection
                connection.Close();
            }
        }


        //this function inserts the id of the metadata it got to the correct column in the table
        public static void MTDHandler(string word, int number, string fileName)
        {
            // Get the WordId using the GetOrCreateEntryId function
            int wordId = GetOrCreateEntryId(word, number);

            // Determine which column to update based on the number
            string columnName = "";
            switch (number)
            {
                case 1:
                    columnName = "Patient";
                    break;
                case 2:
                    columnName = "Doctor";
                    break;
                case 3:
                    columnName = "Diag";
                    break;
                case 4:
                    columnName = "Treat";
                    break;
                default:
                    throw new ArgumentException("Invalid number.");
            }

            // Update the specified column in the table
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string updateQuery = $"UPDATE MetaData SET {columnName} = @wordId WHERE FileName = @fileName";

                using (SqlCommand command = new SqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@wordId", wordId);
                    command.Parameters.AddWithValue("@fileName", fileName);

                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }

        //this function gets the value and metadata type as number and return the id of the metadata combo (existing or create new)
        public static int GetOrCreateEntryId(string word, int number)
        {
            int entryId = -1; // Default value if not found

            // Create a SqlConnection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Open the connection
                connection.Open();

                // Create a SqlCommand to check if the combination exists
                string checkQuery = "SELECT id FROM MTDREG WHERE value = @word AND type = @number";
                using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@word", word);
                    checkCommand.Parameters.AddWithValue("@number", number);

                    // Execute the query
                    using (SqlDataReader reader = checkCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Entry already exists, get the ID
                            entryId = reader.GetInt32(0);
                        }
                    }
                }

                // If entryId is still -1, it means the combination doesn't exist, so we insert a new entry
                if (entryId == -1)
                {
                    string insertQuery = "INSERT INTO MTDREG (value, type) VALUES (@word, @number); SELECT SCOPE_IDENTITY();";
                    using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@word", word);
                        insertCommand.Parameters.AddWithValue("@number", number);

                        // Execute the insert query and get the newly generated ID
                        entryId = Convert.ToInt32(insertCommand.ExecuteScalar());
                    }
                }

                // Close the connection
                connection.Close();
            }

            return entryId;
        }

    }
}