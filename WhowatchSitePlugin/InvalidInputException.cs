using System;
using System.Runtime.Serialization;

namespace WhowatchSitePlugin
{
    [Serializable]
    internal class InvalidInputException : Exception
    {
        public string Input { get; }
        public InvalidInputException(string input)
        {
            Input = input;
        }
    }
}