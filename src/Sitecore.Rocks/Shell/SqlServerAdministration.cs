// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.ServiceProcess;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Shell
{
    [Localizable(false)]
    public class ServiceSwitch
    {
        [NotNull]
        public string CurrentRunningServices
        {
            get
            {
                var r = "";
                var scServicesToRead = ServiceController.GetServices();
                foreach (var scTemp in scServicesToRead)
                {
                    if (scTemp.Status == ServiceControllerStatus.Running)
                    {
                        r += scTemp.ServiceName + ", ";
                    }
                }
                return r;
            }
        }

        [NotNull]
        public string PausedServices
        {
            get
            {
                var r = "";
                var scServicesToRead = ServiceController.GetServices();
                foreach (var scTemp in scServicesToRead)
                {
                    if (scTemp.Status == ServiceControllerStatus.Paused)
                    {
                        r += scTemp.ServiceName + @", ";
                    }
                }
                return r;
            }
        }

        public bool SqlExpressServerIsStopped
        {
            get
            {
                var stopped = false;
                var scServicesToRead = ServiceController.GetServices();
                foreach (var scTemp in scServicesToRead)
                {
                    if (scTemp.ServiceName == "MSSQL$SQLEXPRESS")
                    {
                        if (scTemp.Status == ServiceControllerStatus.Stopped)
                        {
                            stopped = true;
                        }
                        else
                        {
                            stopped = false;
                        }
                    }
                }
                return stopped;
            }
        }

        public bool SqlServerIsStopped
        {
            get
            {
                var stopped = false;
                var scServicesToRead = ServiceController.GetServices();
                foreach (var scTemp in scServicesToRead)
                {
                    if (scTemp.ServiceName == @"MSSQLSERVER")
                    {
                        if (scTemp.Status == ServiceControllerStatus.Stopped)
                        {
                            stopped = true;
                        }
                        else
                        {
                            stopped = false;
                        }
                    }
                }
                return stopped;
            }
        }

        [NotNull]
        public string StoppedServices
        {
            get
            {
                var r = "";
                var scServicesToRead = ServiceController.GetServices();
                foreach (var scTemp in scServicesToRead)
                {
                    if (scTemp.Status == ServiceControllerStatus.Stopped)
                    {
                        r += scTemp.ServiceName + ", ";
                    }
                }

                return r;
            }
        }

        public void PauseAllSqlServices(bool showErrorMessage)
        {
            var scServicesToStart = ServiceController.GetServices();

            foreach (var scTemp in scServicesToStart)
            {
                if (scTemp.Status == ServiceControllerStatus.Running)
                {
                    if ((scTemp.ServiceName == "MsDtsServer100") || (scTemp.ServiceName == "MSSQL$SQLEXPRESS") || (scTemp.ServiceName == "MSSQLSERVER") || (scTemp.ServiceName == "MSSQLFDLauncher") || (scTemp.ServiceName == "MSSQLServerOLAPService") || (scTemp.ServiceName == "SQLBrowser") || (scTemp.ServiceName == "SQLWriter"))
                    {
                        try
                        {
                            if (scTemp.CanPauseAndContinue)
                            {
                                scTemp.Pause();
                            }
                        }
                        catch (Exception exc)
                        {
                            if (showErrorMessage)
                            {
                                throw new Exception("Error in Class ServiceSwitch.StartAllSQLServices: " + exc.Message);
                            }
                        }
                    }
                }
            }
        }

        public void RestartPausedSQLServices(bool showErrorMessage)
        {
            var scServicesToStart = ServiceController.GetServices();
            foreach (var scTemp in scServicesToStart)
            {
                if (scTemp.Status == ServiceControllerStatus.Paused)
                {
                    if ((scTemp.ServiceName == "MsDtsServer100") || (scTemp.ServiceName == "MSSQL$SQLEXPRESS") || (scTemp.ServiceName == "MSSQLSERVER") || (scTemp.ServiceName == "MSSQLFDLauncher") || (scTemp.ServiceName == "MSSQLServerOLAPService") || (scTemp.ServiceName == "SQLBrowser") || (scTemp.ServiceName == "SQLWriter"))
                    {
                        try
                        {
                            scTemp.Continue();
                        }
                        catch (Exception exc)
                        {
                            if (showErrorMessage)
                            {
                                throw new Exception("Error in Class ServiceSwitch.StartAllSQLServices: " + exc.Message);
                            }
                        }
                    }
                }
            }
        }

        public void StartAllSQLServices(bool showErrorMessage)
        {
            var scServicesToStart = ServiceController.GetServices();
            foreach (var scTemp in scServicesToStart)
            {
                if (scTemp.Status == ServiceControllerStatus.Stopped)
                {
                    if ((scTemp.ServiceName == "MsDtsServer100") || (scTemp.ServiceName == "MSSQL$SQLEXPRESS") || (scTemp.ServiceName == "MSSQLSERVER") || (scTemp.ServiceName == "MSSQLFDLauncher") || (scTemp.ServiceName == "MSSQLServerOLAPService") || (scTemp.ServiceName == "SQLBrowser") || (scTemp.ServiceName == "SQLWriter"))
                    {
                        try
                        {
                            scTemp.Start();
                        }
                        catch (Exception exc)
                        {
                            if (showErrorMessage)
                            {
                                throw new Exception("Error in Class ServiceSwitch.StartAllSQLServices: " + exc.Message);
                            }
                        }
                    }
                }
            }
        }

        public void StartSQLExpressServiceIfStopped(bool showErrorMessage)
        {
            var scServicesToStart = ServiceController.GetServices();
            foreach (var scTemp in scServicesToStart)
            {
                if (scTemp.Status == ServiceControllerStatus.Stopped)
                {
                    if (scTemp.ServiceName == "MSSQL$SQLEXPRESS")
                    {
                        try
                        {
                            scTemp.Start();
                        }
                        catch (Exception exc)
                        {
                            if (showErrorMessage)
                            {
                                throw new Exception("Error in Class ServiceSwitch.StartAllSQLServices: " + exc.Message);
                            }
                        }
                    }
                }
            }
        }

        public void StopAllServices(bool showErrorMessage)
        {
            var scServicesToStop = ServiceController.GetServices();
            foreach (var scTemp in scServicesToStop)
            {
                if (scTemp.Status == ServiceControllerStatus.Running)
                {
                    try
                    {
                        scTemp.Stop();
                    }
                    catch (Exception exc)
                    {
                        if (showErrorMessage)
                        {
                            throw new Exception("Error in Class ServiceSwitch.StopAllSQLServices " + exc.Message);
                        }
                    }
                }
            }
        }

        public void StopAllSQLServices(bool showErrorMessage)
        {
            var scServicesToStop = ServiceController.GetServices();
            foreach (var scTemp in scServicesToStop)
            {
                if (scTemp.Status == ServiceControllerStatus.Running)
                {
                    if ((scTemp.ServiceName == "MsDtsServer100") || (scTemp.ServiceName == "MSSQL$SQLEXPRESS") || (scTemp.ServiceName == "MSSQLSERVER") || (scTemp.ServiceName == "MSSQLFDLauncher") || (scTemp.ServiceName == "MSSQLServerOLAPService") || (scTemp.ServiceName == "SQLBrowser") || (scTemp.ServiceName == "SQLWriter"))
                    {
                        try
                        {
                            scTemp.Stop();
                        }
                        catch (Exception exc)
                        {
                            if (showErrorMessage)
                            {
                                throw new Exception("Error in Class ServiceSwitch.StopAllSQLServices " + exc.Message);
                            }
                        }
                    }
                }
            }
        }

        public bool StopAndStartSQLServices(bool showMessages)
        {
            var done = false;
            try
            {
                StopAllSQLServices(showMessages);
                StartAllSQLServices(showMessages);
                done = true;
            }
            catch (Exception exc)
            {
                if (showMessages)
                {
                    throw new Exception("ServiceSwitch.StopAndStartSQLServices: " + exc.Message);
                }
            }
            return done;
        }

        public void StopOnlySQLMainServices(bool showErrorMessage)
        {
            var scServicesToStop = ServiceController.GetServices();
            foreach (var scTemp in scServicesToStop)
            {
                if (scTemp.Status == ServiceControllerStatus.Running)
                {
                    if ((scTemp.ServiceName == "MSSQL$SQLEXPRESS") || (scTemp.ServiceName == "MSSQLSERVER"))
                    {
                        try
                        {
                            scTemp.Stop();
                        }
                        catch (Exception exc)
                        {
                            if (showErrorMessage)
                            {
                                throw new Exception("Error in Class ServiceSwitch.StopAllSQLServices " + exc.Message);
                            }
                        }
                    }
                }
            }
        }
    }
}
