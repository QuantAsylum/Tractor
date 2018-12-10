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

        static public void CreateNew(string connectString, out string resultMsg)
        {
            resultMsg = "???";

            TrDb = new TestResultDatabase(connectString);

            if (TrDb.DatabaseExists())
            {
                if (MessageBox.Show("Database already exists. Overwrite?", "Conflict", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    resultMsg = "Old database deleted.";
                    Log.WriteLine(LogType.Database, resultMsg);
                    TrDb.DeleteDatabase();
                }
            }

            resultMsg = "New database created.";
            Log.WriteLine(LogType.Database, resultMsg);
            TrDb.CreateDatabase();
        }

        /// <summary>
        /// Returns true if existing db was successfully opened
        /// </summary>
        /// <param name="connectString"></param>
        /// <returns></returns>
        static public bool OpenExisting(string connectString)
        {
            TrDb = new TestResultDatabase(connectString);
            if (TrDb.DatabaseExists())
            {
                return true;
            }

            return false;
        }

        static public bool InsertTest(TestRowItem tri)
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
                Log.WriteLine(LogType.Error, "Exception submitting changes in InsertTest(): " + ex.Message);
            }

            return false;

        }
    }
}
