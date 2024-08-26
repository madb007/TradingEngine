namespace LoggingCS.LoggingConfiguration
{
    internal class LoggerConfiguration
    {
        public LoggerType LoggerType { get; set; };
        //public ConsoleLoggerConfiguration ConsoleLoggerConfiguration {get; set };
        public TextLoggerConfiguration TextLoggerConfiguration {  get; set; }
    }
    public class ConsoleLoggerConfiguration
    {

    }

    class TextLoggerConfiguration
    {
        public string Directory { get; set; }
        public string Filename { get; set; }
        public string FileExtension { get; set; }
    }
}
