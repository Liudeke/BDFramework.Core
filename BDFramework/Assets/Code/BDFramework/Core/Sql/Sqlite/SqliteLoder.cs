using System.IO;
using BDFramework.Helper;
using UnityEngine;

namespace SQLite4Unity3d
{
    static public class SqliteLoder
    {
        static public SQLiteConnection Connection { get; private set; }

        /// <summary>
        /// 初始化DB
        /// </summary>
        /// <param name="str"></param>
        static public void Load(string root)
        {
            if (Connection != null)
            {
               Connection.Dispose();
            }
            
            //persistent和streaming中,必须有存在一个
            #if UNITY_EDITOR_OSX
            var path = root+"/"+ Utils.GetPlatformPath(Application.platform) + "/LocalDB";
            #else
            var path = Path.Combine(root, Utils.GetPlatformPath(Application.platform) + "/LocalDB");
            #endif
            //
            Connection = new SQLiteConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
            BDebug.Log("DB加载路径:" + path ,"red");
        }


        /// <summary>
        /// 创建连接
        /// </summary>
        static public SQLiteConnection CreateConnetion(string path)
        {
            return new SQLiteConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        }
              
        /// <summary>
        /// 创建db
        /// </summary>
        /// <param name="pathme"></param>
        static public void CreateDB(string path)
        {
            var _db = new SQLiteConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create, false);
            _db.Dispose();
        }
    }
}