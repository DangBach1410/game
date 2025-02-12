using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Xml.Schema;
using System.Xml.Linq;

namespace ConsoleApp1
{
    class Program
    {
        public static string currentUsername = "";
        public static bool isLoggedIn = false;
        public static string connectionString = "server=localhost;user=root;database=gamedb;port=3306;";
        public static List<Question> questions = new List<Question>();
        public static void Main(string[] args)
        {
            bool exit = false;
            string errorMessage = "";

            while (!exit)
            {
                Console.Clear();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{errorMessage}");
                    Console.ResetColor();
                    errorMessage = "";
                }
                Console.WriteLine("Select an option:");
                Console.WriteLine("1. Login");
                Console.WriteLine("2. Register");
                Console.WriteLine("3. Exit");

                switch (Console.ReadLine())
                {
                    case "1":
                        Login();
                        break;
                    case "2":
                        Register();
                        break;
                    case "3":
                        exit = true;
                        break;
                    default:
                        errorMessage = "Invalid option. Please enter 1, 2, or 3.";
                        break;
                }
            }
        }
        public static void Login()
        {
            string username = "";
            string password = "";
            string errorMessage = "";
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Login");
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{errorMessage}");
                    Console.ResetColor();
                    errorMessage = "";
                }
                Console.WriteLine("Enter your username (or type 'q' to quit): ");
                username = Console.ReadLine();
                if (username.ToLower() == "q")
                {
                    return;
                }
                Console.WriteLine("Enter your password: ");
                password = Console.ReadLine();
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    errorMessage = "Username and password cannot be empty.";
                    continue;
                }
                try
                {
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();

                        string query = "SELECT * FROM user WHERE username = @username AND password = @password";
                        MySqlCommand cmd = new MySqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                currentUsername = username;  
                                isLoggedIn = true;
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Login successful!");
                                Console.ResetColor();
                                Console.ReadLine();
                                MainMenu();
                                break;
                            }
                            else
                            {
                                errorMessage = "Invalid username or password. Please try again.";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                }
            }
        }
        public static void Register()
        {
            string username = "";
            string password = "";
            string confirmPassword = "";
            string name = "";
            string errorMessage = "";

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Register");
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{errorMessage}");
                    Console.ResetColor();
                    errorMessage = "";
                }

                Console.WriteLine("Enter your username (or type 'q' to quit): ");
                username = Console.ReadLine();
                if (username.ToLower() == "q")
                {
                    Console.WriteLine("Exiting login...");
                    break;
                }
                Console.Write("Enter your name: ");
                name = Console.ReadLine();

                Console.Write("Enter your password: ");
                password = Console.ReadLine();

                Console.Write("Confirm your password: ");
                confirmPassword = Console.ReadLine();

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(name))
                {
                    errorMessage = "Username, password, and name cannot be empty.";
                    continue;
                }
                if (password != confirmPassword)
                {
                    errorMessage = "Passwords do not match. Please try again.";
                    continue;
                }

                try
                {
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        string checkQuery = "SELECT COUNT(*) FROM user WHERE username = @username";
                        MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                        checkCmd.Parameters.AddWithValue("@username", username);
                        int userCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (userCount > 0)
                        {
                            errorMessage = "Username is already taken. Please choose another one.";
                            continue;
                        }
                        string insertQuery = "INSERT INTO user (username, password, name) VALUES (@username, @password, @name)";
                        MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection);
                        insertCmd.Parameters.AddWithValue("@username", username);
                        insertCmd.Parameters.AddWithValue("@password", password);
                        insertCmd.Parameters.AddWithValue("@name", name);

                        int rowsAffected = insertCmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Registration successful! You can now log in.");
                            Console.ResetColor();
                            Console.ReadLine(); 
                            break;
                        }
                        else
                        {
                            errorMessage = "An error occurred during registration. Please try again.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                }
            }
        }
        public static void MainMenu()
        {
            bool logout = false;
            string errorMessage = "";
            while (!logout)
            {
                Console.Clear();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(errorMessage);
                    Console.ResetColor();
                    errorMessage = "";
                }
                // Display main menu
                Console.WriteLine("Select mode:");
                Console.WriteLine("1. Create a game (Add/Edit/Delete Questions)");
                Console.WriteLine("2. Play the game");
                Console.WriteLine("3. Play default game");
                Console.WriteLine("4. Log out");

                switch (Console.ReadLine())
                {
                    case "1":
                        CreateGame();
                        break;
                    case "2":
                        PlayGame();
                        break;
                    case "3":
                        PlayDefaultGame();
                        break;
                    case "4":
                        logout = true;
                        questions.Clear();
                        currentUsername = "";
                        isLoggedIn = false;
                        break;
                    default:
                        errorMessage = "Invalid option. Please enter 1, 2, or 3.";
                        break;
                }
            }
        }
        public static void PlayDefaultGame()
        {
            List<Question> default_questions = new List<Question>();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT q.Id, q.QuestionText, q.QuestionType, 
                               mcq.Option1, mcq.Option2, mcq.Option3, mcq.Option4, mcq.CorrectAnswer,
                               oeq.CorrectAnswers, 
                               tfq.IsCorrect
                        FROM Question q
                        LEFT JOIN MultipleChoiceQuestion mcq ON q.Id = mcq.QuestionId
                        LEFT JOIN OpenEndedQuestion oeq ON q.Id = oeq.QuestionId
                        LEFT JOIN TrueFalseQuestion tfq ON q.Id = tfq.QuestionId
                    ";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string questionText = reader["QuestionText"].ToString();
                            string questionType = reader["QuestionType"].ToString();
                            Question question = null;

                            if (questionType == "MultipleChoice")
                            {
                                string[] options = new string[4];
                                options[0] = reader["Option1"].ToString();
                                options[1] = reader["Option2"].ToString();
                                options[2] = reader["Option3"].ToString();
                                options[3] = reader["Option4"].ToString();
                                int correctAnswer = Convert.ToInt32(reader["CorrectAnswer"]);
                                question = new MultipleChoiceQuestion(questionText, options, correctAnswer);
                            }
                            else if (questionType == "OpenEnded")
                            {
                                string correctAnswer = reader["CorrectAnswers"].ToString();
                                string[] answers = correctAnswer.Split(',').Select(answer => answer.Trim()).ToArray();
                                question = new OpenEndedQuestion(questionText, answers);
                            }
                            else if (questionType == "TrueFalse")
                            {
                                bool correctAnswer = Convert.ToBoolean(reader["IsCorrect"]);
                                question = new TrueFalseQuestion(questionText, correctAnswer);
                            }

                            if (question != null)
                            {
                                default_questions.Add(question);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine("Error retrieving questions: " + ex.Message);
                Console.ReadLine();
            }
            List<Question> temp_questions = new List<Question>(questions);
            questions = default_questions;
            (int score, TimeSpan timeSpent) = PlayGame();
            questions = temp_questions;

            string name = string.Empty;
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT name FROM user WHERE username = @username";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@username", currentUsername);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            name = reader["name"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO leaderboard (name, score, TimeSpent) VALUES (@name, @score, @TimeSpent)";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@score", score);
                    cmd.Parameters.AddWithValue("@TimeSpent", timeSpent);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving score: " + ex.Message);
            }

            var leaderboard = new List<(string playerName, int score, TimeSpan timeSpent)>();
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT name, score, TimeSpent FROM leaderboard ORDER BY score DESC, TimeSpent ASC LIMIT 10";
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string playerName = reader["name"].ToString();
                            int playerScore = Convert.ToInt32(reader["score"]);
                            TimeSpan playerTimeSpent = (TimeSpan)reader["TimeSpent"];
                            leaderboard.Add((playerName, playerScore, playerTimeSpent));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving leaderboard: " + ex.Message);
            }
            int playerRank = -1;
            for (int i = 0; i < leaderboard.Count; i++)
            {
                if (leaderboard[i].score == score && leaderboard[i].timeSpent.ToString(@"hh\:mm\:ss") == timeSpent.ToString(@"hh\:mm\:ss") && leaderboard[i].playerName == name)
                {
                    playerRank = i + 1;
                    break;
                }
            }
            Console.Clear();
            Console.WriteLine("Leaderboard:");
            for (int i = 0; i < leaderboard.Count; i++)
            {
                string rank = (i + 1).ToString();
                string player = leaderboard[i].playerName;
                int playerScore = leaderboard[i].score;
                TimeSpan playerTime = leaderboard[i].timeSpent;

                if (playerRank == i + 1)
                {
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.WriteLine($"{rank}. {player} - {playerScore} points - {playerTime.ToString(@"hh\:mm\:ss")}[**You**]");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"{rank}. {player} - {playerScore} points - {playerTime.ToString(@"hh\:mm\:ss")}");
                }
            }

            if (playerRank == -1)
            {
                Console.WriteLine("\nYou are not in the top 10 yet. Try again to improve your score!");
            }
            else
            {
                Console.WriteLine($"\nYour rank: {playerRank}");
            }
            Console.ReadLine();
        }
        public static void CreateGame()
        {
            string errorMessage = "";
            bool exitCreateMode = false;

            while (!exitCreateMode)
            {
                Console.Clear();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(errorMessage);
                    Console.ResetColor();
                    errorMessage = "";
                }
                // Display menu Create Mode
                Console.WriteLine("Create Mode");
                Console.WriteLine("1. Add a new question");
                Console.WriteLine("2. Edit a question");
                Console.WriteLine("3. Delete a question");
                Console.WriteLine("4. Return to main menu");
                Console.Write("Choose an option: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        AddQuestion();
                        break;
                    case "2":
                        EditQuestion();
                        break;
                    case "3":
                        DeleteQuestion();
                        break;
                    case "4":
                        exitCreateMode = true;
                        break;
                    default:
                        errorMessage = "Invalid option. Please enter a number between 1 and 4.";
                        break;
                }
            }
        }
        public static void AddQuestion()
        {
            string errorMessage = "";
            bool exitAddQuestion = false; 

            while (!exitAddQuestion)
            {
                Console.Clear();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(errorMessage);
                    Console.ResetColor();
                    errorMessage = ""; 
                }
                // Display menu options
                Console.WriteLine("Select question type:");
                Console.WriteLine("1. Multiple Choice");
                Console.WriteLine("2. Open-ended");
                Console.WriteLine("3. True/False");
                Console.WriteLine("4. Back");
                Console.Write("Enter your choice: ");

                // Process user input
                switch (Console.ReadLine())
                {
                    case "1":
                        AddMultipleChoiceQuestion();
                        break;
                    case "2":
                        AddOpenEndedQuestion();
                        break;
                    case "3":
                        AddTrueFalseQuestion();
                        break;
                    case "4":
                        exitAddQuestion = true;
                        break;
                    default:
                        errorMessage = "Invalid option. Please enter a number between 1 and 4.";
                        break;
                }
            }
        }
        public static void AddMultipleChoiceQuestion()
        {
            Console.Clear();
            string questionText = "";
            string errorMessage = "";
            while (string.IsNullOrWhiteSpace(questionText))
            {
                Console.Clear();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(errorMessage);
                    Console.ResetColor();
                    errorMessage = "";
                }
                Console.WriteLine("Enter the question:");
                questionText = Console.ReadLine().Trim();
                if (string.IsNullOrWhiteSpace(questionText))
                {
                    errorMessage = "Question cannot be empty. Please enter a valid question.";
                }
            }

            string input = "";
            string[] options = new string[4];
            for (int i = 0; i < 4; i++)
            {
                input = "";
                while (string.IsNullOrEmpty(input) || Array.Exists(options, option => option == input))
                {
                    Console.Clear();
                    Console.WriteLine("Enter the question:");
                    Console.WriteLine(questionText);
                    for (int j = 0; j < i; j++)
                    {
                        Console.WriteLine($"Enter option {j + 1}:");
                        Console.WriteLine(options[j]);
                    }
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(errorMessage);
                        Console.ResetColor();
                        errorMessage = "";
                    }
                    Console.WriteLine($"Enter option {i + 1}:");
                    input = Console.ReadLine().Trim();
                    if (string.IsNullOrEmpty(input))
                    {
                        errorMessage = "Option cannot be empty. Please try again.";
                    }
                    else if (Array.Exists(options, option => option == input))
                    {
                        errorMessage = "Option cannot be duplicate. Please enter a unique option.";
                    }
                }
                options[i] = input;
            }
            int correctAnswer = 0;
            bool isValid = false;

            while (!isValid)
            {
                Console.Clear();
                Console.WriteLine("Enter the question:");
                Console.WriteLine(questionText);
                for (int i = 0; i < 4; i++)
                {
                    Console.WriteLine($"Enter option {i + 1}:");
                    Console.WriteLine(options[i]);
                }
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(errorMessage);
                    Console.ResetColor();
                    errorMessage = "";
                }
                Console.WriteLine("Enter the number (1-4) of the correct answer:");
                input = Console.ReadLine();
                if (int.TryParse(input, out correctAnswer) && correctAnswer >= 1 && correctAnswer <= 4)
                {
                    isValid = true;
                }
                else
                {
                    errorMessage = "Invalid input. Please enter a number between 1 and 4.";
                }
            }
            // Add the question to the list
            questions.Add(new MultipleChoiceQuestion(questionText, options, correctAnswer));

            Console.Clear();
            Console.WriteLine("Enter the question:");
            Console.WriteLine(questionText);
            for (int i = 0; i < 4; i++)
            {
                Console.WriteLine($"Enter option {i + 1}:");
                Console.WriteLine(options[i]);
            }
            Console.WriteLine("Enter the number (1-4) of the correct answer:");
            Console.WriteLine(input);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Question added successfully!");
            Console.ResetColor();
            Console.WriteLine("Press any key to return to the AddQuestion menu...");
            Console.ReadKey();
        }
        public static void AddOpenEndedQuestion()
        {
            string questionText = "";
            string errorMessage = "";

            while (string.IsNullOrWhiteSpace(questionText))
            {
                Console.Clear();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(errorMessage);
                    Console.ResetColor();
                    errorMessage = "";
                }
                Console.WriteLine("Enter the question:");
                questionText = Console.ReadLine().Trim();
                if (string.IsNullOrWhiteSpace(questionText))
                {
                    errorMessage = "Question cannot be empty. Please enter a valid question.";
                }
            }
            string[] answers = null;
            while (answers == null || answers.Length == 0)
            {
                Console.Clear();
                Console.WriteLine("Enter the question:");
                Console.WriteLine(questionText);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(errorMessage);
                    Console.ResetColor();
                    errorMessage = "";
                }
                Console.WriteLine("Enter the correct answer(s), separated by commas (e.g. United Kingdom, UK):");
                string input = Console.ReadLine().Trim();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    answers = input.Split(',').Select(answer => answer.Trim()).ToArray();
                    if (answers.Any(answer => string.IsNullOrWhiteSpace(answer)))
                    {
                        answers = null;
                    }
                }
                if (answers == null)
                {
                    errorMessage = "Each answer must be non-empty and cannot be just spaces.";
                }
            }
            questions.Add(new OpenEndedQuestion(questionText, answers));

            Console.Clear();
            Console.WriteLine("Enter the question:");
            Console.WriteLine(questionText);
            Console.WriteLine("Enter the correct answer(s), separated by commas (e.g. United Kingdom, UK):");
            Console.WriteLine(string.Join(", ", answers));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Question added successfully!");
            Console.ResetColor();
            Console.WriteLine("Press any key to return to the AddQuestion menu...");
            Console.ReadKey();
        }
        public static void AddTrueFalseQuestion()
        {
            string errorMessage = ""; 
            string questionText = ""; 
            bool correctAnswer = false; 
            bool isAnswerValid = false; 

            while (string.IsNullOrEmpty(questionText))
            {
                Console.Clear();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(errorMessage);
                    Console.ResetColor();
                    errorMessage = ""; 
                }

                Console.WriteLine("Enter the statement (True/False question):");
                questionText = Console.ReadLine().Trim();

                if (string.IsNullOrEmpty(questionText))
                {
                    errorMessage = "Question cannot be empty. Please try again.";
                }
            }

            while (!isAnswerValid)
            {
                Console.Clear();
                Console.WriteLine("Enter the statement (True/False question):");
                Console.WriteLine(questionText);

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(errorMessage);
                    Console.ResetColor();
                    errorMessage = "";
                }

                Console.WriteLine("Is this statement true or false? (true/false):");
                string input = Console.ReadLine().Trim().ToLower();

                if (input == "true")
                {
                    correctAnswer = true;
                    isAnswerValid = true;
                }
                else if (input == "false")
                {
                    correctAnswer = false;
                    isAnswerValid = true;
                }
                else
                {
                    errorMessage = "Invalid input. Please enter 'true' or 'false'.";
                }
            }

            questions.Add(new TrueFalseQuestion(questionText, correctAnswer));

            Console.Clear();
            Console.WriteLine("Enter the statement (True/False question):");
            Console.WriteLine(questionText);
            Console.WriteLine("Is this statement true or false? (true/false):");
            Console.WriteLine(correctAnswer);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Question added successfully!");
            Console.ResetColor();
            Console.WriteLine("Press any key to return to the AddQuestion menu...");
            Console.ReadKey();
        }
        public static void EditQuestion()
        {
            string errorMessage = "";
            while (true)
            {
                Console.Clear();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(errorMessage);
                    Console.ResetColor();
                    errorMessage = "";
                }
                DisplayQuestions();
                Console.WriteLine("Select question number to edit (or 'q' to quit):");
                string userChoice = Console.ReadLine().ToLower().Trim();
                if (userChoice == "q")
                {
                    break;
                }
                if (int.TryParse(userChoice, out int index) && index > 0 && index <= questions.Count)
                {
                    index -= 1;
                    var question = questions[index];
                    if (question is MultipleChoiceQuestion mcq)
                    {
                        string questionText = "";
                        while (string.IsNullOrWhiteSpace(questionText))
                        {
                            Console.Clear();
                            DisplayQuestions();
                            Console.WriteLine("Select question number to edit:");
                            Console.WriteLine(index + 1);
                            if (!string.IsNullOrEmpty(errorMessage))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(errorMessage);
                                Console.ResetColor();
                                errorMessage = "";
                            }
                            Console.WriteLine("Enter the new question text:");
                            questionText = Console.ReadLine().Trim();
                            if (string.IsNullOrWhiteSpace(questionText))
                            {
                                errorMessage = "Question cannot be empty. Please enter a valid question.";
                            }
                        }

                        // Input the 4 unique options
                        string input = "";
                        string[] options = new string[4];
                        for (int i = 0; i < 4; i++)
                        {
                            input = "";
                            while (string.IsNullOrEmpty(input) || Array.Exists(options, option => option == input))
                            {
                                Console.Clear();
                                DisplayQuestions();
                                Console.WriteLine("Select question number to edit:");
                                Console.WriteLine(index + 1);
                                Console.WriteLine("Enter the new question text:");
                                Console.WriteLine(questionText);
                                for (int j = 0; j < i; j++)
                                {
                                    Console.WriteLine($"Enter option {j + 1}:");
                                    Console.WriteLine(options[j]);
                                }
                                if (!string.IsNullOrEmpty(errorMessage))
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine(errorMessage);
                                    Console.ResetColor();
                                    errorMessage = "";
                                }
                                Console.WriteLine($"Enter option {i + 1}:");
                                input = Console.ReadLine().Trim();
                                if (string.IsNullOrEmpty(input))
                                {
                                    errorMessage = "Option cannot be empty. Please try again.";
                                }
                                else if (Array.Exists(options, option => option == input))
                                {
                                    errorMessage = "Option cannot be duplicate. Please enter a unique option.";
                                }
                            }
                            options[i] = input;
                        }
                        // Input the correct answer
                        int correctAnswer = 0;
                        bool isValid = false;

                        while (!isValid)
                        {
                            Console.Clear();
                            DisplayQuestions();
                            Console.WriteLine("Select question number to edit:");
                            Console.WriteLine(index + 1);
                            Console.WriteLine("Enter the new question text:");
                            Console.WriteLine(questionText);
                            for (int i = 0; i < 4; i++)
                            {
                                Console.WriteLine($"Enter option {i + 1}:");
                                Console.WriteLine(options[i]);
                            }
                            if (!string.IsNullOrEmpty(errorMessage))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(errorMessage);
                                Console.ResetColor();
                                errorMessage = "";
                            }
                            Console.WriteLine("Enter the number (1-4) of the correct answer:");
                            input = Console.ReadLine();
                            if (int.TryParse(input, out correctAnswer) && correctAnswer >= 1 && correctAnswer <= 4)
                            {
                                isValid = true;
                            }
                            else
                            {
                                errorMessage = "Invalid input. Please enter a number between 1 and 4.";
                            }
                        }
                        // Add the question to the list
                        questions[index] = new MultipleChoiceQuestion(questionText, options, correctAnswer);

                        Console.Clear();
                        DisplayQuestions();
                        Console.WriteLine("Select question number to edit:");
                        Console.WriteLine(index + 1);
                        Console.WriteLine("Enter the new question text:");
                        Console.WriteLine(questionText);
                        for (int i = 0; i < 4; i++)
                        {
                            Console.WriteLine($"Enter option {i + 1}:");
                            Console.WriteLine(options[i]);
                        }
                        Console.WriteLine("Enter the number (1-4) of the correct answer:");
                        Console.WriteLine(input);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Edit question successfully");
                        Console.ResetColor();
                        Console.WriteLine("Press any key to return to the EditQuestion menu...");
                        Console.ReadKey();
                    }
                    else if (question is OpenEndedQuestion oeq)
                    {
                        string questionText = "";
                        while (string.IsNullOrWhiteSpace(questionText))
                        {
                            Console.Clear();
                            DisplayQuestions();
                            Console.WriteLine("Select question number to edit:");
                            Console.WriteLine(index + 1);
                            if (!string.IsNullOrEmpty(errorMessage))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(errorMessage);
                                Console.ResetColor();
                                errorMessage = "";
                            }
                            Console.WriteLine("Enter the new question text:");
                            questionText = Console.ReadLine().Trim();
                            if (string.IsNullOrWhiteSpace(questionText))
                            {
                                errorMessage = "Question cannot be empty. Please enter a valid question.";
                            }
                        }
                        string[] answers = null;
                        while (answers == null || answers.Length == 0)
                        {
                            Console.Clear();
                            DisplayQuestions();
                            Console.WriteLine("Select question number to edit:");
                            Console.WriteLine(index + 1);
                            Console.WriteLine("Enter the new question text:");
                            Console.WriteLine(questionText);
                            if (!string.IsNullOrEmpty(errorMessage))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(errorMessage);
                                Console.ResetColor();
                                errorMessage = "";
                            }
                            Console.WriteLine("Enter the correct answer(s), separated by commas (e.g. United Kingdom, UK):");
                            string input = Console.ReadLine().Trim();
                            if (!string.IsNullOrWhiteSpace(input))
                            {
                                answers = input.Split(',').Select(answer => answer.Trim()).ToArray();
                                if (answers.Any(answer => string.IsNullOrWhiteSpace(answer)))
                                {
                                    answers = null;
                                }
                            }
                            if (answers == null)
                            {
                                errorMessage = "Each answer must be non-empty and cannot be just spaces.";
                            }
                        }
                        questions[index] = new OpenEndedQuestion(questionText, answers);

                        Console.Clear();
                        DisplayQuestions();
                        Console.WriteLine("Select question number to edit:");
                        Console.WriteLine(index + 1);
                        Console.WriteLine("Enter the new question text:");
                        Console.WriteLine(questionText);
                        Console.WriteLine("Enter the correct answer(s), separated by commas (e.g. United Kingdom, UK):");
                        Console.WriteLine(string.Join(", ", answers));
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Edit question successfully!");
                        Console.ResetColor();
                        Console.WriteLine("Press any key to return to the EditQuestion menu...");
                        Console.ReadKey();
                    }
                    else if (question is TrueFalseQuestion tfq)
                    {
                        string questionText = "";
                        bool correctAnswer = false;
                        bool isAnswerValid = false;

                        while (string.IsNullOrEmpty(questionText))
                        {
                            Console.Clear();
                            DisplayQuestions();
                            Console.WriteLine("Select question number to edit:");
                            Console.WriteLine(index + 1);
                            if (!string.IsNullOrEmpty(errorMessage))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(errorMessage);
                                Console.ResetColor();
                                errorMessage = "";
                            }

                            Console.WriteLine("Enter the new statement (True/False question):");
                            questionText = Console.ReadLine().Trim();
                            if (string.IsNullOrEmpty(questionText))
                            {
                                errorMessage = "Question cannot be empty. Please try again.";
                            }
                        }

                        while (!isAnswerValid)
                        {
                            Console.Clear();
                            DisplayQuestions();
                            Console.WriteLine("Select question number to edit:");
                            Console.WriteLine(index + 1);
                            Console.WriteLine("Enter the new statement (True/False question):");
                            Console.WriteLine(questionText);

                            if (!string.IsNullOrEmpty(errorMessage))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(errorMessage);
                                Console.ResetColor();
                                errorMessage = "";
                            }

                            Console.WriteLine("Is this statement true or false? (true/false):");
                            string input = Console.ReadLine().Trim().ToLower();

                            if (input == "true")
                            {
                                correctAnswer = true;
                                isAnswerValid = true;
                            }
                            else if (input == "false")
                            {
                                correctAnswer = false;
                                isAnswerValid = true;
                            }
                            else
                            {
                                errorMessage = "Invalid input. Please enter 'true' or 'false'.";
                            }
                        }

                        questions[index] = new TrueFalseQuestion(questionText, correctAnswer);

                        Console.Clear();
                        DisplayQuestions();
                        Console.WriteLine("Select question number to edit:");
                        Console.WriteLine(index + 1);
                        Console.WriteLine("Enter the new statement (True/False question):");
                        Console.WriteLine(questionText);
                        Console.WriteLine("Is this statement true or false? (true/false):");
                        Console.WriteLine(correctAnswer);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Edit question successfully");
                        Console.ResetColor();
                        Console.WriteLine("Press any key to return to the EditQuestion menu...");
                        Console.ReadKey();
                    }
                }
                else
                {
                    errorMessage = "Invalid question number. Please try again.";
                }
            }
        }
        public static void DeleteQuestion()
        {
            string errorMessage = "";
            while (true)
            {
                Console.Clear();
                DisplayQuestions();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(errorMessage);
                    Console.ResetColor();
                    errorMessage = "";
                }
                // Instructions for the user
                Console.WriteLine("Enter the question number to delete (or 'q' to quit):");
                string userInput = Console.ReadLine()?.Trim();

                // Check if the user wants to quit
                if (userInput?.ToLower() == "q")
                {
                    break;
                }

                // Attempt to parse the user input
                if (int.TryParse(userInput, out int index) && index > 0 && index <= questions.Count)
                {
                    // Adjust for 0-based index and delete the question
                    index -= 1;
                    questions.RemoveAt(index);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Question deleted successfully!");
                    Console.ResetColor();

                    // Pause for user to acknowledge
                    Console.WriteLine("Press any key to return to the Delete menu...");
                    Console.ReadKey();
                }
                else
                {
                    // Set an error message if input is invalid
                    errorMessage = "Invalid question number. Please try again.";
                }
            }
        }
        public static void DisplayQuestions()
        {
            int questionIndex = 0;
            string listOfQuestions = "";
            foreach (var question in questions)
            {
                questionIndex++;
                if (question is MultipleChoiceQuestion multipleChoiceQuestion)
                {
                    listOfQuestions += $"Question {questionIndex}: {multipleChoiceQuestion.QuestionText}\n";
                    for (int i = 0; i < multipleChoiceQuestion.Options.Length; i++)
                    {
                        listOfQuestions += $"{i + 1}. {multipleChoiceQuestion.Options[i]}\n";
                    }
                    listOfQuestions += $"Correct Answer: {multipleChoiceQuestion.GetCorrectAnswer()}\n\n";
                }
                else if (question is TrueFalseQuestion trueFalseQuestion)
                {
                    listOfQuestions += $"Question {questionIndex}: {trueFalseQuestion.QuestionText}\n";
                    listOfQuestions += $"Correct Answer: {(trueFalseQuestion.CorrectAnswer ? "True" : "False")}\n\n";
                }
                else if (question is OpenEndedQuestion openEndedQuestion)
                {
                    listOfQuestions += $"Question {questionIndex}: {openEndedQuestion.QuestionText}\n";
                    listOfQuestions += "Correct Answers: " + openEndedQuestion.GetCorrectAnswer() + "\n\n";
                }
            }
            if (string.IsNullOrEmpty(listOfQuestions))
            {
                Console.WriteLine("There are no questions for updating");
            }
            else
            {
                Console.WriteLine(listOfQuestions);
            }
        }
        public static (int, TimeSpan) PlayGame()
        {
            Console.Clear();
            if (questions.Count == 0)
            {
                Console.WriteLine("No questions available. Please create questions first.");
                Console.WriteLine("Press any key to return to the main menu...");
                Console.ReadKey();
                return (0, TimeSpan.Zero) ;
            }
            Random rng = new Random();
            int numbeofquestions = questions.Count;
            questions = questions.OrderBy(question => rng.Next(0, numbeofquestions)).ToList();
            foreach (var question in questions)
            {
                if (question is MultipleChoiceQuestion mcq)
                {
                    string[] shuffledOptions = mcq.Options.ToArray();
                    int correctIndex = mcq.CorrectAnswer - 1;

                    var indexedOptions = shuffledOptions.Select((option, index) => new { Option = option, Index = index }).OrderBy(obj => rng.Next(0, 4)).ToList();
                    mcq.Options = indexedOptions.Select(obj => obj.Option).ToArray();
                    mcq.CorrectAnswer = indexedOptions.FindIndex(option => option.Index == correctIndex) + 1;
                }
            }
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int score = 0;

            foreach (var question in questions)
            {
                Console.Clear();
                bool isCorrect = question.AskQuestion(questions.IndexOf(question) + 1);
                if (isCorrect)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Correct!");
                    Console.ResetColor();
                    score++;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Incorrect!");
                    Console.ResetColor();
                    Console.WriteLine($"The correct answer is: {question.GetCorrectAnswer()}");
                }

                Console.WriteLine("\nPress Enter to continue...");
                Console.ReadLine();
            }

            stopwatch.Stop();
            TimeSpan timeTaken = stopwatch.Elapsed;
            string formattedTime = timeTaken.ToString(@"hh\:mm\:ss");
            Console.Clear();
            Console.WriteLine($"Game Over!");
            Console.WriteLine($"You scored {score} out of {questions.Count}.");
            //Console.WriteLine($"Time taken: {timeTaken.TotalMinutes:0.00} minutes.\n");
            Console.WriteLine($"Time taken: {formattedTime}.\n");
            Console.WriteLine("Do you want to see all the correct answers? (Enter 'yes' to see all the correct answers)");
            if (Console.ReadLine().Trim().ToLower() == "yes")
            {
                foreach (var question in questions)
                {
                    Console.WriteLine($"Question {questions.IndexOf(question) + 1}: {question.QuestionText}");
                    Console.WriteLine($"Correct Answer: {question.GetCorrectAnswer()}");
                    Console.WriteLine($"Your Answer: {question.UserAnswer}");
                    Console.WriteLine();
                }
            }
            Console.WriteLine("Do you want to play again? (Enter 'yes' to play again)");
            if (Console.ReadLine().Trim().ToLower() == "yes")
            {
                PlayGame();
            }
            return (score, timeTaken);
        }
    }
    abstract class Question
    {
        protected string _questionText;
        public string QuestionText
        {
            get { return _questionText; }
            set { _questionText = value; }
        }
        protected string _userAnswer;
        public string UserAnswer
        {
            get { return _userAnswer; }
            set { _userAnswer = value; }
        }
        public Question(string questionText)
        {
            QuestionText = questionText;
        }

        public abstract bool AskQuestion(int idOfQuestion);
        public abstract string GetCorrectAnswer();
    }

    class MultipleChoiceQuestion : Question
    {
        private string[] _options;
        private int _correctAnswer;

        public string[] Options
        {
            get { return _options; }
            set { _options = value; }
        }

        public int CorrectAnswer
        {
            get { return _correctAnswer; }
            set { _correctAnswer = value; }
        }

        public MultipleChoiceQuestion(string questionText, string[] options, int correctAnswer) : base(questionText)
        {
            Options = options;
            CorrectAnswer = correctAnswer;
        }

        public override bool AskQuestion(int idOfQuestion)
        {
            string errorMessage = "";
            while (true)
            {
                Console.Clear();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(errorMessage);
                    Console.ResetColor();
                }
                Console.WriteLine($"Question {idOfQuestion}: {QuestionText}");
                for (int i = 0; i < Options.Length; i++)
                {
                    Console.WriteLine($"{i + 1}. {Options[i]}");
                }
                Console.Write("Your answer: ");
                string userInput = Console.ReadLine().Trim();
                if (int.TryParse(userInput, out int answer) && answer >= 1 && answer <= 4)
                {
                    UserAnswer = Options[answer - 1];
                    return answer == CorrectAnswer;
                }
                else
                {
                    errorMessage = "Invalid input. Please select a valid option (1, 2, 3, 4).";
                }
            }
        }
        public override string GetCorrectAnswer()
        {
            return Options[CorrectAnswer - 1];
        }
    }

    class OpenEndedQuestion : Question
    {
        private string[] _correctAnswers;

        public string[] CorrectAnswers
        {
            get { return _correctAnswers; }
            set { _correctAnswers = value; }
        }

        public OpenEndedQuestion(string questionText, string[] correctAnswers) : base(questionText)
        {
            CorrectAnswers = correctAnswers;
        }

        public override bool AskQuestion(int idOfQuestion)
        {
            string errorMessage = ""; 
            while (true)
            {
                Console.Clear();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(errorMessage);
                    Console.ResetColor();
                    errorMessage = "";
                }
                Console.WriteLine($"Question {idOfQuestion}: {QuestionText}");
                Console.Write("Your answer: ");
                UserAnswer = Console.ReadLine();
                string userInput = UserAnswer.Trim().ToLower();
                if (string.IsNullOrEmpty(userInput))
                {
                    errorMessage = "You must enter an answer! Please try again.";
                    continue;
                }

                foreach (var correctAnswer in CorrectAnswers)
                {
                    if (userInput == correctAnswer.Trim().ToLower())
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public override string GetCorrectAnswer()
        {
            return string.Join(", ", CorrectAnswers);
        }
    }

    class TrueFalseQuestion : Question
    {
        private bool _correctAnswer;

        public bool CorrectAnswer
        {
            get { return _correctAnswer; }
            set { _correctAnswer = value; }
        }

        public TrueFalseQuestion(string questionText, bool correctAnswer) : base(questionText)
        {
            CorrectAnswer = correctAnswer;
        }

        public override bool AskQuestion(int idOfQuestion)
        {
            string errorMessage = ""; 
            while (true)
            {
                Console.Clear();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(errorMessage);
                    Console.ResetColor();
                }
                Console.WriteLine($"Question {idOfQuestion}: {QuestionText} (true/false)");
                Console.Write("Your answer: ");
                UserAnswer = Console.ReadLine();
                string userInput = UserAnswer.Trim().ToLower();
                if (userInput == "true" || userInput == "false")
                {
                    bool answer = bool.Parse(userInput);
                    return answer == CorrectAnswer;
                }
                else
                {
                    errorMessage = "Invalid input. Please enter 'true' or 'false'.";
                }
            }
        }
        public override string GetCorrectAnswer()
        {
            return CorrectAnswer ? "True" : "False";
        }
    }
}
