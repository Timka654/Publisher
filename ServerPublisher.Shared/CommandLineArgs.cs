using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ServerPublisher.Shared
{
    /// <summary>
    /// Very basic Command Line Args extracter
    /// <para>Parse command line args for args in the following format:</para>
    /// <para>/argname:argvalue /argname:argvalue ...</para>
    /// </summary>
    public class CommandLineArgs
    {
        private const string Pattern = @"\/(?<argname>\w+):(?<argvalue>.+)";
        private readonly Regex _regex = new Regex(
            Pattern,
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Dictionary<String, String> _args =
            new Dictionary<String, String>();

        public CommandLineArgs()
        {
            BuildArgDictionary();
        }

        public CommandLineArgs(string[] args)
        {
            Parse(args);
        }

        public string this[string key]
        {
            get
            {
                return _args.ContainsKey(key) ? _args[key] : null;
            }
        }

        public bool ContainsKey(string key)
        {
            return _args.ContainsKey(key);
        }

        public bool TryGetValue<T>(string key, ref T result)
        {
            if (_args.TryGetValue(key, out var text))
            {
                result = (T)Convert.ChangeType(text, typeof(T));
                return true;
            }

            return false;
        }

        public bool TryGetOutValue<T>(string key, out T result)
        {
            result = default;
            return TryGetValue(key, ref result);
        }

        private void BuildArgDictionary()
        {
            Parse(Environment.GetCommandLineArgs());
        }

        private void Parse(string[] args)
        {
            foreach (var match in args.Select(arg =>
                        _regex.Match(arg)).Where(m => m.Success))
            {
                try
                {
                    _args.Add(
                         match.Groups["argname"].Value,
                         match.Groups["argvalue"].Value);
                }
                // Ignore any duplicate args
                catch (Exception) { }
            }
        }

        public KeyValuePair<string, string>[] GetArgs()
        {
            return _args.ToArray();
        }
    }
}
