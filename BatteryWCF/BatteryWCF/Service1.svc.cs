using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Diagnostics;
using System.Configuration;
using System.Windows.Forms;
using System.Management;
namespace BatteryWCF
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IService1
    {
        PerformanceCounter cpuCounter;
        PerformanceCounter ramCounter;
        /// <summary>
        /// Private Methods
        /// </summary>
        /// <returns></returns>
        private string getCurrentCpuUsage()
        {
            cpuCounter.BeginInit();
            return cpuCounter.NextValue() + "%";
        }

        private string getAvailableRAM()
        {
            return ramCounter.NextValue() + "MB";
        }
        private string getStatusofBattery()
        {
            PowerStatus ps = SystemInformation.PowerStatus;
            String typeofCharging = "";
            String status = "";
            if (PowerLineStatus.Online == ps.PowerLineStatus)
            {
                status += "Adapter:";
            }
            else
            {
                status += "Battery:";
            }
            float percentage = ps.BatteryLifePercent * 100;
            typeofCharging += "<table style=\"width: 100%;\"  border=\"1\">  <tr> <th>Property Name</th> <th>Value</th> </tr>";

            typeofCharging += string.Format("<tr> <td>{0}</td> <td>{1}</td> </tr>", status, percentage);
            typeofCharging += string.Format("<tr> <td>{0}</td> <td>{1}</td> </tr>", "Battery Charged Status", ps.BatteryChargeStatus);
            typeofCharging += string.Format("<tr> <td>{0}</td> <td>{1}</td> </tr>", "Battery FullLifeTime", ps.BatteryFullLifetime);
            typeofCharging += string.Format("<tr> <td>{0}</td> <td>{1}</td> </tr>", "Battery Life Remaining", ps.BatteryLifeRemaining);
            typeofCharging += "</table>";
            //////////////////////////////////////////////////////////////////////////////////////////////
            typeofCharging += "</br> <h2> Battery Report </h2> </br>";
            typeofCharging += "<table style=\"width: 100%;\"  border=\"1\">  <tr> <th>Property Name</th> <th>Value</th> </tr>";

            ///SELECT * FROM Win32_PerfFormattedData_PerfProc_Process
            ////Select * FROM Win32_Battery
            System.Management.ObjectQuery query = new ObjectQuery("Select * FROM Win32_Battery");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

            ManagementObjectCollection collection = searcher.Get();

            foreach (ManagementObject mo in collection)
            {
                foreach (PropertyData property in mo.Properties)
                {
                    //Property {0}: Value is {1} </br>
                    typeofCharging += string.Format("<tr> <td>{0}</td> <td>{1}</td> </tr>", property.Name, property.Value);
                }
            }
            typeofCharging += "</table>";
            //////////////////////////////////////////////////////////

            cpuCounter = new PerformanceCounter();

            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";

            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            /////////////////////////////////////////////////////////

            typeofCharging += "<table style=\"width: 100%;\"  border=\"1\">  <tr> <th>Property Name</th> <th>Value</th> </tr>";
            typeofCharging += string.Format("<tr> <td>{0}</td> <td>{1}</td> </tr>", "CPU Load", getCurrentCpuUsage());
            typeofCharging += string.Format("<tr> <td>{0}</td> <td>{1}</td> </tr>", "Memory Remaing (MB)", getAvailableRAM());
            typeofCharging += "</table>";
            ////////////////////////////////////////////////////////////////////////////////////

            typeofCharging += "</br> <h2> Show All Processes and the CPU Load </h2> </br>";
            typeofCharging += "<table style=\"width: 100%;\"  border=\"1\">  <tr> <th>Process Name</th> <th>CPU Load</th> </tr>";
            foreach (Process process in Process.GetProcesses())
            {
                using (PerformanceCounter pcProc = new PerformanceCounter("Process", "% Processor Time", process.ProcessName))
                {
                    try
                    {
                        pcProc.NextValue();
                        typeofCharging += string.Format("<tr> <td>{0}</td> <td>{1}</td> </tr>", process.ProcessName, pcProc.NextValue());
                    }
                    catch
                    {
                    }
                }
            }
            typeofCharging += "</table>";

            ///////////////////////////////////////////////////////////////////////////////////////
            typeofCharging += "</br> <h2> Current Process </h2> </br>";
            typeofCharging += "<table style=\"width: 100%;\"  border=\"1\">  <tr> <th>Process Name</th> <th>CPU Load</th> </tr>";
            Process p = Process.GetCurrentProcess();

            using (PerformanceCounter pcProcess = new PerformanceCounter("Process", "% Processor Time", p.ProcessName))
            {
                typeofCharging += string.Format("<tr> <td>{0}</td> <td>{1}</td> </tr>", p.ProcessName, pcProcess.NextValue());
            }
            typeofCharging += "</table>";
            ///////////////////////////////////////////////////////////////////////////////////////
            return typeofCharging;

        }

        /// <summary>
        /// Public Methods Exposed by WebService
        /// </summary>
        /// <returns></returns>
        public string GetBatteryStatus()
        {
            return getStatusofBattery();
        }






        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }
    }
}
