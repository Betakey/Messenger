﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace ChatClient
{
    public class Database
    {
        public string ConnectionString { get; private set; }

        public MySqlConnection Connection { get; private set; }

        public bool exists;

        public Database()
        {
            ConnectionString = "SERVER=gethercode.de;UID=MessenJarAdmin;PASSWORD=sUg4n?89;DATABASE=MessenJarDB";
            Connection = new MySqlConnection(ConnectionString);
            exists = false;
        }

        public void OpenConnection()
        {
            try
            {
                Connection.Open();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void CloseConnection()
        {
            Connection.Close();
        }

        public void Register(string userName, string password)  
        {
            OpenConnection();
            using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM User WHERE Name = @name", Connection))
            {
                cmd.Parameters.Add("@name", MySqlDbType.Text);
                cmd.Parameters["@name"].Value = userName;
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        exists = true;
                        MessageBox.Show("This username has been used.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        CloseConnection();
                        return;
                    }
                    else
                    {
                        using (MySqlCommand command = new MySqlCommand(
                    "INSERT INTO User (Name, Password, Friends) VALUES (@name, @pw, @fr)", Connection))
                        {
                            command.Parameters.Add("@name", MySqlDbType.Text);
                            command.Parameters["@name"].Value = userName;
                            command.Parameters.Add("@pw", MySqlDbType.Text);
                            command.Parameters["@pw"].Value = CalculateMD5(password);
                            command.Parameters.Add("@fr", MySqlDbType.Text);
                            command.Parameters["@fr"].Value = "Darki";
                            command.ExecuteNonQuery();
                        }
                        MessageBox.Show("Registration Completed!", "Succeeded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
                using (MySqlCommand command = new MySqlCommand(
                    "INSERT INTO User (Name, Password, Friends) VALUES (@name, @pw, @fr)", Connection))
                {
                    command.Parameters.Add("@name", MySqlDbType.Text);
                    command.Parameters["@name"].Value = userName;
                    command.Parameters.Add("@pw", MySqlDbType.Text);
                    command.Parameters["@pw"].Value = CalculateMD5(password);
                    command.Parameters.Add("@fr", MySqlDbType.Text);
                    command.Parameters["@fr"].Value = "Darki";
                    command.ExecuteNonQuery();
                }
                MessageBox.Show("Registration Completed!", "Succeeded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CloseConnection();
        }

        public string Login(string username, string password)
        {
            OpenConnection();
            using (MySqlCommand command = new MySqlCommand("SELECT * FROM User WHERE Name = @name AND Password = @pw", Connection))
            {
                command.Parameters.Add("@name", MySqlDbType.Text);
                command.Parameters["@name"].Value = username;
                command.Parameters.Add("@pw", MySqlDbType.Text);
                command.Parameters["@pw"].Value = CalculateMD5(password);
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string s = reader["Friends"] as string;
                        if (s == null) s = "";
                        return s;
                    }
                }
            }
            CloseConnection();
            return null;
        }

        private string CalculateMD5(string input)
        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();

            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);

            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)

            {

                sb.Append(hash[i].ToString("X2"));

            }

            return sb.ToString();
        }
    }
}