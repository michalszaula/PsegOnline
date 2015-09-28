using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using System.ServiceModel;
using System.ServiceProcess;
using System.Configuration;
using System.Configuration.Install;

using System.Data.SqlClient;
using PsegOnlineWcfCommunicationService.Models;
using PsegOnlineWcfCommunicationService;
using System.ServiceModel.Description;
using System.Diagnostics;

namespace PsegOnlineCommunicationServiceConsoleApplication
{
    
    

    public class PsegServices : ServiceBase
    {
        public ServiceHost serviceHost = null;
        Uri baseAddress = new Uri("http://localhost:10000/PsegServices/CommunicationService");
        public PsegServices()
        {
            // Name the Windows Service
            ServiceName = "PsegOnlineCommunicationService";
        }

        public static void Main(string[] args)
        {
            ServiceBase.Run(new PsegServices());
         
        }

        // Start the Windows service.
        protected override void OnStart(string[] args)
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
            }
          
            // Create a ServiceHost for the CalculatorService type and 
            // provide the base address.
            serviceHost = new ServiceHost(typeof(PsegOnlineCommunicationService), baseAddress);
            serviceHost.AddServiceEndpoint(typeof(IPsegOnlineCommunicationService), new BasicHttpBinding(), "basicHttpPsegCommunicationService");
            serviceHost.AddServiceEndpoint(typeof(IPsegOnlineCommunicationService), new NetHttpBinding(), "netHttpPsegCommunicationService");
           
            ServiceMetadataBehavior smb = serviceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (smb == null)
                smb = new ServiceMetadataBehavior();
            smb.HttpGetEnabled = true;
            smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            serviceHost.Description.Behaviors.Add(smb);
            serviceHost.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexHttpBinding(),"mex");



            // Open the ServiceHostBase to create listeners and start 
            // listening for messages.
            serviceHost.Open();
        }

        protected override void OnStop()
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
                serviceHost = null;
            }
        }
    }
    /*install
     C:\Users\Michal\Documents\Visual Studio 2013\Projects\PsegOnlineCommunicationCon
    soleApplication\bin\Release>installutil PsegOnlineCommunicationConsoleApplicatio
    n.exe
     */
    /*uninstall
     C:\Users\Michal\Documents\Visual Studio 2013\Projects\PsegOnlineCommunicationCon
    soleApplication\bin\Release>installutil /u PsegOnlineCommunicationConsoleApplica
    tion.exe
    */
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        private ServiceProcessInstaller projectInstaller;
        private ServiceInstaller serviceInstaller;

        public ProjectInstaller()
        {
            projectInstaller = new ServiceProcessInstaller();
            projectInstaller.Account = ServiceAccount.LocalSystem;
            serviceInstaller = new ServiceInstaller();
            serviceInstaller.ServiceName = "PsegOnlineCommunicationService";
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.AfterInstall+=serviceInstaller_AfterInstall;
            Installers.Add(projectInstaller);
            Installers.Add(serviceInstaller);
        }

        private void serviceInstaller_AfterInstall(object sender, InstallEventArgs ev)
        {

        }

        static void ExecuteCommand(string command)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);

            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine("output>>" + e.Data);
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine("error>>" + e.Data);
            process.BeginErrorReadLine();

            process.WaitForExit();

            Console.WriteLine("ExitCode: {0}", process.ExitCode);
            process.Close();
        }
    }
}
