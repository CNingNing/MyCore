using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
namespace DBModels
{
    public static class DbConnnectString
    {
        private static IConfiguration _configuration;
        public static string GetDatabase(string dbname)
        {
            return GetConnnectString(dbname);
        }
       public static string GetConnnectString(string dbname)
        {
            var path = AppContext.BaseDirectory;
            if(path.IndexOf("\\bin")>0)
            {
                path = path.Substring(0, path.IndexOf("\\bin"));
            }
            var filename = $"{path}\\database.json";
            _configuration = new ConfigurationBuilder().AddJsonFile(filename, false, true).Build();
            return _configuration.GetSection($"{dbname}:ConnnectString").Value;
        }


    }
}
