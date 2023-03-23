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

    }
}
