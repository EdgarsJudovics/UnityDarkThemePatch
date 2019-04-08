using System;

namespace UnityDarkThemePatch.Helpers
{
    public static class ConsoleHelpers
    {
        /// <summary>
        /// Writes the specified message to the console, optionally using the specified foregroundColor.
        /// </summary>
        /// <param name="message">The string to write to the console.</param>
        /// <param name="foregroundColor">The ConsoleColor to write to the console.</param>
        public static void Write(object message = null, ConsoleColor foregroundColor = (ConsoleColor)(-1))
        {
            if (foregroundColor == (ConsoleColor)(-1))
            {
                Console.Write(message);
                return;
            }
            var defaultFg = Console.ForegroundColor;
            Console.ForegroundColor = foregroundColor;
            Console.Write(message);
            Console.ForegroundColor = defaultFg;
        }

        /// <summary>
        /// Writes the specified message to the console followed by a linebreak., optionally using the specified foregroundColor.
        /// </summary>
        /// <param name="message">The string to write to the console.</param>
        /// <param name="foregroundColor">The ConsoleColor to write to the console.</param>
        public static void WriteLine(object message = null, ConsoleColor foregroundColor = (ConsoleColor)(-1))
        {
            Write(message, foregroundColor);
            Console.WriteLine();
        }

        /// <summary>
        /// Prints an exit message to the console and awaits user input before exiting.
        /// </summary>
        /// <param name="message">The string to write to the console before prompting for input.</param>
        /// <param name="foregroundColor">The ConsoleColor to write to the console.</param>
        public static void ExitOnInput(object message = null, ConsoleColor foregroundColor = (ConsoleColor)(-1))
        {
            if (message != null)
                WriteLine(message, foregroundColor);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(0);
        }

        /// <summary>
        /// <para>Prompts the user for input and parses either a 'yes' or 'no' response.</para>
        /// </summary>
        /// <param name="message">The string to write to the console before prompting for input.</param>
        /// <param name="yes">The action to execute if 'yes' is input.</param>
        /// <param name="no">The action to execute if 'no' is input.</param>
        public static void YesNoChoice(string message, Action yes = null, Action no = null)
        {
            while (true)
            {
                Console.Write($"{message} [Yes/No]: ");
                var answer = Console.ReadLine()?.ToLower();
                if (string.IsNullOrWhiteSpace(answer))
                    continue;
                if (answer.StartsWith("y"))
                {
                    yes?.Invoke();
                    break;
                }
                if (answer.StartsWith("n"))
                {
                    no?.Invoke();
                    break;
                }
            }
        }
    }
}
