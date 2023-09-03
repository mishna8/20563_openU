     
using System;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Xml.Linq;

namespace PROJ
{
    class Program
    {
        //global variables:

        public static string serverName = System.Environment.MachineName;
        public static string connectionString = $"Server={serverName};Integrated Security=True;";


        static void Main(string[] args)
        {
            string DBNAME = "MYDB";
            //when setting up the user gives the DB name to allow multiple setups 

            string connectionString = $"Server={serverName};Integrated Security=True;";

            Console.WriteLine("welcom ");
            while (true)
            {
                Console.WriteLine("Enter a commands: [start, load, stats, search, view, data mine,  exit ] ");
                string command = Console.ReadLine();
                if (!string.IsNullOrEmpty(command))
                {
                    switch (command)
                    {
                        case "start":
                            {
                                Console.WriteLine("enter database name:");
                                DBNAME = Console.ReadLine();
                                if (!string.IsNullOrEmpty(DBNAME))
                                {
                                    if (!DatabaseExists(DBNAME)) Setup(DBNAME);
                                    else Console.WriteLine("database exists, try again");
                                }
                                else Console.WriteLine("Invalid input. Please provide a non-empty database name.");

                                break;
                            }

                        case "load":
                            {
                                Console.WriteLine("enter directory path:");
                                string DirPath = Console.ReadLine();
                                if (!string.IsNullOrEmpty(DBNAME) && !string.IsNullOrEmpty(DirPath))
                                {
                                    if (DatabaseExists(DBNAME)) loadFiles(DBNAME, DirPath);
                                    else Console.WriteLine("database doesnt exists, please enter 'start'");
                                }
                                else Console.WriteLine("Invalid input. Please provide a non-empty directory or database name.");

                                break;
                            }

                        case "stats":
                            {
                                Console.WriteLine("enter where:");
                                string where = Console.ReadLine();
                                if (!string.IsNullOrEmpty(where))
                                {
                                    Console.WriteLine("hi. ");
                                }
                                else Console.WriteLine("Invalid input. ");

                                break;
                            }

                        case "search":
                            {
                                Console.WriteLine("enter what: [file, word, group, expression ]");
                                string input = Console.ReadLine();
                                if (!string.IsNullOrEmpty(input))
                                {
                                    switch (input)
                                    {
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

                                                if (!string.IsNullOrEmpty(inputString))
                                                {
                                                    printlist = ListWords (inputString, param1,param2,param3,param4);
                                                    print(printlist);
                                                }
                                                else Console.WriteLine("Invalid input. Please provide a non-empty parameters ");

                                                break;
                                            }
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
                                        case "expression":
                                            {
                                                List<string> printlist = new List<string>();
                                                // Get user input for the group name
                                                Console.WriteLine("Enter an expression:");
                                                string exprs = Console.ReadLine();
                                                if (!string.IsNullOrEmpty(exprs))
                                                {
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

                        case "view":
                            {
                                Console.WriteLine("Enter a word to view:");
                                string word = Console.ReadLine();
                                if (!string.IsNullOrEmpty(word))
                                {
                                    view(word);                                    
                                }
                                else Console.WriteLine("Invalid input. Please provide a non-empty parameters ");
                                break;
                            }
                        //apriory data mine
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

        static bool DatabaseExists(string dbName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if entry exists in the files table
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
                    //create the METADATA table
                    //THE COLUMS:  FileName, Date, Patient, Doctor, Diag, Treat, Summary 
                    string createMTDTableQuery = $"CREATE TABLE {"MetaData"} ({"FileName"} NVARCHAR(MAX), {"Date"} DATE, {"Patient"} NVARCHAR(MAX),{"Doctor"} NVARCHAR(MAX),{"Diag"} NVARCHAR(MAX),{"Treat"} NVARCHAR(MAX),{"Summary"} NVARCHAR(MAX))";
                    SqlCommand createMTDTableCommand = new SqlCommand(createMTDTableQuery, connection);
                    createMTDTableCommand.ExecuteNonQuery();

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

                    ///add indexes
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");

                }
            }
        }
        static void loadFiles(string databaseName,  string directoryPath)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    connection.ChangeDatabase(databaseName);

                    string[] fileEntries = Directory.GetFiles(directoryPath, "*.txt");

                    foreach (string filePath in fileEntries)
                    {
                        // Check if file entry exists in the files table
                        // BY USING A COUNT QUERY AND SEE IF THE NUMBER OF FILE ARE 0 OR 1 
                        string fileCheckQuery = $"SELECT COUNT(*) FROM {"Files"} WHERE {"FilePath"} = @Path";
                        SqlCommand fileCheckCommand = new SqlCommand(fileCheckQuery, connection);
                        fileCheckCommand.Parameters.AddWithValue("@Path", filePath);
                        int fileCount = (int)fileCheckCommand.ExecuteScalar();

                        if (fileCount == 0)
                        {                          
                            // handle files table
                            string fileName = Path.GetFileName(filePath);
                            int paragraphCount, lineCount, wordCount;
                            CountFileStats(filePath, out paragraphCount, out lineCount, out wordCount);
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

                            //handle the word and meta data tables 
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

                        if (c == '\n' || c == '\r')
                        {
                            inWord = false;
                            inParagraph = true;
                            lineCount++;
                        }
                        else if (char.IsWhiteSpace(c))
                        {
                            inWord = false;
                        }
                        else
                        {
                            if (!inWord)
                            {
                                wordCount++;
                                inWord = true;
                            }

                            if (inParagraph)
                            {
                                paragraphCount++;
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

        static void InsertIntoContentTable(SqlConnection connection, string word, string file, int charCount, int paragNum, int lineInParagNum, int lineNum, int charInLineNum, int wordInLineNum)
        {
            int lastWordId = GetMaxWordId();
            lastWordId++;
            string insertQuery = "INSERT INTO Content (WordId, WordValue, File, charCount, paragNum, lineInParagNum, lineNum, charInLineNum, wordInLineNum, Exprs) " +
                                 "VALUES (@lastWordId, @WordValue, @File, @charCount, @paragNum, @lineInParagNum, @lineNum, @charInLineNum, @wordInLineNum, 0)";
            using (SqlCommand command = new SqlCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@lastWordId", lastWordId);
                command.Parameters.AddWithValue("@WordValue", word);
                command.Parameters.AddWithValue("@File", file);
                command.Parameters.AddWithValue("@charCount", charCount);
                command.Parameters.AddWithValue("@paragNum", paragNum);
                command.Parameters.AddWithValue("@lineInParagNum", lineInParagNum);
                command.Parameters.AddWithValue("@lineNum", lineNum);
                command.Parameters.AddWithValue("@charInLineNum", charInLineNum);
                command.Parameters.AddWithValue("@wordInLineNum", wordInLineNum);

                command.ExecuteNonQuery();
            }
        }

        static void wordInsert(string filePath)
        {

            int paragraphCount = 0;
            int lineCount = 0;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string paragraphSeparator = Environment.NewLine + Environment.NewLine;
                string lineSeparator = Environment.NewLine;
                string documentContent = File.ReadAllText(filePath);
                string[] paragraphs = documentContent.Split(new[] { paragraphSeparator }, StringSplitOptions.None);

                foreach (string paragraph in paragraphs)
                {
                    paragraphCount++;

                    string[] lines = paragraph.Split(new[] { lineSeparator }, StringSplitOptions.None);

                    foreach (string line in lines)
                    {
                        lineCount++;

                        // Use regular expression to split the line into words
                        string pattern = @"\b\w+\b";
                        MatchCollection matches = Regex.Matches(line, pattern);

                        int wordCount = 0;

                        foreach (Match match in matches)
                        {
                            wordCount++;
                            string word = match.Value;
                            int charStart = match.Index;
                            int charCount = match.Length;

                            // Insert word and information into the Content table
                            InsertIntoContentTable(connection, word,filePath, paragraphCount, lineCount, wordCount, charStart, charCount);
                        }
                    }
                }
            }
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Get the current maximum WordId from the table
                    int lastWordId = GetMaxWordId(connection);
                    int sentenceCount = 0;
                    int paragraphCount = 0;

                    using (StreamReader fileReader = new StreamReader(filePath))
                    {
                        string line;

                        while ((line = fileReader.ReadLine()) != null)
                        {
                            // Trim the line to remove leading/trailing spaces
                            line = line.Trim();

                            // Skip empty lines
                            if (string.IsNullOrWhiteSpace(line))
                                continue;

                            // Tokenize the line into words using space as the separator
                            string[] words = Regex.Split(line, @"\s+");

                            foreach (string word in words)
                            {
                                // Increment the WordId for the next word
                                lastWordId++;

                                // Create a SQL command to insert the data
                                string insertQuery = "INSERT INTO Content (WordId, WordValue, File, charCount," +
                                    " paragNum , lineInParagNum, lineNum, charInLineNum, wordInLineNum, Exprs) " +
                                    "VALUES (@WordId, @WordValue, @path, @charCount, " +
                                    " @WordIndex, 0)";

                                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                                {
                                    command.Parameters.AddWithValue("@WordId", lastWordId);
                                    command.Parameters.AddWithValue("@WordValue", word);
                                    command.Parameters.AddWithValue("@path", filePath);
                                    command.Parameters.AddWithValue("@charCount", word.Length); 
                                    ///fix the indexing of the word reading 
                                    command.Parameters.AddWithValue("@WordIndex", lastWordId); // Word index as the word ID

                                    // Execute the SQL command to insert the data
                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }

                Console.WriteLine("Data from " + filePath + " inserted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
        static int GetMaxWordId()
        {
            string query = "SELECT MAX(WordId) FROM Content";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                var result = command.ExecuteScalar();
                if (result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }
                return 0; // If the table is empty, return 0 as the starting WordId
            }
        }


        static void AddEntriesFromFiles( string databaseName, string directoryPath)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    connection.ChangeDatabase(databaseName);

                    string[] fileEntries = Directory.GetFiles(directoryPath, "*.txt");

                    foreach (string filePath in fileEntries)
                    {
                        // Check if file entry exists in the files table
                        string fileCheckQuery = $"SELECT COUNT(*) FROM {filesTableName} WHERE {filesTablePathColumn} = @Path";
                        SqlCommand fileCheckCommand = new SqlCommand(fileCheckQuery, connection);
                        fileCheckCommand.Parameters.AddWithValue("@Path", filePath);
                        int fileCount = (int)fileCheckCommand.ExecuteScalar();

                        if (fileCount == 0)
                        {
                            string[] words = File.ReadAllText(filePath).Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                            // Insert words into the table
                            foreach (string word in words)
                            {
                                string insertQuery = $"INSERT INTO {tableName} ({columnName}) VALUES (@Word)";
                                SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                                insertCommand.Parameters.AddWithValue("@Word", word);
                                insertCommand.ExecuteNonQuery();
                            }

                            // Insert file entry into the files table
                            string fileName = Path.GetFileName(filePath);
                            int wordCount = words.Length;
                            string insertFileQuery = $"INSERT INTO {filesTableName} ({filesTablePathColumn}, {filesTableNameColumn}, {filesTableWordCountColumn}) VALUES (@Path, @Name, @WordCount)";
                            SqlCommand insertFileCommand = new SqlCommand(insertFileQuery, connection);
                            insertFileCommand.Parameters.AddWithValue("@Path", filePath);
                            insertFileCommand.Parameters.AddWithValue("@Name", fileName);
                            insertFileCommand.Parameters.AddWithValue("@WordCount", wordCount);
                            insertFileCommand.ExecuteNonQuery();
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

        ///--------------------------------------------------------------------------------------------///
        //outputs files that contain the inputed word
        static List<string> FindFileByWord(string word)
        {
            //the list of files 
            List<string> fileList = new List<string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Query to search for records containing the specified word
                string query = $"SELECT File FROM Content WHERE WordValue LIKE '%{word}%'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string fileName = reader["File"].ToString();
                            if (!string.IsNullOrEmpty(fileName) && !fileList.Contains(fileName))
                            {
                                fileList.Add(fileName);
                                //Console.WriteLine($"Found file: {fileName}");
                            }
                        }
                    }
                }
            }
            return fileList;
        }
        //outputs files that contain the inputed word as a specified metadata value 
        static List<string> FindFileByMTD(string MTDtype, string word)
        {
            //the list of files 
            List<string> fileList = new List<string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Query to search for records containing the desired value in the specified metadata type  
                string query = "SELECT FileName FROM MetaData WHERE @MTDtype LIKE '%' + @word + '%'";
                
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MTDtype", MTDtype);
                    command.Parameters.AddWithValue("@word", word);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string fileName = reader["FileName"].ToString();
                            if (!string.IsNullOrEmpty(fileName) && !fileList.Contains(fileName))
                            {
                                fileList.Add(fileName);
                                //Console.WriteLine($"Found file: {fileName}");
                            }
                        }
                    }
                }
            }
            return fileList;
        }

        ///--------------------------------------------------------------------------------------------///

        //outputs all words according to filters(few or none)
        static List<string> ListWords(string file, int paragNum, int lineInParagNum, int lineNum, int charInLineNum)
        {
            //the list of words
            List<string> resultList = new List<string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string baseQuery = $"SELECT WordValue FROM Content ";
                
                //find out what filters can be used 
                List<string> filters = new List<string>();

                if (!string.IsNullOrEmpty(file))
                {
                    filters.Add($"File = {file} ");
                }
                if (paragNum != -1 && lineInParagNum != -1)
                {
                    filters.Add($"paragNum = {paragNum} AND lineInParagNum = {lineInParagNum}");
                }
                if (lineNum != -1 && charInLineNum != -1)
                {
                    filters.Add($"lineNum = {lineNum} AND charInLineNum = {charInLineNum}");
                }
                //build the query from the filters 
                string addon = string.Join(" AND ", filters);

                if (!string.IsNullOrEmpty(addon))
                {
                    baseQuery += $"WHERE "+ addon;
                }

                //execute the query 
                using (SqlCommand command = new SqlCommand(baseQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string result = reader["WordValue"].ToString();
                            resultList.Add(result);
                        }
                    }
                }
            }

            return resultList;
        }

        //outputs the 3 sentences 
        static void view(string givenWord)
        {
           
            List<string> wordsInRange = new List<string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Get the file and sentence number for the given word
                string query = "SELECT File, lineNum FROM Content WHERE WordValue = @givenWord";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@givenWord", givenWord);

                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        string file = reader["File"].ToString();
                        int sentenceNumber = Convert.ToInt32(reader["lineNum"]);

                        // Retrieve words within the specified sentence range from the same file
                        string selectQuery = "SELECT WordValue FROM Content " +
                                             "WHERE File = @file " +
                                             "AND lineNum BETWEEN @minSentence AND @maxSentence " +
                                             "ORDER BY lineNum, wordInLineNum";

                        int sentenceRange = 3;
                        int minSentence = Math.Max(1, sentenceNumber - sentenceRange);
                        int maxSentence = sentenceNumber + sentenceRange;

                        using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                        {
                            selectCommand.Parameters.AddWithValue("@file", file);
                            selectCommand.Parameters.AddWithValue("@minSentence", minSentence);
                            selectCommand.Parameters.AddWithValue("@maxSentence", maxSentence);

                            SqlDataReader wordReader = selectCommand.ExecuteReader();
                            while (wordReader.Read())
                            {
                                string word = wordReader["WordValue"].ToString();
                                wordsInRange.Add(word);
                            }
                        }
                    }
                }
            }
            //print the kist as a peragraph to show the part where the word was 
            Console.WriteLine(string.Join(" ", wordsInRange));
        }


        ///--------------------------------------------------------------------------------------------///
        // gets a list of words and a group name and records all those ords as in this group
        static void GroupCreate(List<string> wordList, string groupName)
        {

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                foreach (string word in wordList)
                {
                    // Check if the word-group pair already exists
                    string checkQuery = $"SELECT COUNT(*) FROM Tags WHERE Word = @Word AND Group = @GroupName";
                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Word", word);
                        checkCommand.Parameters.AddWithValue("@GroupName", groupName);

                        int count = (int)checkCommand.ExecuteScalar();

                        if (count == 0)
                        {
                            // Add the word to the group
                            string insertQuery = $"INSERT INTO Tags (Word, Group) VALUES (@Word, @GroupName)";
                            using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@Word", word);
                                insertCommand.Parameters.AddWithValue("@GroupName", groupName);

                                insertCommand.ExecuteNonQuery();
                            }
                            Console.WriteLine($"Value '{groupName}' updated.");
                        }
                    }
                }
            }
        }
        //gets a group and returns all words in this group with thier location (files and indexes)
        static List<string> GroupSearch(string group)
        {
            //the list of the general words 
            List<string> wordsInGroup = new List<string>();
            //the list of the words with thier locations
            List<string> wordslocations = new List<string>();

            //find the generakl word list from the tags table
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = $"SELECT Word FROM Tags WHERE Group = @GroupName";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@GroupName", group);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string word = reader["Word"].ToString();
                            wordsInGroup.Add(word);
                        }
                    }
                }
            }
            //get the specific words with the locations 
            wordslocations = GetWordslocations(wordsInGroup);

            return wordslocations;
        }
        static List<string> GetWordslocations(List<string> words)
        {
            List<string> wordsWithIndexes = new List<string>();
          
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Query to search for records containing the specified word
                string query = $"SELECT WordValue, File, paragNum , lineInParagNum, lineNum, charInLineNum, FROM Content WHERE WordValue IN ({string.Join(",", words)})";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string wordValue = reader["WordValue"].ToString();
                            string File = reader["File"].ToString();
                            int paragNum = int.Parse(reader["paragNum"].ToString());
                            int lineInParagNum = int.Parse(reader["lineInParagNum"].ToString());
                            int lineNum = int.Parse(reader["lineNum"].ToString());
                            int charInLineNum = int.Parse(reader["charInLineNum"].ToString());

                            string wordWithIndex = $"{wordValue} | File: {File}, File Line: {lineNum}, Char Index: {charInLineNum}, Paragraph: {paragNum}, Part Line: {lineInParagNum}";
                            wordsWithIndexes.Add(wordWithIndex);
                        }
                    }
                }
            }           
            return wordsWithIndexes;
        }

        /// -------------------------------------------------------------------------------------------///
        static List<string> SearchExpression(string expression)
        {
            List<string> exprsIndexes = new List<string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                int ID = 0;//if not exists
                //get the expression ID 
                string GetID = $"SELECT ID FROM Expression WHERE Sentence = @Phrase";
                using (SqlCommand IDcommand = new SqlCommand(GetID, connection))
                {
                    IDcommand.Parameters.AddWithValue("@Phrase", expression);
                    var result = IDcommand.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        ID = Convert.ToInt32(result);
                    }
                }

                //if prase not found in the table , create it to find it and look for it again
                if (ID == 0)
                {
                    CreateExpression(expression);
                    SearchExpression(expression);
                }
                //if prase exists return it indexes 
                else
                {
                    // Query to search for records containing the specified word
                    string query = $"SELECT File, paragNum , lineInParagNum, lineNum, charInLineNum, FROM Content WHERE Exprs = ID)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string File = reader["File"].ToString();
                                int paragNum = int.Parse(reader["paragNum"].ToString());
                                int lineInParagNum = int.Parse(reader["lineInParagNum"].ToString());
                                int lineNum = int.Parse(reader["lineNum"].ToString());
                                int charInLineNum = int.Parse(reader["charInLineNum"].ToString());

                                string wordWithIndex = $" File: {File}, File Line: {lineNum}, Char Index: {charInLineNum}, Paragraph: {paragNum}, Part Line: {lineInParagNum}";
                                exprsIndexes.Add(wordWithIndex);
                            }
                        }
                    }
                }
            }
            return exprsIndexes;
        }
        static void CreateExpression(string expression)
        {
            //step 1 : add the expression to the expression table 
            int MaxID = 0;// If the table is empty, return 1 as the starting ID            
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                //if the table is not empty get the last ID 
                string GetMaxID = $"SELECT MAX(ID) FROM Expression";

                using (SqlCommand IDcommand = new SqlCommand(GetMaxID, connection))
                {
                    var result = IDcommand.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        MaxID = Convert.ToInt32(result);
                    }
                }
                //prep the new ID 
                int ID = MaxID + 1;

                //insert the new expression with the new ID 
                string insertQuery = $"INSERT INTO Expression (Sentence,ID) VALUES (@Expression, @ID);";
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@Expression", expression);
                    command.Parameters.AddWithValue("@ID", ID);

                    command.ExecuteNonQuery();
                }
                Console.WriteLine($"Value '{expression}' inserted with ID '{ID}'.");
            }

            //step 2 : add the expression id to all instances in the content table 

            //handle non existance 
        }
        
        static List<string> ExtractWords(string expression, out int wordCount)
        {
            string[] words = expression.Split(new[] { ' ', ',', '.', ';', ':', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> distinctWords = new List<string>();

            foreach (string word in words)
            {
                string normalizedWord = word.ToLower();
                if (!distinctWords.Contains(normalizedWord))
                {
                    distinctWords.Add(normalizedWord);
                }
            }

            wordCount = distinctWords.Count;
            return distinctWords;
        }


        
        


        static void UpdateWordsWithPhraseId(List<string> words, int phraseId)
        {
            // TODO: Set your database connection string
            string connectionString = "Your_Connection_String_Here";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                foreach (string word in words)
                {
                    string updateQuery = $"UPDATE WordTable SET PhraseId = @PhraseId WHERE WordValue = @Word";

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@PhraseId", phraseId);
                        command.Parameters.AddWithValue("@Word", word);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }        

        ///--------------------------------------------------------------------------------------------///

        static void stats()
        {
            
                while (true)
                {
                    Console.WriteLine("Choose an option:");
                    Console.WriteLine("1. Select a File");
                    Console.WriteLine("2. Exit");
                    string option = Console.ReadLine();

                    switch (option)
                    {
                        case "1":
                            SelectFile();
                            break;
                        case "2":
                            return;
                        default:
                            Console.WriteLine("Invalid option. Please try again.");
                            break;
                    }
                }
        }

        static void SelectFile()
        {
            Console.WriteLine("Enter a file name:");
            string fileName = Console.ReadLine();

            while (true)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    int paragCount = 0;
                    string query = "SELECT ParagCount FROM Files WHERE FileName = @fileName";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@fileName", fileName);
                        object result = command.ExecuteScalar();
                        if (result != null && int.TryParse(result.ToString(), out int paragraphCount)) paragCount = paragraphCount;
                        else { Console.WriteLine($"File '{fileName}' not found in the 'Files' table."); stats(); return; }
                    }
                    Console.WriteLine("Choose an option for the selected file:");
                    Console.WriteLine("1. View Counts");
                    Console.WriteLine($"2. select Paragraph number out of {paragCount}");
                    Console.WriteLine("3. Exit to change file");
                    string option = Console.ReadLine();
                    switch (option)
                    {
                        case "1":
                            {
                                string countquery = "SELECT LineCount, WordCount FROM Files WHERE FileName = @fileName";
                                using (SqlCommand command = new SqlCommand(countquery, connection))
                                {
                                    command.Parameters.AddWithValue("@fileName", fileName);

                                    SqlDataReader reader = command.ExecuteReader();
                                    if (reader.Read())
                                    {
                                        int sentenceCount = Convert.ToInt32(reader["LineCount"]);
                                        int wordCount = Convert.ToInt32(reader["WordCount"]);

                                        Console.WriteLine($"paragraphs in {fileName}: {paragCount}");
                                        Console.WriteLine($"Sentences in {fileName}: {sentenceCount}");
                                        Console.WriteLine($"Words in {fileName}: {wordCount}");
                                    }
                                }
                                break;
                            }
                        case "2":
                            ViewParagraphs(fileName);
                            break;
                        case "3":
                            stats();
                            break;
                        default:
                            Console.WriteLine("Invalid option. Please try again.");
                            break;
                    }
                }
            }
        }

            static void ViewParagraphs(string fileName)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Query to retrieve paragraph and counts information for the selected file
                    string query = "SELECT DISTINCT paragNum, COUNT(lineNum) AS SentenceCount, SUM(charoFwORDCount) AS WordCount " +
                                   "FROM Content " +
                                   "WHERE File = @fileName " +
                                   "GROUP BY paragNum " +
                                   "ORDER BY paragNum";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@fileName", fileName);

                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            int paragraphNumber = Convert.ToInt32(reader["paragNum"]);
                            int sentenceCount = Convert.ToInt32(reader["SentenceCount"]);
                            int wordCount = Convert.ToInt32(reader["WordCount"]);

                            Console.WriteLine($"File: {fileName}");
                            Console.WriteLine($"Paragraph: {paragraphNumber}");
                            Console.WriteLine($"Sentences: {sentenceCount}");
                            Console.WriteLine($"Words: {wordCount}");
                            Console.WriteLine();

                            ViewLinesAndCounts(fileName, paragraphNumber);
                        }
                    }
                }
            }

            static void ViewLinesAndCounts(string fileName, int paragraphNumber)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Query to retrieve line and word counts for the selected file and paragraph
                    string query = "SELECT lineInParagNum, COUNT(lineNum) AS LineCount, SUM(charoFwORDCount) AS WordCount " +
                                   "FROM Content " +
                                   "WHERE File = @fileName AND paragNum = @paragraphNumber " +
                                   "GROUP BY lineInParagNum " +
                                   "ORDER BY lineInParagNum";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@fileName", fileName);
                        command.Parameters.AddWithValue("@paragraphNumber", paragraphNumber);

                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            int lineInParagraph = Convert.ToInt32(reader["lineInParagNum"]);
                            int lineCount = Convert.ToInt32(reader["LineCount"]);
                            int wordCount = Convert.ToInt32(reader["WordCount"]);

                            Console.WriteLine($"Paragraph: {paragraphNumber}");
                            Console.WriteLine($"Line in Paragraph: {lineInParagraph}");
                            Console.WriteLine($"Lines: {lineCount}");
                            Console.WriteLine($"Words: {wordCount}");
                            Console.WriteLine();

                            ViewWordsAndCounts(fileName, paragraphNumber, lineInParagraph);
                        }
                    }
                }
            }

            static void ViewWordsAndCounts(string fileName, int paragraphNumber, int lineInParagraph)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Query to retrieve word and character counts for the selected file, paragraph, and line
                    string query = "SELECT wordInLineNum, WordValue, charoFwORDCount " +
                                   "FROM Content " +
                                   "WHERE File = @fileName AND paragNum = @paragraphNumber AND lineInParagNum = @lineInParagraph " +
                                   "ORDER BY wordInLineNum";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@fileName", fileName);
                        command.Parameters.AddWithValue("@paragraphNumber", paragraphNumber);
                        command.Parameters.AddWithValue("@lineInParagraph", lineInParagraph);

                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            int wordInLine = Convert.ToInt32(reader["wordInLineNum"]);
                            string wordValue = reader["WordValue"].ToString();
                            int charCount = Convert.ToInt32(reader["charoFwORDCount"]);

                            Console.WriteLine($"Word: {wordValue}");
                            Console.WriteLine($"Paragraph: {paragraphNumber}");
                            Console.WriteLine($"Line in Paragraph: {lineInParagraph}");
                            Console.WriteLine($"Word in Line: {wordInLine}");
                            Console.WriteLine($"Character Count: {charCount}");
                            Console.WriteLine();
                        }
                    }
                }
            }
        
                   

        static void ExecuteAprioriOnMetaData()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Retrieve data from the "MetaData" table
                string query = "SELECT * FROM MetaData";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    List<List<string>> transactions = new List<List<string>>();

                    while (reader.Read())
                    {
                        string fileName = reader["FileName"].ToString();
                        string patient = reader["Patient"].ToString();
                        string doctor = reader["Doctor"].ToString();
                        string diag = reader["Diag"].ToString();
                        string treat = reader["Treat"].ToString();
                        string summary = reader["Summary"].ToString();

                        // Add attributes of interest to the transaction
                        List<string> transaction = new List<string> { fileName, patient, doctor, diag, treat, summary };
                        transactions.Add(transaction);
                    }

                    // Implement your Apriori algorithm logic here to mine association rules
                    // For simplicity, let's assume you have mined association rules and stored them in a list
                    List<string> associationRules = FindAssociationRules(transactions);

                    // Print the association rules
                    Console.WriteLine("Association Rules:");
                    foreach (string rule in associationRules)
                    {
                        Console.WriteLine(rule);
                    }
                }
            }
        }

        static List<string> FindAssociationRules(List<List<string>> transactions)
        {
            List<string> associationRules = new List<string>();
            // Implement the Apriori algorithm logic here to mine association rules
            // This is a simplified example; a complete Apriori implementation is more complex
            // You may need to implement support for generating frequent itemsets, confidence, and pruning

            // Example: Association rule {A} => {B} with a minimum support and confidence threshold
            double minSupport = 0.2; // Adjust as needed
            double minConfidence = 0.5; // Adjust as needed

            // Iterate over all possible rules
            foreach (List<string> transaction in transactions)
            {
                // Check if the transaction meets the minimum support threshold
                if (MeetMinSupport(transaction, transactions, minSupport))
                {
                    // Generate candidate rules
                    List<string> candidates = GenerateCandidateRules(transaction);

                    // Iterate over candidate rules
                    foreach (string candidateRule in candidates)
                    {
                        // Calculate confidence
                        double confidence = CalculateConfidence(candidateRule, transaction, transactions);

                        // Check if the confidence meets the minimum confidence threshold
                        if (confidence >= minConfidence)
                        {
                            associationRules.Add(candidateRule);
                        }
                    }
                }
            }

            return associationRules;
        }

        static bool MeetMinSupport(List<string> itemset, List<List<string>> transactions, double minSupport)
        {
            // Calculate the support for an itemset
            int count = transactions.Count(t => itemset.All(item => t.Contains(item)));
            double support = (double)count / transactions.Count;

            return support >= minSupport;
        }

        static List<string> GenerateCandidateRules(List<string> transaction)
        {
            // Generate all possible candidate rules for a transaction
            List<string> candidates = new List<string>();

            for (int i = 0; i < transaction.Count; i++)
            {
                for (int j = 0; j < transaction.Count; j++)
                {
                    if (i != j)
                    {
                        candidates.Add($"{transaction[i]} => {transaction[j]}");
                    }
                }
            }

            return candidates;
        }

        static double CalculateConfidence(string rule, List<string> transaction, List<List<string>> transactions)
        {
            // Calculate the confidence for a rule
            string[] items = rule.Split(" => ");
            string antecedent = items[0];
            string consequent = items[1];

            int antecedentCount = transactions.Count(t => t.Contains(antecedent));
            int ruleCount = transactions.Count(t => t.Contains(antecedent) && t.Contains(consequent));

            return (double)ruleCount / antecedentCount;
        }



        ///--------------------------------------------------------------------------------------------///

        static void info()
        {

            Console.WriteLine("the system commands:" +
                "start - create the database and tables" +
                "load - prompets a directory path to scan your file's data to the db" +
                "group - prompets for a name and then action create/search, create will prompet the words to add" +
                "expression - prompets for the sentence " +
                "stats - ");
        }

    }
}
