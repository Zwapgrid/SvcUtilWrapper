using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using SvcUtilWrapper.Properties;

namespace SvcUtilWrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            var appSettings = AppSettings.Default;

            var svcUtilsPath = appSettings.SvcUtilsPath;

            if (string.IsNullOrEmpty(svcUtilsPath))
                throw new Exception("Cannot find SvcUtil path in app settings.");

            var wsdlPath = appSettings.WsdlPath;
            var outputFilePath = $"/out:{appSettings.OutputFilePath}";
            var ns = $"/namespace:*,{appSettings.Namespace}";
            var configFileName = $"/config:{appSettings.ConfigFileName}";

            var t = "/t:code";
            var s = "/s";
            var mc = "/mc";

            string[] parameters = { wsdlPath, t, outputFilePath, ns, s, mc, configFileName };

            try
            {
                // Magic goes here
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var svcUtilAssembly = Assembly.LoadFile(svcUtilsPath);
                svcUtilAssembly.EntryPoint.Invoke(null, new object[] { parameters });

                RemoveFaultContractAttributes();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            Console.WriteLine("Successfully generated client");
        }

        static void RemoveFaultContractAttributes()
        {
            var generated = File.ReadAllText(AppSettings.Default.OutputFilePath);
            generated = new Regex(@".*\[System.ServiceModel.FaultContractAttribute.*\n").Replace(generated, string.Empty);
            File.WriteAllText(AppSettings.Default.OutputFilePath, generated);
        }
    }
}
