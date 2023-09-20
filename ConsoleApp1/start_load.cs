using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;


///----------------------------------------------------------------------------------------------------
///

static List<string> CreateExpression(string expression)
{
    Console.WriteLine("now searching ");
    //the list of indexes to be returned 
    List<string> exprsIndexes = new List<string>();


    //step 1 : find matching expression in the table
    string[] words = expression.Split(' '); // Split the expression into words

    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        connection.Open();

        //the list of words in current expression
        List<string> wordsID = new List<string>();

        string file, firstWord;
        int firstlineNum, firstWordNum;
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
                    firstWord = reader["Word"].ToString();
                    firstlineNum = int.Parse(reader["lineNum"].ToString());
                    firstWordNum = int.Parse(reader["wordInLineNum"].ToString());

                    //for each possible point check if its the expression and return the list of IDs
                    wordsID = matchCheck(firstWord, file, firstlineNum, firstWordNum);
                    //if it is : then this list is not empty
                    //1. get an id of the expression 
                }
            }
        }

        bool found = false;
        //if the expression was found we have the IDs of all words in the expression 
        if (found)
        {
            //
            //mark them

            //add 




        }
        //this function will also mark the wordes 

        //step 2 : add the expression to the expression table 


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


        //return to the searching function the list of indexes
        return exprsIndexes;
    }

    public void RecordPhraseAppearance(string phrase)
{
    // Split the phrase into words
    string[] words = phrase.Split(' ');

    // Create a dictionary to store the word order within the phrase
    Dictionary<string, int> wordOrder = new Dictionary<string, int>();

    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        connection.Open();

        using (SqlTransaction transaction = connection.BeginTransaction())
        {
            try
            {
                foreach (string word in words)
                {
                    // Check if the word exists in the Words table and get its order
                    int order = GetWordOrder(connection, transaction, word);

                    if (order == -1)
                    {
                        Console.WriteLine($"Word '{word}' not found in the Words table. Skipping phrase.");
                        transaction.Rollback();
                        return;
                    }

                    // Record the word order
                    wordOrder[word] = order;
                }

                // If all words were found in the correct order, record the phrase
                RecordPhrase(connection, transaction, phrase, wordOrder);
                transaction.Commit();
                Console.WriteLine($"Phrase '{phrase}' recorded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                transaction.Rollback();
            }
        }
    }
}

// Other methods (GetWordOrder, RecordPhrase, SerializeWordOrder, etc.)

private int GetWordOrder(SqlConnection connection, SqlTransaction transaction, string word)
{
    string query = "SELECT ID FROM Words WHERE Word = @word";

    using (SqlCommand command = new SqlCommand(query, connection, transaction))
    {
        command.Parameters.AddWithValue("@word", word);

        object result = command.ExecuteScalar();

        if (result != null)
        {
            return (int)result;
        }

        return -1;
    }
}

private void RecordPhrase(SqlConnection connection, SqlTransaction transaction, string phrase, Dictionary<string, int> wordOrder)
{
    string wordOrderString = SerializeWordOrder(wordOrder);

    string insertQuery = "INSERT INTO Phrases (Phrase, WordOrder) VALUES (@phrase, @wordOrder)";

    using (SqlCommand command = new SqlCommand(insertQuery, connection, transaction))
    {
        command.Parameters.AddWithValue("@phrase", phrase);
        command.Parameters.AddWithValue("@wordOrder", wordOrderString);
        command.ExecuteNonQuery();
    }
}

private string SerializeWordOrder(Dictionary<string, int> wordOrder)
{
    string json = Newtonsoft.Json.JsonConvert.SerializeObject(wordOrder);
    return json;
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
