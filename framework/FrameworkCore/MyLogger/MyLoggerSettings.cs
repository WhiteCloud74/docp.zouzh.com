using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace FrameworkCore.MyLogger
{
    public class MyLoggerSettings
    {
        readonly IConfiguration _configuration;
        public IChangeToken ChangeToken { get; private set; }

        public MyLoggerSettings(IConfiguration configuration)
        {
            _configuration = configuration;
            ChangeToken = _configuration.GetReloadToken();
        }

        public string DefaultPath
        {
            get
            {
                return _configuration["DefaultPath"];
            }
        }

        public int DefaultMaxMB
        {
            get
            {
                return int.Parse(_configuration["DefaultMaxMB"]);
            }
        }
        public string DefaultFileName
        {
            get { return _configuration["DefaultFileName"]; }
        }

        public void Reload()
        {
            //update cache settings
        }

        public Tuple<bool, LogLevel> GetSwitch(string name)
        {
            var section = _configuration.GetSection("LogLevel");
            if (section != null)
            {
                if (Enum.TryParse(section[name], true, out LogLevel level))
                    return new Tuple<bool, LogLevel>(true, level);
            }
            return new Tuple<bool, LogLevel>(false, LogLevel.None);
        }
        public Tuple<bool, string> GetDiretoryPath(string name)
        {
            var section = _configuration.GetSection("Path");
            if (section != null)
            {
                var path = section[name];
                if (!String.IsNullOrEmpty(path))
                {
                    return new Tuple<bool, string>(true, path);
                }
            }
            return new Tuple<bool, string>(false, DefaultPath);
        }
        public Tuple<bool, string> GetFileName(string name)
        {
            var section = _configuration.GetSection("FileName");
            if (section != null)
            {
                var path = section[name];
                if (!String.IsNullOrEmpty(path))
                {
                    return new Tuple<bool, string>(true, path);
                }
            }
            return new Tuple<bool, string>(false, DefaultFileName);
        }
    }
}
