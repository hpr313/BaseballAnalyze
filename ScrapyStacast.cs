using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using SharedProject.Model;

namespace Baseball_Project.Model
{
    public class ScrapyStacast
    {
        public string currentYear = "2024";
        public string apiURL(string _name)
        {
            //return $"http://localhost:80/baseballnews/api/home/{_name}/";
            return $"http://localhost:20811/api/home/{_name}/";

        }
        internal WebDriver webDriver()
        {
            WebDriver driver = new FirefoxDriver(@"firefox\geckodriver.exe");
            //WebDriver driver = new ChromeDriver(@"C:\bin");
            return driver;
        }
        internal void searchCond(WebDriver xxx, string _type, string _year)
        {
            string url = $"https://baseballsavant.mlb.com/statcast_search";
            xxx.Navigate().GoToUrl(url);
            Thread.Sleep(5000);
            //Select Type
            IWebElement type = xxx.FindElement(By.Id("player_type"));
            SelectElement selType = new SelectElement(type);
            selType.SelectByValue(_type);
            //Select Year
            if (!(_year == currentYear))
            {
                IWebElement selYear = xxx.FindElement(By.XPath("//*[@id=\"ddlSeason\"]/div[1]/div[2]"));
                selYear.Click();
                IWebElement currentyear = xxx.FindElement(By.Id($"chk_Sea_{currentYear}"));
                currentyear.Click();
                IWebElement year = xxx.FindElement(By.Id($"chk_Sea_{_year}"));
                year.Click();
            }
            //IWebElement year = xxx.FindElement(By.Id($"chk_Sea_{_year}"));
            //Click Search btn
            IWebElement click = xxx.FindElement(By.XPath($"//*[@id=\"search-buttons-flex\"]/div[1]/input"));
            //IWebElement _search = xxx.FindElement(By.Id("search-buttons-flex"));
            //IWebElement search = _search.FindElement(By.XPath("/div[1]/input"));
            //*[@id="search-buttons-flex"]/div[1]/input
            //*[@id="search-buttons-flex"]/div[1]/input
            click.Click();
            Thread.Sleep(1000);
        }

