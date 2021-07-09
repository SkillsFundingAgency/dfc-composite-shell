using System;
using System.Collections.Generic;

namespace DFC.Composite.Shell.Models.Robots
{
    public class Robot
    {
        public string Data => string.Join(Environment.NewLine, Lines);

        public List<string> Lines { get; } = new List<string>();

        public void Add(string text)
        {
            Lines.Add(text);
        }

        public void Append(string text)
        {
            Lines.AddRange(text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}