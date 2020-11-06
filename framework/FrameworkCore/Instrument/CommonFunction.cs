using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkCore.Instrument
{
    public static class CommonFunction
    {
        public static void SearchAllTypeInAssembly(Action<Type> action)
        {
            {
                SearchTypeOnCurrentDirectoryAndSubdirectory(Environment.CurrentDirectory);

                void SearchTypeOnCurrentDirectoryAndSubdirectory(string currentDirectory)
                {
                    try
                    {
                        foreach (string file in Directory.GetFiles(currentDirectory).Where(t => t.EndsWith(".dll") || t.EndsWith(".exe")))
                        {
                            foreach (Type type in Assembly.LoadFile(file).GetTypes())
                            {
                                action(type);
                            }
                        }

                        foreach (string subDirectory in Directory.GetDirectories(currentDirectory))
                        {
                            SearchTypeOnCurrentDirectoryAndSubdirectory(subDirectory);
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }

        public static Type GetTypeByBaseTypeAndTypeName(Type baseType, string typeName)
        {
            Type ret = SearchTypeOnCurrentDirectoryAndSubdirectory(Environment.CurrentDirectory);
            if (ret != null) { return ret; }
            throw new ApplicationException($"Can't find Type：{typeName}");

            Type SearchTypeOnCurrentDirectoryAndSubdirectory(string currentDirectory)
            {
                try
                {
                    foreach (string file in Directory.GetFiles(currentDirectory).Where(t => t.EndsWith(".dll")))// || t.EndsWith(".exe")))
                    {
                        foreach (Type type in Assembly.LoadFile(file).GetTypes())
                        {
                            if (type.IsSubclassOf(baseType) && type.FullName.EndsWith(typeName))
                            {
                                return type;
                            }
                        }
                    }

                    foreach (string subDirectory in Directory.GetDirectories(currentDirectory))
                    {
                        ret = SearchTypeOnCurrentDirectoryAndSubdirectory(subDirectory);
                        if (ret != null)
                        {
                            return ret;
                        }
                    }

                }
                catch (Exception)
                {
                    throw;
                }

                return null;
            }
        }

        public static async Task<Dictionary<string, Type>> GetAllUnabstractTypeAndInheritFromBaseTypeAsync(Type baseType)
        {
            Dictionary<string, Type> ret = new Dictionary<string, Type>();

            await Task.Run(() => { SearchTypeOnCurrentDirectoryAndSubdirectory(Environment.CurrentDirectory); });

            void SearchTypeOnCurrentDirectoryAndSubdirectory(string currentDirectory)
            {
                foreach (string file in Directory.GetFiles(currentDirectory).Where(t => t.EndsWith(".dll") || t.EndsWith(".exe")))
                {
                    foreach (Type type in Assembly.LoadFile(file).GetTypes())
                    {
                        if (!type.IsAbstract && type.IsSubclassOf(baseType))
                        {
                            ret.Add(type.Name, type);
                        }
                    }
                }

                foreach (string subDirectory in Directory.GetDirectories(currentDirectory))
                {
                    SearchTypeOnCurrentDirectoryAndSubdirectory(subDirectory);
                }
            }

            return ret;
        }


    }
}
