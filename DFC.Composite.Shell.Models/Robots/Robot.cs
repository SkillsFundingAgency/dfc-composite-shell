using System;
using System.Collections.Generic;

namespace DFC.Composite.Shell.Models.Robots
{
    public class Robot
    {
        public Robot()
        {
            Lines = new List<string>();
        }

        public Robot(string text)
        {
            Lines = new List<string>
            {
                text
            };
        }

        public string Data => string.Join(Environment.NewLine, Lines);

        public List<string> Lines { get; }

        public void Append(string text)
        {
            Lines.AddRange(text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}