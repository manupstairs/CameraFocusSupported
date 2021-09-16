using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CameraFocusSupported
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page , INotifyPropertyChanged
    {
        public MediaCapture captureManager;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsSupported { get; private set; } = true;

        public ObservableCollection<string> Capabilities { get; set; } = new ObservableCollection<string>();

        public MainPage()
        {
            this.InitializeComponent();
            
            InitializeAsync();
        }

        private async Task<DeviceInformation> FindCameraDevicesAsync(Windows.Devices.Enumeration.Panel panel)
        {
            var videoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            return videoDevices.FirstOrDefault(d => d.EnclosureLocation != null && d.EnclosureLocation.Panel == panel);
        }

        async private void InitializeAsync()
        {
            captureManager = new MediaCapture();
            var cameraDevice = await FindCameraDevicesAsync(Windows.Devices.Enumeration.Panel.Front);
            var settings = new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.Video,
                VideoDeviceId = cameraDevice.Id
            };

            await captureManager.InitializeAsync(settings);

            IsSupported = captureManager.VideoDeviceController.FocusControl.Supported;
            this.OnPropertyChanged(nameof(IsSupported));

            foreach (var property in captureManager.VideoDeviceController.Focus.Capabilities.GetType().GetProperties(BindingFlags.Public| BindingFlags.Instance| BindingFlags.DeclaredOnly))
            {
                Capabilities.Add(property.Name);
                Capabilities.Add(property.GetValue(captureManager.VideoDeviceController.Focus.Capabilities).ToString());
            }

            this.OnPropertyChanged(nameof(Capabilities));

            capturePreview.Source = captureManager;
            await captureManager.StartPreviewAsync();
        }

        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        
    }
}
