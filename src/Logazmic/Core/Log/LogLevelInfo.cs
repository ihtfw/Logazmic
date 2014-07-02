namespace Logazmic.Core.Log
{
    using System;

    [Serializable]
    public class LogLevelInfo
    {
        public LogLevel Level;
        public string Name;
        public int Value;
        public int RangeMin;
        public int RangeMax;


        public LogLevelInfo(LogLevel level)
        {
            Level = level;
            Name = level.ToString();
            RangeMax = RangeMin = 0;
        }

        public LogLevelInfo(LogLevel level, string name,int value, int rangeMin, int rangeMax)
        {
            Level = level;
            Name = name;
            Value = value;
            RangeMin = rangeMin;
            RangeMax = rangeMax;
        }

        public override bool Equals(object obj)
        {
            if (obj is LogLevelInfo)
                return ((obj as LogLevelInfo) == this);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(LogLevelInfo first, LogLevelInfo second)
        {
            if (((object)first == null) || ((object)second == null))
                return (((object)first == null) && ((object)second == null));
            return (first.Value == second.Value);
        }

        public static bool operator !=(LogLevelInfo first, LogLevelInfo second)
        {
            if (((object)first == null) || ((object)second == null))
                return !(((object)first == null) && ((object)second == null));
            return first.Value != second.Value;
        }
    }
}