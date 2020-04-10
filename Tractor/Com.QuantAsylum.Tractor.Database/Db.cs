using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tractor;

namespace Com.QuantAsylum.Tractor.Database
{
    static class Db
    {
        static TestResultDatabase TrDb;

        static public bool CreateNew(string connectString)
        {
            try
            {
                TrDb = new TestResultDatabase(connectString);
                TrDb.CreateDatabase();
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogType.Database, "CreateNew() exception: " + ex.Message);
            }

            return false;
        }

        static public bool DeleteExisting(string connectString)
        {
            try
            {
                TrDb = new TestResultDatabase(connectString);
                TrDb.DeleteDatabase();
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogType.Database, "DeleteExisting() exception: " + ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Returns true if existing db was successfully opened
        /// </summary>
        /// <param name="connectString"></param>
        /// <returns></returns>
        static public bool OpenExisting(string connectString)
        {
            try
            {
                TrDb = new TestResultDatabase(connectString);
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogType.Database, "OpenExisting() exception: " + ex.Message);
            }

            return false;
        }

        static public bool InsertTest(Test tri)
        {
            TrDb.Tests.InsertOnSubmit(tri);

            try
            {
                TrDb.SubmitChanges();
                Log.WriteLine(LogType.Database, "Submitted row to database.");
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogType.Error, "InsertTest() exception: " + ex.Message);
            }

            return false;

        }
    }
}