        internal List<string> getImage(WebDriver xxx)
        {
            List<string> playerImage = new List<string>();
            IWebElement tbody = xxx.FindElement(By.TagName("tbody"));
            IList<IWebElement> _image = xxx.FindElements(By.ClassName("player-mug"));
            foreach (IWebElement element in _image)
            {
                string image = element.GetAttribute("src");
                playerImage.Add(image);
            }
            return playerImage;
        }
        internal List<int> getIDs(WebDriver xxx)
        {
            List<int> playerID = new List<int>();
            IWebElement tbody = xxx.FindElement(By.TagName("tbody"));
            IList<IWebElement> _id = xxx.FindElements(By.ClassName("search_row"));
            foreach (IWebElement element in _id)
            {
                int id = int.Parse(element.GetAttribute("data-player-id"));
                playerID.Add(id);
            }
            return playerID;
        }
        internal List<string> getNames(WebDriver xxx, string type)
        {
            List<string> playersName = new List<string>();
            IWebElement tbody = xxx.FindElement(By.TagName("tbody"));
            IList<IWebElement> _id = xxx.FindElements(By.ClassName("search_row"));
            foreach (IWebElement element in _id)
            {
                IWebElement _name = element.FindElement(By.ClassName("player_name"));
                // EX1: _name(pitcher) = De Los Santos, Yerry RHP
                // EX2: _name(pitcher) = Gonzalez, Chi Chi LHP
                // EX3: _name(batter) = Judge, Aaron
                string[] names = _name.Text.Trim().Split(',');
                string prename;
                string name;
                if (type == "pitcher")
                {
                    prename = names[names.Length - 1].Remove(names[names.Length - 1].Length - 4);
                    name = prename.Trim().ToUpper() + ' ' + names[0].ToUpper();
                    //name = names[0] + "," + prename;
                }
                else
                {
                    prename = names[names.Length - 1].Trim();
                    name = names[0] + ", " + prename;
                }
                playersName.Add(name);
            }
            return playersName;
        }
        internal string getID(string name)
        {
            SqlServer sql = new SqlServer();
            SqlConnection cnn = new SqlConnection(sql.connectTo());
            cnn.Open();
            SqlCommand myCommand = new SqlCommand();
            myCommand = cnn.CreateCommand();
            myCommand.CommandType = CommandType.Text;
            string newName;
            if (name.Contains("'"))
            {
                newName = name.Replace("\'", "\'\'");
                myCommand.CommandText = $@"Select playerID from dbo.playerInfo where playerName = '{newName}'";
            }
            else { myCommand.CommandText = $@"Select playerID from dbo.playerInfo where playerName = '{name}'"; }
            myCommand.ExecuteNonQuery();
            SqlDataReader dr = myCommand.ExecuteReader();
            List<int> vs = new List<int>();
            while (dr.Read()) { vs.Add(dr.GetInt32(0)); }
            cnn.Close();
            try { return vs[0].ToString(); }
            catch { return ""; }
        }
        internal List<string> getAllName()
        {
            StatcastAPIServices api = new StatcastAPIServices();
            string responseBody = api.getMethod("getPlayerName");
            List<string> data = JsonConvert.DeserializeObject<List<string>>(responseBody);
            return data;
        }
        internal void insert_playerInfo_postAPI(int id, string name, string image)
        {
            StatcastAPIServices api = new StatcastAPIServices();
            Statcast player = new Statcast(id, name, image);
            string responseBody = api.objectPostMethod("insertPlayerInfo", player);
        }
        internal void insertPlayerInfo(int _id, string _name, string _image)
        {
            // if not in sql
            if (checkID(_id)) { insert_playerInfo_postAPI(_id, _name, _image); }
        }
        internal bool checkID(int id)
        {
            SqlServer sql = new SqlServer();
            SqlConnection cnn = new SqlConnection(sql.connectTo());
            cnn.Open();
            SqlCommand myCommand = new SqlCommand();
            myCommand = cnn.CreateCommand();
            myCommand.CommandType = CommandType.Text;
            myCommand.CommandText = @"Select playerID from dbo.playerInfo";
            myCommand.ExecuteNonQuery();
            SqlDataReader dr = myCommand.ExecuteReader();
            List<int> data = new List<int>();
            while (dr.Read()) { data.Add(dr.GetInt32(0)); }
            cnn.Close();
            HashSet<int> hs = new HashSet<int>();
            for (int i = 0; i < data.Count; i++) { hs.Add(data[i]); }
            if (!hs.Contains(id)) { return true; }
            else { return false; }
        }
        
        internal IList<IWebElement> getDataBodyList(WebDriver driver, string id)
        {
            IWebElement clickable;
            try { clickable = driver.FindElement(By.Id("id_" + id)); }
            catch { driver.Navigate().Refresh(); clickable = driver.FindElement(By.Id("id_" + id)); }
            clickable.Click();
            Thread.Sleep(10000);
            string IDpath = $"//*[@id=\"ajaxTable_{id}\"]";
            IWebElement table;
            try { table = driver.FindElement(By.Id($"ajaxTable_{id}")); }
            catch
            {
                driver.Navigate().Refresh();
                clickable = driver.FindElement(By.Id("id_" + id));
                clickable.Click();
                Thread.Sleep(10000);
                IWebElement _table = driver.FindElement(By.ClassName("table-savant"));
                table = _table.FindElement(By.Id($"ajaxTable_{id}"));
            }
            IWebElement tbody = table.FindElement(By.TagName("tbody"));
            string tbodyPath = $"//*[@id=\"ajaxTable_{id}\"]/tbody/tr";
            IList<IWebElement> _tbody = tbody.FindElements(By.XPath(tbodyPath));
            return _tbody;
        }

