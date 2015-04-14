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
using MongoDB;
using MongoDB.Configuration;
using System.Timers;
using GemBox.Spreadsheet;
using Microsoft.Office.Interop.Excel;




namespace Demon
{
    public delegate void OnMengEventHandler();
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        //public event OnMengEventHandler mengEvent;
        //链接字符串   
        private string connectionString = "mongodb://localhost";
        //数据库名   
        private string databaseName = "myDatabase";
        //集合名   
        private string collectionName = "myCollection";

        private Document doc;

        private Dictionary<int, GMapMarker> dict;

            //定义Mongo服务   
        Mongo mongo;
        MongoDatabase mDatabase;
        MongoCollection<Document> mCollection;  

        private String str;
        private Timer tmr;
        private ExcelFile ef;
        private ExcelWorksheet ws;

        private Worksheet worksheet;
        private String strConn;

        private int flag;

        public MainWindow()
        {
            InitializeComponent();

            //mDatabase = mongo.GetDatabase(databaseName) as MongoDatabase;
            //mCollection = mDatabase.GetCollection<GMapMarker>(collectionName) as MongoCollection<GMapMarker>;
            //mongo.Connect();
            
            SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
            tmr = new Timer(200);
            tmr.Enabled = false;
            flag = 1;
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

            str = OpenDialog();
            
            SetupMongo();

            //找到那个CSV文件并建立联系

         //   worksheet = LoadExcel(str);

            
            ef = ExcelFile.Load(str);
            ws = ef.Worksheets[0];
            
            //读取起始位置
            flag = 1;


            //fakeReadData();
            //进入副线程，开始定时读写，然后触发画图，直到中断。
            
            tmr.Elapsed += new System.Timers.ElapsedEventHandler(ReadData);

            tmr.AutoReset = true;
            //tmr.AutoReset = false;
            tmr.Enabled = true;


        }

        private void SetupMongo()
        {
            mongo = new Mongo(connectionString);
            //获取databaseName对应的数据库，不存在则自动创建    
            mDatabase = mongo.GetDatabase(databaseName) as MongoDatabase;
            mCollection = mDatabase.GetCollection<Document>(collectionName) as MongoCollection<Document>;
            mongo.Connect();  
            

        }

        private string OpenDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "选择数据文件";
            openFileDialog.Filter = "csv文件|*.csv";
            openFileDialog.FileName = string.Empty;
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.DefaultExt = "csv";
            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }
            else
            {
                return "";
            }
        }

        private void ReadData(object sender, System.Timers.ElapsedEventArgs e)
        {
            //MessageBox.Show("膜蛤膜蛤");
            
            if (flag <= ws.Rows.Count)
            {
                //从CSV导入下一条数据

               // DataRow row = worksheet.Rows[flag + 1];
               // int db_objID = Int32.Parse(row[1 + 1].ToString());   //Int32.Parse(ws.Cells[flag, 1].Value.ToString());
              //  String db_date = row[2 + 1].ToString();//ws.Cells[flag, 2].Value.ToString();
              //  double latitude = Double.Parse(row[4+1].ToString());
               // double longitude = Double.Parse(row[6+1].ToString());
                
                int db_objID = Int32.Parse(ws.Cells[flag, 1].Value.ToString());
                String db_date = ws.Cells[flag, 2].Value.ToString();
                double latitude = Double.Parse(ws.Cells[flag,4].Value.ToString());
                double longitude = Double.Parse(ws.Cells[flag, 6].Value.ToString());

                //导入数据库
                //查询是否存在，如果存在则只更新，若不存在则创建，然后显示。
                Document doc = mCollection.FindOne(new Document{{"ID",db_objID}});
                if (doc != null && doc.Count> 0)
                {
                    doc["Record"] += db_date + " " + latitude.ToString() +" " + longitude.ToString()+ "||";
                    doc["CurLat"] =latitude;
                    doc["CurLng"] =longitude;
 
                    mCollection.Save(doc);

                    Action<GMapMarker,PointLatLng> updatePositionAction = new Action<GMapMarker, PointLatLng>(UpdateTaxiPos);

                    PointLatLng position = new PointLatLng(latitude,longitude);
   
                    mapControl.Dispatcher.BeginInvoke(updatePositionAction, dict[db_objID], position);


                }
                else
                {
                    Document addDoc = new Document();
                    addDoc["ID"] = db_objID;
                    addDoc["Record"] = db_date + " " + latitude.ToString() + " " + longitude.ToString() + "||";
                    addDoc["CurLat"] = latitude;
                    addDoc["CurLng"] = longitude;

                    
                    mCollection.Insert(addDoc);

                    GMapMarker marker = new GMapMarker(new PointLatLng(latitude,longitude));
                    marker.Tag = db_objID;
                    dict.Add(db_objID, marker);                    
                    
                    Action<GMapMarker> updateAction = new Action<GMapMarker>(BackDrawTaxi);
                    mapControl.Dispatcher.BeginInvoke(updateAction, marker);
                }  






                flag++;
            }
        }


        private void EndSimulation(object sender, RoutedEventArgs e)
        {
            //先shut了定时器
            tmr.Enabled = false;

            //清屏
            mapControl.Markers.Clear();
            //关闭mongo
            var allResult = mCollection.FindAll().Documents;
            foreach (Document docItem in allResult)
            {
                mCollection.Remove(docItem,false);
            }
            mongo.Disconnect();
            dict.Clear();

        }

        private void UpdateTaxiPos(GMapMarker marker, PointLatLng position)
        {
            mapControl.Markers.Remove(marker);
            marker.Position = position;
            mapControl.Markers.Add(marker);
        }

        public Worksheet LoadExcel(string pPath)
        {
            Microsoft.Office.Interop.Excel._Application excelApp = new Microsoft.Office.Interop.Excel.Application();
            Workbook excelWB;
            Workbooks excelWBs;
            Worksheet excelWS;

            Sheets excelSts;

            if (excelApp == null)
            {
                throw new Exception("打开Excel应用时发生错误！");
            }
            excelWBs = excelApp.Workbooks;
            //打开一个现有的工作薄  
            excelWB = excelWBs.Add(pPath);
            excelSts = excelWB.Sheets;
            excelWS = excelSts.get_Item(1);
            return excelWS;
        }



       

    }

}