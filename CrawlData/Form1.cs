using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Data.SqlClient;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace CrawlData
{
    public partial class Form1 : Form
    {
        string kn = @"Data Source=D4TOAN-PC\SQLEXPRESS;Initial Catalog=WebTinTuc;Integrated Security=True";
        public Form1()
        {
            InitializeComponent();
        }

        //nút thoát
        private async void button3_Click(object sender, EventArgs e)
        {


            Application.Exit();
            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            //request.Method = "GET";
            //request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:46.0) Gecko/20100101 Firefox/46.0";
            //request.Accept = "*/*";
            //request.Referer = url;




            //HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            //Stream str = response.GetResponseStream();
            //XmlTextReader readxml = new XmlTextReader(str);

            //try
            //{
            //    XmlDocument xmldoc =new XmlDocument();
            //    xmldoc.Load(readxml);

            //}
            //catch (Exception ex)
            //{ }



        }
        //lấy tên bài hát, ca sĩ, link mp3
        private void GetData_Click(object sender, EventArgs e)
        {
            try
            {
                System.Data.SqlClient.SqlConnection con = new SqlConnection(kn);
                string url = txtLink.Text;
                System.Net.WebClient client = new System.Net.WebClient();
                //mã html của link
                string htmldau = client.DownloadString(url);
                client.Encoding = Encoding.UTF8;
                client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
                //lấy file xml tìm kiếm được từ htmldau
                string urlmp3 = Regex.Match(htmldau, @"xmlURL = ""(.+?)""", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
                //lấy mã nguồn trang xml
                string linkxml = client.DownloadString(urlmp3);
                //lấy link nhạc từ trang xml
                string linknhac = "http" + Regex.Match(linkxml.ToString(), @"\[CDATA\[http(.+?)\]", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
                HtmlAgilityPack.HtmlWeb hw = new HtmlAgilityPack.HtmlWeb()
                {
                    AutoDetectEncoding = false,
                    OverrideEncoding = Encoding.UTF8
                };
                //load toàn bộ trang html
                HtmlAgilityPack.HtmlDocument doc = hw.Load(url);
                //chọn node có các node cần thiết
                HtmlNodeCollection linkbh = doc.DocumentNode.SelectNodes("//*[@id='box_playing_id']");

                foreach (var items in linkbh)
                {
                    //lấy node có tên bài hát
                    var linktenbai = items.SelectSingleNode("//*[@id='box_playing_id']//div//div//h1");
                    var tenbai = linktenbai.InnerText;
                    //lấy node có tên ca sĩ
                    var linktencs = items.SelectNodes("//*[@id='box_playing_id']//div//div//h2/a[@class='name_singer']").ToList();
                    string tencs = "";
                    //lấy ký tự nắm trong linktencs
                    foreach (var item in linktencs)
                    {
                        string kq1 = "";
                        kq1 = item.InnerText.ToString();
                        tencs += kq1.ToString() + " ";
                    }

                    DateTime day = DateTime.Now;
                    string them = "insert tintuc(Tieude,Noidung,ngaytao,link) values(N'" + tenbai + "',N'" + tencs + "','" + day + "','" + linknhac + "')";
                    con.Open();
                    //khai báo 1 biến thực hiện lệnh trong SQl
                    SqlCommand cmd = new SqlCommand();
                    //kết nối biến đến CSDL
                    cmd.Connection = con;
                    cmd.CommandText = them;
                    cmd.ExecuteNonQuery();
                    con.Close();
                    _load();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi:" + ex);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _load();
        }

        //Hàm load lại trang
        //sau khi lưu db sẽ tự động hiển thị lại dữ liệu ra gridview
        public void _load()
        {
            System.Data.SqlClient.SqlConnection con = new SqlConnection(kn);
            //mở kết nối
            con.Open();
            //khai báo 1 biến thực hiện lệnh trong SQl
            SqlCommand cmd = new SqlCommand();
            //kết nối biến đến CSDL
            cmd.Connection = con;
            //gán lệnh SQL cho biến
            cmd.CommandText = "select * from tintuc";
            //khai báo 1 biến để nạp dữ liệu từ SQL
            SqlDataAdapter da = new SqlDataAdapter("select * from tintuc", con);
            //khai báo 1 biến chứa dữ liệu dạng bảng
            DataTable db = new DataTable();
            //nạp dữ liệu từ SQl vào biến chứa dữ liệu
            da.Fill(db);
            //đưa dữ liệu ra datagridview
            dataGridView1.DataSource = db;
            //ngắt kết nối
            con.Dispose();
            da.Dispose();
            cmd.Dispose();
        }
    }
}
