using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Microsoft.Win32;
using System.Timers;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core;



namespace Demon
{
    public delegate void OnMengEventHandler();
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {


        private Dictionary<int, GMapMarker> dict;

       // private MongoClient client;

       // private MongoDatabase database;

      //  private MongoCollection<CarRecord> collectionOfRecord;

        //private List<CarRecord> FindCarList;

        //private MongoCollection collectionOfCar;

        private int simuTimeStart;

        private int simuTimeEnd;

        private int simuDelta;

        private Timer tmr;

        public MainWindow()
        {
            InitializeComponent();



            //FindCarList = new List<CarRecord>();

            simuTimeStart = 11 * 3600;
            simuDelta = 5;
            simuTimeEnd = simuTimeStart + simuDelta;


            tmr = new Timer(500);
            tmr.Enabled = false;
            dict = new Dictionary<int, GMapMarker>();

            try
            {
                System.Net.IPHostEntry e = System.Net.Dns.GetHostEntry("www.google.com.hk");
            }
            catch
            {
                mapControl.Manager.Mode = AccessMode.CacheOnly;
                MessageBox.Show("No internet connection avaible, going to CacheOnly mode.", "GMap.NET Demo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            mapControl.MapProvider = GMapProviders.OpenStreetMap; //OpenStreetMap 地图
            mapControl.MinZoom = 2;  //最小缩放
            mapControl.MaxZoom = 17; //最大缩放
            mapControl.Zoom = 15;     //当前缩放
            mapControl.ShowCenter = true; //不显示中心十字点
            mapControl.DragButton = MouseButton.Right; //右键拖拽地图
            mapControl.Position = new PointLatLng(39.98968, 116.3267); //地图中心位置：南京 内容来自17jquery

            mapControl.OnMapZoomChanged += new MapZoomChanged(mapControl_OnMapZoomChanged);
            mapControl.MouseLeftButtonDown += new MouseButtonEventHandler(mapControl_MouseLeftButtonDown);
        }

        void mapControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point clickPoint = e.GetPosition(mapControl);
            PointLatLng point = mapControl.FromLocalToLatLng((int)clickPoint.X, (int)clickPoint.Y);
            if (TbOfDraw.SelectedIndex == 1)
                DrawTaxi(point);
            else if(TbOfDraw.SelectedIndex == 2)
                DrawPassenger(point);
        }

        void mapControl_OnMapZoomChanged()
        {
        }

        private void DrawPassenger(PointLatLng positionLatLng)
        {
            GMapMarker marker = new GMapMarker(positionLatLng);
            marker.Shape = new MarkerOfMan(this,marker);
            mapControl.Markers.Add(marker);
        }

        private void DrawTaxi(PointLatLng positionLatLng)
        {
            GMapMarker marker = new GMapMarker(positionLatLng);
            marker.Shape = new MarkerOfCar(this, marker);
            mapControl.Markers.Add(marker);
        }

        private void DrawPassenger(GMapMarker marker)
        {
            marker.Shape = new MarkerOfMan(this, marker);
            mapControl.Markers.Add(marker);
        }

        private void DrawTaxi(GMapMarker marker)
        {
            marker.Shape = new MarkerOfCar(this, marker);
            mapControl.Markers.Add(marker);
        }

        private void BackDrawTaxi(GMapMarker marker)
        {
            marker.Shape = new MarkerOfCar(this, marker);
            mapControl.Markers.Add(marker);
        }

        private void StartSimulation(object sender, RoutedEventArgs e)
        {

            //fakeReadData();
            //进入副线程，开始定时读写，然后触发画图，直到中断。
            simuTimeStart = 11 * 3600;
            simuTimeEnd = simuTimeStart + simuDelta;


            tmr.Elapsed += new System.Timers.ElapsedEventHandler(FindCarAndDraw);

            tmr.AutoReset = true;
           // tmr.AutoReset = false;
            tmr.Enabled = true;


        }

        private void SetupMongo()
        {
            

        }

        private void OpenDialog()
        {
          
        }

        private async void FindCarAndDraw(object sender, System.Timers.ElapsedEventArgs e)
        {
            //建立数据库连接和准备工作
            var client = new MongoClient();

            var database = client.GetDatabase("test");
            //client = new MongoClient();
            //database = client.GetDatabase("test");
            var collectionOfRecord = database.GetCollection<CarRecord>("Car1");
            
            
            
            //找到本个仿真区间内的所有车的记录存入List
            String simuStart = SimuTrans.TransIntToString(simuTimeStart);
            String simuMiddle = SimuTrans.TransIntToString(simuTimeStart+1);
            String simuAlmost = SimuTrans.TransIntToString(simuTimeStart + 2);
            String simuEnd = SimuTrans.TransIntToString(simuTimeEnd-1);

            var FindCarList = await collectionOfRecord.Find(x => x.currentTime == simuEnd || x.currentTime == simuMiddle || x.currentTime == simuAlmost || x.currentTime == simuStart).ToListAsync();
            //var FindCarList = await collectionOfRecord.Find(x => x.carId != 0).ToListAsync();


            if (FindCarList == null)
            {

            }

            else
            {
                foreach (var carRecord in FindCarList)
                {
                    if (dict.ContainsKey(carRecord.carId) == false)
                    {

                        GMapMarker newMarker = new GMapMarker(new PointLatLng(carRecord.latitude, carRecord.longitude));
                        dict.Add(carRecord.carId, newMarker);
                        dict[carRecord.carId].Tag = carRecord;

                        Action<GMapMarker> drawAction = new Action<GMapMarker>(BackDrawTaxi);

                        mapControl.Dispatcher.BeginInvoke(drawAction, dict[carRecord.carId]);

                    }

                    else
                    {
                        Action<GMapMarker, PointLatLng> updateAction = new Action<GMapMarker, PointLatLng>(UpdateTaxiPos);

                        PointLatLng position = new PointLatLng(carRecord.latitude, carRecord.longitude);

                        mapControl.Dispatcher.BeginInvoke(updateAction, dict[carRecord.carId], position);
                    }
                }

            }

            simuTimeStart = simuTimeEnd;
            simuTimeEnd = simuTimeEnd + simuDelta;
        }


        private void EndSimulation(object sender, RoutedEventArgs e)
        {
            //先shut了定时器
            tmr.Enabled = false;

            //清屏
            mapControl.Markers.Clear();
            //关闭mongo

            dict.Clear();

        }

        private void UpdateTaxiPos(GMapMarker marker, PointLatLng position)
        {
            mapControl.Markers.Remove(marker);
            marker.Position = position;
            mapControl.Markers.Add(marker);
        }



       

    }

}