using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;
using System.Linq.Expressions;
using System.Web.UI.WebControls;
using System.Data.SqlClient;

namespace ProjectTemplate
{
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	[System.Web.Script.Services.ScriptService]

	public class ProjectServices : System.Web.Services.WebService
	{
		private string dbID = "springc2023team1";
		private string dbPass = "springc2023team1";
		private string dbName = "springc2023team1";
		
		// Provides connection string
		private string getConString() {
			return "SERVER=107.180.1.16; PORT=3306; DATABASE=" + dbName+"; UID=" + dbID + "; PASSWORD=" + dbPass;
		}

        [WebMethod(EnableSession = true)]
        public bool LogOn(string username, string password)
        {
            //we return this flag to tell them if they logged in or not
            bool success = false;

            //our connection string comes from our web.config file like we talked about earlier
/*            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
*/            //here's our query.  A basic select with nothing fancy.  Note the parameters that begin with @
            string sqlSelect = "SELECT username, email, is_mentor FROM mentorship_users WHERE username=@usernameValue and password=@passwordValue";

            // Create connection object using connection string method
            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            //set up our command object to use our connection, and our query
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            //tell our command to replace the @parameters with real values
            //we decode them because they came to us via the web so they were encoded
            //for transmission (funky characters escaped, mostly)
            sqlCommand.Parameters.AddWithValue("@usernameValue", HttpUtility.UrlDecode(username));
            sqlCommand.Parameters.AddWithValue("@passwordValue", HttpUtility.UrlDecode(password));

            //a data adapter acts like a bridge between our command object and 
            //the data we are trying to get back and put in a table object
            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            //here's the table we want to fill with the results from our query
            DataTable sqlDt = new DataTable();
            //here we go filling it!
            sqlDa.Fill(sqlDt);
            //check to see if any rows were returned.  If they were, it means it's 
            //a legit account
            if (sqlDt.Rows.Count > 0)
            {
                // store mentor status
                Session["username"] = sqlDt.Rows[0]["username"];
                Session["isMentor"] = sqlDt.Rows[0]["is_mentor"];
                success = true;
            }
            
            // if remain false, it means the query failed because the credentials were incorrect.
            return success;
        }

        [WebMethod(EnableSession = true)]
        public bool LogOff()
        {
            Session.Abandon();
            return true;
        }

        [WebMethod(EnableSession = true)]
        public bool CreateAccount(string username, string password, string email, string firstName, string isPhotographer, 
            string availability, string style, string type, string range, string experience, string sessionLength, string numOutfits, string imageURL)
        {
            string sqlSelect;

            //Insert statements depend on whether or not user is is_photographer
            if (isPhotographer == "Photographer")
            {

                sqlSelect = "insert into users (username, password, email, first_name, is_photographer) " +
                "values(@usernameValue, @passwordValue, @emailValue, @firstNameValue, 1);" +
                "insert into photographers " +
                "values(@usernameValue, @availabilityValue, @styleValue, @typeValue, @rangeValue, " +
                "@experienceValue, @sessionLengthValue, @numOutfitsValue, @imageURLValue);";
            }
            else
            {
                sqlSelect = "insert into users (username, password, email, first_name, is_photographer) " +
                "values(@usernameValue, @passwordValue, @emailValue, @firstNameValue, 0);" +
                "insert into clients (username, availability, style, type, budget_range, experience, session_length, num_outfits)" +
                "values(@usernameValue, @availabilityValue, @styleValue, @typeValue, @rangeValue, @experienceValue, @sessionLengthValue, @numOutfitsValue);";
            }

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@usernameValue", HttpUtility.UrlDecode(username));
            sqlCommand.Parameters.AddWithValue("@passwordValue", HttpUtility.UrlDecode(password));
            sqlCommand.Parameters.AddWithValue("@emailValue", HttpUtility.UrlDecode(email));
            sqlCommand.Parameters.AddWithValue("@firstNameValue", HttpUtility.UrlDecode(firstName));

            sqlCommand.Parameters.AddWithValue("@availabilityValue", HttpUtility.UrlDecode(availability));
            sqlCommand.Parameters.AddWithValue("@styleValue", HttpUtility.UrlDecode(style));
            sqlCommand.Parameters.AddWithValue("@typeValue", HttpUtility.UrlDecode(type));
            sqlCommand.Parameters.AddWithValue("@rangeValue", HttpUtility.UrlDecode(range));
            sqlCommand.Parameters.AddWithValue("@experienceValue", HttpUtility.UrlDecode(experience));
            sqlCommand.Parameters.AddWithValue("@sessionLengthValue", HttpUtility.UrlDecode(sessionLength));
            sqlCommand.Parameters.AddWithValue("@numOutfitsValue", HttpUtility.UrlDecode(numOutfits));
            sqlCommand.Parameters.AddWithValue("@imageURLValue", HttpUtility.UrlDecode(imageURL));


            sqlConnection.Open();

            try{
                sqlCommand.ExecuteNonQuery();
                sqlConnection.Close();
                //Set session variables 
                Session["username"] = username;
                if (isPhotographer == "Photographer")
                {
                    Session["isPhotographer"] = 1;
                }
                else
                {
                    Session["isPhotographer"] = 0;
                }
                return true;
            }
            catch
            {
                // Query will fail if username user submit already exists (primary key field)
                sqlConnection.Close();
                return false;
            }

        }

