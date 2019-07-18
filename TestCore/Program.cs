using System;
using Component.Extension;
namespace TestCore
{
    class Program
    {
        static void Main(string[] args)
        {

          var redispath= ConfigurationManager.GetSectionValue("RedisPath");


            Console.WriteLine("Hello World!");
        }
    }
}
