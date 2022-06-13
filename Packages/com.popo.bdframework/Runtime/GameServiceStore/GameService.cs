﻿using System;
using System.Collections.Generic;

namespace BDFramework.GameServiceStore
{
    /// <summary>
    /// 游戏Service
    /// </summary>
    public class GameService
    {
        private List<object> singletonList = new List<object>();

        /// <summary>
        /// 添加对象
        /// 每一次GetService都会返回相同实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddSingleton<T>() where T : class
        {
            var inst = Activator.CreateInstance(typeof(T));
            this.AddSingleton(inst);
        }

        /// <summary>
        /// 添加对象
        /// 每一次GetService都会返回相同实例
        /// </summary>
        /// <param name="t"></param>
        /// <typeparam name="T"></typeparam>
        public void AddSingleton(object inst)
        {
            var type = inst.GetType();
            var find = this.singletonList.Find((o) => o.GetType() == type);
            if (find == null)
            {
                singletonList.Add(inst);
            }
            else
            {
                BDebug.LogError("已存在同类型的Singleton");
            }
        }

        private List<Type> transientList = new List<Type>();

        /// <summary>
        /// 添加对象
        /// 每一次GetService都会创建一个新的实例
        /// </summary>
        /// <param name="t"></param>
        /// <typeparam name="T"></typeparam>
        public void AddTransient<T>(T obj) where T : class
        {
            var type = obj.GetType();
            var find = this.transientList.Find((t) => t == type);
            if (find == null)
            {
                transientList.Add(type);
            }
            else
            {
                BDebug.LogError("已存在同类型的Transient");
            }
        }

        /// <summary>
        /// 获取一个服务
        /// </summary>
        /// <param name="t"></param>
        /// <typeparam name="T"></typeparam>
        public T GetService<T>(T t) where T : class
        {
            return GetService(typeof(T)) as T;
        }


        /// <summary>
        /// 获取一个服务
        /// </summary>
        /// <param name="type"></param>
        /// <typeparam name="T"></typeparam>
        public object GetService(Type type) //where T : class
        {
            var ret = this.singletonList.Find((o) => o.GetType() == type);
            if (ret == null)
            {
                var transientType = this.transientList.Find((t) => t == type);
                if (transientType != null)
                {
                    ret = Activator.CreateInstance(transientType);
                }
            }

            return ret;
        }
    }
}