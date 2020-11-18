using FrameworkCore.Metadata.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace FrameworkCore.Service
{
    public class DatabaseService
    {
        public static bool InitDatabase()
        {
            try
            {
                return DbServiceProvider.Initialize();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
