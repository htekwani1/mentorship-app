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
using Xceed.Wpf.Toolkit;

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
        public string CreateAccount(string username, string password, string email, string firstName, string lastName, 
            string isMentor, string pointsGoal, string mentorUsername)
        {
            string sqlSelect;

            //Insert statements depend on whether or not user is is_mentor
            //An initial point value of 0 is inserted into points column of mentorship_users table because new users will not have any points right after making an account
            //An initial point value of 0 is inserted into relationship_count column of mentors table because they do not need to enter a mentee's username to make an account while mentees table's relationship_count column will begin with a value of 1 because mentees need to enter a mentor username to create a mentee account, meaning they have a connection
            if (isMentor == "Mentor")
            {
                sqlSelect = "insert into mentorship_users (username, password, email, first_name, last_name, points_goal, is_mentor) " +
                "values(@usernameValue, @passwordValue, @emailValue, @firstNameValue, @lastNameValue, @pointsGoalValue, 1);" +
                "insert into mentors " +
                "values(@usernameValue)";
            }
            else
            {
                sqlSelect = "insert into mentorship_users (username, password, email, first_name, last_name, points_goal, is_mentor) " +
                "values(@usernameValue, @passwordValue, @emailValue, @firstNameValue, @lastNameValue, @pointsGoalValue, 0);" +
                "insert into mentees " +
                "values(@usernameValue);";
            }

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@usernameValue", HttpUtility.UrlDecode(username));
            sqlCommand.Parameters.AddWithValue("@passwordValue", HttpUtility.UrlDecode(password));
            sqlCommand.Parameters.AddWithValue("@emailValue", HttpUtility.UrlDecode(email));
            sqlCommand.Parameters.AddWithValue("@firstNameValue", HttpUtility.UrlDecode(firstName));
            sqlCommand.Parameters.AddWithValue("@lastNameValue", HttpUtility.UrlDecode(lastName));

            sqlCommand.Parameters.AddWithValue("@pointsGoalValue", HttpUtility.UrlDecode(pointsGoal));
            sqlCommand.Parameters.AddWithValue("@mentorUsernameValue", HttpUtility.UrlDecode(mentorUsername));

            // sqlCommand.Parameters.AddWithValue("@availabilityValue", HttpUtility.UrlDecode(availability));
            // sqlCommand.Parameters.AddWithValue("@styleValue", HttpUtility.UrlDecode(style));
            // sqlCommand.Parameters.AddWithValue("@typeValue", HttpUtility.UrlDecode(type));
            // sqlCommand.Parameters.AddWithValue("@rangeValue", HttpUtility.UrlDecode(range));
            // sqlCommand.Parameters.AddWithValue("@experienceValue", HttpUtility.UrlDecode(experience));
            // sqlCommand.Parameters.AddWithValue("@sessionLengthValue", HttpUtility.UrlDecode(sessionLength));
            // sqlCommand.Parameters.AddWithValue("@numOutfitsValue", HttpUtility.UrlDecode(numOutfits));
            // sqlCommand.Parameters.AddWithValue("@imageURLValue", HttpUtility.UrlDecode(imageURL));


            sqlConnection.Open();

            try{
                sqlCommand.ExecuteNonQuery();
                sqlConnection.Close();
                //Set session variables 
                Session["username"] = username;
                if (isMentor == "Mentor")
                {
                    Session["isMentor"] = 1;
                }
                else
                {
                    Session["isMentor"] = 0;
                    addConnection(username, mentorUsername);
                }

                return "Success";
            }
            catch(Exception e)
            {
                // Query will fail if username user submit already exists (primary key field)
                sqlConnection.Close();
                return e.Message;
            }

        }

        // check to see if the mentor username provided actually exists
        [WebMethod(EnableSession = true)]
        public bool checkValidMentor(string mentorUsername)
        {
            string sqlSelect = "SELECT COUNT(*) from mentors WHERE username = @mentorUsernameValue";

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
            sqlCommand.Parameters.AddWithValue("@mentorUsernameValue", HttpUtility.UrlDecode(mentorUsername));

            try
            {
                sqlConnection.Open();
                if (Convert.ToInt32(sqlCommand.ExecuteScalar()) == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return false;

            }
        }

        // handle all the different operations that comes with inserting a new connection
        [WebMethod(EnableSession = true)]
        public bool addConnection(string menteeUsername, string mentorUsername)
        {
            // first check if the mentor username provided actually exists
            if (!checkValidMentor(mentorUsername))
            {
                throw new Exception("Invalid mentor username!");
            }
            else
            {
                // then check if the connection already exists in the connections table
                if(checkConnection(menteeUsername, mentorUsername)){
                    throw new Exception("Connection already exists.");
                }
                else
                {
                    if(checkConnectionsLimit(menteeUsername, mentorUsername) == "Mentor")
                    {
                        throw new Exception("Mentor has exceeded connections limit.");
                    }
                    else if (checkConnectionsLimit(menteeUsername, mentorUsername) == "Mentee")
                    {
                        throw new Exception("You have exceeded the connections limit.");
                    }
                    else
                    {
                        // finally insert our connections if we pass other conditions
                        string sqlSelect = "INSERT INTO connections VALUES(@menteeUsernameValue, @mentorUsernameValue); " +
                            "UPDATE mentorship_users SET connections_count = connections_count + 1 WHERE username IN (@menteeUsernameValue, @mentorUsernameValue);";

                        MySqlConnection sqlConnection = new MySqlConnection(getConString());
                        MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
                        sqlCommand.Parameters.AddWithValue("@menteeUsernameValue", HttpUtility.UrlDecode(menteeUsername));
                        sqlCommand.Parameters.AddWithValue("@mentorUsernameValue", HttpUtility.UrlDecode(mentorUsername));


                        try
                        {
                            sqlConnection.Open();
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

                }
            }
        }

        // check if either the mentor or mentee already has 5 connections
        [WebMethod(EnableSession = true)]
        public string checkConnectionsLimit(string menteeUsername, string mentorUsername)
        {
            if(getConnectionsCount(mentorUsername) == 5)
            {
                return "Mentor";
            }
            else if(getConnectionsCount(menteeUsername) == 5)
            {
                return "Mentee";
            }
            else
            {
                return "";
            }
        }

        [WebMethod(EnableSession = true)]
        public int getConnectionsCount(string username)
        {
            string sqlSelect = "SELECT connections_count FROM mentorship_users WHERE username = @usernameValue";
            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
            sqlCommand.Parameters.AddWithValue("@usernameValue", HttpUtility.UrlDecode(username));

            sqlConnection.Open();
            return Convert.ToInt32(sqlCommand.ExecuteScalar());

        }

        //check if connection exists
        [WebMethod(EnableSession = true)]
        public bool checkConnection(string menteeUsername, string mentorUsername)
        {
            string sqlSelect = "SELECT COUNT(*) FROM connections WHERE mentee_username = @menteeUsernameValue AND mentor_username = @mentorUsernameValue";
            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
            sqlCommand.Parameters.AddWithValue("@menteeUsernameValue", HttpUtility.UrlDecode(menteeUsername));
            sqlCommand.Parameters.AddWithValue("@mentorUsernameValue", HttpUtility.UrlDecode(mentorUsername));


            try
            {
                sqlConnection.Open();
                if (Convert.ToInt32(sqlCommand.ExecuteScalar()) == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return false;

            }
        }

        [WebMethod(EnableSession =true)]
        public string returnPairings()
        {
            string sqlSelect;

            if (isMentorCheck())
            {
                sqlSelect = "SELECT u.username, u.first_name, u.last_name from connections c inner join mentorship_users u on c.mentee_username = u.username " +
                    "WHERE c.mentor_username = @usernameValue";
            }
            else
            {
                sqlSelect = "SELECT u.username, u.first_name, u.last_name from connections c inner join mentorship_users u on c.mentor_username = u.username " +
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
                output += "{" + "\"username\":\"" + sqlDt.Rows[i]["username"] + "\", \"firstName\":\"" + sqlDt.Rows[i]["first_name"] + 
                    "\",\"lastName\":\"" + sqlDt.Rows[i]["last_name"] + "\", \"meetingResponses\":";

                output += getSurveyResponseDateID(Convert.ToString(sqlDt.Rows[i]["username"]));
                output+= "}";

                if (i != sqlDt.Rows.Count - 1)
                {
                    output += ",";
                }
            }

            output += "]";
            return output;
        }

        [WebMethod(EnableSession = true)]
        public bool scheduleMeeting(string otherUsername, string date)
        {
            string sqlInsert = "INSERT INTO meetings (mentor_username, mentee_username, date) VALUES(@mentorUsernameValue, @menteeUsernameValue, @dateValue)";
            string mentorUsername;
            string menteeUsername;

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlInsert, sqlConnection);

            if (isMentorCheck())
            {
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
        public bool parseMeetingSurvey(int meetingID, string subjectUsername, string meetingNotes, int rating, int effectiveness, int didLearn, int didBenefit, int meetingLength, int finalPoints)
        {
            string sqlSelect;

            sqlSelect = "insert into survey_responses (meeting_id, respondent_username, meeting_summary, overall_rating, effectiveness, didLearn, didBenefit, meetingLength)" +
                        "values (@meetingIDValue, @respondentUsernameValue, @meetingSummaryValue, @overallRatingValue, @effectivenessValue, @didLearnValue, @didBenefitValue, @meetingLengthValue);" +
                        "update mentorship_users set points = (points + @finalPointsValue), redeemable_points= (redeemable_points + @finalPointsValue) where username= @subjectUsernameValue;";

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@meetingIDValue", meetingID);
            sqlCommand.Parameters.AddWithValue("@respondentUsernameValue", HttpUtility.UrlDecode(Convert.ToString(Session["username"])));
            sqlCommand.Parameters.AddWithValue("@meetingSummaryValue", HttpUtility.UrlDecode(meetingNotes));
            sqlCommand.Parameters.AddWithValue("@overallRatingValue", rating);
            sqlCommand.Parameters.AddWithValue("@effectivenessValue", effectiveness);
            sqlCommand.Parameters.AddWithValue("@didLearnValue", didLearn);
            sqlCommand.Parameters.AddWithValue("@didBenefitValue", didBenefit);
            sqlCommand.Parameters.AddWithValue("@meetingLengthValue", meetingLength);
            sqlCommand.Parameters.AddWithValue("@subjectUsernameValue", subjectUsername);
            sqlCommand.Parameters.AddWithValue("@finalPointsValue", finalPoints);



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
        public string getMeetingsNoResponse()
        {
            string output = "[";
            string sqlSelect;
            MySqlConnection sqlConnection = new MySqlConnection(getConString());

            if (isMentorCheck())
            {
                sqlSelect = "SELECT m.meeting_id, u.first_name, u.last_name, u.username, m.date " +
                    "FROM meetings m INNER JOIN mentorship_users u on m.mentee_username = u.username " +
                    "WHERE mentor_username = @mentorUsernameValue " +
                    "AND m.meeting_id NOT IN (SELECT meeting_id FROM survey_responses WHERE respondent_username = @mentorUsernameValue) " +
                    "ORDER BY 2";
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@mentorUsernameValue", HttpUtility.UrlDecode(Convert.ToString(Session["username"])));
                MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
                DataTable sqlDt = new DataTable("meetingsWithMentees");
                sqlDa.Fill(sqlDt);

                for (int i = 0; i < sqlDt.Rows.Count; i++)
                {
                    output += "{" + "\"meetingID\":\"" + sqlDt.Rows[i]["meeting_id"] + "\",\"firstName\":\"" + sqlDt.Rows[i]["first_name"] +
                        "\",\"lastName\":\"" + sqlDt.Rows[i]["last_name"] + "\", \"username\":\"" + sqlDt.Rows[i]["username"] + 
                        "\",\"date\":\"" + Convert.ToDateTime(sqlDt.Rows[i]["date"]).ToShortDateString() + "\"}";

                    if (i != sqlDt.Rows.Count - 1)
                    {
                        output += ",";
                    }
                }
            }
            else
            {
                sqlSelect = "SELECT m.meeting_id, u.first_name, u.last_name, u.username, m.date " +
                    "FROM meetings m INNER JOIN mentorship_users u on m.mentor_username = u.username " +
                    "WHERE mentee_username = @menteeUsernameValue " +
                    "AND m.meeting_id NOT IN (SELECT meeting_id FROM survey_responses WHERE respondent_username = @menteeUsernameValue) " +
                    "ORDER BY 2";
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@menteeUsernameValue", HttpUtility.UrlDecode(Convert.ToString(Session["username"])));
                MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
                DataTable sqlDt = new DataTable("meetingsWithMentors");
                sqlDa.Fill(sqlDt);

                for (int i = 0; i < sqlDt.Rows.Count; i++)
                {
                    output += "{" + "\"meetingID\":\"" + sqlDt.Rows[i]["meeting_id"] + "\",\"firstName\":\"" + sqlDt.Rows[i]["first_name"] +
                        "\",\"lastName\":\"" + sqlDt.Rows[i]["last_name"] + "\", \"username\":\"" + sqlDt.Rows[i]["username"] +
                        "\",\"date\":\"" + Convert.ToDateTime(sqlDt.Rows[i]["date"]).ToShortDateString() + "\"}";

                    if (i != sqlDt.Rows.Count - 1)
                    {
                        output += ",";
                    }
                }

            }
            output += "]";
            return output;

        }

        [WebMethod(EnableSession = true)]
        public string getSurveyResponseDateID(string connectionUsername)
        {
            string sqlSelect;
            MySqlConnection sqlConnection;
            MySqlCommand sqlCommand;

            if (isMentorCheck())
            {
                sqlSelect = "SELECT r.response_id, m.date " +
                    "FROM survey_responses r INNER JOIN meetings m " +
                    "ON r.meeting_id = m.meeting_id " +
                    "WHERE r.respondent_username = @usernameValue AND m.mentee_username = @menteeUsernameValue";

                sqlConnection = new MySqlConnection(getConString());
                sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@usernameValue", HttpUtility.UrlDecode(Convert.ToString(Session["username"])));
                sqlCommand.Parameters.AddWithValue("@menteeUsernameValue", HttpUtility.UrlDecode(connectionUsername));


            }
            else
            {
                sqlSelect = "SELECT r.response_id, m.date " +
                    "FROM survey_responses r INNER JOIN meetings m " +
                    "ON r.meeting_id = m.meeting_id " +
                    "WHERE r.respondent_username = @usernameValue AND m.mentor_username = @mentorUsernameValue";
                
                sqlConnection = new MySqlConnection(getConString());
                sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@usernameValue", HttpUtility.UrlDecode(Convert.ToString(Session["username"])));
                sqlCommand.Parameters.AddWithValue("@mentorUsernameValue", HttpUtility.UrlDecode(connectionUsername));

            }


            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            DataTable sqlDt = new DataTable("surveyResponseDateID");
            sqlDa.Fill(sqlDt);

            string output = "[";

            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                output += "{" + "\"responseID\":\"" + sqlDt.Rows[i]["response_id"] + 
                    "\",\"date\":\"" + Convert.ToDateTime(sqlDt.Rows[i]["date"]).ToShortDateString() + "\"}";

                if (i != sqlDt.Rows.Count - 1)
                {
                    output += ",";
                }
            }

            output += "]";
            return output;

        }

        [WebMethod(EnableSession = true)]
        public string getSurveyResponse(int surveyResponseID)
        {
            string sqlSelect = "SELECT meeting_summary FROM survey_responses WHERE response_id = @surveyResponseIDValue";
            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
            sqlCommand.Parameters.AddWithValue("@surveyResponseIDValue", surveyResponseID);

            sqlConnection.Open();
            return Convert.ToString(sqlCommand.ExecuteScalar());

        }

        [WebMethod(EnableSession = true)]
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

        [WebMethod(EnableSession = true)]
        public int getPoints()
        {
            string sqlSelect = "SELECT redeemable_points FROM mentorship_users WHERE username = @usernameValue";

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
            sqlCommand.Parameters.AddWithValue("@usernameValue", HttpUtility.UrlDecode(Convert.ToString(Session["username"])));

            sqlConnection.Open();

            return Convert.ToInt32(sqlCommand.ExecuteScalar());
        }

        [WebMethod(EnableSession = true)]
        public string addMentor(string mentorUsername)
        {
            try {
                addConnection(Convert.ToString(Session["username"]), mentorUsername);
                return "Success";
            }
            catch (Exception e) {
                return e.Message;
            }
            
        }

        [WebMethod(EnableSession = true)]
        public bool updatePoints(int points)
        {
            string sqlUpdate = "UPDATE mentorship_users set redeemable_points = @pointsValue where username = @usernameValue";

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlUpdate, sqlConnection);
            sqlCommand.Parameters.AddWithValue("@usernameValue", HttpUtility.UrlDecode(Convert.ToString(Session["username"])));
            sqlCommand.Parameters.AddWithValue("@pointsValue", points);

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
    }
}