        [WebMethod(EnableSession =true)]
        public string returnPairings()
        {
            string sqlSelect;

            if (isMentorCheck())
            {
                sqlSelect = "SELECT u.username, u.first_name from connections c inner join mentorship_users u on c.mentee_username = u.username " +
                    "WHERE c.mentor_username = @usernameValue";
            }
            else
            {
                sqlSelect = "SELECT u.username, u.first_name from connections c inner join mentorship_users u on c.mentor_username = u.username " +
                    "WHERE c.mentee_username = @usernameValue";
            }

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
            sqlCommand.Parameters.AddWithValue("@usernameValue", HttpUtility.UrlDecode(Convert.ToString(Session["username"])));

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            DataTable sqlDt = new DataTable("pairings");
            sqlDa.Fill(sqlDt);

            string output = "[";

            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                output += "{" + "\"username\":\"" + sqlDt.Rows[i]["username"] + "\", \"firstName\":\"" + sqlDt.Rows[i]["first_name"] + "\"}";

                if (i != sqlDt.Rows.Count - 1)
                {
                    output += ",";
                }
            }

            output += "]";
            return output;
        }

        [WebMethod(EnableSession = true)]
        public bool parseMeetingSurvey(string subject_username, string meetingNotes, int rating, int effectiveness)
        {
            string sqlSelect;

            sqlSelect = "insert into survey_responses (respondent_username, subject_username, meeting_summary, overall_rating, effectiveness)" +
                        "values (@respondentUsernameValue, @subjectUsernameValue, @meetingSummaryValue, @overallRatingValue, @effectivenessValue);";

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@respondentUsernameValue", HttpUtility.UrlDecode(Convert.ToString(Session["username"])));
            sqlCommand.Parameters.AddWithValue("@subjectUsernameValue", HttpUtility.UrlDecode(subject_username));
            sqlCommand.Parameters.AddWithValue("@meetingSummaryValue", HttpUtility.UrlDecode(meetingNotes));
            sqlCommand.Parameters.AddWithValue("@overallRatingValue", rating);
            sqlCommand.Parameters.AddWithValue("@effectivenessValue", effectiveness);


            sqlConnection.Open();

            try
            {
                sqlCommand.ExecuteNonQuery();
                sqlConnection.Close();

                return true;
            }
            catch (Exception e)
            {
                sqlConnection.Close();
                return false;
            }
        }
        
        [WebMethod(EnableSession = true)]
        public bool scheduleMeeting(string otherUsername, string date)
        {
            string sqlInsert = "INSERT INTO meetings (mentor_username, mentee_username, date) VALUES(@mentorUsernameValue, @menteeUsernameValue, @dateValue)";
            string mentorUsername;
            string menteeUsername;

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlInsert, sqlConnection);

