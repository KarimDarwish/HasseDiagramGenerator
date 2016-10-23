using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GraphVizWrapper;
using System.IO;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;


namespace HasseDiagram2._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private Point origin;
        private Point start;
        private GraphGeneration wrapperForGraph;
        public MainWindow()
        {
            InitializeComponent();
            #region "bs for zoom"
            TransformGroup group = new TransformGroup();
            ScaleTransform xform = new ScaleTransform();
            group.Children.Add(xform);
            TranslateTransform tt = new TranslateTransform();
            group.Children.Add(tt);
            img_graph.RenderTransform = group;
            img_graph.MouseWheel += image_MouseWheel;
            img_graph.MouseLeftButtonDown += image_MouseLeftButtonDown;
            img_graph.MouseLeftButtonUp += image_MouseLeftButtonUp;
            img_graph.MouseMove += image_MouseMove;
            #endregion

            var getStartProcessQuery = new GetStartProcessQuery();
            var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
            var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);

            var wrapper = new GraphGeneration(getStartProcessQuery,
                                              getProcessStartInfoQuery,
                                              registerLayoutPluginCommand);
            wrapperForGraph = wrapper;
           

        }
        private void GetGraph(GraphGeneration wrapper)
        {
            String[] set = txt_vars.Text.Split(',');
            StringBuilder sb = new StringBuilder();
            sb.Append("digraph{");
            for (int i = 0, max = 1 << set.Length; i < max; i++)
            {
                List<string> newList = new List<string>();
                for (int j = 0; j < set.Length; j++)
                {
                    int isList = (int)i & (1 << j); if (isList > 0)
                    {
                        newList.Add(set[j]);
                    }
                }
                if (newList.Count != set.Length)
                {
                    printLinks(newList, set, sb);
                }
            }
            sb.Append("}");
            byte[] output = wrapper.GenerateGraph(sb.ToString(), Enums.GraphReturnType.Png);
            BitmapImage img = LoadImage(output);
            this.Width = img.Width + 20;
            this.Height = img.Height + 20;
            img_graph.Source = LoadImage(output);
        }
        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        private static void printLinks(List<string> list, String[] arr, StringBuilder sb)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                String value = arr[i];
                if (list.Contains(value) == false)
                {
                    List<string> newList = new List<string>();
                    newList.AddRange(list);
                    newList.Add(value);
                    sb.Append(GetListAsString(newList)+ " -> " + GetListAsString(list)  + "[dir=back];"
                            + " \n");
                }
            }
        }
        private static String GetListAsString(List<string> set)
        {
            set.Sort();
            if (set.Count == 0)
            {
                return "\"{}\"";
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("\"{");
            for (int i = 0; i < set.Count; i++)
            {
                String value = set[i];
                sb.Append(value);
                if (i != set.Count - 1)
                {
                    sb.Append(",");
                }
            }
            sb.Append("}\"");
            return sb.ToString();
        }
        private void image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            img_graph.ReleaseMouseCapture();
        }

        private void image_MouseMove(object sender, MouseEventArgs e)
        {
            if (!img_graph.IsMouseCaptured) return;

            var tt = (TranslateTransform)((TransformGroup)img_graph.RenderTransform).Children.First(tr => tr is TranslateTransform);
            Vector v = start - e.GetPosition(border);
            tt.X = origin.X - v.X;
            tt.Y = origin.Y - v.Y;
        }

        private void image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            img_graph.CaptureMouse();
            var tt = (TranslateTransform)((TransformGroup)img_graph.RenderTransform).Children.First(tr => tr is TranslateTransform);
            start = e.GetPosition(border);
            origin = new Point(tt.X, tt.Y);
        }

        private void image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            TransformGroup transformGroup = (TransformGroup)img_graph.RenderTransform;
            ScaleTransform transform = (ScaleTransform)transformGroup.Children[0];

            double zoom = e.Delta > 0 ? .2 : -.2;
            transform.ScaleX += zoom;
            transform.ScaleY += zoom;
        }

        private void txt_vars_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetGraph(wrapperForGraph);
            }
        }

      
    }
}

