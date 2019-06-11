using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFC.Composite.Shell.Models.Robots
{
    public class Robot
    {
        private readonly StringBuilder robotData;

        public Robot()
        {
            robotData = new StringBuilder();
        }

        public string Data { get
            {
                return robotData.ToString();
            }
        }

        public void Add(string text)
        {
            robotData.AppendLine(text);
        }
    }
}
