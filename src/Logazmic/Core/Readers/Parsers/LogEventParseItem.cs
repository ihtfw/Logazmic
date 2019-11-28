namespace Logazmic.Core.Readers.Parsers
{
    public struct LogEventParseItem
    {
        public LogEventParseItem(int startIndex, int length)
        {
            StartIndex = startIndex;
            Length = length;
        }

        public int StartIndex { get; }
        public int Length { get; }
        
        public override string ToString()
        {
            return $"{StartIndex}:{Length}";
        }
    }
}