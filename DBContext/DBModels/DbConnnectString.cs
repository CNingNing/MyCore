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
            var path = AppContext.BaseDirectory;
           
            var filename = $"{path}\\Configuration\\database.json";
            _configuration = new ConfigurationBuilder().AddJsonFile(filename, false, true).Build();
            return _configuration.GetSection($"{dbname}:ConnnectString").Value;
        }

    }
}