        public string getLink(IWebElement _tbody, string id, int i)
        {
            Statcast data = new Statcast();
            string path = $"//*[@id=\"ajaxTable_{id}\"]/tbody/tr[{i}]";
            IWebElement td = _tbody.FindElement(By.XPath(path));
            IList<IWebElement> td_list = td.FindElements(By.TagName("td"));
            //check the repitibility for data by videoLink
            string videoLink;
            //get videoLink
            try { videoLink = td_list[14].FindElement(By.XPath($"//*[@id=\"ajaxTable_{id}\"]/tbody/tr[{i}]/td[15]/a")).GetAttribute("href"); }
            catch { videoLink = ""; }
            return videoLink;
        }
        internal void getData(IWebElement _tbody, string id, int i)
        {
            List<string> batterName = new List<string>();
            List<DateTime> gameDate = new List<DateTime>();
            Statcast data = new Statcast();
            string path = $"//*[@id=\"ajaxTable_{id}\"]/tbody/tr[{i}]";
            IWebElement td = _tbody.FindElement(By.XPath(path));
            IList<IWebElement> td_list = td.FindElements(By.TagName("td"));
            //check the repitibility for data by videoLink
            //string videoLink;
            //get videoLink
            //try { videoLink = td_list[14].FindElement(By.XPath($"//*[@id=\"ajaxTable_{id}\"]/tbody/tr[{i}]/td[15]/a")).GetAttribute("href"); }
            //catch { videoLink = ""; }
            //HashSet<string> videoLinks = getVideoLinks(int.Parse(id));
            //if (checkVideoLink(videoLink, videoLinks))
            //{            
                //process playerID (pitcherID)
                data.playerID = int.Parse(id);
                //int pitcherID = int.Parse(getID(data.pitcher));
                //process batter. EX: Moniak, Mickey(L)
                string[] _batter = td_list[5].Text.Remove(td_list[5].Text.Length - 3).Split(',');
                data.batter = _batter[1].Remove(0, 1).ToUpper() + ' ' + _batter[0].ToUpper();
                batterName.Add(data.batter);
                //process batterID
                try { data.batterID = int.Parse(getID(data.batter)); }
                catch { data.batterID = null; }
                //process datetime
                data.date = DateTime.Parse(td_list[0].Text);
                gameDate.Add(data.date);
                //process pitch
                string pitch = td_list[1].Text;
                if (pitch == "") { data.pitch = null; }
                else { data.pitch = pitch; }
                //process MPH
                try { data.mph = double.Parse(td_list[2].Text); }
                catch { data.mph = null; }
                //process spin
                try { data.spinRate = int.Parse(td_list[3].Text); }
                catch { data.spinRate = null; }
                //process pitcher. EX: Dunning, Dane(R)
                string[] _pitcher = td_list[4].Text.Remove(td_list[4].Text.Length - 3).Split(',');
                data.pitcher = _pitcher[1].Remove(0, 1).ToUpper() + ' ' + _pitcher[0].ToUpper();
                //process EV(MPH). EX: 82.3 or --
                string ev = td_list[6].Text;
                if (ev == "--") { data.evMph = null; }
            else { data.evMph = double.Parse(ev); }
                //process la. Ex: -12° or °
                string la = td_list[7].Text;
                if (la == "°") { data.la = null; }
                else { data.la = double.Parse(la.Remove(la.Length - 1)); }
                //process distance
                string dis = td_list[8].Text;
                try { data.distance = int.Parse(dis); }
                catch { data.distance = null; }
                //process zone. EX: Zone 12
                IWebElement _zone = td.FindElement(By.ClassName("zone-icon"));
                string[] zone = _zone.GetAttribute("title").Split(' ');
                try { data.zone = int.Parse(zone[zone.Length - 1]); }
                catch { data.zone = null; }
                //data.zone = int.Parse(td_list[8].Text);
                //process count
                data.count = td_list[10].Text;
                //process inning
                data.inning = td_list[11].Text;
                //process pitch result
                data.pitchResult = td_list[12].Text;
                //process PA result
                data.paResult = td_list[13].Text;
                //process batterResult 
                //Case 1: first data or batter changed or paResult is "Hit Into Play" => has batterResult
                //Case 2: batter does not changed but game date changed  => has batterResult
                //Case 3: Otherwise => no batterResult
                if (batterName.Count == 1 || !(data.batter == batterName[batterName.Count - 2]) || data.pitchResult == "Hit Into Play") { batterAndHitResult(data.paResult, data); }
                else if (data.batter == batterName[batterName.Count - 2] && !(data.date == gameDate[gameDate.Count - 2])) { batterAndHitResult(data.paResult, data); }
                else { data.batterResult = null; data.hitType = null; }
            //process videoLink  
            data.videoLink = getLink(_tbody, id, i);
            insert_pitcherData_postAPI(data);
            //}
        }
        internal void insert_pitcherData_postAPI(Statcast xxx)
        {
            HttpClient client = new HttpClient();
            string jsonText = JsonConvert.SerializeObject(xxx);
            var requestContent = new System.Net.Http.StringContent(jsonText, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(apiURL("insertPitcherData"), requestContent).Result; //雖然沒有response，但需要post出去
        }
        private void batterAndHitResult(string xxx, Statcast ooo)
        {
            string[] batterResult = { "strikes out", "pops out", "grounds out", "out on strikes", "flies out", "double play", "sacrifice fly",
                                        "force out","lines out","single","double","triple","homers","walks","error","hit by pitch",
                                        "sacrifice bunt", "choice", "grand slam", "home run"};
            // Add "sacrifice bunt", "choice", "grand slam", "home run", 
            switch (batterResult.FirstOrDefault<string>(s => xxx.Contains(s)))
            {
                case "strikes out":
                    ooo.batterResult = "strikes out swinging";
                    ooo.hitType = null;
                    ooo.hitSpeed = null;
                    break;
                case "double play":
                    ooo.batterResult = "double play";
                    ooo.hitType = null;
                    hitSpeedResult(xxx, ooo);
                    break;
                case "pops out":
                    ooo.batterResult = "pops out";
                    ooo.hitType = null;
                    hitSpeedResult(xxx, ooo);
                    break;
                case "grounds out":
                    ooo.batterResult = "grounds out";
                    ooo.hitType = null;
                    hitSpeedResult(xxx, ooo);
                    break;
                case "out on strikes":
                    ooo.batterResult = "called out on strikes";
                    ooo.hitType = null;
                    ooo.hitSpeed = null;
                    break;
                case "flies out":
                    ooo.batterResult = "flies out";
                    ooo.hitType = null;
                    hitSpeedResult(xxx, ooo);
                    break;
                case "force out":
                    ooo.batterResult = "force out";
                    ooo.hitType = null;
                    hitSpeedResult(xxx, ooo);
                    break;
                case "lines out":
                    ooo.batterResult = "lines out";
                    ooo.hitType = null;
                    hitSpeedResult(xxx, ooo);
                    break;
                case "sacrifice fly":
                    ooo.batterResult = "sacrifice fly";
                    ooo.hitType = null;
                    hitSpeedResult(xxx, ooo);
                    break;
                case "single":
                    ooo.batterResult = "singles";
                    hitTypeResult(xxx, ooo);
                    hitSpeedResult(xxx, ooo);
                    break;
                case "double":
                    ooo.batterResult = "doubles";
                    hitTypeResult(xxx, ooo);
                    hitSpeedResult(xxx, ooo);
                    break;
                case "triple":
                    ooo.batterResult = "triples";
                    hitTypeResult(xxx, ooo);
                    hitSpeedResult(xxx, ooo);
                    break;
                case "homer":
                    ooo.batterResult = "homers";
                    hitTypeResult(xxx, ooo);
                    hitSpeedResult(xxx, ooo);
                    break;
                case "walks":
                    ooo.batterResult = "walks";
                    ooo.hitType = null;
                    ooo.hitSpeed = null;
                    break;
                case "error":
                    ooo.batterResult = "error";
                    ooo.hitType = null;
                    hitSpeedResult(xxx, ooo);
                    break;
                case "hit by pitch":
                    ooo.batterResult = "hit by pitch";
                    ooo.hitType = null;
                    ooo.hitSpeed = null;
                    break;
                case "sacrifice bunt":
                    ooo.batterResult = "sacrifice bunt";
                    ooo.hitType = null;
                    hitSpeedResult(xxx, ooo);
                    break;
                case "choice":
                    ooo.batterResult = "choice";
                    ooo.hitType = null;
                    hitSpeedResult(xxx, ooo);
                    break;
                case "grand slam":
                    ooo.batterResult = "homer";
                    ooo.hitType = "fly ball";
                    hitSpeedResult(xxx, ooo);
                    break;
                case "home run":
                    ooo.batterResult = "homer";
                    hitTypeResult(xxx, ooo);
                    hitSpeedResult(xxx, ooo);
                    break;
                default:
                    ooo.batterResult = null;
                    ooo.hitType = null;
                    ooo.hitSpeed = null;
                    break;
            }
        }
        private void hitTypeResult(string xxx, Statcast ooo)
        {
            string[] hitType = { "ground ball", "fly ball", "line drive" };
            switch (hitType.FirstOrDefault<string>(s => xxx.Contains(s)))
            {
                case "ground ball":
                    ooo.hitType = "ground ball";
                    break;
                case "fly ball":
                    ooo.hitType = "fly ball";
                    break;
                case "line drive":
                    ooo.hitType = "line drive";
                    break;
                default:
                    ooo.hitType = null;
                    break;
            }
        }
        private void hitSpeedResult(string xxx, Statcast ooo)
        {
            string[] hitType = { "sharp", "medium", "soft" };
            if (ooo.evMph == null)
            {
                switch (hitType.FirstOrDefault<string>(s => xxx.Contains(s)))
                {
                    case "sharp":
                        ooo.hitSpeed = "sharp";
                        break;
                    case "soft":
                        ooo.hitSpeed = "soft";
                        break;
                    default:
                        ooo.hitSpeed = "medium";
                        break;
                }
            }
            else
            {
                if (ooo.evMph >= 95) { ooo.hitSpeed = "sharp"; }
                else if (ooo.evMph < 70) { ooo.hitSpeed = "soft"; }
                else { ooo.hitSpeed = "medium"; }
            }

        }
        internal List<string> getVideo(WebDriver driver, string id)
        {
            IWebElement clickable;
            try { clickable = driver.FindElement(By.Id("id_" + id)); }
            catch { driver.Navigate().Refresh(); clickable = driver.FindElement(By.Id("id_" + id)); }
            clickable.Click();
            Thread.Sleep(10000);
            string IDpath = $"//*[@id=\"ajaxTable_{id}\"]";
            IWebElement table;
            try { table = driver.FindElement(By.Id($"ajaxTable_{id}")); }
            catch
            {
                driver.Navigate().Refresh();
                clickable = driver.FindElement(By.Id("id_" + id));
                clickable.Click();
                Thread.Sleep(15000);
                IWebElement _table = driver.FindElement(By.ClassName("table-savant"));
                table = _table.FindElement(By.Id($"ajaxTable_{id}"));
            }

            IWebElement tbody = table.FindElement(By.TagName("tbody"));
            string tbodyPath = $"//*[@id=\"ajaxTable_{id}\"]/tbody/tr";
            List<string> vedioLink = new List<string>();
            IList<IWebElement> _tbody = tbody.FindElements(By.XPath(tbodyPath));
            for (int i = 1; i <= _tbody.Count; i++)
            {
                Statcast data = new Statcast();
                string path = $"//*[@id=\"ajaxTable_{id}\"]/tbody/tr[{i}]";
                IWebElement td = tbody.FindElement(By.XPath(path));
                IList<IWebElement> td_list = td.FindElements(By.TagName("td"));
                string video;
                try { video = td_list[14].FindElement(By.XPath($"//*[@id=\"ajaxTable_{id}\"]/tbody/tr[{i}]/td[15]/a")).GetAttribute("href"); }
                catch { video = ""; }
                vedioLink.Add(video);
            }
            return vedioLink;
        }
        //
        // To Update batterResult
        //
        //internal List<Statcast> getBatterResult()
        //{
        //    string url = "http://localhost:20811/api/home/getBatterResult/";
        //    HttpClient client = new HttpClient();
        //    HttpResponseMessage response = client.GetAsync(url).Result;
        //    string responseBody = response.Content.ReadAsStringAsync().Result;
        //    List<Statcast> data = JsonConvert.DeserializeObject<List<Statcast>>(responseBody);
        //    return data;
        //}
        //internal void updateBatterResult(Statcast xxx)
        //{
        //    batterAndHitResult(xxx.paResult, xxx);
        //    string url = "http://localhost:20811/api/home/updateBatterResult/";
        //    HttpClient client = new HttpClient();
        //    string jsonText = JsonConvert.SerializeObject(xxx);
        //    var requestContent = new System.Net.Http.StringContent(jsonText, System.Text.Encoding.UTF8, "application/json");
        //    HttpResponseMessage response = client.PostAsync(url, requestContent).Result; //雖然沒有response，但需要post出去
        //}
        //
        //get all current playerID
        //
        internal List<int> getCurrentIds()
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.GetAsync(apiURL("getCurrentIDs")).Result;
            string responseBody = response.Content.ReadAsStringAsync().Result;
            List<int> data = JsonConvert.DeserializeObject<List<int>>(responseBody);
            return data;
        }
        public List<PitchAnalyze> getPitchAnalyze()
        {
            string url = apiURL("getPitchAnalyze"); //"http://localhost:20811/api/home/getPitchAnalyze/";
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.GetAsync(url).Result;
            string responseBody = response.Content.ReadAsStringAsync().Result;
            List<PitchAnalyze> data = JsonConvert.DeserializeObject<List<PitchAnalyze>>(responseBody);
            return data;
        }

