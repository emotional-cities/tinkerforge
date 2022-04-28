using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Bonsai.Expressions;
using Tinkerforge;
using System.Threading;
using System.Diagnostics;

namespace Bonsai.Tinkerforge
{
    internal class BrickletDeviceNameConverter : TypeConverter
    {
        public List<DeviceData> devices;
        //private Stopwatch stopwatch;

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// For Bricklets, we have to specify a UID to connect to. To get that, we need to know what IP connections are present,
        /// and what UIDs are present on those connections
        /// </summary>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context != null)
            {
                // We need to know which CreateIPConnection nodes are present, to know what channels we should look for devices on
                // Therefore we search the workflow for CreateIPConnection nodes. From each node we extract the Host / Port
                var workflowBuilder = (WorkflowBuilder)context.GetService(typeof(WorkflowBuilder));
                if (workflowBuilder != null)
                {
                    var connectionIDs = (from builder in workflowBuilder.Workflow.Descendants()
                                         let createIP = ExpressionBuilder.GetWorkflowElement(builder) as CreateIPConnection
                                         where createIP != null
                                         select new ConnectionID { HostName = createIP.HostName, Port = createIP.Port })
                                         .Distinct().ToList();

                    // For each IP connection, we search the connected devices and add it to the list - TODO test for multiple IP
                    devices = new List<DeviceData>();
                    foreach (ConnectionID connectionID in connectionIDs)
                    {
                        var ipcon = new IPConnection();
                        ipcon.EnumerateCallback += EnumerateConnection;
                        ipcon.Connect(connectionID.HostName, connectionID.Port);
                        ipcon.Enumerate();
                        /// N.B. GetStandardValues is called twice by Windows Forms. Once to check the dropdown list and then again
                        /// to check cursor position in the list. This is annoying here because it means the enumerate thread will
                        /// be called twice leading to e.g. the device list being cleared in the middle of population. The 'hack' 
                        /// here is to sleep the thread so it can complete before the 2nd call. Not optimal but the Enumerate 
                        /// method doesn't give us much choice. TODO - look for more elegant solution
                        Thread.Sleep(10);
                        ipcon.Disconnect();
                    }
                }
                return new StandardValuesCollection(devices.Select(x => x.UID).ToList());
            }

            return base.GetStandardValues(context);
        }

        /// <summary>
        /// EnumerateCallback used to add devices to the device list when IPConnection.Enumerate() is called
        /// </summary>
        private void EnumerateConnection(IPConnection sender, string uid, string connectedUid, char position,
            short[] hardwareVersion, short[] firmwareVersion, int deviceIdentifier, short enumerationType)
        {
            devices.Add(new DeviceData { UID = uid, 
                                         ConnectedUID = connectedUid, 
                                         Position = position, 
                                         DeviceIdentifier = deviceIdentifier });

            Console.WriteLine(deviceIdentifier);
        }

        // TODO - move this to its own file if it needs to be reused across modules
        public struct ConnectionID
        {
            public string HostName;
            public int Port;

            public override string ToString()
            {
                return $"{HostName}:{Port}";
            }
        }

        /// <summary>
        /// Data representation of a connected TinkerForge module
        /// </summary>
        public class DeviceData
        {
            public string UID; // Unique module ID
            public string ConnectedUID; // IDs of connected modules
            public char Position; // Position in the network
            public int DeviceIdentifier; // Number corresponding to device name
        }

        // Goncalo's method of doing the above, also works and probably more scaleable for more complex workflows
        //public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        //{
        //    // TODO - some of these need to go into a helper class for dealing with grabbing connected stuff
        //    // Here we grab all the existing Tinkerforge IPConnection nodes as output. TODO - should give the associated UIDs for those connections
        //    if (context != null)
        //    {
        //        if (stopwatch == null || stopwatch.ElapsedMilliseconds > 1000)
        //        {
        //            var workflowBuilder = (WorkflowBuilder)context.GetService(typeof(WorkflowBuilder));
        //            if (workflowBuilder != null)
        //            {
        //                stopwatch = Stopwatch.StartNew();
        //                Console.WriteLine("request");
        //                // Get the distinct connection specs (host / port)
        //                var connectionIDs = (from builder in workflowBuilder.Workflow.Descendants()
        //                                     let createIP = ExpressionBuilder.GetWorkflowElement(builder) as CreateIPConnection
        //                                     where createIP != null
        //                                     select new ConnectionID { HostName = createIP.HostName, Port = createIP.Port })
        //                                 .Distinct().ToList();

        //                // Test uid
        //                devices = new List<string>();
        //                var ipcon = new IPConnection();
        //                ipcon.Connect("localhost", 4223);

        //                ipcon.EnumerateCallback += EnumerateConnection;
        //                ipcon.Enumerate();
        //                Thread.Sleep(100);

        //                ipcon.Disconnect();
        //            }
        //        }

        //        return new StandardValuesCollection(devices);
        //    }

        //    return base.GetStandardValues(context);
        //}
    }
}
