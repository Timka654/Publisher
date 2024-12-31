using NSL.SocketCore.Utils.Logger;
using NSL.Utils.CommandLine.CLHandles.Arguments;
using System;

namespace ServerPublisher.Shared.Utils
{
    public static class CommandParameterReader
    {
        public static TResult Read<TResult>(string text, IBasicLogger logger, TResult defaultValue = default)
            where TResult : IConvertible
        {
            var haveDefaultValue = !Equals(defaultValue, default(TResult));
            if (haveDefaultValue)
                text = $"{text} (default: {defaultValue})";

            while (true)
            {
                Console.Write($"{text}: ");

                var answer = Console.ReadLine();

                if (string.IsNullOrEmpty(answer) && haveDefaultValue)
                {
                    answer = defaultValue.ToString();
                }

                TResult result = default;

                try
                {
                    result = (TResult)Convert.ChangeType(answer, typeof(TResult));
                }
                catch (Exception)
                {
                    logger.Append(NSL.SocketCore.Utils.Logger.Enums.LoggerLevel.Error, $"Error, cannot convert \"{answer}\" input to type {typeof(TResult).Name}");
                    continue;
                }


                while (true)
                {
                    logger.Append(NSL.SocketCore.Utils.Logger.Enums.LoggerLevel.Info, $"Value set to \"{answer}\" (y - continue, n - cancel, c - close)");

                    answer = Console.ReadLine().Trim();

                    if (string.Equals(answer, "y")) return result;
                    else if (string.Equals(answer, "c")) Environment.Exit(0);
                    else if (string.Equals(answer, "n")) break;
                    else logger.Append(NSL.SocketCore.Utils.Logger.Enums.LoggerLevel.Error, $"Invalid value {answer}");
                }
            }
        }

        public static bool CheckCommandHaveFlag(this CLArgumentValues values, IBasicLogger logger, string key)
        {
            if (values.ContainsArg(key))
            {
                logger.Append(NSL.SocketCore.Utils.Logger.Enums.LoggerLevel.Info, $"flag \"{key}\" = y");

                return true;
            }

            logger.Append(NSL.SocketCore.Utils.Logger.Enums.LoggerLevel.Info, $"\"{key}\" = n");

            return false;
        }

        public static bool ConfirmCommandAction(this CLArgumentValues args, IBasicLogger logger)
        {
            if (args.TryGetValue("flags", out string flags))
            {
                if (flags.Contains("y", StringComparison.OrdinalIgnoreCase))
                {
                    logger.Append(NSL.SocketCore.Utils.Logger.Enums.LoggerLevel.Info, $"Flags contains 'y' - confirm action");
                    return true;
                }
            }

            if (CheckCommandHaveFlag(args, logger, "y"))
            {
                logger.Append(NSL.SocketCore.Utils.Logger.Enums.LoggerLevel.Info, $"Flags contains 'y' - confirm action");
                return true;
            }

            string latestInput = default;

            do
            {
                logger.Append(NSL.SocketCore.Utils.Logger.Enums.LoggerLevel.Info, "You confirm action? 'y' - yes/'n' - no:");

                latestInput = Console.ReadLine();

                if (latestInput.Equals("y", StringComparison.OrdinalIgnoreCase))
                    return true;
                else if (latestInput.Equals("n", StringComparison.OrdinalIgnoreCase))
                    return false;
                else
                    logger.Append(NSL.SocketCore.Utils.Logger.Enums.LoggerLevel.Error, $"Value cannot be {latestInput}. Try again or press Ctrl+C for cancel");

            } while (true);
        }
    }
}