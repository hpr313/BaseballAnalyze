using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace Statcast_WebApplication.Controllers
{
    [Route("api/home")]
    [ApiController]


    public class StatcastController : ControllerBase
    {
        [HttpPost]
        [Route("insertPlayerInfo")]

        public void insertPlayerInfo(Statcast xxx)
        {
            SqlServer sql = new SqlServer();
            SqlConnection cnn = new SqlConnection(sql.connectTo());
            cnn.Open();
            SqlCommand myCommand = new SqlCommand();
            myCommand = cnn.CreateCommand();
            myCommand.CommandType = CommandType.Text;
            myCommand.CommandText = "Insert into dbo.playerInfo (playerID, playerName, playerImage) VALUES (@playerID, @playerName, @playerImage)";
            myCommand.Parameters.AddWithValue("@playerID", xxx.playerID);
            //myCommand.Parameters["@playerID"].Value = xxx.playerID;
            myCommand.Parameters.AddWithValue("@playerName", xxx.pitcher);
            myCommand.Parameters.AddWithValue("@playerImage", xxx.playerImage);
            myCommand.ExecuteNonQuery();  //執行以上的程式
            cnn.Close();
        }

        [HttpPost]
        [Route("searchPlayerName")]

        public void searchPlayerName(string xxx)
        {
            SqlServer sql = new SqlServer();
            SqlConnection cnn = new SqlConnection(sql.connectTo());
            cnn.Open();
            SqlCommand myCommand = new SqlCommand();
            myCommand = cnn.CreateCommand();
            myCommand.CommandType = CommandType.Text;
            myCommand.CommandText = "Select playerName from dbo.playerInfo where upper(playerName) like upper(@name)";
            myCommand.Parameters.AddWithValue("@name", "%" + xxx + "%");
            myCommand.ExecuteNonQuery();  //執行以上的程式
            cnn.Close();
        }

        [HttpPost]
        [Route("insertPitcherData")]

        public void insertPitcherData(Statcast xxx)
        {
            SqlServer sql = new SqlServer();
            SqlConnection cnn = new SqlConnection(sql.connectTo());
            cnn.Open();
            SqlCommand myCommand = new SqlCommand();
            myCommand = cnn.CreateCommand();
            myCommand.CommandType = CommandType.StoredProcedure;//宣告為SP
            myCommand.CommandText = "insert_pitcherData";
            // Add playerID
            myCommand.Parameters.AddWithValue("@playerID", xxx.playerID);
            // Add pitch
            if (!(xxx.pitch == null)) { myCommand.Parameters.AddWithValue("@pitch", xxx.pitch); }
            else { myCommand.Parameters.AddWithValue("@pitch", DBNull.Value); }
            // Add MPH
            if (!(xxx.mph == null)) { myCommand.Parameters.AddWithValue("@mph", xxx.mph); }
            else { myCommand.Parameters.AddWithValue("@mph", DBNull.Value); }
            // Add evMPH
            if (!(xxx.evMph == null)) { myCommand.Parameters.AddWithValue("@evMph", xxx.evMph); }
            else { myCommand.Parameters.AddWithValue("@evMph", DBNull.Value); }
            // Add pitcher
            myCommand.Parameters.AddWithValue("@pitcher", xxx.pitcher);
            // Add batter
            myCommand.Parameters.AddWithValue("@batter", xxx.batter);
            // Add distance
            if (!(xxx.distance == null)) { myCommand.Parameters.AddWithValue("@distance", xxx.distance); }
            else { myCommand.Parameters.AddWithValue("@distance", DBNull.Value); }
            // Add spinRate
            if (!(xxx.spinRate == null)) { myCommand.Parameters.AddWithValue("@spinRate", xxx.spinRate); }
            else { myCommand.Parameters.AddWithValue("@spinRate", DBNull.Value); }
            // Add la
            if (!(xxx.la == null)) { myCommand.Parameters.AddWithValue("@la", xxx.la); }
            else { myCommand.Parameters.AddWithValue("@la", DBNull.Value); }
            // Add batterResult
            if (!(xxx.batterResult == null)) { myCommand.Parameters.AddWithValue("@batterResult", xxx.batterResult); }
            else { myCommand.Parameters.AddWithValue("@batterResult", DBNull.Value); }
            // Add hitType
            if (!(xxx.hitType == null)) { myCommand.Parameters.AddWithValue("@hitType", xxx.hitType); }
            else { myCommand.Parameters.AddWithValue("@hitType", DBNull.Value); }
            // Add hitSpeed
            if (!(xxx.hitSpeed == null)) { myCommand.Parameters.AddWithValue("@hitSpeed", xxx.hitSpeed); }
            else { myCommand.Parameters.AddWithValue("@hitSpeed", DBNull.Value); }
            // Add batterID
            if (!(xxx.batterID == null)) { myCommand.Parameters.AddWithValue("@batterID", xxx.batterID); }
            else { myCommand.Parameters.AddWithValue("@batterID", DBNull.Value); }
            // Add zone
            if (!(xxx.zone == null)) { myCommand.Parameters.AddWithValue("@zone", xxx.zone); }
            else { myCommand.Parameters.AddWithValue("@zone", DBNull.Value); }
            // Add others......
            myCommand.Parameters.AddWithValue("@gameDate", xxx.date);
            myCommand.Parameters.AddWithValue("@pitchCount", xxx.count);
            myCommand.Parameters.AddWithValue("@pitchResult", xxx.pitchResult);
            myCommand.Parameters.AddWithValue("@paResult", xxx.paResult);
            myCommand.Parameters.AddWithValue("@inning", xxx.inning);
            myCommand.Parameters.AddWithValue("@videoLink", xxx.videoLink);
            myCommand.ExecuteNonQuery();
            cnn.Close();
        }

        [HttpGet]
        [Route("getPlayerInfo")]

        public List<playerInfo> getPlayerInfo()
        {
            SqlServer sql = new SqlServer();
            SqlConnection cnn = new SqlConnection(sql.connectTo());
            cnn.Open();
            SqlCommand myCommand = new SqlCommand();
            myCommand = cnn.CreateCommand();
            myCommand.CommandType = CommandType.Text;
            myCommand.CommandText = "select distinct pitcherData.playerID, playerName, playerImageNew from playerInfo " +
                                    "inner join pitcherData " +
                                    "on pitcherData.playerID = playerInfo.playerID";
            myCommand.ExecuteNonQuery();  //執行以上的程式
            SqlDataAdapter myDataAdapter = new SqlDataAdapter(myCommand);
            cnn.Close();
            DataSet myDataSet = new DataSet();
            myDataAdapter.Fill(myDataSet);
            List<playerInfo> infos = new List<playerInfo>();
            foreach (DataRow myRow in myDataSet.Tables[0].Rows)
            {
                playerInfo info = new playerInfo(Convert.ToInt32(myRow[0]), myRow[1].ToString(), myRow[2].ToString());
                // The type of myRow is object, so must to use Convert.ToDouble(). double.Parse() is only for string type.
                infos.Add(info);
            }
            return infos;
        }

        [HttpGet]
        [Route("getBatterResult")]

        public List<Statcast> getBatterResult()
        {
            SqlServer sql = new SqlServer();
            SqlConnection cnn = new SqlConnection(sql.connectTo());
            cnn.Open();
            SqlCommand myCommand = new SqlCommand();
            myCommand = cnn.CreateCommand();
            myCommand.CommandType = CommandType.Text;
            myCommand.CommandText = "select * from dbo.pitcherData where pitchResult = 'Hit Into Play' and batterResult is Null";
            //myCommand.CommandText = "select * from dbo.pitcherData";

            myCommand.ExecuteNonQuery();  //執行以上的程式
            SqlDataAdapter myDataAdapter = new SqlDataAdapter(myCommand);
            cnn.Close();
            DataSet myDataSet = new DataSet();
            myDataAdapter.Fill(myDataSet);
            List<Statcast> newsList = new List<Statcast>();
            foreach (DataRow myRow in myDataSet.Tables[0].Rows)
            {
                Statcast statcast = new Statcast();
                // The type of myRow is object, so must to use Convert.ToDouble(). double.Parse() is only for string type.
                try { statcast.evMph = Convert.ToDouble(myRow[7]); }
                catch (InvalidCastException e) { statcast.evMph = null; }
                statcast.pitchResult = myRow[13].ToString();
                statcast.paResult = myRow[14].ToString();
                statcast.recID = Convert.ToInt32(myRow[15]);
                statcast.batterResult = myRow[17].ToString();
                statcast.hitType = myRow[18].ToString();
                statcast.hitSpeed = myRow[19].ToString();
                newsList.Add(statcast);
            }
            return newsList;
        }

        [HttpPost]
        [Route("getVideoLink")]
        public List<Statcast> getVideoLink(Statcast xxx)
        {
            SqlServer sql = new SqlServer();
            SqlConnection cnn = new SqlConnection(sql.connectTo());
            cnn.Open();
            SqlCommand myCommand = new SqlCommand();
            myCommand = cnn.CreateCommand();
            myCommand.CommandType = CommandType.Text;
            myCommand.CommandText = "Select videoLink from pitcherData where playerID = @playerID";
            myCommand.Parameters.AddWithValue("@playerID", xxx.playerID);
            myCommand.ExecuteNonQuery();
            SqlDataAdapter myDataAdapter = new SqlDataAdapter(myCommand);
            cnn.Close();
            DataSet myDataSet = new DataSet();
            myDataAdapter.Fill(myDataSet);
            List<Statcast> videoList = new List<Statcast>();
            foreach (DataRow myRow in myDataSet.Tables[0].Rows)
            {
                Statcast video = new Statcast(myRow[0].ToString());
                videoList.Add(video);
            }
            return videoList;
        }
        [HttpGet]
        [Route("getPlayerName")]
        public List<string> getPlayerName()
        {
            SqlServer sql = new SqlServer();
            string cmdText = @"Select playerName from dbo.playerInfo order by playerName";
            DataSet myDataSet = sql.getDataByText(cmdText);
            List<string> names = new List<string>();
            foreach (DataRow myRow in myDataSet.Tables[0].Rows)
            {
                names.Add(myRow[0].ToString());
            }
            return names;
        }

        [HttpPost]
        [Route("updateBatterResult")]

        public void updateResult(Statcast xxx)
        {
            SqlServer sql = new SqlServer();
            SqlConnection cnn = new SqlConnection(sql.connectTo());
            cnn.Open();
            SqlCommand myCommand = new SqlCommand();
            myCommand = cnn.CreateCommand();
            myCommand.CommandType = CommandType.Text;
            //myCommand.CommandText = "Select playerName from dbo.playerInfo where upper(playerName) like upper(@name)";
            myCommand.CommandText = "UPdate dbo.pitcherData set batterResult = @batterResult, hitType = @hitType, hitSpeed = @hitSpeed where recID = @recID";
            if (!(xxx.batterResult == null)) { myCommand.Parameters.AddWithValue("@batterResult", xxx.batterResult); }
            else { myCommand.Parameters.AddWithValue("@batterResult", DBNull.Value); }
            // Add hitType
            if (!(xxx.hitType == null)) { myCommand.Parameters.AddWithValue("@hitType", xxx.hitType); }
            else { myCommand.Parameters.AddWithValue("@hitType", DBNull.Value); }
            // Add hitSpeed
            if (!(xxx.hitSpeed == null)) { myCommand.Parameters.AddWithValue("@hitSpeed", xxx.hitSpeed); }
            else { myCommand.Parameters.AddWithValue("@hitSpeed", DBNull.Value); }
            myCommand.Parameters.AddWithValue("@recID", xxx.recID);
            myCommand.ExecuteNonQuery();  //執行以上的程式
            cnn.Close();
        }

        [HttpGet]
        [Route("getCurrentIDs")]

        public List<int> getCurrentIDs()
        {
            SqlServer sql = new SqlServer();
            SqlConnection cnn = new SqlConnection(sql.connectTo());
            cnn.Open();
            SqlCommand myCommand = new SqlCommand();
            myCommand = cnn.CreateCommand();
            myCommand.CommandType = CommandType.Text;
            myCommand.CommandText = "select distinct playerID from pitcherData where videoLink is Null";
            //myCommand.CommandText = "select * from dbo.pitcherData";

            myCommand.ExecuteNonQuery();  //執行以上的程式
            SqlDataAdapter myDataAdapter = new SqlDataAdapter(myCommand);
            cnn.Close();
            DataSet myDataSet = new DataSet();
            myDataAdapter.Fill(myDataSet);
            List<int> Ids = new List<int>();
            foreach (DataRow myRow in myDataSet.Tables[0].Rows)
            {
                Ids.Add(Convert.ToInt32(myRow[0]));
            }
            return Ids;
        }

        [HttpPost]
        [Route("getRecID")]
        public List<int> RecID(Statcast xxx)
        {
            SqlServer sql = new SqlServer();
            SqlConnection cnn = new SqlConnection(sql.connectTo());
            cnn.Open();
            SqlCommand myCommand = new SqlCommand();
            myCommand = cnn.CreateCommand();
            myCommand.CommandType = CommandType.Text;
            myCommand.CommandText = "select recID from pitcherData where playerID = @playerID order by recID";
            //myCommand.CommandText = "select * from dbo.pitcherData";
            myCommand.Parameters.AddWithValue("@playerID", xxx.playerID);
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
        [HttpGet]
        [Route("getPitchAnalyze")]
        public List<PitchAnalyze> getPitchAnalyze()
        {
            SqlServer sql = new SqlServer();
            SqlConnection cnn = new SqlConnection(sql.connectTo());
            cnn.Open();
            SqlCommand myCommand = new SqlCommand();
            myCommand = cnn.CreateCommand();
            myCommand.CommandType = CommandType.Text;
            myCommand.CommandText = "select * from PitchAnalyze";
            myCommand.ExecuteNonQuery();  //執行以上的程式
            SqlDataAdapter myDataAdapter = new SqlDataAdapter(myCommand);
            cnn.Close();
            DataSet myDataSet = new DataSet();
            myDataAdapter.Fill(myDataSet);
            List<PitchAnalyze> analyzes = new List<PitchAnalyze>();
            foreach (DataRow myRow in myDataSet.Tables[0].Rows)
            {
                PitchAnalyze analyze = new PitchAnalyze();
                analyze.playerID = Convert.ToInt32(myRow[1]);
                //analyze.pitcher = myRow[2].ToString();
                analyze.gameYear = Convert.ToInt32(myRow[3]);
                analyze.gameMonth = Convert.ToInt32(myRow[4]);
                analyze.pitch = myRow[5].ToString();
                //analyze.totalPitchCount = Convert.ToInt32(myRow[6]);
                analyze.ballPercentage = Convert.ToDecimal(myRow[7]);
                analyze.SwStPercentage = Convert.ToDecimal(myRow[8]);
                analyze.callStPercentage = Convert.ToDecimal(myRow[9]);
                analyze.foulPercentage = Convert.ToDecimal(myRow[10]);
                analyze.softPercentage = Convert.ToDecimal(myRow[13]);
                analyze.mediumPercentage = Convert.ToDecimal(myRow[14]);
                analyze.sharpPercentage = Convert.ToDecimal(myRow[15]);
                analyze.totalCount = Convert.ToInt32(myRow[17]);
                analyze.AverageMPH = Convert.ToDecimal(myRow[18]);
                analyze.AverageSpin = Convert.ToDecimal(myRow[19]);
                analyzes.Add(analyze);
            }
            return analyzes;
        }
    }
    public class SqlServer
    {
        public string connectTo()
        {
            string connetionString = $@"data source=localhost;
                                initial catalog=Baseball;user id=sa;
                                persist security info=True;
                                password=1qaz2wsx;
                                workstation id=LAPTOP-CSG847EV;
                                packet size=4096;";
            return connetionString;
        }
        public DataSet getDataByText(string commandText)
        {
            SqlServer sql = new SqlServer();
            SqlConnection cnn = new SqlConnection(sql.connectTo());
            cnn.Open();
            SqlCommand myCommand = new SqlCommand();
            myCommand = cnn.CreateCommand();
            myCommand.CommandType = CommandType.Text;
            myCommand.CommandText = @commandText;
            myCommand.ExecuteNonQuery();
            SqlDataAdapter myDataAdapter = new SqlDataAdapter(myCommand);
            cnn.Close();
            DataSet myDataSet = new DataSet();
            myDataAdapter.Fill(myDataSet);
            return myDataSet;
        }
    }
}
