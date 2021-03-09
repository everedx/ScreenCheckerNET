using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScreenCheckerNET
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        public delegate void UpdateGUI();
        public delegate void UpdateGUIText(string text);

        Bitmap previousImage;
        Bitmap currentImage;

        System.Drawing.Color[,] _first;
        System.Drawing.Color[,] _second;

        System.Drawing.Point pointTopLeft;
        System.Drawing.Point pointBottomRight;

        private bool continueCheck;
        Thread thread;

        Marker marker;

        public MainWindow()
        {
            InitializeComponent();
            continueCheck = false;
           
            thread = new Thread(new ThreadStart(CheckImages));
            marker = new Marker();
            marker.Show();
            marker.SetSize(0, 0);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && !thread.IsAlive)
            {
                if (!e.IsRepeat)
                {
                    pointTopLeft = GetMousePosition();
                    Point1.Content = "X = " + pointTopLeft.X + ", Y = " + pointTopLeft.Y;
                    marker.SetLocation(pointTopLeft.X,pointTopLeft.Y);
                    marker.SetSize(0, 0);
                }
                else
                {
                    System.Drawing.Point pointMouse = GetMousePosition();
                    SizeLabel.Content = "X = " + Math.Abs(pointTopLeft.X - pointMouse.X) + ", Y = " + Math.Abs(pointTopLeft.Y - pointMouse.Y);
                    marker.SetSize(Math.Abs(pointTopLeft.X - pointMouse.X), Math.Abs(pointTopLeft.Y - pointMouse.Y));
                }
                
            }
        }


        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && !thread.IsAlive)
            {
                StartButton.IsEnabled = true;
                pointBottomRight = GetMousePosition();
                Point2.Content = "X = " + pointBottomRight.X + ", Y = " + pointBottomRight.Y;
            }
            SizeLabel.Content = "";
      
        }

        private void Window_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private bool CompareImages(Bitmap b1, Bitmap b2)
        {
            if (b1 != null && b2 != null)
                return CompareMemCmp(b1, b2);
            else
                return true;
            
        }


        private void CheckImages()
        {
            while (continueCheck)
            {
                currentImage = GetScreenCapture();

                if (!CompareImages(currentImage, previousImage))
                {
                    this.Dispatcher.Invoke(
                         new UpdateGUIText(UpdateLogGUI),
                         new object[] { DateTime.Now.ToString("MM_dd_yyyy_hh_mm_ss_fff") + ".png" }
                      );

                    if (!Directory.Exists("Proofs"))
                    {
                        Directory.CreateDirectory("Proofs");
                    }
                    DateTime date = DateTime.Now;
                    //currentImage.Save(date.ToString("MM-dd-yyyy_hh:mm:ss.fff") + ".png", ImageFormat.Png);
                    SaveBMP(currentImage, date.ToString("MM_dd_yyyy_hh_mm_ss_fff") + ".png");

                }


                previousImage = currentImage;

                this.Dispatcher.Invoke(
                         new UpdateGUI(UpdateImageGUI),
                         new object[] { }
                      );
               // Thread.Sleep(50);
            }
           
        }

        private void UpdateImageGUI()
        {
            ScreenShot.Source = BitmapToImageSource(currentImage);
        }

        private void UpdateLogGUI(string text)
        {
            string newText = "Change: " + text + "\n" + Log.Text;
            Log.Text = newText.Substring(0,newText.Length < 180 ? newText.Length-1:180);
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            EndButton.IsEnabled = true;
            continueCheck = true;
            thread = new Thread(new ThreadStart(CheckImages));
            thread.Start();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = true;
            EndButton.IsEnabled = false;
            continueCheck = false;
            //Stop thread
        }

        private void Button_Click_Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_Minimize(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        Bitmap GetScreenCapture()
        {
            //Creating a new Bitmap object

            Bitmap captureBitmap = new Bitmap(Math.Abs(pointTopLeft.X - pointBottomRight.X), Math.Abs(pointTopLeft.Y - pointBottomRight.Y), System.Drawing.Imaging.PixelFormat.Format32bppArgb);


            Rectangle captureRectangle = new Rectangle(pointTopLeft.X,pointTopLeft.Y,Math.Abs(pointTopLeft.X - pointBottomRight.X), Math.Abs(pointTopLeft.Y - pointBottomRight.Y));





            Graphics captureGraphics = Graphics.FromImage(captureBitmap);



            captureGraphics.CopyFromScreen(captureRectangle.Left, captureRectangle.Top, 0, 0, captureRectangle.Size);


            return captureBitmap;
        }


        private System.Drawing.Point GetMousePosition()
        {
            var w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);

            return new System.Drawing.Point(w32Mouse.X, w32Mouse.Y);
        }

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }




        [DllImport("msvcrt.dll")]
        private static extern int memcmp(IntPtr b1, IntPtr b2, long count);

        public static bool CompareMemCmp(Bitmap b1, Bitmap b2)
        {
            if ((b1 == null) != (b2 == null)) return false;
            if (b1.Size != b2.Size) return false;

            var bd1 = b1.LockBits(new Rectangle(new System.Drawing.Point(0, 0), b1.Size), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var bd2 = b2.LockBits(new Rectangle(new System.Drawing.Point(0, 0), b2.Size), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                IntPtr bd1scan0 = bd1.Scan0;
                IntPtr bd2scan0 = bd2.Scan0;

                int stride = bd1.Stride;
                int len = stride * b1.Height;

                return memcmp(bd1scan0, bd2scan0, len) == 0;
            }
            finally
            {
                b1.UnlockBits(bd1);
                b2.UnlockBits(bd2);
            }
        }


        private void SaveBMP(Bitmap bmp, string path) // now 'ref' parameter
        {
            using (MemoryStream memory = new MemoryStream())
            {
                using (FileStream fs = new FileStream("Proofs/" + path, FileMode.Create, FileAccess.ReadWrite))
                {
                    bmp.Save(memory, ImageFormat.Png);
                    byte[] bytes = memory.ToArray();
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
        }

    }
}
