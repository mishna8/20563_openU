using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
///return a list of locations of the expression given 
static List<string> returnExpression(string expression)
{
    List<string> exprsIndexes = new List<string>();

    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        connection.Open();

        int ID = 0;//if not exists

        //step 1: find the expression ID 
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

        //if prase not found in the table , means will not be in the location table and needs to be search in the files
        if (ID == 0)
        {
            Console.WriteLine("expression was not found ");
            exprsIndexes = newExpression(expression);
        }
        //if prase exists return the locations
        else
        {
            // Query to search for records containing the specified word
            string query = $"SELECT File, lineNum, wordInLineNum, FROM PhraseLocation WHERE Exprs = @ID )";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        command.Parameters.AddWithValue("@ID", ID);
                        string File = reader["File"].ToString();
                        int lineNum = int.Parse(reader["lineNum"].ToString());
                        int wordInLineNum = int.Parse(reader["wordInLineNum"].ToString());

                        string wordWithIndex = $" File: {File}, File Line: {lineNum}, word Index: {wordInLineNum}";
                        exprsIndexes.Add(wordWithIndex);
                    }
                }
            }
        }
    }
    return exprsIndexes;
}

//searches for the expressions accross the files in the 
static List<string> newExpression(string expression)
{
    //every string in this list looks like this: File: {File}, File Line: {lineNum}, word Index: {wordInLineNum}
    List<string> exprsIndexes = new List<string>();
    // Split the phrase into words
    string[] words = expression.Split(' ');
    //get the first word 
    string exprWord = words.FirstOrDefault();

    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        connection.Open();

        //find the properties of the first word that 
        string file;
        int firstlineNum, firstWordNum;

        //find in the content table of all words possible points that can continue to the phrase 
        string query = $"SELECT File, lineNum, wordInLineNum FROM Content WHERE WordValue=@first";
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@first", exprWord);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    file = reader["File"].ToString();
                    firstlineNum = int.Parse(reader["lineNum"].ToString());
                    firstWordNum = int.Parse(reader["wordInLineNum"].ToString());

                    //for each possible point check if its the expression and return the list of IDs
                    bool found;
                    found = matchCheck(expression, file, firstlineNum, firstWordNum);
                    //if it is then add the properties, if not skip
                    if (found)
                    {
                        //1/ add the properties to the list that wil retuen to the user for the current request
                        string exprsIndex = $" File: {file}, File Line: {firstlineNum}, word Index: {firstWordNum}";
                        exprsIndexes.Add(exprsIndex);
                        //2/ add the properties to the expression table for future requests 
                        addExpression(expression, file, firstlineNum, firstWordNum);
                    }

                }
            }
        }
        return exprsIndexes;
    }
}

public void addExpression(string expression, string file, int firstlineNum, int firstWordNum) 
{
    //first step: add or get the expression id from the expression table
    int id = exprsID(expression);
    //second step: add the properties to the locations tagble
    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        connection.Open();

        string insertQuery = $"INSERT INTO PhraseLocation (ID, File, lineNum, wordInLineNum) VALUES (@id, @file, @lineNum, @WordNum)";
        using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
        {
            insertCommand.Parameters.AddWithValue("@id", id);
            insertCommand.Parameters.AddWithValue("@file", file);
            insertCommand.Parameters.AddWithValue("@firstlineNum", firstlineNum);
            insertCommand.Parameters.AddWithValue("@WordNum", firstWordNum)

            insertCommand.ExecuteNonQuery();
        }
    }
}
static int exprsID(string expression)
{
    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        connection.Open();
        int ID=0;

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
            ID = MaxID + 1;

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
        return ID;
    }
}

//get a expression and a starting point in the table , get each next word and compare
public bool matchCheck(string expression, string file, int firstlineNum, int firstWordNum)
{
    
    //for the length of the expression 
    for(int i=0, i)
    {
        //get the expression word
        string expWord;
        //get the table word 
        string nextWord;
        //if they mismatch , break and return false 
        if (expWord != nextWord) return false;

    }
    //if we finished the loop passing all the expression word and didnt break
    return true;
}

//when a new file is added the old expressions that exissts needs to update 
//use the newExpression function on a partial view of the content table ,
//for each file from the new wordId look for existing expressions
static List<string> updateExpression(string expression)
{


}

///----------------------------------------------------------------------------------------------------
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
    static void FindExpression(string expression)
    {



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
