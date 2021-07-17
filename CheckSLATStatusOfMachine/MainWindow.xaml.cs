using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Management;
using System.Security;
using AppProp = CheckSLATStatusOfMachine.Properties;

namespace CheckSLATStatusOfMachine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnCheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isOSVerWindows8orLater = CheckWin8orLater();

                if (!isOSVerWindows8orLater) // Unsupported OS Version
                {
                    ShowStr(AppProp.Resources.NotWin8, Brushes.Red);
                }
                else // OS Version Windows 8 or Later
                {
                    // First Check if HyperV is running. If yes then HyperV is Supported and Enabled!                    
                    bool isHyperVRunning = false;
                    ManagementClass mgtClass = new ManagementClass("Win32_ComputerSystem");
                    ManagementObjectCollection mftObjCollection = mgtClass.GetInstances();
                    foreach (ManagementObject mgtObj in mgtClass.GetInstances()) // There will be only One object of this class
                    {
                        isHyperVRunning = Convert.ToBoolean(mgtObj["HypervisorPresent"]);
                    }

                    if (isHyperVRunning) // If yes then HyperV is Supported and Enabled!  
                    {
                        ShowStr(AppProp.Resources.SlatEnabled, Brushes.Green);
                    }
                    else
                    {
                        // If HyperV is not Running, then only we Check if its supported and enabled in Bios.
                        // We cannot directly check if HyperV is supported and enabled in Bios because that gives wrong result when HyperV is Running
                        bool isHardwareVirtualizationSupported = false;
                        bool isHardwareVirtualizationEnabledInBIOS = false;
                        mgtClass = new ManagementClass("Win32_Processor");
                        mftObjCollection = mgtClass.GetInstances();
                        foreach (ManagementObject mgtObj in mgtClass.GetInstances()) // There will be only One object of this class
                        {
                            isHardwareVirtualizationSupported = Convert.ToBoolean(mgtObj["SecondLevelAddressTranslationExtensions"]);
                            if (isHardwareVirtualizationSupported)
                                isHardwareVirtualizationEnabledInBIOS = Convert.ToBoolean(mgtObj["VirtualizationFirmwareEnabled"]);
                        }
                        if (!isHardwareVirtualizationSupported)
                        {
                            ShowStr(AppProp.Resources.NotSlatEnabled, Brushes.Red);
                        }
                        else if (isHardwareVirtualizationSupported && isHardwareVirtualizationEnabledInBIOS)
                        {
                            ShowStr(AppProp.Resources.SlatEnabled, Brushes.Green);
                        }
                        else if (isHardwareVirtualizationSupported && !isHardwareVirtualizationEnabledInBIOS)
                        {
                            ShowStr(AppProp.Resources.HWVirtualizationDisabled, Brushes.Green);
                        }
                    }
                }
            }
            catch (SecurityException ex)
            {
                ShowStr(string.Format(AppProp.Resources.SecurityExceptionStr, Environment.NewLine, ex.Message), Brushes.Red);
            }
            catch (UnauthorizedAccessException ex1)
            {
                ShowStr(string.Format(AppProp.Resources.UnauthorizedAccessExceptionStr, Environment.NewLine, ex1.Message), Brushes.Red);
            }
            catch (Exception ex2)
            {
                ShowStr(string.Format(AppProp.Resources.GeneralExceptionStr, Environment.NewLine, ex2.Message), Brushes.Red);
            }

        }
        public static bool CheckWin8orLater()
        {
            bool isOSVerWindows8orLater = false;
            OperatingSystem osInfo = Environment.OSVersion;
            if (((osInfo.Version.Major == 6 && osInfo.Version.Minor >= 2) || osInfo.Version.Major > 6) && osInfo.Platform == PlatformID.Win32NT)
                isOSVerWindows8orLater = true;
            return isOSVerWindows8orLater;
        }

        public void ShowStr(string msg, Brush color)
        {
            Details.Foreground = color;
            Details.Text = msg;
        }
    }
}