using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Input;
using Windows.UI.Input.Preview.Injection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace InjectedPenPressure
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        InputInjector inputInjector;
        public MainPage()
        {
            this.InitializeComponent();

            // Initialize input injection for pen
            inputInjector = InputInjector.TryCreate();
            inputInjector.InitializePenInjection(InjectedInputVisualizationMode.Default);
        }

        // Inputs from topGrid are transferred to bottomGrid in this method
        void ProcessTopGridInput(PointerRoutedEventArgs args)
        {
            var point = args.GetCurrentPoint(topGrid);
            SetPressureText(point, topPressureRun);

            // Calculate the actual position of bottomGrid on the monitor, so that pen inputs can injected onto bottomGrid
            var bottomGridPointerPosition = GetBottomGridPointerPosition(point.Position);

            // Initialize the InjectedInputPenInfo using the pressure from topGrid's pointer input
            var injectedPenInfo = new InjectedInputPenInfo()
            {
                PenParameters = InjectedInputPenParameters.Pressure,
                Pressure = point.Properties.Pressure,
                PointerInfo = new InjectedInputPointerInfo()
                {
                    PointerId = point.PointerId,
                    PointerOptions = (point.IsInContact) ? InjectedInputPointerOptions.InContact : InjectedInputPointerOptions.None,
                    PixelLocation = new InjectedInputPoint() { PositionX = (int)bottomGridPointerPosition.X, PositionY = (int)bottomGridPointerPosition.Y }
                }
            };

            inputInjector.InjectPenInput(injectedPenInfo);

        }

        void ProcessBottomGridInput(PointerRoutedEventArgs args)
        {
            var point = args.GetCurrentPoint(bottomGrid);
            SetPressureText(point, bottomPressureRun);
        }

        void SetPressureText(PointerPoint point, Run pressureRun)
        {
            if (point.IsInContact)
                pressureRun.Text = point.Properties.Pressure.ToString();
            else ResetPressureText(pressureRun);
        }

        void ResetPressureText(Run pressureRun)
        {
            pressureRun.Text = "0";
        }

        Point GetBottomGridPointerPosition(Point relativePosition)
        {
            var bottomGridPosition = bottomGrid.TransformToVisual(this).TransformPoint(new Point(0, 0));
            var windowBounds = Window.Current.Bounds;
            return new Point(bottomGridPosition.X + windowBounds.X + relativePosition.X, bottomGridPosition.Y + windowBounds.Y + relativePosition.Y);
        }

        private async void manualInputButton_Click(object sender, RoutedEventArgs e)
        {
            // Get a point at the center of bottomGrid, this is the position the pen input will be injected at
            var position = GetBottomGridPointerPosition(new Point(bottomGrid.ActualWidth / 2, bottomGrid.ActualHeight / 2));

            var injectedPenInfo = new InjectedInputPenInfo()
            {
                PenParameters = InjectedInputPenParameters.Pressure,
                Pressure = pressureSlider.Value,
                PointerInfo = new InjectedInputPointerInfo()
                {
                    PointerId = 35254,
                    PointerOptions = InjectedInputPointerOptions.InContact,
                    PixelLocation = new InjectedInputPoint() { PositionX = (int)position.X, PositionY = (int)position.Y }
                }
            };

            inputInjector.InjectPenInput(injectedPenInfo);

            // Release the pen input after a second
            await Task.Delay(1000);

            injectedPenInfo = new InjectedInputPenInfo()
            {
                PenParameters = InjectedInputPenParameters.Pressure,
                Pressure = pressureSlider.Value,
                PointerInfo = new InjectedInputPointerInfo()
                {
                    PointerId = 35254,
                    PixelLocation = new InjectedInputPoint() { PositionX = (int)position.X, PositionY = (int)position.Y }
                }
            };

            inputInjector.InjectPenInput(injectedPenInfo);
        }

        #region Input Events
        private void topGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            ProcessTopGridInput(e);
        }

        private void topGrid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            ProcessTopGridInput(e);
        }

        private void topGrid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ResetPressureText(topPressureRun);
        }

        private void topGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ResetPressureText(topPressureRun);
        }

        private void bottomGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            ProcessBottomGridInput(e);
        }

        private void bottomGrid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            ProcessBottomGridInput(e);
        }

        private void bottomGrid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ResetPressureText(bottomPressureRun);
        }

        private void bottomGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ResetPressureText(bottomPressureRun);
        }
        #endregion

        string GetPressureText(double pressure)
        {
            if (pressure <= 1)
            {
                return $"Pressure: {pressure}";
            }
            else
            {
                return $"Pressure: {pressure} (injecting a pressure value greater than 1 will create an exception)";
            }
        }
        
    }
}
