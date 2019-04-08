using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityDarkThemePatch.Models;

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
                {
                    continue;
                }
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

        /// <summary>
        /// <para>Prompts the user for input, parsing a selection from the specified choices.</para>
        /// </summary>
        /// <param name="choices">An IEnumerable of ConsoleChoice objects.</param>
        /// <param name="message">An optional message to display before options are printed.</param>
        public static void MultipleChoice(IEnumerable<ConsoleChoice> choices, Action quitAction = null, string message = "Please select an option")
        {
            if (choices == null)
            {
                throw new ArgumentNullException(nameof(choices));
            }

            var choicesWithIndex = choices.Select((c, i) => new { Index = i, Value = c });

            while (true)
            {
                Console.WriteLine($"{message}: ");
                choicesWithIndex
                    .ToList()
                    .ForEach(c => Console.WriteLine($"{c.Index}: {c.Value.ChoiceDescription}"));
                Console.WriteLine();
                if (quitAction != null)
                {
                    Console.WriteLine("Q: Quit");
                    Console.WriteLine();
                }

                var input = Console.ReadLine();

                if (input.ToLower().StartsWith("q") && quitAction != null)
                {
                    quitAction.Invoke();
                    break;
                }

                bool validInt = int.TryParse(Regex.Replace(input ?? string.Empty, "[^.0-9]", ""), out int answer);
                if (!validInt) { continue; }

                var selectedChoice = choicesWithIndex.FirstOrDefault(c => c.Index == answer);
                if (selectedChoice == null) { continue; }

                selectedChoice.Value?.ChoiceAction?.Invoke();
                break;
            }
        }
    }
}
