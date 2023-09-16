     
using System;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Transactions;
using System.Xml.Linq;
using System.Collections.Generic;
using PROJ;
using System.Reflection.Metadata;

namespace PROJ
{
    class wordProcess
    {
        
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
                                Console.WriteLine($"Found file: {fileName}");
                            }
                        }
                    }
                }
            }
            return fileList;
        }

        //outputs files that contain the inputed word as a specified metadata value 
        static List<string> FindFileByMTD(int number, string word)
        {
            //the list of files 
            List<string> fileList = new List<string>();

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
                    Console.WriteLine($"This metadata was not found");
                    return fileList;
                }

                //if the combination was found find the file according to the ID found
                //get the column
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

                //find files matching the entryId in the specified column
                string query = $"SELECT FileName FROM MetaData WHERE {columnName} = @entryId";
                using (SqlCommand fileQueryCommand = new SqlCommand(query, connection))
                {
                    fileQueryCommand.Parameters.AddWithValue("@entryId", entryId);

                    using (SqlDataReader reader = fileQueryCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fileList.Add(reader.GetString(0));
                            //Console.WriteLine($"Found file: {fileName}");
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
                //if prase exists (or was just created) return the indexes of first word of the prase
                else
                {
                    //get the startt of the expression
                    string first = expression?.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)?.FirstOrDefault();
                    // Query to search for records containing the specified word
                    string query = $"SELECT File, paragNum , lineInParagNum, lineNum, charInLineNum, FROM Content WHERE Exprs = @ID AND Word=@first)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                command.Parameters.AddWithValue("@first", first);
                                command.Parameters.AddWithValue("@ID", ID);
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

        //---half made---/
        static void CreateExpression(string expression)
        {
            //step 1 : add the expression to the expression table 
                      
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                int MaxID = 0;// If the table is empty, return 1 as the starting ID  
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

        public void ProcessExpression(string expression)
        {
            string[] words = expression.Split(' '); // Split the expression into words

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string file, currWord;
                int currlineNum, currWordNum;
                //get the start of the expression
                string exprWord = words.FirstOrDefault();
                //find possible points that can continue to the phrase 
                string query = $"SELECT Word, File, lineNum, wordInLineNum FROM Content WHERE Word=@first";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@first", exprWord);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            file = reader["File"].ToString();
                            currWord = reader["Word"].ToString();
                            currlineNum = int.Parse(reader["lineNum"].ToString());
                            currWordNum = int.Parse(reader["wordInLineNum"].ToString());

                            if (matchCheck(currWord, file, currlineNum, currWordNum))
                            {
                                // Process the matching entry here.
                                // You can perform any action you need with the matching parameters.
                                // For example:
                                // DoSomething(file, currWord, currlineNum, currWordNum);
                            }
                        }
                    }
                }

                // Define your matchCheck function to determine whether to include the parameter
                bool matchCheck(string word, string file, int lineNum, int wordInLineNum)
                {
                    // Add your matching logic here.
                    // Return true if the parameters should be included, false otherwise.
                }

                //now start a loop from the found 

                using (SqlCommand command = new SqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@Word", word);

                    command.ExecuteNonQuery();
                }
                // for the length of the expression 
                for (int i = 0; i < words.Length; i++)
                {
                    string word = words[i];
                    List<(string file, int lineNum, int wordInLineNum)> matchingWords = new List<(string, int, int)>();

                    // Query the Content table to find words that match the current word in the sentence
                    string query = @"
                    SELECT File, LineNum, WordInLineNum
                    FROM Content
                    WHERE WordValue = @word
                    AND Exprs = 0"; // Make sure we haven't marked this word as part of an expression yet

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@word", word);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string file = reader.GetString(0);
                                int lineNum = reader.GetInt32(1);
                                int wordInLineNum = reader.GetInt32(2);
                                matchingWords.Add((file, lineNum, wordInLineNum));
                            }
                        }
                    }

                    if (matchingWords.Count > 0)
                    {
                        // Start matching the expression
                        foreach (var (file, lineNum, wordInLineNum) in matchingWords)
                        {
                            bool isExpressionMatch = true;
                            int currentIndex = i;
                            int firstPoint = i;

                            for (int j = 1; j < words.Length; j++)
                            {
                                currentIndex++;
                                if (currentIndex >= words.Length)
                                {
                                    isExpressionMatch = false;
                                    break;
                                }

                                var nextWord = GetNextWord(words[currentIndex], lineNum, wordInLineNum, file);
                                if (nextWord == null || !nextWord.Equals(words[currentIndex], StringComparison.OrdinalIgnoreCase))
                                {
                                    isExpressionMatch = false;
                                    break;
                                }
                            }

                            if (isExpressionMatch)
                            {
                                // Mark all the words in the expression with Exprs = 1
                                for (int k = firstPoint; k <= currentIndex; k++)
                                {
                                    MarkWordAsExpression(words[k], lineNum, wordInLineNum, file);
                                }
                            }
                        }
                    }
                }
            }
        }
        //get the next word in the sentence and its index 
        public (string nextWord, int nextLineNum, int nextWordInLineNum) GetNextWord(string currWord, int currLineNum, int currWordInLineNum, string file)
        {
            int nextWordInLine = currWordInLineNum + 1;
            int nextLineNum = currLineNum;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"SELECT Word FROM Content WHERE File = @file AND (LineNum = @nextLineNum AND WordInLineNum = @nextWordInLine)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@file", file);
                    command.Parameters.AddWithValue("@nextLineNum", nextLineNum);
                    command.Parameters.AddWithValue("@nextWordInLine", nextWordInLine);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string nextWord = reader.GetString(0);
                            return (nextWord, nextLineNum, nextWordInLine);
                        }
                    }
                }
            }

            // If no word was found in the same line with WordInLineNum + 1, 
            // increment currLineNum (and WordInLineNum = 0 )
            nextLineNum = currLineNum + 1;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string queryNextLine = @"SELECT Word FROM Content WHERE File = @file AND (LineNum = @nextLineNum AND WordInLineNum = 0)";

                using (SqlCommand commandNextLine = new SqlCommand(queryNextLine, connection))
                {
                    commandNextLine.Parameters.AddWithValue("@file", file);
                    commandNextLine.Parameters.AddWithValue("@nextLineNum", nextLineNum);

                    using (SqlDataReader readerNextLine = commandNextLine.ExecuteReader())
                    {
                        if (readerNextLine.Read())
                        {
                            string nextWord = readerNextLine.GetString(0);
                            return (nextWord, nextLineNum, 0);
                        }
                    }
                }
            }

            // If no word was found in the next line, return (-1, 0, 0).
            return ("-1", 0, 0);
        }

        private void MarkWordAsExpression(string word, int lineNum, int wordInLineNum, string file, int ID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
                UPDATE Content
                SET Exprs = @ID
                WHERE WordValue = @word
                AND LineNum = @lineNum
                AND WordInLineNum = @wordInLineNum
                AND File = @file";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", ID);
                    command.Parameters.AddWithValue("@word", word);
                    command.Parameters.AddWithValue("@lineNum", lineNum);
                    command.Parameters.AddWithValue("@wordInLineNum", wordInLineNum);
                    command.Parameters.AddWithValue("@file", file);

                    command.ExecuteNonQuery();
                }
            }
        }
        ///--------------------------------------------------------------------------------------------///

        ///shows the user the statistics of each level they choose to view
        static void stats()
        {
            //first loop is file level with option to drill down to paragraph and line level
            while (true)
            {
                Console.WriteLine("Choose an option:");
                Console.WriteLine("1. Select a File");
                Console.WriteLine("2. Exit");
                string option = Console.ReadLine();

                switch (option)
                {
                    //here user inputs the file and gets the stats 
                    case "1":
                        {
                            Console.WriteLine("Enter a file name:");
                            string fileName = Console.ReadLine();
                            //first check if file exists
                            using (SqlConnection connection = new SqlConnection(connectionString))
                            {
                                connection.Open();
                                int paragCounter = 0;
                                // Create a parameterized SQL command to check if the file exists
                                string query = "SELECT COUNT(*) FROM Files WHERE FileName = @fileNameOrPath OR FilePath = @fileNameOrPath";
                                using (SqlCommand command = new SqlCommand(query, connection))
                                {
                                    command.Parameters.AddWithValue("@fileNameOrPath", fileNameOrPath);

                                    int fileCount = Convert.ToInt32(command.ExecuteScalar());
                                    //if it does , find its stats and present
                                    if (fileCount > 0)
                                    {
                                        string countquery = "SELECT ParagCount, LineCount, WordCount FROM Files WHERE FileName = @fileName";
                                        using (SqlCommand countcommand = new SqlCommand(countquery, connection))
                                        {
                                            countcommand.Parameters.AddWithValue("@fileName", fileName);

                                            SqlDataReader reader = countcommand.ExecuteReader();
                                            if (reader.Read())
                                            {
                                                paragCounter = Convert.ToInt32(reader["ParagCount"]);
                                                int sentenceCounter = Convert.ToInt32(reader["LineCount"]);
                                                int wordCounter = Convert.ToInt32(reader["WordCount"]);

                                                Console.WriteLine($"paragraphs in {fileName}: {paragCounter}");
                                                Console.WriteLine($"Sentences in {fileName}: {sentenceCounter}");
                                                Console.WriteLine($"Words in {fileName}: {wordCounter}");
                                            }
                                        }
                                        //give the user option to drill down 
                                        Console.WriteLine("do you wish to continue? Y/N");
                                        string paragoption = Console.ReadLine();
                                        switch (paragoption)
                                        {
                                            case "Y":
                                                //go to a loop to view stats within the file
                                                drill(fileName);
                                                break;     
                                            case "N": break; //if not return to the first loop
                                            default:
                                                Console.WriteLine("Invalid option. Please try again.");
                                                break;
                                        }
                                    }
                                    //if not print warning and return to loop
                                    else Console.WriteLine($"File '{fileName}' not found in the 'Files' table.");
                                }
                            }

                            break;
                        }
                    //here you exit the loop
                    case "2":
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }


        static void drill(string fileName)
        {
            //you enter the second loop where you can drill down to see paragraph/line level
            while (true)
            {
                Console.WriteLine("Choose an option:");
                Console.WriteLine("1. Select a paragraph");
                Console.WriteLine("2. select a sentence");
                Console.WriteLine("3. go back");
                string option = Console.ReadLine();
                switch (option)
                {
                    case "1":
                        //sends to function that will print the paragraph stats
                        Console.WriteLine("choose a paragaraph");
                        int parag = Convert.ToInt32(Console.ReadLine());
                        statsParagraphs(fileName, parag);
                        break;
                    case "2":
                        {
                            //sends to function that will print the line stats
                            Console.WriteLine("choose a line");
                            int line = Convert.ToInt32(Console.ReadLine());
                            statsline(fileName, line);
                            //give the user option to drill down 
                            Console.WriteLine("choose a word ? Y/N");
                            string wordoption = Console.ReadLine();
                            switch (wordoption)
                            {
                                case "Y":
                                    {
                                        Console.WriteLine("choose a word number in the sentence");
                                        int word = Convert.ToInt32(Console.ReadLine());
                                        //get the number of characters in the word 
                                        using (SqlConnection connection = new SqlConnection(connectionString))
                                        {
                                            connection.Open();                                            //get charecter number
                                            //query the charNum in the entry of the word/line/file combo
                                            string wordquery = "SELECT charCount FROM Content WHERE wordInLineNum = @word AND lineNum = @lineNum AND  File = @fileName ";
                                            using (SqlCommand wordcommand = new SqlCommand(wordquery, connection))
                                            {
                                                wordcommand.Parameters.AddWithValue("@word", word);
                                                wordcommand.Parameters.AddWithValue("@lineNum", line);
                                                wordcommand.Parameters.AddWithValue("@FileName", fileName);

                                                SqlDataReader reader = wordcommand.ExecuteReader();
                                                if (reader.Read())
                                                {
                                                    int chars = Convert.ToInt32(reader["charCount"]);
                                                    Console.WriteLine($"Word number {word} has {chars} charecters");
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case "N": break;
                                default:
                                    Console.WriteLine("Invalid option. Please try again.");
                                    break;
                            }
                            break;
                        }
                    case "3": return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        // Query to retrieve paragraph stats for the selected file and given paragraoh number
        static void statsParagraphs(string fileName, int num)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                int sentenceCounter=0; int wordCounter = 0;
                //get sentence number
                //count the unique line numbers in the given paragraph nunmer of the file
                //or get the max line in paragraph to indicate how many line there is 
                string linequery = "SELECT MAX(lineInParagNum) AS lineCount FROM Content WHERE paragNum = @num AND File = @fileName  ";
                using (SqlCommand linecommand = new SqlCommand(linequery, connection))
                {
                    linecommand.Parameters.AddWithValue("@num", num);
                    linecommand.Parameters.AddWithValue("@FileName", fileName);

                    SqlDataReader reader = linecommand.ExecuteReader();
                    if (reader.Read())
                    {
                        sentenceCounter = Convert.ToInt32(reader["lineCount"]);  
                    }  
                }

                //get word number
                //query to sum the last word number in each sentence in the given paragraph nunmer of the file
                string wordquery = "SELECT SUM(MaxWordInLineNum) AS wordCount" +
                    "    FROM (SELECT MAX(wordInLineNum) AS MaxWordInLineNum" +
                    "        FROM Content  WHERE paragNum = @num AND File = @fileName GROUP BY lineNum" +
                    "    ) AS Subquery ";
                using (SqlCommand wordcommand = new SqlCommand(wordquery, connection))
                {
                    wordcommand.Parameters.AddWithValue("@num", num);
                    wordcommand.Parameters.AddWithValue("@FileName", fileName);

                    SqlDataReader reader = wordcommand.ExecuteReader();
                    if (reader.Read())
                    {
                        wordCounter = Convert.ToInt32(reader["wordCount"]);
                    }
                }

                //present
                Console.WriteLine($"paragraph number is : {num}");
                Console.WriteLine($"Sentences : {sentenceCounter}");
                Console.WriteLine($"Words : {wordCounter}");
            }
        }
        // Query to retrieve line stats for the selected file and given line number
        static void statsline(string fileName, int num)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                 int lineCounter = 0;                
                //get word number
                //query the last word number in the sentence in the given file
                string wordquery = "SELECT MAX(wordInLineNum) AS LineCount FROM Content WHERE lineNum = @num AND File = @fileName ";
                using (SqlCommand wordcommand = new SqlCommand(wordquery, connection))
                {
                    wordcommand.Parameters.AddWithValue("@num", num);
                    wordcommand.Parameters.AddWithValue("@FileName", fileName);

                    SqlDataReader reader = wordcommand.ExecuteReader();
                    if (reader.Read())
                    {
                        lineCounter = Convert.ToInt32(reader["LineCount"]);
                    }
                }

                //present
                Console.WriteLine($"line  number is : {num}");
                Console.WriteLine($"Words : {lineCounter}");
            }
        }


        /// --------------------------------------------------------------------------------------------//

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



    }
}
