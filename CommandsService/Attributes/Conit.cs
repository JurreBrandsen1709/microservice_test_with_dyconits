using System.Net;
namespace CommandsService.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class Conit : Attribute
    {
        public double numericalErrorThreshold { get; set; } = 0.0;
        public int orderErrorThreshold { get; set; } = 0;
        public int stalenessThreshold { get; set; } = 0;
    }
}