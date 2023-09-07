     
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
                                //Console.WriteLine($"Found file: {fileName}");
                            }
                        }
                    }
                }
            }
            return fileList;
        }

        //----will be changed---/
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

        //---half made---/
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

        //--------------half made----------/
        
        /*gets from the user a file ti view file-wide stats
        /then an option to drill dowwn to a pragraph to view in the selectd file a stats to a pargraph 
         */
        static void stats()
        {
                //you enter a loop where you can drill down to see different stats 
                while (true)
                {
                    Console.WriteLine("Choose an option:");
                    Console.WriteLine("1. Select a File");
                    Console.WriteLine("2. Exit");
                    string option = Console.ReadLine();

                    switch (option)
                    {
                        //here you pass to the function where you can see the file level view
                        case "1":
                            SelectFile();
                            break;
                        //here you exit the loop
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
            
            while (true)
            {
                //enter the file you want to view
                Console.WriteLine("Enter a file name:");
                string fileName = Console.ReadLine();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    //preper paragraph information for next choise 
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
                        //here you get the file level stats
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
                        //here you drill down to the poaragraph number in the file
                        case "2":
                            Console.WriteLine("Enter a file name:");
                            int paranum = Convert.ToInt32(Console.ReadLine());
                            statsParagraphs(fileName, paranum);
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

            static void statsParagraphs(string fileName, int paranum)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                int words; int paragraph;
                // Query to retrieve paragraph and counts information for the selected file
                //query to sum the last word number in each sentence which is the max where the given paragraph nunmer and file
                    string query = @"
                SELECT SUM(LastWordInSentence.wordInLineNum) AS WordCountInParagraph
                FROM Content AS LastWordInSentence
                WHERE LastWordInSentence.paragNum = @ParagraphNumber
                AND LastWordInSentence.file = @FileName
                AND NOT EXISTS (
                    SELECT 1
                    FROM Content AS NextWordInSentence
                    WHERE NextWordInSentence.paragNum = @ParagraphNumber
                    AND NextWordInSentence.file = @FileName
                    AND NextWordInSentence.lineNum = LastWordInSentence.lineNum
                    AND NextWordInSentence.wordInLineNum > LastWordInSentence.wordInLineNum
                )";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ParagraphNumber", paragraphNumber);
                        command.Parameters.AddWithValue("@FileName", fileName);

                        object result = command.ExecuteScalar();

                        if (result != DBNull.Value)
                        {
                             Convert.ToInt32(result);
                        }
                        else
                        {
                            return 0; // No words found in the specified paragraph
                        }

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
