using System;

using MySql.Data.MySqlClient;


namespace ProjectBrowser
{
    /// <summary>
    /// Wraps calls to the users database
    /// </summary>
    public class DbWrapper
    {
        private MySqlConnection sqlConn;
        private string connStr;
        private bool isConnected;

        /// <summary>
        /// Creates a new database wrapper object that wraps around
        /// the users table.
        /// </summary>
        /// <param name="svr">The name of the server</param>
        /// <param name="db">The database catalog to use</param>
        /// <param name="user">The user name</param>
        /// <param name="pass">The user password</param>
        public DbWrapper(string svr, string db, string user, string pass)
        {
            this.connStr = "Server=" + svr + ";Database=" + db + ";Uid=" + user + ";Pwd=" + pass + ";";

            try
            {
                sqlConn = new MySqlConnection(this.connStr);
            }
            catch (Exception excp)
            {
                Exception myExcp = new Exception("Error connecting you to " +
                    "the my sql server. Internal error message: " + excp.Message, excp);
                throw myExcp;
            }

            this.isConnected = false;
        }

        /// <summary>
        /// Creates a new database wrapper object that wraps around
        /// the users table.
        /// </summary>
        /// <param name="connStr">A connection string to provide to connect
        /// to the database</param>
        public DbWrapper(string connStr)
        {
            this.connStr = connStr;

            try
            {
                sqlConn = new MySqlConnection(this.connStr);
            }
            catch (Exception excp)
            {
                Exception myExcp = new Exception("Error connecting you to " +
                    "the my sql server. Error: " + excp.Message, excp);

                throw myExcp;
            }

            this.isConnected = false;
        }

        /// <summary>
        /// Opens the connection to the SQL database.
        /// </summary>
        public void Connect()
        {
            bool success = true;

            if (this.isConnected == false)
            {
                try
                {
                    this.sqlConn.Open();
                }
                catch (Exception excp)
                {
                    this.isConnected = false;
                    success = false;
                    Exception myException = new Exception("Error opening connection" +
                        " to the sql server. Error: " + excp.Message, excp);

                    throw myException;
                }

                if (success)
                {
                    this.isConnected = true;
                }
            }
        }

        /// <summary>
        /// Closes the connection to the sql connection.
        /// </summary>
        public void Disconnect()
        {
            if (this.isConnected)
            {
                this.sqlConn.Close();
            }
        }

        /// <summary>
        /// Gets the current state (boolean) of the connection.
        /// True for open, false for closed.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return this.isConnected;
            }
        }

        /// <summary>
        /// Adds a user into the database
        /// </summary>
        /// <param name="username">The user login</param>
        /// <param name="password">The user password</param>
        public void AddUser(string username, string password)
        {
            string Query = "INSERT INTO users(usr_name, usr_pass) values" +
                "('" + username + "','" + password + "')";

            MySqlCommand addUser = new MySqlCommand(Query, this.sqlConn);

            try
            {
                addUser.ExecuteNonQuery();
            }
            catch (Exception excp)
            {
                Exception myExcp = new Exception("Could not add user. Error: " +
                    excp.Message, excp);
                throw (myExcp);
            }
        }

        /// <summary>
        /// Verifies whether a user with the supplied user
        /// credentials exists in the database or not. User
        /// credentials are case-sensitive.
        /// </summary>
        /// <param name="username">The user login</param>
        /// <param name="password">The user password</param>
        /// <returns>A boolean value. True if the user exists
        /// in the database, false if the user does not exist
        /// in the database.</returns>
        public bool VerifyUser(string username, string password)
        {
            int returnValue = 0;

            string Query = "SELECT COUNT(*) FROM users where (usr_Name=" +
                "'" + username + "' and usr_Pass='" + password + "') LIMIT 1";

            MySqlCommand verifyUser = new MySqlCommand(Query, this.sqlConn);

            try
            {
                verifyUser.ExecuteNonQuery();

                MySqlDataReader myReader = verifyUser.ExecuteReader();

                while (myReader.Read() != false)
                {
                    returnValue = myReader.GetInt32(0);
                }

                myReader.Close();
            }
            catch (Exception excp)
            {
                Exception myExcp = new Exception("Could not verify user. Error: " +
                    excp.Message, excp);
                throw (myExcp);
            }

            if (returnValue == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Checks whether a supplied user name exists or not
        /// </summary>
        /// <param name="username">The user name</param>
        /// <returns>True if the username is already in the table,
        /// false if the username is not in the table</returns>
        public bool UserExists(string username)
        {
            int returnValue = 0;

            string Query = "SELECT COUNT(*) FROM users where (usr_Name=" +
                "'" + username + "') LIMIT 1";

            MySqlCommand verifyUser = new MySqlCommand(Query, this.sqlConn);

            try
            {
                verifyUser.ExecuteNonQuery();

                MySqlDataReader myReader = verifyUser.ExecuteReader();

                while (myReader.Read() != false)
                {
                    returnValue = myReader.GetInt32(0);
                }

                myReader.Close();
            }
            catch (Exception excp)
            {
                Exception myExcp = new Exception("Could not verify user. Error: " +
                    excp.Message, excp);
                throw (myExcp);
            }

            if (returnValue == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


    }
}