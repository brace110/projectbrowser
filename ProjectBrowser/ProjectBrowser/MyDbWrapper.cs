using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace ProjectBrowser
{
    class MyDbWrapper
    {
        MySqlConnection conn = new MySqlConnection("server=localhost;user=root;database=project_browser;port=3306;password=;convert zero datetime=True");
        MySqlDataReader rdr = null;

        public MyDbWrapper()
        {

            
            // Boolean connected = false;

            

            //try
            //{
            //    Console.WriteLine("Connecting to MySQL...");
            //    conn.Open();
            //    connected = true;
            //     Perform database operations.


            //    string stm = "SELECT VERSION()";
            //    MySqlCommand cmd = new MySqlCommand(stm, conn);
            //    string version = Convert.ToString(cmd.ExecuteScalar());
            //    Console.WriteLine("MySQL version : {0}", version);


            //    MySqlCommand cmd = new MySqlCommand();
            //    cmd.Connection = conn;
            //    cmd.CommandText = "INSERT INTO Authors(Name) VALUES(@Name)";
            //    cmd.Prepare();

            //    cmd.Parameters.AddWithValue("@Name", "Trygve Gulbranssen");
            //    cmd.ExecuteNonQuery();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.ToString());
            //}


            //if (connected)
            //{
            //    Console.WriteLine("WOWOWOWOWO it's working!!!!!!!!!");
            //}
            //else
            //{
            //    Console.WriteLine(":'(");
            //}

            //conn.Close(); 

            //Console.ReadLine();
        }

        public Dictionary<Int32, Project> returnProjects()
        {
            Dictionary<Int32, Project> projects = new Dictionary<Int32, Project>();

            try
            {
                conn.Open();

                String stm = "SELECT * FROM projecten";
                MySqlCommand cmd = new MySqlCommand(stm, conn);
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    Project project = new Project(rdr.GetInt32(0), rdr.GetString(1), rdr.GetString(2), rdr.GetDateTime(3), rdr.GetString(4), rdr.GetString(5), rdr.GetInt32(9), rdr.GetString(10), rdr.GetString(11), rdr.GetString(12));
                    projects.Add(project.id, project);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.ToString());
            }
            finally
            {
                if (rdr != null)
                {
                    rdr.Close();
                }

                if (conn != null)
                {
                    conn.Close();
                }
            }

            return projects;
        }

        public void writeView(Project project)
        {
            try
            {
                conn.Open();

                String stm = "UPDATE projecten SET viewcount = viewcount + 1 WHERE ID = ?projectID";
                MySqlCommand cmd = new MySqlCommand(stm, conn);
                cmd.Parameters.AddWithValue("?projectID", project.id);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
        }
    }
}
