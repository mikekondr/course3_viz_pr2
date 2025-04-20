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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace example2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RotateTransform3D myYRotate, myZRotate, myYRotate2, myZRotate2;
        private AxisAngleRotation3D myYAxis, myZAxis, myYAxis2, myZAxis2;
        private Transform3DGroup myTransform1, myTransform2;
        private DispatcherTimer MyTimer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Створюємо перетворення для 1 об'єкта
            myYRotate = new RotateTransform3D();
            myYAxis = new AxisAngleRotation3D();
            myYAxis.Axis = new Vector3D(0, 1, 0);
            myYAxis.Angle = 0;
            myYRotate.Rotation = myYAxis;
            myZRotate = new RotateTransform3D();
            myZAxis = new AxisAngleRotation3D();
            myZAxis.Axis = new Vector3D(0, 0, 1);
            myZAxis.Angle = 0;
            myZRotate.Rotation = myZAxis;
            myTransform1 = new Transform3DGroup();
            MyModel.Transform = myTransform1;
            myTransform1.Children.Add(myYRotate);
            myTransform1.Children.Add(myZRotate);
            //Створюємо перетворення для 2 об'єктів
            myYRotate2 = new RotateTransform3D();
            myYAxis2 = new AxisAngleRotation3D();
            myYAxis2.Axis = new Vector3D(0, 1, 0);
            myYAxis2.Angle = 0;
            myYRotate2.Rotation = myYAxis2;
            myZRotate2 = new RotateTransform3D();
            myZAxis2 = new AxisAngleRotation3D();
            myZAxis2.Axis = new Vector3D(0, 0, 1);
            myZAxis2.Angle = 0;
            myZRotate2.Rotation = myZAxis2;
            myTransform2 = new Transform3DGroup();
            MyModel2.Transform = myTransform2;
            myTransform2.Children.Add(myYRotate2);
            myTransform2.Children.Add(myZRotate2);
            // Підготовлюємо таймер до роботи
            MyTimer = new DispatcherTimer();
            MyTimer.Tick += new EventHandler(MyTimer_Tick);
            MyTimer.Interval = new TimeSpan(100000);
        }

        private void MyTimer_Tick(object sender, EventArgs e)
        {
            myYAxis.Angle += 1;
            myZAxis.Angle += 1;
            myYAxis2.Angle -= 2;
            myZAxis2.Angle -= 2;
        }
        
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MyTimer.Start();
        }
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            MyTimer.Stop();
        }
    }
}
