﻿using Gymble.Repositories;
using Gymble.Utils;
using System.Data.SQLite;
using System.IO;

namespace Gymble.Services
{
    public class SQLiteManager
    {
        public string DatabaseName = Constants.Database.FileName;
        public string FolderName = Constants.Database.FolderName;
        public SQLiteConnection connection = null;

        public SQLiteManager()
        {
            CreateDatabaseFile();
        }

        private string GetFolderPath()
        {
            return Path.Combine(Utils.Utils.CurrentDirectory, FolderName);
        }

        private string GetFilePath()
        {
            return Path.Combine(GetFolderPath(), DatabaseName);
        }

        private void CreateDatabaseFile()
        {
            DirectoryInfo di = new DirectoryInfo(GetFolderPath());
            if (!di.Exists)
            {
                di.Create();
                //Utils.log.Info("Create Database Folder.");
            }

            string filePath = GetFilePath();
            if (!File.Exists(filePath))
            {
                SQLiteConnection.CreateFile(GetFilePath());
                //Utils.log.Info("Create Database.");
                CreateTables();
            }
        }

        public void OpenConnection()
        {
            string s = GetFilePath();
            connection = new SQLiteConnection($"Data Source={GetFilePath()}");
            connection.Open();
        }

        public void CloseConnection()
        {
            connection?.Dispose();
            connection = null;
        }

        public void CreateTables()
        {
            OpenConnection();

            SQLiteCommand cmd = new SQLiteCommand(connection);
            cmd.CommandText = SqlMemberQuery.CREATE_MEMBER_TABLE;
            cmd.ExecuteNonQuery();

            cmd.CommandText = SqlProductQuery.CREATE_PRODUCT_TABLE;
            cmd.ExecuteNonQuery();

            cmd.CommandText = SqlMembershipQuery.CREATE_MEMBERSHIP_TABLE;
            cmd.ExecuteNonQuery();

            cmd.CommandText = SqlAttendanceQuery.CREATE_ATTENDANCE_TABLE;
            cmd.ExecuteNonQuery();

            CloseConnection();
        }

        public void UseMemberRepository()
        {
            OpenConnection();

            var repo = new MemberRepository(connection);
            var all = repo.GetAllMembers();

            CloseConnection();
        }

        public void UseMembershipRepository()
        {
            OpenConnection();

            var repo = new MemberRepository(connection);
            var all = repo.GetAllMembers();

            CloseConnection();
        }

        public void UseAttendaceRepository()
        {
            OpenConnection();

            var repo = new MemberRepository(connection);
            var all = repo.GetAllMembers();

            CloseConnection();
        }

        public void UseProductRepository()
        {
            OpenConnection();

            var repo = new MemberRepository(connection);
            var all = repo.GetAllMembers();

            CloseConnection();
        }

        public void GetAllRepositories()
        {
            UseMemberRepository();
            UseMembershipRepository();
            UseAttendaceRepository();
            UseProductRepository();
        }
    }
}