        public HashSet<string> getVideoLinks(int id)
        {
            string url = apiURL("getVideoLink"); //"http://localhost:20811/api/home/getVideoLink/";
            HttpClient client = new HttpClient();
            Statcast player = new Statcast(id);
            string jsonText = JsonConvert.SerializeObject(player);
            var requestContent = new System.Net.Http.StringContent(jsonText, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(url, requestContent).Result;
            string responseBody = response.Content.ReadAsStringAsync().Result;
            List<Statcast> data = JsonConvert.DeserializeObject<List<Statcast>>(responseBody);
            HashSet<string> hslinks = new HashSet<string>();
            for (int i = 0; i < data.Count; i++)
            {
                hslinks.Add(data[i].videoLink);
            }
            return hslinks;
        }

        public bool checkVideoLink(string link, HashSet<string> hs)
        {
            //
            // not in => true, otherwise => false
            //
            //string url = apiURL("getVideoLink"); //"http://localhost:20811/api/home/getVideoLink/";
            //HttpClient client = new HttpClient();
            //Statcast player = new Statcast(id);
            //string jsonText = JsonConvert.SerializeObject(player);
            //var requestContent = new System.Net.Http.StringContent(jsonText, System.Text.Encoding.UTF8, "application/json");
            //HttpResponseMessage response = client.PostAsync(url, requestContent).Result;
            //string responseBody = response.Content.ReadAsStringAsync().Result;
            //List<Statcast> data = JsonConvert.DeserializeObject<List<Statcast>>(responseBody);

            //HashSet<string> hs = new HashSet<string>();
            //for (int i = 0; i < data.Count; i++)
            //{
            //    hs.Add(data[i].videoLink);
            //}
            if (!hs.Contains(link))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //
        //get all recID
        //
        internal List<int> getRecID(int id)
        {
            SqlServer sql = new SqlServer();
            SqlConnection cnn = new SqlConnection(sql.connectTo());
            cnn.Open();
            SqlCommand myCommand = new SqlCommand();
            myCommand = cnn.CreateCommand();
            myCommand.CommandType = CommandType.Text;
            myCommand.CommandText = "select recID from pitcherData where playerID = @playerID order by recID";
            //myCommand.CommandText = "select * from dbo.pitcherData";
            myCommand.Parameters.AddWithValue("@playerID", id);
            myCommand.ExecuteNonQuery();  //執行以上的程式
            SqlDataAdapter myDataAdapter = new SqlDataAdapter(myCommand);
            cnn.Close();
            DataSet myDataSet = new DataSet();
            myDataAdapter.Fill(myDataSet);
            List<int> recID = new List<int>();
            foreach (DataRow myRow in myDataSet.Tables[0].Rows)
            {
                recID.Add(Convert.ToInt32(myRow[0]));
            }
            return recID;
        }
        internal void updateVideo(string videoLink, int recID)
        {
            SqlServer sql = new SqlServer();
            SqlConnection cnn = new SqlConnection(sql.connectTo());
            cnn.Open();
            SqlCommand myCommand = new SqlCommand();
            myCommand = cnn.CreateCommand();
            myCommand.CommandType = CommandType.Text;
            myCommand.CommandText = $"Update dbo.pitcherData set videoLink = @videoLink where recID = @recID";
            //myCommand.CommandText = "select * from dbo.pitcherData";
            myCommand.Parameters.AddWithValue("@videoLink", videoLink);
            myCommand.Parameters.AddWithValue("@recID", recID);
            
            myCommand.ExecuteNonQuery();  //執行以上的程式
            cnn.Close();
        }
        internal List<playerInfo> getPlayerName()
        {
            SqlServer sql = new SqlServer();
            SqlConnection cnn = new SqlConnection(sql.connectTo());
            cnn.Open();
            SqlCommand myCommand = new SqlCommand();
            myCommand = cnn.CreateCommand();
            myCommand.CommandType = CommandType.Text;
            myCommand.CommandText = $"select recID, pitcher from PitchAnalyze";
            myCommand.ExecuteNonQuery();  //執行以上的程式
            cnn.Close();
            SqlDataAdapter myDataAdapter = new SqlDataAdapter(myCommand);
            DataSet myDataSet = new DataSet();
            myDataAdapter.Fill(myDataSet);
            List<playerInfo> infos = new List<playerInfo>();
            foreach (DataRow myRow in myDataSet.Tables[0].Rows)
            {
                string[] _name = myRow[1].ToString().Split(',');
                string name = _name[1].Remove(0, 1).ToUpper() + ' ' + _name[0].ToUpper();
                playerInfo info = new playerInfo(Convert.ToInt32(myRow[0]), name);
                // The type of myRow is object, so must to use Convert.ToDouble(). double.Parse() is only for string type.
                infos.Add(info);
            }
            return infos;
        }

        internal void updateUpperName(int playerID, string name)
        {
            SqlServer sql = new SqlServer();
            SqlConnection cnn = new SqlConnection(sql.connectTo());
            cnn.Open();
            SqlCommand myCommand = new SqlCommand();
            myCommand = cnn.CreateCommand();
            myCommand.CommandType = CommandType.Text;
            myCommand.CommandText = $"Update playerInfo Set playerNameUpper = @playerNameUpper where playerID = @playerID";
            myCommand.Parameters.AddWithValue("@playerNameUpper", name);
            myCommand.Parameters.AddWithValue("@playerID", playerID);

            myCommand.ExecuteNonQuery();  //執行以上的程式
            cnn.Close();
        }
        internal string getNewImage(string name)
        {
            SqlServer sql = new SqlServer();
            SqlConnection cnn = new SqlConnection(sql.connectTo());
            cnn.Open();
            SqlCommand myCommand = new SqlCommand();
            myCommand = cnn.CreateCommand();
            myCommand.CommandType = CommandType.Text;
            myCommand.CommandText = $"Select distinct playerImage from playerNews where playerName = @playerName";
            myCommand.Parameters.AddWithValue("@playerName", name);
            myCommand.ExecuteNonQuery();  //執行以上的程式
            cnn.Close();
            SqlDataAdapter myDataAdapter = new SqlDataAdapter(myCommand);
            DataSet myDataSet = new DataSet();
            myDataAdapter.Fill(myDataSet);
            string image;
            try { image = myDataSet.Tables[0].Rows[0][0].ToString(); }
            catch { image = ""; }
            return image;
        }
        internal void updateImage(string name, string image)
        {
            SqlServer sql = new SqlServer();
            SqlConnection cnn = new SqlConnection(sql.connectTo());
            cnn.Open();
            SqlCommand myCommand = new SqlCommand();
            myCommand = cnn.CreateCommand();
            myCommand.CommandType = CommandType.Text;
            myCommand.CommandText = $"Update playerInfo set playerImageNew = @playerImageNew where playerName = @playerName";
            myCommand.Parameters.AddWithValue("@playerName", name);
            myCommand.Parameters.AddWithValue("@playerImageNew", image);
            myCommand.ExecuteNonQuery();  //執行以上的程式
            cnn.Close();
        }
        internal void updateName(int recID, string name)
        {
            SqlServer sql = new SqlServer();
            SqlConnection cnn = new SqlConnection(sql.connectTo());
            cnn.Open();
            SqlCommand myCommand = new SqlCommand();
            myCommand = cnn.CreateCommand();
            myCommand.CommandType = CommandType.Text;
            myCommand.CommandText = $"Update PitchAnalyze set pitcherNew = @pitcherNew where recID = @recID";
            myCommand.Parameters.AddWithValue("@pitcherNew", name);
            myCommand.Parameters.AddWithValue("@recID", recID);
            myCommand.ExecuteNonQuery();  //執行以上的程式
            cnn.Close();
        }
    }
    
}
