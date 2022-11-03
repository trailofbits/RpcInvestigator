using Newtonsoft.Json;
using NtApiDotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RpcInvestigator
{
    public class Settings
    {
        private static string m_DefaultPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RpcInvestigator");
        private static string m_DefaultSettingsFileName = "settings.json";
        public string m_DbghelpPath;
        public string m_SymbolPath;
        public TraceLevel m_TraceLevel;

        public Settings()
        {
            if (!Directory.Exists(m_DefaultPath))
            {
                try
                {
                    Directory.CreateDirectory(m_DefaultPath);
                }
                catch (Exception ex) // swallow
                {
                    Trace.TraceError("Unable to create settings directory '" +
                        m_DefaultPath + "': " + ex.Message);
                }
            }

            m_SymbolPath = @"srv*c:\\symbols*https://msdl.microsoft.com/download/symbols";
            m_DbghelpPath = FindDbghelpDll();
            m_TraceLevel = TraceLevel.Info;
        }

        public static void Validate (Settings Object)
        {
            return;
        }

        static public void Save(Settings Object, string Target)
        {
            string target = Target;

            if (string.IsNullOrEmpty(target))
            {
                target = Path.Combine(m_DefaultPath, m_DefaultSettingsFileName);
            }

            string json;
            try
            {
                json = JsonConvert.SerializeObject(Object, Formatting.Indented);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (var sw = new StreamWriter(ms))
                    {
                        sw.Write(json);
                        sw.Flush();
                    }
                }
                File.WriteAllText(target, json);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not serialize the Settings object " +
                    "to JSON:  " +
                    ex.Message);
            }
        }

        static public Settings Load(string Location)
        {
            if (!File.Exists(Location))
            {
                throw new Exception("File does not exist");
            }

            Settings settings;

            try
            {
                var json = File.ReadAllText(Location);
                settings = (Settings)JsonConvert.DeserializeObject(json, typeof(Settings));
                Validate(settings);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not deserialize settings: " + ex.Message);
            }
            return settings;
        }

        static public Settings LoadDefault()
        {
            var target = Path.Combine(m_DefaultPath, m_DefaultSettingsFileName);
            if (!File.Exists(target))
            {
                return new Settings();
            }
            return Load(target);
        }

        static private string FindDbghelpDll()
        {
            string baseDir = "C:\\Program Files (x86)\\Windows Kits\\10\\bin";
            List<string> potentialKits = new List<string>();
            foreach (string dirname in Directory.GetDirectories(baseDir))
            {
                if (Path.GetFileName(dirname).StartsWith("10."))
                {
                    potentialKits.Add(dirname);
                }
            }

            // Sort and then reverse potential kits so that we look at the most recent kit first
            potentialKits.Sort();
            potentialKits.Reverse();
            foreach (string dirname in potentialKits)
            {
                string dll = $"{dirname}\\x64\\dbghelp.dll";
                try
                {
                    FileInfo info = new FileInfo(dll);
                    if (info.Exists && info.Length > 0)
                    {
                        return dll;
                    }
                } catch { }
            }

            // We haven't found a dbghelp.dll module for any installed kits. Final try is to get
            // the debugger's version of dbghelp.dll
            try
            {
                FileInfo info = new FileInfo("C:\\Program Files (x86)\\Windows Kits\\10\\Debuggers\\x64\\dbghelp.dll");
                if (info.Exists && info.Length > 0)
                {
                    return info.FullName;
                }
            } catch { }

            return null;
        }
    }
}
