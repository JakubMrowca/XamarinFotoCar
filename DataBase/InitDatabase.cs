using System;
using System.IO;
using Realms;

namespace DataBase
{
    public class InitDatabase
    {
      
        public InitDatabase()
        {
            _db = Realm.GetInstance();
        }

    }
}
