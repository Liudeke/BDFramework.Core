﻿using System;
using BDFramework.Sql;

namespace BDFramework.Configure
{
    /// <summary>
    /// 游戏加密处理器
    /// </summary>
    [GameConfig(2,"加密")]
    public class GameCipherConfigProcessor : AConfigProcessor
    {
        /// <summary>
        /// 游戏加密设置
        /// </summary>
        public class Config : ConfigDataBase
        {
            /// <summary>
            /// 数据库密码
            /// </summary>
            public string SqlitePassword = "password123";
            /// <summary>
            /// 公钥
            /// </summary>
            public string ScriptPubKey = "";
            /// <summary>
            /// 私钥
            /// </summary>
            public string ScriptPrivateKey = "";
        }


        /// <summary>
        /// 当加载成功
        /// </summary>
        /// <param name="config"></param>
        public override void OnConfigLoad(ConfigDataBase config)
        {
            var con = config as Config;
            //Sqlite秘钥
            SqliteLoder.Password = con.SqlitePassword;
            //DLL秘钥
            ScriptLoder.PrivateKey = con.ScriptPrivateKey;
            ScriptLoder.PublicKey = con.ScriptPubKey;
        }
    }
    
    

}