            if (isMentorCheck()){
                sqlCommand.Parameters.AddWithValue("@mentorUsernameValue", HttpUtility.UrlDecode(Convert.ToString(Session["username"])));
                sqlCommand.Parameters.AddWithValue("@menteeUsernameValue", HttpUtility.UrlDecode(otherUsername));

                mentorUsername = Convert.ToString(Session["username"]);
                menteeUsername = otherUsername;
            }
            else
            {
                sqlCommand.Parameters.AddWithValue("@mentorUsernameValue", HttpUtility.UrlDecode(otherUsername));
                sqlCommand.Parameters.AddWithValue("@menteeUsernameValue", HttpUtility.UrlDecode(Convert.ToString(Session["username"])));

                mentorUsername = otherUsername;  
                menteeUsername = Convert.ToString(Session["username"]);
            }

            sqlCommand.Parameters.AddWithValue("@dateValue", HttpUtility.UrlDecode(date));

            if (checkExistingMeeting(mentorUsername, menteeUsername, date))
            {
                return false;
            }
            else
            {
                sqlConnection.Open();
                sqlCommand.ExecuteNonQuery();
                sqlConnection.Close();
                return true;
            }
        }

        [WebMethod(EnableSession = true)]
        public bool checkExistingMeeting(string mentorUsername, string menteeUsername, string date)
        {
            string sqlSelect = "SELECT COUNT(*) FROM meetings where mentor_username = @mentorUsernameValue AND mentee_username = @menteeUsernameValue AND date = @dateValue";
            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);


            sqlCommand.Parameters.AddWithValue("@mentorUsernameValue", HttpUtility.UrlDecode(mentorUsername));
            sqlCommand.Parameters.AddWithValue("@menteeUsernameValue", HttpUtility.UrlDecode(menteeUsername));
            sqlCommand.Parameters.AddWithValue("@dateValue", HttpUtility.UrlDecode(date));

            sqlConnection.Open();

            if (Convert.ToInt32(sqlCommand.ExecuteScalar()) == 1 ){
                return true;
            }
            else
            {
                return false;
            }

        }

        [WebMethod(EnableSession =true)]
        public bool isMentorCheck()
        {
            if (Convert.ToInt32(Session["isMentor"]) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [WebMethod(EnableSession =true)]
        public string getMeetings()
        {
            string output = "[";
            string sqlSelect;
            MySqlConnection sqlConnection = new MySqlConnection(getConString());

            if (isMentorCheck())
            {
                sqlSelect = "SELECT m.meeting_id, u.first_name, m.date FROM meetings m INNER JOIN mentorship_users u on m.mentee_username = u.username " +
                    "WHERE mentor_username = @mentorUsernameValue ORDER BY 2";
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@mentorUsernameValue", HttpUtility.UrlDecode(Convert.ToString(Session["username"])));
                MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
                DataTable sqlDt = new DataTable("meetingsWithMentees");
                sqlDa.Fill(sqlDt);

                for (int i = 0; i < sqlDt.Rows.Count; i++)
                {
                    output += "{" + "\"meetingID\":" + sqlDt.Rows[i]["meeting_id"] + "\",\"connection\":\"" + sqlDt.Rows[i]["first_name"] + "\",\"date\":\"" + 
                        Convert.ToDateTime(sqlDt.Rows[i]["date"]).ToShortDateString() + "\"}";

                    if (i != sqlDt.Rows.Count - 1)
                    {
                        output += ",";
                    }

                }

            }
            else
            {
                sqlSelect = "SELECT m.meeting_id, u.first_name, m.date FROM meetings m INNER JOIN mentorship_users u on m.mentor_username = u.username " +
                    "WHERE mentee_username = @menteeUsernameValue ORDER BY 2";
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@menteeUsernameValue", HttpUtility.UrlDecode(Convert.ToString(Session["username"])));
                MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
                DataTable sqlDt = new DataTable("meetingsWithMentors");
                sqlDa.Fill(sqlDt);

                for (int i = 0; i < sqlDt.Rows.Count; i++)
                {
                    output += "{" + "\"meetingID\":" + sqlDt.Rows[i]["meeting_id"] + "\",\"connection\":\"" + sqlDt.Rows[i]["first_name"] + "\",\"date\":\"" +
                        Convert.ToDateTime(sqlDt.Rows[i]["date"]).ToShortDateString() + "\"}";

                    if (i != sqlDt.Rows.Count - 1)
                    {
                        output += ",";
                    }
                }

            }

            return output;

        }
    }
}
