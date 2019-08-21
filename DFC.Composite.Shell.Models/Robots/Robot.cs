using System.Text;

namespace DFC.Composite.Shell.Models.Robots
{
    public class Robot
    {
        private readonly StringBuilder robotData;

        public Robot()
        {
            robotData = new StringBuilder();
        }

        public string Data => robotData.ToString();

        public void Append(string text)
        {
            robotData.AppendLine(text);
        }
    }
}