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
            string sqlSelect = "SELECT username, email, is_photographer FROM users WHERE username=@usernameValue and password=@passwordValue";

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
                // store photographer status
                Session["username"] = sqlDt.Rows[0]["username"];
                Session["isPhotographer"] = sqlDt.Rows[0]["is_photographer"];
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
        
        [WebMethod(EnableSession = true)]
        public string ReturnPotentialMatches()
        {
            string sqlSelect;

            if (Convert.ToInt32(Session["isPhotographer"]) == 1)
            {
                //Return clients who have not been matched, who have not been rejected by the prhotographer and are not in a pending match with the photographer
                sqlSelect = "SELECT username, availability, style, type, budget_range, experience, session_length, num_outfits FROM clients WHERE has_match = 0 AND username not in " +
                    "(SELECT client_username FROM rejects WHERE photographer_username = @photographerUsernameValue) " +
                    "AND username not in (SELECT client_username FROM pendings WHERE photographer_username = @photographerUsernameValue);";    
            }
            else
            {
                if (CheckMatchedStatus() == 1){
                    return "Already Matched!";
                }
                else
                {
                    //Return photographers who have already accepted the client
                    sqlSelect = "SELECT username, availability, style, type, budget_range, experience, session_length, num_outfits FROM photographers WHERE username in" +
                        "(SELECT photographer_username FROM pendings WHERE client_username = @clientUsernameValue);";
                }
            }
            
            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@clientUsernameValue", HttpUtility.UrlDecode(Convert.ToString(Session["username"])));
            sqlCommand.Parameters.AddWithValue("@photographerUsernameValue", HttpUtility.UrlDecode(Convert.ToString(Session["username"])));

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);

            DataTable sqlDt = new DataTable("clientAccounts");
            sqlDa.Fill(sqlDt);

            // Retun each user's information as a javascript object in an array
            // begin the array string
            string output = "[";

            //Returning the pulled users in javascript objects
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                // need to surround key + value in double quotes, requires use of escape sequences
                output += "{" + "\"username\":\"" + sqlDt.Rows[i]["username"] + "\", \"availability\":\"" + sqlDt.Rows[i]["availability"] + "\",\"style\":\"" +
                    sqlDt.Rows[i]["style"] + "\", \"type\":\"" + sqlDt.Rows[i]["type"] + "\", \"budget_range\":\"" + sqlDt.Rows[i]["budget_range"] +
                    "\", \"experience\":\"" + sqlDt.Rows[i]["experience"] + "\", \"session_length\":\"" + sqlDt.Rows[i]["session_length"]+
                    "\", \"num_outfits\":\"" + sqlDt.Rows[i]["num_outfits"] + "\"}";

                // don't want to add comma after the last object as jsonparse method will not accept the string
                if (i != sqlDt.Rows.Count - 1)
                {
                    output += ",";
                }
            }
            output += "]";

            return output;
        }

        [WebMethod(EnableSession = true)]
        public bool RejectMatch(string username)
        {
            string clientUsername;
            string photographerUsername;
            string sqlSelect;

            if (Convert.ToInt32(Session["isPhotographer"]) == 1)
            {
                clientUsername = username;
                photographerUsername = Convert.ToString(Session["username"]);
                sqlSelect = "INSERT INTO rejects VALUES(@clientUsernameValue, @photographerUsernameValue);";
            }
            else
            {
                photographerUsername = username;
                clientUsername = Convert.ToString(Session["username"]);
                //Delete from pendings because client does not want to match with the photographer who accepted it
                sqlSelect = "INSERT INTO rejects VALUES(@clientUsernameValue, @photographerUsernameValue);" +
                    "DELETE FROM pendings WHERE client_username = @clientUsernameValue AND photographer_username = @photographerUsernameValue;";
            }


            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@clientUsernameValue", HttpUtility.UrlDecode(clientUsername));
            sqlCommand.Parameters.AddWithValue("@photographerUsernameValue", HttpUtility.UrlDecode(photographerUsername));

            sqlConnection.Open();
            try
            {
                sqlCommand.ExecuteNonQuery();
                sqlConnection.Close();
                return true;
                
            }
            catch(Exception e)
            {
                sqlConnection.Close();
                return false;
            }
        }

        [WebMethod(EnableSession = true)]
        public bool AcceptMatch(string username)
        {
            string clientUsername;
            string photographerUsername;
            string sqlSelect;

            if(Convert.ToInt32(Session["isPhotographer"]) == 1)
            {
                clientUsername = username;
                photographerUsername = Convert.ToString(Session["username"]);
                // Create record in pendings table, awaits client action.
                sqlSelect = "INSERT INTO pendings VALUES(@clientUsernameValue, @photographerUsernameValue);";
            }
            else
            {
                photographerUsername = username;
                clientUsername= Convert.ToString(Session["username"]);
                // User accepts, no longer want to keep that pending pair on record. Keep track of match for further reference. Ensure client flagged as having match.
                sqlSelect = "DELETE FROM pendings WHERE client_username = @clientUsernameValue AND photographer_username = @photographerUsernameValue;" +
                    "INSERT INTO matches VALUES(@clientUsernameValue, @photographerUsernameValue);" +
                    "UPDATE clients SET has_match = 1 WHERE username = @clientUsernameValue";
            }

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@clientUsernameValue", HttpUtility.UrlDecode(clientUsername));
            sqlCommand.Parameters.AddWithValue("@photographerUsernameValue", HttpUtility.UrlDecode(photographerUsername));

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

        [WebMethod(EnableSession =true)]
        public string GetNotificationSummary()
        {
            if (Convert.ToInt32(Session["isPhotographer"]) == 0)
            {
                if(CheckMatchedStatus() == 1)
                {
                    // don't need to get a count of matches as clients are only paired with one photographer.
                    return $"Hello, {GetFirstName()}. <br><br>You have been matched with a photographer. Please see the 'Your Matches' page for more details";
                }
                else
                {
                    int pendingCount = GetClientPendingCount();
                    return $"Hello, {GetFirstName()}. <br><br>You have not been matched with a photographer yet. However, you currently have {pendingCount} " +
                        $"photographer(s) pending your acceptance. Please visit the 'Pending Matches' page to make a selection.";
                }
            }
            // display info is photographer
            else
            {
                return $"Hello, {GetFirstName()}. <br><br>You have been matched with {GetPhotographerMatchCount()} client(s) and " +
                    $"have {GetPhotographerPendingCount()} client(s) pending your acceptance. Please visit either the 'Pending Matches' " +
                    $"or 'Your Matches' page for more details.";
            }
        }

        [WebMethod(EnableSession = true)]
        public string GetMatchesInfo()
        {
            string sqlSelect;
            MySqlConnection sqlConnection = new MySqlConnection(getConString());

            if (Convert.ToInt32(Session["isPhotographer"]) == 1)
            {
                if (GetPhotographerMatchCount() == 0)
                {
                    // may change to a different string
                    return "No Matches - Photographer";
                }
                else
                {
                    // Pulls multiple records
                    sqlSelect = "SELECT username, first_name, email FROM users u INNER JOIN matches m on u.username = m.client_username " +
                        "WHERE m.photographer_username = @photographerUsernameValue";
                    
                    MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@photographerUsernameValue", HttpUtility.UrlDecode(Convert.ToString(Session["username"])));

                    MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
                    DataTable sqlDt = new DataTable("matchedClients");
                    sqlDa.Fill(sqlDt);

                    // create string consisting of array of js objects that can be parsed
                    string output = "[";

                    for (int i = 0; i < sqlDt.Rows.Count; i++)
                    {
                        output += "{" + "\"username\":\"" + sqlDt.Rows[i]["username"] + "\", \"first_name\":\"" + sqlDt.Rows[i]["first_name"] + 
                            "\",\"email\":\"" + sqlDt.Rows[i]["email"] + "\"}";

                        if (i != sqlDt.Rows.Count - 1)
                        {
                            output += ",";
                        }
                    }
                    output += "]";
                    return output;
                }
            }
            // code for clients
            else
            {
                if (CheckMatchedStatus() == 1)
                {
                    // Need to pull img url here too. After parsing string into json object, can determine if length of an object is 4, 
                    // to determine how to format page
                    // Retrieves just 1 record
                    sqlSelect = "SELECT u.username, first_name, email, image_url FROM matches m " +
                        "INNER JOIN users u on m.photographer_username = u.username " +
                        "INNER JOIN photographers p on m.photographer_username = p.username " +
                        "WHERE m.client_username = @clientUsernameValue";
                    
                    MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@clientUsernameValue", HttpUtility.UrlDecode(Convert.ToString(Session["username"])));

                    MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
                    DataTable sqlDt = new DataTable("matchedPhotographer");
                    sqlDa.Fill(sqlDt);

                    // Format as array of one object. Allows a check of the first element's length
                    string output = "[{" + "\"username\":\"" + sqlDt.Rows[0]["username"] + "\", \"first_name\":\"" + sqlDt.Rows[0]["first_name"] +
                            "\",\"email\":\"" + sqlDt.Rows[0]["email"] + "\", \"image_url\":\"" + sqlDt.Rows[0]["image_url"] + "\"}]";

                    return output;
                }
                else
                {
                    // may change to different string here too
                    return "No Matches - Client";
                }
            }
        }

        // Helper functions below
        //Helper function for checking if a client has already been matched
        [WebMethod(EnableSession = true)]
        public int CheckMatchedStatus()
        {
            string sqlSelect = "SELECT has_match FROM clients WHERE username = @clientUsernameValue;";

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@clientUsernameValue", HttpUtility.UrlDecode(Convert.ToString(Session["username"])));

            sqlConnection.Open();

            return Convert.ToInt32(sqlCommand.ExecuteScalar());

        }

        // Helper functions to get pending counts.
        [WebMethod(EnableSession =true)]
        public int GetClientPendingCount()
        {
            string sqlSelect = "SELECT COUNT(*) FROM pendings WHERE client_username = @clientUsernameValue;";

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@clientUsernameValue", HttpUtility.UrlDecode(Convert.ToString(Session["username"])));

            sqlConnection.Open();

            return Convert.ToInt32(sqlCommand.ExecuteScalar());
        }

        [WebMethod(EnableSession =true)]
        public int GetPhotographerPendingCount()
        {
            string sqlSelect = "SELECT COUNT(*) FROM clients WHERE has_match = 0 AND username not in " +
                    "(SELECT client_username FROM rejects WHERE photographer_username = @photographerUsernameValue) " +
                    "AND username not in (SELECT client_username FROM pendings WHERE photographer_username = @photographerUsernameValue);";

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@photographerUsernameValue", HttpUtility.UrlDecode(Convert.ToString(Session["username"])));

            sqlConnection.Open();

            return Convert.ToInt32(sqlCommand.ExecuteScalar());
        }

        // Helper function to count how many clients photographer matched with.
        [WebMethod(EnableSession =true)]
        public int GetPhotographerMatchCount()
        {
            string sqlSelect = "SELECT COUNT(*) FROM matches WHERE photographer_username = @photographerUsernameValue;";

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@photographerUsernameValue", HttpUtility.UrlDecode(Convert.ToString(Session["username"])));

            sqlConnection.Open();

            return Convert.ToInt32(sqlCommand.ExecuteScalar());
        }

        [WebMethod(EnableSession =true)]
        public string GetFirstName()
        {
            string sqlSelect = "SELECT first_name FROM users WHERE username = @usernameValue;";

            MySqlConnection sqlConnection = new MySqlConnection(getConString());
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@usernameValue", HttpUtility.UrlDecode(Convert.ToString(Session["username"])));

            sqlConnection.Open();

            return Convert.ToString(sqlCommand.ExecuteScalar());
        }
    }
}
